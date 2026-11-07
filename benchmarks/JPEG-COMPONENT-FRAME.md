# Shared JPEG component-frame storage

Complete single-CPU decode improves on two frozen513×259 controls. Both remain
slower than FFmpeg; exact managed pixels, approved native bounds and complete
operation boundaries are unchanged. No encoded packet or encoder changes.

| Independent comparison | Lokad µs before → after | Raw / native-normalized gain | Allocated B/op before → after |
| --- | ---: | ---: | ---: |
| DirectRgb444 | 1,876.89 → 1,841.96 | 1.86% / 1.89% | 10,088 → 10,088 |
| CmykAdobeSparse2D | 592.01 → 543.34 | 8.22% / 5.53% | 1,085,757 → 4,520 |

Filtered and all-actual raw/normalized intervals separate. Native moves+0.026%
forRGB and−2.84% forCMYK. Current ratios/uppers are1.270/1.279 and1.242/1.247.
RGB's equal sampling grid isH1/V2, not an assumed1×1 grid. The first RGB-only
screen was inconclusive; only the coherent final stage has accepted comparisons.

Baseline equal-grid RGB/CMYK/YCCK share the existing entropy/restart/IDCT walk,
component placement and private final owner. Complete MCUs transform directly into
bounded component slots, then publish all channels once. Four-component data keeps
K as raw data until complete container validation and exact in-place color conversion.
CMYK retains value×black×257>>16; no rounded/255 substitution. Separate/partial scans,
portable tails, final/late Adobe interpretation and failure cleanup remain supported.
Progressive/subsampled inputs keep their existing storage. Caller sample decoding
still borrows output without mandatory RGBA. No new session/cache/owner/budget model.

The original549,120B CMYK planes disappear. Workspace is admitted before its
no-inline stack borrower:384B onRGB,256B onCMYK, at most640B generically. The
existing fallback64B scratch remains. Shared refusal is terminal; local workspace
selection happens before admission. All original cancellation polls remain.

BothOS untimed storage checks measure governed peak1,084,722→535,858B; the output
retains531,468B until disposal. Default sequential calls now leave531,468B idle in
the existing4MiB-capped exact-size pool; governed calls bypass it. These are admitted
capacity and retained storage, not physical/native peaks. Governed CPU/GC is not
measured. The unchanged generic storage adapter is pinned by actual CMYK input,
output and assembly hashes; its earlier static banner has no measurement role.

## Current affected comparisons

All23 named baseline/caller/stream/live/validation boundaries have12actuals and
12native samples per bracket on the same immutable Linux DLL. Native contexts
remain retained as declared. Per-operation parsing, required preparation, entropy,
IDCT, conversion, normalization and independent output/disposal remain timed as
applicable. Fresh streams remain fresh. No new images/settings grid or equal-number
quality assumption. This is a current cost reconciliation, not23causal speedup claims.

| Existing case | Lokad / FFmpeg µs | Ratio / conservative upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| DirectRgb444 | 1,841.96 / 1,450.50 | 1.270 / 1.279 | 10,088 | gap |
| Decode8 | 1,846.44 / 1,901.57 | 0.971 / 0.984 | 948,077 | pass-stop |
| NativeDecode8 | 2,001.28 / 1,718.81 | 1.164 / 1.170 | 966,370 | gap |
| NativeDecode420 | 1,037.62 / 1,007.13 | 1.030 / 1.039 | 757,362 | gap |
| NativeDecode422 | 1,353.06 / 1,346.28 | 1.005 / 1.012 | 829,176 | confidence-open |
| OwnedDecodeGray | 705.51 / 680.87 | 1.036 / 1.043 | 672,202 | gap |
| NativeDecode420Restart | 1,221.61 / 1,012.89 | 1.206 / 1.214 | 761,737 | gap |
| StreamDecode420Restart | 1,218.76 / 991.32 | 1.229 / 1.235 | 761,833 | gap |
| NativeDecodeCancelable8 | 2,188.08 / 1,719.51 | 1.273 / 1.281 | 966,367 | gap |
| DirectRgb422 | 1,385.17 / 1,094.49 | 1.266 / 1.270 | 829,164 | gap |
| DirectRgb420 | 980.85 / 742.86 | 1.320 / 1.335 | 757,363 | gap |
| CmykAdobe | 312.90 / 381.23 | 0.821 / 0.830 | 4,520 | pass-stop |
| CmykAdobeAc | 475.07 / 392.22 | 1.211 / 1.217 | 4,520 | gap |
| CmykAdobeGeneral | 2,448.42 / 3,111.90 | 0.787 / 0.788 | 4,520 | pass-stop |
| CmykAdobeSparse2D | 543.34 / 437.35 | 1.242 / 1.247 | 4,520 | gap |
| Ycck420 | 416.50 / 706.67 | 0.589 / 0.591 | 895,547 | pass-stop |
| CmykCaller | 338.62 / 331.44 | 1.022 / 1.025 | 1,085,959 | gap |
| Ycck420CallerRgb | 428.34 / 604.11 | 0.709 / 0.711 | 762,704 | pass-stop |
| Decode411 | 523.45 / 447.61 | 1.169 / 1.188 | 751,299 | gap |
| Decode410 | 364.80 / 506.86 | 0.720 / 0.735 | 720,838 | pass-stop |
| CallerGray | 626.09 / 646.50 | 0.968 / 0.972 | 273,430 | pass-stop |
| CallerRgb | 1,017.86 / 1,160.18 | 0.877 / 0.881 | 624,511 | pass-stop |
| ValidateJpegBaseline420 | 537.90 / 679.19 | 0.792 / 0.800 | 9,984 | pass-stop |

Nine pass/stop, thirteen gaps and one confidence-open case. Six progressive
paths reuse source-identical executed branches and [current evidence](JPEG-PROGRESSIVE-OUTPUT.md):
1pass/3gaps/2uncertain. Other codecs, encoders, editing and text are unaffected.
The largest refreshed JPEG gap is live-token native444, about469µs; RGB444
retains about391µs. Resolve numerical/ownership boundaries before further tuning.

Windows/Linux each pass5,845 full/live checks (2,580internal/2,317public/948differential),
plus88internal/375public affected checks in normal/scalar modes. New actual-store,
all65,536sample/black pairs, K-first separate scans, dirty result, terminal refusal,
cancellation, retained-survivor/recovery andasync checks pass. The same actual
private0.2.1 package and all three usage examples pass bothOS. Existing consumers
exercise frozen varyingCMYK/native hashes, caller buffers, orientation andstream/async
ownership, plus directRGB MCU storage. Published releases are unchanged; publication
and a full125-row actual-package performance refresh remain postponed.

The [earlier owned-frame](OWNED-CMYK-FRAME-UNQUALIFIED.md) and
[overwrite-allocation](CMYK-OVERWRITE-ALLOCATION-REJECTED.md) studies retain their
inconclusive/adverse results. This coherent implementation combines shared3/4channel
MCU publication with the existing final owner andpool, uses smaller admitted slots,
and has new independent evidence; neither old numerical thresholds nor family closures
apply. The [previous current JPEG owner](WARM-JPEG-CONTAINER-SCAN.md) is historical
for these23cases. The renderer reads this current owner rather than superseded
experiments. Compression correctness and all283 required paths remain the objective.

[Evidence](jpeg-component-frame-evidence.json) pins identities, sample counts,
filtered/all-actual confidence, gates, qualification, storage and reproduction.
Raw `jpeg-component-frame-qualified.tar.xz`, SHA256 `b0c6f24a3f0e349386117ad52c821f70d9a172688fe6affc4421c86ac5258a2f`, 10998368B/465members, is mirrored and member-verified. All actuals, both
stages/original modules/sources, cached input/reference providers and qualification
logs are retained. One setup attempt rejected an old library identity before any
actuals; only that field was rebound, all input/descriptors/scopes/native facts stayed
identical, and only the missing validation case was measured. Completed jobs were
not repeated. No native/harness/selector/diagnostic builds or workload expansion.
