# Column vertical traversal study

CVQ754 tests sharing vertical row/weight traversal between two adjacent column
pairs. Both implementations preserve independent original-order sums, source
sample loads, certificates/repair and ownership. The finite model reduces logical
row/weight visits132,736→66,368; complete measurements do not show a useful gain.

| Complete513×259→256×129 RGBA8 public resize / zimg,three-actual screens | Managed µs | Native µs | Ratio / upper | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Qualified original | 494.43 | 417.62 | 1.184 / 1.255 | 113 |
| Separate leaf | 750.53 | 415.75 | 1.805 / 1.845 | 113 |
| Inline revision | 496.44 | 416.77 | 1.191 / 1.260 | 113 |

The separate253B leaf keeps its sums in registers, but thousands of calls and
wide result transfers make the complete operation substantially slower. A revised
inline implementation removes those calls and restores the cost near the original,
without a useful screen gain. The overlapping intervals establish no equivalence
or causal small regression. No12-actual confirmation or favorable repeated sampling.
Neither result replaces the source-valid504.20/native419.02µs accepted cost.

Exact frozen/rational managed pixels, original native max/RMSE bounds, source/
output ownership and quiet single-CPU idle-v2 gates pass. Both variants pass
330internal/729public focused checks normal/scalar on Windows/Linux. A new actual
helper test checks72 stride/slot/count/edge combinations against separately rounded
scalar bits, poisoned borrowed guards and zero allocation. Emitted code confirms
ordered separate multiply/add, word-mode erasure and inlining in the revision.
Both retain4KiB admission, existing stack/result loans,113B/op, independent output
and scratchretention0. Physical/native/JIT-stack peaks are unmeasured.

Complete managed geometry/preparation, scratch and owned output/disposal stay
timed. The labeled zimg3.0.6 FFmpeg zscale backend retains its established graph/
input/scratch and halo/conversion/filter/packing boundary. No numerical relaxation,
new workload grid, dependency, array, cache/session, root or owner is introduced.

Both production variants and the tests solely for their helper are removed.
The qualified source, main/embedded modules and public bindings are restored
byte-for-byte on both OSes; the VM source manifest also verifies. Current PRR752
6155full/live perOS,329internal/729public scalar/ISA and SAME-package/allthree
examples remain valid. No full/package qualification is repeated for restored
inputs; the package and published releases are unchanged. No obsolete helper,
API wrapper or benchmark-renderer dependency remains. CIe4bc4de2/42774508302 is green.

[Attribution](BOUNDED-COLUMN-CURRENT-ATTRIBUTION.md) and this study remain evidence,
not a permanent ban on revised designs. The next lead shares duplicated byte
certificate completion between narrow/wide publishers and reuses final rounding
for uncertainty checks. Existing error bounds, exact repair decisions, pixels and
ownership must remain; model/code size alone will not prove a CPU benefit.

[Evidence](column-vertical-quad-study-evidence.json) retains adverse screens,
immutable original/two candidate modules, source, code and restoration receipts.
Mirrored/member-verified raw `tmp/column-vertical-quad-study.tar.xz`, SHA256
`3d861a90a2a00de71298608e3459ccf4e8bc3e53cffff033ceeee5bbbb9f14ff`,7,734,208bytes/165members, preserves
reproducibility. The283-path performance objective remains open; publication is
postponed and independent codec/resizing/text work remains eligible.
