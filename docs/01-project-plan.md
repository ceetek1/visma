# 01: Project Plan and Feasibility Study

**Project:** VISMA: Visitor Management System
**Course:** SEN 201, C# Software Engineering Group Project
**Project number:** _[fill in from the project list]_

## 1. Problem statement

Many offices and schools record visitors in a paper logbook at the gate or front desk. The logbook is slow to search, often unreadable, cannot show who is *still* on the premises, and makes it hard to answer questions such as "who came to see the Bursary last Tuesday?". VISMA replaces the logbook with a simple web application.

## 2. Scope

**In scope (MVP):**

| # | Capability |
|---|---|
| 1 | Register visitors and recognise returning visitors |
| 2 | Record the purpose of each visit |
| 3 | Identify the person being visited from a managed host directory |
| 4 | Track check-in and check-out times; show who is on site |
| 5 | Search visit history |

**Out of scope (possible future increments):** staff login and roles, printing physical badges, photo capture, SMS/e-mail notifications to hosts, pre-booked appointments, multi-site support.

**Assumption:** the system runs on a trusted front-desk computer on the organisation's internal network, so the MVP has no login (see risk R6).

## 3. Process model: Incremental

We chose the **Incremental model**: the system is delivered as a series of working increments, each adding features on top of a stable foundation.

| Increment | Owner | Delivers |
|---|---|---|
| 0: Foundation | Person 1 | Architecture, domain model, database, repositories, UI shell, test infrastructure, documentation skeleton |
| 1: Registration and hosts | Person 2 | Visitor registration, returning-visitor lookup, visitor search/edit, host directory |
| 2: Visits and dashboard | Person 3 | Check-in, check-out, live dashboard, visit history |

**Why Incremental (and not the alternatives)?**

- **vs Waterfall:** we can demo a working system after every increment instead of only at the end, which lowers the risk of missing the deadline with nothing running.
- **vs Agile/Scrum:** a three-person student team has no product owner and can't run daily stand-ups and sprint ceremonies. Incremental gives structure without that overhead.
- **Fits the team:** Increments 1 and 2 both depend only on Increment 0, so Persons 2 and 3 can work **in parallel** once the foundation is merged.

Each increment still passes through requirements → design → implementation → testing before it is merged.

## 4. Feasibility study

### 4.1 Technical feasibility: **Feasible**

| Need | Solution | Notes |
|---|---|---|
| Language/framework | C# on .NET 11, ASP.NET Core MVC | Required by the brief; free, cross-platform (Windows/macOS/Linux) |
| Data storage | SQLite via Entity Framework Core | A single file; no database server to install, so it works on every team member's laptop |
| UI | Razor views + Bootstrap 5 | Bootstrap ships with the MVC template; responsive out of the box |
| Testing | xUnit + in-memory SQLite | Fast, automated, and enforces the same constraints as the real database |
| Version control | Git + GitHub | Free; shows individual contributions |

All tools are free and the team has introductory C# experience. The main skill gap (EF Core and MVC) is covered by the foundation code and handoff notes.

### 4.2 Operational feasibility: **Feasible**

- Front-desk staff already understand the logbook workflow; VISMA mirrors it (arrive, then sign in; leave, then sign out).
- The interface uses large, high-legibility type, works on a phone or a desk PC, and needs no training beyond a short demo.
- Returning visitors are found by phone number, so repeat visitors check in in seconds.

### 4.3 Schedule feasibility: **Feasible with risk**

The work fits within the project window because increments run in parallel. The schedule risk is integration at the end, so Increment 0 defines all interfaces up front.

### 4.4 Economic feasibility: **Feasible**

No licence or hosting cost: all software is open-source or free, and the app runs locally.

## 5. Schedule

_Adjust dates to your submission deadline._

| Week | Activities | Owner(s) | Milestone |
|---|---|---|---|
| 1 | Planning, feasibility, requirements (SRS), high-level design | All | Plan + SRS agreed |
| 2 | Increment 0: foundation code, diagrams, test infrastructure | Person 1 | Foundation merged to `main` |
| 3 | Increment 1 (registration and hosts) ‖ Increment 2 (visits and dashboard) | Persons 2 and 3 | Feature branches in PR |
| 4 | Integration, system testing, acceptance testing, bug fixes | All (QA lead: Person 3) | All tests pass |
| 5 | Screenshots, demo video, final report PDF | All | Submission |

## 6. Team and roles

| Name | Matric. No. | Lifecycle role(s) | Increment |
|---|---|---|---|
| _[Person 1 name]_ | _[matric]_ | Project Manager, System Architect, C# Developer | 0: Foundation |
| _[Person 2 name]_ | _[matric]_ | Requirements Analyst, C# Developer | 1: Registration and hosts |
| _[Person 3 name]_ | _[matric]_ | QA / Tester, C# Developer | 2: Visits and dashboard |

Everyone writes the unit tests and diagrams for their own increment.

## 7. Risk register

| ID | Risk | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| R1 | A teammate's increment is late, blocking the demo | Medium | High | Increments are independent; nav shows "coming soon" instead of breaking; weekly check-ins |
| R2 | Merge conflicts in shared files | Medium | Medium | Each person owns separate folders; shared files have marked sections; branch-per-person workflow |
| R3 | Team members on different .NET versions / OSes | Medium | Medium | Single target (.NET 11), SQLite (no server), `dotnet tool restore` pins EF tooling; README setup steps |
| R4 | Data loss or corrupt history | Low | High | Visits and hosts are never hard-deleted (Restrict deletes, host deactivation); DB-level unique and foreign-key constraints |
| R5 | Wrong times recorded (time zones) | Medium | Medium | All times stored in UTC and shown in local time; tests use a controllable clock |
| R6 | Unauthorised use (no login in MVP) | Medium | Medium | Documented assumption: trusted front-desk PC on internal network; login is first item for a future increment |
| R7 | Integration bugs found late | Medium | High | Interfaces fixed in Increment 0; integration tests on repositories; system test checklist in week 4 |
