# Codec byte compatibility

The qualification target is byte-exact encoder output, followed by exact decoder
samples, against pinned FFmpeg builds and explicit settings. CPU/allocation tuning
follows correctness. JPEG defaults to the `Reference` profile in0.2.0 development;
Original/CompactColor are superseded development experiments scheduled for removal.
Their historical size/quality passes do not establish this target or create a
compatibility obligation.

Only identified encoder-attribution metadata may differ. For the current JPEG
reference, its single `Lavc62.28.100\0` COM payload becomes `Lokad.Image\0`,
with its length updated. Every other byte must match. JFIF, ICC, EXIF, orientation,
color information, quantization, entropy and extra comments are not exempt.
Other codecs need their own explicit attribution inventory.

The managed JPEG `JpegQuantizationPolicy.Reference` profile reproduces the frozen
**513×259 RGBA8/16** controls at **4:4:4, 4:2:2 and 4:2:0** completely. Encoding receives source samples and options;
it reads no oracle packet or table. It generates color
samples, integer transforms, quantization, optimal Huffman tables, headers and
the complete scan, including all **195 vertical padding blocks per 4:4:4 control**.

The pinned oracle is FFmpeg n8.1, revision
`9047fa1b084f76b1b4d065af2d743df1b40dfb56`: single-thread MJPEG, qscale 5,
optimal Huffman, full-range BT.601 4:4:4, baseline/no restart, 0x204060 flattening.
The test-only prototype with native attribution reproduces its exact
**133,041/133,043 B** hashes. Production Lokad.Image
attribution produces **133,039/133,041 B**, with only the approved COM difference.
All these checks pass on Windows/Linux, preserving inputs and surviving outputs.
The Huffman generator additionally matches **32 original bounded histograms**,
including ties, sparse alphabets and length limiting; changed tables are rejected.

The quantization generator uses normative format data from
[H.262 §6.3.11](https://www.itu.int/rec/dologin_pub.asp?id=T-REC-H.262-200002-S%21%21PDF-E&lang=s&type=items).
Transform constants are generated mathematically. No upstream implementation,
implementation table or corpus is incorporated.
[Numeric evidence](../benchmarks/jpeg-byte-parity-evidence.json) identifies the
sources, binaries, earlier exact stages and mirrored raw results.

Production also reproduces the four existing **4:2:2/4:2:0, RGBA8/16** controls
on Windows/Linux: **71,168/71,210 B** and **52,093/52,117 B**
with native attribution, two bytes less with Lokad.Image attribution. All **931,360**
submitted color samples per OS match exactly. Six small functional controls also
qualify even-width RGB pairing, tiny one/two/three-cell filters and partial MCUs.
These reuse the original procedural recipe and do not broaden content claims.

The existing **4:2:0 RGBA8 row-restart33** control also matches: **58,012 B** with
native attribution, **58,010 B** with the approved COM replacement, including all
16 sequenced restart markers. The native reference uses two **serial** slice
contexts on one codec thread; these force fixed tables despite its requested
optimal setting. Lokad.Image independently generates those optional format
examples from [T.81 K.3.3](https://www.w3.org/Graphics/JPEG/itu-t81.pdf#page=162).
No oracle packet or coding table is an encoder input.
At18×19, non-row intervals2/3 have functional checks for444/422 from RGBA8 and
420 from RGBA16, plus native-decoder interoperability checks for all three from RGBA8.
Those checks establish no additional native-packet or exact-decoder claim.

The production **grayscale Reference profile** also reproduces its existing
**58,910 B** packet exactly, using the approved SIMD libjpeg-turbo3.1.4.1 reference,
quality69 and the existing owned quantizers. Its **137,280 raw transforms** and
**137,280 quantized coefficients** match; the native scalar/SIMD transforms agree.
The independent [T.81 K.2](https://www.w3.org/Graphics/JPEG/itu-t81.pdf#page=148)
table prototype matches **34 original histograms**, including length limiting;
production tests also freeze its complete packet and uniform-alphabet table.
No metadata exception is needed: the grayscale reference has no attribution COM.
The existing RGBA16 control also matches its complete **58,942 B** packet after
independent16-bit normalization. The RGBA8 grayscale control with explicit MCU
restart interval3 matches **61,152 B**, including714 sequenced markers and
optimized tables. These checks use the same backend, quality and quantizers.

The production encoder uses at most a 512-byte MCU buffer, with no
full-frame YUV or coefficient/event copy. Chroma weights are generated on the
stack; no retained row or filter plan is needed. Optimal tables use two passes;
color restarts use fixed tables in one pass. Grayscale uses a single 64-byte
block. RGBA8/16 images and caller Gray8/RGB8 spans with stride support sync,
forward-only and async delivery. Color quality maps to native qscale
`1 + (100 - Quality) * 30 / 99`; default85 maps to the frozen qscale5 reference.
Grayscale uses its existing owned quantization formula and the same quality as its
pinned backend, with69 frozen above. Caller Gray8 overrides Sampling and needs no
RGBA intermediate. Grayscale packet claims cover the two declared source depths
without restarts, plus the8-bit interval3 control. Other intervals have functional
coverage; quality endpoints do not imply a broader native-packet claim.

Shared admission reserves fixed/table capacity and coarse table-building peaks
before allocation. Temporary storage is released before async delivery; encoded
chunks remain charged until the actual writer finishes, including cancellation
and failure. Inputs and the shared root are borrowed. Existing local/work limits
remain independent; refusal has no fallback. Other required codecs, JPEG settings
and exact decoding remain open.
These two procedural controls establish no general compression claim. CPU and allocations for the Reference profile are unmeasured; tables retain
Original-profile observations with their recorded producer identities.

PNG `Optimal` now uses the independently implemented managed streaming level6
encoder for grayscale/RGB/grayscale-alpha/RGBA at8/16bits. All eight existing
**513×259** controls reproduce complete frozen FFmpeg packets, raw-exact with
no metadata exemption. Source pixels alone determine tokens, Huffman trees,
stored/fixed/dynamic blocks, zlib checksums and4096-byte PNG chunks. The initial
RGBA8 prototype also matches111,511observed tokens,21trees and1549code-length runs;
the independent RFC1950/1951 observer retains those intermediate checks.

Production keeps a bounded history/dictionary and one token block rather than
full-frame filtered/coefficient copies. One conservative1MiB workspace reservation
precedes allocation through the borrowed operation context. It remains charged
until its synchronous users finish; encoded async output keeps its separate owner.
Refusal, cancellation and writer failure release workspace without encoder fallback
or cleanup writes. Source/stream ownership and independent surviving outputs remain
qualified. Other policies and CPU/allocation qualification remain open.
[Numeric evidence](../benchmarks/png-byte-parity-evidence.json) records the tested scope.

In `tools/WarmReference`, `verify-jpeg-scan-stage.ps1` checks the independent
complete encoder and its supplied-table stage; `verify-jpeg-huffman-stage.ps1`
checks the table generator. The color/forward-stage verifiers retain intermediate
checks. `compare-jpeg-packets.py ACTUAL REFERENCE --allow-encoder-attribution`
enables only the exception above, reporting raw equality separately. Its boundary
checks run with `python -B tools/WarmReference/test-jpeg-packet-comparison.py`.
`verify-jpeg-subsampling-stage.ps1` checks the source-only 4:2:2/4:2:0 prototype.
`verify-jpeg-gray-stage.ps1` checks the grayscale stages, tables and complete packet.

Lossless WebP retains the approved libwebp `exact=1` reference; grayscale JPEG
retains the explicitly labeled SIMD-enabled libjpeg-turbo reference. Existing
quality checks remain until exact packet/sample gates cover the same required
paths; then retire calibration/dB analysis, keeping minimal sanity tests and
historical evidence. All functional, resource and performance objectives remain.
