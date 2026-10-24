# Decoder exactness

[The finite checklist and numeric evidence](decoder-exactness-evidence.json) reuse
original controls and pinned references. Exact normalized samples and typed source
metadata are required; remaining error bounds are transitional functional checks.

| Required path | Current evidence and next action |
|---|---|
| PNG normalization, Adam7, entropy, packed depths and tRNS | Existing frozen/live exact assertions pass; stop at that scope. |
| VP8L transforms, palette, cache and copies | Existing60-fixture/authored/live exact assertions pass; stop at that scope. |
| JPEG inverse and progressive coefficients | Integer primitive and all30 frozen grayscale/YCbCr444/422/420 controls pass. |
| JPEG444/422/420 YCbCr color | Exact frozen/live samples, odd edges, restarts and progressive scans pass on both OSes. |
| JPEG direct-RGB422/420 | Two original513x259 packets pass exact normalized RGBA, callerRGB and callerGRAY; stop. |
| JPEG CMYK/YCCK and411/410 | Named constant/varying/progressive/saturated/AC and quarter-width controls pass at E106/E108 scope. |
| JPEG caller grayscale/RGB/CMYK | Named source/options, padding/orientation, stream and retained-root checks pass; canonical ink contracts retained. |
| VP8 reconstruction and deblocking | Current suites already pass exact YUV over110 native+54 authored fixtures. Reconfirm two named live controls. |
| VP8 chroma/RGB and ALPH | RGB exactness uncertain; retain passing exact-alpha controls. |
| Typed source metadata, orientation and delivery | Existing sync/async, caller-buffer and lifetime checks pass. |

E108 adopts independently derived quarter-width Q16 positions, Q14 horizontal/
Q12 vertical weights, Q7/Q9 filtered chroma and Q13 color.4:1:0 computes its initial
half-grid expansion on demand. No filter table, owned scratch or intermediate
raster is added. Existing4096 block/32-per-pixel row work, admission, cancellation
and ownership remain. Superseded floating renderers/tests and the unused floating
rounding helper are removed.

The original67x19 baseline/progressive, constant/varying controls require exact
pixels; arrays/sync/async and oriented/padded caller RGB/Y cover public boundaries.
One high-contrast410 control at the existing513x259 size isolates near-zero leading-
tap reduction, a distinct required branch found during source inspection. Both
frozen513x259 companion packets also match the existing live-JPEG decode options,
on Windows/Linux with normal/scalar native execution.

Both OSes pass5,044 full checks, including947 live comparisons, plus33 internal/
128 public affected scalar checks. The SAME actual private0.2.0 package passes
both consumers and all3 usage examples. Earlier qualified inverse/YCbCr/CMYK and
encoder/metadata/editing/text paths retain their evidence and pass these suites.

Historical quarter-width timing workers perform additional centered normalization.
Those baselines/raw data remain unchanged and prior-production; the new byte
checks do not prove equivalent timing workloads or a performance pass. Align
current output contracts before future CPU/allocation qualification.

Next, reconfirm one existing31x23 VP8 native packet and one authored filter case
against pinned live planar references, reusing the164 exact frozen controls.
Resolve normalized chroma/RGB precision on those same inputs. No settings/shape
grid or speed-only tuning. Broader decoder equality, warm single-CPU parity and
replacement readiness remain open.

E109 confirms owned RGBA and caller RGB on the two existing513x259 direct-RGB422/
420 packets, preserving approved phase/component normalization and original hashes.
Both OSes and native modes agree exactly. Stop those passing paths. CallerGRAY on
the same packets differs at20,376/18,306 samples by one; the prior constant8x8 gray
control did not isolate fractional component expansion plus grayscale quantization.
E110 adopts the shared rational BT.601 Q15/Q14 precision after normalized8-bit
component rounding. Owned RGBA/caller RGB remain unchanged. No owned heap storage
or RGBA intermediate is added; existing4096 block/32-per-pixel row work, admission,
cancellation and ownership remain. Exact hashes, oriented/padded destinations,
streams and independent input/result/root lifetimes are durable tests. No new
image/settings grid, timing or baseline change. Replaced implementations have no
production compatibility obligation.
