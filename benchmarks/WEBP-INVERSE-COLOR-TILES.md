# Lossless WebP inverse color tiles

Inverse color reconstruction now prepares each tile's three signed coefficients
once and restores eight independent pixels per AVX2 batch, with a scalar tail and
portable path. Red wraps to8bits before its signed value participates in blue.
Original modulo256 arithmetic, alpha/green/hidden RGB, per-row cancellation and
all ownership/admission remain exact. No buffers, pools, retained state or API.
The entire original file reconstructs by replacing only this inverse-color loop;
readers, other transforms and encoders are unchanged.

Complete live-token decoding saves **191.79µs/6.14% raw/7.18% normalized** against
the [accepted Huffman baseline](WEBP-HUFFMAN-READ-FRONTEND.md). All-actual and
diagnostic raw confidence and conservative normalized bounds separate; native
moves+1.127%. Current2,933.10/FFmpeg3,059.18µs,range0.907–0.989 passes/stops.
No gain is inferred from sample counts or a sum of historical stages.

| Existing complete public decode | Lokad / FFmpeg µs | Conservative ratio interval; status | Managed B/op |
| --- | ---: | ---: | ---: |
| NativeLosslessCancelable8 | 2,933.10 / 3,059.18 | 0.907–0.989; pass | 152,728 |
| IndexedNative16Colors8 | 420.09 / 544.27 | 0.766–0.778; pass | 288,465 |
| WebpResident | 2,719.46 / 3,020.83 | 0.896–0.907; pass | 152,728 |
| Lossless8 | 1,552.01 / 2,318.20 | 0.665–0.675; pass | 10,840 |
| WebpStream | 2,906.33 / 3,151.62 | 0.866–0.983; pass | 152,824 |
| Native8 | 5,789.88 / 5,965.87 | 0.967–0.975; pass | 240,400 |
| Vp8Stream | 6,187.90 / 6,465.82 | 0.948–0.964; pass | 240,496 |

All six material lossless read/validation controls now pass. Seven actual decode
costs refresh before qualification. The two validators skip inverse arithmetic
and reuse source-valid HFR847 receipts:VP8L2,225.12/3,204.19µs upper0.730,
297,070B/op; VP8+ALPH4,350.11/5,453.13µs upper0.801,15,600B/op.
With the unchanged opaque VP8 controls, VP8/ALPH has5passes/1uncertain; the live
opaque uncertainty remains open. No uniform causal companion gain is claimed.
Lossless8 raw/native clocks both rise while its normalized intervals overlap;
that is retained, not presented as an allocation or uniform raw improvement.

The original global short screen3,051.03/3,230.29µs is separate from the independent
matched4op/12pair natural-GC confirmation. Every actual and all12native samples
before/paired/after remain. Candidate native brackets3,059.18/3,135.86/3,149.28µs
are disclosed; the conservative faster bracket is used. Six companion confirmations
reuse their original12actual selectors once. No tooling, selector, fixture, native
worker or public-test build; one immutable core/embedded build per OS.

Complete public Decode, declared stream acquisition/copies/EOF, independent output
allocation/disposal and native reset/decode/conversion/owned output release stay
timed. Native retains documented contexts/wrappers. Primary managed input has a
live unsignaled token; native has no equivalent cancellation setting. Token/source
creation/disposal, startup, IPC, generation, hashes and validation are outside clocks.
Original input/settings/reference identities and exact output/owner/normal+scalar
CLI/alpha/hiddenRGB/cancellation/idle-v2 gates pass, with oneCPU/CPU0. The primary
frozen input/output are183,812/531,468B. No reference tolerance is relaxed.

Primary allocation stays152,728B/op; no new owned or retained capacity and zero
retained operation scratch. Result lifetime and caller-budget borrowing remain
unchanged. ALPH totals retain earlier VP8 preparation. Total allocation, admitted
capacity, output ownership and pool retention are separate; physical/native/JIT
peaks are unmeasured. Main decoder code remains8,245B; Apply shrinks786→699B,
and the new tile helper is897B with three vector signed multiplies. No physical
peak, universal allocation saving or unrelated-path speedup is claimed.

Seven tests compare exact pixels against a floor/modulo oracle across signed
coefficient extremes, updated-red wraparound, tile/row/vector/tail edges, hidden
RGB, poisoned borders, zero admission/allocation and canceled-before-write behavior.
Windows/Linux each pass6,456full/live checks (3,127internal+2,381public+948live),
plus1,033internal/239public in normal/scalar/AVX512-off/AVX2-off modes. Unchanged
Huffman BMI2, resizing, PNG/JPEG/text evidence is reused. The SAMEactual Release
package and all three usage examples pass both OSes. Published packages remain
immutable and publication is postponed. No extra qualification campaign.

[Evidence](webp-inverse-color-tiles-evidence.json) retains source proof, all costs,
two justified reused validators, original gates, codegen, tests and package hashes.
Raw`webp-inverse-color-tiles-qualified.tar.xz`,SHA256`b3c16dcb02daea7d98332a85d07e42bb91069a5b90d1cb87ad933b2a8f0f8a8d` (2578468B/143members), is mirrored and
member-verified; dependencies preserve the prior package and attribution.
All289required paths remain. Stop confirmed controls and continue an independent
demonstrated resizing path; do not retune passes or treat uncertainties as gaps.
