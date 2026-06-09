#!/usr/bin/env python3
"""
Seed GitHub issues + Project (v2) depuis MOSCOW_EPICS_US.md.

Découpe :
  - Chaque EPIC  -> 1 issue "epic" par repo concerné (API et/ou WEB).
  - Chaque US    -> 1 issue dans le repo API (sous-tâches [DB]/[Back])
                    et/ou 1 issue dans le repo WEB (sous-tâches [Front]).
  - Les sous-tâches techniques -> checklist `- [ ]` dans le corps de l'US.
  - Les US sont rattachées à leur Epic via la relation native "sub-issues".
  - Tout est ajouté au Project (v2) avec les champs Priorité MoSCoW / Story Points / Type.

Le script NE crée PAS les repos : tu les crées toi-même, puis tu passes leurs
noms en paramètres. Il est idempotent : relancé, il saute ce qui existe déjà
(match par titre exact d'issue).

Prérequis :
  gh auth refresh -s project --hostname github.com   # ajoute le scope 'project'

Exemples :
  # Aperçu sans rien écrire :
  python3 scripts/seed_github_project.py --dry-run

  # Création réelle :
  python3 scripts/seed_github_project.py \
      --owner ZekJulien --project 6 \
      --api-repo ZekJulien/IETC-TKT-API \
      --web-repo ZekJulien/IETC-TKT-WEB
"""
from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from dataclasses import dataclass, field
from pathlib import Path

# --------------------------------------------------------------------------- #
# Modèle de données
# --------------------------------------------------------------------------- #

PRIORITY_LABELS = {
    "MUST HAVE": ("must-have", "MoSCoW : Must have", "B60205"),
    "SHOULD HAVE": ("should-have", "MoSCoW : Should have", "D93F0B"),
    "COULD HAVE": ("could-have", "MoSCoW : Could have", "FBCA04"),
    "WON'T HAVE": ("wont-have", "MoSCoW : Won't have (hors scope)", "C5DEF5"),
}
# Valeurs du champ single-select "Priorité MoSCoW" du projet
PRIORITY_FIELD_OPTIONS = ["Must have", "Should have", "Could have", "Won't have"]
PRIORITY_FIELD_MAP = {
    "MUST HAVE": "Must have",
    "SHOULD HAVE": "Should have",
    "COULD HAVE": "Could have",
    "WON'T HAVE": "Won't have",
}


@dataclass
class UserStory:
    number: str          # "1.1"
    title: str           # "Inscription utilisateur"
    sp: int | None       # story points
    priority: str        # "MUST HAVE"
    description: str      # paragraphe "En tant que..."
    acceptance: list[str] = field(default_factory=list)
    api_tasks: list[str] = field(default_factory=list)   # [DB] / [Back]
    web_tasks: list[str] = field(default_factory=list)   # [Front]
    note: str = ""       # éventuelle note ("Hors scope...")


@dataclass
class Epic:
    number: int          # 1
    title: str
    description: str
    stories: list[UserStory] = field(default_factory=list)


# --------------------------------------------------------------------------- #
# Parsing du markdown
# --------------------------------------------------------------------------- #

EPIC_RE = re.compile(r"^#\s+EPIC\s+(\d+)\s+[—-]\s+(.+?)\s*$")
US_RE = re.compile(r"^##\s+(\d+\.\d+)\s+(.+?)\s+[—-]\s+(\d+)\s*SP\s+[—-]\s+(.+?)\s*$")
TASK_RE = re.compile(r"^-\s+\[(DB|Back|Front)\]\s+(.*)$")


def parse_markdown(path: Path) -> list[Epic]:
    lines = path.read_text(encoding="utf-8").splitlines()
    epics: list[Epic] = []
    cur_epic: Epic | None = None
    cur_us: UserStory | None = None
    section: str | None = None  # "accept" | "tasks" | "desc"
    epic_desc_pending = False

    def flush_us():
        nonlocal cur_us
        if cur_us and cur_epic:
            cur_us.description = cur_us.description.strip()
            cur_epic.stories.append(cur_us)
        cur_us = None

    for raw in lines:
        line = raw.rstrip()

        m = EPIC_RE.match(line)
        if m:
            flush_us()
            cur_epic = Epic(int(m.group(1)), m.group(2).strip(), "")
            epics.append(cur_epic)
            section = None
            epic_desc_pending = True
            continue

        m = US_RE.match(line)
        if m:
            flush_us()
            cur_us = UserStory(
                number=m.group(1),
                title=m.group(2).strip(),
                sp=int(m.group(3)),
                priority=m.group(4).strip().upper(),
            )
            section = "desc"
            continue

        # Description de l'epic : 1ère ligne non vide après le titre d'epic
        if epic_desc_pending and cur_epic and not cur_us:
            if line.strip():
                cur_epic.description = line.strip()
                epic_desc_pending = False
            continue

        if cur_us is None:
            continue

        stripped = line.strip()

        if stripped.startswith("**Critères d'acceptation**"):
            section = "accept"
            continue
        if stripped.startswith("**Sous-tâches techniques**"):
            section = "tasks"
            continue
        # Autre marqueur ** ... ** dans la description (ex: "En tant que")
        if section == "desc":
            if stripped.startswith(">"):
                continue
            if stripped.startswith("*") and stripped.endswith("*") and "Hors scope" in stripped:
                cur_us.note = stripped.strip("* ")
            elif stripped:
                cur_us.description += (" " if cur_us.description else "") + stripped
            continue

        if section == "accept":
            if stripped.startswith("- "):
                cur_us.acceptance.append(stripped[2:].strip())
            continue

        if section == "tasks":
            mt = TASK_RE.match(stripped)
            if mt:
                tag, text = mt.group(1), mt.group(2).strip()
                entry = f"[{tag}] {text}"
                if tag == "Front":
                    cur_us.web_tasks.append(entry)
                else:  # DB ou Back -> repo API
                    cur_us.api_tasks.append(entry)
            continue

    flush_us()
    return epics


# --------------------------------------------------------------------------- #
# Construction des corps d'issue
# --------------------------------------------------------------------------- #

def us_body(us: UserStory, tasks: list[str], epic: Epic, scope: str) -> str:
    parts = [f"> **Epic {epic.number} — {epic.title}**", ""]
    if us.description:
        parts += [us.description, ""]
    parts += [f"**Story points :** {us.sp}  |  **Priorité MoSCoW :** {us.priority}", ""]
    if us.acceptance:
        parts.append("## Critères d'acceptation")
        parts += [f"- {a}" for a in us.acceptance]
        parts.append("")
    if tasks:
        label = "API (backend / base de données)" if scope == "api" else "WEB (frontend Angular)"
        parts.append(f"## Sous-tâches techniques — {label}")
        parts += [f"- [ ] {t}" for t in tasks]
        parts.append("")
    if us.note:
        parts += [f"> ℹ️ {us.note}", ""]
    parts.append(f"<!-- us:{us.number} scope:{scope} -->")
    return "\n".join(parts).strip() + "\n"


def epic_body(epic: Epic, scope: str) -> str:
    label = "API" if scope == "api" else "WEB"
    parts = [epic.description, "", f"_Suivi {label} — les User Stories sont rattachées comme sub-issues._",
             "", f"<!-- epic:{epic.number} scope:{scope} -->"]
    return "\n".join(parts).strip() + "\n"


# --------------------------------------------------------------------------- #
# Helpers gh
# --------------------------------------------------------------------------- #

class Gh:
    def __init__(self, dry_run: bool):
        self.dry_run = dry_run

    def _run(self, args: list[str], write: bool, capture=True) -> str:
        if write and self.dry_run:
            print(f"   [dry-run] gh {' '.join(args)}")
            return ""
        res = subprocess.run(["gh", *args], capture_output=capture, text=True)
        if res.returncode != 0:
            raise RuntimeError(f"gh {' '.join(args)}\n{res.stderr.strip()}")
        return (res.stdout or "").strip()

    def read(self, args: list[str]) -> str:
        return self._run(args, write=False)

    def write(self, args: list[str]) -> str:
        return self._run(args, write=True)


def ensure_repo(gh: Gh, repo: str) -> bool:
    try:
        gh.read(["repo", "view", repo, "--json", "name"])
        return True
    except RuntimeError:
        print(f"⚠️  Repo introuvable : {repo} — crée-le sur GitHub puis relance.")
        return False


def ensure_label(gh: Gh, repo: str, name: str, desc: str, color: str, seen: set):
    key = (repo, name)
    if key in seen:
        return
    seen.add(key)
    try:
        gh.write(["label", "create", name, "--repo", repo,
                  "--description", desc, "--color", color, "--force"])
    except RuntimeError as e:
        print(f"   (label '{name}' : {e})")


def existing_issues(gh: Gh, repo: str) -> dict[str, dict]:
    """titre -> {number, url, id(databaseId)}."""
    if gh.dry_run:
        return {}
    out = gh.read(["issue", "list", "--repo", repo, "--state", "all",
                   "--limit", "500", "--json", "number,title,url,id"])
    res = {}
    for it in json.loads(out or "[]"):
        res[it["title"]] = it
    return res


def create_issue(gh: Gh, repo: str, title: str, body: str, labels: list[str]) -> dict:
    body_arg = ["--body", body]
    label_args = []
    for l in labels:
        label_args += ["--label", l]
    if gh.dry_run:
        print(f"   [dry-run] create issue [{repo}] {title}  labels={labels}")
        return {"number": 0, "url": "(dry-run)", "id": ""}
    url = gh.write(["issue", "create", "--repo", repo, "--title", title,
                    *body_arg, *label_args])
    info = json.loads(gh.read(["issue", "view", url, "--json", "number,url,id"]))
    return info


def db_id(gh: Gh, repo: str, number: int) -> int | None:
    if gh.dry_run:
        return None
    out = gh.read(["api", f"repos/{repo}/issues/{number}", "--jq", ".id"])
    return int(out) if out else None


def link_sub_issue(gh: Gh, repo: str, parent_number: int, child_db_id: int):
    """Relation native parent/enfant via l'API sub-issues."""
    if gh.dry_run or not parent_number or not child_db_id:
        return
    try:
        gh.read(["api", "-X", "POST",
                 f"repos/{repo}/issues/{parent_number}/sub_issues",
                 "-F", f"sub_issue_id={child_db_id}"])
    except RuntimeError as e:
        if "already" not in str(e).lower():
            print(f"   (sub-issue link {parent_number}<-{child_db_id} : {e})")


# --------------------------------------------------------------------------- #
# Project v2
# --------------------------------------------------------------------------- #

class Project:
    def __init__(self, gh: Gh, owner: str, number: int):
        self.gh = gh
        self.owner = owner
        self.number = str(number)
        self.id = ""
        self.fields: dict[str, dict] = {}  # name -> field json

    def load(self):
        if self.gh.dry_run:
            return
        info = json.loads(self.gh.read(
            ["project", "view", self.number, "--owner", self.owner, "--format", "json"]))
        self.id = info["id"]
        fl = json.loads(self.gh.read(
            ["project", "field-list", self.number, "--owner", self.owner,
             "--format", "json"]))
        for f in fl.get("fields", []):
            self.fields[f["name"]] = f

    def ensure_single_select(self, name: str, options: list[str]):
        if self.gh.dry_run:
            return
        if name in self.fields:
            return
        self.gh.write(["project", "field-create", self.number, "--owner", self.owner,
                       "--name", name, "--data-type", "SINGLE_SELECT",
                       "--single-select-options", ",".join(options)])
        # recharge pour récupérer les option-ids
        fl = json.loads(self.gh.read(
            ["project", "field-list", self.number, "--owner", self.owner, "--format", "json"]))
        for f in fl.get("fields", []):
            self.fields[f["name"]] = f

    def ensure_number(self, name: str):
        if self.gh.dry_run or name in self.fields:
            return
        self.gh.write(["project", "field-create", self.number, "--owner", self.owner,
                       "--name", name, "--data-type", "NUMBER"])
        fl = json.loads(self.gh.read(
            ["project", "field-list", self.number, "--owner", self.owner, "--format", "json"]))
        for f in fl.get("fields", []):
            self.fields[f["name"]] = f

    def add_item(self, url: str) -> str:
        if self.gh.dry_run:
            print(f"   [dry-run] project add {url}")
            return ""
        out = self.gh.read(["project", "item-add", self.number, "--owner", self.owner,
                            "--url", url, "--format", "json"])
        return json.loads(out)["id"]

    def _option_id(self, field_name: str, option_name: str) -> str | None:
        f = self.fields.get(field_name, {})
        for o in f.get("options", []):
            if o["name"] == option_name:
                return o["id"]
        return None

    def set_single_select(self, item_id: str, field_name: str, option_name: str):
        if self.gh.dry_run or not item_id:
            return
        fid = self.fields[field_name]["id"]
        oid = self._option_id(field_name, option_name)
        if not oid:
            return
        self.gh.write(["project", "item-edit", "--id", item_id, "--project-id", self.id,
                       "--field-id", fid, "--single-select-option-id", oid])

    def set_number(self, item_id: str, field_name: str, value: float):
        if self.gh.dry_run or not item_id:
            return
        fid = self.fields[field_name]["id"]
        self.gh.write(["project", "item-edit", "--id", item_id, "--project-id", self.id,
                       "--field-id", fid, "--number", str(value)])


# --------------------------------------------------------------------------- #
# Orchestration
# --------------------------------------------------------------------------- #

def main():
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--file", default=str(Path.home() / "Téléchargements" / "MOSCOW_EPICS_US.md"),
                    help="Chemin du markdown source")
    ap.add_argument("--owner", default="ZekJulien")
    ap.add_argument("--project", type=int, default=6)
    ap.add_argument("--api-repo", default="ZekJulien/IETC-TKT-API")
    ap.add_argument("--web-repo", default="ZekJulien/IETC-TKT-WEB")
    ap.add_argument("--dry-run", action="store_true", help="N'écrit rien, affiche le plan")
    ap.add_argument("--no-project", action="store_true",
                    help="Crée seulement les issues, sans toucher au Project v2")
    args = ap.parse_args()

    src = Path(args.file)
    if not src.exists():
        sys.exit(f"Fichier introuvable : {src}")

    epics = parse_markdown(src)
    n_us = sum(len(e.stories) for e in epics)
    print(f"📄 Parsé : {len(epics)} epics, {n_us} user stories\n")

    gh = Gh(args.dry_run)

    # Vérif repos
    repos = {"api": args.api_repo, "web": args.web_repo}
    if not args.dry_run:
        ok = True
        for r in repos.values():
            ok &= ensure_repo(gh, r)
        if not ok:
            sys.exit(1)

    # Labels
    label_seen: set = set()
    for scope, repo in repos.items():
        for code, desc, color in PRIORITY_LABELS.values():
            ensure_label(gh, repo, code, desc, color, label_seen)
        ensure_label(gh, repo, "epic", "Epic", "5319E7", label_seen)
        ensure_label(gh, repo, "user-story", "User Story", "0E8A16", label_seen)

    # Project
    proj = None
    if not args.no_project:
        proj = Project(gh, args.owner, args.project)
        proj.load()
        proj.ensure_single_select("Priorité MoSCoW", PRIORITY_FIELD_OPTIONS)
        proj.ensure_single_select("Type", ["Epic", "User Story"])
        proj.ensure_number("Story Points")

    existing = {scope: existing_issues(gh, repo) for scope, repo in repos.items()}

    def add_to_project(url: str, *, type_name: str, priority: str | None, sp: int | None):
        if not proj:
            return
        item = proj.add_item(url)
        proj.set_single_select(item, "Type", type_name)
        if priority and priority in PRIORITY_FIELD_MAP:
            proj.set_single_select(item, "Priorité MoSCoW", PRIORITY_FIELD_MAP[priority])
        if sp is not None:
            proj.set_number(item, "Story Points", sp)

    def get_or_create(scope: str, repo: str, title: str, body: str, labels: list[str]) -> dict:
        if title in existing[scope]:
            print(f"   = existe : [{scope}] {title}")
            return existing[scope][title]
        print(f"   + crée   : [{scope}] {title}")
        info = create_issue(gh, repo, title, body, labels)
        existing[scope][title] = info
        return info

    for epic in epics:
        # Un epic concerne le repo API si au moins une US a des tâches API, idem WEB.
        scopes_needed = set()
        for us in epic.stories:
            if us.api_tasks or (not us.api_tasks and not us.web_tasks):
                scopes_needed.add("api")  # les US sans tâches (WON'T HAVE) -> côté API
            if us.web_tasks:
                scopes_needed.add("web")

        print(f"\n=== EPIC {epic.number} — {epic.title} ===")
        epic_issue = {}
        for scope in ("api", "web"):
            if scope not in scopes_needed:
                continue
            repo = repos[scope]
            title = f"[Epic {epic.number}] {epic.title}"
            info = get_or_create(scope, repo, title, epic_body(epic, scope), ["epic"])
            epic_issue[scope] = info
            add_to_project(info["url"], type_name="Epic", priority=None, sp=None)

        for us in epic.stories:
            prio_label = PRIORITY_LABELS[us.priority][0]
            targets = []
            if us.api_tasks or (not us.api_tasks and not us.web_tasks):
                targets.append(("api", us.api_tasks))
            if us.web_tasks:
                targets.append(("web", us.web_tasks))

            for scope, tasks in targets:
                repo = repos[scope]
                title = f"[{us.number}] {us.title}"
                body = us_body(us, tasks, epic, scope)
                info = get_or_create(scope, repo, title, body, ["user-story", prio_label])
                add_to_project(info["url"], type_name="User Story",
                               priority=us.priority, sp=us.sp)
                # rattachement sub-issue à l'epic du même repo
                if scope in epic_issue and info.get("number"):
                    parent_no = epic_issue[scope]["number"]
                    cid = info.get("id")  # databaseId déjà fourni par 'gh issue view --json id'
                    # 'id' renvoyé par gh est le node-id GraphQL -> on récupère le databaseId via API
                    cdb = db_id(gh, repo, info["number"])
                    link_sub_issue(gh, repo, parent_no, cdb)

    print("\n✅ Terminé." + ("  (dry-run : rien écrit)" if args.dry_run else ""))


if __name__ == "__main__":
    main()
