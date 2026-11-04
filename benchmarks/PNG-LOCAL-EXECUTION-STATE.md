# PNG lazy-parser execution state

The lazy Deflate parser now holds its cursor, deferred match and token count in
locals while processing acquired input. It publishes persistent state before
history-origin advances, token-block output and every return or failure. Search
order, lazy decisions, tie-breaking, hash links, blocks, cancellation checkpoints
and complete packets remain exact. Fastest/stored paths are unchanged.

This removes repeated object-field traffic without a second parser, cache,
session or new buffer. The end state has one persistent representation and one
execution-local view. Preparation, the 32 KiB history slide, output growth and
independent output ownership remain inside complete operation timing.

| Existing 513×259 Optimal6 / native6, filter None | Before µs | Selected µs | FFmpeg µs | Ratio / upper | Allocated B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| RGBA16 | 27,173.04 | 25,943.81 | 22,945.50 | 1.131 / 1.144 | 3,420,516 | 963,202 |
| RGBA8 | 20,378.52 | 20,178.17 | 19,010.96 | 1.061 / 1.078 | 932,516 | 210,973 |

Independent confirmation follows the two-control short screen. The word control
saves **1,229.24 µs / 4.52% raw / 4.31% reference-normalized**, with separated
filtered and all-actual raw confidence intervals and conservative normalized
intervals. Its native bracket clock is 0.224% faster between baseline and selected
runs. The byte point improves 0.98% raw / 0.74% normalized, but all-actual and
normalized intervals overlap: **no byte incremental gain or regression claim**.
Both controls retain demonstrated gaps to parity.

All twelve actual iterations per confirmation are retained, including iterations
excluded by the benchmark's normal estimator. Four complete managed operations
per actual, native brackets, both versioned quiet guards and one CPU are preserved.
Each side creates/disposes fresh encoder/zlib/packet state and independent output;
the word reference also allocates, converts and releases its endian frame inside
timing. Source/settings, hidden RGB, full samples, normal/scalar native checks and
exact complete packets pass. No new workload grid or favorable resampling.

One immutable core/internal-test build per OS is reused throughout. No harness,
native, selector or public-test builds. Focused normal/scalar checks pass per OS:
561 internal PNG and 211 public PNG. Full/live checks pass **5,543 per OS**
(2,320 internal + 2,275 public + 948 differential). The same actual Release package
and all three examples pass Windows/Linux, including retained/async survivors.

Total allocation is unchanged. Deflate workspace admission remains **384 KiB**,
new heap arrays zero, retained operation scratch zero. Complete admitted peaks,
physical peaks and native allocations are not newly measured; these quantities
are not inferred from B/op. The caller root, failure-prefix behavior, cancellation,
refusal-before-allocation, independent outputs and cleanup remain qualified.

Only level6/9 parsing changes. Other affected color/depth/async/SmallestSize costs
remain historical until reconciled. Level1/0 PNG, decoder/inspection, other codecs,
resizing, drawing and text reuse relevant unchanged-path evidence. The four
current one-shot PNG rows retain gaps; the full 282-path objective remains open.
This development update is not a full release refresh; publication is postponed.

[Evidence](png-local-execution-state-evidence.json) pins both producers, all
screens/confirmations, source hashes, output gates and actual-package proof. Raw
`png-local-execution-state-qualified.tar.xz`: 3,614,944 B / 161 members, SHA256
`80499ff9fdf5d68e1a23d57c69c813c97bfcd8cb446f31970a556e39d678de00`,
mirrored/member-verified. It retains before/selected source and binaries, logs,
actuals, guards, packages and reproduction scripts. Qualified baseline,
[one-shot reference](PNG-WORD-ONE-SHOT-BOUNDARY.md) and
[pre-change attribution](PNG-WORD-CURRENT-ATTRIBUTION.md) archives are pinned
dependencies. A missing local audit file initially stops archival before archive
creation; corrected transfer reuses all valid measurements and qualification.
