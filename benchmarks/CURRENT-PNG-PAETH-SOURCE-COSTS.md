# Current PNG Paeth source-layout costs

Three existing source layouts pass and stop. RGB8 remains a demonstrated gap.
No production edit or historical speedup claim.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| RgbPaeth8 | 2,860.610 | 1.085 / 1.104 | 3,448 | Gap |
| GrayPaeth8 | 969.038 | 0.937 / 0.960 | 1,400 | Pass; stop |
| GrayAlphaPaeth8 | 1,851.785 | 0.925 / 0.941 | 2,424 | Pass; stop |
| RgbPaeth16 | 3,863.840 | 0.865 / 0.877 | 6,520 | Pass; stop |

Each control retains all12actuals, native brackets, filtered/unfiltered confidence
envelopes and idle-v2 guards. Frozen packets/settings, exactRGBA/alpha/hiddenRGB,
source ownership and independent owned outputs pass. Both subjects include
normalization and disposal; native allocations are unmeasured.

Zero builds or repeated qualification: reuse the immutable PAI1core, generic
selector/native worker, full5094checks/OS and SAMEactualpackage/threeexamples.
Original native-worker descriptors are retained and explicitly reconciled to the
approved current worker without changing reference inputs/settings or pixels.
21existing PNG costs remain unrefreshed; the full268pathobjective remains active.

[Evidence](current-png-paeth-source-costs-evidence.json). Mirrored/member-verified
archive current-png-paeth-source-costs.tar.xz, 51members,
SHA256 09082c910c585b6c75312c3e4cd8d419ad0a5eaf98f6f338521bb18b53bcd2a7; qualified-build dependency pinned by hash.
