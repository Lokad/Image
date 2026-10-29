# Current PNG palette boundary costs

Four existing controls resolve current costs without production changes or a
historical speedup claim. Confirmed passing paths stop.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Palette8, partial alpha | 330.007 | 0.902 / 0.911 | 2,224 | Pass; stop |
| Palette8, 16 entries | 128.064 | 1.173 / 1.180 | 1,512 | Gap |
| Adam7 palette8 | 1,703.925 | 1.274 / 1.283 | 4,976 | Gap |
| Adam7 palette4 | 1,414.857 | 1.250 / 1.251 | 2,608 | Gap |

Each control retains all 12 actuals, native brackets, filtered/unfiltered
confidence envelopes and idle-v2 guards. Frozen packets/settings, exact RGBA,
alpha, hidden RGB, borrowed source and independent outputs pass. Both subjects
include complete decoding, normalization, output ownership and disposal;
native allocations are unmeasured. Native PAL8-to-RGBA expansion is included.

Zero builds or repeated qualification: reuse the immutable PGT1 core, generic
selector, native worker, 5,129 checks/OS and same actual package/three examples.
13 existing PNG costs remain unrefreshed; the full 268-path objective stays active.

[Evidence](current-png-palette-boundary-costs-evidence.json). Mirrored and verified
archive current-png-palette-boundary-costs.tar.xz, 53 members,
SHA256 d6b257bb782d53bb829a4bee63a6b76b6557aa32a2cf4333a1a8e404fb6e167b; qualified-build dependency pinned by hash.
