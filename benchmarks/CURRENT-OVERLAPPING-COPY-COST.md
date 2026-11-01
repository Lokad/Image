# Current paired overlapping-copy cost

The existing required DrawingCopyOverlap8 boundary has its first independent
warm measurement: **1.106× / conservative upper 1.152×**, **112 B/op**.
The lower bound is 1.073, so this is a confirmed gap.

| Complete paired operation | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op |
|---|---:|---:|---:|---:|
| Overlapping RGBA8 copy + reverse restoration | 4.180 | 3.779 | 1.106 / 1.152 | 112 |

Costs are **per pair**, including two complete public moves on the existing
original 513×259 byte canvas: source (17,19), 256×129, to (127,65), then the
reverse move. Reverse restoration is counted in both elapsed/allocation totals.
The original source rectangle is restored; the pair's full state is stable
after preparation, while each forward move still changes overlapping pixels.
There is no hidden reset, temporary raster, new output or per-single-copy claim.
Managed validation, admission, cancellation and scope cleanup are included.
Startup, preparation, IPC, I/O and verification are excluded equally.

The reference uses public FFmpeg av_image_copy_plane in three disjoint
46-row-or-smaller bands per move, six calls per pair. Bands progress in the
required direction; every call explicitly checks its source/destination byte
intervals are disjoint. This provides snapshot semantics without relying on
undocumented overlapping memcpy behavior or an extra pixel buffer.

One guarded twelve retains all actuals, filtered/unfiltered confidence and twelve
samples in each native bracket. Full forward/final snapshots, alpha, hidden RGB,
untouched pixels, caller ownership and both idle-v2 guards pass. The same portable
correctness consumer checks single/repeated pairs on the frozen Windows/Linux
DLLs with shared admission limited to one canvas and final release to zero.
The source intentionally aliases the mutable canvas; source immutability is
not the contract for this operation.

One bounded production trial processes descending rows in disjoint forward
bands and moves direction selection outside the row loop. Its single short is
4.203 µs: 0.56% higher filtered / 0.07% higher all-actual / 0.09% higher normalized
points, with unchanged allocation and exact pixels. This is no useful gain,
not a confirmed regression. No independent trial confirmation, variants or
companion retiming follows.

The original two source files, four trial-only cases and all 53 saved Linux
binaries are restored byte exactly without rebuilding. Candidate 76 public
normal/scalar checks and restored 72 pass. Windows production and the package
never change. The unchanged 5,378 checks/OS, 948 live comparisons and SAME actual
package/three examples are reused; no full-suite or package proof repeats.

The summary has 110 selected comparisons in three tables. Nine audited paths
remain unmeasured; all 282 required paths remain in scope. This gap stays open,
and the row-band family closes without further tuning.

[Evidence](current-overlapping-copy-cost-evidence.json). Retained
current-overlapping-copy-cost.tar.xz, 1,534,584 B / 122 members,
SHA256 d9581fe3c7adf21ccb2a620d0aa2402754882e6831736187095a1f7c03777aa5,
mirrored and member-verified. Raw contains actual original/rejected producers,
drivers, source, input, worker, protocol/correctness controls, all samples/guards,
restoration records and the retained fill-round qualification dependency.
Reproduce with build-copy-region-overlap-linux.sh,
verify-copy-region-overlap-linux.py and archived immutable commands/selector.
Fresh tooling selects --warm-family CopyOverlap and --warm-cases DrawingCopyOverlap8.
