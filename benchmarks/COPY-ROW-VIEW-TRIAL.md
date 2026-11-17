# Complete-row view trial

**Not retained.** Complete original word forward/reverse copying measures
**5.527440 → 6.602130 µs**, an adverse **19.44% / 1.074689 µs** point,
with an inconclusive ratio interval **.988096–1.444034**. Allocation stays
**0 B/op**. No confirmation, favorable replay or standalone-reference refresh.
The qualified [prepared-row implementation](RECTANGLE-COPY-PREPARED-ROWS.md)
and all current displayed costs remain unchanged.

A two-second profile of the actual qualified DLL retains **1,997 leaves / zero
lost**: 1,984 in the row helper, seven in its public caller, six elsewhere.
Same-process codegen/map reconciliation matches all 1,984 helper instruction
boundaries. Locations cover bulk copying (1,350), tail handling (326), row advance
and cancellation (237), bulk-view setup (70), and one other location. These are
code locations, not phase times, stalls or a savings forecast. Public-validation
tuning is not selected.

The isolated candidate establishes one bounded complete row per side and shares
it across the existing four-load/four-store blocks, remaining vectors and one
overlapping final vector for a partial end. It aims to remove repeated span
preparation. Exact coverage, original snapshot traversal and row checkpoints
remain; public selection, shared Copy, portable/same-row/separate-word fallbacks,
admission and ownership are verbatim. No workspace, pool, pin or API is added.
The JIT instead retains more checks and spills: helper **438 → 588 B**, local
stack **24 → 56 B**. This does not isolate the cause of the adverse timing.

Both OSes pass **six internal /190 public checks** per normal, AVX-512-disabled
and scalar mode. Two new private theories cover every byte length 256–1,025 in
both row directions, five rows, complete canaries and unchanged inputs. Original
word-worker snapshots, hidden RGB, cancellation, refusal and recovery pass both
DLLs in all three modes. CPU0, natural nonconcurrent GC, equal warmups, three
reversed 16,384-operation pairs and both idle-v2 guards remain; every actual stays.

New owned/admitted/retained capacity is zero; caller storage and physical/native/
JIT peaks remain distinct. One isolated Windows core/test build; zero VM,
canonical, public-test, actor, reference, fixture or package builds. Reuse **6,794
full/live checks**, the same actual package and three examples each OS. Offline
selector/recipe corrections reuse completed captures and builds; no operation
repeats. No neighboring row-view, tail, pin or group sweep follows.

[Evidence](copy-row-view-trial-evidence.json) pins attribution, actuals and checks.
Mirrored/member-verified `copy-row-view-trial.tar.xz`: **2,691,584 B /86 members**,
SHA256 `9ecf74139d2777f6e689f5930fe1d625b43cb8ee757b6ffd8625200cb93fc311`.
Archived `cwa1197-*` / `wvr1198-*` recipes and the declared qualified dependency
reproduce the round. It supplies no current displayed costs or renderer dependency.
