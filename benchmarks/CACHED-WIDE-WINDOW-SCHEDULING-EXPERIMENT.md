# Cached wide window and scheduling experiment

Two coherent stages investigate the [current horizontal tap lead](CACHED-WIDE-CURRENT-PREPARATION-ATTRIBUTION.md).
CWW700 bounds contiguous source windows before each tap traversal. CWF701 replaces
that two-output specialization with four independent ordered sums. Both preserve
the original irregular pair and singleton paths, weights, geometry, normalization,
certificates and exact repair. No storage, API, pool or numerical policy is added.

| Complete Wide8 | Managed µs | zimg µs | Ratio / upper | B/op |
| --- | ---: | ---: | ---: | ---: |
| Qualified original, short | 503.72 | 377.07 | 1.336 / 1.467 | 198,449 |
| Two windows, short | 472.09 | 348.98 | 1.353 / 1.858 | 198,448 |
| Original, independent confirmation | 504.10 | 370.07 | 1.362 / 1.431 | 198,448 |
| Two windows, independent confirmation | 468.25 | 352.24 | 1.329 / 1.405 | 198,450 |
| Four windows, short | 474.71 | 349.57 | 1.358 / 1.494 | 198,450 |

Two windows have separated raw confirmation intervals in both views:7.11% point
reduction. Native movement leaves the normalized point gain2.41%, with overlapping
intervals. **The attributed effect is inconclusive.** The four-output successor
supplies no useful further complete lead. Neither stage is selected. No causal
regression/equivalence claim follows, and no additional confirmation, variant or
favorable sampling follows. Unchanged original scoring is reused for stage two.
This outcome does not permanently close materially different designs.

Generated code verifies the mechanism. The original8,141-byte caller has two
offset loads and checked pixel indexes per tap; the contiguous loops remove them.
Two/four-window callers are8,441/9,452bytes. The four-output tap body has no stack
spills and retains four separate Multiply/Add sequences. Checked window creation,
eligibility and larger caller costs remain inside complete execution. Instruction
counts and additional independent work do not establish CPU savings. Generated-code
smoke elapsed is discarded.

All controls retain the original513×259→128×64 RGBA8 input, frozen rational/native
frames, one pinned CPU, idle-v2 guards and declared prepared zimg graph/scratch.
Public Resize preparation, workspace, independent output and disposal stay timed.
Both stages pass258internal/729public checks per normal/scalar mode on each OS,
including12 direct ordered-sum, full/unequal/zero-padding, odd-tail, canary and
cancellation cases. The four-output path checks cancellation before publishing
each pair; extra accumulation is bounded and all cache/result storage is private.
Owned workspace remains198,432B; result32,768B survives until disposal; new scratch
retention is zero. Ceil-rounded allocation differs by≤2B/op without capacity changes.
Physical/native peaks are unmeasured.

Both variants' sources and main/embedded binaries are retained. The first new
cancellation test cancelled before constructing OperationContext; its correction
requires one test-only rebuild. Both variants use one production build per OS;
total embedded builds are3Windows/2Linux. A smoke post-report expected three native
samples where smoke returns one; report-only recovery reuses its completed log and
JIT. No measurement is repeated.

Original sources, main/embedded binaries and public bindings are restored byte for
byte on both OSes, including source-manifest identity. Candidate-only tests are
archived and removed; existing behavioral coverage remains. Unchanged
[6,064 full/live checks and the same actual Release package](TEXT-ENDPOINT-STORAGE.md)
are reused, without rebuilding or repeating qualification. Both relevant CI runs
are green. The [accepted standalone costs](CACHED-WIDE-PIXEL-PREPARATION.md) remain
valid; this experiment adds no renderer dependency or current timing replacement.

[Evidence](cached-wide-window-scheduling-experiment-evidence.json) records both
designs, confidence, actual code and exact restoration. Mirrored/member-verified
raw `cached-wide-window-scheduling-experiment.tar.xz` contains7,581,000B /179members,
SHA256 `77ef963bb8ff45afd4b522c55c46d0923b8aa16ba731d96b8dc8047385667c9a`.
Worker and qualified-package providers remain linked there. Independent codec
work continues; the283-path objective remains open and publication is postponed.
