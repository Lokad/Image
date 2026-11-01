# Bounded JPEG cancellation polling: rejected

One existing live-token progressive444 short supplies **no useful gain**:
4,962.387 µs versus the retained independent baseline's 4,925.835 µs,
**0.74% higher raw / 1.07% higher native-normalized** point means. Allocation
stays **984,212 B/op**. This screen establishes no confirmed regression and earns
no longer confirmation. Selected production and its 1.445/upper1.515 gap stay.

The prototype moves per-correction-bit polls to nonempty correction groups and
ordinary-cache refills, bounded by at most63coefficient updates. It changes polling
frequency while retaining symbol/field checks, scalar tails, exact correction/
error state and no-read empty EOB tails. No new counter, quota, storage or owner.
The identical frozen 132,961-byte input and exact 531,468-byte managed/native
output pass all original alpha, CLI, source, independent-owner and quiet gates.
The paired ordinary/live controls motivate this review; their cost difference
alone does not isolate cancellation overhead.

Candidate **85 internal / 365 public** checks pass normal/scalar on Linux,
including two additional dense pre-canceled pending phases. Original source,
test cases and every saved core/test binary are restored byte-exactly without
rebuilding; **83 internal / 365 public** original checks pass both modes.
Windows and the actual development package never change. Applicable **5,376
checks per OS**, same actual package and three-example proof are reused;
no repeated full/package runs. One core/internal-test build, zero other builds,
baseline retimes, new images, profiles or workload expansion.

Original per-bit behavior remains. Close this polling primitive without group,
frequency, refill or code-layout variants. All268required paths and the105-row
summary remain; full warm parity is open.

[Numeric evidence](jpeg-bounded-cancellation-polls-rejected-evidence.json) gives
costs, actuals, confidence and restoration. Raw
`jpeg-bounded-cancellation-polls-rejected.tar.xz`, SHA256
5c6a99ecdb23dfd40027f67548f4c2eb33bbb7516fce30186c20d25d9672cd83 (3,165,876 B / 65 members),
is mirrored and member-verified. It retains full samples/brackets, guards,
original/candidate source and binaries, cached driver/worker, fixtures, logs and
reproduction scripts. Dependencies retain baseline and unchanged qualification.
