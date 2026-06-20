# IETC-TKT-API

API REST du projet **TKT** — un système de **ticketing multi-tenant** (helpdesk interne) en C# / **ASP.NET Core (.NET 10)**, structuré en **Clean Architecture** avec **Dapper** et **PostgreSQL 18**.

> Cours « Projet de développement web » — BAC 2, Bachelier en Informatique : orientation développement d'applications.
> **Frontend Angular** : dépôt séparé **`IETC-TKT-WEB`** (Angular 21, consomme cette API).

---

## Le produit

Plateforme de helpdesk interne multi-tenant : chaque **tenant** est une entreprise qui gère ses tickets d'assistance. Les utilisateurs d'un tenant ont des responsabilités différenciées par leur rôle :

| Rôle | Responsabilité |
|------|----------------|
| **Owner** | propriétaire du tenant, gère l'abonnement et les accès |
| **Admin** | administre membres, catégories, etc. |
| **Agent** | prend en charge, traite et résout les tickets |
| **Member** | soumet ses demandes et suit **ses propres** tickets |

Un même compte peut appartenir à **plusieurs tenants**, avec un **rôle différent dans chacun**.

---

## Stack

- **.NET 10** / C# — ASP.NET Core (Minimal API)
- **Clean Architecture** — `TKT.Api` → `TKT.Core` ← `TKT.Infrastructure`
- **Dapper** — accès données, SQL explicite (pas d'ORM ; pas d'Entity Framework)
- **PostgreSQL 18** — schéma `ticketing_system`, isolation tenant par **Row-Level Security**
- **JWT** (access + refresh token) — authentification stateless
- **Argon2id** — hachage des mots de passe (Isopoh)
- **Scalar** — documentation d'API interactive (dev)
- **xUnit** — 107 tests (domaine, value objects, policies, autorisation)

---

## Architecture

### Sens des dépendances

```
TKT.Api  ──►  TKT.Core  ◄──  TKT.Infrastructure
```

- **TKT.Core** — ne référence **aucun** autre projet. Cœur métier : entités, value objects, use cases, services applicatifs, exceptions, interfaces (ports : `IGateways`, `Abstractions`).
- **TKT.Infrastructure** — référence Core. Implémente les ports (repositories Dapper/Npgsql, gateways, sécurité).
- **TKT.Api** — référence Core + Infrastructure. Endpoints (routers), middleware, composition de la DI.

La règle est garantie par le compilateur : `TKT.Core.csproj` n'a **aucune** référence projet (impossible d'y écrire `using TKT.Infrastructure...`).

### Chaîne d'une requête

```
HTTP
 └─ TKT.Api/EndPoints            (DTO ↔ Input/Result, claims → use case)
     └─ TKT.Core/UseCases        (règles métier, validation, autorisation applicative)
         └─ TKT.Core/IGateways + Abstractions   (ports)
             └─ TKT.Infrastructure/Gateways      (mapping Row ↔ domaine)
                 └─ TKT.Infrastructure/Repositories  (Dapper)
                     └─ PostgreSQL
```

---

## Décisions d'architecture

**1. Autorisation à deux niveaux**
- *Isolation tenant — DB (RLS)* : PostgreSQL applique une Row-Level Security stricte (`company_id = current_company_id()`). Inviolable, indépendante du code.
- *Permissions métier — applicatif* : les règles par rôle (un `member` ne voit que ses tickets, le staff voit tout) sont dans `TKT.Core`, centralisées dans `ICompanyMemberAuthorizer` + les policies (`TicketAuthorizationPolicy`, `CompanyAccessPolicy`). Évolutif sans migration DB.

**2. Erreurs métier → HTTP**
Les use cases lèvent des exceptions de domaine (`NotFoundException`, `ValidationException`, `ConflictException`, `ForbiddenException`) ; `ExceptionHandlingMiddleware` les traduit en 400/401/403/404/409. Aucun `try/catch` dans les endpoints.

**3. Transaction par requête (Unit of Work)**
`DbSession` (Scoped) ouvre une connexion + une transaction par requête (lazy). `TransactionMiddleware` **commit** en cas de succès, **rollback** sur exception → chaque requête est atomique. Le contexte tenant (`SET LOCAL app.current_user_id` / `app.current_company_id`) est posé ici pour la RLS et l'audit.

**4. Privacy by design (RGPD)**
Le schéma sépare l'authentification (`accounts`, zéro PII) des données personnelles (`user_profiles`, PII) → effacement, minimisation, rétention différenciée.

**5. Audit append-only**
Triggers PostgreSQL sur les tables sensibles (`tickets`, `ticket_comments`, `company_members`, `sla_policies`) → `audit_logs` (avant/après en JSONB), source de vérité même pour les écritures hors application.

**6. UUIDv7** — clés primaires générées côté C# (`Guid.CreateVersion7()`), `DEFAULT uuidv7()` en DB en fallback.

---

## Périmètre implémenté

| Domaine | Endpoints |
|---|---|
| **Auth** | `POST /api/auth/register` · `GET /api/auth/confirm-email` · `POST /api/auth/login` · `POST /api/auth/refresh` · `POST /api/auth/switch-tenant` |
| **Onboarding** | `POST /api/onboarding/create-company` · `POST /api/onboarding/join-invitation` |
| **Identité** | `GET /api/me` · `GET /api/users/me/companies` |
| **Membres** | `GET /api/companies/{id}/members` · `POST .../members/invite` · `PATCH .../members/{accountId}` (rôle) · `PATCH .../members/{accountId}/status` · `GET .../members/directory` · `DELETE /api/companies/{id}/invitations/{invitationId}` |
| **Tickets** | `POST /api/tickets` · `GET /api/tickets` (filtres : `status`, `priority`, `assignedTo` (ou `unassigned`), `categoryId`, `sort`, `page`, `pageSize`) · `GET /api/tickets/stats` · `GET /api/tickets/{id}` · `PATCH /api/tickets/{id}` |
| **Commentaires** | `POST /api/tickets/{id}/comments` · `GET /api/tickets/{id}/comments` · `PATCH /api/comments/{id}` |

Règles métier notables : workflow de statuts (`open → in_progress → pending → resolved → closed`) avec transitions automatiques (assignation → `in_progress` ; commentaire public agent → `pending` ; réponse du demandeur → `in_progress`), quotas d'abonnement (Free/Pro), invitations (compte existant vs `pending_invitation`), protection du dernier owner.

---

## Base de données

Schéma `ticketing_system` (`Database/tkt.sql`) :

- **Auth & Identity** : `accounts`, `user_profiles`, `refresh_tokens`
- **Tenants** : `companies`, `company_subscriptions`, `company_members`, `pending_invitations`
- **Ticketing** : `categories`, `tickets`, `ticket_comments`, `tags`, `ticket_tags`
- **Attachments** : `attachments`, `ticket_attachments`, `comment_attachments`
- **SLA** : `sla_policies`, `ticket_sla_tracking`, `sla_escalation_rules`
- **Organisation** : `teams`, `team_members`
- **Notifications** : `notifications`, `notification_preferences`, `email_queue`
- **Config & Audit** : `company_settings`, `audit_logs` (append-only, partitionnée)
- **RGPD** : `gdpr_consent_log`, `data_processing_log`

Points notables : FK composites `(entity_id, company_id)` pour l'intégrité tenant, soft-delete (`deleted_at`), génération de `ticket_number` par trigger, contraintes `CHECK`, deux rôles SQL (`app_user` RLS-enforced / `app_system` BYPASSRLS).

---

## Méthode A — Docker, prêt à l'emploi (recommandé)

Toute la stack (PostgreSQL + API + frontend Angular) démarre avec **une seule commande**. Application sur **http://localhost:8080**, API sur **http://localhost:5083**. Prérequis : **Docker** uniquement.

### A.1 — Build depuis le code source (archive ZIP / dépôt local)

Les deux dépôts doivent être côte à côte :

```
<dossier-parent>/
├── IETC-TKT-API/   ← exécuter la commande ici
└── IETC-TKT-WEB/
```

```bash
docker compose -f docker-compose.full.yml up --build
```

Construit l'API (.NET 10) et le front (Angular 21), lance PostgreSQL 18 et charge le schéma + les **comptes de démo**.

### A.2 — Images pré-construites (GitHub Container Registry, aucun build)

Aucun code source du frontend requis : les images publiques sont tirées de `ghcr.io`. Depuis le dépôt `IETC-TKT-API` (qui contient les scripts SQL) :

```bash
docker compose -f docker-compose.ghcr.yml up
```

Images : `ghcr.io/zekjulien/ietc-tkt-api` et `ghcr.io/zekjulien/ietc-tkt-web`.

> Repartir d'une base propre : ajouter `down -v` (ex. `docker compose -f docker-compose.full.yml down -v`).
> Comptes de démo : voir « Comptes de démo » plus bas. Mot de passe commun **`Demo1234`**.

---

## Méthode B — Lancement manuel (étape par étape)

Sans Docker tout-en-un. Pour le développement et la modification de code en direct.

### Prérequis
- **.NET 10 SDK**
- **Docker** (lance PostgreSQL 18 + initialise tout) — ou un PostgreSQL 18 local

### 1. Base de données (Docker)
```bash
cp .env.example .env          # ajuste les mots de passe si besoin
docker compose up -d
```
Au **premier démarrage** (volume vide), le conteneur exécute dans l'ordre :
1. `Database/tkt.sql` — schéma complet + rôles `app_user` / `app_system`
2. `Database/dev-credentials.sh` — mots de passe des rôles (depuis `.env`)
3. `Database/seed.sql` — **données de démonstration** (voir « Comptes de démo »)

> Pour repartir d'une base propre : `docker compose down -v && docker compose up -d`.

### 2. Configuration
La connection string de dev est dans `TKT.Api/appsettings.Development.json` (base `ticketing_system`, user `app_user`). Les mots de passe du conteneur viennent du `.env` (gitignoré ; voir `.env.example`).

### 3. Lancer l'API
```bash
dotnet run --project TKT.Api
```
- API : `http://localhost:5083`
- Doc Scalar (dev) : `http://localhost:5083/scalar`
- OpenAPI JSON : `http://localhost:5083/openapi/v1.json`

### 4. Tests
```bash
dotnet test
```

---

## Comptes de démo

Chargés automatiquement par `Database/seed.sql`. **Mot de passe commun : `Demo1234`.** Tous les comptes sont **déjà confirmés** → connexion directe via `POST /api/auth/login`.

| Email | Nom | Rôle(s) |
|---|---|---|
| `owner@acme.test` | Alice Martin | **owner** Acme (+ admin Globex) — *multi-tenant* |
| `admin@acme.test` | Bruno Lefebvre | admin (Acme) |
| `agent1@acme.test` | Chloé Dubois | agent (Acme) |
| `agent2@acme.test` | David Bernard | agent (Acme) |
| `member1@acme.test` | Emma Rousseau | member (Acme) |
| `member2@acme.test` | Lucas Moreau | member (Acme) |
| `nadia@acme.test` | Nadia Haddad | **member chez Acme + agent chez Globex** — *multi-tenant* |
| `owner@globex.test` | Sophie Laurent | owner (Globex) |

Le seed contient 2 entreprises (Acme en plan **Pro**, Globex en **Free**), des tickets dans tous les statuts (assignés et non assignés), des commentaires publics/internes et des tags.

**Multi-tenant** : un compte membre de plusieurs entreprises (`owner@acme.test`, `nadia@acme.test`) doit choisir son tenant actif via `POST /api/auth/switch-tenant` `{ "companyId": "..." }` ; le token renvoyé porte alors le contexte de l'entreprise.

> **Compte créé manuellement** : l'envoi d'email est simulé en dev (`ConsoleEmailSender`). Le lien de confirmation (`GET /api/auth/confirm-email?token=...`) est **affiché dans la console de l'API**. Les comptes de démo ci-dessus évitent cette étape.

---

## Conventions
- **Erreurs métier** : exceptions de `TKT.Core/Domain/Exceptions`, traduites en HTTP par `ExceptionHandlingMiddleware`.
- **Mapping** : explicite, à la main (pas d'AutoMapper).
- **Async** : de bout en bout.
- **Secrets locaux** : dans `.env` (gitignoré) ; voir `.env.example`.
