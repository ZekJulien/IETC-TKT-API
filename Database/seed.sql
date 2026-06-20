-- ============================================================
-- TKT — Données de démonstration
-- ------------------------------------------------------------
-- À exécuter APRÈS tkt.sql, en tant que superuser (postgres) :
--   docker exec -i tkt_pg psql -U postgres -d ticketing_system -f /chemin/seed.sql
-- Le superuser bypasse la RLS, ce qui permet de peupler toutes
-- les tables (y compris user_profiles, tickets, etc.) directement.
--
-- Tous les comptes de démo partagent le mot de passe : Demo1234
-- (hash Argon2id réel généré par l'API, vérifiable au login).
--
-- Couvre tout le périmètre développé : multi-tenant, rôles,
-- profils (nom/prénom), catégories, tags, tickets (tous statuts
-- et priorités, assignés/non assignés), commentaires public/interne
-- avec threading. Les ticket_number sont générés par le trigger.
-- ============================================================

\c ticketing_system

BEGIN;

-- ─── Comptes (mot de passe : Demo1234) ───────────────────────
INSERT INTO accounts (account_id, email, normalized_email, email_confirmed, password_hash) VALUES
('a0000000-0000-4000-8000-000000000001', 'owner@acme.test',   'OWNER@ACME.TEST',   TRUE, '$argon2id$v=19$m=65536,t=3,p=1$qGQbj5qqJWYqSWhOuzifcQ$PyWU7ZIDWsw1nNkxS3QbHUnhsoUNWvVUF0xomEhKrbw'),
('a0000000-0000-4000-8000-000000000002', 'admin@acme.test',   'ADMIN@ACME.TEST',   TRUE, '$argon2id$v=19$m=65536,t=3,p=1$qGQbj5qqJWYqSWhOuzifcQ$PyWU7ZIDWsw1nNkxS3QbHUnhsoUNWvVUF0xomEhKrbw'),
('a0000000-0000-4000-8000-000000000003', 'agent1@acme.test',  'AGENT1@ACME.TEST',  TRUE, '$argon2id$v=19$m=65536,t=3,p=1$qGQbj5qqJWYqSWhOuzifcQ$PyWU7ZIDWsw1nNkxS3QbHUnhsoUNWvVUF0xomEhKrbw'),
('a0000000-0000-4000-8000-000000000004', 'agent2@acme.test',  'AGENT2@ACME.TEST',  TRUE, '$argon2id$v=19$m=65536,t=3,p=1$qGQbj5qqJWYqSWhOuzifcQ$PyWU7ZIDWsw1nNkxS3QbHUnhsoUNWvVUF0xomEhKrbw'),
('a0000000-0000-4000-8000-000000000005', 'member1@acme.test', 'MEMBER1@ACME.TEST', TRUE, '$argon2id$v=19$m=65536,t=3,p=1$qGQbj5qqJWYqSWhOuzifcQ$PyWU7ZIDWsw1nNkxS3QbHUnhsoUNWvVUF0xomEhKrbw'),
('a0000000-0000-4000-8000-000000000006', 'member2@acme.test', 'MEMBER2@ACME.TEST', TRUE, '$argon2id$v=19$m=65536,t=3,p=1$qGQbj5qqJWYqSWhOuzifcQ$PyWU7ZIDWsw1nNkxS3QbHUnhsoUNWvVUF0xomEhKrbw'),
('a0000000-0000-4000-8000-000000000007', 'owner@globex.test', 'OWNER@GLOBEX.TEST', TRUE, '$argon2id$v=19$m=65536,t=3,p=1$qGQbj5qqJWYqSWhOuzifcQ$PyWU7ZIDWsw1nNkxS3QbHUnhsoUNWvVUF0xomEhKrbw'),
-- Compte MULTI-TENANT : membre des deux entreprises, avec un rôle différent dans chacune.
('a0000000-0000-4000-8000-000000000008', 'nadia@acme.test',   'NADIA@ACME.TEST',   TRUE, '$argon2id$v=19$m=65536,t=3,p=1$qGQbj5qqJWYqSWhOuzifcQ$PyWU7ZIDWsw1nNkxS3QbHUnhsoUNWvVUF0xomEhKrbw');

-- ─── Profils (PII : nom / prénom) ────────────────────────────
INSERT INTO user_profiles (account_id, first_name, last_name) VALUES
('a0000000-0000-4000-8000-000000000001', 'Alice',  'Martin'),
('a0000000-0000-4000-8000-000000000002', 'Bruno',  'Lefebvre'),
('a0000000-0000-4000-8000-000000000003', 'Chloé',  'Dubois'),
('a0000000-0000-4000-8000-000000000004', 'David',  'Bernard'),
('a0000000-0000-4000-8000-000000000005', 'Emma',   'Rousseau'),
('a0000000-0000-4000-8000-000000000006', 'Lucas',  'Moreau'),
('a0000000-0000-4000-8000-000000000007', 'Sophie', 'Laurent'),
('a0000000-0000-4000-8000-000000000008', 'Nadia',  'Haddad');

-- ─── Entreprises (multi-tenant) ──────────────────────────────
INSERT INTO companies (company_id, company_name, company_slug, description) VALUES
('c0000000-0000-4000-8000-000000000001', 'Acme Helpdesk',  'acme',   'Support IT interne de la société Acme.'),
('c0000000-0000-4000-8000-000000000002', 'Globex Support', 'globex', 'Helpdesk interne Globex.');

-- ─── Abonnements (Acme = Pro, Globex = Free) ─────────────────
INSERT INTO company_subscriptions (company_id, plan_type, max_users, max_tickets_per_month, max_storage_gb) VALUES
('c0000000-0000-4000-8000-000000000001', 'pro',  50, 5000, 50.00);
INSERT INTO company_subscriptions (company_id) VALUES
('c0000000-0000-4000-8000-000000000002');

-- ─── Membres (rôles owner / admin / agent / member) ──────────
-- Alice est owner d'Acme ET admin de Globex (démo du switch de tenant).
INSERT INTO company_members (company_id, account_id, role, department, job_title, joined_at) VALUES
('c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000001', 'owner',  'Direction',     'Gérante',                  NOW() - INTERVAL '90 days'),
('c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000002', 'admin',  'IT',            'Responsable IT',           NOW() - INTERVAL '88 days'),
('c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000003', 'agent',  'IT',            'Technicienne support',     NOW() - INTERVAL '80 days'),
('c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000004', 'agent',  'IT',            'Technicien support',       NOW() - INTERVAL '80 days'),
('c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000005', 'member', 'Marketing',     'Chargée de communication', NOW() - INTERVAL '60 days'),
('c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000006', 'member', 'Comptabilité',  'Comptable',                NOW() - INTERVAL '45 days'),
('c0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000007', 'owner',  'Direction',     'Gérant',                   NOW() - INTERVAL '30 days'),
('c0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000001', 'admin',  'Direction',     'Consultante',              NOW() - INTERVAL '20 days'),
-- Nadia : MEMBER chez Acme, AGENT chez Globex (même compte, rôle différent par tenant)
('c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000008', 'member', 'Externe',       'Consultante',              NOW() - INTERVAL '15 days'),
('c0000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000008', 'agent',  'Support',       'Consultante support',      NOW() - INTERVAL '15 days');

-- ─── Tags — Acme ─────────────────────────────────────────────
INSERT INTO tags (tag_id, company_id, tag_name, color) VALUES
('e0000000-0000-4000-8000-000000000001', 'c0000000-0000-4000-8000-000000000001', 'urgent',   '#dc2626'),
('e0000000-0000-4000-8000-000000000002', 'c0000000-0000-4000-8000-000000000001', 'bug',      '#b91c1c'),
('e0000000-0000-4000-8000-000000000003', 'c0000000-0000-4000-8000-000000000001', 'question', '#2563eb'),
('e0000000-0000-4000-8000-000000000004', 'c0000000-0000-4000-8000-000000000001', 'matériel', '#6b7280'),
('e0000000-0000-4000-8000-000000000005', 'c0000000-0000-4000-8000-000000000001', 'rgpd',     '#7c3aed');

-- ─── Tickets — Acme (ticket_number généré par trigger) ───────
-- Tous statuts : open / in_progress / pending / resolved / closed,
-- toutes priorités, certains assignés, certains non assignés.
INSERT INTO tickets (ticket_id, company_id, title, description, status, priority, created_by, assigned_to, created_at, resolved_at, closed_at) VALUES
('70000000-0000-4000-8000-000000000001', 'c0000000-0000-4000-8000-000000000001', 'Imprimante du 2e étage hors service',                'Plus aucune impression possible depuis ce matin, voyant rouge clignotant.', 'open',        'high',   'a0000000-0000-4000-8000-000000000005', NULL,                                   NOW() - INTERVAL '2 days',  NULL,                      NULL),
('70000000-0000-4000-8000-000000000002', 'c0000000-0000-4000-8000-000000000001', 'Accès VPN impossible en télétravail',                'Connexion refusée depuis hier soir, erreur affichée à l''ouverture du client.', 'in_progress', 'medium', 'a0000000-0000-4000-8000-000000000005', 'a0000000-0000-4000-8000-000000000003', NOW() - INTERVAL '3 days',  NULL,                      NULL),
('70000000-0000-4000-8000-000000000003', 'c0000000-0000-4000-8000-000000000001', 'Demande d''installation d''Adobe Creative Cloud',    'Besoin de Photoshop et Illustrator pour la nouvelle campagne.',              'pending',     'medium', 'a0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000004', NOW() - INTERVAL '4 days',  NULL,                      NULL),
('70000000-0000-4000-8000-000000000004', 'c0000000-0000-4000-8000-000000000001', 'Réinitialisation du mot de passe Windows',           'Compte verrouillé après plusieurs tentatives.',                              'resolved',    'low',    'a0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000003', NOW() - INTERVAL '6 days',  NOW() - INTERVAL '5 days', NULL),
('70000000-0000-4000-8000-000000000005', 'c0000000-0000-4000-8000-000000000001', 'Nouvel écran pour le poste de travail',              'Écran actuel défaillant (scintillements), demande de remplacement.',         'closed',      'low',    'a0000000-0000-4000-8000-000000000005', 'a0000000-0000-4000-8000-000000000004', NOW() - INTERVAL '14 days', NOW() - INTERVAL '10 days', NOW() - INTERVAL '8 days'),
('70000000-0000-4000-8000-000000000006', 'c0000000-0000-4000-8000-000000000001', 'Serveur de fichiers inaccessible',                   'Le lecteur réseau partagé ne monte plus pour toute l''équipe compta.',        'open',        'urgent', 'a0000000-0000-4000-8000-000000000006', NULL,                                   NOW() - INTERVAL '5 hours', NULL,                      NULL),
('70000000-0000-4000-8000-000000000007', 'c0000000-0000-4000-8000-000000000001', 'Onboarding nouveau collaborateur — création comptes','Arrivée lundi : créer comptes AD, email et accès aux outils métier.',         'in_progress', 'high',   'a0000000-0000-4000-8000-000000000005', 'a0000000-0000-4000-8000-000000000004', NOW() - INTERVAL '1 days',  NULL,                      NULL),
('70000000-0000-4000-8000-000000000008', 'c0000000-0000-4000-8000-000000000001', 'Badge d''accès défectueux',                          'Le badge ne fonctionne plus à l''entrée principale depuis ce matin.',         'pending',     'medium', 'a0000000-0000-4000-8000-000000000006', 'a0000000-0000-4000-8000-000000000003', NOW() - INTERVAL '2 days',  NULL,                      NULL),
-- Ticket créé par Nadia en tant que MEMBER d'Acme (ne voit que ses propres tickets sur ce tenant)
('70000000-0000-4000-8000-00000000000b', 'c0000000-0000-4000-8000-000000000001', 'Demande d''accès au dossier partagé Marketing',     'Besoin d''un accès en lecture/écriture au dossier réseau Marketing.',          'open',        'low',    'a0000000-0000-4000-8000-000000000008', NULL,                                   NOW() - INTERVAL '6 hours', NULL,                      NULL);

-- ─── Ticket — Globex (second tenant, isolation) ──────────────
INSERT INTO tickets (ticket_id, company_id, title, description, status, priority, created_by, created_at) VALUES
('70000000-0000-4000-8000-000000000009', 'c0000000-0000-4000-8000-000000000002', 'Configuration boîte mail partagée', 'Créer une boîte support@globex partagée par l''équipe.', 'open', 'medium', 'a0000000-0000-4000-8000-000000000007', NOW() - INTERVAL '1 days');

-- Ticket Globex assigné à Nadia (AGENT sur ce tenant) — visible quand elle bascule sur Globex
INSERT INTO tickets (ticket_id, company_id, title, description, status, priority, created_by, assigned_to, created_at) VALUES
('70000000-0000-4000-8000-00000000000a', 'c0000000-0000-4000-8000-000000000002', 'Migration de la messagerie vers Microsoft 365', 'Planifier la bascule des boîtes mail vers M365 pour toute l''équipe.', 'in_progress', 'high', 'a0000000-0000-4000-8000-000000000007', 'a0000000-0000-4000-8000-000000000008', NOW() - INTERVAL '2 days');

-- ─── Liaisons tickets ↔ tags — Acme ──────────────────────────
INSERT INTO ticket_tags (ticket_id, tag_id, company_id, added_by) VALUES
('70000000-0000-4000-8000-000000000001', 'e0000000-0000-4000-8000-000000000001', 'c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000003'),
('70000000-0000-4000-8000-000000000001', 'e0000000-0000-4000-8000-000000000004', 'c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000003'),
('70000000-0000-4000-8000-000000000006', 'e0000000-0000-4000-8000-000000000001', 'c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000004'),
('70000000-0000-4000-8000-000000000006', 'e0000000-0000-4000-8000-000000000002', 'c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000004'),
('70000000-0000-4000-8000-000000000003', 'e0000000-0000-4000-8000-000000000003', 'c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000004'),
('70000000-0000-4000-8000-000000000007', 'e0000000-0000-4000-8000-000000000003', 'c0000000-0000-4000-8000-000000000001', 'a0000000-0000-4000-8000-000000000004');

-- ─── Commentaires (public / interne, avec threading) ─────────
INSERT INTO ticket_comments (comment_id, company_id, ticket_id, account_id, reply_to_id, comment_text, is_internal, created_at) VALUES
('c1000000-0000-4000-8000-000000000001', 'c0000000-0000-4000-8000-000000000001', '70000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000003', NULL,                                   'Bonjour, pouvez-vous préciser le message d''erreur affiché à la connexion ?', FALSE, NOW() - INTERVAL '3 days' + INTERVAL '2 hours'),
('c1000000-0000-4000-8000-000000000002', 'c0000000-0000-4000-8000-000000000001', '70000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000005', 'c1000000-0000-4000-8000-000000000001', 'Erreur 800 : impossible d''établir la connexion au serveur distant.',         FALSE, NOW() - INTERVAL '3 days' + INTERVAL '4 hours'),
('c1000000-0000-4000-8000-000000000003', 'c0000000-0000-4000-8000-000000000001', '70000000-0000-4000-8000-000000000002', 'a0000000-0000-4000-8000-000000000003', NULL,                                   'Note interne : vérifier le certificat VPN côté serveur, expiration probable.', TRUE,  NOW() - INTERVAL '3 days' + INTERVAL '5 hours'),
('c1000000-0000-4000-8000-000000000004', 'c0000000-0000-4000-8000-000000000001', '70000000-0000-4000-8000-000000000003', 'a0000000-0000-4000-8000-000000000004', NULL,                                   'Pouvez-vous confirmer la version de votre système pour la licence ?',          FALSE, NOW() - INTERVAL '4 days' + INTERVAL '3 hours'),
('c1000000-0000-4000-8000-000000000005', 'c0000000-0000-4000-8000-000000000001', '70000000-0000-4000-8000-000000000004', 'a0000000-0000-4000-8000-000000000003', NULL,                                   'Mot de passe réinitialisé et compte déverrouillé, merci de tester.',           FALSE, NOW() - INTERVAL '5 days' - INTERVAL '2 hours'),
('c1000000-0000-4000-8000-000000000006', 'c0000000-0000-4000-8000-000000000001', '70000000-0000-4000-8000-000000000004', 'a0000000-0000-4000-8000-000000000006', 'c1000000-0000-4000-8000-000000000005', 'Ça fonctionne, merci beaucoup !',                                              FALSE, NOW() - INTERVAL '5 days' - INTERVAL '1 hours');

COMMIT;
