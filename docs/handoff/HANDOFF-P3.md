# Handoff: Increment 2 (Visits and dashboard) and final integration

> **Template.** Person 3 fills this in when Increment 2 is merged and system testing is complete. Replace every _[…]_ and delete this note.
> This is the team's **final integration note**: it records the state of the finished system for the report and the demo.

**From:** _[Person 3 name]_
**Branch / PR:** `feature/p3-visits-dashboard` → _[PR link]_
**Date merged:** _[date]_
**Final `main` status:** _[e.g. builds with 0 warnings, 48/48 tests pass]_

## 1. What was delivered

| Requirement | Delivered? | Where (controller action / service method) |
|---|---|---|
| FR-11 Check in (host, purpose, time) | _[Yes/Partly/No]_ | _[VisitsController.CheckIn, VisitService.CheckInAsync]_ |
| FR-12 Active hosts only | | |
| FR-13 No double check-in | | |
| FR-14 Check out (no double check-out) | | |
| FR-15 Who is on site | | |
| FR-16 Dashboard figures | | |
| FR-17 Visit history with filters | | |

## 2. Files added or changed

| File | Purpose |
|---|---|
| _[src/Visma.Core/Services/VisitService.cs]_ | _[check-in/out rules]_ |
| | |

## 3. Differences from the brief

_[Anything you did differently from [P3-BRIEF.md](P3-BRIEF.md), and why. Write "None" if nothing.]_

## 4. Integration check (on merged `main`)

| Check | Result |
|---|---|
| All sidebar links open real pages (no "soon" tags) | |
| Visitor details "Check in" opens check-in with the visitor pre-selected | |
| Check-in "Register them" opens registration with the phone pre-filled | |
| Visit log visitor names link to visitor details | |
| Fresh clone → `dotnet run` → demo data appears | |

## 5. Test summary (whole system)

| Stage | IDs | Passed / total |
|---|---|---|
| Unit | TC-U01–U__ | |
| Integration | TC-I01–I__ | |
| System | TC-S01–S11 | |
| Acceptance | TC-A01–A05 | |

Open defects: _[list D-xx still open, or "None"]_

## 6. Demo

- Script: `docs/demo-script.md`
- Video link: _[unlisted YouTube / Drive link with view access]_
- Recorded on: _[date]_

## 7. Known issues / future work

_[e.g. no login (risk R6), no badge printing, no host notifications.]_

## 8. Documentation updated

- [ ] 03-design.md: check-in sequence and activity diagram finalised, class diagram updated
- [ ] 04-test-plan.md: §2.4–2.6 and defect log complete
- [ ] `docs/demo-script.md` written
- [ ] Screenshots collected in `docs/screenshots/`
- [ ] 05-report-checklist.md: every item ticked
