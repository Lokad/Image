# Fixed-workspace PNG header controls

Updated 2026-10-20. **Correctness controls pass; performance remains unmeasured.**
This is the existing `InspectHeaders/PngRgba8FreshSeekable` case. Numeric coverage
remains 237 of 254; this preparation closes no timing or release gate.

The proposed reference uses public **FFmpeg AVIO and owned descriptor APIs with
an original bounded PNG header/EXIF adapter**. The public PNG packet parser frames
complete contiguous packets; incremental use buffers whole input. Including that
parser, full CRC validation or pixel decoding would change this fixed-workspace
operation. The reference choice awaits explicit approval before timing.

Both sides acquire a fresh read-only seekable source exposing no input view, scan
framing/header/EXIF through EOF, skip opaque IDAT, and produce six independent
metadata fields. The managed operation admits 8,448 bytes; native uses an 8,192-byte
AVIO scratch buffer plus a 32-byte header and fixed wrapper/descriptor storage.
No whole-input, metadata-block or raster allocation. Acquisition, reads/seeks,
output ownership and all private release belong inside the future measurement.
The original 218,775-byte packet stays unchanged; actual bytes read are
32,780 managed / 46,678 native, demonstrating opaque skips.

All **21 original controls** agree on the current managed producer and normal/
scalar native adapter: dimensions, orientation, truncation, ordering, unsupported
animation/critical chunks, EXIF and EOF. Corrupted IDAT/IEND CRC and invalid Deflate
remain accepted, as the declared header contract requires. Full Identify remains
separate. Two live descriptors, source mutation/disposal, peer release, exact
input/work/8,448-byte admission, one-below limits, pre- and mid-read cancellation,
caller left open and subsequent recovery pass. Existing shared-budget/deferred-
await qualification is reused for unchanged production.

One successful tooling build per OS and one native build; no production/selector
build or repeated full/package proof. Original typed short/confirmation selectors
are prepared from qualified archives. New controls execute on Linux; Windows
compilation passes. Reuse [production qualification](WARM-LANCZOS3-COEFFICIENT-PERMUTATIONS.md):
4,448 tests/OS, affected scalar/AVX512F-off checks, same actual private package and
three consumer examples on both OS. No production change or new workload matrix.

[Evidence](warm-png-headers-controls-evidence.json). Raw `h17-controls-linux.tar.gz`:
`61b0d4fed828fec4f863ecac68ea23ad0b13897950ad98e3e64b8832baa757e4`. Windows tooling `h17-tooling-windows.tar.gz`:
`c09284982173cc14c7ec320e7bbce3c2e7dc78467a96eea7b3b8eb0380a2ac75`. The frozen build can serve the next
screen after the reference decision. Full warm parity and release readiness remain open.
