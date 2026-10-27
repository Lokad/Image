# Exact scored-plane color histograms

The fixed photograph independently uses **6.4% less elapsed**: 112.76 ms versus
the fresh unchanged-producer baseline of120.54 ms. It remains a **1.350× /
upper1.371** CPU gap. Allocations are unchanged; complete packets remain exact.
These are separate sequential independent observations, not a paired-sample
statistical estimate or a general-content speed claim.

| Existing control | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Both packet B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Photograph baseline, independent | 120,535.71 | 82,159.98 | 1.467 / 1.511 | 20,409,400 | 160,032 |
| Photograph SIMD, independent | 112,763.24 | 83,526.34 | 1.350 / 1.371 | 20,409,400 | 160,032 |
| RGBA8, screen | 174,761.42 | 121,059.75 | 1.444 / 1.702 | 29,688,704 | 183,812 |
| RGBA16, screen | 174,544.82 | 121,397.18 | 1.438 / 1.686 | 29,688,704 | 183,812 |
| Async nonseekable, screen | 179,408.83 | 120,784.16 | 1.485 / 2.434 | 29,689,088 | 183,812 |

All controls retain513×259 inputs, method4, libwebp Q75/exact1, explicitly FFmpeg's
encoder backend. Complete operations include normalization, fresh independent
output ownership, async delivery and release. Native allocations are unmeasured.
Photograph confirmations retain every actual, including two baseline values
filtered from its mean. Both idle-v2 guards pass. The three companions are short
current-cost screens; no independent gain or CPU-pass claim follows from them.

Safe Vector256 lanes compute only the red or blue plane being scored, widening
signed bytes before exact multiplication/shift. Scalar bin increments preserve
duplicate lanes; portable scalar/tail arithmetic preserves modulo256 results.
The bounded32-byte stack scratch adds no owned array, cache, reservation or frame
copy. Existing7296B scratch admission and per-row cancellation remain unchanged.

Both OSes pass5,044 Release checks each, including947 live comparisons, plus13
internal and27 public exact-packet scalar checks. New proofs cover both planes,
all256 coefficient residues, signed wrapping, strided/unaligned clipped tiles,
vector edges/tails, guards, source ownership, zero allocation and early cancellation.
Native method4/5 maps/residuals, scratch peaks, refusal/cancellation cleanup and
exact packets remain. The SAME actual private0.2.1 package and three usage examples
pass both systems; published releases are unchanged.

One current two-second diagnostic on the qualified baseline collected1,980 samples:
color-cost evaluation14.6%, predictor scoring12.1%, merged populations11.0%.
Scored color loops plus the full transform account for11.4% of leaf samples.
These are diagnostic locations, not scored method costs. A helper assertion
stopped the first attempt before sampling; the corrected capture passes both
guards. Count-only evidence rejects a small per-bin cache before a production
trial. Neither that cache nor another SIMD variant is added.

Internal tests compile linked production sources; they do not build the package
DLL. Its frozen hash identified the first screen as the unchanged baseline. The
candidate DLL was then built explicitly once per OS and reused through selection
and full qualification. The correction, baseline and every actual are retained.
Benchmark/native/selector builds are reused. Other affected named encoding paths
need current evidence before any pass claim; diagram4 and compressed VP8 alpha
skip this color search. Other codecs, decoders, editing and text remain unchanged.

[Evidence](warm-vp8l-scored-plane-histograms-evidence.json) records the producers,
boundaries, raw observations, qualification and mirrored/member-verified archive
`vp8l-scored-plane-histograms-qualified.tar.xz`, SHA256 `eafaa52039368e4b9b738f991249d0a4832477a62a9f37ae72f1c07f9b7ae90e`.
Stop this round and prioritize the remaining larger VP8/JPEG gaps. The full
267-path warm single-CPU ≤1× objective remains open.
