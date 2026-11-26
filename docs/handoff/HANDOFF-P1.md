# Handoff: Increment 0 (Foundation) → Persons 2 and 3

**From:** Person 1 (Foundation)
**Branch:** `feature/p1-foundation` → merged into `main`
**Status:** builds with 0 warnings, 26/26 tests pass, app runs and seeds demo data.

This note explains what already exists, what each of you builds next, and how to build it so the pieces fit together without merge conflicts. Read sections 1–4 together, then open **your brief** (section 5).

---

## 1. What Increment 0 delivered

| Area | Files | What it gives you |
|---|---|---|
| Domain model | `src/Visma.Core/Entities/` (`Visitor`, `Host`, `Visit`), `Enums/` (`VisitPurpose`, `VisitStatus`) | The three tables, with validation attributes already on every field |
| Helpers | `src/Visma.Core/Common/` (`OperationResult<T>`, `PhoneNormalizer`, `DashboardStats`) | Return success/failure from services; one canonical phone format |
| Contracts | `src/Visma.Core/Interfaces/` (repositories), `src/Visma.Core/Services/` (services) | **The interfaces you implement**, fully documented |
| Data access | `src/Visma.Data/` (`VismaDbContext`, `Repositories/`, `Migrations/`, `Seed/DbSeeder.cs`, `DependencyInjection.cs`) | Working, tested repositories; DB auto-created and seeded on startup |
| Web shell | `src/Visma.Web/Program.cs`, `Views/Shared/_Layout.cshtml`, `wwwroot/css/visma.css`, `wwwroot/js/site.js` | Sidebar layout, theme, light/dark mode, live clock |
| Reusable UI | `Views/Shared/_VisitorPass`, `_StatusBadge`, `_Alerts`, `_EmptyState`; `Infrastructure/DisplayExtensions.cs` | Drop-in pieces so every page looks consistent |
| Smart navigation | `Infrastructure/NavigationHelper.cs` | Your sidebar link turns on **automatically** when your controller exists |
| Tests | `tests/Visma.Tests/Infrastructure/` (`TestDbFactory`, `FakeClock`, `TestData`) + 26 tests | Copy the patterns for your own tests |
| Docs | `docs/01`–`04`, `README.md`, `CONTRIBUTING.md` | Plan, SRS, diagrams, test log; you add your rows |

### Demo data (seeded into an empty DB)

- **Hosts:** 7 across Administration, Admissions, ICT, HR, Bursary, Library, Security. *Mr. Segun Afolabi (Security) is inactive* so you can test FR-12.
- **Visitors:** Ada Okafor `08031234567`, Ibrahim Musa `08129876543`, Grace Olaniyan `07065554433`.
- **Visits:** 2 completed in the past, and **Grace is currently on site** (checked in 40 minutes before the app first ran).

Reset any time: stop the app, delete `src/Visma.Web/visma.db`, run again.

---

## 2. Getting started (both of you)

```bash
git clone https://github.com/ceetek1/visma.git && cd visma
git checkout main && git pull
git checkout -b feature/p2-registration-hosts      # Person 3: feature/p3-visits-dashboard
dotnet tool restore
dotnet build            # expect: 0 Warning(s), 0 Error(s)
dotnet test             # expect: Passed 26
dotnet run --project src/Visma.Web
```

Open the printed URL. The sidebar shows your pages with a **soon** tag; clicking one opens a "isn't built yet" page naming you as owner. That's expected.

---

## 3. How the layers fit (the rules)

```
Browser → Controller (Visma.Web) → Service (Visma.Core) → Repository interface (Visma.Core)
                                                         ↳ implemented by Repository (Visma.Data) → SQLite
```

1. **Controllers are thin.** They read the form, call a service, and choose a view or redirect. No business rules in controllers or views.
2. **Services hold the rules** (duplicates, active host, double check-in). They live in `src/Visma.Core/Services/` next to their interface. They depend only on repository **interfaces** and `TimeProvider`.
3. **Never call `DateTime.Now` / `DateTime.UtcNow`** in services; inject `TimeProvider` and use `clock.GetUtcNow().UtcDateTime`. Store UTC; display with `.ToDisplayTime()` in views.
4. **Expected failures** ("already checked in") return `OperationResult<T>.Failure("message")`. The message is shown to the user, so write it for a receptionist: say what happened and what to do.
5. **Register your services** in `Program.cs`. The exact lines are already there, commented out; uncomment yours.
6. **Normalise phone numbers** with `PhoneNormalizer.Normalize()` before saving or comparing. The repository lookup already does this for searches.

### Pattern: controller action (Post/Redirect/Get + flash message)

```csharp
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Visitor model)
{
    if (!ModelState.IsValid) return View(model);           // field errors from data annotations

    var result = await _visitors.RegisterAsync(model);
    if (!result.Succeeded)
    {
        ModelState.AddModelError(string.Empty, result.Error!); // shown by asp-validation-summary
        return View(model);
    }

    TempData["Success"] = $"Registered {result.Value!.FullName}.";   // shown once by _Alerts
    return RedirectToAction(nameof(Details), new { id = result.Value.Id });
}
```

### Pattern: service with an injected clock

```csharp
public class VisitorService(IVisitorRepository visitors, TimeProvider clock) : IVisitorService
{
    public async Task<OperationResult<Visitor>> RegisterAsync(Visitor visitor)
    {
        visitor.FullName = visitor.FullName.Trim();
        visitor.Phone = PhoneNormalizer.Normalize(visitor.Phone);

        if (await visitors.PhoneExistsAsync(visitor.Phone))
            return OperationResult<Visitor>.Failure("This phone number is already registered. Use the existing visitor instead.");

        visitor.CreatedAt = clock.GetUtcNow().UtcDateTime;
        await visitors.AddAsync(visitor);
        return OperationResult<Visitor>.Success(visitor);
    }
    // ...
}
```

### Pattern: unit-testing a service

```csharp
[Trait("Level", "Unit")]
public class VisitorServiceTests
{
    // TC-U21
    [Fact]
    public async Task Register_DuplicatePhone_Fails()
    {
        using var db = new TestDbFactory();
        var service = new VisitorService(new VisitorRepository(db.Context), new FakeClock());
        await service.RegisterAsync(TestData.Visitor(phone: "08031234567"));

        var result = await service.RegisterAsync(TestData.Visitor("Someone Else", phone: "0803 123 4567"));

        Assert.False(result.Succeeded);
    }
}
```

(Strictly, a test that uses `TestDbFactory` is an *integration* test. That's fine; tag it `Level=Integration` if you want the distinction to be exact.)

---

## 4. UI kit: use these so pages match

| Need | Use |
|---|---|
| Page title in the top bar | `@{ ViewData["Title"] = "Visitors"; }` |
| Content box | `<section class="panel"> <header class="panel-head"><h2 class="panel-title">…</h2><p class="panel-sub">…</p></header> … </section>` |
| Table | `<div class="table-responsive"><table class="table table-hover">…</table></div>` (already styled as a ruled logbook) |
| Form fields | Bootstrap: `<label asp-for="FullName" class="form-label"></label><input asp-for="FullName" class="form-control" /><span asp-validation-for="FullName" class="small"></span>` (ASP.NET adds the red error style automatically) |
| Client-side validation | At the end of a form view: `@section Scripts { <partial name="_ValidationScriptsPartial" /> }` |
| Primary/secondary buttons | `btn btn-primary` / `btn btn-outline-secondary`; icons: `<i class="bi bi-person-plus" aria-hidden="true"></i>` ([Bootstrap Icons](https://icons.getbootstrap.com/)) |
| Success/error message after redirect | `TempData["Success"] = "…"` or `TempData["Error"] = "…"` (the layout shows it) |
| Nothing to show | `<partial name="_EmptyState" model='new EmptyStateModel("bi-people", "No visitors yet", "Register the first visitor to see them here.", "Register a visitor", Url.Action("Create"))' />` |
| Visit status pill | `<partial name="_StatusBadge" model="visit.Status" />` |
| A visit as a lanyard pass | `<partial name="_VisitorPass" model="visit" />` (needs `Visitor` and `Host` loaded; repository methods already include them) |
| Purpose dropdown | `<select asp-for="Purpose" asp-items="Html.GetEnumSelectList<VisitPurpose>()" class="form-select"></select>` |
| Friendly enum text / time / duration | `visit.Purpose.GetDisplayName()`, `visit.CheckInTime.ToDisplayTime()`, `visit.Duration(now).ToFriendlyDuration()` |
| Link to a teammate's page that may not exist yet | `@inject NavigationHelper Nav` then `Nav.Exists("Visitors", "Create")` |
| Your own CSS | `wwwroot/css/visma.css`, **only** under your marked section at the bottom; use the `--v-*` colour tokens so dark mode works |

**Writing style for UI text:** sentence case, plain verbs, the button says what happens ("Check in", "Save changes", "Deactivate host"), and the success message repeats the verb ("Checked in Ada Okafor."). No ALL-CAPS labels.

---

## 5. Your brief

Each of you has a standalone brief with the exact business rules and user messages, a spec for every screen, the tests to write (with IDs), the documentation you own, a commit plan and a definition of done:

| Person | Brief | When you finish, fill in |
|---|---|---|
| Person 2: Registration and hosts | [P2-BRIEF.md](P2-BRIEF.md) | [HANDOFF-P2.md](HANDOFF-P2.md) |
| Person 3: Visits and dashboard (QA lead) | [P3-BRIEF.md](P3-BRIEF.md) | [HANDOFF-P3.md](HANDOFF-P3.md) |

## 6. Where your work meets (cross-team contract)

Your increments link to each other through these URLs. Both briefs repeat this table; don't change a URL without telling the other person.

| From | To | URL | Show only if |
|---|---|---|---|
| P2 visitor list/details **Check in** | P3 check-in, visitor pre-selected | `/Visits/CheckIn?visitorId={id}` | `Nav.Exists("Visits", "CheckIn")` |
| P3 check-in **Register them** | P2 register form, phone pre-filled | `/Visitors/Create?phone={term}` | `Nav.Exists("Visitors", "Create")` |
| P3 visit log visitor name | P2 visitor details | `/Visitors/Details/{id}` | `Nav.Exists("Visitors", "Details")` |

**Shared files and who may edit what:**

| File | Person 2 | Person 3 |
|---|---|---|
| `Program.cs` | Uncomment the two Person 2 lines | Uncomment the Person 3 line |
| `wwwroot/css/visma.css` | Only below "Person 2" marker | Only below "Person 3" marker |
| `HomeController` + `Views/Home/Index.cshtml` | — | Owns (becomes the dashboard) |
| `docs/03-design.md` | §4 (add classes), §5 | §4 (add classes), §6, §7 |
| `docs/04-test-plan.md` | §2.3, rows S03–S05 | §2.4–2.6, §3 (QA lead) |

Assembling the final PDF report: see [05-report-checklist.md](../05-report-checklist.md).

## 7. Changing the database

Only if you **must** (e.g. a new column), add a migration. Never edit `InitialCreate`:

```bash
dotnet ef migrations add AddSomething --project src/Visma.Data --startup-project src/Visma.Web
```

Say so in your PR so the others delete their local `visma.db`.

---

## 8. Design decisions (and why)

| Decision | Reason |
|---|---|
| SQLite, not SQL Server | No server to install; one file; identical on Windows/macOS |
| Repository + service interfaces | Separation of concerns required by the brief; services are testable without a web server |
| `OperationResult<T>` instead of exceptions for rule violations | "Already checked in" is normal, not exceptional; controllers show the message |
| Phone is the unique visitor key | Everyone has one, and it's how front desks identify returning visitors |
| Hosts deactivated, never deleted; `Restrict` deletes | Visit history must never lose who was visited |
| UTC storage + `TimeProvider` | Correct times regardless of server time zone; deterministic tests |
| Real SQLite in tests (not EF InMemory) | InMemory ignores unique/foreign-key constraints, hiding bugs |
| `NavigationHelper` "soon" links | Teammates merge independently without editing the layout |

---

## 9. Troubleshooting

| Problem | Fix |
|---|---|
| `dotnet: command not found` / wrong version | Install the .NET 11 SDK; `dotnet --version` must start with `11` |
| `dotnet ef` not found | Run `dotnet tool restore` in the repo root |
| `SQLite Error 1: no such column…` after pulling | Someone added a migration: delete `src/Visma.Web/visma.db` and run again |
| Port already in use | Another copy is running. Stop it, or `dotnet run --project src/Visma.Web --urls http://localhost:5050` |
| Page has no styling (macOS) and CSS downloads as 0 bytes | Your files have the macOS *hidden* flag (seen when the repo sits inside a hidden folder like `.claude/`). ASP.NET skips hidden files. Run `chflags -R nohidden src/Visma.Web/wwwroot`, or clone into a normal folder |
| Changes to CSS not showing | Hard refresh (Ctrl/Cmd + Shift + R) |
