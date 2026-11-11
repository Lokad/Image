# Ordered word bicubic horizontal windows

The existing bounded RGBA16 reverse-mixed control saves **49.41 µs / 5.25% raw /
6.36% reference-normalized**. Height-preserving reduction saves **34.44 µs /
6.53% raw / 15.36% normalized**. Filtered, all-actual and normalized intervals
separate for both. Native clocks move +1.19%/+10.44%, so the two gain views are
reported separately. Both independently pass; stop these controls.

| Existing public Resize, input+output+4 KiB | Lokad µs | zimg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| TightCross16 | 892.36 | 900.40 | 0.991 / 0.999 | 114 | pass |
| TightHorizontal16 | 493.16 | 640.62 | 0.770 / 0.774 | 113 | pass |

The original 513×259 RGBA16 input produces 256×519/259 output. Timing includes
preparation, source processing, complete filtering/repair, independently owned
output acquisition and disposal. The explicitly labeled native zimg worker keeps
its graph/scratch prepared. This retains the existing boundary, not a fresh-graph
comparison. Original inputs, settings, native bounds and exact managed hashes
remain frozen. No new benchmark fixture, grid, selector or reference worker.

CrossWord now uses the existing ordered eight-tap/+2 helper for eligible pairs:
ten decoded pixels feed two original eight-tap accumulations. Eligibility is
prepared once for full-row geometry or each strip; 124 of 128 pairs qualify on
the existing raster. Clipped, unequal and nonconsecutive windows keep their
original loop. Logical load/MAC counts are not phase timing or gain forecasts.

A preparation overload supports full-row int indexes, including offsets above
255. It stores proof in dead record slot nine, preserving every live tap and
full-capacity records; checks all ten decoded values are live; rejects negative
starts; and keeps cancellation bounded while preparing wide geometry. Original
byte-index preparation and all four/eight/sixteen-tap helper bodies are unchanged.
Removing only this overload and CrossWord's preparation/dispatch reconstructs
both whole previous source files. All original horizontal weights/normalizers,
source decoding, vertical arithmetic, certificates, original/tiny-alpha repair,
NoInlining retirement, admission and owners remain verbatim. No reassociation,
FMA, padding or numerical-policy change. Twelve metadata controls supplement
existing exact-lane, frozen/rational, odd-edge, low-alpha and ownership checks.

**Allocation stays 114/113 B/op**, with no added heap/stack-array capacity,
owner, pool or retained scratch. The existing one-row/five-row designs keep
their 58,400/91,168 B views of already-owned output and 4 KiB admission. Explicit
strip arrays remain 2,352/3,440 B; output payloads 1,062,912/530,432 B stay charged
through independent ownership. Default pool/governed bypass are unchanged.
Physical/native/JIT peaks remain unmeasured. Refusal stays terminal, cancellation
and failure clean up, and inputs and the borrowed shared root remain caller-owned.

Three-actual screens remain diagnostics. The table uses independent twelve-actual
confirmations on the same immutable build, with CPU0, tiering off/nonconcurrent GC,
idle-v2 before/after and all actuals/native brackets retained. No counterchecks or
favorable repeats. After both costs reconcile: **6,321 full/live checks per OS**,
412 internal/789 public resize checks in normal/scalar/AVX2off/AVX512off modes,
including all three named storage suites. SAME actual Release-package consumer
and all three examples pass Windows/Linux, including both original frozen word
frames, refusal/cancellation/recovery and independent survivors. One core/embedded
build per OS; zero public/harness/native/fixture builds or compile failures.
Other kernels/codec/text paths reuse source-valid evidence.

The private 0.2.1 package `4a66c486a7a528a5712f891523b0be5a71164b88dc254d4a5d017db16892ab90` contains timed DLL
`d93309af02ed6cc138546c102adf1d478d1ae46c0b49876a29f142306fbadb50`. Published releases remain immutable; publication
is postponed. [Evidence](bicubic-ordered-horizontal-windows-evidence.json) pins
producers, current costs, storage, scope and qualification. Mirrored/member-verified
raw `bicubic-ordered-horizontal-windows.tar.xz`: 3,652,588 B/140 members, SHA256
`fd5862c1296eafc03ace7ae19241ebd791a06a05f260cd9e0798d7274625bb47`. Its dependency retains the original [identity-height
design](BICUBIC-IDENTITY-HEIGHT.md), adverse/inconclusive experiments and original
fixture/reference identities. All 289 required paths remain in scope.
