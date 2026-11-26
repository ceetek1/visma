# VISMA: Visitor Management System

VISMA replaces the paper visitor logbook at a front desk. It:

1. **Registers visitors** (and recognises returning visitors by phone or e-mail)
2. **Records the purpose** of every visit
3. **Identifies the person being visited** from a managed staff (host) directory
4. **Tracks check-in and check-out** times, so the desk always knows who is on site

Built in C# with **ASP.NET Core MVC**, **Entity Framework Core** and **SQLite** for the SEN 201 group project.

---

## Run it

**Prerequisite:** [.NET 11 SDK](https://dotnet.microsoft.com/download). Check with `dotnet --version`, which should print `11.x`.

```bash
git clone https://github.com/ceetek1/visma.git
cd visma
dotnet tool restore                      # installs the pinned dotnet-ef tool
dotnet run --project src/Visma.Web       # then open the URL it prints (http://localhost:5130)
```

On first run the app creates `src/Visma.Web/visma.db` and fills it with demo hosts, visitors and visits.
To start from a clean database, stop the app, delete `visma.db`, and run it again.

```bash
dotnet test                              # run every automated test
dotnet test --filter Level=Unit          # only unit tests
dotnet test --filter Level=Integration   # only integration tests
```

## How the code is organised

The solution has one project per layer, so the user interface, business logic and data storage stay separate:

```
Visma.slnx
├── src/
│   ├── Visma.Core/     Business layer: entities, rules, interfaces (no database or web code)
│   ├── Visma.Data/     Data layer: EF Core DbContext, repositories, migrations, seed data
│   └── Visma.Web/      Presentation layer: MVC controllers, Razor views, CSS
├── tests/
│   └── Visma.Tests/    xUnit unit + integration tests
└── docs/               Project plan, SRS, design diagrams, test plan, handoff notes
```

Dependencies point inward: `Web → Core ← Data`. Controllers only ever talk to **interfaces** from `Visma.Core`; `Program.cs` decides which class implements each one.

## Team and increments

VISMA follows the **Incremental** process model. Each increment is a working, demonstrable slice:

| Increment | Owner | Scope | Status |
|---|---|---|---|
| 0: Foundation | Person 1 | Solution structure, domain model, database, repositories, layout and theme, test infrastructure, documentation | Done |
| 1: Registration and hosts | Person 2 | Register/search/edit visitors, returning-visitor lookup, host directory | To do |
| 2: Visits and dashboard | Person 3 | Check-in, check-out, live dashboard, visit history | To do |

Fill in names and matriculation numbers in [docs/01-project-plan.md](docs/01-project-plan.md#6-team-and-roles).

**New to the codebase? Start with [docs/handoff/HANDOFF-P1.md](docs/handoff/HANDOFF-P1.md), then your own brief ([Person 2](docs/handoff/P2-BRIEF.md) · [Person 3](docs/handoff/P3-BRIEF.md)).**

## Documentation

| Document | What's in it |
|---|---|
| [01 Project plan](docs/01-project-plan.md) | Process model, scope, feasibility study, schedule, roles, risks |
| [02 Requirements (SRS)](docs/02-srs.md) | Functional and non-functional requirements, scenarios |
| [03 Design](docs/03-design.md) | Use case, class, ER, architecture, sequence and activity diagrams |
| [04 Test plan](docs/04-test-plan.md) | Test strategy and test-case log |
| [05 Report checklist](docs/05-report-checklist.md) | Every report component, where its content lives, who owns it |
| [Handoff: Person 1](docs/handoff/HANDOFF-P1.md) | What the foundation provides; shared rules, patterns and UI kit |
| [Brief: Person 2](docs/handoff/P2-BRIEF.md) | Registration and hosts: rules, screens, tests, docs to deliver |
| [Brief: Person 3](docs/handoff/P3-BRIEF.md) | Visits and dashboard + QA lead duties |
| [Handoff templates](docs/handoff/) | `HANDOFF-P2.md`, `HANDOFF-P3.md`, filled in when each increment is done |
| [Contributing](CONTRIBUTING.md) | Branches, commits, pull requests |
