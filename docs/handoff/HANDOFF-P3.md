# Handoff: Increment 2 (Visits and dashboard) and final integration

This is the team's **final integration note**: it records the state of the system for the report and the demo.
Sections 4 and 6 still need finishing once Increment 1 (Person 2) is merged and the demo is recorded.

**From:** Person 3 (`eh1nm3n`): `VisitService` and its DI registration ([PR #2](https://github.com/ceetek1/visma/pull/2)).
The remaining Increment 2 work (tests, screens, dashboard, docs, QA) was completed by Person 1 (`ceetek1`) on
`feature/p3-visits-completion`, following [P3-BRIEF.md](P3-BRIEF.md).
**Branch / PR:** `feature/p3-visits-dashboard` → [PR #2](https://github.com/ceetek1/visma/pull/2) (merged 27 Nov 2026);
`feature/p3-visits-completion` → _PR to be opened_
**Date merged:** service on 27 Nov 2026; the rest _not merged yet_
**Status of `feature/p3-visits-completion`:** `dotnet build` 0 warnings, 0 errors · `dotnet test` 40 passed, 0 failed
(25 unit, 15 integration)

## 1. What was delivered

| Requirement | Delivered? | Where (controller action / service method) |
|---|---|---|
| FR-11 Check in (host, purpose, time) | Yes | `VisitsController.CheckIn` (GET step 1 and 2, POST), `VisitService.CheckInAsync` |
| FR-12 Active hosts only | Yes | Step 2 lists `IHostRepository.GetActiveAsync()` only; `CheckInAsync` also refuses inactive hosts |
| FR-13 No double check-in | Yes | `CheckInAsync` rule 5; step 1 shows "On site" instead of Select; step 2 offers Check out instead of the form |
| FR-14 Check out (no double check-out) | Yes | `VisitsController.CheckOut` (POST + antiforgery, local `returnUrl` only), `VisitService.CheckOutAsync` |
| FR-15 Who is on site | Yes | `HomeController.Index`: visitor passes (or a table above 4 people), each with Check out |
| FR-16 Dashboard figures | Yes | `VisitService.GetDashboardStatsAsync` ("today" = local day, converted to UTC) |
| FR-17 Visit history with filters | Yes | `VisitsController.History`: local date range (default last 7 days) + host, including inactive hosts |

## 2. Files added or changed

| File | Purpose |
|---|---|
| `src/Visma.Core/Services/VisitService.cs` | Check-in/out rules and dashboard figures (Person 3, PR #2) |
| `src/Visma.Web/Program.cs` | `IVisitService` → `VisitService` registered (Person 3, PR #2) |
| `src/Visma.Web/Controllers/VisitsController.cs` | Check-in wizard, pass, check-out, visit log |
| `src/Visma.Web/Controllers/HomeController.cs` | Dashboard now reads `IVisitService` |
| `src/Visma.Web/Models/VisitViewModels.cs` | `CheckInFindViewModel`, `CheckInFormModel`, `CheckInDetailsViewModel`, `VisitHistoryViewModel`, `VisitorPickRow` |
| `src/Visma.Web/Models/ViewModels.cs` | `HomeIndexViewModel` gains `Stats`; `ActiveHosts` / `RegisteredVisitors` removed |
| `src/Visma.Web/Infrastructure/LocalDay.cs` | Local calendar day → UTC start instant (uses the injected `TimeProvider`) |
| `src/Visma.Web/Views/Visits/*.cshtml` | `CheckInFind`, `CheckInDetails`, `Pass`, `History`, partials `_CheckInSteps`, `_VisitorPickTable` |
| `src/Visma.Web/Views/Home/Index.cshtml` | Live dashboard |
| `src/Visma.Web/wwwroot/css/visma.css` | "Person 3" section: wizard, pass page, dashboard, visit log, stacked tables on phones, secondary buttons |
| `tests/Visma.Tests/Services/VisitServiceTests.cs` | TC-U40–U51 |
| `tests/Visma.Tests/Services/VisitFlowIntegrationTests.cs` | TC-I40–I41 |
| `docs/03-design.md` | §4 class diagram updated; §6 sequence and §7 activity diagrams final |
| `docs/04-test-plan.md` | §2.4–2.6 results; defects D-04, D-05 |
| `docs/demo-script.md`, `docs/screenshots/p3-*.png` | Demo running order; 9 screenshots |

## 3. Differences from the brief

- **Branch:** the service landed on `feature/p3-visits-dashboard` (brief: same); the rest is on `feature/p3-visits-completion`.
- **Extra tests:** TC-U49 (unknown visit), TC-U50 (undefined purpose), TC-U51 ("today" in UTC+1) and TC-I41 (history range edges).
- **Unit tests use in-memory SQLite** for the repositories instead of hand-written fakes, so the open-visit query behind the
  double check-in rule is the real one. They are still tagged `Level=Unit` because the service is the unit under test.
- **Visit log date conversion** uses the time zone's rules for each date (`TimeZoneInfo.ConvertTimeToUtc`) rather than
  today's UTC offset, so a range that crosses a daylight-saving change would still be right.
- **Step 2 already-on-site view:** besides the message, it shows the visitor's current pass and a Check out button that
  returns to step 2, so the officer can check them out and straight back in.
- **The increments panel stays** on the dashboard until Increment 1 is merged (brief: remove once all three are merged).

## 4. Integration check (on merged `main`)

To run after Increment 1 is merged:

| Check | Result |
|---|---|
| All sidebar links open real pages (no "soon" tags) | _Pending Increment 1_ (Check in and Visit log already lit) |
| Visitor details "Check in" opens check-in with the visitor pre-selected | _Pending Increment 1_ (target URL `/Visits/CheckIn?visitorId={id}` works) |
| Check-in "Register them" opens registration with the phone pre-filled | _Pending Increment 1_ (link appears once `Visitors/Create` exists) |
| Visit log visitor names link to visitor details | _Pending Increment 1_ (link appears once `Visitors/Details` exists) |
| Fresh clone → `dotnet run` → demo data appears | ✅ Fresh database: Grace on site, 3 visitors, 6 active hosts |

Then remove the increments panel: `HomeIndexViewModel.Increments`, the `Increments = [...]` block in
`HomeController.Index`, and the `<section class="panel increments">` in `Views/Home/Index.cshtml`.

## 5. Test summary (whole system)

| Stage | IDs | Passed / total |
|---|---|---|
| Unit | TC-U01–U08, TC-U40–U51 | 20 / 20 IDs (25 test methods) |
| Integration | TC-I01–I12, TC-I40–I41 | 14 / 14 IDs (15 test methods) |
| System | TC-S01–S11 | 8 pass, 1 partly (S05), 2 blocked on Increment 1 (S03, S04) |
| Acceptance | TC-A01–A05 | 3 pass (A02, A03, A05), 1 partly (A04), 1 blocked (A01); team walkthrough still to do |

Person 2's TC-U20–U28 are not written yet. Open defects: None (D-04 and D-05 found and fixed in this increment).

## 6. Demo

- Script: [`docs/demo-script.md`](../demo-script.md)
- Video link: _[unlisted YouTube / Drive link with view access]_
- Recorded on: _[date]_

## 7. Known issues / future work

- **No login** (risk R6): anyone who can open the site can check visitors in and out.
- **No badge printing** and **no host notifications** when a visitor arrives.
- **"0m" for very short visits:** durations under a minute show as "0m".
- **Dashboard figures don't auto-refresh:** the page shows the state when it was loaded; reload to update.
- **Visit log has no paging:** fine at front-desk scale; add paging if months of data are kept.

## 8. Documentation updated

- [x] 03-design.md: check-in sequence and activity diagram finalised, class diagram updated
- [x] 04-test-plan.md: §2.4–2.6 and defect log updated (S03/S04/A01 waiting on Increment 1)
- [x] `docs/demo-script.md` written
- [x] Screenshots collected in `docs/screenshots/` (`p3-*`; Person 2 adds `p2-*`)
- [ ] 05-report-checklist.md: every item ticked (needs Increment 1, the video and the team's names)
