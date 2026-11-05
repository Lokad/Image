# Shared opaque VP8 input preparation

Opaque byte and word input now share one row-pair driver and the existing exact
luma/chroma kernels. Only two sixteen-pixel word tiles (128 B) are normalized
before immediate use. Gamma/permutation state is prepared once per operation;
no full-frame intermediate, new heap buffer, owner, cache or pool is added.
Scalar/transparent paths, edge replication, rounding, alpha and packets stay exact.

One counterbalanced comparison confirms **826.26 µs / 4.44% elapsed improvement**
on the frozen 513×259 opaque RGBA16 control, Q75/effort4:
18,621.61→17,795.35 µs. Twelve alternating pairs × sixteen complete operations
per subject, all retained; ratio interval **0.95192–0.95935**. Both subjects
allocate 1,201,552 B/op. Necessary normalization, fresh output creation/growth,
complete encoding and independent output/disposal remain timed. Equal resident
source setup, JIT and validation are excluded. This is a scoped managed-build
gain, not FFmpeg parity or a claim across all word inputs/settings.

| Existing complete 513×259 control | Lokad µs | FFmpeg/libwebp µs | Ratio / upper | Managed B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Opaque RGBA16 / normalization | 18,202.78 | 16,486.10 | 1.104 / 1.110 | 1,201,552 | 72,380 |
| Opaque photograph, effort4 | 12,188.05 | 10,181.34 | 1.197 / 1.233 | 809,072 | 17,688 |
| RGBA8 + ALPH | 64,887.87 | 56,328.84 | 1.152 / 1.162 | 7,165,880 | 98,590 |
| RGBA16 + ALPH / normalization | 65,778.97 | 56,503.47 | 1.164 / 1.177 | 7,165,472 | 98,590 |
| Opaque RGBA8 | 18,429.42 | 16,324.18 | 1.129 / 1.136 | 1,201,552 | 72,380 |
| Opaque diagram, effort4 | 7,347.75 | 6,119.72 | 1.201 / 1.208 | 741,137 | 7,100 |
| Diagram, effort2 / distortion | 3,304.45 | 2,702.94 | 1.223 / 1.235 | 741,375 | 7,330 |
| Diagram, effort6 / trellis | 13,826.59 | 10,599.23 | 1.304 / 1.327 | 754,344 | 6,818 |

All eight costs remain gaps. Exact complete packets, source/settings, independent
output, original pixel/quality gates and quiet guards pass. Twelve actuals and
both native brackets are retained per case, including estimator exclusions.
The other117 displayed comparisons reuse unaffected evidence.

The byte photograph's pair is0.23% adverse, interval0.99933–1.00535. A standalone
opaque8 adverse point warrants one targeted comparison on that existing control:
its pair is0.49% faster, with wide interval0.97222–1.01828. Both remain uncertain,
without gain/regression or equivalence claims. Original adverse shorts/current
points are retained; native clocks move. In particular, current word elapsed is
18,202.78µs versus the prior17,971.95µs while native elapsed rises5.59%. That
independent capture is not the source of the paired improvement claim. No further
tuning, favorable sampling or unchanged confirmation follows.

Eligible word preparation charges **4KiB instead of1KiB** before stack use; byte
vector preparation stays4KiB and scalar stays1KiB. State is1088B of gamma/geometry
plus128B of word tiles within that coarse allowance. Shared refusal is terminal,
with no scalar retry. Charges end before preparation returns; retained output
capacity is unchanged. No heap-allocation saving, native allocation or physical/
complete admitted-peak claim is made.

Windows/Linux pass **5,651 full/live checks each**, plus862internal/357public
affected checks normal/scalar and23 input checks with AVX512 disabled. Unaffected
quantizer fallback proof is reused. Nine added checks cover every16-bit sample,
512/256-bit batches/scalar, independent scalar YUV/gamma, tails/edges/padding,
word scratch refusal, output/scratch cancellation, cleanup and surviving results.
The **same actual private Release package and all three examples pass both OSes**;
its new word consumer checks exact sync/async packets, terminal refusal,
cancellation recovery and independent caller-owned output after source disposal.

One immutable main/internal build per OS serves the round. Existing harnesses,
selectors and native workers are reused; one private paired-tool build adds
16-bit inputs. A photograph receipt-key mismatch stops before timing. The completed
word result is reused and only the unstarted photograph job resumes; initial
guard/failed invocation remain. No completed measurement repeats.

The preceding one bounded capture has135of1962samples in input Create/Reduce;
samples/skid are attribution, not phase times or predicted savings. Preparation
is not the largest remaining location. Earlier
[luma4](VP8-LUMA4-PREPARATION.md), [residual](VP8-RESIDUAL-PREPARATION.md) and
[fixed trellis](VP8-TRELLIS-IMPORTANCE.md) gains remain scoped history; their
displayed VP8 costs are superseded here. Other affected H boundaries remain
historical. This is development-source qualification, not a full actual-package
release refresh. The282-path objective stays open; publication is postponed.

[Evidence](vp8-shared-input-preparation-evidence.json) pins all producers, exact
packets, paired confidence, adverse observations and qualification. Reproduce
with retained `wnp309-profile*.py`, `wit310*-checks.py`, `wit310-paired*.py`,
`wit310-refresh*.py`, qualification and package scripts. Full16-bit input hashes
match the frozen recipe. Source/module snapshots and unchanged reference/runtime
dependencies are retained.

Raw `vp8-shared-input-preparation-qualified.tar.xz`: 6,853,780 B / 374 members,
SHA256 `9bba8745139ae79b4f27b9d44f5c69ee1e280dfa106909244ed74bae9416a394`; mirrored/member-verified.
