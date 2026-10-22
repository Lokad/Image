# Codec byte compatibility

The qualification target is byte-exact encoder output, followed by exact decoder
samples, against pinned FFmpeg builds and explicit settings. CPU/allocation tuning
follows correctness. Production still uses the earlier encoders; its size/quality
passes do not establish this target.

Only identified encoder-attribution metadata may differ. For the current JPEG
reference, its single `Lavc62.28.100\0` COM payload becomes `Lokad.Image\0`,
with its length updated. Every other byte must match. JFIF, ICC, EXIF, orientation,
color information, quantization, entropy and extra comments are not exempt.
Other codecs need their own explicit attribution inventory.

The independent managed JPEG prototype now reproduces both frozen **513×259
RGBA8/16** controls completely. Encoding receives only source samples, depth and
attribution selection; it reads no oracle packet or table. It generates color
samples, integer transforms, quantization, optimal Huffman tables, headers and
the complete scan, including all **195 padding blocks per control**.

The pinned oracle is FFmpeg n8.1, revision
`9047fa1b084f76b1b4d065af2d743df1b40dfb56`: single-thread MJPEG, qscale 5,
optimal Huffman, full-range BT.601 4:4:4, baseline/no restart, 0x204060 flattening.
Native attribution reproduces its exact **133,041/133,043 B** hashes. Lokad.Image
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

This remains development tooling. Production adoption must remove the prototype's
full-frame YUV copy and preserve shared admission, local/work limits, cancellation,
stream/input ownership and asynchronous output lifetimes. Other dimensions,
settings, sampling, public boundaries, codecs and exact decoding remain open.
These two procedural controls establish no general compression claim. Prototype
CPU and allocations are unmeasured; benchmark tables still describe production.

In `tools/WarmReference`, `verify-jpeg-scan-stage.ps1` checks the independent
complete encoder and its supplied-table stage; `verify-jpeg-huffman-stage.ps1`
checks the table generator. The color/forward-stage verifiers retain intermediate
checks. `compare-jpeg-packets.py ACTUAL REFERENCE --allow-encoder-attribution`
enables only the exception above, reporting raw equality separately. Its boundary
checks run with `python -B tools/WarmReference/test-jpeg-packet-comparison.py`.

Lossless WebP retains the approved libwebp `exact=1` reference; grayscale JPEG
retains the explicitly labeled SIMD-enabled libjpeg-turbo reference. Existing
quality checks remain until exact packet/sample gates cover the same required
paths; then retire calibration/dB analysis, keeping minimal sanity tests and
historical evidence. All functional, resource and performance objectives remain.
