# Compact mixed-axis geometry experiment

RGA589/RGC590/RGS591, unselected. Two implementations shrink shared horizontal
geometry from 84 to 64 bytes per column. Seven original doubles and first/count
remain; clamped sample indices are reconstructed during reading. The second
implementation replaces scalar reconstruction with a four-index managed SIMD
clamp and an optional fifth index. It retains one schema and shared execution.

| Complete RGBA16 resize | Qualified original µs | Scalar screen µs | SIMD screen µs |
| --- | ---: | ---: | ---: |
| Width enlargement/height reduction, 4KiB workspace | 987.47 | 1,048.89 | 1,043.00 |
| Same geometry, default workspace | 917.38 | 931.95 | 923.56 |

The existing 513×259→1027×129 inputs, exact managed pixels and approved native
float bounds are unchanged. FFmpeg's explicitly labeled zimg backend retains
graph/scratch preparation; conversion, filtering, independent output and disposal
remain timed. Each screen has three actuals with quiet single-CPU guards. Native
means remain roughly 845–852µs. No useful point gain is demonstrated; short bounds
do not establish an isolated cause or blanket statistical regression. No
independent confirmation, further variant or affected-byte refresh follows.

The smaller plan uses 65,728 instead of 86,268 bytes inside the already owned
output. Its bounded eight-row lifetime extends through row 120 instead of 112,
reducing recomputed tail rows from 17 to nine. The default sixteen-row boundary
stays 112. These counts do not establish elapsed-time savings. Allocations and
admitted capacity do not fall: the output remains 1,059,864B, explicit scratch
3,992/7,596B within unchanged 4/8KiB reservations, with zero retained scratch.
Physical/native peaks are unmeasured.

A simple cross-band overlap cache was ruled out before coding: retaining the
seven overlapping rows across all columns needs 57,456B of premultiplied samples
or 114,912B decoded, beyond the declared workspace. Existing decoded scratch holds
one column pair and is overwritten by certification; it cannot serve that cache.

Windows/Linux each pass 73 internal and 688 public affected checks per normal/
scalar mode for the scalar stage, and 79/688 for SIMD. Six literal edge/duplicate
index controls exercise both depths and guard storage. Existing rational/frozen
pixels, clipping, tiny alpha, cancellation, refusal and surviving owners pass.
Weights, sum order, pixel certificates/repair and every checkpoint are unchanged.

Qualified sources, main/embedded modules and public bindings are restored byte
for byte; prototype-only tests are removed. The previous 5,909 full/live checks
per OS and SAME actual package/three-example qualification remain valid without
rebuild or repetition. Current benchmark costs remain unchanged. These failed
implementations do not close materially revised geometry or scheduling designs.

[Evidence](bicubic-compact-geometry-experiment-evidence.json) retains both stages
and restoration. Raw `bicubic-compact-geometry-experiment.tar.xz`, SHA256
`a32e2297000c19eaeaae4dd5eec116f77fc5cd71692dd6c415829e64d30ff592`,
6,933,272B/170 members, is mirrored and member-verified with its source-valid
attribution and qualification dependencies. Publication remains postponed.
