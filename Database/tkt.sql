-- ============================================================
-- SCRIPT v4.6 — Système de Ticketing Multi-Tenant (Privacy by Design)
-- Base de Données : PostgreSQL 18
-- Stack cible : ASP.NET 10 (C# 14) + Dapper + Angular 20+
-- Auteur : Paquet Julien — BAC2 Informatique
-- Basé sur : v4.5 — RLS tenant strict + autorisations applicatives
-- ============================================================
--
-- CHANGEMENTS vs v4.5 (partitionnement à blanc de audit_logs) :
--   - audit_logs déclarée PARTITION BY RANGE (created_at) avec partition
--     par défaut unique (audit_logs_default).
--     Justification : préparer la table à la montée en charge SaaS
--     (plusieurs centaines de tenants × milliers d'événements/jour) sans
--     qu'aucune migration douloureuse ne soit nécessaire le jour venu.
--     À grande échelle, il suffira de créer des partitions mensuelles
--     dédiées et d'y migrer les données via DETACH/ATTACH PARTITION
--     CONCURRENTLY, opération atomique et sans downtime — alors que
--     partitionner après coup une table existante non partitionnée
--     impliquerait recopie complète des données et fenêtre de
--     maintenance.
--     Coût actuel : nul — une partition unique se comporte comme une
--     table normale. Le code Dapper ne change pas (les requêtes sur
--     audit_logs restent identiques, le moteur route automatiquement).
--   - PRIMARY KEY de audit_logs étendue à (log_id, created_at) :
--     contrainte technique de PostgreSQL pour les tables partitionnées
--     (la PK doit inclure la colonne de partitionnement). Aucun impact
--     applicatif puisque log_id reste l'identifiant logique unique
--     (uuidv7() garantit l'unicité globale).
--
-- CHANGEMENTS apportés par v4.5 vs v4.4 :
--   - Policy RLS rls_tickets_sel SIMPLIFIÉE : retour à un filtre tenant
--     strict (company_id = current_company_id()) sans logique de rôle.
--     Justification : le filtre "member ne voit que ses propres tickets"
--     est désormais traité en couche applicative via TicketAuthorizationPolicy.
--     Avantages :
--       (1) Performance : comparaison scalaire indexée, pas de sous-requête
--           EXISTS sur company_members à chaque lecture
--       (2) Évolutivité : ajouter un nouveau rôle, modifier les règles de
--           visibilité, ou ajouter des permissions granulaires (visibility
--           scope par équipe, par catégorie, etc.) ne nécessite plus de
--           migration SQL — uniquement du code C#
--       (3) Séparation des préoccupations : la DB garantit l'INVIOLABLE
--           (isolation tenant, qui ne doit jamais échouer), l'app gère
--           l'ÉVOLUTIF (permissions métier, qui changent)
--
-- CHANGEMENTS apportés par v4.4 vs v4.3 :
--   - enum_plan_type simplifié : ('free', 'pro') au lieu de 4 plans
--   - company_subscriptions : DEFAULT alignés sur le plan Free de la doc
--     (max_users=3, max_tickets_per_month=50, plan_status='active')
--   - tickets : ajout colonne search_vector + index GIN (US 3.8)
--   - sla_policies : contrainte UNIQUE (company_id, priority, category_id)
--     avec NULLS NOT DISTINCT (US 5.1)
--   - RLS tickets enrichie : un member ne voit que ses propres tickets,
--     les rôles staff (owner/admin/agent) voient tous les tickets (US 3.2)
--   - Nouvelle table pending_invitations : invitations en attente pour
--     utilisateurs sans compte préexistant (US 2.1, US 1.2)
--   - Nouvelle table notification_preferences : toggles par type x canal
--     (US 7.3)
--   - Triggers d'audit automatiques sur tickets, ticket_comments,
--     company_members, sla_policies (US 8.3) — fonction générique
--     trg_audit_log() exploitant TG_OP, current_user_id() et to_jsonb()
--
-- CHANGEMENTS apportés par v4.3 vs v4.2.1 :
--   - SÉPARATION AUTH / PROFIL (Privacy by Design — Art. 25 RGPD)
--     • users → accounts (authentification pure : email, password_hash,
--       MFA, lockout, security stamps). Aucune PII ici.
--     • Nouvelle table user_profiles (PII : first_name, last_name,
--       avatar_url, timezone, language). Relation 1:1 avec accounts.
--     • Toutes les FK user_id → account_id (rename sémantique).
--   - MULTI-TENANT inchangé : company_members lie account ↔ company.
--     Un compte peut appartenir à N companies.
--   - NOUVELLES TABLES RGPD :
--     • gdpr_consent_log : registre des consentements (Art. 7)
--     • data_processing_log : registre des traitements (Art. 30)
--   - ANONYMISATION : fonction anonymize_account() pour le droit à
--     l'effacement (Art. 17). Supprime user_profiles, anonymise
--     accounts.email, purge les données sensibles. Les FK restent
--     intactes (tombstone pattern).
--   - RLS ajustée pour accounts + user_profiles.
--   - Tout le reste (FK composites, RLS tenant, soft delete, triggers,
--     audit_logs append-only, dual-pool roles) est conservé.
--
-- NOTE Application — UUIDv7 :
--   En production, les UUIDv7 sont générés côté C# via Guid.CreateVersion7()
--   pour éviter le RETURNING après chaque INSERT (la PK est connue avant
--   la requête). Le DEFAULT uuidv7() en DB sert de fallback pour les accès
--   directs, migrations et seeds.
--
-- NOTE Application — company_id coherence :
--   Les tables enfants et de liaison portent un company_id dénormalisé
--   nécessaire pour la RLS. La cohérence avec le parent est garantie par
--   DEUX couches :
--     Couche 1 (DB) : FK composites (parent_id, company_id) → parent(id, company_id)
--     Couche 2 (App) : un wrapper Dapper (DapperContext / repository de base)
--                      injecte automatiquement @company_id dans les
--                      DynamicParameters de chaque requête, depuis le
--                      ITenantContext (claim JWT de la requête HTTP).
--   PostgreSQL refuse toute insertion dont le company_id ne correspond pas
--   au parent, même via raw SQL ou accès direct.
--
-- NOTE RGPD — Séparation auth/profil :
--   La séparation permet :
--   • Droit à l'effacement (Art. 17) : DELETE user_profiles, anonymize accounts
--   • Minimisation (Art. 5.1.c) : services auth n'accèdent jamais aux PII
--   • Portabilité (Art. 20) : export user_profiles en JSON
--   • Rétention différenciée : profils purgés après X mois, comptes
--     conservés Y années pour l'audit
--   • Base légale distincte : accounts = exécution du contrat,
--     user_profiles = consentement ou intérêt légitime
--
-- 28 TABLES :
--   Auth & Identity : accounts, user_profiles, refresh_tokens
--   Tenants         : companies, company_subscriptions, company_members,
--                     pending_invitations
--   Ticketing       : categories, tickets, ticket_comments, tags, ticket_tags
--   Attachments     : attachments, ticket_attachments, comment_attachments
--   SLA             : sla_policies, ticket_sla_tracking, sla_escalation_rules
--   Organisation    : teams, team_members
--   Notifications   : notifications, notification_preferences, email_queue
--   Config & Audit  : company_settings, audit_logs
--   RGPD            : gdpr_consent_log, data_processing_log
--   Séquences       : company_sequences
-- ============================================================

DROP DATABASE IF EXISTS ticketing_system;
CREATE DATABASE ticketing_system;
\c ticketing_system;

CREATE EXTENSION IF NOT EXISTS "pgcrypto";
CREATE EXTENSION IF NOT EXISTS "btree_gist";


-- ============================================================
-- 1. (enums remplaces par des CHECK constraints sur chaque colonne)
-- ============================================================




-- ============================================================
-- 2. FONCTION UTILITAIRE
-- ============================================================

CREATE OR REPLACE FUNCTION trg_set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- NOTE : Ces triggers updated_at sont conservés volontairement.
-- Coût : ~microsecondes par UPDATE. Bénéfice : avec Dapper, le code applicatif
-- ne dispose pas d'auto-update built-in comme pourrait l'offrir un ORM complet.
-- Le trigger DB est donc la source de vérité unique pour updated_at, ce qui
-- garantit la cohérence du champ pour toutes les écritures : requêtes Dapper,
-- scripts de migration, requêtes manuelles via psql. Le code applicatif n'a
-- pas à s'en préoccuper.


-- ============================================================
-- 3. TABLES
-- ============================================================

-- ─── 1. accounts (ex "users" — auth/credentials UNIQUEMENT) ──
-- Contient ZÉRO donnée personnelle identifiante (PII).
-- Compatible ASP.NET Identity : colonnes Identity standards.
-- Les PII (nom, prénom, avatar…) sont dans user_profiles (table 2).
-- Justification RGPD :
--   • Minimisation (Art. 5.1.c) : services auth n'accèdent jamais aux PII
--   • Effacement (Art. 17) : anonymize accounts + DELETE user_profiles
--   • Rétention différenciée : auth conservé pour audit, PII purgeable
CREATE TABLE accounts (
    account_id              UUID PRIMARY KEY DEFAULT uuidv7(),

    -- ASP.NET Identity standard fields
    email                   VARCHAR(255) NOT NULL,
    normalized_email        VARCHAR(255) NOT NULL,
    email_confirmed         BOOLEAN NOT NULL DEFAULT FALSE,
    password_hash           VARCHAR(255) NOT NULL DEFAULT '',
    security_stamp          VARCHAR(255) DEFAULT gen_random_uuid()::TEXT,
    concurrency_stamp       VARCHAR(255) DEFAULT gen_random_uuid()::TEXT,
    phone_number            VARCHAR(20),
    phone_number_confirmed  BOOLEAN NOT NULL DEFAULT FALSE,
    two_factor_enabled      BOOLEAN NOT NULL DEFAULT FALSE,
    lockout_end             TIMESTAMPTZ,
    lockout_enabled         BOOLEAN NOT NULL DEFAULT TRUE,
    access_failed_count     INT NOT NULL DEFAULT 0,

    -- Custom auth fields
    mfa_secret              VARCHAR(255),
    password_changed_at     TIMESTAMPTZ DEFAULT NOW(),
    last_login_at           TIMESTAMPTZ,
    last_login_ip           INET,
    is_active               BOOLEAN NOT NULL DEFAULT TRUE,

    -- RGPD : horodatage d'anonymisation (Art. 17)
    -- NULL = compte actif, NOT NULL = compte anonymisé (tombstone)
    anonymized_at           TIMESTAMPTZ,

    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_account_email UNIQUE (email),
    CONSTRAINT uq_account_normalized_email UNIQUE (normalized_email)
);

CREATE INDEX idx_accounts_active ON accounts (is_active) WHERE is_active = TRUE;
CREATE INDEX idx_accounts_locked ON accounts (lockout_end) WHERE lockout_end IS NOT NULL;
CREATE INDEX idx_accounts_anonymized ON accounts (anonymized_at) WHERE anonymized_at IS NOT NULL;

CREATE TRIGGER trg_accounts_updated
    BEFORE UPDATE ON accounts
    FOR EACH ROW EXECUTE FUNCTION trg_set_updated_at();


-- ─── 2. user_profiles (PII — données personnelles) ───────────
-- Relation 1:1 avec accounts. Contient UNIQUEMENT les PII.
-- Cycle de vie indépendant : peut être supprimé sans casser les FK
-- du système (tickets, audit_logs, etc. pointent vers accounts).
-- RGPD :
--   • Effacement (Art. 17) : DELETE FROM user_profiles WHERE account_id = ?
--   • Portabilité (Art. 20) : SELECT * FROM user_profiles WHERE account_id = ?
--   • Rectification (Art. 16) : UPDATE user_profiles SET ... WHERE account_id = ?
CREATE TABLE user_profiles (
    profile_id              UUID PRIMARY KEY DEFAULT uuidv7(),
    account_id              UUID NOT NULL REFERENCES accounts(account_id) ON DELETE CASCADE,

    first_name              VARCHAR(100) NOT NULL,
    last_name               VARCHAR(100) NOT NULL,
    display_name            VARCHAR(200) GENERATED ALWAYS AS (first_name || ' ' || last_name) STORED,
    avatar_url              VARCHAR(500),
    timezone                VARCHAR(50) NOT NULL DEFAULT 'Europe/Brussels',
    language                VARCHAR(5) NOT NULL DEFAULT 'fr' CHECK (language ~ '^[a-z]{2}(-[A-Z]{2})?$'),

    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- 1:1 strict : un seul profil par compte
    CONSTRAINT uq_profile_account UNIQUE (account_id)
);

CREATE TRIGGER trg_profiles_updated
    BEFORE UPDATE ON user_profiles
    FOR EACH ROW EXECUTE FUNCTION trg_set_updated_at();


-- ─── 3. refresh_tokens (RFC 9700 §4.14) ──────────────────────
-- NOTE : Prévoir un BackgroundService C# pour purger les tokens expirés/révoqués.
-- Requête de purge : DELETE FROM refresh_tokens WHERE expires_at < NOW() - INTERVAL '30 days';
CREATE TABLE refresh_tokens (
    token_id            UUID PRIMARY KEY DEFAULT uuidv7(),
    account_id          UUID NOT NULL REFERENCES accounts(account_id) ON DELETE CASCADE,

    -- Famille : tous les tokens d'une chaîne de rotation
    family_id           UUID NOT NULL DEFAULT uuidv7(),

    -- Jamais le token en clair, toujours SHA-256
    token_hash          VARCHAR(64) NOT NULL,

    -- Chaînage de rotation
    replaced_by_id      UUID REFERENCES refresh_tokens(token_id) ON DELETE SET NULL,

    -- Lifetimes (RFC 9700 : nouveau RT ne dépasse jamais absolute_expires_at)
    expires_at          TIMESTAMPTZ NOT NULL,
    absolute_expires_at TIMESTAMPTZ NOT NULL,

    -- État
    is_revoked          BOOLEAN NOT NULL DEFAULT FALSE,
    revoked_at          TIMESTAMPTZ,
    revoked_reason      VARCHAR(50),  -- 'rotated', 'reuse_detected', 'logout', 'admin'

    -- Metadata sécurité
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by_ip       INET,
    user_agent          TEXT,

    CONSTRAINT uq_token_hash UNIQUE (token_hash)
);

CREATE INDEX idx_rt_family ON refresh_tokens (family_id);
CREATE INDEX idx_rt_account ON refresh_tokens (account_id);
CREATE INDEX idx_rt_active ON refresh_tokens (token_hash) WHERE is_revoked = FALSE;
CREATE INDEX idx_rt_cleanup ON refresh_tokens (expires_at) WHERE is_revoked = FALSE;


-- ─── 4. companies ─────────────────────────────────────────────
CREATE TABLE companies (
    company_id    UUID PRIMARY KEY DEFAULT uuidv7(),
    company_name  VARCHAR(255) NOT NULL,
    company_slug  VARCHAR(100) NOT NULL CHECK (company_slug ~ '^[a-z0-9][a-z0-9-]*[a-z0-9]$'),
    domain        VARCHAR(255),
    logo_url      VARCHAR(500),
    description   TEXT,
    is_active     BOOLEAN NOT NULL DEFAULT TRUE,

    -- Soft delete (enforced : app_user n'a pas DELETE sur cette table)
    deleted_at    TIMESTAMPTZ,
    deleted_by    UUID REFERENCES accounts(account_id) ON DELETE SET NULL,

    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_company_name UNIQUE (company_name),
    CONSTRAINT uq_company_slug UNIQUE (company_slug)
);

CREATE INDEX idx_companies_active ON companies (is_active) WHERE is_active = TRUE AND deleted_at IS NULL;

CREATE TRIGGER trg_companies_updated
    BEFORE UPDATE ON companies
    FOR EACH ROW EXECUTE FUNCTION trg_set_updated_at();


-- ─── 5. company_subscriptions ─────────────────────────────────
CREATE TABLE company_subscriptions (
    subscription_id        UUID PRIMARY KEY DEFAULT uuidv7(),
    company_id             UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    stripe_customer_id     VARCHAR(255),
    stripe_subscription_id VARCHAR(255),
    plan_type              VARCHAR(20) NOT NULL DEFAULT 'free' CHECK (plan_type IN ('free', 'pro')),
    plan_status            VARCHAR(20) NOT NULL DEFAULT 'active' CHECK (plan_status IN ('active', 'canceled', 'past_due', 'trialing')),
    max_users              INT NOT NULL DEFAULT 3 CHECK (max_users > 0),
    max_tickets_per_month  INT NOT NULL DEFAULT 50 CHECK (max_tickets_per_month > 0),
    max_storage_gb         NUMERIC(10,2) NOT NULL DEFAULT 1.00 CHECK (max_storage_gb >= 0),
    features               JSONB NOT NULL DEFAULT '{}',
    trial_ends_at          TIMESTAMPTZ,
    valid_during           TSTZRANGE NOT NULL DEFAULT tstzrange(NOW(), 'infinity', '[)'),
    canceled_at            TIMESTAMPTZ,
    created_at             TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at             TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_sub_company_period UNIQUE (company_id, valid_during WITHOUT OVERLAPS)
);

CREATE INDEX idx_sub_stripe ON company_subscriptions (stripe_customer_id) WHERE stripe_customer_id IS NOT NULL;
CREATE INDEX idx_sub_status ON company_subscriptions (plan_status);
CREATE INDEX idx_sub_features ON company_subscriptions USING GIN (features);

CREATE TRIGGER trg_subscriptions_updated
    BEFORE UPDATE ON company_subscriptions
    FOR EACH ROW EXECUTE FUNCTION trg_set_updated_at();


-- ─── 6. company_members ───────────────────────────────────────
-- Table de liaison account ↔ company (multi-tenant N:N).
-- Un account peut être membre de plusieurs companies.
-- Le department et job_title sont ici car ils sont spécifiques
-- à la relation account↔company, pas au profil global.
CREATE TABLE company_members (
    membership_id  UUID PRIMARY KEY DEFAULT uuidv7(),
    company_id     UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    account_id     UUID NOT NULL REFERENCES accounts(account_id) ON DELETE CASCADE,
    role           VARCHAR(20) NOT NULL DEFAULT 'member' CHECK (role IN ('owner', 'admin', 'agent', 'member')),
    is_active      BOOLEAN NOT NULL DEFAULT TRUE,
    department     VARCHAR(100),
    job_title      VARCHAR(100),
    invited_by     UUID REFERENCES accounts(account_id) ON DELETE SET NULL,
    invited_at     TIMESTAMPTZ DEFAULT NOW(),
    joined_at      TIMESTAMPTZ,
    deactivated_at TIMESTAMPTZ,

    CONSTRAINT uq_member_per_company UNIQUE (company_id, account_id)
);

CREATE INDEX idx_cm_company ON company_members (company_id);
CREATE INDEX idx_cm_account ON company_members (account_id);
CREATE INDEX idx_cm_active ON company_members (company_id, is_active) WHERE is_active = TRUE;
-- Index pour la requête inverse "dans quelles companies est ce compte" (login multi-tenant)
CREATE INDEX idx_cm_account_companies ON company_members (account_id, company_id) WHERE is_active = TRUE;


-- ─── 6b. pending_invitations (US 2.1 — invitations sans compte) ─
-- Permet d'inviter un email qui n'a pas encore de compte. À l'acceptation
-- (création du compte ou login si le compte est créé entre-temps), une
-- ligne company_members est créée et l'invitation passe à 'accepted'.
-- Si l'email correspond déjà à un account, on peut soit insérer directement
-- dans company_members soit utiliser cette table comme journal d'invitations.
CREATE TABLE pending_invitations (
    invitation_id   UUID PRIMARY KEY DEFAULT uuidv7(),
    company_id      UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    email           VARCHAR(255) NOT NULL,
    role            VARCHAR(20) NOT NULL DEFAULT 'member' CHECK (role IN ('owner', 'admin', 'agent', 'member')),
    department      VARCHAR(100),
    job_title       VARCHAR(100),
    invitation_code VARCHAR(64) NOT NULL,         -- token unique envoyé par email
    invited_by      UUID NOT NULL REFERENCES accounts(account_id) ON DELETE CASCADE,
    invited_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at      TIMESTAMPTZ NOT NULL DEFAULT (NOW() + INTERVAL '7 days'),
    accepted_at     TIMESTAMPTZ,
    accepted_by     UUID REFERENCES accounts(account_id) ON DELETE SET NULL,
    revoked_at      TIMESTAMPTZ,

    CONSTRAINT uq_invitation_code UNIQUE (invitation_code),
    -- Une seule invitation active par (company, email)
    CONSTRAINT uq_pending_invitation UNIQUE (company_id, email, accepted_at)
);

CREATE INDEX idx_pi_company ON pending_invitations (company_id);
CREATE INDEX idx_pi_email ON pending_invitations (email) WHERE accepted_at IS NULL AND revoked_at IS NULL;
CREATE INDEX idx_pi_code ON pending_invitations (invitation_code) WHERE accepted_at IS NULL AND revoked_at IS NULL;
CREATE INDEX idx_pi_expires ON pending_invitations (expires_at) WHERE accepted_at IS NULL AND revoked_at IS NULL;


-- ─── 7. categories ────────────────────────────────────────────
CREATE TABLE categories (
    category_id   UUID PRIMARY KEY DEFAULT uuidv7(),
    company_id    UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    parent_id     UUID,
    category_name VARCHAR(100) NOT NULL,
    description   TEXT,
    color         VARCHAR(7) CHECK (color ~ '^#[0-9a-fA-F]{6}$'),
    icon          VARCHAR(50),
    sort_order    INT NOT NULL DEFAULT 0,
    is_active     BOOLEAN NOT NULL DEFAULT TRUE,

    CONSTRAINT uq_category_per_company UNIQUE (company_id, category_name),
    -- FK composite target : permet aux tables enfants de valider (category_id, company_id)
    CONSTRAINT uq_category_company UNIQUE (category_id, company_id),
    -- FK composite : parent_id doit être dans la même company
    CONSTRAINT fk_cat_parent_company FOREIGN KEY (parent_id, company_id)
        REFERENCES categories(category_id, company_id) ON DELETE SET NULL
);

CREATE INDEX idx_categories_company ON categories (company_id);


-- ─── 8. teams ─────────────────────────────────────────────────
CREATE TABLE teams (
    team_id     UUID PRIMARY KEY DEFAULT uuidv7(),
    company_id  UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    team_name   VARCHAR(100) NOT NULL,
    description TEXT,
    team_lead   UUID REFERENCES accounts(account_id) ON DELETE SET NULL,
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT uq_team_per_company UNIQUE (company_id, team_name),
    -- FK composite target : permet aux tables enfants de valider (team_id, company_id)
    CONSTRAINT uq_team_company UNIQUE (team_id, company_id)
);

CREATE TRIGGER trg_teams_updated
    BEFORE UPDATE ON teams
    FOR EACH ROW EXECUTE FUNCTION trg_set_updated_at();


-- ─── 9. team_members ──────────────────────────────────────────
CREATE TABLE team_members (
    team_id    UUID NOT NULL,
    account_id UUID NOT NULL REFERENCES accounts(account_id) ON DELETE CASCADE,
    company_id UUID NOT NULL,
    joined_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    PRIMARY KEY (team_id, account_id),
    -- FK composite : garantit que team et team_member sont dans la même company
    CONSTRAINT fk_tm_team_company FOREIGN KEY (team_id, company_id)
        REFERENCES teams(team_id, company_id) ON DELETE CASCADE
);

CREATE INDEX idx_team_members_company ON team_members (company_id);


-- ─── 10. company_sequences (remplacement MAX() + advisory lock) ─
CREATE TABLE company_sequences (
    company_id    UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    sequence_type VARCHAR(50) NOT NULL DEFAULT 'ticket',
    last_value    INT NOT NULL DEFAULT 0,

    PRIMARY KEY (company_id, sequence_type)
);


-- ─── 11. tickets ──────────────────────────────────────────────
CREATE TABLE tickets (
    ticket_id     UUID PRIMARY KEY DEFAULT uuidv7(),
    company_id    UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    ticket_number VARCHAR(50) NOT NULL,
    title         VARCHAR(255) NOT NULL CHECK (length(title) >= 3),
    description   TEXT,
    status        VARCHAR(20) NOT NULL DEFAULT 'open' CHECK (status IN ('open', 'in_progress', 'pending', 'resolved', 'closed')),
    priority      VARCHAR(20) NOT NULL DEFAULT 'medium' CHECK (priority IN ('low', 'medium', 'high', 'urgent')),
    created_by    UUID NOT NULL REFERENCES accounts(account_id),
    assigned_to   UUID REFERENCES accounts(account_id) ON DELETE SET NULL,
    team_id       UUID,
    category_id   UUID,
    source        VARCHAR(50) NOT NULL DEFAULT 'web',
    due_date      TIMESTAMPTZ,
    is_locked     BOOLEAN NOT NULL DEFAULT FALSE,

    -- Soft delete (enforced : app_user n'a pas DELETE sur cette table)
    deleted_at    TIMESTAMPTZ,
    deleted_by    UUID REFERENCES accounts(account_id) ON DELETE SET NULL,

    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    resolved_at   TIMESTAMPTZ,
    closed_at     TIMESTAMPTZ,

    -- Recherche full-text (US 3.8) : titre pondéré 'A', description 'B'
    -- Configuration 'french' pour le stemming (imprimante → imprimer, imprimantes)
    search_vector tsvector GENERATED ALWAYS AS (
        setweight(to_tsvector('french', coalesce(title, '')), 'A') ||
        setweight(to_tsvector('french', coalesce(description, '')), 'B')
    ) STORED,

    CONSTRAINT uq_ticket_number_per_company UNIQUE (company_id, ticket_number),
    -- FK composite target : permet aux tables enfants de valider (ticket_id, company_id)
    CONSTRAINT uq_ticket_company UNIQUE (ticket_id, company_id),
    -- FK composites : team et category doivent être dans la même company
    CONSTRAINT fk_tickets_team_company FOREIGN KEY (team_id, company_id)
        REFERENCES teams(team_id, company_id) ON DELETE SET NULL,
    CONSTRAINT fk_tickets_category_company FOREIGN KEY (category_id, company_id)
        REFERENCES categories(category_id, company_id) ON DELETE SET NULL
);

CREATE INDEX idx_tickets_company ON tickets (company_id);
CREATE INDEX idx_tickets_status ON tickets (company_id, status);
CREATE INDEX idx_tickets_priority ON tickets (company_id, priority);
CREATE INDEX idx_tickets_created_by ON tickets (created_by);
CREATE INDEX idx_tickets_assigned ON tickets (assigned_to) WHERE assigned_to IS NOT NULL;
CREATE INDEX idx_tickets_created_at ON tickets (company_id, created_at DESC);
CREATE INDEX idx_tickets_open ON tickets (company_id, created_at) WHERE status IN ('open', 'in_progress') AND deleted_at IS NULL;
CREATE INDEX idx_tickets_active ON tickets (company_id) WHERE deleted_at IS NULL;
-- Index GIN pour la recherche full-text (US 3.8)
CREATE INDEX idx_tickets_search ON tickets USING GIN (search_vector);

CREATE TRIGGER trg_tickets_updated
    BEFORE UPDATE ON tickets
    FOR EACH ROW EXECUTE FUNCTION trg_set_updated_at();


-- ─── 12. ticket_comments ──────────────────────────────────────
CREATE TABLE ticket_comments (
    comment_id   UUID PRIMARY KEY DEFAULT uuidv7(),
    company_id   UUID NOT NULL,
    ticket_id    UUID NOT NULL,
    account_id   UUID NOT NULL REFERENCES accounts(account_id),
    reply_to_id  UUID REFERENCES ticket_comments(comment_id) ON DELETE SET NULL,
    comment_text TEXT NOT NULL CHECK (length(comment_text) >= 1),
    is_internal  BOOLEAN NOT NULL DEFAULT FALSE,
    edited_at    TIMESTAMPTZ,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- FK composite : garantit que comment et ticket sont dans la même company
    CONSTRAINT fk_tc_ticket_company FOREIGN KEY (ticket_id, company_id)
        REFERENCES tickets(ticket_id, company_id) ON DELETE CASCADE,
    -- FK composite target : permet à comment_attachments de valider (comment_id, company_id)
    CONSTRAINT uq_comment_company UNIQUE (comment_id, company_id)
);

CREATE INDEX idx_comments_ticket ON ticket_comments (ticket_id, created_at);
CREATE INDEX idx_comments_company ON ticket_comments (company_id);
CREATE INDEX idx_comments_internal ON ticket_comments (ticket_id) WHERE is_internal = TRUE;

CREATE TRIGGER trg_comments_updated
    BEFORE UPDATE ON ticket_comments
    FOR EACH ROW EXECUTE FUNCTION trg_set_updated_at();


-- ─── 13. attachments (table centrale — métadonnées fichier) ───
-- Table de métadonnées pure : un attachment = un fichier physique.
-- Le lien avec les entités parentes passe par des tables de liaison
-- dédiées (ticket_attachments, comment_attachments, etc.).
-- Extensibilité : ajouter une nouvelle entité = une nouvelle table de
-- liaison. Zéro modification des tables existantes (Open/Closed SOLID).
-- Intégrité : FK composites PostgreSQL sur chaque liaison, mapping vers
-- des POCOs C# typés côté Dapper (Attachment, TicketAttachment, etc.).
CREATE TABLE attachments (
    attachment_id     UUID PRIMARY KEY DEFAULT uuidv7(),
    company_id        UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    filename          VARCHAR(255) NOT NULL,
    original_filename VARCHAR(255) NOT NULL,
    file_path         VARCHAR(500) NOT NULL,
    file_size         BIGINT NOT NULL CHECK (file_size > 0),
    mime_type         VARCHAR(100) NOT NULL,
    file_hash         VARCHAR(64),
    thumbnail_path    VARCHAR(500),
    uploaded_by       UUID NOT NULL REFERENCES accounts(account_id),
    uploaded_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- FK composite target : permet aux tables de liaison de valider (attachment_id, company_id)
    CONSTRAINT uq_attachment_company UNIQUE (attachment_id, company_id)
);

CREATE INDEX idx_att_company ON attachments (company_id);
CREATE INDEX idx_att_hash ON attachments (file_hash) WHERE file_hash IS NOT NULL;


-- ─── 13a. ticket_attachments (liaison ticket ↔ attachment) ────
CREATE TABLE ticket_attachments (
    ticket_id     UUID NOT NULL,
    attachment_id UUID NOT NULL,
    company_id    UUID NOT NULL,
    added_by      UUID REFERENCES accounts(account_id) ON DELETE SET NULL,
    added_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    PRIMARY KEY (ticket_id, attachment_id),
    -- FK composites : ticket et attachment doivent être dans la même company
    CONSTRAINT fk_ta_ticket_company FOREIGN KEY (ticket_id, company_id)
        REFERENCES tickets(ticket_id, company_id) ON DELETE CASCADE,
    CONSTRAINT fk_ta_attachment_company FOREIGN KEY (attachment_id, company_id)
        REFERENCES attachments(attachment_id, company_id) ON DELETE CASCADE
);

CREATE INDEX idx_ta_company ON ticket_attachments (company_id);
CREATE INDEX idx_ta_attachment ON ticket_attachments (attachment_id);


-- ─── 13b. comment_attachments (liaison comment ↔ attachment) ──
CREATE TABLE comment_attachments (
    comment_id    UUID NOT NULL,
    attachment_id UUID NOT NULL,
    company_id    UUID NOT NULL,
    added_by      UUID REFERENCES accounts(account_id) ON DELETE SET NULL,
    added_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    PRIMARY KEY (comment_id, attachment_id),
    -- FK composites : comment et attachment doivent être dans la même company
    CONSTRAINT fk_ca_comment_company FOREIGN KEY (comment_id, company_id)
        REFERENCES ticket_comments(comment_id, company_id) ON DELETE CASCADE,
    CONSTRAINT fk_ca_attachment_company FOREIGN KEY (attachment_id, company_id)
        REFERENCES attachments(attachment_id, company_id) ON DELETE CASCADE
);

CREATE INDEX idx_ca_company ON comment_attachments (company_id);
CREATE INDEX idx_ca_attachment ON comment_attachments (attachment_id);


-- ─── 14. tags ─────────────────────────────────────────────────
CREATE TABLE tags (
    tag_id     UUID PRIMARY KEY DEFAULT uuidv7(),
    company_id UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    tag_name   VARCHAR(50) NOT NULL,
    color      VARCHAR(7) CHECK (color ~ '^#[0-9a-fA-F]{6}$'),

    CONSTRAINT uq_tag_per_company UNIQUE (company_id, tag_name),
    -- FK composite target : permet à ticket_tags de valider (tag_id, company_id)
    CONSTRAINT uq_tag_company UNIQUE (tag_id, company_id)
);

CREATE INDEX idx_tags_company ON tags (company_id);


-- ─── 15. ticket_tags ──────────────────────────────────────────
CREATE TABLE ticket_tags (
    ticket_id  UUID NOT NULL,
    tag_id     UUID NOT NULL,
    company_id UUID NOT NULL,
    added_by   UUID REFERENCES accounts(account_id) ON DELETE SET NULL,
    added_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    PRIMARY KEY (ticket_id, tag_id),
    -- FK composites : ticket et tag doivent être dans la même company
    CONSTRAINT fk_tt_ticket_company FOREIGN KEY (ticket_id, company_id)
        REFERENCES tickets(ticket_id, company_id) ON DELETE CASCADE,
    CONSTRAINT fk_tt_tag_company FOREIGN KEY (tag_id, company_id)
        REFERENCES tags(tag_id, company_id) ON DELETE CASCADE
);

CREATE INDEX idx_ticket_tags_company ON ticket_tags (company_id);


-- ─── 16. sla_policies ────────────────────────────────────────
CREATE TABLE sla_policies (
    sla_policy_id          UUID PRIMARY KEY DEFAULT uuidv7(),
    company_id             UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    policy_name            VARCHAR(100) NOT NULL,
    description            TEXT,
    priority               VARCHAR(20) CHECK (priority IN ('low', 'medium', 'high', 'urgent')),
    category_id            UUID,
    first_response_time    INT NOT NULL CHECK (first_response_time > 0),
    resolution_time        INT NOT NULL CHECK (resolution_time > 0),
    escalation_after_pct   INT DEFAULT 80 CHECK (escalation_after_pct BETWEEN 1 AND 100),
    business_hours_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    business_hours         JSONB,
    is_active              BOOLEAN NOT NULL DEFAULT TRUE,
    priority_order         INT NOT NULL DEFAULT 0,
    created_at             TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at             TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- FK composite target : permet aux tables enfants de valider (sla_policy_id, company_id)
    CONSTRAINT uq_sla_policy_company UNIQUE (sla_policy_id, company_id),
    -- US 5.1 — Une seule politique par (priorité, catégorie) par tenant.
    -- NULLS NOT DISTINCT (PG15+) traite NULL comme une valeur : permet d'avoir
    -- une politique "priorité = high, catégorie = NULL" unique elle aussi.
    CONSTRAINT uq_sla_priority_category UNIQUE NULLS NOT DISTINCT (company_id, priority, category_id),
    -- FK composite : category doit être dans la même company
    CONSTRAINT fk_sla_category_company FOREIGN KEY (category_id, company_id)
        REFERENCES categories(category_id, company_id) ON DELETE SET NULL
);

CREATE INDEX idx_sla_company_active ON sla_policies (company_id) WHERE is_active = TRUE;

CREATE TRIGGER trg_sla_policies_updated
    BEFORE UPDATE ON sla_policies
    FOR EACH ROW EXECUTE FUNCTION trg_set_updated_at();


-- ─── 17. ticket_sla_tracking ─────────────────────────────────
CREATE TABLE ticket_sla_tracking (
    tracking_id                    UUID PRIMARY KEY DEFAULT uuidv7(),
    ticket_id                      UUID NOT NULL,
    company_id                     UUID NOT NULL,
    sla_policy_id                  UUID NOT NULL,
    first_response_due_at          TIMESTAMPTZ NOT NULL,
    first_response_at              TIMESTAMPTZ,
    first_response_breached        BOOLEAN NOT NULL DEFAULT FALSE,
    first_response_breach_duration INT,
    resolution_due_at              TIMESTAMPTZ NOT NULL,
    resolved_at                    TIMESTAMPTZ,
    resolution_breached            BOOLEAN NOT NULL DEFAULT FALSE,
    resolution_breach_duration     INT,
    is_paused                      BOOLEAN NOT NULL DEFAULT FALSE,
    paused_at                      TIMESTAMPTZ,
    total_paused_minutes           INT NOT NULL DEFAULT 0,
    escalated_at                   TIMESTAMPTZ,
    escalation_level               SMALLINT NOT NULL DEFAULT 0,
    created_at                     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at                     TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    -- Un seul tracking SLA par ticket
    CONSTRAINT uq_sla_track_ticket UNIQUE (ticket_id),
    -- FK composites : ticket et sla_policy doivent être dans la même company
    CONSTRAINT fk_st_ticket_company FOREIGN KEY (ticket_id, company_id)
        REFERENCES tickets(ticket_id, company_id) ON DELETE CASCADE,
    CONSTRAINT fk_st_policy_company FOREIGN KEY (sla_policy_id, company_id)
        REFERENCES sla_policies(sla_policy_id, company_id)
);

CREATE INDEX idx_sla_track_company ON ticket_sla_tracking (company_id);
CREATE INDEX idx_sla_track_breaches ON ticket_sla_tracking (first_response_breached, resolution_breached);
CREATE INDEX idx_sla_track_due ON ticket_sla_tracking (resolution_due_at) WHERE resolved_at IS NULL;

CREATE TRIGGER trg_sla_tracking_updated
    BEFORE UPDATE ON ticket_sla_tracking
    FOR EACH ROW EXECUTE FUNCTION trg_set_updated_at();


-- ─── 18. sla_escalation_rules ────────────────────────────────
CREATE TABLE sla_escalation_rules (
    rule_id       UUID PRIMARY KEY DEFAULT uuidv7(),
    sla_policy_id UUID NOT NULL,
    company_id    UUID NOT NULL,
    level         SMALLINT NOT NULL CHECK (level BETWEEN 1 AND 5),
    trigger_after_pct INT NOT NULL CHECK (trigger_after_pct BETWEEN 1 AND 200),
    action        VARCHAR(30) NOT NULL CHECK (action IN ('notify', 'reassign', 'change_priority', 'notify_manager')),
    target_user   UUID REFERENCES accounts(account_id) ON DELETE SET NULL,
    target_team   UUID,
    new_priority  VARCHAR(20) CHECK (new_priority IN ('low', 'medium', 'high', 'urgent')),
    notify_message TEXT,
    is_active     BOOLEAN NOT NULL DEFAULT TRUE,

    CONSTRAINT uq_escalation_level UNIQUE (sla_policy_id, level),
    -- FK composite : sla_policy et rule doivent être dans la même company
    CONSTRAINT fk_ser_policy_company FOREIGN KEY (sla_policy_id, company_id)
        REFERENCES sla_policies(sla_policy_id, company_id) ON DELETE CASCADE,
    -- FK composite : target_team doit être dans la même company
    CONSTRAINT fk_ser_team_company FOREIGN KEY (target_team, company_id)
        REFERENCES teams(team_id, company_id) ON DELETE SET NULL
);

CREATE INDEX idx_escalation_company ON sla_escalation_rules (company_id);


-- ─── 19. notifications ───────────────────────────────────────
CREATE TABLE notifications (
    notification_id   UUID PRIMARY KEY DEFAULT uuidv7(),
    company_id        UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    recipient_id      UUID NOT NULL REFERENCES accounts(account_id) ON DELETE CASCADE,
    notification_type VARCHAR(30) NOT NULL CHECK (notification_type IN ('ticket_assigned', 'ticket_updated', 'ticket_status_changed', 'comment_added', 'mention', 'sla_warning', 'sla_breach', 'ticket_escalated')),
    title             VARCHAR(255) NOT NULL,
    message           TEXT NOT NULL,
    entity_type       VARCHAR(50),
    entity_id         UUID,
    action_url        VARCHAR(500),
    channel           VARCHAR(20) NOT NULL DEFAULT 'in_app',
    is_read           BOOLEAN NOT NULL DEFAULT FALSE,
    read_at           TIMESTAMPTZ,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_notif_unread ON notifications (recipient_id, created_at DESC) WHERE is_read = FALSE;
CREATE INDEX idx_notif_company ON notifications (company_id);


-- ─── 19b. notification_preferences (US 7.3 — toggles utilisateur) ─
-- Préférences par compte, par type de notification, par canal.
-- Si pas de ligne pour une combinaison (account, type, channel) → enabled = TRUE
-- (fallback applicatif). Cela évite de pré-remplir 16 lignes par compte.
-- Le compte n'est PAS scopé par tenant : un utilisateur garde ses préférences
-- d'un tenant à l'autre.
CREATE TABLE notification_preferences (
    account_id        UUID NOT NULL REFERENCES accounts(account_id) ON DELETE CASCADE,
    notification_type VARCHAR(30) NOT NULL CHECK (notification_type IN ('ticket_assigned', 'ticket_updated', 'ticket_status_changed', 'comment_added', 'mention', 'sla_warning', 'sla_breach', 'ticket_escalated')),
    channel           VARCHAR(20) NOT NULL CHECK (channel IN ('in_app', 'email')),
    enabled           BOOLEAN NOT NULL DEFAULT TRUE,
    updated_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    PRIMARY KEY (account_id, notification_type, channel)
);

CREATE INDEX idx_notif_prefs_account ON notification_preferences (account_id);

CREATE TRIGGER trg_notif_prefs_updated
    BEFORE UPDATE ON notification_preferences
    FOR EACH ROW EXECUTE FUNCTION trg_set_updated_at();


-- ─── 20. email_queue ─────────────────────────────────────────
CREATE TABLE email_queue (
    queue_id        UUID PRIMARY KEY DEFAULT uuidv7(),
    company_id      UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    recipient_email VARCHAR(255) NOT NULL,
    recipient_name  VARCHAR(255),
    subject         VARCHAR(255) NOT NULL,
    body_html       TEXT NOT NULL,
    body_text       TEXT,
    headers         JSONB,  -- Point d'extension : Reply-To, X-Ticket-Id, References (threading email)
    email_type      VARCHAR(50) NOT NULL,
    entity_type     VARCHAR(50),
    entity_id       UUID,
    status          VARCHAR(20) NOT NULL DEFAULT 'pending' CHECK (status IN ('pending', 'processing', 'sent', 'failed', 'bounced')),
    attempts        SMALLINT NOT NULL DEFAULT 0 CHECK (attempts >= 0),
    max_attempts    SMALLINT NOT NULL DEFAULT 3 CHECK (max_attempts > 0),
    last_error      TEXT,
    scheduled_for   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    sent_at         TIMESTAMPTZ
);

CREATE INDEX idx_email_pending ON email_queue (status, scheduled_for) WHERE status IN ('pending', 'processing');


-- ─── 21. company_settings ────────────────────────────────────
CREATE TABLE company_settings (
    company_id UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    key        VARCHAR(100) NOT NULL,
    value      JSONB NOT NULL,
    updated_by UUID REFERENCES accounts(account_id) ON DELETE SET NULL,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    PRIMARY KEY (company_id, key)
);


-- ─── 22. audit_logs (append-only pour app_user, partitionnée) ──
-- app_user : SELECT + INSERT uniquement. Pas d'UPDATE ni DELETE.
-- app_system : accès complet pour rétention, archivage, purge.
--
-- PARTITIONNEMENT À BLANC (v4.6) :
--   La table est déclarée PARTITION BY RANGE (created_at) dès aujourd'hui
--   avec une seule partition par défaut. Coût opérationnel actuel : nul —
--   une partition unique se comporte comme une table normale.
--   Bénéfice : à grande échelle (SaaS multi-tenant, plusieurs centaines de
--   tenants), il suffira de créer des partitions mensuelles supplémentaires
--   et de migrer les données via DETACH PARTITION CONCURRENTLY, sans avoir
--   à reconstruire toute la table (qui aurait été une opération longue,
--   risquée et impliquant un downtime). Le code applicatif Dapper ne
--   change pas : les requêtes sur audit_logs restent identiques, le moteur
--   PostgreSQL route automatiquement vers les bonnes partitions.
--
-- CONSÉQUENCE TECHNIQUE :
--   La PRIMARY KEY d'une table partitionnée doit inclure la colonne de
--   partitionnement. La PK passe donc de log_id à (log_id, created_at).
--   Aucun impact sur le code applicatif : log_id reste l'identifiant
--   logique unique (généré via uuidv7() qui garantit l'unicité globale).
CREATE TABLE audit_logs (
    log_id      UUID NOT NULL DEFAULT uuidv7(),
    company_id  UUID NOT NULL REFERENCES companies(company_id) ON DELETE CASCADE,
    account_id  UUID REFERENCES accounts(account_id) ON DELETE SET NULL,
    action      VARCHAR(50) NOT NULL,
    entity_type VARCHAR(50) NOT NULL,
    entity_id   UUID NOT NULL,
    changes     JSONB,
    metadata    JSONB,
    ip_address  INET,
    user_agent  TEXT,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    PRIMARY KEY (log_id, created_at)
) PARTITION BY RANGE (created_at);

-- Partition par défaut couvrant toutes les dates. Pour le MVP, toutes les
-- entrées d'audit y atterrissent. À grande échelle, on pourra créer des
-- partitions mensuelles dédiées (ex: audit_logs_2027_01) et y migrer les
-- données via DETACH/ATTACH, sans toucher au code applicatif.
CREATE TABLE audit_logs_default PARTITION OF audit_logs DEFAULT;

CREATE INDEX idx_audit_company_date ON audit_logs (company_id, created_at DESC);
CREATE INDEX idx_audit_entity ON audit_logs (entity_type, entity_id);
CREATE INDEX idx_audit_action ON audit_logs (action);
CREATE INDEX idx_audit_account ON audit_logs (account_id);
CREATE INDEX idx_audit_changes ON audit_logs USING GIN (changes);


-- ─── 23. gdpr_consent_log (Art. 7 RGPD — registre consentements) ─
-- Append-only : app_user ne peut que SELECT + INSERT.
-- Chaque ligne = un consentement donné ou retiré, avec preuve.
-- NOTE : Le consentement n'est PAS un booléen dans user_profiles.
-- C'est un EVENT LOG. Le statut actuel se déduit du dernier événement
-- par (account_id, purpose) : dernier granted = TRUE → consentement actif.
CREATE TABLE gdpr_consent_log (
    consent_id   UUID PRIMARY KEY DEFAULT uuidv7(),
    account_id   UUID NOT NULL REFERENCES accounts(account_id) ON DELETE CASCADE,
    purpose      VARCHAR(30) NOT NULL CHECK (purpose IN ('terms_of_service', 'privacy_policy', 'marketing_email', 'analytics', 'third_party_sharing', 'profiling')),
    granted      BOOLEAN NOT NULL,
    policy_version VARCHAR(20),           -- Version de la politique acceptée
    given_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ip_address   INET,
    user_agent   TEXT,

    -- Pour requête "dernier consentement par finalité"
    CONSTRAINT uq_consent_latest UNIQUE (account_id, purpose, given_at)
);

CREATE INDEX idx_consent_account ON gdpr_consent_log (account_id);
CREATE INDEX idx_consent_purpose ON gdpr_consent_log (account_id, purpose, given_at DESC);


-- ─── 24. data_processing_log (Art. 30 RGPD — registre traitements) ─
-- Trace toute opération sur les données personnelles.
-- Append-only : sert de preuve en cas de contrôle CNIL/APD.
CREATE TABLE data_processing_log (
    log_id        UUID PRIMARY KEY DEFAULT uuidv7(),
    account_id    UUID NOT NULL REFERENCES accounts(account_id) ON DELETE CASCADE,
    action_type   VARCHAR(30) NOT NULL CHECK (action_type IN ('access_request', 'rectification', 'erasure', 'restriction', 'portability_export', 'objection', 'consent_given', 'consent_revoked', 'anonymization', 'data_breach_notification')),
    legal_basis   VARCHAR(30) NOT NULL CHECK (legal_basis IN ('consent', 'contract', 'legal_obligation', 'vital_interest', 'public_interest', 'legitimate_interest')),
    description   TEXT,                  -- Détail de l'opération
    performed_by  UUID REFERENCES accounts(account_id) ON DELETE SET NULL,
    ip_address    INET,
    metadata      JSONB,                 -- Données supplémentaires (e.g. champs modifiés)
    performed_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_dpl_account ON data_processing_log (account_id);
CREATE INDEX idx_dpl_action ON data_processing_log (action_type, performed_at DESC);


-- ─── 25. company_sequences ───────────────────────────────────
-- (déjà créée en table 10, listée ici pour le décompte)


-- ============================================================
-- 4. RÔLES APPLICATIFS (dual-pool RLS)
-- ============================================================

-- Rôle standard : RLS enforced, jamais bypassable
DO $$ BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'app_user') THEN
        CREATE ROLE app_user LOGIN PASSWORD 'CHANGE_ME_app_user_strong_pwd' NOINHERIT;
    END IF;
END $$;

-- Rôle système : bypass RLS pour admin cross-tenant, migrations, jobs
DO $$ BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'app_system') THEN
        CREATE ROLE app_system LOGIN PASSWORD 'CHANGE_ME_app_system_strong_pwd' BYPASSRLS;
    END IF;
END $$;

-- Grants de base (SELECT, INSERT, UPDATE, DELETE) sur toutes les tables
GRANT USAGE ON SCHEMA public TO app_user, app_system;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO app_user, app_system;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO app_user, app_system;

-- Future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO app_user, app_system;
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT USAGE, SELECT ON SEQUENCES TO app_user, app_system;

-- ── Restrictions ciblées pour app_user ──

-- Soft delete enforced : app_user ne peut pas DELETE sur tickets et companies.
REVOKE DELETE ON tickets FROM app_user;
REVOKE DELETE ON companies FROM app_user;

-- audit_logs append-only : app_user ne peut que lire et insérer.
REVOKE UPDATE, DELETE ON audit_logs FROM app_user;

-- gdpr_consent_log append-only : même logique que audit_logs.
REVOKE UPDATE, DELETE ON gdpr_consent_log FROM app_user;

-- data_processing_log append-only.
REVOKE UPDATE, DELETE ON data_processing_log FROM app_user;


-- ============================================================
-- 5. ROW-LEVEL SECURITY (RLS)
-- ============================================================

-- Fonctions de contexte (utilisées par les policies RLS)
CREATE OR REPLACE FUNCTION current_company_id() RETURNS UUID AS $$
BEGIN
    RETURN NULLIF(current_setting('app.current_company_id', TRUE), '')::UUID;
EXCEPTION WHEN OTHERS THEN RETURN NULL;
END;
$$ LANGUAGE plpgsql STABLE;

CREATE OR REPLACE FUNCTION current_user_id() RETURNS UUID AS $$
BEGIN
    RETURN NULLIF(current_setting('app.current_user_id', TRUE), '')::UUID;
EXCEPTION WHEN OTHERS THEN RETURN NULL;
END;
$$ LANGUAGE plpgsql STABLE;

-- ── Activer RLS + FORCE sur toutes les tables tenant-scoped ──
DO $$
DECLARE tbl TEXT;
BEGIN
    FOR tbl IN SELECT unnest(ARRAY[
        'companies', 'company_subscriptions', 'company_members',
        'pending_invitations',
        'categories', 'tickets', 'ticket_comments', 'attachments',
        'ticket_attachments', 'comment_attachments',
        'tags', 'ticket_tags', 'sla_policies', 'ticket_sla_tracking',
        'sla_escalation_rules', 'teams', 'team_members',
        'notifications', 'email_queue', 'company_settings', 'audit_logs',
        'company_sequences'
    ]) LOOP
        EXECUTE format('ALTER TABLE %I ENABLE ROW LEVEL SECURITY', tbl);
        EXECUTE format('ALTER TABLE %I FORCE ROW LEVEL SECURITY', tbl);
    END LOOP;
END $$;

-- accounts : table d'IDENTITÉ GLOBALE (multi-tenant, zéro PII, anonymisable RGPD).
-- PAS de RLS : un compte se crée et se connecte HORS de tout contexte tenant
-- (register/login sans company). Mettre une RLS « par appartenance company » ici
-- bloquerait le signup/login (aucune company au moment de la création).
-- L'anti-énumération d'emails est gérée en couche applicative (cf. US 1.8),
-- et l'unicité/visibilité de l'identité ne relève pas de l'isolation tenant.

-- RLS sur user_profiles (données personnelles — accès restreint)
ALTER TABLE user_profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE user_profiles FORCE ROW LEVEL SECURITY;

-- RLS sur refresh_tokens (defense in depth — table sensible)
ALTER TABLE refresh_tokens ENABLE ROW LEVEL SECURITY;
ALTER TABLE refresh_tokens FORCE ROW LEVEL SECURITY;

-- RLS sur notification_preferences (scopée au compte, pas au tenant)
ALTER TABLE notification_preferences ENABLE ROW LEVEL SECURITY;
ALTER TABLE notification_preferences FORCE ROW LEVEL SECURITY;

-- RLS sur tables RGPD (accès restreint au propriétaire)
ALTER TABLE gdpr_consent_log ENABLE ROW LEVEL SECURITY;
ALTER TABLE gdpr_consent_log FORCE ROW LEVEL SECURITY;
ALTER TABLE data_processing_log ENABLE ROW LEVEL SECURITY;
ALTER TABLE data_processing_log FORCE ROW LEVEL SECURITY;

-- ── Policies CRUD standard — tables avec les 4 opérations ──
-- (exclut tickets, companies, audit_logs, gdpr_consent_log,
--  data_processing_log qui ont des restrictions spécifiques)
DO $$
DECLARE tbl TEXT;
BEGIN
    FOR tbl IN SELECT unnest(ARRAY[
        'categories', 'ticket_comments', 'attachments',
        'ticket_attachments', 'comment_attachments',
        'tags', 'ticket_tags', 'sla_policies', 'ticket_sla_tracking',
        'sla_escalation_rules', 'teams', 'team_members',
        'notifications', 'email_queue',
        'pending_invitations',
        'company_sequences'
    ]) LOOP
        EXECUTE format('CREATE POLICY rls_%I_sel ON %I FOR SELECT USING (company_id = current_company_id())', tbl, tbl);
        EXECUTE format('CREATE POLICY rls_%I_ins ON %I FOR INSERT WITH CHECK (company_id = current_company_id())', tbl, tbl);
        EXECUTE format('CREATE POLICY rls_%I_upd ON %I FOR UPDATE USING (company_id = current_company_id())', tbl, tbl);
        EXECUTE format('CREATE POLICY rls_%I_del ON %I FOR DELETE USING (company_id = current_company_id())', tbl, tbl);
    END LOOP;
END $$;

-- ── Policies spécifiques ──

-- tickets : SELECT + INSERT + UPDATE seulement (soft delete enforced, pas de DELETE policy)
--
-- ARBITRAGE RLS (v4.5) — RLS tenant strict + autorisations applicatives :
--   La policy RLS filtre uniquement par tenant. Les règles de visibilité
--   par rôle (member ne voit que ses tickets, agent voit tout, etc.) sont
--   appliquées en couche applicative via TicketAuthorizationPolicy
--   (cf. dossier de projet section 4.1.3).
--
--   Pourquoi ce découpage :
--     - DB = garantie d'isolation tenant (INVIOLABLE — RGPD critique)
--     - App = règles de permissions métier (ÉVOLUTIVES — visibility_scope,
--       rôles paramétrables, équipes, etc.)
--
--   Performance : comparaison scalaire sur colonne indexée, sans sous-requête
--   EXISTS ni JOIN — la RLS s'exécute en index scan instantané grâce à la
--   dénormalisation de company_id et aux index composites (company_id, X).
CREATE POLICY rls_tickets_sel ON tickets FOR SELECT USING (
    company_id = current_company_id()
);
CREATE POLICY rls_tickets_ins ON tickets FOR INSERT WITH CHECK (company_id = current_company_id());
CREATE POLICY rls_tickets_upd ON tickets FOR UPDATE USING (company_id = current_company_id());

-- companies : SELECT + UPDATE seulement (soft delete enforced, pas de DELETE policy)
CREATE POLICY rls_companies_sel ON companies FOR SELECT USING (company_id = current_company_id());
CREATE POLICY rls_companies_upd ON companies FOR UPDATE USING (company_id = current_company_id());

-- audit_logs : SELECT + INSERT seulement (append-only)
CREATE POLICY rls_audit_logs_sel ON audit_logs FOR SELECT USING (company_id = current_company_id());
CREATE POLICY rls_audit_logs_ins ON audit_logs FOR INSERT WITH CHECK (company_id = current_company_id());

-- company_subscriptions : lecture/modif restreinte
CREATE POLICY rls_sub_sel ON company_subscriptions FOR SELECT USING (company_id = current_company_id());
CREATE POLICY rls_sub_upd ON company_subscriptions FOR UPDATE USING (company_id = current_company_id());

-- company_members : CRUD restreint
CREATE POLICY rls_cm_sel ON company_members FOR SELECT USING (company_id = current_company_id());
CREATE POLICY rls_cm_ins ON company_members FOR INSERT WITH CHECK (company_id = current_company_id());
CREATE POLICY rls_cm_upd ON company_members FOR UPDATE USING (company_id = current_company_id());
CREATE POLICY rls_cm_del ON company_members FOR DELETE USING (company_id = current_company_id());

-- company_settings : CRUD restreint
CREATE POLICY rls_cs_sel ON company_settings FOR SELECT USING (company_id = current_company_id());
CREATE POLICY rls_cs_ins ON company_settings FOR INSERT WITH CHECK (company_id = current_company_id());
CREATE POLICY rls_cs_upd ON company_settings FOR UPDATE USING (company_id = current_company_id());
CREATE POLICY rls_cs_del ON company_settings FOR DELETE USING (company_id = current_company_id());

-- accounts : AUCUNE policy RLS (RLS désactivée sur cette table d'identité globale).
-- Le contrôle « je ne modifie que mon propre compte » est assuré en couche
-- applicative (use cases TKT.Core), pas par la DB, car le flux d'auth opère
-- hors contexte tenant/utilisateur. Voir le commentaire sur ENABLE RLS plus haut.

-- user_profiles : un compte ne voit que les profils des membres de son tenant
CREATE POLICY rls_profiles_sel ON user_profiles FOR SELECT USING (
    EXISTS (
        SELECT 1 FROM company_members
        WHERE company_members.account_id = user_profiles.account_id
          AND company_members.company_id = current_company_id()
          AND company_members.is_active = TRUE
    )
);
-- UPDATE/DELETE sur user_profiles : seulement son propre profil
CREATE POLICY rls_profiles_upd ON user_profiles FOR UPDATE USING (
    account_id = current_user_id()
);
CREATE POLICY rls_profiles_del ON user_profiles FOR DELETE USING (
    account_id = current_user_id()
);
-- INSERT sur user_profiles : seulement pour soi-même
CREATE POLICY rls_profiles_ins ON user_profiles FOR INSERT WITH CHECK (
    account_id = current_user_id()
);

-- refresh_tokens : un compte ne voit/modifie que ses propres tokens
CREATE POLICY rls_rt_sel ON refresh_tokens FOR SELECT USING (account_id = current_user_id());
CREATE POLICY rls_rt_ins ON refresh_tokens FOR INSERT WITH CHECK (account_id = current_user_id());
CREATE POLICY rls_rt_upd ON refresh_tokens FOR UPDATE USING (account_id = current_user_id());
CREATE POLICY rls_rt_del ON refresh_tokens FOR DELETE USING (account_id = current_user_id());

-- notification_preferences : un compte ne gère que ses propres préférences
CREATE POLICY rls_np_sel ON notification_preferences FOR SELECT USING (account_id = current_user_id());
CREATE POLICY rls_np_ins ON notification_preferences FOR INSERT WITH CHECK (account_id = current_user_id());
CREATE POLICY rls_np_upd ON notification_preferences FOR UPDATE USING (account_id = current_user_id());
CREATE POLICY rls_np_del ON notification_preferences FOR DELETE USING (account_id = current_user_id());

-- gdpr_consent_log : append-only, scoped au compte
CREATE POLICY rls_gcl_sel ON gdpr_consent_log FOR SELECT USING (account_id = current_user_id());
CREATE POLICY rls_gcl_ins ON gdpr_consent_log FOR INSERT WITH CHECK (account_id = current_user_id());

-- data_processing_log : append-only, scoped au compte
CREATE POLICY rls_dpl_sel ON data_processing_log FOR SELECT USING (account_id = current_user_id());
CREATE POLICY rls_dpl_ins ON data_processing_log FOR INSERT WITH CHECK (account_id = current_user_id());


-- ============================================================
-- 6. FONCTIONS — ticket_number via company_sequences
-- ============================================================

CREATE OR REPLACE FUNCTION generate_ticket_number(p_company_id UUID)
RETURNS VARCHAR(50) AS $$
DECLARE v_next INT;
BEGIN
    INSERT INTO company_sequences (company_id, sequence_type, last_value)
    VALUES (p_company_id, 'ticket', 1)
    ON CONFLICT (company_id, sequence_type)
    DO UPDATE SET last_value = company_sequences.last_value + 1
    RETURNING last_value INTO v_next;

    RETURN 'TKT-' || lpad(v_next::TEXT, 6, '0');
END;
$$ LANGUAGE plpgsql;


-- ============================================================
-- 7. FONCTION — Anonymisation RGPD (Art. 17 — Droit à l'effacement)
-- ============================================================
-- Exécutée par app_system uniquement (bypass RLS nécessaire).
-- Workflow :
--   1. Supprime user_profiles (PII)
--   2. Anonymise accounts (email → hash, purge phone/IP)
--   3. Révoque tous les refresh_tokens
--   4. Désactive tous les company_members
--   5. Log dans data_processing_log
-- Les FK (tickets.created_by, audit_logs.account_id, etc.) restent
-- intactes — elles pointent vers le tombstone anonymisé.
CREATE OR REPLACE FUNCTION anonymize_account(
    p_account_id UUID,
    p_performed_by UUID DEFAULT NULL,
    p_ip_address INET DEFAULT NULL
)
RETURNS VOID AS $$
DECLARE
    v_anon_email VARCHAR(255);
BEGIN
    -- Vérifier que le compte existe et n'est pas déjà anonymisé
    IF NOT EXISTS (
        SELECT 1 FROM accounts
        WHERE account_id = p_account_id AND anonymized_at IS NULL
    ) THEN
        RAISE EXCEPTION 'Account % does not exist or is already anonymized', p_account_id;
    END IF;

    -- 1. Générer email anonymisé (irréversible)
    v_anon_email := encode(digest(p_account_id::TEXT || NOW()::TEXT, 'sha256'), 'hex');
    v_anon_email := substring(v_anon_email FROM 1 FOR 16) || '@deleted.local';

    -- 2. Supprimer le profil (PII) — CASCADE depuis accounts n'est pas
    --    déclenché ici car on UPDATE accounts, pas DELETE
    DELETE FROM user_profiles WHERE account_id = p_account_id;

    -- 3. Anonymiser le compte (garder comme tombstone)
    UPDATE accounts SET
        email = v_anon_email,
        normalized_email = UPPER(v_anon_email),
        password_hash = '',
        security_stamp = gen_random_uuid()::TEXT,
        concurrency_stamp = gen_random_uuid()::TEXT,
        phone_number = NULL,
        phone_number_confirmed = FALSE,
        two_factor_enabled = FALSE,
        mfa_secret = NULL,
        last_login_ip = NULL,
        is_active = FALSE,
        anonymized_at = NOW()
    WHERE account_id = p_account_id;

    -- 4. Révoquer tous les refresh tokens
    UPDATE refresh_tokens SET
        is_revoked = TRUE,
        revoked_at = NOW(),
        revoked_reason = 'gdpr_erasure',
        created_by_ip = NULL,
        user_agent = NULL
    WHERE account_id = p_account_id AND is_revoked = FALSE;

    -- 5. Désactiver tous les memberships
    UPDATE company_members SET
        is_active = FALSE,
        deactivated_at = NOW()
    WHERE account_id = p_account_id AND is_active = TRUE;

    -- 6. Logger l'opération
    INSERT INTO data_processing_log (
        account_id, action_type, legal_basis, description,
        performed_by, ip_address, metadata
    ) VALUES (
        p_account_id,
        'erasure',
        'consent',
        'Account anonymized per GDPR Art. 17 right to erasure',
        COALESCE(p_performed_by, p_account_id),
        p_ip_address,
        jsonb_build_object(
            'anonymized_email', v_anon_email,
            'profile_deleted', TRUE,
            'tokens_revoked', TRUE,
            'memberships_deactivated', TRUE
        )
    );
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- Seul app_system peut exécuter cette fonction
REVOKE EXECUTE ON FUNCTION anonymize_account FROM PUBLIC;
GRANT EXECUTE ON FUNCTION anonymize_account TO app_system;


-- ============================================================
-- 8. FONCTION — Export portabilité RGPD (Art. 20)
-- ============================================================
-- Retourne toutes les données personnelles d'un compte en JSON.
-- Exécutée par app_system (bypass RLS pour accéder cross-tenant).
CREATE OR REPLACE FUNCTION export_account_data(p_account_id UUID)
RETURNS JSONB AS $$
DECLARE
    v_result JSONB;
BEGIN
    SELECT jsonb_build_object(
        'export_date', NOW(),
        'account', (
            SELECT jsonb_build_object(
                'email', a.email,
                'email_confirmed', a.email_confirmed,
                'phone_number', a.phone_number,
                'two_factor_enabled', a.two_factor_enabled,
                'created_at', a.created_at
            ) FROM accounts a WHERE a.account_id = p_account_id
        ),
        'profile', (
            SELECT jsonb_build_object(
                'first_name', p.first_name,
                'last_name', p.last_name,
                'avatar_url', p.avatar_url,
                'timezone', p.timezone,
                'language', p.language
            ) FROM user_profiles p WHERE p.account_id = p_account_id
        ),
        'memberships', (
            SELECT COALESCE(jsonb_agg(jsonb_build_object(
                'company_name', c.company_name,
                'role', cm.role,
                'department', cm.department,
                'job_title', cm.job_title,
                'joined_at', cm.joined_at
            )), '[]'::jsonb)
            FROM company_members cm
            JOIN companies c ON c.company_id = cm.company_id
            WHERE cm.account_id = p_account_id
        ),
        'consents', (
            SELECT COALESCE(jsonb_agg(jsonb_build_object(
                'purpose', gcl.purpose,
                'granted', gcl.granted,
                'given_at', gcl.given_at
            ) ORDER BY gcl.given_at DESC), '[]'::jsonb)
            FROM gdpr_consent_log gcl WHERE gcl.account_id = p_account_id
        )
    ) INTO v_result;

    -- Log l'export
    INSERT INTO data_processing_log (
        account_id, action_type, legal_basis, description,
        performed_by
    ) VALUES (
        p_account_id,
        'portability_export',
        'consent',
        'Data export per GDPR Art. 20 right to data portability',
        p_account_id
    );

    RETURN v_result;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

REVOKE EXECUTE ON FUNCTION export_account_data FROM PUBLIC;
GRANT EXECUTE ON FUNCTION export_account_data TO app_system;


-- ============================================================
-- 9. TRIGGERS CONSERVÉS
-- ============================================================

-- Trigger 1 : ticket_number auto (validation membership déplacée côté C#)
CREATE OR REPLACE FUNCTION trg_before_ticket_insert()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.ticket_number IS NULL OR NEW.ticket_number = '' THEN
        NEW.ticket_number := generate_ticket_number(NEW.company_id);
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_before_ticket_insert
    BEFORE INSERT ON tickets FOR EACH ROW EXECUTE FUNCTION trg_before_ticket_insert();

-- Trigger 2 : Auto-set resolved_at / closed_at
-- CHOIX DE DESIGN : Ce trigger capture la PREMIÈRE transition vers
-- 'resolved' ou 'closed'. Si un ticket est rouvert puis re-résolu,
-- resolved_at conserve la date de la première résolution.
-- Justification : pour les SLA et les métriques, c'est le temps de
-- première résolution qui est significatif. L'historique complet des
-- transitions est tracé dans audit_logs via les triggers d'audit (cf. section 9b).
CREATE OR REPLACE FUNCTION trg_before_ticket_status_dates()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.status = 'resolved' AND OLD.status != 'resolved' AND NEW.resolved_at IS NULL THEN
        NEW.resolved_at := NOW();
    END IF;
    IF NEW.status = 'closed' AND OLD.status != 'closed' AND NEW.closed_at IS NULL THEN
        NEW.closed_at := NOW();
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_before_ticket_status_dates
    BEFORE UPDATE ON tickets FOR EACH ROW EXECUTE FUNCTION trg_before_ticket_status_dates();


-- ============================================================
-- 9b. TRIGGERS D'AUDIT AUTOMATIQUE (US 8.3)
-- ============================================================
-- Filet de sécurité DB : tous les INSERT/UPDATE/DELETE sur les tables
-- sensibles sont tracés dans audit_logs, même si l'app contourne le
-- wrapper Dapper habituel (requêtes Dapper directes hors repository,
-- scripts SQL manuels, requêtes via psql).
-- Le trigger DB est la source de vérité unique pour l'audit. Un
-- intercepteur côté application (autour de IDbConnection ou dans un
-- repository de base) peut être ajouté en complément si l'on souhaite
-- enrichir les entrées d'audit avec un contexte applicatif (ex. nom
-- d'opération métier), mais ce n'est pas requis : le trigger capture
-- déjà OLD/NEW et tous les champs métadonnées via les variables de
-- session (cf. ci-dessous).
--
-- L'auteur (account_id) et le contexte (ip_address) sont lus depuis les
-- variables de session posées par le middleware TenantContext :
--   SET LOCAL app.current_user_id = '...'
--   SET LOCAL app.current_company_id = '...'
--   SET LOCAL app.current_ip = '...'

CREATE OR REPLACE FUNCTION trg_audit_log()
RETURNS TRIGGER AS $$
DECLARE
    v_pk_column TEXT;
    v_company_id UUID;
    v_entity_id UUID;
    v_changes JSONB;
    v_action VARCHAR(50);
    v_ip TEXT;
BEGIN
    -- Le nom de la PK est passé en argument du trigger : TG_ARGV[0]
    v_pk_column := TG_ARGV[0];
    v_action := lower(TG_OP);  -- 'insert', 'update', 'delete'

    IF TG_OP = 'DELETE' THEN
        v_company_id := OLD.company_id;
        v_entity_id := (to_jsonb(OLD)->>v_pk_column)::UUID;
        v_changes := jsonb_build_object('old', to_jsonb(OLD));
    ELSIF TG_OP = 'INSERT' THEN
        v_company_id := NEW.company_id;
        v_entity_id := (to_jsonb(NEW)->>v_pk_column)::UUID;
        v_changes := jsonb_build_object('new', to_jsonb(NEW));
    ELSE -- UPDATE
        v_company_id := NEW.company_id;
        v_entity_id := (to_jsonb(NEW)->>v_pk_column)::UUID;
        v_changes := jsonb_build_object(
            'old', to_jsonb(OLD),
            'new', to_jsonb(NEW)
        );
    END IF;

    v_ip := NULLIF(current_setting('app.current_ip', TRUE), '');

    INSERT INTO audit_logs (
        company_id, account_id, action, entity_type, entity_id,
        changes, ip_address
    ) VALUES (
        v_company_id,
        current_user_id(),
        v_action,
        TG_TABLE_NAME,
        v_entity_id,
        v_changes,
        CASE WHEN v_ip IS NOT NULL THEN v_ip::INET ELSE NULL END
    );

    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

-- SECURITY DEFINER : permet au trigger d'INSERT dans audit_logs même
-- si app_user n'a pas vu sa policy RLS satisfaite (la policy exige
-- company_id = current_company_id(), ce qui est toujours vrai ici
-- car v_company_id vient de la ligne touchée, mais SECURITY DEFINER
-- évite tout conflit de privilèges).

-- ── Triggers sur les 4 tables sensibles (US 8.3) ──
CREATE TRIGGER trg_audit_tickets
    AFTER INSERT OR UPDATE OR DELETE ON tickets
    FOR EACH ROW EXECUTE FUNCTION trg_audit_log('ticket_id');

CREATE TRIGGER trg_audit_ticket_comments
    AFTER INSERT OR UPDATE OR DELETE ON ticket_comments
    FOR EACH ROW EXECUTE FUNCTION trg_audit_log('comment_id');

CREATE TRIGGER trg_audit_company_members
    AFTER INSERT OR UPDATE OR DELETE ON company_members
    FOR EACH ROW EXECUTE FUNCTION trg_audit_log('membership_id');

CREATE TRIGGER trg_audit_sla_policies
    AFTER INSERT OR UPDATE OR DELETE ON sla_policies
    FOR EACH ROW EXECUTE FUNCTION trg_audit_log('sla_policy_id');


-- ============================================================
-- 10. pgAudit (OPTIONNEL — configuration dans postgresql.conf)
-- ============================================================
-- À ajouter dans postgresql.conf :
--   shared_preload_libraries = 'pgaudit'
--   pgaudit.log = 'write, ddl'
--   pgaudit.log_relation = on
--   pgaudit.log_parameter = on
--
-- Puis exécuter :
--   CREATE EXTENSION IF NOT EXISTS pgaudit;


-- ============================================================
-- 11. VUES UTILITAIRES (confort développeur)
-- ============================================================

-- Vue qui reconstitue l'ancien "users" pour les requêtes qui ont
-- besoin du display_name (ex: UI ticket list, commentaires).
-- Utilisable comme un JOIN léger dans les requêtes Dapper, ou directement
-- mappable vers un POCO `AccountProfile` côté C#.
-- NOTE : Ne pas utiliser pour l'auth ! Utiliser accounts directement.
CREATE OR REPLACE VIEW v_account_profiles AS
SELECT
    a.account_id,
    a.email,
    a.is_active,
    a.last_login_at,
    a.anonymized_at,
    p.first_name,
    p.last_name,
    p.display_name,
    p.avatar_url,
    p.timezone,
    p.language
FROM accounts a
LEFT JOIN user_profiles p ON p.account_id = a.account_id;

-- Vue pour vérifier le consentement actif par finalité.
-- Retourne le dernier événement par (account_id, purpose).
CREATE OR REPLACE VIEW v_active_consents AS
SELECT DISTINCT ON (account_id, purpose)
    account_id,
    purpose,
    granted,
    given_at,
    policy_version
FROM gdpr_consent_log
ORDER BY account_id, purpose, given_at DESC;


-- ============================================================
-- 12. RÉSUMÉ
-- ============================================================

SELECT 'v4.6 — PostgreSQL 18 Ticketing System (Privacy by Design, GDPR-ready, Tenant-isolation strict + App-level authorization, Audit pre-partitioned)' AS status;

SELECT 'Tables' AS type, (SELECT COUNT(*) FROM pg_tables WHERE schemaname = 'public') AS count
UNION ALL SELECT 'Views', (SELECT COUNT(*) FROM pg_views WHERE schemaname = 'public')
UNION ALL SELECT 'Indexes', (SELECT COUNT(*) FROM pg_indexes WHERE schemaname = 'public')
UNION ALL SELECT 'Triggers', (SELECT COUNT(*) FROM information_schema.triggers WHERE trigger_schema = 'public')
UNION ALL SELECT 'Functions', (SELECT COUNT(*) FROM pg_proc p JOIN pg_namespace n ON p.pronamespace = n.oid WHERE n.nspname = 'public')
UNION ALL SELECT 'RLS Policies', (SELECT COUNT(*) FROM pg_policies WHERE schemaname = 'public')
UNION ALL SELECT 'Enum Types', (SELECT COUNT(*) FROM pg_type t JOIN pg_namespace n ON t.typnamespace = n.oid WHERE n.nspname = 'public' AND t.typtype = 'e');
