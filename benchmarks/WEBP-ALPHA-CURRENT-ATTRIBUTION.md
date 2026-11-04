# Transparent WebP attribution

## WCS291 current-production capture

One current two-second capture reuses the immutable WCS291 DLL and its frozen
transparent RGBA8 control. All 98,590-byte packet/source/owner gates and both quiet
guards pass; zero builds or new fixtures. Profiled elapsed is discarded.

| Exclusive location | Samples, out of 1,962 |
| --- | ---: |
| Specialized VP8L matcher | 289 |
| VP8 coefficient coding | 131 |
| Green predictor histogram | 88 |
| Cache history | 84 |
| VP8 intra4 decisions | 77 |

The 2,362 B current matcher retains scalar prefix bounds checks. A bounded
[window-preparation experiment](WEBP-SCALAR-WINDOW-PREPARATION.md) hoists them,
but its counterbalanced photograph comparison measures 1.11% slower. It restores
exactly without builds; current costs and qualification remain valid. Samples,
skid and code size are attribution, not component times or predicted savings.
The same report retains current raw profile/JIT/maps and the adverse experiment.

## Earlier capture (PAF213)

One two-second capture reuses the existing 513×259 transparent Q75/effort4 control,
worker, harness and selector with the then-qualified DLL. Both quiet guards,
frozen settings/source/output ownership and exact 98,590-byte packet pass. No
build or production change; profiled elapsed is discarded.

| Exclusive location | Samples, out of 1,944 |
| --- | ---: |
| VP8L matcher | 285 |
| VP8 coefficient coding | 126 |
| Green predictor histogram | 91 |
| VP8 intra4 decisions | 82 |
| Cache history | 75 |
| VP8 token statistics | 67 |
| Prepared population sums | 65 |

The matcher JIT body is 2,819 B. Hot instructions include candidate-chain access,
the best-length pruning probe and inlined scalar prefix comparisons. This broadly
overlaps the retained [matching preparation](WEBP-MATCHING-PREPARATION-INCONCLUSIVE.md),
[prefix](WEBP-MATCH-PREFIX-INCONCLUSIVE.md) and
[compact matching](WEBP-COMPACT-MATCHING-INCONCLUSIVE.md) investigations. Counts,
instruction skid and code size are attribution, not method times or predicted
savings. They do not justify replaying those unchanged implementations.

No candidate is accepted or family closed. A materially different repeated-work
or workspace design remains eligible with concrete evidence. The [Lanczos preparation/output work](LANCZOS-PAIRED-ROW-OUTPUT.md) is now
qualified. Unchanged production retains applicable current correctness/package
proof; no broad repeat follows this attribution.

[Evidence](webp-current-alpha-execution-attribution-evidence.json) pins selected
samples, exact-source JIT, producers, output gates and prior investigations. Raw
`webp-current-alpha-execution-attribution.tar.xz`: 261,412 B / 22 members, SHA256
`a9ce200e792628392960adcd4190c65f1aa4cf9709b7c448f4a17fe8cb0e4e89`,
mirrored/member-verified. The trace, map, complete analysis, JIT, guard logs,
current owned sources and reproduction scripts are retained, with qualified-core
and existing reference/runtime archives pinned. Publication remains postponed.


## Bounded neighbor-query reuse audit

One untimed complete encode on the original transparent control validates exact
consecutive-query suffix recurrences and conservative bounds. Across two main
and three auxiliary match builds:

| Query | Calls | Original prefix comparisons | Logically avoidable comparisons |
| --- | ---: | ---: | ---: |
| above | 132,610 | 208,720 | 34,933 |
| left | 132,834 | 189,050 | 26,818 |
| chain | 386,441 | 2,103,847 | 0 |

Only61,751of2,501,617prefix comparisons (2.47%)could be removed before adding
cache-state/branch costs. **No neighbor-cache prototype follows.** These counts
exclude other matcher work, including the outer chain's best-length probe; they
are not CPU-time shares or savings bounds. Existing FAST alpha candidate selection
and fused prepared body counting are already present, so neither is replayed.

The one private source-projection build retains the exact98,590Bpacket, unchanged
original input and borrowed-root cleanup; both quiet guards pass. Telemetry and
extra diagnostic head comparisons are tool storage/work, with no production,
timing, allocation or peak claim. [Evidence](webp-neighbor-prefix-reuse-audit-evidence.json)
retains all five build/query counts, recurrence checks and producer identities.
Raw `webp-neighbor-prefix-reuse-audit.tar.xz`:317,616B/23members, SHA256
`d271feb4f501ed4ef90beda559b8865132c4172613f25807150331c7c3a2f886`,
mirrored/member-verified. The qualified baseline source/package dependency is pinned.
Next reconcile already required PNG9 costs before selecting further tuning.


## Current tree-preparation lifetime audit

The current code already reuses match indexes, token tails, trace capacity,
sparse populations and ALPH storage. A separate remaining repetition is real:
tree counting and output each rebuild complex code-length runs and their
19-symbol Huffman header. One warmup and one observed complete encode per
existing control check this on the current source, without changing production.

| Frozen complete control | Distinct trees / calls | Repeated calls | Allocated inside repeated calls, diagnostic B | Header bits, once per tree |
| --- | ---: | ---: | ---: | ---: |
| VP8 + lossless alpha | 50 / 100 | 50 | 40,816 | 11,096 |
| Lossless photograph | 60 / 120 | 60 | 78,296 | 8,558 |

All repeated tree bit counts agree; complete98,590/160,032B packets, frozen inputs,
source ownership, borrowed-root cleanup and quiet guards pass. The source
projection records roughly101–103µs of repeated instrumented tree calls. These
ticks are **not production phase times or predicted savings**. Allocation inside
those calls includes admission and the caller test-root implementation; it is
neither an avoided-allocation prediction nor a complete allocation/peak measure.
Per-tree byte ceilings total1,411/1,097B before owner/capacity overhead; retaining
headers would require explicit admission and preserve independent candidates.

Keep this as a lower-priority preparation lead. The observed repetition does not
justify displacing the larger complete-operation gaps; no header prototype or
timing confirmation follows. This does not close the family or establish a CPU
savings bound. Current comparison rows and the actual qualified package stay
unchanged, reusing applicable Windows/Linux/scalar/package proof. One diagnostic
build, zero production/harness/native/package builds; no broader suite or timing
repeat. Full282-path objective stays open; publication remains postponed.

[Evidence](webp-tree-preparation-audit-evidence.json) pins current sources, driver,
all calls, producer and guards. Mirrored/member-verified raw
`webp-tree-preparation-audit.tar.xz`:857,836B/262members, SHA256
`c2bcba51a795bb7979d85ae572592c86f17631e6d2a0566951fc6b6ef1917ad0`.
All current owned sources and the diagnostic assembly are retained for replay;
the qualified source/package/framework dependency is pinned separately.
