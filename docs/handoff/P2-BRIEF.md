# Person 2 brief: Increment 1: Registration and hosts

**Your branch:** `feature/p2-registration-hosts`
**Your report role:** Requirements Analyst + C# Developer
**Requirements you deliver:** FR-01 to FR-10 ([SRS](../02-srs.md))
**Read first:** [HANDOFF-P1.md](HANDOFF-P1.md) sections 1–4 (how the layers fit, code patterns, UI kit)

You turn the foundation into something a receptionist can use to **register visitors**, **recognise returning visitors**, and **keep the staff (host) directory up to date**. Person 3 works on check-in at the same time; you don't depend on each other, but you link to each other's pages (section 4).

---

## 1. What you can rely on (already built and tested)

| You need | Already exists | Where |
|---|---|---|
| Visitor/Host tables with validation | `Visitor`, `Host` entities | `src/Visma.Core/Entities/` |
| Save/find visitors | `IVisitorRepository`: `AddAsync`, `UpdateAsync`, `GetByIdAsync`, `GetWithVisitsAsync`, `SearchAsync`, `FindByPhoneOrEmailAsync`, `PhoneExistsAsync` | `src/Visma.Core/Interfaces/` |
| Save/find hosts | `IHostRepository`: `GetAllAsync`, `GetActiveAsync`, `GetByIdAsync`, `HasVisitsAsync`, `AddAsync`, `UpdateAsync` | same |
| One phone format | `PhoneNormalizer.Normalize("0803 123 4567")` → `"08031234567"` | `src/Visma.Core/Common/` |
| Success/failure results | `OperationResult<T>.Success(x)` / `.Failure("message")` | same |
| Demo data | 7 hosts (Mr. Segun Afolabi is **inactive**), 3 visitors | `DbSeeder` |
| Test helpers | `TestDbFactory`, `FakeClock`, `TestData` | `tests/Visma.Tests/Infrastructure/` |

---

## 2. Business rules (your services)

Implement these exactly. The **message** column is the text the receptionist sees, so use it word for word; your tests can then assert on it.

### `VisitorService : IVisitorService` (`src/Visma.Core/Services/VisitorService.cs`)

Constructor: `(IVisitorRepository visitors, TimeProvider clock)`

| Method | Rule | On failure: message |
|---|---|---|
| `RegisterAsync(visitor)` | Trim name/company/e-mail; lower-case e-mail; empty optional fields become `null`; normalise phone; set `CreatedAt = clock` UTC now | — |
| | Phone already registered (any formatting) → fail | `A visitor with this phone number is already registered.` |
| `UpdateAsync(visitor)` | Visitor must exist; same trimming/normalising; keep the original `CreatedAt` | `Visitor not found.` |
| | New phone belongs to a *different* visitor → fail (`PhoneExistsAsync(phone, excludeVisitorId: visitor.Id)`) | `Another visitor already uses this phone number.` |
| `FindReturningVisitorAsync(term)` | Delegate to `FindByPhoneOrEmailAsync`; blank term → `null` | — |
| `SearchAsync(term)` | Delegate to repository | — |
| `GetDetailsAsync(id)` | Delegate to `GetWithVisitsAsync` | — |

### `HostService : IHostService` (`src/Visma.Core/Services/HostService.cs`)

Constructor: `(IHostRepository hosts)`

| Method | Rule | On failure: message |
|---|---|---|
| `CreateAsync(host)` | Trim fields; `IsActive = true` | — |
| `UpdateAsync(host)` | Host must exist; copy editable fields onto the loaded entity; **don't** change `IsActive` here | `Host not found.` |
| `SetActiveAsync(id, isActive)` | Host must exist; set flag; never delete | `Host not found.` |
| `GetAllAsync` / `GetActiveAsync` / `GetByIdAsync` | Delegate | — |

Register both in `Program.cs` by uncommenting the two **Person 2** lines.

---

## 3. Screens

All pages use `_Layout` automatically. Set `ViewData["Title"]`, wrap content in `<section class="panel">`, and use the UI kit in HANDOFF-P1 §4.

### 3.1 Visitors list: `GET /Visitors?q=…`

- **Purpose:** find anyone who has ever registered.
- **Top:** a search box (`name="q"`, placeholder *"Name, phone, e-mail or company"*) and a primary button **Register a visitor**.
- **Table columns:** Name (links to Details), Phone, E-mail, Company, Registered (`CreatedAt.ToDisplayTime()`), and actions **Edit** and **Check in**. The Check in link goes to Person 3's page; see section 4.
- **Empty states:**
  - no visitors at all: `_EmptyState` "No visitors yet", with the button **Register a visitor**
  - search found nothing: "No visitors match '{q}'", with the button **Register a visitor**, phone pre-filled if `q` looks like a phone number

### 3.2 Returning-visitor lookup: `GET /Visitors/Find?term=…`

This is the fastest path at the desk (FR-04).

- **One field:** "Phone number or e-mail", with the button **Find visitor**.
- **Found:** redirect to Details with `TempData["Success"] = "Welcome back, {FullName}."`
- **Not found:** redirect to `Create`, pre-filling `Phone` (or `Email` if the term contains `@`), with `TempData["Error"] = "No visitor with that phone or e-mail. Register them below."`
- Put a link to this page at the top of the Visitors list ("Returning visitor?").

### 3.3 Register visitor: `GET/POST /Visitors/Create?phone=…&email=…`

- **Fields:** Full name\*, Phone number\*, E-mail, Company (\* = required).
- **Query parameters** `phone` and `email` pre-fill the form. Person 3 links here with `?phone=`.
- **Button:** **Register visitor**.
- **Invalid input:** re-show the form with field errors (data annotations already supply the messages).
- **Duplicate phone:** re-show the form with the error message *and* a link: "Open {existing name}'s record". Use `FindReturningVisitorAsync(phone)` to get the id.
- **Success:** redirect to Details, with the message `Registered {FullName}.`

### 3.4 Edit visitor: `GET/POST /Visitors/Edit/{id}`

- Same fields as Create; the button is **Save changes**; the success message is `Saved changes to {FullName}.`
- Unknown id: return `NotFound()` (the app shows the friendly 404 page).

### 3.5 Visitor details: `GET /Visitors/Details/{id}`

- **Header:** name, phone, e-mail, company, "Registered {date}". Buttons: **Edit**, **Check in**.
- **Visit history table:** Date in, Host (with department), Purpose (`GetDisplayName()`), Checked out, Status (`_StatusBadge`).
- **No visits yet:** `_EmptyState` "No visits yet".

### 3.6 Host directory: `GET /Hosts`

- **Table:** Name, Department, E-mail, Phone/ext., Status, and actions **Edit** and **Deactivate** / **Reactivate**.
- **Status:** use the same pill style as `_StatusBadge`: an "Active" pill, or a muted "Inactive" pill (add a small `.status-inactive` class in your CSS section if needed).
- **Deactivate/Reactivate** is a small `<form method="post" asp-action="ToggleActive" asp-route-id="…">` with `@Html.AntiForgeryToken()` (the form tag helper adds it automatically). Never a GET link, because a link that changes data can be triggered by accident.
- **Messages:** `Deactivated {name}. They no longer appear at check-in.` / `Reactivated {name}.`
- A primary button **Add host** goes to Create.

### 3.7 Add / edit host: `GET/POST /Hosts/Create`, `/Hosts/Edit/{id}`

- **Fields:** Full name\*, Department\*, E-mail, Phone / extension.
- **Buttons:** **Add host** / **Save changes**.
- **Messages:** `Added {name} to the directory.` / `Saved changes to {name}.`

---

## 4. Links between your pages and Person 3's (the cross-team contract)

Agree on these URLs so the two increments join up without either of you editing the other's files:

| From | To | URL | Guard |
|---|---|---|---|
| Your Visitors list and Details **Check in** | Person 3's check-in, visitor pre-selected | `/Visits/CheckIn?visitorId={id}` | Show the button only if `Nav.Exists("Visits", "CheckIn")` |
| Person 3's check-in "Visitor not found? **Register them**" | Your Create page | `/Visitors/Create?phone={term}` | Person 3 shows it only if `Nav.Exists("Visitors", "Create")` |

`Nav` is `@inject NavigationHelper Nav` at the top of the view.

---

## 5. Tests you write

Put them in `tests/Visma.Tests/Services/`. Use the ID ranges reserved for you (TC-U20–U39, TC-I20–I39) and add each to [04-test-plan.md](../04-test-plan.md) §2.3 with the actual result.

| ID | Test | Expect |
|---|---|---|
| TC-U20 | Register valid visitor with `FakeClock` | Success; `CreatedAt` = clock; phone normalised; e-mail lower-cased |
| TC-U21 | Register phone that exists in another format | Failure, exact message |
| TC-U22 | Update visitor to another visitor's phone | Failure "Another visitor already uses this phone number." |
| TC-U23 | Update own record keeping the same phone | Success (not a clash with itself) |
| TC-U24 | Update unknown id | Failure "Visitor not found." |
| TC-U25 | Create host | Success; `IsActive` true |
| TC-U26 | Deactivate host, then `GetActiveAsync` | Host absent from active list, present in `GetAllAsync` |
| TC-U27 | `SetActiveAsync` unknown id | Failure "Host not found." |
| TC-U28 | `UpdateAsync` host doesn't change `IsActive` | Inactive host stays inactive after edit |

Also run, and record in §2.5 of the test plan, the system tests **TC-S03, TC-S04, TC-S05**.

---

## 6. Documentation you own

The brief marks individual contributions, so your documentation commits count as much as your code.

| Deliverable | Where | What to do |
|---|---|---|
| Registration sequence diagram | [03-design.md §5](../03-design.md) | Update the **draft** so it matches your real method names, then remove "DRAFT" |
| Class diagram | [03-design.md §4](../03-design.md) | Add `VisitorService`, `HostService`, `VisitorsController`, `HostsController` and their arrows |
| SRS validation | [02-srs.md](../02-srs.md) | As Requirements Analyst: check FR-01–FR-10 still match what you built; add a short "Validated by Person 2 on {date}" note in §5 |
| Test log | [04-test-plan.md](../04-test-plan.md) §2.3, §2.5 | Fill in your rows with actual results |
| Screenshots | `docs/screenshots/p2-*.png` | Visitors list, lookup (found and not found), register with a validation error, duplicate-phone error, details with history, host directory, deactivated host |
| Your handoff | [HANDOFF-P2.md](HANDOFF-P2.md) | Fill in the template when you finish, for Person 3 and the report |

---

## 7. Suggested commit sequence

1. `feat(visitors): implement VisitorService registration with duplicate-phone check`
2. `test(visitors): cover register/update rules (TC-U20–U24)`
3. `feat(visitors): add visitors list with search`
4. `feat(visitors): add register and edit forms`
5. `feat(visitors): add returning-visitor lookup`
6. `feat(visitors): add visitor details with visit history`
7. `feat(hosts): implement HostService` + `test(hosts): cover TC-U25–U28`
8. `feat(hosts): add host directory and add/edit forms`
9. `feat(hosts): deactivate and reactivate hosts`
10. `docs(p2): registration sequence + class diagram updates`
11. `docs(p2): test log, screenshots and HANDOFF-P2`

## 8. Definition of done

- [ ] Every rule in §2 implemented with the exact messages, and covered by a test
- [ ] All screens in §3 work, including empty and error states
- [ ] Cross-links in §4 appear once Person 3's pages exist, and stay hidden before that
- [ ] Sidebar *Visitors* and *Hosts* lose their **soon** tag; the dashboard shows Increment 1 **Ready**
- [ ] `dotnet build`: 0 warnings · `dotnet test`: all green
- [ ] Documentation in §6 committed, and `HANDOFF-P2.md` filled in
- [ ] PR opened into `main`, reviewed by Person 1 or Person 3
