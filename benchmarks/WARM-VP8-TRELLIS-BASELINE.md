# Effort-6 WebP trellis warm baseline

The existing diagram at effort6 exposes the coefficient-domain dynamic-programming
trellis algorithm absent from the effort4 warm controls. It adds one required
named path to the finite checklist (254 total), with no new image/depth/quality
grid. Complete source/settings and the frozen 6,818-byte packet are unchanged.

| DiagramLossyTrellis8, Q75 / effort6 | Lokad µs | FFmpeg/libwebp µs | Ratio / upper | Allocated B/op | Both packet bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| Current short | 27,547.84 | 9,794.54 | 2.813 / 2.948 | 854,968 | 6,818 |

Both idle-v2 guards pass. This is a demonstrated gap, not an independent parity
confirmation. Both complete operations include normalization/source processing,
fresh output ownership and release. No ToArray, preparation, startup, IPC or
validation is timed. Complete packets, registered reconstructed pixels, source
integrity and independent outputs pass, including normal/scalar native decoding.

The original FFmpeg adapter supplies a fixed method6 variant through three checked
effort substitutions. Its generated source, binary and identity stay ignored.
Original effort4 source/binary/identity and both existing shared harnesses remain
byte exact. Existing effort4 lossy/lossless branches also pass validation-only
replays with their original selectors and exact 7,100/7,234-byte packets.

Tooling compiles on Windows/Linux. The actual qualified production DLL is reused;
full cross-platform/scalar/package and usage-example proof carries forward.
No new production qualification or package is claimed. The first BDN preparation
compiled an unrequested library only in its child output. Primary builds stayed
intact; that output was discarded, the selector reused and validated against the
selected immutable core before scoring. An explicit binary-reference project
override prevents that dependency rebuild in subsequent preparations. Preparation
and validation timings are unscored.

[Evidence](warm-vp8-trellis-baseline-evidence.json) retains all raw samples, guards,
source/core/harness/native identities and the mirrored member-verified archive.
The next bounded candidate reuses the existing exact batched forward transform
inside trellis search, preserving per-mode decisions, scalar/cancellation behavior
and current workspace; uniform-score reuse remains disabled for trellis.
