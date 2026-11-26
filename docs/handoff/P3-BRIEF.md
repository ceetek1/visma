# Person 3 brief: Increment 2: Visits and dashboard

**Your branch:** `feature/p3-visits-dashboard`
**Your report role:** QA / Tester (QA lead) + C# Developer
**Requirements you deliver:** FR-11 to FR-17 ([SRS](../02-srs.md))
**Read first:** [HANDOFF-P1.md](HANDOFF-P1.md) sections 1–4 (how the layers fit, code patterns, UI kit)

You build the heart of VISMA: **checking visitors in to a host with a purpose**, **checking them out**, the **live dashboard** of who is on site, and the **visit log**. As QA lead you also run the system and acceptance tests and script the demo video.

You do **not** need Person 2's code to start. Visitors and hosts are reachable through repositories that already exist.

---

## 1. What you can rely on (already built and tested)

| You need | Already exists | Where |
|---|---|---|
| Find visitors | `IVisitorRepository.SearchAsync(term)`, `FindByPhoneOrEmailAsync(term)`, `GetByIdAsync(id)` | `src/Visma.Core/Interfaces/` |
| Hosts for the dropdown | `IHostRepository.GetActiveAsync()`, `GetByIdAsync(id)` | same |
| Visits | `IVisitRepository`: `AddAsync`, `UpdateAsync`, `GetByIdAsync`, `GetActiveVisitForVisitorAsync`, `GetActiveVisitsAsync`, `GetHistoryAsync`, `CountCheckInsAsync` (all reads include Visitor and Host) | same |
| Visit status/duration | `visit.Status`, `visit.Duration(nowUtc)` | `Visit.cs` |
| Purpose list | `VisitPurpose` enum with display names | `Enums/` |
| Pass display | `<partial name="_VisitorPass" model="visit" />` | `Views/Shared/` |
| Status pill | `<partial name="_StatusBadge" model="visit.Status" />` | same |
| Controllable clock | `FakeClock` (local time = UTC in tests) | `tests/Visma.Tests/Infrastructure/` |
| Demo data | Grace Olaniyan is **already on site**; Ada and Ibrahim are not; Mr. Segun Afolabi is an **inactive** host | `DbSeeder` |

---

## 2. Business rules (your service)

`VisitService : IVisitService` in `src/Visma.Core/Services/VisitService.cs`
Constructor: `(IVisitRepository visits, IVisitorRepository visitors, IHostRepository hosts, TimeProvider clock)`

Use these messages word for word. They're what the receptionist sees, and your tests assert on them.

### `CheckInAsync(visitorId, hostId, purpose, notes)`

Check in this order; the first failure wins:

| # | Rule | Message |
|---|---|---|
| 1 | Visitor must exist | `Visitor not found.` |
| 2 | Host must exist | `Host not found.` |
| 3 | Host must be active (FR-12) | `{host.FullName} is no longer receiving visitors. Choose another host.` |
| 4 | `purpose` must be a defined `VisitPurpose` (`Enum.IsDefined`) | `Choose a purpose for the visit.` |
| 5 | Visitor must have **no open visit** (FR-13) | `{visitor.FullName} is already checked in. Check them out before checking in again.` |
| ✓ | Create the visit with `CheckInTime = clock UTC now`. Trim `notes`; blank becomes `null`. Set `visit.Visitor` and `visit.Host` before returning, so the pass page can show names. | — |

### `CheckOutAsync(visitId)`

| # | Rule | Message |
|---|---|---|
| 1 | Visit must exist | `Visit not found.` |
| 2 | Visit must still be open (FR-14) | `This visit was already checked out.` |
| ✓ | `CheckOutTime = clock UTC now`; save; return the visit | — |

### `GetDashboardStatsAsync()`

| Figure | How |
|---|---|
| `OnSiteNow` | `GetActiveVisitsAsync().Count` |
| `CheckInsToday` | `CountCheckInsAsync(todayStartUtc, todayEndUtc)` |
| `CheckedOutToday` | Today's visits (`GetHistoryAsync(todayStartUtc, todayEndUtc)`) where `CheckOutTime != null` |
| `TotalVisitors` | `visitors.SearchAsync(null).Count` (fine at MVP scale) |

**"Today" is local, but the database is UTC.** Compute the range like this:

```csharp
var localNow = clock.GetLocalNow();                                    // e.g. 26 Nov 14:05 +01:00
var startLocal = new DateTimeOffset(localNow.Date, localNow.Offset);   // 26 Nov 00:00 +01:00
var todayStartUtc = startLocal.UtcDateTime;                            // 25 Nov 23:00 UTC
var todayEndUtc = startLocal.AddDays(1).UtcDateTime;
```

### `GetActiveVisitsAsync()` / `GetHistoryAsync(...)`

Delegate to the repository.

Register the service in `Program.cs` by uncommenting the **Person 3** line.

---

## 3. Screens

### 3.1 Check in, step 1: find the visitor: `GET /Visits/CheckIn?q=…`

- **One search field:** "Phone, e-mail or name" (`name="q"`), with the button **Find visitor**.
- **If `FindByPhoneOrEmailAsync(q)` matches exactly:** go straight to step 2 (`?visitorId=`).
- **Otherwise:** list `SearchAsync(q)` results (name, phone, company). Each row has a **Select** button, or an "On site" `_StatusBadge` and **no** Select button if that visitor already has an open visit.
- **No match:** `_EmptyState` "No visitor found", plus the cross-link **Register them** (section 4).
- **Before any search:** show a short hint and the 5 most recently registered visitors, if you like.

### 3.2 Check in, step 2: host and purpose: `GET /Visits/CheckIn?visitorId={id}`

- **Visitor summary at top:** name, phone, company, and a "Change visitor" link back to step 1.
- **Form fields:**
  - Person being visited\*: a `<select>` of **active** hosts grouped by department, using `<optgroup>`
  - Purpose of visit\*: `Html.GetEnumSelectList<VisitPurpose>()`
  - Notes: a textarea, max 250 characters
- **Button:** **Check in**.
- **If the visitor is already on site:** show the error message and a **Check out** button for that visit instead of the form.

### 3.3 Check in: `POST /Visits/CheckIn`

- **Failure:** re-show step 2 with `ModelState.AddModelError("", result.Error)`.
- **Success:** redirect to `/Visits/Pass/{id}` with `TempData["Success"] = "Checked in {visitor} to see {host}."`

### 3.4 Visitor pass: `GET /Visits/Pass/{id}`

- `_VisitorPass` for the visit, plus buttons **Check in another visitor** and **Back to dashboard**.
- This is the screen to show in the demo video when a check-in succeeds.

### 3.5 Check out: `POST /Visits/CheckOut/{id}` (form field `returnUrl`)

- **Success:** `TempData["Success"] = "Checked out {visitor} after {duration}."` (`ToFriendlyDuration()`)
- **Failure:** `TempData["Error"] = result.Error`
- **Redirect:** to `returnUrl` only if `Url.IsLocalUrl(returnUrl)`; otherwise go to the dashboard. This blocks open-redirect attacks.
- It must be a POST form with an antiforgery token, never a GET link.

### 3.6 Visit log: `GET /Visits/History?from=2026-11-20&to=2026-11-26&hostId=3`

- **Filters:** From and To (`<input type="date">`, local dates; default is the last 7 days) and Host (all hosts, including inactive ones, because history must still find them). Button: **Show visits**.
- **Converting dates:** treat the local `from` date's midnight as the start and `to + 1 day` midnight as the end, both converted to UTC as in §2.
- **Table:** Visitor (a link to `/Visitors/Details/{id}` if `Nav.Exists("Visitors", "Details")`), Host + department, Purpose, Checked in, Checked out, Duration, Status.
- **Summary line:** "{n} visits between {from} and {to}".
- **Empty state:** "No visits in this period".

### 3.7 Dashboard: `GET /` (you take over `HomeController.Index` and `Views/Home/Index.cshtml`)

- Keep the greeting and date if you like them.
- **Figures:** the four `DashboardStats` numbers, in the same quiet style as the current `.facts` list (big number, small label). Don't use a row of identical cards.
- **On site now:** each open visit as `_VisitorPass` (or a compact table if there are more than about 4), each with a **Check out** form button (`returnUrl` = `/`).
- **Nobody on site:** the current `_EmptyState` "The building is empty".
- Remove the "Project increments" panel and `HomeIndexViewModel.Increments` once all three increments are merged.

---

## 4. Links between your pages and Person 2's (the cross-team contract)

| From | To | URL | Guard |
|---|---|---|---|
| Your step 1 "No visitor found: **Register them**" | Person 2's register page | `/Visitors/Create?phone={q}` | Show only if `Nav.Exists("Visitors", "Create")` |
| Person 2's **Check in** buttons | Your step 2 | `/Visits/CheckIn?visitorId={id}` | Person 2 guards with `Nav.Exists("Visits", "CheckIn")` |
| Your visit log visitor name | Person 2's details page | `/Visitors/Details/{id}` | Show as a link only if `Nav.Exists("Visitors", "Details")` |

`Nav` is `@inject NavigationHelper Nav` at the top of the view.

---

## 5. Tests you write

Put them in `tests/Visma.Tests/Services/VisitServiceTests.cs`. IDs TC-U40–U59 and TC-I40–I59. Record each in [04-test-plan.md](../04-test-plan.md) §2.4.

| ID | Test | Expect |
|---|---|---|
| TC-U40 | Check in valid visitor/host, `FakeClock` at 09:00 | Success; `CheckInTime` = 09:00; Visitor and Host set |
| TC-U41 | Check in the same visitor twice | Second fails with the double check-in message |
| TC-U42 | Check in to an inactive host | Failure with the host message |
| TC-U43 | Check in with unknown visitor / unknown host | `Visitor not found.` / `Host not found.` |
| TC-U44 | Advance clock 75 min, check out | `CheckOutTime` = 10:15; `Duration` = 1h 15m |
| TC-U45 | Check out twice | Second fails `This visit was already checked out.` |
| TC-U46 | Check out, then check in again | Allowed: a new visit |
| TC-U47 | Stats: 2 open + 1 closed today, 1 visit yesterday | OnSiteNow 2, CheckInsToday 3, CheckedOutToday 1 |
| TC-U48 | Notes "   " | Stored as `null` |
| TC-I40 | Full flow through service + SQLite: check in → active list → check out → history | Visit has both times; not on site; in history |

---

## 6. QA lead duties (whole system)

You coordinate testing for the team. Don't do all of it yourself; assign rows.

1. **System tests:** run TC-S01–S11 in [04-test-plan.md §2.5](../04-test-plan.md) on the **merged** `main` (after both increments land). Fill in Actual and Result. Rows S03–S05 are Person 2's to execute; collect them.
2. **Acceptance tests:** walk through scenarios S1–S5 ([SRS §4](../02-srs.md)) with the whole team watching (TC-A01–A05).
3. **Defect log:** record anything that fails in §3 of the test plan (D-04, D-05, …) with who fixed it.
4. **Demo video script:** write `docs/demo-script.md`, a 3–5 minute running order that follows S1 → S5, plus edge cases: blank required field, duplicate phone, double check-in refused, double check-out refused, inactive host missing from the list, phone layout and dark mode.
5. **Screenshots:** collect everyone's into `docs/screenshots/` and check that every core feature has one.

---

## 7. Documentation you own

| Deliverable | Where |
|---|---|
| Check-in sequence diagram: update the draft to match your code and remove "DRAFT" | [03-design.md §6](../03-design.md) |
| Visit lifecycle activity diagram: same | [03-design.md §7](../03-design.md) |
| Class diagram: add `VisitService`, `VisitsController` and the dashboard view model | [03-design.md §4](../03-design.md) |
| Test log §2.4–2.6 and defect log §3 | [04-test-plan.md](../04-test-plan.md) |
| Demo script | `docs/demo-script.md` |
| Screenshots | `docs/screenshots/p3-*.png`: check-in step 1 and step 2, pass, double check-in error, dashboard, check-out message, visit log with filters |
| Your handoff | [HANDOFF-P3.md](HANDOFF-P3.md): fill in the template; it becomes the team's final integration note |

---

## 8. Suggested commit sequence

1. `feat(visits): implement CheckInAsync with host and double-check-in rules`
2. `test(visits): cover check-in rules with FakeClock (TC-U40–U43)`
3. `feat(visits): implement CheckOutAsync` + `test(visits): TC-U44–U46`
4. `feat(visits): add dashboard stats` + `test(visits): TC-U47`
5. `feat(visits): add check-in step 1 (find visitor)`
6. `feat(visits): add check-in step 2 and visitor pass page`
7. `feat(dashboard): live figures and on-site list with check-out`
8. `feat(visits): visit log with date and host filters`
9. `test(visits): integration test for check-in → check-out (TC-I40)`
10. `docs(p3): check-in sequence, activity diagram, class diagram`
11. `docs(qa): system and acceptance test results, defect log, demo script`
12. `docs(p3): screenshots and HANDOFF-P3`

## 9. Definition of done

- [ ] Every rule in §2 implemented with the exact messages, and covered by a test
- [ ] All screens in §3 work, including empty and error states and phone width
- [ ] Cross-links in §4 appear once Person 2's pages exist, and stay hidden before that
- [ ] Sidebar *Check in* and *Visit log* lose their **soon** tag; the dashboard shows live figures
- [ ] `dotnet build`: 0 warnings · `dotnet test`: all green
- [ ] System and acceptance tests run on merged `main`; results recorded
- [ ] Documentation in §7 committed, and `HANDOFF-P3.md` filled in
- [ ] PR opened into `main`, reviewed by Person 1 or Person 2
