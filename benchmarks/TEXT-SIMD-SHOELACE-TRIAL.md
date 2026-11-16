# SIMD text quadrilateral trial

**Not retained.** Four independent shoelace cross-products use AVX multiply and
subtract, followed by the original ordered scalar additions. The original
scalar fallback remains verbatim. No fused operations, horizontal reduction,
clipping/event changes, storage, owner, cache, API or numerical-policy change.

| Twelve adjacent managed DLL pairs | Ordinary RGBA16 µs | Public GC B/op |
| --- | ---: | ---: |
| Qualified producer | 55.636587 | 83,136 |
| SIMD quadrilateral terms | 57.189452 | 83,136 |

The candidate costs **2.79% elapsed**, paired interval **1.015922–1.040101**;
all-actual mean intervals also separate. No specific code-generation cause is
established. Exact pixels alone do not establish a performance improvement.

The unchanged helper uses original public ordinary drawing in two independent
processes: twelve alternating 1,024-operation pairs, 1,024 warmups each, one CPU0
operation at a time and natural nonconcurrent GC. Reset, validation, IPC and
allocation reads remain outside each operation's two clocks. Every line is
durably captured before statistics and all pairs are retained. The timing
adapter adds 24 B on both sides. Owned/retained capacity is unchanged; physical,
native/JIT peaks and affected complete-pipeline costs are unmeasured.

ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12 checks run without
reference timing. This is a scoped DLL diagnostic, not native parity or evidence
of an overall text-processing slowdown. The qualified complete one-shot and
eight-draw integrations remain passing after exact restoration; historical
stage timings are never added to infer end-to-end performance.

Linux passes 266 internal / 163 public checks and all eight frozen full-word
frames in normal, AVX-disabled and scalar modes. The always-clipped oracle covers
30,736 finite/generated and 4,096 special cases with poisoned scratch and canaries.
Admission refusal, cancellation, owner cleanup and recovery remain covered.

Source, tests, core/embedded/public modules and source inventory are exactly
restored without rebuilding. Windows modules and the actual package never change.
Reuse 6,730 full/live checks per OS and the SAME actual-package/three-example
proofs. One Linux core/embedded build; zero other builds. No independent timing
confirmation, broader qualification or repack follows this adverse screen.

[Evidence](text-simd-shoelace-trial-evidence.json) pins observations and restoration.
Raw `text-simd-shoelace-trial.tar.xz` (1,818,216 B / 99 members), SHA256
`bbced1ca482eb7bf858b2e79e326477b0ffa70eaf50f3521002e6d381355a5f5`, is mirrored and member-verified with its qualified-baseline
dependency. The current-cost renderer does not depend on this rejected trial.
The full 289-path objective remains open; publication stays postponed.
