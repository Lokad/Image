# Current PNG color-key transparency costs

Four existing controls resolve current costs without production changes or a
historical speedup claim. Confirmed passing paths stop.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Gray8 tRNS | 336.446 | 0.443 / 0.446 | 1,432 | Pass; stop |
| Gray16 tRNS | 2,939.882 | 1.441 / 1.443 | 2,456 | Gap |
| Rgb8 tRNS | 1,085.873 | 0.845 / 0.852 | 3,480 | Pass; stop |
| Rgb16 tRNS | 2,225.576 | 0.700 / 0.705 | 6,552 | Pass; stop |

Each control retains all12actuals, native brackets, filtered/unfiltered confidence
envelopes and idle-v2 guards. Frozen packets/settings, exactRGBA/alpha/hiddenRGB,
source ownership and independent outputs pass. Both subjects include complete
decode/normalization/owned output/disposal; native allocations are unmeasured.
RGB8 includes native color-key expansion in decode; RGB16 additionally includes
endian conversion and second owned output. Original equivalent boundaries remain.

Zero builds or repeated qualification: reuse immutable PPT1core, generic selector,
native worker, full5107checks/OS and SAMEactualpackage/threeexamples.17existingPNG
costs remain unrefreshed; the full268pathobjective remains active.

[Evidence](current-png-transparency-costs-evidence.json). Mirrored/member-verified
archive current-png-transparency-costs.tar.xz, 51members,
SHA256 bb5fc399bc53a6d160e3f52670ced66f2b80d031914bb2185353e121248a85d2; qualified-build dependency pinned by hash.
