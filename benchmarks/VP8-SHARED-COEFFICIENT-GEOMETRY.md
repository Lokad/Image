# Shared VP8 coefficient geometry

One independently derived16byte immutable module mapping replaces four identical
private coefficient-band switches in trellis, coefficient emission, token
statistics and rate scoring, including the SIMD partial's row preparation.
The inlined helper preserves the exact map for positions0..15 and default6for
every other Int32. All four old implementations are removed; no forwarding
wrapper, dynamic cache, owner, pool, reservation or generic pixel-loop layer is
added. Geometry follows the owned RFC6386section13 interpretation; no upstream
implementation table or source is copied.

Independent counterbalanced complete operations confirm:

- **Diagram Q75/effort6:912.94µs /7.42%**,12,295.28→11,382.34µs;
  ratio.925753, interval[.922897,.928612],754,344B/op on both builds.
- **Photograph Q75/effort4:319.40µs /2.70%**,11,848.18→11,528.77µs;
  ratio.973048, interval[.969279,.976822],809,072B/op on both builds.

Each confirmation retains12alternating pairs ×16complete public encodes,
following separate three-pair screens. Exact packets remain6,818and17,688B.
Equal resident-source setup/JIT/checks are excluded; source processing, public
EncodeWebp/admission, fresh output growth, independent output/disposal stay timed
on one quiet pinned CPU. These are scoped incremental managed-build gains.

Current VZS generated code confirms three out-of-line band calls in the original
Search. The shared mapping inlines: Search shrinks2541→2436B and stack-pointer
subtraction424→408B; the72Bzero router is unchanged. Old Band source is identical
across the zero-router stage, so the earlier72samples remain an unchanged-source
lead. No new profile/build is used for this audit. Code size and source counts
do not establish component times, savings or total stack peaks.

| Current frozen complete513×259 encode | Managed µs | FFmpeg/libwebp µs | Ratio / upper | B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opaque RGBA8, effort4 | 16,030.23 | 15,414.86 | 1.040 / 1.045 | 1,201,552 | 72,380 |
| Opaque RGBA16, normalization | 16,246.16 | 15,620.69 | 1.040 / 1.043 | 1,201,552 | 72,380 |
| Opaque photograph, effort4 | 11,731.38 | 9,706.75 | 1.209 / 1.211 | 809,225 | 17,688 |
| Opaque text/diagram, effort4 | 6,907.19 | 5,817.28 | 1.187 / 1.195 | 741,137 | 7,100 |
| Diagram, effort2/distortion | 2,991.60 | 2,518.91 | 1.188 / 1.195 | 741,386 | 7,330 |
| Diagram, effort6/trellis | 11,494.40 | 9,802.99 | 1.173 / 1.178 | 754,497 | 6,818 |
| RGBA8 + lossless alpha, effort4 | 55,294.64 | 53,437.28 | 1.035 / 1.043 | 7,034,392 | 98,590 |
| RGBA16 + alpha, normalization | 55,767.70 | 53,407.08 | 1.044 / 1.055 | 7,033,984 | 98,590 |

All eight remain demonstrated CPU gaps. Every original input/settings, complete
packet/pixel/alpha/quality and source/output/ownership gate passes. Each comparison
retains12actuals, filtered and all-actual confidence, both native brackets and
quiet guards. Native wrappers/private scratch retain their stated boundary;
normalization/import, every pixel and independent packet ownership stay timed.
Other six refreshed costs are current observations, not independently proved
incremental gains. Other117displayed paths reuse unchanged evidence. Six current
VP8L rows retain their existing four passes/two uncertain cases; no new image,
quality or shape matrix is introduced. Full282-path parity remains open.

Total paired allocations are unchanged;16bytes of format module data are added,
with zero new owned heap capacity or retained operation scratch. Existing
admitted capacities/lifetimes, direct cancellation and terminal refusal stay.
Report actual standalone allocation readings, including photograph/trellis
809225/754497B/op; their batching/GC measurement differs from the paired driver.
No library allocation gain/regression, physical/admitted peak or native allocation
is inferred. No resampling follows a diagnostic-allocation difference.

One immutable main/embedded build perOS passes **5,716full/live checks each**
(2,479internal/2,289public/948differential),62internal/137public focused normal/
scalar checks, and SAMEactual private0.2.1Release package/all3examples bothOS.
Existing independent probability/rate, scalar/SIMD residual, token, trellis and
native full-packet oracles cover behavior, including boundaries, custom weights,
extreme lambda/DC/tails, zero/nonzero disposal/cancellation, shared admission,
async cleanup and independently owned surviving outputs. No tests merely mirror
the literal mapping. Unchanged54AVX512-off/JPEG/wide proofs are reused. Published
packages are immutable; publication/full release refresh remain postponed.

Current rendering uses one qualified owner for all eight VP8 costs, replacing
superseded alpha/trellis row overrides. The
[zero-router gain](VP8-ZERO-ROUTE-SEARCH.md),
[earlier shared-alpha costs](VP8-SHARED-ALPHA-PREPARATION.md),
[green-predictor stage](WEBP-ISOLATED-GREEN-PREDICTION.md) and
[inconclusive matcher entry](WEBP-PAIRED-MATCH-ENTRY-INCONCLUSIVE.md) remain history
with cited raw retained, rather than extra current-row rendering dependencies.
[Evidence](vp8-shared-coefficient-geometry-evidence.json) pins all sources/modules,
observations/gates/commands, original/selected versions and actual package proof.
Raw `vp8-shared-coefficient-geometry-qualified.tar.xz`: 5,995,152B/282members, SHA256
`f8d9bd1f0e3766874f09fa1322b87ed2bf1c651ecbd7ad5f2c3a37f6c8a7f6d2`, mirrored/member-verified.
