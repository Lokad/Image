# Completed header skips

Completing buffered or seekable skips without an async state machine improves the
existing seekable JPEG dimension probe by **8.25% filtered / 8.27% all samples /
8.73% native-normalized**, with separated confidence intervals. It now confirms
**0.326 µs, 1.681× / upper 1.736, 744 B/op**, still a gap. The ordinary 5% gain
gate passes; no exception is required.

The original independent JMR1 observation is reused after twelve relevant source
pins match and one fresh original short is within −1.35% raw. One candidate short
and one independent twelve establish the gain. No input/settings grid, variants,
profiles or repeated successful scores. Complete fresh seekable input, bounded
prefix acquisition and fresh owned nine-field descriptor boundaries remain.

Two affected companions each receive one independent twelve:

- Async nonseekable JPEG probe: **0.404 µs, 2.007× / upper 2.052, 744 B/op**.
  It is **1.74% filtered / 1.70% all-sample / 1.60% normalized** slower by point
  estimate than JMR1. Filtered intervals narrowly separate; all-sample intervals
  overlap. This small cost is disclosed alongside the primary gain, without an
  exact no-regression or companion speedup claim.
- Complete seekable PNG8 headers: **0.703 µs, 1.215× / upper 1.235,
  8,528 B/op**. The earlier short is superseded as current cost; no paired gain
  is claimed. The same frozen 218,775-byte PNG, bounded 8,448-byte workspace,
  opaque IDAT seek skips, EOF, typed metadata and native adapter remain.

Only the completed skip wrapper changes. The actual forward-read loop is moved
unchanged into an awaited helper. Validation, cancellation, seeks, acquisition
counts, byte counters and cleanup order remain. There is no new allocation,
read-ahead, owner or charge. Real pending users are awaited before storage release.
All twelve read-boundary controls pass on both original/candidate cores. All 21
existing PNG-header controls agree managed/native in normal/scalar modes. An
initial untimed setup treated an already nine-field proof as six fields; the
failure is retained, with no fixture or gate change.

Both Windows and Linux pass **5,332 checks**: 2,170 internal, 2,214 public and
948 live FFmpeg comparisons. The 104 affected public checks pass normal/scalar
on each OS. One immutable core build and one accepted internal assembly build
per OS; zero public/differential, harness, native or selector builds. The SAME
actual Windows Release package and three examples pass both OSes, including
pending-I/O, refusal, cancellation, retained outputs and independent ownership.
Package `dfa0e669b84872990e01a0e65f21cdd87753b802859be904ac592945cfd49425` is 388,802 B; published releases are unchanged.

Only skip-executing Probe/InspectHeaders costs are invalidated. Frozen PNG and
WebP dimension controls return before a skip. Identify, pixel validation,
codecs, editing and fonts bypass this reader and retain applicable unchanged
proof. Other affected named header/orientation costs need current evidence before
pass claims. Close skip-state variants and continue the full **268-path ≤1×**
objective; these three controls remain confirmed gaps.

[Numeric evidence](warm-header-skip-fast-path-evidence.json) retains actuals,
confidence, source pins, companion cost and qualification. Mirrored/member-verified
`header-skip-fast-path-qualified.tar.xz`, SHA256
`b09a04c6cba2c31a25b1eb9ef8f80e60f2c9dd0c099371676b6b6003f753df09` (5,245,952 B / 394 members),
contains original/candidate cores/source, cached drivers/workers, fixtures,
controls, guards, logs, actual package and reproduction scripts. JFN1/JMR1 raw
dependencies remain retained. Native allocation is unmeasured; managed B/op
includes harness costs. Only quiet-VM single-CPU warm costs are reported.
