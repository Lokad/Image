# Warm RGBA16 JPEG420 entropy checks

**No production change is accepted.** A fresh quiet screen leaves the original
encoder at **2.046× FFmpeg**, upper **3.240×**, allocating **527,306 B/op**.
Three entropy/output candidates fail to establish useful parity headroom and
are discarded. No twelve-actual confirmation repeats these screens. The
validated implementation is restored; JPEG420 and uniform warm parity remain
open.

| Screen | Lokad µs | Ratio / upper | Lokad allocated B/op | Decision |
| --- | ---: | ---: | ---: | --- |
| Validated implementation | 3,775.72 | 2.046 / 3.240 | 527,306 | Open |
| Entropy Write inlining | 3,665.49 | 1.994 / 2.201 | 527,306 | Reject: only 2.9% gain, insufficient headroom |
| Cached stock MemoryStream byte path | 3,816.62 | 2.036 / 4.145 | 527,322 | Reject: no useful gain |
| Batched Huffman code and amplitude | 3,972.19 | 2.160 / 3.780 | 527,322 | Reject: slower |

Each screen retains all three actual results and passes both exclusive VM
quietness guards. They use the same existing original 513×259 RGBA16 raster,
background RGB 0x204060, managed quality 69 and native qscale 5. Complete
16-bit blending/normalization, conversion, encode, independent owned output
acquisition and release remain timed on one pinned CPU/thread in Release.
Startup, I/O, IPC and checks are excluded. Native allocations are unmeasured.

Every candidate preserves the exact original managed/native packets of
**147,730 / 52,117 bytes**, source-quality gap 0.105942 dB, normalized samples,
alpha, geometry, decoder agreement, source and held owners. Fixed versus
optimal Huffman coding remains explicit; this is source-quality timing,
not compression efficiency parity.

One six-second current-source profile attributes 19.6% of samples to entropy
Write, 5.0% to byte output admission and 2.9% to span output admission. The
encoder leaf accounts for 38.4%, including inlined work. Inlining changes only
one flag. Cached dispatch preserves all other flags and bodies except the
bounded stream constructor/byte path. Symbol batching preserves all 601
existing flags and the other 599 bodies, with two added methods and routing
changes. Its 36 focused normal/scalar checks cover all 131,070 canonical signed
amplitudes, pending bits, stuffing, restart/padding, tight limits, callbacks,
capacity boundaries and cancellation after a partial write. Exact correctness
does not justify retaining a slower candidate.

Restoration reuses the already validated actual package's **2,749 tests per OS**
and all three examples through the same package on both OSes. Fresh builds on
both OSes prove all **601 original method bodies and flags** match that package,
alongside the existing 30 original JPEG controls. No rejected method, flag,
cache field or test requiring the rejected API remains in production.

The [numeric evidence](warm-jpeg42016-entropy-rejected-evidence.json) preserves
the profile, four screens, allocations, candidate deltas and restoration proofs.
All four actual child files, source, raw evidence and each actual package were
frozen before the next preparation or restoration. Earlier
[component-row evidence](WARM-JPEG42016-CHROMA-ROW.md) remains intact.

The next investigation targets a larger algorithmic opportunity: independently
implemented JPEG Huffman coding. The inspected pinned FFmpeg encoder caches
entropy-symbol information while constructing optimal length-limited tables.
This is insight only; no upstream sources or tables are copied. Any Lokad.Image
candidate must preserve transform coefficients, decoded samples and the quality
anchor, explicitly document changed packet bytes, and admit its workspace.
