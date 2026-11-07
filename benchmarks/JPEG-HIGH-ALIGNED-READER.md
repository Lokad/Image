# JPEG high-aligned entropy readers

Current baseline costs are superseded by [buffered canonical lookup](JPEG-BUFFERED-LONG-LOOKUP.md).
This report retains the earlier paired gains, sparse contradictions and raw evidence.

One high-aligned local bit reservoir and shared signed-field arithmetic replace
repeated pending-dependent extraction in fused DC/AC, plain AC and live-token AC.
Original fractional bits and minimum logical cursor publish at every existing
exit. Slow readers, syntax failures, predictor/coefficient writes and every
cancellation checkpoint remain unchanged. No scan copy, widened table, owner,
cache/session or production dependency is introduced.

Independent complete-operation confirmations save **37.62 µs / 2.13% raw / 2.38%
reference-normalized** on full-grid RGB444 (1,764.20 → 1,726.58 µs), and **82.26 µs /
4.01% raw / 3.58% normalized** on live-token decode (2,053.83 → 1,971.57 µs).
Filtered/all-actual raw/normalized intervals separate for both. Native moves +0.25%
and −0.45%; allocation remains 10,088 B/op. These are scoped incremental gains.

Sparse CMYK initially improves 532.57 → 527.36 µs raw while its native reference
moves −2.78%, producing a contradictory normalized loss. One prespecified
reversed-order pair has a stable reference (+0.14%): 530.61 → 526.35 µs, with raw
intervals separated and normalized intervals overlapping. Both pairs remain
retained separately. No reliable reference-normalized CMYK gain, loss or
equivalence is claimed; no further sampling follows. The latest candidate supplies
its current parity cost below. No blanket JPEG speedup is claimed.

## Current scope

Twenty-three affected baseline costs refresh; six unchanged progressive
confirmations are reused. All retain twelve actuals and twelve native samples per
bracket. The existing authored 513×259 controls keep frozen inputs/settings,
managed pixels, native bounds, source and independently owned output checks.
Parsing, table preparation, entropy, reconstruction/color, input delivery and
output acquisition/disposal remain timed. Native context/conversion/input/frame
wrappers remain retained as declared; this is not fresh-native-context parity.

| Existing case | Lokad / FFmpeg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| DirectRgb444 | 1,726.58 / 1,454.05 | 1.187 / 1.206 | 10,088 | Gap |
| Decode8 | 1,599.80 / 1,902.55 | 0.841 / 0.848 | 4,304 | Pass |
| NativeDecode8 | 1,772.84 / 1,718.18 | 1.032 / 1.038 | 10,088 | Gap |
| NativeDecode420 | 993.21 / 1,005.63 | 0.988 / 0.993 | 757,363 | Pass |
| NativeDecode422 | 1,291.68 / 1,346.78 | 0.959 / 0.965 | 829,176 | Pass |
| OwnedDecodeGray | 652.75 / 677.77 | 0.963 / 0.970 | 672,202 | Pass |
| ProgressiveGray (reused) | 1,897.32 / 1,710.22 | 1.109 / 1.115 | 683,136 | Gap |
| Progressive420 (reused) | 1,733.75 / 1,951.39 | 0.888 / 0.893 | 775,323 | Pass |
| Progressive444 (reused) | 3,490.27 / 3,361.16 | 1.038 / 1.046 | 169,584 | Gap |
| Progressive422 (reused) | 2,213.98 / 2,251.17 | 0.983 / 0.991 | 847,116 | Pass |
| NativeDecode420Restart | 1,171.37 / 1,013.21 | 1.156 / 1.162 | 761,737 | Gap |
| StreamDecode420Restart | 1,166.59 / 990.28 | 1.178 / 1.182 | 761,833 | Gap |
| NativeDecodeCancelable8 | 1,971.57 / 1,717.80 | 1.148 / 1.163 | 10,088 | Gap |
| ProgressiveCancelable444 (reused) | 3,535.19 / 3,355.01 | 1.054 / 1.058 | 169,584 | Gap |
| DirectRgb422 | 1,311.61 / 1,092.60 | 1.200 / 1.210 | 829,176 | Gap |
| DirectRgb420 | 923.00 / 743.46 | 1.241 / 1.252 | 757,363 | Gap |
| CmykAdobe | 323.42 / 379.99 | 0.851 / 0.854 | 4,520 | Pass |
| CmykAdobeAc | 466.37 / 398.81 | 1.169 / 1.173 | 4,520 | Gap |
| CmykAdobeGeneral | 2,221.81 / 3,111.90 | 0.714 / 0.717 | 4,520 | Pass |
| CmykAdobeSparse2D | 526.35 / 437.63 | 1.203 / 1.221 | 4,520 | Gap |
| Ycck420 | 426.02 / 707.58 | 0.602 / 0.603 | 895,547 | Pass |
| CmykCaller | 331.02 / 331.70 | 0.998 / 1.001 | 1,085,963 | Uncertain |
| Ycck420CallerRgb | 428.13 / 604.06 | 0.709 / 0.710 | 762,704 | Pass |
| Decode411 | 516.26 / 446.62 | 1.156 / 1.185 | 751,295 | Gap |
| Decode410 | 355.57 / 507.41 | 0.701 / 0.708 | 720,838 | Pass |
| CallerGray | 572.48 / 646.46 | 0.886 / 0.890 | 273,431 | Pass |
| CallerRgb | 975.24 / 1,157.47 | 0.843 / 0.845 | 624,511 | Pass |
| ValidateJpegBaseline420 | 475.81 / 676.16 | 0.704 / 0.709 | 9,984 | Pass |
| ValidateJpegProgressive420 (reused) | 271.02 / 268.34 | 1.010 / 1.019 | 4,072 | Gap |

Fourteen pass and stop; fourteen gaps and caller-CMYK uncertainty remain. Native
baseline420 now passes. Caller-CMYK upper 1.001 is uncertain, without a demonstrated
optimization gap. The other refreshed costs establish current parity status,
without causal historical-gain claims. Small allocation-counter differences on
caller CMYK, 4:1:1 and caller gray do not establish storage savings or growth.

## Correctness and ownership

All 4,094 signed amplitude fields are checked through the actual shared helper,
including unrelated following bits. Existing every-category/position/phase,
truncation, marker/stuffing, restart, predictor, partial-write, next-read and
cancellation tests exercise the production readers. Independent cache arithmetic
checks 12,800 refill/fractional-publication controls. Production fields and slow
helpers outside the selected loops remain unchanged; no precision policy changes.

Windows/Linux each pass **5,867 full/live checks**, plus **398 internal / 375 public**
per normal/scalar mode. The SAME actual private Release package and all three
examples pass both OSes. One immutable core/embedded build per OS; zero public,
harness, selector, native or fixture builds. No full/package repeats follow.

Substantial storage and ownership are unchanged: governed native444 admitted peak
541,253 B and retained output 531,468 B reuse [full-grid ownership](JPEG-FULL-GRID-OUTPUT.md).
The existing default 4 MiB pool and shared-budget bypass remain. No new capacity or
retained state; physical/native peaks are unmeasured. GC totals above are separate.
Encoder packets, metadata exceptions, managed pixels, terminal refusal, cleanup,
borrowed roots, independent survivors and actual async lifetimes remain qualified.

## Evidence

The first adapted worker omitted the existing qualified metadata projection;
the unchanged original rejected setup before timing. Restoring its verified scoped
apply/restore wrapper preserves frozen inputs/gates. A wrapper-variable collision
after the completed live original short was corrected and the complete result
recovered without rerun. All original preparation bytes restore exactly. These
failures, all actuals and the adverse CMYK comparison remain retained.

[Current evidence](jpeg-high-aligned-reader-evidence.json) pins sources, producers,
comparisons, current cases and same-package qualification. Earlier
[plain-reader publication](JPEG-PLAIN-READER.md) and
[architectural attribution](JPEG-PLAIN-READER.md#architectural-follow-up) retain
source-specific history; the profile predates this reservoir and is not current
method attribution. The unselected amplitude-nibble experiment is not replayed.

Raw `jpeg-high-aligned-reader-qualified.tar.xz`, SHA256
`119b474aba87b720c6021f2ab02a252ff84f20aef336068fe3cf4bb7bba35148`, 7,848,840 B / 487 members, is mirrored/member-verified.
It retains original/current modules, sources, scripts, actuals, gates, failed setup,
corrections, regression/package logs and pinned input-provider dependencies.
Other families reuse unchanged evidence. This is not a 125-row actual-package
release refresh or completion of all 283 material paths. Publication is postponed.
