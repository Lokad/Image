# Codec byte compatibility

The qualification target is byte-exact encoder output, followed by exact decoder
samples, against pinned FFmpeg builds and explicit settings. CPU/allocation tuning
follows correctness. The explicit JPEG `Reference` profile is the first production adoption; defaults
retain Original/CompactColor during migration. Their size/quality passes do not
establish this target.

Only identified encoder-attribution metadata may differ. For the current JPEG
reference, its single `Lavc62.28.100\0` COM payload becomes `Lokad.Image\0`,
with its length updated. Every other byte must match. JFIF, ICC, EXIF, orientation,
color information, quantization, entropy and extra comments are not exempt.
Other codecs need their own explicit attribution inventory.

The managed JPEG `JpegQuantizationPolicy.Reference` profile reproduces both frozen **513×259
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

The production encoder uses a 384-byte MCU buffer in two passes, with no
full-frame YUV or coefficient/event copy. RGBA8/16 images and caller RGB8 spans
with stride support sync, forward-only and async delivery. Quality maps to native
qscale `1 + (100 - Quality) * 30 / 99`; default85 maps to the frozen qscale5
reference. Small dimensions and quality endpoints have functional checks; exact
packet claims remain limited to the two controls above. Other sampling and restart
markers currently return structured unsupported-feature failures for this profile.

Shared admission reserves fixed/table capacity and coarse table-building peaks
before allocation. Temporary storage is released before async delivery; encoded
chunks remain charged until the actual writer finishes, including cancellation
and failure. Inputs and the shared root are borrowed. Existing local/work limits
remain independent; refusal has no fallback. Other required codecs, JPEG settings
and exact decoding remain open.
These two procedural controls establish no general compression claim. CPU and allocations for the Reference profile are unmeasured; tables retain
Original-profile observations with their recorded producer identities.

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
