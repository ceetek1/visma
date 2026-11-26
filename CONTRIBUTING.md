# Contributing to VISMA

The marking scheme looks at **commit history showing individual contributions**. Following these rules keeps everyone's work visible and stops us overwriting each other.

## Golden rules

1. **Never commit directly to `main`.** Work on your own branch and open a pull request (PR).
2. **Small, focused commits.** One logical change per commit (e.g. "add VisitorService.RegisterAsync"), not one giant commit at the end.
3. **`dotnet build` and `dotnet test` must pass** before you push.
4. **Only touch your own files** unless you've agreed otherwise. Shared files (`Program.cs`, `visma.css`, `docs/*`) have marked sections; edit only yours.

## Branches

| Person | Branch |
|---|---|
| Person 1 | `feature/p1-foundation` |
| Person 2 | `feature/p2-registration-hosts` |
| Person 3 | `feature/p3-visits-dashboard` |

```bash
git checkout main
git pull
git checkout -b feature/p2-registration-hosts
```

Keep your branch up to date with `main` while you work:

```bash
git fetch origin
git merge origin/main
```

## Commit messages

Use the form `type(area): what changed`, written in the present tense:

| Type | Use for |
|---|---|
| `feat` | New behaviour (`feat(visitors): add returning-visitor lookup`) |
| `fix` | Bug fix (`fix(visits): block check-out of closed visit`) |
| `test` | Tests only (`test(visits): cover double check-in rule`) |
| `docs` | Documentation (`docs(srs): add FR-12 scenario`) |
| `style` | CSS/markup only (`style(hosts): align table columns`) |
| `refactor` | Code change with no behaviour change |
| `chore` | Tooling, config, packages |

Add a body (blank line, then bullet points) when the change needs explaining.

## Pull requests

1. Push your branch: `git push -u origin feature/p2-registration-hosts`
2. Open a PR into `main` on GitHub. Describe what you built and how you tested it, and add screenshots for UI changes.
3. Ask one teammate to review. Fix comments with new commits; don't force-push over review history.
4. Merge once approved and checks pass.

## Database changes

If you change an entity or `VismaDbContext`, add a migration (don't edit existing ones):

```bash
dotnet tool restore
dotnet ef migrations add DescribeYourChange --project src/Visma.Data --startup-project src/Visma.Web
```

Tell the team in the PR, because everyone needs to delete their local `visma.db`, or let the app apply the migration on start.

## Code style

- Public classes and methods get `/// <summary>` XML comments (the brief requires well-commented code).
- Business rules live in **services** (`Visma.Core/Services`), not controllers or views.
- Never use `DateTime.Now`/`UtcNow` in services; inject `TimeProvider`.
- Controllers return expected failures as messages (`OperationResult`), not exceptions.
