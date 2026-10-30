# Current PNG palette boundary costs

The original four-control round resolved costs without production changes or a
historical speedup claim. The 16-entry case now passes via [prepared lookups](WARM-PNG-SMALL-PALETTE-LOOKUP.md). Adam7 palette8/4 rows below are superseded by
[packed sparse expansion](WARM-PNG-SPARSE-PALETTE-EXPANSION.md), which passes.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Palette8, partial alpha | 330.007 | 0.902 / 0.911 | 2,224 | Pass; stop |
| Palette8, 16 entries | 128.064 | 1.173 / 1.180 | 1,512 | Prior; superseded |
| Adam7 palette8 | 1,703.925 | 1.274 / 1.283 | 4,976 | Prior; superseded |
| Adam7 palette4 | 1,414.857 | 1.250 / 1.251 | 2,608 | Prior; superseded |

Each control retains all 12 actuals, native brackets, filtered/unfiltered
confidence envelopes and idle-v2 guards. Frozen packets/settings, exact RGBA,
alpha, hidden RGB, borrowed source and independent outputs pass. Both subjects
include complete decoding, normalization, output ownership and disposal;
native allocations are unmeasured. Native PAL8-to-RGBA expansion is included.

Zero builds or repeated qualification: reuse the immutable PGT1 core, generic
selector, native worker, 5,129 checks/OS and same actual package/three examples.
All planned PNG costs are now resolved in the current summary; the full 268-path objective stays active.

[Evidence](current-png-palette-boundary-costs-evidence.json). Mirrored and verified
archive current-png-palette-boundary-costs.tar.xz, 53 members,
SHA256 d6b257bb782d53bb829a4bee63a6b76b6557aa32a2cf4333a1a8e404fb6e167b; qualified-build dependency pinned by hash.
