# Shared ordinary VP8 block preparation

Prepare the existing five quantizer operands once per independent 8- or 16-block
luma/chroma sequence. Five plane-level callers share one executor; the existing
single-block arithmetic, prepared publication and scalar/AVX2 fallbacks remain
source-exact. This extends preparation beyond the already prepared Luma4 candidate
path; it does not replay paired-mode quantization or packed trellis workspace.

Complete effort5 encoding independently saves **600.76µs / 8.14% raw**,
7,384.12→6,783.36µs. Both filtered/all-actual raw confidence intervals separate.
The normalized point gain is9.59%, with overlapping confidence; native point
movement is+1.61%. Allocation remains741,065B/op and the exact7,026Bpacket is
unchanged. The initial short saves858.09µs/11.62%; it is a lead, not the accepted
gain estimate. No unchanged favorable repeat follows.

Default diagram now passes actual FFmpeg parity, upper0.97467. All nine existing
controls refresh below: **3pass,3gaps,3uncertain**. Other rows establish current
costs and status, not independent incremental gains.

| Complete frozen encode | Managed µs | FFmpeg/libwebp µs | Ratio / upper | B/op | Exact packet B, both | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Opaque RGBA8, effort4 | 14,204.14 | 15,474.54 | 0.918 / 0.924 | 1,201,669 | 72,380 | pass |
| Opaque RGBA16, normalization | 14,073.31 | 15,604.57 | 0.902 / 0.922 | 1,201,552 | 72,380 | pass |
| Photograph, effort4 | 10,936.02 | 9,728.01 | 1.124 / 1.152 | 809,225 | 17,688 | gap |
| Diagram, effort4 | 5,630.07 | 5,831.58 | 0.965 / 0.975 | 741,222 | 7,100 | pass |
| Diagram, effort5 selected trellis | 6,783.36 | 6,556.45 | 1.035 / 1.083 | 741,065 | 7,026 | uncertain |
| Diagram, effort6 candidate trellis | 11,433.47 | 10,498.01 | 1.089 / 1.135 | 754,497 | 6,818 | gap |
| Diagram, effort2 distortion | 3,139.96 | 2,702.54 | 1.162 / 1.169 | 741,375 | 7,330 | gap |
| RGBA8 + lossless alpha | 57,478.19 | 57,847.55 | 0.994 / 1.003 | 7,034,392 | 98,590 | uncertain |
| RGBA16 + alpha, normalization | 53,575.70 | 54,318.21 | 0.986 / 1.021 | 7,033,984 | 98,590 | uncertain |

Photograph standalone confirmation is inconclusive: candidate+295.31µs/2.78%raw,
normalized+3.12%point, with both intervals overlapping against a contemporary
original. A targeted existing paired-DLL comparison finds a scoped5.87%gain,
ratio interval[0.935455,0.947210]. That narrower result is not a claim that the
standalone photograph operation improved. Its actual native gap remains open.

Effort6 standalone raw+603.23µs/5.57% has separated raw intervals but coincides
with native+5.65%; normalized−0.072%point confidence overlaps. One targeted
paired comparison is inconclusive: +0.08%, interval[0.994371,1.007324]. It
corroborates clock attribution without proving equivalence or native parity.
Each paired diagnostic retains12alternating pairs×16complete encodes per subject,
with its own immutable module inputs and guards. It resolves a concrete adverse
point; it does not replace the standalone cost or authorize favorable resampling.

Every standalone row retains12actuals, filtered/all confidence and both native
brackets on quiet pinned CPU0. Normalization, preparation, public admission, fresh
output growth, independent ownership and disposal remain timed. Source fixtures,
sampling/settings, native workers and complete packets/pixels/alpha stay frozen.
Native wrappers/private scratch retain their declared lifetimes. No new image,
settings grid, harness, worker or selector build is needed.

Only independent block publication moves before reconstruction within disjoint
private planes. Walsh/DC diffusion, mode order, pruning, strict ties, rate refresh,
prediction and transform/reconstruction arithmetic stay unchanged. Every existing
per-mode cancellation checkpoint remains; no partial candidate escapes. The matrix
is borrowed for the synchronous sequence, with no new cache, session, owner, pool
or admission model. The same single-block kernel serves portable fallbacks.

Prepared operands occupy320logical stack bytes in the existing16KiBmacroblock
admission. There is no new heap allocation site or retained capacity. Report
complete GC totals separately: eight totals are unchanged; distortion falls
741,386→741,375B/op, a11BGC/batch difference. Paired totals have their own
boundary. Physical/admitted peak and native allocations are not newly measured;
fewer operand loads alone are not a CPU or peak-memory measurement.

One immutable main/embedded build per OS passes **5,744 full/live checks**
(2,491internal/2,305public/948differential), plus155internal/137public each in
normal, scalar, AVX2-disabled and AVX512-disabled modes. Two bounded sequence
controls cover mixed zero/signed blocks, nonzero bit order, guarded prefixes/tails
and zero allocation against the independently scalar/native-qualified kernel.
Existing native coefficient/golden packets, cancellation/refusal, async/shared
admission and surviving output ownership remain covered. The **same actual**
private0.2.1Release package and all3usage examples pass both OSes, including all22
original VP8 tuples through sync/async APIs. Published packages remain immutable.

Package a58cfb8a247cf0cbae899ca086277b997f848b94c66a2fd45b0a150e3c9f1913 (418,644B); Windows core
`84d6f2c549636dd7c42ac07330ae1821207b12b61e1840be01ca1193c544fcdb`; Linux core
`4eed4378d8e38e6647b331c81cba2a31fdcdd27c1423280837e7f9a5f41878a3`.

Only the nine named lossy-encode costs are invalidated. Relevant lossless/alpha
subcodec, decoder, PNG/JPEG, resizing, text/drawing, stream/inspection and ownership
paths remain unchanged and reuse valid evidence. Stop the passing diagram/opaque
cases. Effort5/alpha uncertainties remain explicit. A current actual photograph
gap may justify fresh attribution after this changed execution path; do not infer
new phase times or replay the failed DP/paired-mode designs blindly.

[Evidence](vp8-ordinary-block-preparation-evidence.json) pins all sources/modules,
actuals/brackets/guards, focused/full/package checks and adverse paired/standalone
points. Earlier [DPworkspace audit](VP8-TRELLIS-ARCHITECTURE-AUDIT.md),
[operand preparation](VP8-QUANTIZER-OPERAND-PREPARATION.md) and
[prediction preparation](VP8-COMPLETE-PREDICTION-PREPARATION.md) retain history.

Raw `vp8-ordinary-block-preparation-qualified.tar.xz`: 2,695,076B/267members,
SHA256 `0d3dfc7b21f6f3b7fb3b546b367ae3fb270615ad4d3264f4de1b077a807c987f`, mirrored/member-verified;
commands, source/binary snapshots and hash-addressed dependencies are retained.
Full283-path parity, a125-row actual-package release refresh and publication
remain separate outstanding work.
