# 04: Test Plan and Validation Log

## 1. Strategy

Testing happens in four stages. Each stage builds on the previous one.

| Stage | What is tested | How | Who |
|---|---|---|---|
| **Unit** | One class or method in isolation (entities, normaliser, services) | xUnit, automated, no database or a fake clock | Author of the code |
| **Integration** | Modules working together (repository ↔ EF Core ↔ SQLite, service ↔ repository) | xUnit with real SQLite in memory (`TestDbFactory`) | Author of the code |
| **System** | The whole running application against the requirements | Manual, in the browser, following the steps below | Person 3 (QA lead), all |
| **Acceptance** | End-to-end scenarios from the SRS, as a front-desk officer would use it | Manual walkthrough, recorded for the demo video | Whole team |

### Running the automated tests

```bash
dotnet test                              # everything
dotnet test --filter Level=Unit
dotnet test --filter Level=Integration
dotnet test --logger "console;verbosity=detailed"   # see each test name, for screenshots
```

Tag new test classes with `[Trait("Level", "Unit")]` or `[Trait("Level", "Integration")]`, and put the test ID in a comment above each test.

### Test ID scheme

| Prefix | Stage | Range reserved for |
|---|---|---|
| TC-U01–U19 | Unit | Person 1 (foundation) |
| TC-U20–U39 | Unit | Person 2 (registration and hosts) |
| TC-U40–U59 | Unit | Person 3 (visits and dashboard) |
| TC-I01–I19 | Integration | Person 1 |
| TC-I20–I39 | Integration | Person 2 |
| TC-I40–I59 | Integration | Person 3 |
| TC-S01– | System | All |
| TC-A01– | Acceptance | All |

### Pass criteria

- All automated tests pass (`dotnet test` reports 0 failed).
- Every **Must** functional requirement in the SRS has at least one passing test case.
- No open defect blocks a core MVP feature.

## 2. Test-case log

**Actual** and **Result** are filled in after running. For automated tests, "Actual" is the observed behaviour when the test ran.

### 2.1 Unit tests: Increment 0 (Person 1)

| ID | Requirement | Input | Expected output | Actual | Result |
|---|---|---|---|---|---|
| TC-U01 | FR-04 | Phone "0803 123 4567", "0803-123-4567", "(0803) 1234567", " +234 803 123 4567 " | "08031234567" (and "+2348031234567") | As expected | ✅ Pass |
| TC-U02 | FR-04 | Phone null / "" / "   " | "" (no crash) | As expected | ✅ Pass |
| TC-U03 | FR-14 | Visit with no check-out, then with check-out | Status CheckedIn, then CheckedOut | As expected | ✅ Pass |
| TC-U04 | FR-15 | Check-in 09:00; now 09:30; then check-out 09:45 | Duration 30 min while open; 45 min once closed | As expected | ✅ Pass |
| TC-U05 | FR-02 | Visitor with empty name and phone | Validation errors on FullName and Phone | As expected | ✅ Pass |
| TC-U06 | FR-02 | Phone "call me", e-mail "not-an-email" | Validation errors on Phone and Email | As expected | ✅ Pass |
| TC-U07 | FR-01 | Valid name, phone, e-mail | No validation errors | As expected | ✅ Pass |
| TC-U08 | NFR-03 | Success(42) / Failure("…") | Succeeded + value / not succeeded + message | As expected | ✅ Pass |

### 2.2 Integration tests: Increment 0 (Person 1)

| ID | Requirement | Input | Expected output | Actual | Result |
|---|---|---|---|---|---|
| TC-I01 | FR-18 | Add visitor, read back in a new context | Id assigned; same name returned | As expected | ✅ Pass |
| TC-I02 | FR-03, NFR-07 | Insert two visitors with phone 08031234567 | Database rejects the second (DbUpdateException) | As expected | ✅ Pass |
| TC-I03 | FR-04 | Lookup "0803 123 4567" and "ADA.Okafor@Example.com" | Finds Ada Okafor both times | As expected | ✅ Pass |
| TC-I04 | FR-04 | Lookup unknown phone | null | As expected | ✅ Pass |
| TC-I05 | FR-05 | Search "okaf"; PhoneExists with/without excluding self | 1 result; true; false | As expected | ✅ Pass |
| TC-I06 | FR-12 | One active, one inactive host | GetActive returns only the active host | As expected | ✅ Pass |
| TC-I07 | FR-13 | Visitor with closed visit, then open visit | No active visit, then the open one (with Host/Visitor loaded) | As expected | ✅ Pass |
| TC-I08 | NFR-08 | Save visit times, read back | DateTimeKind = Utc | As expected | ✅ Pass |
| TC-I09 | FR-17 | 3 visits over 3 days, 2 hosts | Date filter → 1, host filter → 2, today's count → 1, on site → 1 | As expected | ✅ Pass |
| TC-I10 | FR-14 | Load open visit, set check-out, save | No visits on site afterwards | As expected | ✅ Pass |
| TC-I11 | FR-20 | Seed twice | 7 hosts, 3 visitors, 3 visits (1 open), with no duplicates | As expected | ✅ Pass |
| TC-I12 | NFR-05 | Seed inactive host, reload | IsActive remains false (regression) | As expected | ✅ Pass |

### 2.3 Unit and integration tests: Increment 1 (Person 2, TO DO)

Planned cases from [P2-BRIEF.md §5](handoff/P2-BRIEF.md). Fill in Actual/Result as you run them; add rows for any extra tests.

| ID | Requirement | Input | Expected output | Actual | Result |
|---|---|---|---|---|---|
| TC-U20 | FR-01 | Register valid visitor (FakeClock) | Success; CreatedAt = clock; phone normalised; e-mail lower-cased | | |
| TC-U21 | FR-03 | Register phone that exists in another format | Failure "A visitor with this phone number is already registered." | | |
| TC-U22 | FR-06 | Update visitor to another visitor's phone | Failure "Another visitor already uses this phone number." | | |
| TC-U23 | FR-06 | Update own record keeping the same phone | Success | | |
| TC-U24 | FR-06 | Update unknown id | Failure "Visitor not found." | | |
| TC-U25 | FR-09 | Create host | Success; IsActive true | | |
| TC-U26 | FR-10 | Deactivate host, then list active | Absent from active list; present in all hosts | | |
| TC-U27 | FR-10 | SetActive on unknown id | Failure "Host not found." | | |
| TC-U28 | FR-10 | Edit an inactive host | Stays inactive | | |

### 2.4 Unit and integration tests: Increment 2 (Person 3)

Planned cases from [P3-BRIEF.md §5](handoff/P3-BRIEF.md), plus TC-U49–U51 and TC-I41 added during development.
Code: `tests/Visma.Tests/Services/VisitServiceTests.cs` (unit) and `VisitFlowIntegrationTests.cs` (integration).
Run on 27 Nov 2026: `dotnet test` → 40 passed, 0 failed.

| ID | Requirement | Input | Expected output | Actual | Result |
|---|---|---|---|---|---|
| TC-U40 | FR-11 | Check in valid visitor/host at FakeClock 09:00 | Success; CheckInTime = 09:00; Visitor and Host set | As expected; notes kept | ✅ Pass |
| TC-U41 | FR-13 | Check in same visitor twice | Second fails "{name} is already checked in. …" | "Ada Okafor is already checked in. Check them out before checking in again."; still 1 visit on site | ✅ Pass |
| TC-U42 | FR-12 | Check in to inactive host | Failure "{host} is no longer receiving visitors. …" | "Mr. Segun Afolabi is no longer receiving visitors. Choose another host." | ✅ Pass |
| TC-U43 | FR-11 | Unknown visitor / unknown host | "Visitor not found." / "Host not found." | As expected | ✅ Pass |
| TC-U44 | FR-14 | Advance clock 75 min, check out | CheckOutTime = 10:15; duration 1h 15m | CheckOutTime 10:15; duration 75 min; nobody on site | ✅ Pass |
| TC-U45 | FR-14 | Check out twice | Second fails "This visit was already checked out." | As expected | ✅ Pass |
| TC-U46 | FR-13 | Check out, then check in again | New visit allowed | New visit with a new Id; history shows 2 visits | ✅ Pass |
| TC-U47 | FR-16 | 2 open + 1 closed today, 1 yesterday | OnSiteNow 2, CheckInsToday 3, CheckedOutToday 1 | 2, 3, 1 (TotalVisitors 4) | ✅ Pass |
| TC-U48 | FR-11 | Notes "   " | Stored as null | null; "  Parcel  " stored as "Parcel" | ✅ Pass |
| TC-U49 | FR-14 | Check out visit 9999 | "Visit not found." | As expected | ✅ Pass |
| TC-U50 | FR-11 | Purpose `(VisitPurpose)99` | "Choose a purpose for the visit." | As expected | ✅ Pass |
| TC-U51 | FR-16, NFR-08 | Clock in UTC+1; visits at 23:30 and 22:30 UTC the evening before | Only the 23:30 UTC visit (00:30 local) counts as today | CheckInsToday 1, CheckedOutToday 1 | ✅ Pass |
| TC-I40 | FR-11–FR-14, FR-17 | Full check-in → active list → check-out → history (service + SQLite, new DbContext per step) | Both times stored; not on site; appears in history | Check-in 09:00, check-out 09:50, status CheckedOut, notes kept; figures 0 / 1 / 1 | ✅ Pass |
| TC-I41 | FR-17 | Visit at exactly 00:00; query the day before and the same day | Day before: none; same day: 1 | As expected (range is start-inclusive, end-exclusive) | ✅ Pass |

### 2.5 System tests (run the app; all members)

| ID | Requirement | Steps | Expected output | Actual | Result |
|---|---|---|---|---|---|
| TC-S01 | FR-18, FR-20 | Delete `visma.db`, run app, open dashboard | DB created; demo data shown; 1 person on site | Dashboard shows Grace Olaniyan on site, 6 active hosts, 3 visitors | ✅ Pass |
| TC-S02 | FR-19 | Click every menu item | Each opens its page (or "isn't built yet" page before its increment) | Dashboard, Check in and Visit log open real pages; Visitors and Hosts still show "soon" (Increment 1 not merged) | ✅ Pass (Increments 0 and 2) |
| TC-S03 | FR-01, FR-02 | Register visitor with blank phone | Form re-shown, error under Phone | Not run: registration is Increment 1 (Person 2), not merged | ⏸ Blocked |
| TC-S04 | FR-03, FR-04 | Register with an existing phone in a different format | Existing visitor offered instead | Not run: registration is Increment 1. (Check-in lookup of "0803 123 4567" does find Ada Okafor; see TC-A02.) | ⏸ Blocked |
| TC-S05 | FR-10, FR-12 | Deactivate a host, open check-in | Host not in the list | Deactivating needs Increment 1. The seeded inactive host, Mr. Segun Afolabi, is **not** in the check-in host list | ◐ Partly (check-in side passes) |
| TC-S06 | FR-11 | Check in a visitor | Pass shown; appears on dashboard | Ada checked in to Mrs. Ngozi Adeyemi (Interview); pass shown with "Checked in Ada Okafor to see Mrs. Ngozi Adeyemi."; dashboard shows 2 on site | ✅ Pass |
| TC-S07 | FR-13 | Check the same visitor in again | Refused with a clear message | Grace (on site) step 2 shows "Grace Olaniyan is already checked in. Check them out before checking in again." with her pass and a Check out button; submitting the form twice also refused | ✅ Pass |
| TC-S08 | FR-14 | Check out; then try again | Times recorded; second attempt refused | "Checked out Grace Olaniyan after 40m."; check-out time in visit log; repeat gives "This visit was already checked out." | ✅ Pass |
| TC-S09 | NFR-06 | Stop and restart the app | All data still present | Same figures (2 / 3 / 1 / 3) and 5 visits in the log after restart | ✅ Pass |
| TC-S10 | NFR-02 | View at 375 px wide; toggle dark mode | Usable layout, menu opens, readable in both themes | Shell, check-in, pass, dashboard and visit log checked at 375×812 and desktop, light and dark; no sideways scrolling (after fixing D-04, D-05) | ✅ Pass |
| TC-S11 | FR-17 | Filter visit log by yesterday + one host | Only matching visits | 1–27 Nov + Dr. Chinedu Eze → 1 visit (Grace); default last 7 days → 7; start after end → clear error | ✅ Pass |

### 2.6 Acceptance tests (SRS scenarios)

| ID | Scenario | Expected outcome | Result |
|---|---|---|---|
Run on 27 Nov 2026 by Person 1 against the Increment 2 branch. **Repeat as a team walkthrough** once Increment 1
is merged, and record it for the demo video.

| ID | Scenario | Expected outcome | Result |
|---|---|---|---|
| TC-A01 | S1: First-time visitor | Registered and checked in within 3 screens | ⏸ Blocked: registration (Increment 1) not merged. Check-in half: 3 screens (find → host and purpose → pass) ✅ |
| TC-A02 | S2: Returning visitor (+ double check-in refused) | Found by formatted phone; double check-in blocked | ✅ Pass: "0803 123 4567" went straight to step 2 for Ada; checking in an on-site visitor was refused |
| TC-A03 | S3: Visitor leaves | Checked out; no longer on site; repeat refused | ✅ Pass: Check out on the dashboard pass; gone from "On site now"; repeat refused |
| TC-A04 | S4: Staff member leaves | Host hidden from check-in, kept in history | ◐ Partly: inactive Mr. Afolabi is hidden from check-in and listed as "(no longer here)" in the visit log filter; deactivating someone needs Increment 1 |
| TC-A05 | S5: Question about the past | Correct visits found by date + host | ✅ Pass: date range + host filter returned only that host's visits |

## 3. Defect log

| ID | Found in | Description | Fix | Status |
|---|---|---|---|---|
| D-01 | Increment 0, design review | A database default of `IsActive = true` would silently re-activate hosts saved as inactive (EF cannot tell `false` from "not set" for bool) | Removed DB default; kept C# initialiser; regression test TC-I12 | Fixed |
| D-02 | Increment 0, system test | Sidebar background transparent on desktop (Bootstrap `offcanvas-lg` override) | Scoped CSS override | Fixed |
| D-03 | Increment 0, accessibility review | White text on light-green pass strip in dark mode below 4.5:1 contrast | Dark text in dark mode | Fixed |
| D-04 | Increment 2, system test TC-S10 | At 375 px the visit log's date and host filters overflowed their panel, and the visit log and dashboard tables were cut off (Check out button off-screen) | Shrinkable filter grid; tables become labelled stacked cards below 576 px | Fixed |
| D-05 | Increment 2, accessibility review | Bootstrap's grey outline buttons ("Check out", "Back to dashboard") below 4.5:1 contrast in dark mode | Secondary buttons use the theme's ink and rule colours | Fixed |
