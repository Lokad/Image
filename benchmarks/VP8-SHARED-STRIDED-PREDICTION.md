# VP8 shared large-prediction experiment

Two exact prototypes reuse the encoder's large-block predictor in decoding.
Neither establishes a useful complete-operation gain or reduces allocation, so
the qualified implementation is restored. This closes these implementations,
without establishing a loss or ruling out a materially revised design.

The compact adapter snapshots the left border, predicts into a256Bstack tile and
copies its rows into the decoder plane. The direct variant gives the shared kernel
a validated destination stride, removing the tile/copy. Compact encoder callers
pass their existing block width. Mode mapping, absent-border defaults, rounding,
saturation, border snapshots and portable fallback remain exact. No new heap,
owner, pool, cache, session or numerical policy is introduced.

| Complete operation | Original µs | Candidate µs | B/op, both | Result |
| --- | ---: | ---: | ---: | --- |
| Compact adapter, live opaque decode, independent12actuals | 6,307.18 | 6,307.63 | 760,513 | Essentially flat; inconclusive |
| Direct stride, live opaque decode, independent12actuals | 6,407.68 | 6,381.16 | 760,513 | 26.52µs point gain; inconclusive |
| Direct stride, photograph encode, three-actual caller screen | 11,400.49 | 11,442.54 | 809,225 | Inconclusive; no regression/equivalence claim |

Direct stride's short decode point improved133.71µs/2.09%, but independent
confirmation does not separate raw or reference-normalized confidence in either
filtered/all-actual view. Its raw candidate/original interval is.989399–1.002357;
normalized interval.980807–1.009499. No unchanged measurement is repeated for a
favorable result. Compact adaptation's flat result does not identify copying as
causal. The direct variant's modest consolidation does not justify changing five
production files and all eight encoder boundaries without a demonstrated benefit.

Measurements use the frozen513×259controls, actual standalone/default-context
workers, quiet pinned CPU0 and existing complete input-delivery, parsing,
reconstruction/filter/color or normalization/encoding, output ownership and
disposal boundaries. Setup/JIT are excluded; native contexts/private scratch are
retained as previously labeled. Exact531,468Bdecoded frames and17,688Bencoded
packets pass the original checks. Native allocation and complete peak live
capacity are unmeasured; new owned heap capacity and retained scratch are zero.
Stack storage stays under existing8KiBdecoder/16KiBencoder coarse admission.

Windows/Linux focused normal/scalar checks pass for both immutable stages:
compact47internal/61public, direct95internal/115public per mode. Independent
per-pixel border/default geometry, native YUV/RGBA fixtures, alias/padding/stride
footprints and frozen encoder packets remain the correctness boundaries. One
initial short selector ran a different case: discard that run. A single dynamic
selector reuses the existing typed driver with only job/count changes. A completed
correct run's receipt collision is recovered without remeasurement. Both diagnostics
and all valid/adverse points are retained.

Source, main/embedded assemblies, public/differential bindings and VM manifest
restore byte for byte to8baebd84. The actual private package remains
`64c927a93659d2cf6f18555ab2beae7e54f5ce495d8d1e479748d30aa6458f8f`;
existing5,735Windows/Linux full/live, affected ISA and SAME-package/all-three-example
proof remains valid. No rollback build, repeated broad suite or package proof is
needed. Current benchmark costs remain unchanged; this report is historical
evidence and is not a renderer dependency.

[Evidence](vp8-shared-strided-prediction-evidence.json) records source identities,
both preparations, comparisons and exact restoration. Mirrored/member-verified
raw `vp8-shared-strided-prediction-experiment.tar.xz`:2,031,736B/196members,
SHA256`08e4057afe897c3203d8bda76a65ef890f650ea58b894efa964ce8987d5f1f6d`.
Unchanged inputs are pinned through retained archive dependencies. Full282-path
parity remains open; publication is postponed.
