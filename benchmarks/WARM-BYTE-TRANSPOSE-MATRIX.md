# Exact byte transpose with SSE2

The existing 513×259 RGBA8 transpose improves **31.02% elapsed** (31.03% all-actual /
**32.38% native-normalized**), with exact pixels and unchanged **112 B/op**.
Both raw confidence intervals separate. Normalized candidate upper **0.711** is
below original lower **1.019**. This scoped warm single-CPU path passes and stops.

| Complete 513×259 → 259×513 transpose | Lokad µs | Ratio / upper | Managed B/op | Status |
|---|---:|---:|---:|---|
| Original RGBA8, reused independent12 | 38.855 | 1.022 / 1.024 | 112 | Gap |
| SSE2 RGBA8, independent12 | 26.802 | 0.691 / 0.711 | 112 | Pass / stop |

One safe SSE2 4×4 UInt32 matrix rearranges whole pixels inside the existing 32×32
tiles. Scalar tails and portable fallback remain. Channels, alpha and hidden RGB
are never interpreted; there is no new storage, owner or allocation. Admission,
local bounds and tile cancellation remain. Complete public operations include
output materialization, independent ownership and release on both subjects.
Original inputs/settings, exact managed/native frames and normal/scalar FFmpeg
CLI pixels pass. Native allocation is unmeasured. The native bracket is 2.01%
slower between runs; normalized gain is reported separately.

The original independent twelve is reused: its primitive source hash still
matches, and subsequent JPEG changes do not call it. One short earns one
independent candidate twelve; no retiming, new image, profile or settings grid.
The AVX2 word kernel and common dispatch are unchanged, so its passing cost is
reused. Direct and EXIF transpose call this byte kernel; rotations, mirrored
transpose and normal-orientation stream compositions dispatch separately.

**5,376 checks per OS** pass: 2,176 internal + 2,252 public + 948 live. Focused
**100 public checks pass normal/scalar on both OSes**, including exact placement,
matrix/tile/tail boundaries, hidden RGB, shared refusal, cleanup, recovery and
independent survivors. The **same actual Windows Release package** passes both
OSes and three examples, including 35×37 transpose ownership at both depths.
One core/public-test build per OS; zero internal/differential/harness/native/
selector builds. The immutable measured build serves qualification.

Only the existing byte transpose cost is replaced. The summary retains 105
selected comparisons and all **268 required material paths**; full parity remains
open. Other applicable evidence and published packages are preserved. This is
not a full immutable-package release refresh or a claim about broader geometry.

[Numeric evidence](byte-transpose-matrix-evidence.json) retains actuals, native
brackets, confidence, identities, reuse scope, qualification and actual package.
Raw `byte-transpose-matrix-qualified.tar.xz`, SHA256
77d0915b0879158c2342012dbd9424615a9ff78123ea752faea23c240991cc56 (2,859,056 B / 102 members),
is mirrored and member-verified. It includes source/tests, measured DLLs, cached
drivers/worker, actual package, consumer, logs and reproduction scripts; cited
dependencies retain the original baseline and unchanged qualification.
