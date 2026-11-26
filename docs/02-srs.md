# 02: Software Requirements Specification (SRS)

## 1. Users (actors)

| Actor | Description |
|---|---|
| **Front-desk officer** (primary) | Receptionist or security officer who registers visitors and checks them in and out. Uses VISMA all day. |
| **Office administrator** | Maintains the host (staff) directory. In the MVP this can be the same person as the front-desk officer. |
| **Host** (secondary, passive) | Staff member being visited. Does not use the system directly in the MVP. |
| **Visitor** (passive) | Gives their details to the front-desk officer. Does not use the system directly. |

## 2. Functional requirements

Priority uses MoSCoW: **M**ust, **S**hould, **C**ould. *Owner* is the team member who implements it.

### 2.1 Visitor registration (Increment 1, Person 2)

| ID | Requirement | Priority |
|---|---|---|
| FR-01 | The system shall register a visitor with full name and phone number (required), and e-mail and company (optional). | M |
| FR-02 | The system shall reject registration when required fields are missing or the phone/e-mail format is invalid, and show which field is wrong. | M |
| FR-03 | The system shall not allow two visitors with the same phone number; instead it shall offer the existing visitor (returning-visitor flow). | M |
| FR-04 | The system shall find a returning visitor by exact phone number or e-mail address, ignoring spacing/dashes in phone numbers and letter case in e-mail. | M |
| FR-05 | The system shall list visitors and search them by partial name, phone, e-mail or company. | M |
| FR-06 | The system shall allow a visitor's details to be edited. | S |
| FR-07 | The system shall show a visitor's profile with their full visit history. | S |

### 2.2 Host directory (Increment 1, Person 2)

| ID | Requirement | Priority |
|---|---|---|
| FR-08 | The system shall maintain a directory of hosts with full name and department (required), and e-mail and phone/extension (optional). | M |
| FR-09 | The system shall allow hosts to be added and edited. | M |
| FR-10 | The system shall allow a host to be deactivated (and reactivated). Deactivated hosts cannot receive new visits but remain in visit history. Hosts are never permanently deleted. | M |

### 2.3 Visit tracking (Increment 2, Person 3)

| ID | Requirement | Priority |
|---|---|---|
| FR-11 | The system shall check a registered visitor in by recording the host being visited, the purpose (category plus optional notes), and the current date and time. | M |
| FR-12 | The system shall only allow check-in to an **active** host. | M |
| FR-13 | The system shall prevent a visitor from being checked in again while they already have an open visit (no double check-in). | M |
| FR-14 | The system shall check a visit out by recording the current date and time, and shall refuse to check out a visit that is already checked out. | M |
| FR-15 | The system shall show everyone currently on site, with who they are seeing, why, and since when. | M |
| FR-16 | The system shall show dashboard figures: on site now, check-ins today, checked out today, total registered visitors. | S |
| FR-17 | The system shall list visit history, filterable by date range and host. | M |

### 2.4 Platform (Increment 0, Person 1)

| ID | Requirement | Priority |
|---|---|---|
| FR-18 | The system shall store all data persistently in a database and create it automatically on first run. | M |
| FR-19 | The system shall provide navigation to every feature from a single menu. | M |
| FR-20 | The system shall load demo data into an empty database so it can be demonstrated immediately. | C |

## 3. Non-functional requirements

| ID | Category | Requirement | How it is met / verified |
|---|---|---|---|
| NFR-01 | Usability | A returning visitor can be checked in within **3 screens** and about 30 seconds. | Check-in flow design (P3); acceptance test TC-A01 |
| NFR-02 | Usability | The interface works on screens from **375 px** (phone) to desktop, in light and dark mode, with high-legibility type and visible keyboard focus. | Responsive layout (P1); system test TC-S10 |
| NFR-03 | Usability | Error messages say what is wrong and how to fix it. | Validation messages on entities; review |
| NFR-04 | Performance | Any page loads in under **1 second** with 10,000 stored visits on a typical laptop. | Database indexes on phone, check-in time, (visitor, check-out) |
| NFR-05 | Reliability | No visit or host record is ever lost: deletes are restricted and hosts are deactivated, not deleted. | `DeleteBehavior.Restrict`, `Host.IsActive`; TC-I12 |
| NFR-06 | Reliability | Data survives application restarts. | SQLite file storage; TC-S09 |
| NFR-07 | Data validation | All input is validated on the **server** (and in the browser where possible); the database enforces unique phone numbers and valid foreign keys as a final safeguard. | Data annotations + service checks + DB constraints; TC-U05–U07, TC-I02 |
| NFR-08 | Data integrity | All times are stored in UTC and shown in local time. | UTC converter in `VismaDbContext`; TC-I08 |
| NFR-09 | Maintainability | Code is split into presentation, business and data layers, uses interfaces and dependency injection, and public members are documented. | Solution structure; code review |
| NFR-10 | Portability | Runs on Windows, macOS and Linux with only the .NET SDK installed. | .NET 11 + SQLite |
| NFR-11 | Testability | Business rules are unit-testable without a database or real clock. | Interfaces + `TimeProvider`; test infrastructure |

## 4. Scenarios

**S1: First-time visitor (FR-01, FR-02, FR-11).**
Ada arrives for a meeting with the ICT department. The officer searches her phone number, gets no match, and registers her (name, phone, company). They then check her in to Dr. Chinedu Eze (ICT) with purpose "Meeting" and the note "Network upgrade proposal". The dashboard shows Ada as on site.

**S2: Returning visitor (FR-03, FR-04, FR-13).**
Ibrahim returns a week later. The officer types "0812-987-6543"; VISMA finds Ibrahim (despite the dashes) and goes straight to check-in. If the officer accidentally tries to check Ibrahim in again before he leaves, VISMA refuses and shows that he is already on site.

**S3: Visitor leaves (FR-14, FR-15).**
Ada returns her pass. The officer clicks **Check out** next to her name on the dashboard. Her visit shows the check-out time and duration; she no longer appears as on site. Clicking check-out again is refused.

**S4: Staff member leaves the organisation (FR-10, FR-12).**
Mr. Afolabi leaves. The administrator deactivates him. He no longer appears in the check-in host list, but his past visits still show his name in the history.

**S5: Answering a question about the past (FR-17).**
Security asks who visited the Bursary last Tuesday. The officer filters the visit log by that date and by host Mr. Femi Okonkwo.

## 5. Validation of requirements

Requirements were reviewed by all three members against the MVP brief (register, record purpose, identify host, track check-in/out). Each requirement has an owner, and each **M**ust requirement maps to at least one test case in [04-test-plan.md](04-test-plan.md).
