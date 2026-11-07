# JPEG plain-reader publication

Two noncancellable baseline entropy loops now publish their canonical cursor at
one ordinary exit. Partial blocks, EOB, long-code and marker/stuffing fallbacks
retain their exact cursor, predictor, coefficients and next-read behavior. The
production boundary supplies validated tables and full blocks; expected syntax
errors remain in canonical fallback. Cancellable and progressive readers are unchanged.

The frozen 513×259 full-grid RGB complete decode improves **1,856.71 → 1,801.70 µs**:
**55.01 µs / 2.96% raw / 2.94% native-normalized**. Both filtered and all-actual
raw/normalized intervals separate; native changes −0.020%. Allocations stay
10,088 B/op. Current ratio/upper **1.241 / 1.263** remains a gap. This is scoped
incremental evidence. Ordinary native444 also saves89.47µs/4.66%; RGB422/420
save36.91/32.69µs, each with separated raw/normalized confidence views. Native422,
owned grayscale and caller CMYK now pass and stop. No full-library parity claim.

**Tradeoff:** live-token decode regresses2,040.63→2,063.20µs:22.57µs/1.11%raw,
1.29%normalized, with separated loss intervals in both views. Allocation stays
10,088B/op; native moves−0.177%. A caller bypass did not demonstrate repair in
two short screens (live2,092.61/RGB1,809.17µs; wide intervals). Reject that
implementation without confirmation. Preserve its sources/modules/results and
restore the verified final build byte-exactly, without rebuild or repeated tests.
The tradeoff is accepted alongside scoped ordinary gains and simplification;
the live gap and its CPU cause remain open. No claim of improvement across JPEG.

Parsing, preparation, entropy, inverse transform, publication and independent
output acquisition/disposal stay timed. FFmpeg retains codec/conversion/input/frame
wrappers as declared. No new cache, pool, owner, quota or numerical policy. Encoder
packets, source metadata, managed pixels, cancellation and admission remain unchanged.

The earlier [AC-only baseline420 screen](JPEG-BASELINE-AC-PUBLICATION-REJECTED.md)
was inconclusive and stopped under the retired fixed5% continuation rule. This
round targets both current fused/plain loops on a later implementation, motivated
by current profile evidence and judged by independent complete-operation results.

## Current JPEG controls

Twenty-three affected baseline controls refresh on one immutable build; six
unchanged progressive-path confirmations are reused. Each has twelve actuals and
twelve samples per native bracket. Passing cases stop.

| Case | Lokad / FFmpeg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| DirectRgb444 | 1,801.70 / 1,451.29 | 1.241 / 1.263 | 10,088 | gap |
| Decode8 | 1,660.75 / 1,903.44 | 0.873 / 0.880 | 4,304 | pass-stop |
| NativeDecode8 | 1,831.14 / 1,717.89 | 1.066 / 1.084 | 10,088 | gap |
| NativeDecode420 | 1,021.16 / 1,008.42 | 1.013 / 1.021 | 757,363 | gap |
| NativeDecode422 | 1,314.27 / 1,349.76 | 0.974 / 0.981 | 829,176 | pass-stop |
| OwnedDecodeGray | 678.67 / 682.63 | 0.994 / 0.999 | 672,202 | pass-stop |
| ProgressiveGray (reused) | 1,897.32 / 1,710.22 | 1.109 / 1.115 | 683,136 | gap |
| Progressive420 (reused) | 1,733.75 / 1,951.39 | 0.888 / 0.893 | 775,323 | pass-stop |
| Progressive444 (reused) | 3,490.27 / 3,361.16 | 1.038 / 1.046 | 169,584 | gap |
| Progressive422 (reused) | 2,213.98 / 2,251.17 | 0.983 / 0.991 | 847,116 | pass-stop |
| NativeDecode420Restart | 1,191.77 / 1,013.00 | 1.176 / 1.182 | 761,737 | gap |
| StreamDecode420Restart | 1,191.48 / 992.18 | 1.201 / 1.207 | 761,833 | gap |
| NativeDecodeCancelable8 | 2,063.20 / 1,717.75 | 1.201 / 1.207 | 10,088 | gap |
| ProgressiveCancelable444 (reused) | 3,535.19 / 3,355.01 | 1.054 / 1.058 | 169,584 | gap |
| DirectRgb422 | 1,345.96 / 1,093.92 | 1.230 / 1.235 | 829,176 | gap |
| DirectRgb420 | 949.55 / 742.11 | 1.280 / 1.296 | 757,363 | gap |
| CmykAdobe | 308.64 / 380.89 | 0.810 / 0.815 | 4,520 | pass-stop |
| CmykAdobeAc | 473.42 / 392.31 | 1.207 / 1.214 | 4,520 | gap |
| CmykAdobeGeneral | 2,368.66 / 3,111.35 | 0.761 / 0.766 | 4,520 | pass-stop |
| CmykAdobeSparse2D | 534.10 / 436.82 | 1.223 / 1.228 | 4,520 | gap |
| Ycck420 | 426.21 / 707.10 | 0.603 / 0.604 | 895,547 | pass-stop |
| CmykCaller | 328.75 / 333.98 | 0.984 / 0.989 | 1,085,966 | pass-stop |
| Ycck420CallerRgb | 430.02 / 604.32 | 0.712 / 0.714 | 762,704 | pass-stop |
| Decode411 | 517.88 / 447.25 | 1.158 / 1.175 | 751,299 | gap |
| Decode410 | 362.10 / 507.51 | 0.713 / 0.719 | 720,838 | pass-stop |
| CallerGray | 597.78 / 646.46 | 0.925 / 0.932 | 273,428 | pass-stop |
| CallerRgb | 996.92 / 1,158.61 | 0.860 / 0.864 | 624,511 | pass-stop |
| ValidateJpegBaseline420 | 502.50 / 674.08 | 0.745 / 0.750 | 9,984 | pass-stop |
| ValidateJpegProgressive420 (reused) | 271.02 / 268.34 | 1.010 / 1.019 | 4,072 | gap |

Current status: {'confirmed-pass-stop': 14, 'confirmed-gap': 15, 'confirmed-confidence-open': 0}.

Windows/Linux each pass **5,850 full/live checks** (2,585 internal, 2,317 public,
948 differential), plus108 internal/375 public affected checks in normal/scalar
modes. Existing category/position/sign, malformed syntax/partial state, EOB,
stuffing/markers, predictor overflow, restart, cancellation, ownership and async
coverage exercises the actual production readers. One core/embedded build per source epoch/OS; two epochs, finalA reused.
public/harness/native/selector/fixture inputs are reused. The **same actual Release
package and all three usage examples** pass both OSes. Publication remains postponed.

Substantial storage/lifetimes are unchanged from [full-grid output ownership](JPEG-FULL-GRID-OUTPUT.md):
governed native444 peak541,253B, retained output531,468B, governed pool bypass;
default retained capacity stays within the existing4MiB pool. These are reused
capacity facts, not newly measured physical/native peaks or governed CPU costs.

[Evidence](jpeg-plain-reader-evidence.json) pins producer identities, cases,
qualification, source proof and reproduction. [Full-grid ownership](JPEG-FULL-GRID-OUTPUT.md)
retains earlier paired gains, adverse intermediate results and source-specific
history; affected current costs are superseded here. Other families reuse valid evidence.

Raw `jpeg-plain-reader-qualified.tar.xz`, SHA256
`17a0f0c611c43fa315f1642062c6aa49488a2bd9518e36dec6492ae3440e3f40`, 17613968B/503members,
is mirrored and member-verified, with referenced input/history providers retained.
All283 material paths remain in scope; this is not a release-wide timing refresh.

## Architectural follow-up

A two-second current-DLL profile retains **1,741 leaf samples**: fused baseline
reader 690, plain AC 145, inverse AVX2 320. CoreLib/unresolved 170 and other runtime
frames remain included. Sampling starts after the existing short driver's pilot
begins and includes warmup; these are attribution leads, not phase times or
predicted savings. The operation uses CPU0; diagnostic profiler affinity is
inherited. No elapsed-time claim follows. Frozen input/output, native settings,
source, ownership and quiet-process checks pass. Zero builds or production changes;
the costs above and later correctness/package qualification remain valid.

Source/JIT review selects a **shared high-aligned local bit reservoir** for the
next bounded experiment. The existing right-aligned loops repeatedly derive
prefix/amplitude shifts from the changing pending count and reconstruct signed
thresholds. Target one reservoir convention across fused DC/AC, plain AC and
live-token AC, retaining exact canonical cursor publication at every existing
exit, slow readers, malformed-input behavior and cancellation checkpoints. No
scan copy, widened table, new owner or cross-operation state is proposed.
FFmpeg's `decode_block` and `ff_mjpeg_unescape_sos` supply strategy insight only:
its owned unescaped scan and retained context do not justify excluding managed
preparation or copying full scans. No upstream implementation is copied.

Use the existing DirectRgb444, NativeDecodeCancelable8 and CmykAdobeSparse2D
complete controls. Judge elapsed time, allocations and simplification together;
preserve adverse/inconclusive results and confirm a promising completed stage
independently. This is a materially different design from the unselected
[prepared-amplitude nibble](JPEG-PREPARED-AMPLITUDE-EXPERIMENT.md), without a
performance claim or merge authorization. An independent integer identity checks
all 4,094 amplitude fields in categories 1–11; production tests still govern actual
cursor, coefficient, marker/restart, failure and ownership behavior.

Raw `jpeg-current-entropy-architecture.tar.xz`, SHA256
`ed873b12e8ddadde0ca05a64f9f282a0ea2a56e166307917780770c362d747f8`,
779,220 B / 32 members, is mirrored/member-verified. It retains current producers,
source/JIT/map/samples, gates, scripts, the post-processing mode correction and
the bounded design, with pinned dependencies on this reader's qualification and
shared-word preparation. No completed profile or timing job was repeated.
