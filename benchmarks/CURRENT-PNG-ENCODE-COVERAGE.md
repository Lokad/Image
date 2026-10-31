# Current default and stored PNG encoding costs

Six existing named uncertainties now independently confirm warm single-CPU gaps
on the unchanged qualified core. One twelve-actual job per case; these are current
costs, without paired gain/regression claims against older encoders.

| Existing 513×259 control | Lokad µs | Ratio / upper | Managed B/op | Packet B, exact both |
| --- | ---: | ---: | ---: | ---: |
| Encode8 | 24,157.64 | 1.342 / 1.373 | 1,325,732 | 210,973 |
| Encode16 | 26,842.58 | 1.337 / 1.359 | 3,813,732 | 963,202 |
| EncodeRgb16 | 24,119.32 | 1.457 / 1.479 | 3,571,252 | 739,472 |
| EncodeGray16 | 8,637.95 | 1.587 / 1.621 | 1,506,972 | 265,433 |
| EncodeGrayAlpha16 | 16,121.94 | 1.459 / 1.480 | 2,264,564 | 506,887 |
| EncodeNoCompression8 | 253.82 | 1.338 / 1.471 | 1,203,724 | 533,444 |

All six complete packets match their original FFmpeg references byte for byte.
RGBA8/16, RGB16, GRAY16 and gray-alpha16 retain default level6/None; stored RGBA8
retains level0/None. Original raster/alpha/hidden-RGB projections, full precision,
source and independent surviving output ownership, normal/scalar native decoding
and both idle-v2 guards pass. All packet/raster hashes are in the numeric evidence.

Complete operations include source validation, color/endian packing, compression,
output growth, ownership and disposal. GRAY16 native red-plane extraction/filter
allocation and gray-alpha16 storage packing remain inside their boundaries.
Native allocation is unmeasured; managed B/op includes harness costs. Startup,
IPC, fixture preparation, control hashing and validation are excluded.

Older costs used the replaced BCL encoder and IDAT tolerance; owned/native packet
sizes differed. Exact managed encoding and shared GFNI checksum work invalidate
those earlier CPU passes. The six-case audit found no current complete-packet
cost in the existing PNG receipt schemas. Reference inputs/settings/packets and
lossless pixel oracles are unchanged. This adds no shape/depth/filter/quality grid
and does not establish whole-family parity or broad compression claims.

**Zero builds, profiles, new inputs/settings or repeated full/package checks.**
The SAME immutable restored SKP1 runtime, cached four-call selector and unchanged
native worker serve every job. Worker hash/mtime and all runtime/fixture pins
remain. Existing **5,332 checks per OS**, 948 live comparisons, the SAME actual
Release package/three examples and 201 restored public PNG checks normal/scalar
remain applicable. Package stays
`dfa0e669b84872990e01a0e65f21cdd87753b802859be904ac592945cfd49425`.

Stop retiming these six resolved gaps. **Gray16 at 1.587×** is the largest new
PNG ratio; RGB16/gray-alpha16 are about 1.46×. Source review identifies repeated
per-sample row acquisition in 16-bit color packing, while byte packing already
uses bounded spans/SIMD. Profile that concrete new lead before one primitive;
do not reopen closed dictionary/rolling-hash/long-match variants. Exact packets,
pixels, resources, cancellation, ownership and all **268 material paths** remain
in the warm conservative-upper ≤1× objective.

[Numeric evidence](current-png-encode-coverage-evidence.json) retains all actuals,
filtered/unfiltered confidence, pins, exact gates and qualification reuse.
Mirrored/member-verified `current-png-encode-coverage.tar.xz`, SHA256
`e61efe509e9fe097a546c74599b89e8b210f2550bafaf636e9b3cc4fc14c290e` (4,937,416 B / 94 members),
retains the core, cached driver/worker, original inputs/reference packets,
guards, validation/logs and reproduction scripts. CBP1/CF16/SKP1 raw dependencies
remain retained.
