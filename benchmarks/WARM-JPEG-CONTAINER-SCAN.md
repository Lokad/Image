# Bounded JPEG decoder container scan

The existing owned baseline444 decode control independently saves **13.81%
elapsed** (14.08% all-actual, **13.97% native-normalized**), with separated raw
and normalized confidence. It now passes and stops at **0.970× / upper 0.989**.
Measured allocation is **948,077 B/op**, versus 948,069 (+8 bytes).
The six confidence-open rows below are superseded by the
[bounded confidence audit](CURRENT-JPEG-CONFIDENCE.md); other23costs remain current.

The decoder preflight now searches ordinary entropy in spans of at most 4 KiB
using safe managed Span.IndexOf. It stops before every FF; original marker,
stuffing, restart, segment-length, item/metadata limits, source metadata, EXIF,
EOI and trailing-byte rules remain. Each search returns to the existing cancellation
checkpoint. No allocation, ownership, admission, IDCT or pixel-publication change.
The frozen 324,002-byte packet has 323,579 ordinary bytes in eight runs: 82 bounded
searches replace their individual loop visits. Those counts describe work, not time.

| Existing complete 513×259 operation | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Status |
|---|---:|---:|---:|---:|---|
| DirectRgb444 | 1,921.90 | 1,453.55 | 1.322 / 1.362 | 10,088 | confirmed-gap |
| Decode8 | 1,849.55 | 1,905.81 | 0.970 / 0.989 | 948,077 | confirmed-pass-stop |
| NativeDecode8 | 2,054.34 | 1,721.13 | 1.194 / 1.225 | 966,367 | confirmed-gap |
| NativeDecode420 | 1,055.77 | 1,034.46 | 1.021 / 1.067 | 757,362 | confirmed-confidence-open |
| NativeDecode422 | 1,383.58 | 1,360.19 | 1.017 / 1.059 | 829,164 | confirmed-confidence-open |
| OwnedDecodeGray | 717.59 | 690.04 | 1.040 / 1.085 | 672,202 | confirmed-confidence-open |
| ProgressiveGray | 2,210.71 | 1,714.85 | 1.289 / 1.319 | 683,126 | confirmed-gap |
| Progressive420 | 2,152.83 | 1,979.17 | 1.088 / 1.137 | 775,318 | confirmed-gap |
| Progressive444 | 4,404.74 | 3,370.40 | 1.307 / 1.335 | 984,212 | confirmed-gap |
| Progressive422 | 2,706.44 | 2,246.52 | 1.205 / 1.214 | 847,116 | confirmed-gap |
| NativeDecode420Restart | 1,225.34 | 1,015.59 | 1.207 / 1.235 | 761,736 | confirmed-gap |
| StreamDecode420Restart | 1,236.77 | 994.56 | 1.244 / 1.265 | 761,833 | confirmed-gap |
| NativeDecodeCancelable8 | 2,183.89 | 1,720.75 | 1.269 / 1.301 | 966,367 | confirmed-gap |
| ProgressiveCancelable444 | 4,925.83 | 3,408.60 | 1.445 / 1.515 | 984,212 | confirmed-gap |
| DirectRgb422 | 1,405.70 | 1,096.72 | 1.282 / 1.318 | 829,164 | confirmed-gap |
| DirectRgb420 | 995.92 | 759.10 | 1.312 / 1.376 | 757,363 | confirmed-gap |
| CmykAdobe | 350.12 | 379.47 | 0.923 / 0.940 | 1,085,759 | confirmed-pass-stop |
| CmykAdobeAc | 518.51 | 396.74 | 1.307 / 1.353 | 1,085,763 | confirmed-gap |
| CmykAdobeGeneral | 2,575.62 | 3,161.31 | 0.815 / 0.860 | 1,085,661 | confirmed-pass-stop |
| CmykAdobeSparse2D | 607.81 | 446.62 | 1.361 / 1.429 | 1,085,757 | confirmed-gap |
| Ycck420 | 436.05 | 728.08 | 0.599 / 0.626 | 895,546 | confirmed-pass-stop |
| CmykCaller | 342.19 | 333.47 | 1.026 / 1.056 | 1,085,967 | confirmed-confidence-open |
| Ycck420CallerRgb | 441.76 | 624.43 | 0.707 / 0.742 | 762,700 | confirmed-pass-stop |
| Decode411 | 529.90 | 450.86 | 1.175 / 1.213 | 751,290 | confirmed-gap |
| Decode410 | 372.23 | 509.10 | 0.731 / 0.756 | 720,838 | confirmed-pass-stop |
| CallerGray | 662.83 | 649.65 | 1.020 / 1.050 | 273,430 | confirmed-confidence-open |
| CallerRgb | 1,069.50 | 1,180.92 | 0.906 / 0.952 | 624,516 | confirmed-pass-stop |
| ValidateJpegBaseline420 | 543.78 | 689.48 | 0.789 / 0.815 | 9,984 | confirmed-pass-stop |
| ValidateJpegProgressive420 | 283.04 | 276.19 | 1.025 / 1.068 | 4,072 | confirmed-confidence-open |

All 29 affected existing boundaries have independent current costs: 25 RGBA
decodes, two caller-sample outputs and two coding validations. Only Decode8 has
a paired gain claim. Passing cases stop; uncertain evidence is resolved before
assuming optimization is needed. No new source, shape, depth, quality or case grid.

The primary original confirmation is reused from the
[retained-result round](WARM-JPEG-DIRECT-RGB-RESULT.md): subsequent four-component
color changes have no call from this three-component444 input. The native bracket
is 0.18% slower; elapsed and normalized gains are reported separately. Exact pixels,
input/settings/native identities, output ownership, padding, cancellation and
existing bounded-native gates pass. All costs include the same complete operations,
one pinned CPU, 12 actuals, native brackets and idle-v2 guards. Native allocations
are unmeasured. Current read-proof identity follows fresh functional qualification;
temporary qualified fixture expectations are restored byte-exactly afterward.

Fresh **5,369 checks per OS** (2,176 internal, 2,245 public, 948 live) pass.
Focused 28 internal/399 public checks pass normal/scalar on both OSes. Existing
independent metadata expectations, 4 KiB/vector/tail/stuffing/restart boundaries,
malformed/truncated/trailing cases, limits, late declarations, cancellation,
shared admission and surviving outputs remain. The **same actual Release package
and three usage examples** pass Windows/Linux. One core build per OS; zero test,
harness, native, selector or differential builds. One primary short/confirmation
and one confirmation per affected companion; no new profile or repeated valid score.

Public identification/header inspection, encoders, other codecs, editing and text
reuse applicable unchanged evidence. Prior JDO36/JYC37 CPU/allocation observations
for these affected decoder boundaries are historical; their implementation gains
remain recorded. The full 268-path warm objective is still open.

[Evidence](jpeg-container-scan-evidence.json). Raw jpeg-container-scan-qualified.tar.xz,
485 members, SHA256 1fac10336e4dad0a9a22bc4bc4c4494cea37cf35ad311107417ce0b02c220006.
The mirrored/member-verified archive retains sources and actual selected/original
binaries, immutable-runtime pins, frozen input/expectation projections, cached
selectors/workers, every sample/guard/receipt, both-platform qualification and actual
packages. Reused raw dependencies are pinned explicitly. No release-wide refresh
or broader content/layout claim.
