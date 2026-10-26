# Exact adjacent VP8 forward-block batching

The costs below describe the recorded producer. [Shared small-prediction rows](WARM-VP8-SHARED-SMALL-ROWS.md)
supersedes affected VP8 encode/decode calling costs; unchanged arithmetic and
packet/pixel proofs remain valid. Read that report for current measurements.

The existing 513×259 opaque photograph independently confirms27.78 ms,
2.879× reference time / upper2.900×, with unchanged909,696 B/op and exact17,688 B.
Elapsed is 8.6% lower than the preceding quiet short. This is not a paired
independent-baseline gain claim; full CPU parity remains open.

| Quiet one-CPU run | Lokad µs | FFmpeg/libwebp µs | Ratio / upper | Allocated B/op | Both packet bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| PhotographLossy8 short | 27,814.38 | 9,719.47 | 2.862 / 3.018 | 909,696 | 17,688 |
| PhotographLossy8 confirm | 27,777.10 | 9,647.05 | 2.879 / 2.900 | 909,696 | 17,688 |
| DiagramLossy8 short | 15,927.54 | 5,826.94 | 2.733 / 2.762 | 841,608 | 7,100 |
| vp8 short | 86,847.48 | 53,412.61 | 1.626 / 1.733 | 23,358,400 | 98,590 |

Both idle-v2 guards pass for every row; photograph retains all12actual iterations.
Diagram/asymmetric VP8+alpha are current shorts, not independent gain claims.
Complete operations include normalization/import as applicable, source processing,
encoding, fresh output growth/independent ownership and release. No ToArray is
timed. Preparation, validation, I/O, IPC and startup are excluded. Same frozen
settings/reference packets; equal quality numbers are not the acceptance criterion.

Four adjacent4×4 residual blocks share the existing exact four-lane forward
arithmetic. Strided16-byte source/prediction rows are widened before subtraction.
Luma/DC order, rounding, chroma diffusion and quantization remain exact. Separated
coefficient-pair stores preserve U8|V8 order without a reorder buffer. Both inputs
are read completely before publication, including scalar fallback. Invalid row,
stride and pair extents reject before writes. Existing mode checkpoints/admitted
16KiB scratch remain; no heap/frame storage, reservation, owner or lifetime change.

Windows and Linux each pass4,932 Release checks(1,950internal/2,035public/947live),
611scalarWebP and57focused transform/decision checks. Existing native coefficient,
mode, score, probability and reconstruction traces remain exact. New tests cover
6,144batch/output/alias placements against the actual scalar transform, including
extreme/unaligned/strided inputs, compact and separated pairs, overlapping inputs,
untouched holes/tails, rejected geometry and zero warm allocation. The same actual
private0.2.0 package and allthree usage examples pass bothOS;0.1.0 is unchanged.

The intra16-only intermediate screen and exact source/core/test identities are
retained; only the final combined luma/chroma/replay producer is independently and
fully qualified. No new images/settings or benchmark/native/selector rebuild.
Older affected VP8costs are historical; VP8L/JPEG/PNG, decoders/editing/text and valid
unrelated evidence remain unchanged. [Evidence](warm-vp8-adjacent-forward-evidence.json)
records samples, allocations, geometry proof, qualified core/package identities
and the mirrored member-verified raw archive. Stop this primitive round.
The first short-target collection failed when the process exited before EventPipe
Stop. Its raw trace/logs are retained; no phase attribution or timing is accepted.
Production qualification and measured costs above remain valid.

A follow-up reused the prepared longer selector and same qualified core: 706
workload samples, including 101 exclusive SmallBlock samples and 343 inclusive
Luma4 samples. Counts overlap and inlining limits attribution; instrumented timings
are discarded. Source inspection identifies encoder per-pixel geometric dispatch
where the decoder already has verified whole-row formulas. Sharing those formulas
is the next bounded primitive, preserving enum mapping, snapshot/overlap behavior,
rounding, clamping, errors and tails. No memoization or new workload/settings grid.
The evidence retains both profile attempts and their mirrored raw archives.
