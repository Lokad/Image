# Full-grid JPEG output ownership

Full-resolution baseline three-component JPEG keeps raw samples in its private
final image and converts after complete scans/Adobe validation. This removes padded
sample planes and separate final acquisition while retaining exact managed pixels.
Encoded source metadata remains independent: untagged inputs still report Unknown.
Encoder packets, native bounds, caller buffers and optional orientation are unchanged.

| Independent complete operation | Lokad µs before → final | Raw / normalized gain | Allocated B/op before → final |
| --- | ---: | ---: | ---: |
| NativeDecode8 | 2,001.28 → 1,920.61 | 4.03% / 4.06% | 966,370 → 10,088 |
| NativeDecodeCancelable8 | 2,188.08 → 2,040.63 | 6.74% / 6.81% | 966,367 → 10,088 |

Both filtered/all-actual raw and normalized intervals separate. Native moves
.033%/.075%; current ratios/uppers1.117/1.130 and1.186/1.192 remain gaps.
All necessary parsing/preparation/entropy/IDCT/color/output/disposal stays timed.
Native codec/conversion/input/frame wrappers remain retained as declared.

A shared SIMD/scalar Q13 core preserves coefficient derivation, signed products,
bias, clipping and packing. In-place loads precede stores; disjoint WriteRow still
rejects aliasing. Raw data stays private until final interpretation and all scans
succeed. Caller sample/validation paths never enable this owner. Progressive and
subsampled storage and portable kernels remain. Two full-grid fallback branches
are now unreachable and removed; no duplicate entropy/geometry or caching framework.

Primary input is the frozen133,041B,513×259,H1/V2 packet. Its424,320B sample
planes overlap531,468B output in the original. New384B MCU workspace is admitted
before its borrower, with local selection before admission and terminal shared
refusal. Governed peak965,189→541,253B; result531,468B stays charged until disposal.
Default sequential idle pool+531,468B is explicit within the existing4MiB cap;
governed calls bypass it. Physical/native peaks and governed CPU/GC are unmeasured.

The intermediate stage regresses RGB64.79µs/3.52% despite stable native timing.
Moving only final YCbCr row work to a private no-inline borrower recovers50.04µs/
2.62% against that stage, both confidence views separated. FinalRGB is1,856.71µs
versus FFmpeg1,451.57µs,upper1.285. Residual14.75µs/.80% against the qualified
original is a point loss: filtered raw separates; all-actual raw and normalized
intervals overlap. This is not equivalence. All intermediate/adverse results and
modules remain retained; no unchanged favorable resampling. Scoped JIT facts are
captured in the required untimed storage checks, not a dynamic traffic attribution.

## Current JPEG comparisons

All 29 existing color/owned/caller/stream/live/coding-validation controls have twelve actuals
and twelve native samples per bracket on the same immutable final DLL. Shared
decoder codegeneration can affect validation despite skipping color; both existing validation costs refresh. No new matrix.

| Existing case | Lokad / FFmpeg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| DirectRgb444 | 1,856.71 / 1,451.57 | 1.279 / 1.285 | 10,088 | gap |
| Decode8 | 1,742.09 / 1,896.55 | 0.919 / 0.924 | 4,304 | pass-stop |
| NativeDecode8 | 1,920.61 / 1,719.39 | 1.117 / 1.130 | 10,088 | gap |
| NativeDecode420 | 1,044.45 / 1,009.47 | 1.035 / 1.040 | 757,362 | gap |
| NativeDecode422 | 1,354.95 / 1,346.17 | 1.007 / 1.013 | 829,176 | gap |
| OwnedDecodeGray | 702.17 / 679.99 | 1.033 / 1.040 | 672,202 | gap |
| ProgressiveGray | 1,897.32 / 1,710.22 | 1.109 / 1.115 | 683,136 | gap |
| Progressive420 | 1,733.75 / 1,951.39 | 0.888 / 0.893 | 775,323 | pass-stop |
| Progressive444 | 3,490.27 / 3,361.16 | 1.038 / 1.046 | 169,584 | gap |
| Progressive422 | 2,213.98 / 2,251.17 | 0.983 / 0.991 | 847,116 | pass-stop |
| NativeDecode420Restart | 1,232.91 / 1,010.84 | 1.220 / 1.227 | 761,736 | gap |
| StreamDecode420Restart | 1,227.54 / 990.39 | 1.239 / 1.247 | 761,833 | gap |
| NativeDecodeCancelable8 | 2,040.63 / 1,720.80 | 1.186 / 1.192 | 10,088 | gap |
| ProgressiveCancelable444 | 3,535.19 / 3,355.01 | 1.054 / 1.058 | 169,584 | gap |
| DirectRgb422 | 1,382.87 / 1,095.64 | 1.262 / 1.279 | 829,164 | gap |
| DirectRgb420 | 982.24 / 744.11 | 1.320 / 1.324 | 757,363 | gap |
| CmykAdobe | 312.51 / 379.05 | 0.824 / 0.829 | 4,520 | pass-stop |
| CmykAdobeAc | 477.94 / 391.55 | 1.221 / 1.228 | 4,520 | gap |
| CmykAdobeGeneral | 2,466.99 / 3,111.37 | 0.793 / 0.795 | 4,520 | pass-stop |
| CmykAdobeSparse2D | 542.48 / 437.50 | 1.240 / 1.245 | 4,520 | gap |
| Ycck420 | 424.93 / 707.07 | 0.601 / 0.603 | 895,547 | pass-stop |
| CmykCaller | 350.23 / 330.42 | 1.060 / 1.062 | 1,085,964 | gap |
| Ycck420CallerRgb | 451.59 / 604.09 | 0.748 / 0.749 | 762,703 | pass-stop |
| Decode411 | 525.76 / 448.55 | 1.172 / 1.210 | 751,299 | gap |
| Decode410 | 360.19 / 506.59 | 0.711 / 0.719 | 720,838 | pass-stop |
| CallerGray | 623.82 / 647.15 | 0.964 / 0.969 | 273,430 | pass-stop |
| CallerRgb | 1,026.60 / 1,162.99 | 0.883 / 0.885 | 624,511 | pass-stop |
| ValidateJpegBaseline420 | 537.30 / 673.56 | 0.798 / 0.803 | 9,984 | pass-stop |
| ValidateJpegProgressive420 | 271.02 / 268.34 | 1.010 / 1.019 | 4,072 | gap |

Current status: {'confirmed-pass-stop': 11, 'confirmed-gap': 18, 'confirmed-confidence-open': 0}. Passing cases stop.

Windows/Linux each pass5,850 full/live checks (2,585 internal / 2,317 public / 948 live),
plus101 internal / 375 public normal/scalar affected checks. Five new facts cover
458,752 independent Q13 triples, native-qualified full image, complete untagged/
late/split/restart/nonunit frames, dirty output and terminal refusal/cancellation/
survivor/recovery/async ownership. Four existing local-ceiling expectations now
reflect actual removed capacity, retaining exact-ceiling success and one-byte-less
failure; that test-only fix reused production/embedded modules. The same actual
private Release package and all three examples pass both OSes. Publication remains
postponed, published releases unchanged, and all283 material paths remain in scope.

[Earlier shared components](JPEG-COMPONENT-FRAME.md) and
[progressive ownership](JPEG-PROGRESSIVE-OUTPUT.md) retain historical paired gains
and raw. Their affected timings are superseded here. Other codecs/encoders/editing/
text costs reuse valid evidence. This is not a full release performance refresh.

[Evidence](jpeg-full-grid-output-evidence.json) pins producers, cases, decisions,
ownership and raw reproduction. Raw `jpeg-full-grid-output-qualified.tar.xz`, SHA256
`d1dc971452111f0370e16a23257f479e3296105ddd320c0790481dd04fd67289`, 6305828B/617members,
is mirrored/member-verified. It retains all stages/originals, zero-actual setup
failures, inputs/references, current samples, qualification and actual packages.
No upstream code or production dependency is added.
