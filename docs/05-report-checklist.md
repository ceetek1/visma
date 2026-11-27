# 05: Final report checklist

The submission is **one PDF**. This table maps every component the brief asks for to where its content already lives in the repo, and who is responsible for it. Tick items off as they're ready.

| ✓ | Report component (from the brief) | Source in this repo | Owner |
|---|---|---|---|
| [ ] | Project overview: project number and title, methodology | [01-project-plan.md](01-project-plan.md) §1–3 | Person 1 |
| [ ] | Feasibility study (technical, operational, schedule) | [01-project-plan.md](01-project-plan.md) §4 | Person 1 |
| [ ] | Risk management summary | [01-project-plan.md](01-project-plan.md) §7 | Person 1 |
| [ ] | SRS: functional and non-functional requirements, scenarios | [02-srs.md](02-srs.md) | Person 2 (validated by all) |
| [ ] | Use case diagram | [03-design.md](03-design.md) §1 | Person 1 |
| [ ] | Class diagram | [03-design.md](03-design.md) §4 | Person 1 + updates from P2 and P3 |
| [ ] | Sequence diagram(s) | [03-design.md](03-design.md) §5 (P2), §6 (P3) | Persons 2 and 3 |
| [x] | Activity diagram | [03-design.md](03-design.md) §7 | Person 3 |
| [ ] | System architecture diagram (layers + data storage) | [03-design.md](03-design.md) §2, §3 | Person 1 |
| [ ] | Testing and validation log: test plan + test-case table | [04-test-plan.md](04-test-plan.md) | Person 3 (QA lead), rows by all |
| [ ] | GitHub repository link (public or view access) | `https://github.com/ceetek1/visma` | Person 1 |
| [ ] | Video demonstration link | [HANDOFF-P3.md](handoff/HANDOFF-P3.md) §6 | Person 3 |
| [ ] | Application screenshots | `docs/screenshots/` | All (collected by Person 3) |
| [ ] | Group members, matriculation numbers and roles | [01-project-plan.md](01-project-plan.md) §6 | Person 1 |

## Turning the docs into the PDF

1. **Diagrams:** paste each Mermaid block from `03-design.md` into <https://mermaid.live>, then use *Actions → PNG*. Save the files as `docs/diagrams/*.png`.
2. **Text:** the Markdown files can be pasted into Word or Google Docs (headings and tables survive), or exported directly. In VS Code, the "Markdown PDF" extension does this.
3. **Order:** cover page → overview and feasibility → SRS → diagrams → architecture and design notes → testing log → screenshots → links (GitHub, video) → members and roles.
4. **Before submitting:**
   - [ ] Fresh clone of `main` builds and runs with no errors (the brief requires it)
   - [ ] GitHub repo and video links open in a private/incognito window
   - [ ] Every **Must** requirement has a passing test in the log
