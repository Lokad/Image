# Prepared VP8 encoder prediction

Historical accepted round; [current zero-error search results](WARM-VP8-ZERO-ERROR.md) replace these four complete-operation timings. Prepared geometric prediction remains in use.

Complete opaque VP8 encoding is **13.1-13.7% faster**, with unchanged packets,
quality and allocations. RGBA16 improves from1.685x to1.460x FFmpeg; RGBA8 from
1.661x to1.429x. Both remain qualified short gaps. The two affected alpha paths
were independently re-confirmed below parity, with12.6-14.0% less managed time.

Mode search gathers thirteen immutable top/left/corner samples once per4x4luma
block, reuses them through the original ordered candidates and final best write,
and emits whole-block predictions with one mode dispatch. Effort4 originally
gathered the same neighborhood seven times and dispatched112pixel modes perblock.
The52Bbounded stack scratch adds no heap, table, field or mandatory admission.
First/every subsequent Work(192), exact scores, strict-less ties, final writes,
failure/cancellation order, mode choices and packets are preserved.

| Existing selector | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Managed time improvement | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Opaque RGBA16 | 22,813.15 / 15,629.98 | 1.460 / 1.611 | 2,536,099 | 13.14% | Gap (short; qualified) |
| Opaque RGBA8 | 22,212.16 / 15,539.04 | 1.429 / 1.538 | 2,536,280 | 13.71% | Gap (short; qualified) |
| Alpha RGBA16 | 23,199.00 / 53,597.53 | 0.433 / 0.437 | 2,615,835 | 12.63% | Parity confirmed |
| Alpha RGBA8 | 22,598.85 / 53,698.20 | 0.421 / 0.424 | 2,616,016 | 14.05% | Parity confirmed |

All four short screens retain3actuals at200ms and native50ms brackets. The changed
alpha paths, which already passed, each receive one independent12actual repeat
on the same immutable build. RGBA16 selects11 but retains all12actuals and its
outlier; the conservative upper uses the larger selected/all-actual bound. Both
`exclusive-vm-idle-v2` guards pass for all6jobs. Opaque means remain above1, so no
routine long repeats. B/op includes harness costs; native allocation is unmeasured.

The original two513x259 rasters/projections and frozen managed100/native75,
effort4 settings remain. Full normalization/conversion, prediction, quantization,
entropy, packet ownership/materialization and release are timed. Exact packets
remain333,580/72,380B managed/native for opaque inputs and471,436/98,590B with
alpha. Source-PSNR gaps remain0.025638/0.003646dB, within the unchanged0.25dB
gate. Exact alpha, normalized source, decoded hashes, source preservation and
independent owners pass. Compression-efficiency parity is not established.

The retained profile's complete encoder/prediction sources match the predecessor;
no new profile was needed. An address audit covers192boundary geometries; an
independent697,120pixel comparison checks all10required block formulas. Two new
facts compare whole planes through repeated modes/default and right-edge samples,
and preserve invalid-mode errors before writes. Original decoder Small/Large/
SmallPixel bodies and calls remain unchanged, as do all5P57decode timings.

**3,720 Release tests perOS** pass:1,386internal+1,436public+898live FFmpeg, plus
32internal+147public scalar controls. The same actual Release package's consumer
and allthree examples pass on Windows/Linux. One producer build perOS; zero
harness/selector/native builds, four shorts+two necessary confirmations, no new
performance image, case or settings grid. All control means and conservative
alpha bounds were read before full qualification.

Exactly four encode rows change; the other203comparisons and58passing proofs
remain applicable, with two renewed alpha passes. Current totals stay60passes/
143gaps/4confidence,127selected independent+80qualified shorts. Stop the two
confirmed alpha paths. The published0.1.0 tag/packages remain intact.
[Numeric evidence](warm-vp8-prepared-prediction-evidence.json) pins all builds,
allocations, sizes, actuals/outliers, conservative bounds, raw archives and package.
The full207-case <=1x objective remains open.
