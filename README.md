# IETC-TKT-API

API REST du projet **TKT** — un système de **ticketing multi-tenant** (helpdesk interne) en C# / ASP.NET 10, structuré en **Clean Architecture** avec **Dapper** et **PostgreSQL 18**.

> Cours « Projet de développement web » — BAC 2, Bachelier en Informatique : orientation développement d'applications — *Kamal BELOUH*.
> Frontend Angular : dépôt séparé `IETC-TKT-WEB`.

> **État** : squelette Clean Architecture prêt à développer. Les fonctionnalités (auth, tickets, …) sont construites au fil des epics / user stories — voir [État actuel](#état-actuel-du-dépôt) et [À venir](#à-venir).

---

## Le produit (cible)

Plateforme de helpdesk interne en SaaS multi-tenant : chaque **tenant** est une entreprise qui gère ses tickets d'assistance. Les utilisateurs d'un tenant ont des responsabilités différenciées par leur rôle :

| Rôle | Responsabilité |
|------|----------------|
| **Owner** | propriétaire du tenant, gère l'abonnement et les accès globaux |
| **Admin** | administre catégories, équipes, politiques SLA, journaux d'audit |
| **Agent** | prend en charge, traite et résout les tickets |
| **Member** | soumet ses demandes et suit ses propres tickets |

---

## Stack

- **.NET 10** / C# — ASP.NET Core (Minimal API)
- **Clean Architecture** — `TKT.Api` → `TKT.Core` ← `TKT.Infrastructure`
- **Dapper** — accès données, SQL explicite (pas d'ORM lourd)
- **PostgreSQL 18** — schéma `ticketing_system` (28 tables)
- **JWT** (validation bearer configurée) — authentification stateless
- **Scalar** — documentation d'API interactive (dev)

---

## Architecture

### Le sens des dépendances

```
TKT.Api  ──►  TKT.Core  ◄──  TKT.Infrastructure
```

- **TKT.Core** — ne référence **aucun** autre projet. Cœur métier : logique, modèles de domaine, exceptions, interfaces (ports).
- **TKT.Infrastructure** — référence Core. Implémente les ports (accès données Dapper/Npgsql, etc.).
- **TKT.Api** — référence Core + Infrastructure. Endpoints, middleware, câblage DI.

Le compilateur garantit la règle : impossible d'écrire `using TKT.Infrastructure...` dans `TKT.Core` (`TKT.Core.csproj` n'a aucune référence projet).

### Chaîne d'une requête (patron cible)

```
HTTP
 └─ TKT.Api/Endpoints            (DTO ↔ Command/Result)
     └─ TKT.Core/UseCases        (règles métier, validations, autorisation applicative)
         └─ TKT.Core/Abstractions (ports : gateways + services)
             └─ TKT.Infrastructure/Gateways      (mapping Record ↔ domaine)
                 └─ TKT.Infrastructure/Repositories  (Dapper)
                     └─ PostgreSQL
```

---

## État actuel du dépôt

Squelette en place (compile, 0 erreur) — pas encore de fonctionnalité métier.

```
TKT.Core/
├── Domain/Exceptions/        exceptions de domaine (→ HTTP par le middleware)
└── DependencyInjection.cs    AddCore()

TKT.Infrastructure/
├── Persistence/
│   ├── IDbConnectionFactory.cs / NpgsqlConnectionFactory.cs   pool de connexions (NpgsqlDataSource)
│   └── DbSession.cs          transaction par requête (Unit of Work, lazy)
└── DependencyInjection.cs    AddInfrastructure()

TKT.Api/
├── Extensions/AuthenticationExtensions.cs   config JWT bearer (validation) + policies
├── Middleware/ExceptionHandlingMiddleware.cs  exceptions métier → codes HTTP
├── Middleware/TransactionMiddleware.cs        commit/rollback de la transaction par requête
└── Program.cs                composition root (DI + middleware + JWT + OpenAPI/Scalar)

Database/
└── tkt.sql                   schéma PostgreSQL 18 complet

docker-compose.yml · .env.example
```

---

## Décisions d'architecture (cible)

### 1. Autorisation à deux niveaux
- **Isolation tenant — DB (RLS)** : PostgreSQL applique une *Row-Level Security* stricte (`company_id = current_company_id()`). Inviolable.
- **Permissions métier — applicatif** : les règles par rôle (un `member` ne voit que ses tickets, le staff voit tout) sont appliquées dans `TKT.Core`. Évolutif sans migration DB.

### 2. Erreurs métier → HTTP
Les use cases lèvent des exceptions de domaine (`NotFoundException`, `ValidationException`, …) ; `ExceptionHandlingMiddleware` les traduit en codes HTTP (400/401/403/404/409). Aucun `try/catch` dans les endpoints.

### 3. Privacy by design (RGPD)
Le schéma sépare l'authentification (`accounts`, zéro PII) des données personnelles (`user_profiles`, PII) — droit à l'effacement, minimisation, rétention différenciée.

### 4. UUIDv7
Clés primaires en UUIDv7, générées côté C# (`Guid.CreateVersion7()`) ; le `DEFAULT uuidv7()` en DB sert de fallback.

### 5. Transaction par requête (Unit of Work)
`DbSession` (Scoped) ouvre **une** connexion + **une** transaction par requête (lazy : seulement si la DB est touchée). `TransactionMiddleware` **commit** à la fin si succès, **rollback** sur exception → chaque requête est atomique par défaut. C'est aussi là que le `SET LOCAL app.current_company_id` (RLS) sera posé une fois `ITenantContext` en place.

---

## Base de données

Schéma `ticketing_system` — **28 tables** (`Database/tkt.sql`) :

- **Auth & Identity** : `accounts`, `user_profiles`, `refresh_tokens`
- **Tenants** : `companies`, `company_subscriptions`, `company_members`, `pending_invitations`
- **Ticketing** : `categories`, `tickets`, `ticket_comments`, `tags`, `ticket_tags`
- **Attachments** : `attachments`, `ticket_attachments`, `comment_attachments`
- **SLA** : `sla_policies`, `ticket_sla_tracking`, `sla_escalation_rules`
- **Organisation** : `teams`, `team_members`
- **Notifications** : `notifications`, `notification_preferences`, `email_queue`
- **Config & Audit** : `company_settings`, `audit_logs` (append-only, partitionnée)
- **RGPD** : `gdpr_consent_log`, `data_processing_log`

Points notables : FK composites `(entity_id, company_id)` pour l'intégrité tenant, soft-delete (`deleted_at`), audit append-only par triggers, valeurs contraintes par `CHECK`, deux rôles SQL (`app_user` RLS-enforced / `app_system` BYPASSRLS).

---

## Démarrage

### Prérequis
- .NET 10 SDK
- Docker (ou un PostgreSQL 18 local)

### 1. Base de données (Docker)
```bash
cp .env.example .env          # ajuste les mots de passe si besoin
docker compose up -d
```
Le conteneur initialise `Database/tkt.sql` au premier démarrage : base `ticketing_system`, 28 tables, rôles `app_user` / `app_system`.

### 2. Configuration
La connection string de dev est dans `TKT.Api/appsettings.Development.json` (base `ticketing_system`, user `app_user`). Les mots de passe du conteneur viennent du `.env`.

### 3. Lancer l'API
```bash
dotnet run --project TKT.Api
```

- API : `http://localhost:5xxx`
- Doc Scalar (dev) : `http://localhost:5xxx/scalar`
- OpenAPI JSON : `http://localhost:5xxx/openapi/v1.json`

---

## Périmètre fonctionnel (MoSCoW)

Priorisation : **72 SP** actifs — MUST 25 % · SHOULD 40 % · COULD 31 % · WON'T 4 %.

| Epic | Domaine |
|------|---------|
| 1 | Authentification & gestion des utilisateurs |
| 2 | Entreprises & multi-tenant (membres, rôles, abonnements) |
| 3 | **Tickets (cœur métier)** — CRUD, workflow, commentaires, Kanban, recherche full-text |
| 4 | Catégories, tags, pièces jointes |
| 5 | SLA & escalade |
| 6 | Équipes & organisation |
| 7 | Notifications & emails |
| 8 | Configuration & audit |
| 9 | Enrichissements UX (mentions, dashboard, temps réel) — post-MVP |

---

## À venir

Construits au fil des epics, en suivant le patron Clean Architecture :

- Module **accounts / auth** : register, login, émission JWT, refresh token
- **`ITenantContext`** + middleware de résolution tenant (validation membership + rôle)
- **Repositories / gateways / use cases** par entité
- **Policies** : RBAC (route) + autorisation métier (Core)

---

## Conventions

- **Erreurs métier** : exceptions de `TKT.Core/Domain/Exceptions`, traduites en HTTP par `ExceptionHandlingMiddleware`.
- **Mapping** : explicite, à la main (pas d'AutoMapper).
- **Async** : de bout en bout.
- **Secrets locaux** : dans `.env` (gitignoré) ; voir `.env.example`.
