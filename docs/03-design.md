# 03: System Modelling and Design

All diagrams are written in [Mermaid](https://mermaid.js.org/), which GitHub renders automatically.
**For the PDF report:** paste each diagram into <https://mermaid.live>, then use *Actions → PNG* to export it.

| Diagram | Status | Owner |
|---|---|---|
| Use case | Final | Person 1 |
| Layered architecture | Final | Person 1 |
| Entity–relationship (data storage) | Final | Person 1 |
| Class | Final for Core/Data and Increment 2; Person 2 adds their services | Person 1 (+ P2, P3) |
| Sequence: register visitor | **Draft**: confirm against your code | Person 2 |
| Sequence: check in | Final (matches `VisitsController` / `VisitService`) | Person 3 |
| Activity: visit lifecycle | Final | Person 3 |

---

## 1. Use case diagram

Mermaid has no native UML use-case notation, so actors are drawn as nodes on the left and use cases as rounded nodes inside the system boundary.

```mermaid
flowchart LR
    officer(["👤 Front-desk officer"])
    admin(["👤 Office administrator"])

    subgraph VISMA["VISMA system"]
        direction TB
        UC1(["Register visitor"])
        UC2(["Find returning visitor"])
        UC3(["Search / edit visitors"])
        UC4(["Check visitor in"])
        UC5(["Record purpose of visit"])
        UC6(["Select host being visited"])
        UC7(["Check visitor out"])
        UC8(["View who is on site"])
        UC9(["View visit history"])
        UC10(["Manage host directory"])
        UC11(["Deactivate host"])
    end

    officer --- UC1
    officer --- UC2
    officer --- UC3
    officer --- UC4
    officer --- UC7
    officer --- UC8
    officer --- UC9
    admin --- UC10

    UC4 -. "«include»" .-> UC5
    UC4 -. "«include»" .-> UC6
    UC2 -. "«extend»" .-> UC4
    UC1 -. "«extend»" .-> UC4
    UC11 -. "«extend»" .-> UC10
```

## 2. Layered architecture

```mermaid
flowchart TB
    browser["🌐 Browser<br/>(front-desk PC or phone)"]

    subgraph Web["Presentation layer: Visma.Web (ASP.NET Core MVC)"]
        controllers["Controllers<br/>Home · Visitors · Hosts · Visits"]
        views["Razor views + partials<br/>_Layout · _VisitorPass · _StatusBadge"]
        program["Program.cs<br/>(dependency injection)"]
    end

    subgraph Core["Business layer: Visma.Core"]
        services["Services<br/>VisitorService · HostService · VisitService"]
        entities["Entities<br/>Visitor · Host · Visit"]
        contracts["Interfaces<br/>IVisitorRepository · IHostRepository · IVisitRepository"]
    end

    subgraph Data["Data layer: Visma.Data"]
        repos["Repositories<br/>(EF Core implementations)"]
        ctx["VismaDbContext<br/>+ Migrations + DbSeeder"]
    end

    db[("SQLite<br/>visma.db")]

    browser <-->|HTTP| controllers
    controllers --> views
    controllers -->|calls| services
    services -->|uses| contracts
    services --> entities
    repos -.->|implements| contracts
    repos --> ctx
    ctx <-->|SQL| db
    program -.->|wires up| controllers
    program -.->|wires up| repos
```

**Rules of the architecture**

- Controllers never touch `VismaDbContext` directly; they call services (or, for simple reads, repository interfaces).
- `Visma.Core` references no other project, so business rules can be unit-tested without a web server or database.
- All bindings between interfaces and classes live in `Program.cs`.

## 3. Entity–relationship diagram (data storage)

```mermaid
erDiagram
    VISITORS ||--o{ VISITS : "makes"
    HOSTS    ||--o{ VISITS : "receives"

    VISITORS {
        int      Id PK
        string   FullName    "required, 2-100"
        string   Phone       "required, UNIQUE, normalised digits"
        string   Email       "optional, indexed"
        string   Company     "optional"
        datetime CreatedAt   "UTC"
    }
    HOSTS {
        int      Id PK
        string   FullName    "required"
        string   Department  "required"
        string   Email       "optional"
        string   Phone       "optional"
        bool     IsActive    "false = deactivated"
    }
    VISITS {
        int      Id PK
        int      VisitorId FK "RESTRICT delete"
        int      HostId    FK "RESTRICT delete"
        string   Purpose      "enum stored as text"
        string   Notes        "optional, max 250"
        datetime CheckInTime  "UTC, indexed"
        datetime CheckOutTime "UTC, NULL = on site"
    }
```

Indexes: `IX_Visitors_Phone` (unique), `IX_Visitors_Email`, `IX_Visits_CheckInTime`, `IX_Visits_VisitorId_CheckOutTime` (speeds up the double check-in rule and the on-site list), `IX_Visits_HostId`.

## 4. Class diagram

```mermaid
classDiagram
    direction LR

    class Visitor {
        +int Id
        +string FullName
        +string Phone
        +string? Email
        +string? Company
        +DateTime CreatedAt
        +ICollection~Visit~ Visits
    }
    class Host {
        +int Id
        +string FullName
        +string Department
        +string? Email
        +string? Phone
        +bool IsActive
        +ICollection~Visit~ Visits
    }
    class Visit {
        +int Id
        +int VisitorId
        +int HostId
        +VisitPurpose Purpose
        +string? Notes
        +DateTime CheckInTime
        +DateTime? CheckOutTime
        +VisitStatus Status
        +Duration(DateTime nowUtc) TimeSpan
    }
    class VisitPurpose {
        <<enumeration>>
        Meeting
        Interview
        Delivery
        Maintenance
        Official
        Personal
        Other
    }
    class VisitStatus {
        <<enumeration>>
        CheckedIn
        CheckedOut
    }
    class OperationResult~T~ {
        +bool Succeeded
        +T? Value
        +string? Error
        +Success(T value)$
        +Failure(string error)$
    }

    class IVisitorRepository {
        <<interface>>
        +GetByIdAsync(int id)
        +GetWithVisitsAsync(int id)
        +FindByPhoneOrEmailAsync(string term)
        +SearchAsync(string? term)
        +PhoneExistsAsync(string phone, int? excludeId)
        +AddAsync(Visitor v)
        +UpdateAsync(Visitor v)
    }
    class IHostRepository {
        <<interface>>
        +GetByIdAsync(int id)
        +GetAllAsync()
        +GetActiveAsync()
        +HasVisitsAsync(int hostId)
        +AddAsync(Host h)
        +UpdateAsync(Host h)
    }
    class IVisitRepository {
        <<interface>>
        +GetByIdAsync(int id)
        +GetActiveVisitForVisitorAsync(int visitorId)
        +GetActiveVisitsAsync()
        +GetHistoryAsync(from, to, hostId, visitorId)
        +CountCheckInsAsync(from, to)
        +AddAsync(Visit v)
        +UpdateAsync(Visit v)
    }

    class IVisitorService {
        <<interface>>
        +RegisterAsync(Visitor v) OperationResult~Visitor~
        +UpdateAsync(Visitor v) OperationResult~Visitor~
        +FindReturningVisitorAsync(string term)
        +SearchAsync(string? term)
        +GetDetailsAsync(int id)
    }
    class IHostService {
        <<interface>>
        +GetAllAsync()
        +GetActiveAsync()
        +CreateAsync(Host h) OperationResult~Host~
        +UpdateAsync(Host h) OperationResult~Host~
        +SetActiveAsync(int id, bool active) OperationResult~Host~
    }
    class IVisitService {
        <<interface>>
        +CheckInAsync(visitorId, hostId, purpose, notes) OperationResult~Visit~
        +CheckOutAsync(int visitId) OperationResult~Visit~
        +GetActiveVisitsAsync()
        +GetHistoryAsync(from, to, hostId, visitorId)
        +GetDashboardStatsAsync() DashboardStats
    }

    class VisitorRepository
    class HostRepository
    class VisitRepository
    class VismaDbContext {
        +DbSet~Visitor~ Visitors
        +DbSet~Host~ Hosts
        +DbSet~Visit~ Visits
    }

    Visitor "1" --> "0..*" Visit : makes
    Host "1" --> "0..*" Visit : receives
    Visit --> VisitPurpose
    Visit --> VisitStatus

    VisitorRepository ..|> IVisitorRepository
    HostRepository ..|> IHostRepository
    VisitRepository ..|> IVisitRepository
    VisitorRepository --> VismaDbContext
    HostRepository --> VismaDbContext
    VisitRepository --> VismaDbContext

    IVisitorService ..> IVisitorRepository : uses
    IHostService ..> IHostRepository : uses

    class VisitService {
        -IVisitRepository visits
        -IVisitorRepository visitors
        -IHostRepository hosts
        -TimeProvider clock
        +CheckInAsync(visitorId, hostId, purpose, notes) OperationResult~Visit~
        +CheckOutAsync(int visitId) OperationResult~Visit~
        +GetActiveVisitsAsync()
        +GetHistoryAsync(from, to, hostId, visitorId)
        +GetDashboardStatsAsync() DashboardStats
    }
    class DashboardStats {
        <<record>>
        +int OnSiteNow
        +int CheckInsToday
        +int CheckedOutToday
        +int TotalVisitors
    }
    class VisitsController {
        +CheckIn(string? q, int? visitorId) IActionResult
        +CheckIn(CheckInFormModel form) IActionResult
        +Pass(int id) IActionResult
        +CheckOut(int id, string? returnUrl) IActionResult
        +History(DateOnly? from, DateOnly? to, int? hostId) IActionResult
    }
    class HomeController {
        +Index() IActionResult
    }
    class CheckInFormModel {
        +int VisitorId
        +int? HostId
        +VisitPurpose? Purpose
        +string? Notes
    }
    class HomeIndexViewModel {
        +IReadOnlyList~Visit~ OnSite
        +DashboardStats Stats
    }
    class VisitHistoryViewModel {
        +DateOnly From
        +DateOnly To
        +int? HostId
        +IReadOnlyList~Visit~ Visits
    }

    VisitService ..|> IVisitService
    VisitService ..> IVisitRepository : uses
    VisitService ..> IVisitorRepository : uses
    VisitService ..> IHostRepository : uses
    VisitService ..> DashboardStats : creates
    VisitsController --> IVisitService
    VisitsController ..> CheckInFormModel : binds
    VisitsController ..> VisitHistoryViewModel : builds
    HomeController --> IVisitService
    HomeController ..> HomeIndexViewModel : builds
```

> Person 3's classes (Increment 2) are in the diagram. Controllers depend only on the `IVisitService`
> interface; `Program.cs` binds it to `VisitService`.
> **Person 2:** add `VisitorService ..|> IVisitorService`, `HostService ..|> IHostService` and your controllers here.

## 5. Sequence diagram: register a new visitor (DRAFT, Person 2)

```mermaid
sequenceDiagram
    actor Officer as Front-desk officer
    participant C as VisitorsController
    participant S as VisitorService
    participant R as IVisitorRepository
    participant DB as SQLite

    Officer->>C: POST /Visitors/Create (name, phone, email, company)
    C->>C: ModelState.IsValid? (data annotations)
    alt invalid input
        C-->>Officer: Re-show form with field errors
    else valid
        C->>S: RegisterAsync(visitor)
        S->>S: Trim fields, normalise phone, set CreatedAt
        S->>R: PhoneExistsAsync(phone)
        R->>DB: SELECT EXISTS … WHERE Phone = @p
        DB-->>R: true / false
        alt phone already registered
            S-->>C: Failure("already registered")
            C-->>Officer: Offer existing visitor (returning-visitor flow)
        else new phone
            S->>R: AddAsync(visitor)
            R->>DB: INSERT INTO Visitors …
            DB-->>R: new Id
            S-->>C: Success(visitor)
            C-->>Officer: Redirect to visitor details + "Registered" message
        end
    end
```

## 6. Sequence diagram: check a visitor in (Person 3)

Matches `VisitsController.CheckIn` (POST) and `VisitService.CheckInAsync`. The service checks its rules in the order
shown; the first failure is returned to the controller as `OperationResult.Failure` and shown on the form.

```mermaid
sequenceDiagram
    actor Officer as Front-desk officer
    participant C as VisitsController
    participant S as VisitService
    participant VR as IVisitorRepository
    participant HR as IHostRepository
    participant R as IVisitRepository
    participant T as TimeProvider

    Officer->>C: POST /Visits/CheckIn (VisitorId, HostId, Purpose, Notes + antiforgery token)
    C->>VR: GetByIdAsync(visitorId)
    alt visitor not found
        C-->>Officer: Redirect to step 1 + "Visitor not found."
    else ModelState invalid (no host or purpose chosen, notes too long)
        C-->>Officer: Re-show step 2 with field errors
    else
        C->>S: CheckInAsync(visitorId, hostId, purpose, notes)
        S->>VR: GetByIdAsync(visitorId)
        S->>HR: GetByIdAsync(hostId)
        alt host missing or inactive, or purpose undefined
            S-->>C: Failure("Host not found." / "… is no longer receiving visitors. …" / "Choose a purpose …")
            C-->>Officer: Re-show step 2 with the message
        else
            S->>R: GetActiveVisitForVisitorAsync(visitorId)
            R-->>S: open visit (or null)
            alt already on site
                S-->>C: Failure("… is already checked in. Check them out before checking in again.")
                C-->>Officer: Re-show step 2: message, current pass and a Check out button
            else
                S->>T: GetUtcNow()
                T-->>S: now (UTC)
                S->>R: AddAsync(Visit{…, Notes trimmed or null, CheckInTime = now})
                S-->>C: Success(visit with Visitor and Host)
                C-->>Officer: 302 to /Visits/Pass/{id} + "Checked in … to see …."
            end
        end
    end
```

## 7. Activity diagram: visit lifecycle (Person 3)

From arrival to departure, as the front-desk officer works through VISMA. Registration (the left branch) is
Person 2's Increment 1; everything from "Already on site?" onwards is Increment 2.

```mermaid
flowchart TD
    start((●)) --> arrive[Visitor arrives at front desk]
    arrive --> lookup[Check in, step 1: search phone, e-mail or name]
    lookup --> exact{Exact phone / e-mail match?}
    exact -- Yes --> onsite
    exact -- No --> list{Anyone in the result list?}
    list -- No --> register[Register visitor - Increment 1]
    register --> valid{Details valid?}
    valid -- No --> fix[Show field errors] --> register
    valid -- Yes --> onsite
    list -- Yes --> pick[Officer selects the visitor] --> onsite
    onsite{Already on site?}
    onsite -- Yes --> refuse[Refuse: show current pass and Check out button] --> stop1((◉))
    onsite -- No --> choose[Step 2: select active host, purpose, optional notes]
    choose --> rules{Host active and purpose chosen?}
    rules -- No --> choose
    rules -- Yes --> checkin[Record check-in time - UTC]
    checkin --> pass[Step 3: show visitor pass; visitor goes in]
    pass --> leave[Visitor returns to desk]
    leave --> checkout[Officer clicks Check out on the dashboard or pass]
    checkout --> already{Already checked out?}
    already -- Yes --> refuse2[Refuse: This visit was already checked out.] --> stop2((◉))
    already -- No --> record[Record check-out time; show duration]
    record --> history[Visit appears in the visit log]
    history --> stop3((◉))
```
