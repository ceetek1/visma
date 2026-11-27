# Demo video script (3–5 minutes)

**Owner:** Person 3 (QA lead). **Presenters:** all three members, one section each.
**Follows:** the SRS scenarios S1 → S5 ([02-srs.md §4](02-srs.md)), then the edge cases.

## Before recording

1. Stop the app, delete `src/Visma.Web/visma.db`, then `dotnet run --project src/Visma.Web`. A fresh database means
   the seeded data is predictable: **Grace Olaniyan is on site** (seeing Dr. Chinedu Eze), Ada and Ibrahim are not,
   and **Mr. Segun Afolabi** (Security) is an inactive host.
2. Browser at about 1280 × 800, light mode, zoom 100%. Close other tabs.
3. Have a second window ready at phone width (Chrome DevTools → device toolbar → iPhone SE) for the last section.
4. Record the screen with audio. Speak to the **front-desk officer's job**, not the code.

## Running order

| Time | Presenter | On screen | Say (in your own words) |
|---|---|---|---|
| 0:00–0:20 | Person 1 | Dashboard | "VISMA replaces the paper visitor book. It registers visitors, records why they came and who they're seeing, and tracks check-in and check-out." Point at the figures and Grace's pass. |
| 0:20–0:35 | Person 1 | Sidebar | "We built it in three increments; each person owned one." Name the menu items. |
| 0:35–1:20 | Person 2 | **S1: first-time visitor.** Check in → search a new phone → no match → *Register them* → fill the form (try a blank phone first: error under the field) → save | "A new visitor is registered once. Phone numbers are stored in one format so they can't be registered twice." *(Needs Increment 1 merged.)* |
| 1:20–1:50 | Person 3 | Step 2 for the new visitor: pick **Dr. Chinedu Eze** (ICT), purpose **Meeting**, note "Network upgrade proposal" → **Check in** → the visitor pass | "Only staff who are still here appear in this list, grouped by department. The pass shows who, whom, why and since when." |
| 1:50–2:25 | Person 3 | **S2: returning visitor.** Check in → type **0812-987-6543** → goes straight to step 2 for Ibrahim Musa → check him in (Delivery). Then open check-in for Ibrahim again | "Dashes and spaces don't matter: it finds Ibrahim. If someone tries to check him in twice, VISMA refuses and offers check-out instead." |
| 2:25–2:50 | Person 3 | **S3: visitor leaves.** Dashboard → **Check out** on Grace's pass → message "Checked out Grace Olaniyan after …" | "Check-out records the time and duration, and she leaves the on-site list. It's a button, not a link, so it can't happen by accident." |
| 2:50–3:10 | Person 2 | **S4: staff member leaves.** Hosts → deactivate a host → back to check-in step 2: they're gone from the list. Visit log host filter: still there, marked "(no longer here)" | "We never delete staff, so their visit history stays." *(Deactivation needs Increment 1.)* |
| 3:10–3:35 | Person 3 | **S5: question about the past.** Visit log → From/To dates → Host = Mr. Femi Okonkwo (Bursary) → **Show visits** | "Security asks who visited the Bursary last Tuesday. Filter by date and host." |
| 3:35–4:10 | Person 3 | **Edge cases, quickly:** Check in with no host or purpose chosen (field errors); visit log with start date after end date (clear message); check out a visit twice (refused) | "Every rule has a clear message telling the officer what to do next." |
| 4:10–4:35 | Person 1 | Phone-width window: dashboard, open the menu, check-in step 1; toggle **Dark mode** | "It works on a phone at the gate and in dark mode." |
| 4:35–5:00 | Person 1 | Terminal: `dotnet test` (40+ passed) | "Every rule is covered by automated unit and integration tests." Close. |

## If something goes wrong while recording

- Wrong data on screen: stop, delete `visma.db`, restart, and record that section again.
- Increment 1 not merged yet: skip the registration and deactivation steps (S1 first half, S4 first half). Seeded
  visitors Ada and Ibrahim cover check-in, and Mr. Afolabi is already inactive for S4's check-in half.

## Screenshots for the report

Already in [`docs/screenshots/`](screenshots/):

| File | Shows |
|---|---|
| `p3-checkin-step1.png` | Check-in step 1: search plus recently registered visitors (Grace marked On site) |
| `p3-checkin-step2.png` | Step 2: host grouped by department, purpose, notes |
| `p3-pass.png` | Visitor pass after a successful check-in |
| `p3-double-checkin.png` | Double check-in refused, with the current pass and Check out |
| `p3-dashboard.png` | Dashboard: today's figures and on-site passes |
| `p3-checkout-message.png` | Check-out confirmation |
| `p3-visit-log.png` | Visit log filtered by date range and host |
| `p3-dashboard-phone-dark.png`, `p3-visit-log-phone-dark.png` | Phone width (375 px), dark mode |

Person 2 adds `p2-*.png` for registration and the host directory.
