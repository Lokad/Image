# Lokad.Image benchmark state

Updated 2026-10-24. **Declared encoder and decoder byte controls are qualified**
against pinned FFmpeg builds/settings; warm single-CPU parity remains open.
Only explicitly approved encoder-attribution metadata may differ; see
[codec compatibility](codec-parity.md).

The default managed JPEG Reference encoder matches ten frozen packets across
grayscale/444/422/420,8/16-bit sources and declared color/grayscale restarts.
Color uses only the approved COM replacement; grayscale is raw-exact. Required
image/caller/sync/async/ImageBudget boundaries are qualified. Superseded
Original/CompactColor experiments and their public switches have been removed.

All four production [managed PNG policies](../benchmarks/PNG-MANAGED-POLICIES.md)
match14 frozen complete packets raw-exact: all eight Optimal color/depth controls
plus RGBA8/16 stored, fast and smallest. The17 existing delivery/profile rows
are exact, including checksums/framing. Both platforms pass4,807 Release checks,
69 affected PNG scalar checks and the SAME actual private0.2.0 package with all
three examples. Four current RGBA8 encoder anchors have guarded packet-exact cost
screens. JPEG has independently confirmed improvements from bounded MCU conversion,
integer SIMD transforms/quantization, bounded chroma rows and coefficient replay.
The caller-root RGBA16 JPEG420 control now confirms 2.495× after exact
entropy-record replay, using 10.7% less elapsed than its preceding confirmation
and 804,019 B/op (+48). Admitted peak increases by 128 B to 1,704,856 B;
data capacity is unchanged on this control. Nongoverned RGBA8/RGBA16 have current
2.274×/2.439× screens and 372,856/372,859 B/op. Each platform passes 5,107 checks
and validates the same actual private package;
[entropy-record evidence](../benchmarks/WARM-JPEG-ENTROPY-RECORDS.md).
Immutable workers were reused. Other affected color-JPEG timings stay historical.
The grayscale 0.962× claim came from the replaced 58,823-byte encoder. Current
raw-exact 58,910-byte RGBA-gray encoding confirms1.726× after entropy replay,
using35.3% less elapsed than its preceding confirmation and154,883B/op. Caller-gray
has a current1.886× screen and154,886B/op. The48B owner adds bounded record storage;
fullcapacity/replacement peaks remain governed. Each platform passes5,111checks
and the same actual package; [replay evidence](../benchmarks/WARM-JPEG-GRAY-ENTROPY-RECORDS.md).
The reference is approved standalone SIMDlibjpeg-turbo for both complete boundaries.
VP8/colorJPEG/VP8L now lead the measured encoder gaps; older affected gray timings
stay historical.
Other codecs, decoders, editing and text reuse
unchanged confirmations where their production paths remain unchanged.
VP8 now confirms a 2.412× gap after exact cache-history reuse, using
3.6% less elapsed than its preceding confirmation, with 24,329,736 B/op
(+248,784) and exact 98,590 B packets. Both platforms pass 5,115 checks and the
same actual package; [cache-history evidence](../benchmarks/WARM-VP8-CACHE-HISTORY.md).
Affected VP8L has a current 2.442× screen/upper3.019, 48,184,648 B/op (+124,392)
and the same exact 183,812 B packet; no independent lossless speedup claim.
The faster path applies to noncancellable paired selection; other checks remain.
Color JPEG and VP8L now lead the measured encoder gaps.
No image/settings controls were added.

Other replaced encoder observations remain historical.
All17 historical BCL PNG timing rows below are superseded, as are the earlier
JPEG encoder producers. No new timing or image/settings controls were added.

The production VP8 pipeline now matches20 pinned complete packet identities
across22 source/settings rows, including all efforts and default-method8/16-bit
normalization. Photograph/diagram packets are exactly17,688/7,100B at the pinned
Q75/method4 settings. The separate large-header control also passes. Shared
admission, exact normalized alpha, stream ownership and direct async delivery
are qualified; [VP8 scope](../benchmarks/VP8-INPUT-STAGE.md) records details.
All seven prior VP8 time/allocation rows are superseded. In particular, the old
32M-work timing describes a success on the prior algorithm; current encoding
must fail that resource boundary, so it is not a new performance pass. No CPU
measurement or retuning accompanies byte qualification.

The managed [VP8L encoder](../benchmarks/VP8L-COMPLETE-PACKET.md) now reproduces
all19 selected packet identities through25 existing settings checks, plus two
additional forced spatial/box candidates (21 distinct complete packets). Public sync/async/ImageBudget
boundaries retain the asymmetric packet at8/16bits. The photograph/diagram controls
match160,032/7,234B exactly. Source-only proofs are retained as earlier evidence;
public production no longer uses the superseded full-image encoder.
Prepared transforms/tokens/maps/canonical codes share one borrowed admission root.
One normalized working image releases before emission; counting/write reuse prepared
state without an encoded-frame buffer. Refusal/cancellation and output ownership
pass. Both platforms pass1,855 internal+2,070 public+940 live FFmpeg checks and27
scalar public/axis checks, with one immutable worker set. A test-only extension
against the unchanged core passes26 internal/27 public packet checks and27 scalar
packet checks, including the two16383 axes. The SAME private Release
package and all3usage examples pass on both. The native encoder refuses16384;
those axis cases remain functional extensions without an encoder-byte claim.
Six affected encoder time/allocation rows below describe the prior producer and
require new costs later. Frozen decoder inputs and unchanged code retain evidence.

Current JPEG decoding uses the pinned integer inverse and rounded-chroma color
policy. Thirty frozen grayscale/4:4:4/4:2:2/4:2:0 controls and the declared
public stream/caller/orientation boundaries have exact pixel gates. Named CMYK/YCCK
controls also require exact RGB/grayscale, including black-before-color precision.
Named4:1:1/4:1:0 controls retain fractional chroma through final RGB rounding.
Their historical timing workers include additional centered normalization; those
unchanged references remain historical and need equivalent current outputs before
new timing qualification. No baseline or performance pass is inferred from the
new byte checks on the same frozen513x259 packets.
Direct-RGB422/420 caller grayscale shares integer BT.601 precision after normalized
8-bit components; the existing RGBA/RGB reference settings and hashes remain.
Earlier JPEG
decode timings and allocations describe the replaced producer and are marked
**Prior production; reconfirm**. No new decoder timing/allocation run is claimed.

VP8 decoder RGB now uses the exact integer normalization on164 frozen controls.
One current fresh-Stream screen uses the same98,590-byte input, exact normalized
pixels and unchanged native baseline. Both quiet guards pass; it measures
10,523.95µs versus5,981.68µs,1,966,591B/op. This is a short1.759× gap observation,
with no independent confirmation or speedup claim. The other four VP8 decode
rows still describe the replaced renderer.

The initial PNG/JPEG/VP8L encoder adoption had no timing/allocation runs; four existing anchors now have current guarded screens. Unchanged paths
retain evidence on their recorded frozen inputs; new default-encoded fixtures do
not silently replace those inputs. CPU tuning/independent confirmation resumes
after exact codec work. The full warm single-CPU objective remains independent
conservative upper<=1 for every material required path. Published0.1.0 is immutable;
replacement readiness remains open.

The [finite checklist](../benchmarks/WARM-COVERAGE.md) contains 257 named cases:
240 numeric and 17 unmeasured. Tested-producer rows contain **45 passes,
122 gaps, 1 confidence-open case, 2 short cost
screens and 70 superseded encoder rows**. The latter predate
JPEG decision/adaptive work or the managed PNG/VP8L encoder replacements; current packet evidence is
reconciled and new costs are deferred while compression gates remain open.
Their stored values are historical, not demonstrated current gaps. Another 30 marked
decode/inspection rows await confirmation after typed source-metadata changes;
the PNG metadata reference first needs equivalent typed fields. Unaffected evidence
is reused; the scope of every tested producer remains in its linked report.

Dedicated AMD EPYC 9V45; Ubuntu 24.04.4; .NET 10.0.8/SDK 10.0.204; FFmpeg n8.1.
One pinned CPU and codec/conversion/filter thread, normal SIMD, tiering off,
workstation nonconcurrent GC. Times cover complete public operations, admission,
normalization, fresh output ownership and release. Source preparation, startup,
IPC, I/O, hashing and validation are excluded. Fresh-stream rows include wrapper,
read/copy/EOF and release costs. Context/private scratch reuse is allowed;
completed decoded or encoded outputs are not cached.

Ratio uses the faster native pre/post mean. Upper is the larger selected/all-actual
bound against the faster native bracket's lower confidence bound. Independent
jobs retain 12 actuals; short screens retain 3, with both idle-v2 guards. **B/op** is
managed BenchmarkDotNet/raw-GC allocation rounded up. Native allocation is
**unmeasured**, not zero. Compression gains may cost more CPU; no cost-only screen
is a CPU-parity confirmation.

**Decode and metadata**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [PNG RGBA8, two retained outputs / caller-owned memory root](../benchmarks/WARM-DEFLATE-MATERIALIZED-CURSOR.md) | 3,604.48 | 2,850.06 | 1.265 / 1.304 | 2,574,671 | Gap (short; qualified); metadata timing open |
| [PNG DecodeAsync, fresh nonseekable Stream](../benchmarks/WARM-DEFLATE-MATERIALIZED-CURSOR.md) | 1,574.38 | 1,414.79 | 1.113 / 1.176 | 12,784 | Gap (short; qualified); metadata timing open |
| [PNG IdentifyAsync, fresh nonseekable Stream¶](../benchmarks/WARM-PNG-ASYNC-IDENTIFY.md) | 24.10 | 19.34 | 1.246 / 1.571 | 8,408 | Gap (short; qualified); metadata timing open |
| [PNG Identify, fresh Stream¶](../benchmarks/WARM-PNG-STREAM-IDENTIFY.md) | 22.94 | 15.87 | 1.445 / 1.459 | 184 | Gap (independent) |
| [WebP Identify, resident span (parser)](../benchmarks/WARM-WEBP-IDENTIFY.md) | 0.02 | 4.54 | 0.004 / 0.004 | 88 | Parity confirmed |
| [JPEG Identify, resident span‖](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 0.59 | 1.21 | 0.488 / 0.494 | 88 | Parity confirmed |
| [PNG Identify, resident span¶](../benchmarks/WARM-PNG-IDENTIFY.md) | 16.09 | 13.04 | 1.234 / 1.238 | 88 | Gap (independent) |
| [PNG palette4/full tRNS Adam7](../benchmarks/WARM-PNG-PALETTE4-ADAM7.md) | 1,480.35 | 1,135.35 | 1.304 / 1.387 | 2,592 | Gap (independent) |
| [PNG8 Stream, orient/crop/nearest fit](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,044.52 | 1,476.97 | 1.384 / 1.613 | 701,312 | Gap (short; qualified) |
| [PNG16 Stream, orient/crop/nearest fit](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 3,355.49 | 2,946.76 | 1.139 / 1.213 | 1,401,314 | Gap (short; qualified) |
| [PNG RGBA8, fresh seekable Stream](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 1,918.97 | 1,393.95 | 1.377 / 1.583 | 4,552 | Gap (short; qualified) |
| [PNG RGBA16, fresh seekable Stream](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 3,054.27 | 3,091.94 | 0.988 / 0.991 | 8,648 | Parity confirmed |
| [JPEG420 restart, fresh seekable Stream](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,329.08 | 996.80 | 1.333 / 1.411 | 761,840 | Prior production; reconfirm; metadata timing open |
| [WebP VP8 + alpha, fresh seekable Stream](../benchmarks/WARM-CURRENT-VP8-SAMPLES.md) | 10,523.95 | 5,981.68 | 1.759 / 1.903 | 1,966,591 | Gap (short; qualified) |
| [WebP VP8L, fresh seekable Stream](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 4,320.01 | 3,018.37 | 1.431 / 1.494 | 1,216,276 | Gap (short; qualified) |
| [PNG RGBA8, None](../benchmarks/WARM-CURRENT-RESIDENT-PNG.md) | 1,575.30 | 1,388.57 | 1.134 / 1.330 | 4,480 | Gap (short; qualified); metadata timing open |
| [PNG RGBA8, resident / ImageBudget quota](../benchmarks/WARM-SHARED-BUDGET.md) | 2,004.67 | 1,392.79 | 1.439 / 1.482 | 4,561 | Gap (short; qualified) |
| [PNG RGBA8, None / live cancelable token](../benchmarks/WARM-PNG-CANCEL-BLOCKS.md) | 1,732.37 | 1,390.42 | 1.246 / 1.320 | 4,456 | Gap (short; qualified) |
| [PNG RGBA8, None / exact working limit](../benchmarks/WARM-PNG-STREAMING-ROWS.md) | 2,028.73 | 1,395.91 | 1.453 / 1.597 | 68,200 | Gap (short; qualified) |
| [PNG RGBA8, None / exact limit + live cancelable token](../benchmarks/WARM-PNG-STREAMING-ROWS.md) | 2,067.97 | 1,397.05 | 1.480 / 1.826 | 68,200 | Gap (short; qualified) |
| [PNG RGBA8, stored Deflate / None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 150.38 | 136.49 | 1.102 / 1.114 | 4,456 | Gap (independent) |
| [PNG RGBA16, stored Deflate / None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 322.73 | 296.18 | 1.090 / 1.096 | 8,552 | Gap (independent) |
| [PNG RGBA16, None](../benchmarks/WARM-CURRENT-RESIDENT-PNG.md) | 3,015.32 | 2,901.23 | 1.039 / 1.064 | 8,576 | Gap (short; qualified); metadata timing open |
| [PNG RGBA16, None / exact working limit](../benchmarks/WARM-PNG-STREAMING-ROWS.md) | 3,103.58 | 2,902.28 | 1.069 / 1.242 | 70,248 | Gap (short; qualified) |
| [PNG RGBA16, None / live cancelable token](../benchmarks/WARM-PNG-CANCEL-BLOCKS.md) | 3,068.88 | 2,897.86 | 1.059 / 1.108 | 8,552 | Gap (short; qualified) |
| [PNG RGBA16, None / exact limit + live cancelable token](../benchmarks/WARM-PNG-STREAMING-ROWS.md) | 3,120.43 | 2,904.38 | 1.074 / 1.145 | 70,248 | Gap (short; qualified) |
| [PNG RGBA8, Paeth](../benchmarks/WARM-PAETH-SIX-CHANNEL.md) | 3,149.31 | 2,847.74 | 1.106 / 1.128 | 4,456 | Gap (short; qualified) |
| [PNG RGBA8, Up](../benchmarks/WARM-PNG-UP-SIMD.md) | 1,484.65 | 1,509.16 | 0.984 / 0.989 | 4,456 | Parity confirmed |
| [PNG RGBA16, Up](../benchmarks/WARM-PNG-UP-SIMD.md) | 1,902.38 | 1,911.94 | 0.995 / 0.997 | 8,552 | Parity confirmed |
| [PNG RGBA8, Sub](../benchmarks/WARM-PNG-SUB-PREFIX.md) | 584.20 | 686.53 | 0.851 / 0.855 | 4,456 | Parity confirmed |
| [PNG RGBA16, Sub](../benchmarks/WARM-PNG-SUB16.md) | 357.99 | 743.02 | 0.482 / 0.493 | 8,552 | Parity confirmed |
| [PNG RGBA8, Average](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,380.32 | 1,850.31 | 1.286 / 1.298 | 4,456 | Gap (independent) |
| [PNG RGBA16, Average](../benchmarks/WARM-PNG-AVERAGE16.md) | 5,894.17 | 4,384.37 | 1.344 / 1.357 | 8,552 | Gap (independent) |
| [PNG RGBA16, Paeth](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 5,401.14 | 5,511.74 | 0.980 / 0.990 | 8,552 | Parity confirmed |
| [PNG RGB8, Paeth](../benchmarks/WARM-PAETH-SIX-CHANNEL.md) | 3,140.69 | 2,632.59 | 1.193 / 1.241 | 3,432 | Gap (short; qualified) |
| [PNG RGB16, Paeth](../benchmarks/WARM-PAETH-SIX-CHANNEL.md) | 4,290.59 | 4,472.22 | 0.959 / 0.969 | 6,504 | Parity confirmed |
| [PNG gray8, Paeth](../benchmarks/WARM-PNG-GRAY8-PAETH.md) | 1,038.34 | 1,029.60 | 1.008 / 1.014 | 1,384 | Gap (independent) |
| [PNG gray-alpha8, Paeth](../benchmarks/WARM-PNG-GRAY-ALPHA8-PAETH.md) | 2,051.11 | 2,016.66 | 1.017 / 1.068 | 2,408 | Gap (independent) |
| [PNG RGBA16, Adam7](../benchmarks/WARM-CURRENT-PNG-ADAM7.md) | 5,393.13 | 3,917.00 | 1.377 / 1.381 | 23,600 | Gap (independent) |
| [PNG gray1 → RGBA8, Adam7](../benchmarks/WARM-CURRENT-PNG-ADAM7.md) | 1,512.17 | 1,150.81 | 1.314 / 1.320 | 1,424 | Gap (independent) |
| [PNG palette8 + tRNS, Adam7](../benchmarks/WARM-PNG-PALETTE8-ADAM7.md) | 1,710.03 | 1,465.40 | 1.167 / 1.177 | 4,960 | Gap (independent) |
| [PNG RGB16, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,297.09 | 2,227.02 | 1.031 / 1.034 | 6,504 | Gap (independent) |
| [PNG gray8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 357.47 | 363.27 | 0.984 / 0.993 | 1,384 | Parity confirmed |
| [PNG gray8 + tRNS key127, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 357.50 | 758.36 | 0.471 / 0.474 | 1,416 | Parity confirmed |
| [PNG gray16 + tRNS key32639, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,970.49 | 2,055.01 | 1.445 / 1.450 | 2,440 | Gap (independent) |
| [PNG gray-alpha8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 845.98 | 1,156.04 | 0.732 / 0.733 | 2,408 | Parity confirmed |
| [PNG RGB8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 1,293.31 | 1,523.89 | 0.849 / 0.853 | 3,432 | Parity confirmed |
| [PNG RGB8 + tRNS key(48,139,231), None](../benchmarks/WARM-PNG-RGB-TRANSPARENCY-CONFIRM.md) | 1,302.04 | 1,286.75 | 1.012 / 1.014 | 3,464 | Gap (independent) |
| [PNG RGB16 + tRNS key(12347,35806,59265), None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 2,316.17 | 3,182.24 | 0.728 / 0.732 | 6,536 | Parity confirmed |
| [PNG gray16, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 714.81 | 957.95 | 0.746 / 0.751 | 2,408 | Parity confirmed |
| [PNG gray-alpha16, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 1,563.09 | 1,795.59 | 0.871 / 0.874 | 4,456 | Parity confirmed |
| [PNG byte indices / sixteen-entry palette + tRNS, None](../benchmarks/WARM-PNG-SHORT-PALETTE.md) | 144.22 | 109.47 | 1.317 / 1.327 | 1,496 | Gap (independent) |
| [PNG palette8 + tRNS, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 349.92 | 364.06 | 0.961 / 0.970 | 2,456 | Parity confirmed |
| [PNG palette8 + two-entry tRNS, None](../benchmarks/WARM-PNG-PARTIAL-ALPHA.md) | 360.10 | 365.80 | 0.984 / 0.992 | 2,208 | Parity confirmed |
| [PNG palette4 + tRNS → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 109.81 | 140.53 | 0.781 / 0.799 | 984 | Parity confirmed |
| [PNG palette2 + tRNS → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 66.42 | 97.53 | 0.681 / 0.686 | 688 | Parity confirmed |
| [PNG palette1 + tRNS → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 54.06 | 88.92 | 0.608 / 0.611 | 552 | Parity confirmed |
| [PNG opaque gray1 → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 59.11 | 303.42 | 0.195 / 0.199 | 488 | Parity confirmed |
| [PNG gray1 + tRNS key1 → RGBA8, exact timed FFmpeg color-key](../benchmarks/WARM-PNG-GRAY1-TRANSPARENCY.md) | 64.68 | 684.86 | 0.094 / 0.098 | 520 | Parity confirmed |
| [PNG gray2 + tRNS key0 → RGBA8, None](../benchmarks/WARM-PNG-PACKED-TRANSPARENCY.md) | 71.49 | 497.90 | 0.144 / 0.145 | 648 | Parity confirmed |
| [PNG gray1/key1 and gray2/key1, unfiltered native](../benchmarks/WARM-PNG-PACKED-TRANSPARENCY.md) | — | — | — | — | Unscored: native alpha differs |
| [PNG opaque gray2 → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 71.73 | 106.38 | 0.674 / 0.679 | 616 | Parity confirmed |
| [PNG opaque gray4 → RGBA8, None](../benchmarks/WARM-CURRENT-PNG-DECODE.md) | 116.09 | 169.54 | 0.685 / 0.698 | 872 | Parity confirmed |
| [JPEG baseline gray](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 927.74 | 681.93 | 1.360 / 1.455 | 672,210 | Prior production; reconfirm; metadata timing open |
| [JPEG gray → caller GRAY8](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 817.85 | 646.73 | 1.265 / 1.308 | 273,436 | Prior production; reconfirm; metadata timing open |
| [JPEG untagged → caller CMYK8](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 467.59 | 330.92 | 1.413 / 1.474 | 1,635,495 | Prior production; reconfirm; metadata timing open |
| [JPEG 4:2:0 → caller RGB24](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,075.13 | 1,160.67 | 0.926 / 0.931 | 624,524 | Prior production; reconfirm; metadata timing open |
| [JPEG Adobe CMYK, four full-resolution planes](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 469.48 | 377.61 | 1.243 / 1.306 | 1,085,694 | Prior production; reconfirm; metadata timing open |
| [JPEG Adobe CMYK, one AC control](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 547.30 | 389.60 | 1.405 / 1.469 | 1,085,688 | Prior production; reconfirm; metadata timing open |
| [JPEG Adobe CMYK, general transform control](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 3,281.94 | 3,111.92 | 1.055 / 1.105 | 1,085,546 | Prior production; reconfirm; metadata timing open |
| [JPEG Adobe CMYK, two-dimensional sparse control](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 642.99 | 440.11 | 1.461 / 1.519 | 1,085,689 | Prior production; reconfirm; metadata timing open |
| [JPEG YCCK420, full-resolution black](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 786.35 | 708.51 | 1.110 / 1.178 | 1,470,280 | Prior production; reconfirm; metadata timing open |
| [JPEG YCCK420 → caller RGB24](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 865.17 | 604.01 | 1.432 / 1.476 | 1,337,511 | Prior production; reconfirm; metadata timing open |
| [JPEG H4V1 / 4:1:1 → owned RGBA8](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 588.74 | 456.30 | 1.290 / 1.347 | 751,303 | Prior production; reconfirm; metadata timing open |
| [JPEG H4V2 / 4:1:0 → owned RGBA8](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 591.35 | 477.47 | 1.238 / 1.282 | 720,844 | Prior production; reconfirm; metadata timing open |
| [JPEG baseline 4:2:0](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,159.27 | 1,008.12 | 1.150 / 1.246 | 757,369 | Prior production; reconfirm; metadata timing open |
| [JPEG baseline 4:2:0, row restart markers](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,321.56 | 1,013.22 | 1.304 / 1.462 | 761,744 | Prior production; reconfirm; metadata timing open |
| [JPEG baseline 4:2:2](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,469.44 | 1,348.59 | 1.090 / 1.194 | 829,172 | Prior production; reconfirm; metadata timing open |
| [JPEG baseline 4:4:4, owned input](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 2,391.79 | 1,899.20 | 1.259 / 1.304 | 948,060 | Prior production; reconfirm; metadata timing open |
| [JPEG baseline 4:4:4, native input](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 2,222.98 | 1,720.03 | 1.292 / 1.361 | 966,341 | Prior production; reconfirm; metadata timing open |
| [JPEG baseline direct RGB, full resolution](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 2,093.21 | 1,451.23 | 1.442 / 1.524 | 966,341 | Prior production; reconfirm; metadata timing open |
| [JPEG direct RGB 4:2:2, timed centered correction](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,401.73 | 1,094.82 | 1.280 / 1.405 | 829,172 | Prior production; reconfirm; metadata timing open |
| [JPEG direct RGB 4:2:0, timed component-plane interpretation](../benchmarks/WARM-JPEG-BUFFERED-EOB.md) | 1,020.52 | 743.18 | 1.373 / 1.414 | 757,378 | Prior production; reconfirm; metadata timing open |
| [JPEG baseline 4:4:4, native input / live cancelable token](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,392.14 | 1,720.11 | 1.391 / 1.482 | 966,317 | Prior production; reconfirm |
| [JPEG progressive gray](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,647.80 | 1,712.34 | 1.546 / 1.634 | 683,138 | Prior production; reconfirm |
| [JPEG progressive 4:2:0](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 2,438.11 | 1,947.56 | 1.252 / 1.446 | 775,336 | Prior production; reconfirm |
| [JPEG progressive 4:4:4 / live cancelable token](../benchmarks/WARM-JPEG-411-SIMD.md) | 5,069.78 | 3,349.49 | 1.514 / 1.611 | 984,298 | Prior production; reconfirm |
| [JPEG progressive 4:2:2](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 3,071.46 | 2,238.43 | 1.372 / 1.508 | 847,162 | Prior production; reconfirm |
| [JPEG progressive 4:4:4](../benchmarks/WARM-INPUT-RECONCILIATION.md) | 5,037.72 | 3,361.01 | 1.499 / 1.715 | 984,298 | Prior production; reconfirm |
| [WebP VP8 + alpha](../benchmarks/WARM-VP8-CANCELLABLE-BLOCK.md) | 8,353.50 | 5,968.68 | 1.400 / 1.436 | 1,966,648 | Prior production; reconfirm; metadata timing open |
| [WebP VP8 opaque, simple luma filter](../benchmarks/WARM-VP8-CANCELLABLE-BLOCK.md) | 6,888.59 | 4,890.64 | 1.409 / 1.450 | 760,572 | Prior production; reconfirm; metadata timing open |
| [WebP VP8 opaque, live cancelable token](../benchmarks/WARM-VP8-CANCELLABLE-BLOCK.md) | 6,687.81 | 4,793.83 | 1.395 / 1.437 | 760,572 | Prior production; reconfirm; metadata timing open |
| [WebP VP8, opaque](../benchmarks/WARM-VP8-CANCELLABLE-BLOCK.md) | 6,638.33 | 4,795.62 | 1.384 / 1.443 | 760,572 | Prior production; reconfirm; metadata timing open |
| [WebP VP8L, sixteen-color indexed input](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 785.58 | 544.96 | 1.442 / 1.495 | 1,351,972 | Gap (short; qualified) |
| [WebP VP8L, literal/run input](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 2,269.12 | 2,157.09 | 1.052 / 1.074 | 1,074,326 | Gap (short; qualified) |
| [WebP VP8L, native input](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 4,576.04 | 3,022.99 | 1.514 / 1.615 | 1,216,180 | Gap (short; qualified) |
| [WebP VP8L, native input / live cancelable token](../benchmarks/WARM-VP8L-SIMD-PREDICTION.md) | 4,573.29 | 3,016.50 | 1.516 / 1.579 | 1,216,180 | Gap (short; qualified) |

**Encode**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [WebP EncodeWebpAsync, exact backend* / fresh nonseekable Stream](../benchmarks/WARM-VP8L-COLOR-COST.md) | 441,626.63 | 121,213.42 | 3.643 / 4.686 | 55,170,368 | Prior production; reconfirm |
| [JPEG EncodeJpegAsync, caller RGB420 / fresh nonseekable Stream](../benchmarks/WARM-JPEG-ASYNC-SAMPLES.md) | 2,076.73 | 1,628.02 | 1.276 / 1.430 | 232,614 | Prior production; reconfirm |
| [JPEG EncodeJpegAsync, RGBA8 / fresh nonseekable Stream / matched quality](../benchmarks/WARM-JPEG-ASYNC-ENCODE.md) | 3,982.03 | 3,849.79 | 1.034 / 1.158 | 494,254 | Prior production; reconfirm |
| [PNG EncodePngAsync, Optimal managed9/native6 / prior timing](../benchmarks/PNG-OPTIMAL-RGBA8.md) | 5,397.36 | 17,268.11 | 0.313 / 0.314 | 658,560 | Prior production; reconfirm |
| [PNG RGBA8 Optimal6/None](../benchmarks/WARM-CURRENT-ENCODER-COSTS.md) | 25,089.49 | 17,183.73 | 1.460 / 1.489 | 1,325,678 | Gap (short; qualified) |
| [PNG RGBA8, NoCompression / None](../benchmarks/WARM-PNG-STORED-CAPACITY.md) | 245.37 | 189.47 | 1.295 / 1.430 | 604,319 | Prior production; reconfirm |
| [PNG RGBA16, NoCompression / None](../benchmarks/WARM-PNG-STORED-CAPACITY.md) | 349.92 | 406.43 | 0.861 / 0.870 | 1,140,504 | Prior production; reconfirm |
| [PNG RGBA8, Fastest managed2/native1 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 3,053.16 | 4,355.98 | 0.701 / 0.704 | 527,694 | Prior production; reconfirm |
| [PNG RGBA16, Fastest managed2/native1 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 13,289.83 | 17,822.40 | 0.746 / 0.776 | 2,103,738 | Prior production; reconfirm |
| [PNG RGBA16, Fastest managed2/native1 / live cancelable token](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 12,846.40 | 17,513.32 | 0.734 / 0.743 | 2,103,738 | Prior production; reconfirm |
| [PNG RGBA16, Fastest managed2/native1 / forward-only output](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 12,790.89 | 17,465.07 | 0.732 / 0.740 | 2,103,770 | Prior production; reconfirm |
| [PNG RGBA16, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 14,604.45 | 20,095.18 | 0.727 / 0.731 | 2,103,624 | Prior production; reconfirm |
| [PNG RGBA8, SmallestSize9 / native9](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 14,486.25 | 17,324.40 | 0.836 / 0.843 | 527,564 | Prior production; reconfirm |
| [PNG RGBA16, SmallestSize managed9/native9 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 19,610.01 | 20,110.26 | 0.975 / 0.980 | 2,103,522 | Prior production; reconfirm |
| [PNG RGB8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-PACKING.md) | 8,712.51 | 9,507.55 | 0.916 / 0.922 | 527,032 | Prior production; reconfirm |
| [PNG RGB16, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 13,163.23 | 15,313.53 | 0.860 / 0.863 | 2,102,732 | Prior production; reconfirm |
| [PNG gray8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-PACKING.md) | 1,871.37 | 1,934.46 | 0.967 / 0.974 | 263,699 | Prior production; reconfirm |
| [PNG gray16, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 5,762.37 | 4,913.26 | 1.173 / 1.246 | 1,051,421 | Prior production; reconfirm |
| [PNG gray-alpha8, Optimal managed9/native6 / None](../benchmarks/WARM-PNG-BYTE-PACKING.md) | 4,174.10 | 4,289.79 | 0.973 / 0.980 | 264,179 | Prior production; reconfirm |
| [PNG gray-alpha16, level 6 / None](../benchmarks/WARM-PNG-CHUNK-CRC.md) | 9,426.43 | 10,150.04 | 0.929 / 0.933 | 1,052,317 | Prior production; reconfirm |
| [JPEG 4:4:4, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 3,894.69 | 3,771.76 | 1.033 / 1.115 | 559,418 | Prior production; reconfirm |
| [JPEG 4:4:4, forward-only / coded source histograms](../benchmarks/JPEG444-CODED-SOURCE-HISTOGRAMS.md) | 1,929,269.70 | 3,768.09 | 512.002 / 585.587 | 644,112 | Cost screen (CPU parity deferred) |
| [JPEG 4:4:4, live cancelable token / prior geometric Huffman](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 4,465.15 | 3,775.40 | 1.183 / 1.364 | 525,997 | Prior production; reconfirm |
| [JPEG 4:4:4 from RGBA16, adaptive Huffman / full normalization](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 3,980.90 | 3,855.74 | 1.032 / 1.135 | 556,284 | Prior production; reconfirm |
| [JPEG 4:2:2, adaptive Huffman / matched quality](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,579.93 | 2,354.66 | 1.096 / 1.227 | 294,116 | Prior production; reconfirm |
| [JPEG 4:2:2 from RGBA16, adaptive Huffman / full normalization](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,633.91 | 2,421.91 | 1.088 / 1.220 | 294,116 | Prior production; reconfirm |
| [JPEG RGBA8 4:2:0](../benchmarks/WARM-JPEG-ENTROPY-RECORDS.md) | 4,017.11 | 1,766.53 | 2.274 / 2.628 | 372,856 | Gap (short; qualified) |
| [JPEG RGBA8 4:2:0 / ImageBudget quota](../benchmarks/WARM-SHARED-BUDGET.md) | 2,026.04 | 1,769.43 | 1.145 / 1.272 | 297,781 | Prior production; reconfirm |
| [JPEG RGBA16 4:2:0 / caller IMemoryBudget](../benchmarks/WARM-JPEG-ENTROPY-RECORDS.md) | 4,584.96 | 1,837.34 | 2.495 / 2.510 | 804,019 | Gap (independent) |
| [JPEG 4:2:0, adaptive / row restart markers](../benchmarks/WARM-JPEG-VECTOR-AC.md) | 2,019.53 | 1,611.25 | 1.253 / 1.455 | 297,781 | Prior production; reconfirm |
| [JPEG 4:2:0, forward-only output / geometric AC Huffman](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 2,310.38 | 1,769.17 | 1.306 / 1.488 | 263,819 | Prior production; reconfirm |
| [JPEG 4:2:0 from RGBA16, forward-only output](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 2,393.98 | 1,839.60 | 1.301 / 1.415 | 263,819 | Prior production; reconfirm |
| [JPEG 4:2:0 from RGBA16, live cancelable token](../benchmarks/WARM-JPEG-FIXED-FIELDS.md) | 2,444.30 | 1,841.37 | 1.327 / 1.443 | 263,787 | Prior production; reconfirm |
| [JPEG 4:2:0 from RGBA16, adaptive Huffman](../benchmarks/WARM-JPEG-ENTROPY-RECORDS.md) | 4,488.07 | 1,839.99 | 2.439 / 2.727 | 372,859 | Gap (short; qualified) |
| [JPEG grayscale, standalone SIMD libjpeg-turbo](../benchmarks/WARM-JPEG-GRAY-ENTROPY-RECORDS.md) | 2,432.29 | 1,409.20 | 1.726 / 1.739 | 154,883 | Gap (independent) |
| [JPEG from caller gray samples, standalone SIMD libjpeg-turbo](../benchmarks/WARM-JPEG-GRAY-ENTROPY-RECORDS.md) | 2,038.89 | 1,080.83 | 1.886 / 2.154 | 154,886 | Gap (short; qualified) |
| [JPEG 4:2:0 from caller RGB / optional CompactColor](../benchmarks/WARM-JPEG-COMPACT-COLOR.md) | 1,774.21 | 1,576.12 | 1.126 / 1.362 | 162,110 | Gap (short; qualified) |
| [JPEG 4:2:0 from caller RGB / AC trellis and padding pruning](../benchmarks/JPEG420-AC-TRELLIS.md) | 68,162.15 | 1,579.24 | 43.161 / 50.354 | 170,648 | Cost screen (CPU parity deferred) |
| [WebP VP8, opaque‡](../benchmarks/WARM-VP8-NOISE-SPECTRUM.md) | 74,428.73 | 15,537.01 | 4.790 / 4.924 | 1,006,520 | Prior production; reconfirm |
| [WebP VP8, opaque from RGBA16‡](../benchmarks/WARM-VP8-NOISE-SPECTRUM.md) | 75,226.22 | 15,616.85 | 4.817 / 5.060 | 1,006,520 | Prior production; reconfirm |
| [VP8 RGBA8](../benchmarks/WARM-VP8-CACHE-HISTORY.md) | 129,327.57 | 53,621.51 | 2.412 / 2.430 | 24,329,736 | Gap (independent) |
| [WebP VP8 + alpha from RGBA16‡](../benchmarks/WARM-VP8-NOISE-SPECTRUM.md) | 83,863.51 | 53,727.26 | 1.561 / 1.652 | 1,537,304 | Prior production; reconfirm |
| [WebP VP8 + alpha‡, caller work budget32M](../benchmarks/WARM-VP8-HISTOGRAM384.md) | 19,502.47 | 53,435.71 | 0.365 / 0.433 | 2,036,564 | Prior production; reconfirm |
| [VP8L RGBA8 / libwebp exact=1](../benchmarks/WARM-VP8-CACHE-HISTORY.md) | 296,245.81 | 121,310.53 | 2.442 / 3.019 | 48,184,648 | Gap (short; qualified) |
| [WebP VP8L, opaque RGBA8 / exact backend*](../benchmarks/WARM-VP8L-COLOR-COST.md) | 481,885.87 | 69,570.25 | 6.927 / 8.486 | 53,104,872 | Prior production; reconfirm |
| [WebP VP8L from RGBA16, exact backend*](../benchmarks/WARM-VP8L-COLOR-COST.md) | 464,921.62 | 121,865.49 | 3.815 / 4.757 | 55,238,624 | Prior production; reconfirm |
| [WebP VP8L photograph RGBA8](../benchmarks/WARM-VP8L-COLOR-COST.md) | 612,338.64 | 83,533.61 | 7.330 / 8.250 | 67,414,112 | Prior production; reconfirm |
| [WebP VP8L text/diagram RGBA8](../benchmarks/WARM-VP8L-COLOR-COST.md) | 21,878.12 | 9,549.20 | 2.291 / 3.344 | 1,721,182 | Prior production; reconfirm |
| [WebP VP8 photograph RGBA8 / matched quality](../benchmarks/WARM-VP8-NOISE-SPECTRUM.md) | 35,062.49 | 9,726.09 | 3.605 / 3.820 | 590,152 | Prior production; reconfirm |
| [WebP VP8 text/diagram RGBA8 / matched quality](../benchmarks/WARM-VP8-NOISE-SPECTRUM.md) | 13,923.43 | 5,847.14 | 2.381 / 2.458 | 570,856 | Prior production; reconfirm |

**Editing**

| Operation / fixed profile | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| [Horizontal flip RGBA8, two live outputs / per image](../benchmarks/WARM-HORIZONTAL-FLIP512.md) | 9.56 | 9.08 | 1.053 / 1.142 | 128 | Gap (short; qualified) |
| [Nearest resize RGBA8](../benchmarks/WARM-COVERAGE-QUIET.md) | 58.67 | 252.66 | 0.232 / 0.241 | 132,294 | Parity confirmed |
| [Nearest upscale RGBA8](../benchmarks/WARM-NEAREST-UP8.md) | 935.05 | 1,006.00 | 0.929 / 0.955 | 2,133,091 | Parity confirmed |
| [Nearest upscale RGBA16](../benchmarks/WARM-NEAREST-UP16.md) | 1,071.13 | 1,019.06 | 1.051 / 1.095 | 4,265,431 | Gap (independent) |
| [Nearest resize RGBA16](../benchmarks/WARM-PREPARED-CONFIRM.md) | 105.69 | 170.58 | 0.620 / 0.629 | 264,466 | Parity confirmed |
| [Bilinear resize RGBA8](../benchmarks/WARM-BILINEAR-BYTE-ROW-CACHE.md) | 500.71 | 387.70 | 1.291 / 1.406 | 157,960 | Gap (short; qualified) |
| [Bilinear resize RGBA16†](../benchmarks/WARM-BILINEAR-WORD-CACHE.md) | 534.35 | 418.51 | 1.277 / 1.336 | 290,127 | Gap (short; qualified) |
| [Bilinear upscale RGBA8†](../benchmarks/WARM-BILINEAR-BYTE-FACTORIZATION.md) | 3,138.47 | 2,155.65 | 1.456 / 1.549 | 2,235,599 | Gap (short; qualified) |
| [Bilinear upscale RGBA16†](../benchmarks/WARM-CURRENT-BILINEAR.md) | 2,563.23 | 1,869.40 | 1.371 / 1.382 | 4,498,974 | Gap (independent) |
| [RGBA8 bicubic, single-pixel reduction](../benchmarks/WARM-BICUBIC-SINGLE-PIXEL.md) | 1,045.31 | 4,897.54 | 0.213 / 0.227 | 130 | Parity confirmed |
| [Bicubic resize RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 635.08 | 416.65 | 1.524 / 1.610 | 332,178 | Gap (short; qualified) |
| [Bicubic resize RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 729.05 | 571.10 | 1.277 / 1.715 | 464,343 | Gap (short; qualified) |
| [Bicubic upscale RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 3,919.91 | 3,036.45 | 1.291 / 1.357 | 3,244,646 | Gap (short; qualified) |
| [Bicubic upscale RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 4,258.22 | 2,871.12 | 1.483 / 1.690 | 5,016,578 | Gap (short; qualified) |
| [Bicubic mixed-axis RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-PAIRED.md) | 1,135.34 | 868.67 | 1.307 / 1.473 | 530,385 | Gap (short; qualified) |
| [Bicubic mixed-axis RGBA16§](../benchmarks/WARM-BICUBIC-PAIRED-COLUMNS.md) | 1,237.22 | 846.01 | 1.462 / 1.883 | 1,060,429 | Gap (short; qualified) |
| [Bicubic width reduction / height enlargement RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 994.41 | 840.45 | 1.183 / 1.259 | 731,753 | Gap (short; qualified) |
| [Bicubic width reduction / height enlargement RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 1,056.31 | 819.48 | 1.289 / 1.388 | 1,263,255 | Gap (short; qualified) |
| [Bicubic RGBA8, 128×64 / wider support§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 531.73 | 346.74 | 1.534 / 1.540 | 231,229 | Gap (independent) |
| [Bicubic RGBA16, 128×64 / wider support§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 504.51 | 416.18 | 1.212 / 1.218 | 263,991 | Gap (independent) |
| [Bicubic width reduction, unchanged height RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 734.08 | 574.67 | 1.277 / 1.365 | 465,374 | Gap (short; qualified) |
| [Bicubic width reduction, unchanged height RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 866.64 | 579.18 | 1.496 / 1.646 | 730,691 | Gap (short; qualified) |
| [Bicubic width enlargement, unchanged height RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 2,211.79 | 1,516.35 | 1.459 / 1.596 | 1,930,617 | Gap (short; qualified) |
| [Bicubic width enlargement, unchanged height RGBA16§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 1,965.91 | 1,460.21 | 1.346 / 1.467 | 2,129,333 | Gap (short; qualified) |
| [Bicubic RGBA8, 64×32 / support beyond 32§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 418.52 | 369.03 | 1.134 / 1.215 | 205,884 | Gap (short; qualified) |
| [Bicubic RGBA16, 64×32 / support beyond 32§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 522.57 | 390.56 | 1.338 / 1.351 | 214,071 | Gap (independent) |
| [Bicubic RGBA8, 32×16 / support beyond 64§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 452.57 | 375.86 | 1.204 / 1.208 | 199,355 | Gap (independent) |
| [Bicubic RGBA16, 32×16 / support beyond 64§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 455.52 | 412.28 | 1.105 / 1.110 | 201,398 | Gap (independent) |
| [Bicubic RGBA8, 16×8 / adaptive support beyond 128§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 363.20 | 452.30 | 0.803 / 0.809 | 201,728 | Parity confirmed |
| [Bicubic RGBA16, 16×8 / adaptive support beyond 128§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 441.93 | 496.89 | 0.889 / 0.893 | 202,231 | Parity confirmed |
| [Bicubic RGBA8, 256×129 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-BYTE-STRIPS.md) | 654.01 | 416.59 | 1.570 / 1.660 | 132,293 | Gap (short; qualified) |
| [Bicubic RGBA16, 256×129 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 818.75 | 570.81 | 1.434 / 1.445 | 264,469 | Gap (independent) |
| [Bicubic RGBA8, 128×64 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-FOUR-COLUMNS.md) | 498.19 | 347.76 | 1.433 / 1.644 | 32,913 | Gap (short; qualified) |
| [Bicubic RGBA16, 128×64 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 606.16 | 414.59 | 1.462 / 1.479 | 65,658 | Gap (independent) |
| [Bicubic RGBA8, 64×32 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-WEIGHT-BROADCAST.md) | 560.51 | 367.05 | 1.527 / 1.797 | 8,313 | Gap (short; qualified) |
| [Bicubic RGBA16, 64×32 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-WORD-BROADCAST.md) | 527.50 | 383.98 | 1.374 / 2.313 | 16,505 | Gap (short; qualified) |
| [Bicubic RGBA16, 32×16 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 570.51 | 412.45 | 1.383 / 1.394 | 4,217 | Gap (independent) |
| [Bicubic RGBA8, 32×16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-PACKED-REDUCTION.md) | 477.99 | 377.49 | 1.266 / 2.275 | 2,169 | Gap (short; qualified) |
| [Bicubic RGBA16, 16×8 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 487.53 | 495.21 | 0.984 / 0.995 | 1,145 | Parity confirmed |
| [Bicubic RGBA8, 16×8 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 489.92 | 451.81 | 1.084 / 1.090 | 633 | Gap (independent) |
| [Bicubic RGBA16 enlargement / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 4,188.86 | 2,870.82 | 1.459 / 1.637 | 4,264,616 | Gap (short; qualified) |
| [Bicubic RGBA8 enlargement / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 4,766.98 | 3,042.21 | 1.567 / 1.693 | 2,493,057 | Gap (short; qualified) |
| [Bicubic RGBA16, width up / height down / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-PAIRED-COLUMNS.md) | 1,197.19 | 853.79 | 1.402 / 1.555 | 1,060,439 | Gap (short; qualified) |
| [Bicubic RGBA8, width up / height down / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-BYTE-PAIRED.md) | 1,192.42 | 868.12 | 1.374 / 1.571 | 530,385 | Gap (short; qualified) |
| [Bicubic RGBA16, width down / height up / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,298.90 | 825.27 | 1.574 / 1.579 | 1,063,477 | Gap (independent) |
| [Bicubic RGBA8, width down / height up / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,249.21 | 831.82 | 1.502 / 1.513 | 531,910 | Gap (independent) |
| [Bicubic RGBA16, 8×4 / support beyond 256 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 445.77 | 693.80 | 0.643 / 0.651 | 377 | Parity confirmed |
| [Bicubic RGBA8, 8×4 / support beyond 256 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 508.41 | 664.36 | 0.765 / 0.772 | 249 | Parity confirmed |
| [Bicubic RGBA16, 4×2 / packed support beyond 304 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 624.81 | 1,235.83 | 0.506 / 0.509 | 185 | Parity confirmed |
| [Bicubic RGBA8, 4×2 / packed support beyond 304 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 786.38 | 1,138.73 | 0.691 / 0.695 | 153 | Parity confirmed |
| [Bicubic RGBA16, 2×2 / generated support beyond 544 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,069.82 | 2,336.26 | 0.458 / 0.487 | 154 | Parity confirmed |
| [Bicubic RGBA16, 2×9 / bounded row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,474.00 | 1,286.00 | 1.146 / 1.180 | 266 | Gap (independent) |
| [Bicubic RGBA16, 4×9 / packed row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 808.81 | 789.44 | 1.025 / 1.030 | 409 | Gap (independent) |
| [Bicubic RGBA16, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-8X17-RGBA16.md) | 516.33 | 530.91 | 0.973 / 0.981 | 1,209 | Parity confirmed |
| [Bicubic RGBA8, 8×17 / full-coefficient row blocks / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-8X17-RGBA8.md) | 538.78 | 494.65 | 1.089 / 1.100 | 665 | Gap (independent) |
| [Bicubic RGBA8, 4×9 / packed row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,002.56 | 675.80 | 1.484 / 1.488 | 266 | Gap (independent) |
| [Bicubic RGBA8, 2×9 / bounded row blocks / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,337.80 | 1,050.39 | 1.274 / 1.286 | 194 | Gap (independent) |
| [Bicubic RGBA8, 2×2 / generated support beyond 544 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 1,173.66 | 1,633.74 | 0.718 / 0.745 | 138 | Parity confirmed |
| [Bicubic height reduction, unchanged width RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 624.02 | 583.77 | 1.069 / 1.273 | 648,997 | Gap (short; qualified) |
| [Bicubic height reduction, unchanged width RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-BYTE-PAIRED.md) | 725.00 | 589.64 | 1.230 / 1.378 | 265,003 | Gap (short; qualified) |
| [Bicubic height reduction, unchanged width RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 782.41 | 592.55 | 1.320 / 1.672 | 913,737 | Gap (short; qualified) |
| [Bicubic height reduction, unchanged width RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-PAIRED-COLUMNS.md) | 770.52 | 589.55 | 1.307 / 1.361 | 529,847 | Gap (short; qualified) |
| [Bicubic width reduction, unchanged height RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-CURRENT-BICUBIC.md) | 838.73 | 579.56 | 1.447 / 1.459 | 530,858 | Gap (independent) |
| [Bicubic width reduction, unchanged height RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-TIGHT-HORIZONTAL8.md) | 811.19 | 573.66 | 1.414 / 1.425 | 265,502 | Gap (independent) |
| [Bicubic height enlargement, unchanged width RGBA8§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 1,776.78 | 1,742.77 | 1.020 / 1.024 | 1,664,251 | Gap (independent) |
| [Bicubic height enlargement, unchanged width RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 2,614.86 | 1,744.07 | 1.499 / 1.722 | 1,280,402 | Gap (short; qualified) |
| [Bicubic height enlargement, unchanged width RGBA16§](../benchmarks/WARM-BICUBIC-BYTE-EMITTER.md) | 1,998.15 | 1,670.35 | 1.196 / 1.482 | 2,516,599 | Gap (short; qualified) |
| [Bicubic height enlargement, unchanged width RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 2,286.62 | 1,672.40 | 1.367 / 1.438 | 2,133,103 | Gap (short; qualified) |
| [Bicubic width enlargement, unchanged height RGBA16 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 1,920.59 | 1,453.02 | 1.322 / 1.373 | 2,129,333 | Gap (short; qualified) |
| [Bicubic width enlargement, unchanged height RGBA8 / input+output+4 KiB limit§](../benchmarks/WARM-BICUBIC-ENLARGEMENT-STRIPS.md) | 2,125.43 | 1,509.12 | 1.408 / 1.529 | 1,178,425 | Gap (short; qualified) |
| [Crop RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 2.37 | 2.27 | 1.042 / 1.053 | 96 | Gap (independent) |
| [Crop RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 4.11 | 4.30 | 0.955 / 0.963 | 96 | Parity confirmed |
| [Clone RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 9.50 | 7.95 | 1.195 / 1.209 | 96 | Gap (independent) |
| [Clone RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 19.06 | 17.02 | 1.120 / 1.141 | 96 | Gap (independent) |
| [Normal orientation RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 9.29 | 7.63 | 1.217 / 1.254 | 128 | Gap (independent) |
| [Normal orientation RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 18.58 | 16.70 | 1.112 / 1.132 | 128 | Gap (independent) |
| [Vertical flip RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 9.28 | 8.37 | 1.108 / 1.124 | 128 | Gap (independent) |
| [Vertical flip RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 17.68 | 17.39 | 1.017 / 1.036 | 128 | Gap (independent) |
| [Horizontal flip RGBA8](../benchmarks/WARM-HORIZONTAL-FLIP512.md) | 9.08 | 9.55 | 0.950 / 0.965 | 128 | Parity confirmed |
| [Horizontal flip RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 21.83 | 27.03 | 0.807 / 0.811 | 128 | Parity confirmed |
| [Half turn RGBA8](../benchmarks/WARM-EDIT-CONFIDENCE.md) | 11.47 | 11.42 | 1.004 / 1.024 | 128 | Gap (independent) |
| [Half turn RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 18.59 | 21.86 | 0.850 / 0.874 | 128 | Parity confirmed |
| [Clockwise quarter turn RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 40.77 | 37.31 | 1.093 / 1.126 | 128 | Gap (independent) |
| [Clockwise quarter turn RGBA16](../benchmarks/WARM-EDIT-CONFIDENCE.md) | 43.45 | 44.08 | 0.986 / 1.010 | 128 | Confidence open |
| [Counterclockwise quarter turn RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 38.11 | 37.49 | 1.016 / 1.030 | 128 | Gap (independent) |
| [Counterclockwise quarter turn RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 49.42 | 45.49 | 1.086 / 1.103 | 128 | Gap (independent) |
| [Transpose RGBA8](../benchmarks/WARM-EDIT-CONFIDENCE.md) | 38.15 | 38.42 | 0.993 / 0.995 | 128 | Parity confirmed |
| [Transpose RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 48.17 | 44.86 | 1.074 / 1.114 | 128 | Gap (independent) |
| [Mirrored transpose RGBA16](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 44.34 | 43.25 | 1.025 / 1.064 | 128 | Gap (independent) |
| [Mirrored transpose RGBA8](../benchmarks/WARM-CURRENT-EXACT-EDITS.md) | 39.10 | 38.54 | 1.015 / 1.041 | 128 | Gap (independent) |
| [Lanczos3 reduction RGBA8](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) | 758.75 | 490.07 | 1.548 / 1.646 | 138,466 | Gap (short; qualified) |
| [Lanczos3 reduction RGBA16](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) | 804.22 | 506.28 | 1.588 / 1.759 | 138,454 | Gap (short; qualified) |
| [Lanczos3 enlargement RGBA8](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) | 4,324.39 | 4,076.28 | 1.061 / 1.225 | 319,046 | Gap (short; qualified) |
| [Lanczos3 enlargement RGBA16](../benchmarks/WARM-LANCZOS3-CERTIFIED-FMA.md) | 5,248.30 | 3,935.96 | 1.333 / 1.459 | 4,584,270 | Gap (short; qualified) |

* VP8L encode uses **libwebp exact=1, FFmpeg's encoder backend**, through its
public API because FFmpeg does not expose hidden-RGB preservation. All decoded
RGBA samples remain exact. Single-component grayscale JPEG uses the approved
benchmark-only **SIMD libjpeg-turbo3.1.4.1** reference; FFmpeg emits three components.
Production stays safe managed .NET 10 without dependencies or native bindings.

‡ Historical procedural VP8 timings use managed/native quality100/75 and
matched-source tolerances. The replacement encoder instead uses the pinned native
policy and complete-packet equality; those times/allocations do not qualify it.
The historical32M successful call is now a required terminal Work failure.


¶ PNG Identify references use the public FFmpeg PNG parser/CRC plus original
complete structural/EXIF validation and owned descriptors, without inflation;
the Stream reference also times fresh AVIO/scratch/input/EOF and release.
‖ JPEG Identify uses FFmpeg MJPEG discard-all, original complete structural/EXIF
validation and a timed17-byte canonical SOF2 metadata projection, fresh packet/
descriptor ownership and reset, with zero pixel-buffer requests. WebP Identify
uses the public FFmpeg WebP parser plus original complete RIFF/EXIF validation,
including mandatory parser buffering/EOF flush and descriptor ownership. Its
0.0040x/upper0.0045x is specific to this metadata boundary and parser copy cost.
All six managed metadata fields remain exact.

† RGBA16 bilinear and explicit RGBA8 upscale allow native-only alpha max1/RMSE≤0.25;
Up8 times zero-alpha RGB clearing. Managed pixels remain exact; Down8 alpha is exact.
§ Bicubic uses **zimg, FFmpeg's zscale backend**, with all required clamped halo,
packing, premultiplied float filtering, unpremultiplication/quantization, zero-alpha
RGB clearing and independent owners timed. Native RGB/alpha max1 and both RMSE≤0.25;
only RGBA16 513×259→513×519 permits native RGB≤2 where reference alpha≤16,≤1
elsewhere, retaining alpha≤1/both RMSE≤0.25. Managed frozen pixels and independently
certified rational-half cases remain exact. Linked reports specify every halo
and scope control; no managed tolerance is introduced.

The historical BCL PNG timing rows used managed9/native6 for Optimal,2/1 for
Fastest and9/9 for SmallestSize, with a5% IDAT timing-equivalence gate.
[Their packet audit](../benchmarks/PNG-COMPLETE-PACKET-AUDIT.md) found15 passes
and two open level-6 RGBA8 boundaries. These are superseded producer observations:
the current managed encoder instead requires all14 declared complete packets
to match exactly. A timing pass or IDAT tolerance establishes no packet-size pass.
Packed-gray
transparency uses the approved exact timed native color-key correction. Raw
incompatible baselines/refusals remain unscored; approved equivalent replacements
are confirmed. GIF/animation, startup, concurrency and arbitrary profiles are
outside this still-image fixed warm scope.

Maintain the selected evidence in [the renderer](../benchmarks/render-benchmark.py),
regenerate with `python benchmarks/render-benchmark.py`, then run `--check`.
Preserve raw evidence, failures, controls and package/source qualification.
Defer CPU-only optimization while compression gates are open; retain passing
performance evidence. Add a later case only when evidence identifies a distinct
required algorithm or public boundary omitted from the finite checklist.
