# Shared word matrices for counterclockwise rotation

Historical accepted word stage. [Current shared tile costs](SHARED-TRANSPOSE-TILES.md)
replace its four displayed costs and retain this evidence unchanged.

The existing RGBA16 counterclockwise operation passes warm parity: **39.79 /
FFmpeg48.27µs, upper0.884**, **112B/op**. An independent contemporary baseline
is50.69/native44.78µs. All-actual saving is**10.90µs/21.51% raw**, **27.18%
reference-normalized**; both confidence views separate. The reference clock
moves+7.79%, disclosed separately. Exact complete pixels remain unchanged.

| Original complete513×259→259×513 operation | Lokad / FFmpeg µs | Conservative upper | GC B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Counterclockwise RGBA16 | 39.79 / 48.27 | .884 | 112 | Pass/stop |
| Plain transpose RGBA16 | 39.70 / 48.13 | .925 | 112 | Pass/stop |
| Counterclockwise RGBA8 | 44.77 / 40.87 | 1.147 | 112 | Gap |
| Plain transpose RGBA8 | 30.26 / 40.46 | .770 | 112 | Pass/stop |

One shared AVX2 4×4 UInt64 matrix/tile schedule now serves transpose and the
counterclockwise word turn. Compile-time destination-row order changes placement;
channels, alpha and hidden RGB are never interpreted. Original32×32 checkpoints,
scalar tails/fallbacks, byte kernels, admission, owners and cleanup remain. There
is no extra image, pass, workspace, reservation, pool or public API.

The changed shared forward body is checked against a contemporary baseline:
39.21→39.70µs, raw+1.27%/normalized+.375%, both intervals overlap; no demonstrated
regression. Earlier35.36µs is historical and its native clock differs. Byte raw
historical costs rise12.22%/8.71%, with native movement+8.83%/+6.47% and overlapping
normalized bounds. Those historical snapshots establish no causal byte effect.
The byte turn's existing gap remains visible; there is no byte optimization claim.

Every timed operation includes complete public admission/allocation/materialization/
disposal and an independent output. The public FFmpeg transpose reference retains
graph/source/frame wrappers; output allocation/processing/release is timed and padded
rows need no redundant packing copy. Original frozen inputs/settings, exact managed
and normal/scalar CLI pixels, source preservation, independent survivors and idle-v2
guards pass on quietCPU0. A cheap existing12×64operation adjacent screen precedes
one independent primary capture; every pair and three native brackets are retained.
The same immutable build supplies all four current costs. A targeted forward baseline
resolves a historical raw-cost discrepancy; there is no favorable sampling repeat,
new fixture/grid or instrumented timing substitution.

Allocations, ownership and retention stay distinct. The primary borrows1,062,936B
input and owns1,062,936B output, with zero additional scratch or retained operation
state. Caller-governed storage bypasses the unchanged two-array/4MiB idle pool.
The35×37 ownership control peaks at31,080B for source plus two results. Physical,
native and JIT peaks are unmeasured; no capacity saving is claimed.

**6,580full/live checks perOS** pass:3,236internal/2,396public/948live. Focused
13internal/241public checks pass normal/scalar on bothOSes. Seven new public cases
cover4×4/32×32 boundaries and full64bit payload, hiddenRGB, terminal shared refusal,
admission-triggered cancellation cleanup and independently surviving outputs. Full
encoder packets/decoder/editing tests stay exact. Unchanged resize/codec/text/ISA
evidence is reused. One core/embedded/public build perOS; no harness/native/fixture
build. No production rebuild after timing.

The **same actual Release package** contains the timed Linux DLL and passes bothOS
consumers/all3examples, including the new exact-word rotation/refusal/cancellation/
survivor check. Pack is Release/--no-build directly to artifacts/nugets, once. Published
packages remain immutable; publication is postponed. The191current owner views now
have165passes/10gaps/16uncertainties. All289required paths remain; these are scoped
results, not full-library parity or a release-wide benchmark refresh.

[Evidence](counterclockwise-word-matrix-evidence.json) retains original identities,
complete boundaries, all samples/confidence, adverse/inconclusive companion views,
sources, immutable producers, qualification, package and replay recipes. Raw
`counterclockwise-word-matrix-qualified.tar.xz`, SHA256`86bb02c751ffe5a2d66cbe8be7ac2be05a3f6993fd567f208a5776ffc7a8b217`
(2,440,216B/132members), is mirrored/member-verified. Its
dependency preserves the preceding actual package and unchanged evidence.
