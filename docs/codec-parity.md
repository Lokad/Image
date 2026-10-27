# Codec reference parity

Lokad.Image 0.2.0 compares complete encoded packets and decoded samples on the
fixed controls below. These are tested content/settings scopes, not universal
byte-identity or speed claims. Inputs are original deterministic fixtures; no
reference implementation, packet or implementation table is a production input.

| Case | Verified scope and result | Reproducible evidence |
| --- | --- | --- |
| PNG encode | 14 complete packets exact: 513×259 gray/RGB/gray-alpha/RGBA, 8/16-bit Optimal; RGBA8/16 also stored/Fastest/SmallestSize. Includes metadata, chunks and checksums. | [Controls/settings](../benchmarks/png-byte-parity-evidence.json) |
| JPEG color encode | 4:4:4/4:2:2/4:2:0 RGBA8/16 and 4:2:0 row-restart33: complete packets exact except the COM replacement below; direct caller RGB and stream delivery share the encoder. Frozen color quality 85 → reference qscale 5. | [Controls/settings](../benchmarks/jpeg-byte-parity-evidence.json) |
| JPEG grayscale encode | Complete 58,910/58,942-byte RGBA8/16 packets and 61,152-byte 8-bit restart3 packet exact; quality 69, SIMD libjpeg-turbo 3.1.4.1. Caller GRAY8 shares this encoder. | [Controls/settings](../benchmarks/jpeg-byte-parity-evidence.json) |
| WebP lossless encode | 19 selected plus two forced packet identities exact, including 160,032-byte photograph, 7,234-byte diagram, alpha/hidden RGB, palettes, effort and 8/16 normalization controls. libwebp 1.6.0, quality 75, matching method, exact=1, one CPU. Native supports axes through 16,383; managed 16,384-axis cases have functional coverage only. | [Controls/settings](../benchmarks/vp8l-byte-parity-evidence.json) |
| WebP lossy encode | 20 packet identities across 22 513×259 rows, all efforts and named qualities: exact complete packets, including 17,688-byte photograph and 7,100-byte diagram (Q75/method4). Separate 5,632² retry control exact at 16,787,132 bytes. Alpha exact after 8-bit normalization. | [Controls/settings](../benchmarks/vp8-byte-parity-evidence.json) |
| PNG decode | Exact analytical/reference RGBA on the 72 authored static profiles; palette/tRNS, packed depths, filters and Adam7 included. Packed-gray native tRNS correction is explicit in the reference tooling. | [Profiles](../tests/Lokad.Image.PublicApi.Tests), [live suite](../tests/Lokad.Image.DifferentialTests) |
| JPEG gray/YCbCr decode | Exact samples on 30 frozen baseline/progressive gray/4:4:4/4:2:2/4:2:0 controls, including refinement, split scans, odd edges and restarts; named caller/stream/orientation boundaries also exact. | [Decoder tests](../tests/Lokad.Image.PublicApi.Tests), [live suite](../tests/Lokad.Image.DifferentialTests) |
| JPEG CMYK/YCCK, 4:1:1/4:1:0 decode | Named constant/varying/progressive/saturated/AC and subsampled controls require exact reference RGB/grayscale; canonical caller CMYK keeps its explicit ink contract. | [Decoder tests](../tests/Lokad.Image.PublicApi.Tests), [live suite](../tests/Lokad.Image.DifferentialTests) |
| WebP VP8L decode | Exact RGBA, including alpha and hidden RGB, on frozen/live and authored transform/entropy controls. | [Decoder tests](../tests/Lokad.Image.PublicApi.Tests), [live suite](../tests/Lokad.Image.DifferentialTests) |
| WebP VP8 decode | Exact Y/U/V, alpha and 164 frozen authored/reference RGBA controls. Reference RGBA16 normalization avoids its direct-byte saturated-blue wrap; managed samples remain exact. | [Decoder tests](../tests/Lokad.Image.PublicApi.Tests), [live suite](../tests/Lokad.Image.DifferentialTests) |

The pinned FFmpeg reference is n8.1, revision
`9047fa1b084f76b1b4d065af2d743df1b40dfb56`. Color JPEG uses single-thread MJPEG,
full-range BT.601, optimal Huffman and the frozen sampling/background settings.
Grayscale JPEG uses the approved, explicitly labeled standalone SIMD libjpeg-turbo
reference. Lossless WebP uses libwebp exact=1 to retain transparent RGB.

Only the identified color-JPEG attribution COM may differ:
`Lavc62.28.100\0` becomes `Lokad.Image\0`, with its length adjusted (two bytes
smaller). No other comments, JFIF, ICC, EXIF, color or coding bytes are exempt.
PNG/WebP have no metadata exception. [Packet comparison](../tools/WarmReference/compare-jpeg-packets.py)
enforces this boundary. New content/settings need explicit evidence; numerical
quality equality, PSNR alone or IDAT-only size comparisons do not establish parity.

[Supported contract](spec.md) defines API/format limits. [Benchmarks](benchmark.md)
report the separately measured CPU/allocation scope; complete performance parity
and general replacement readiness remain open.
