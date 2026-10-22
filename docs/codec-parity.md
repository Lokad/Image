# Codec byte compatibility

The next qualification target is byte-exact encoder output, followed by exact
decoder samples, against pinned FFmpeg builds and explicit settings. CPU and
allocation tuning follows correctness. Current size/quality passes do **not**
establish byte compatibility; the production encoders still differ.

Only identified encoder-attribution metadata may differ. For the current JPEG
reference, its single `Lavc62.28.100\0` COM payload may become `Lokad.Image\0`,
with the segment length updated. Every other byte must match. JFIF, ICC, EXIF,
orientation, color information, quantization, entropy and extra comments are
not exempt. Other codecs need their own explicit attribution inventory.

The first oracle is FFmpeg n8.1, revision
`9047fa1b084f76b1b4d065af2d743df1b40dfb56`, using the existing single-thread
MJPEG worker, qscale 5, optimal Huffman coding, full-range BT.601 4:4:4 and
0x204060 alpha flattening. Its flags and reference packets remain frozen.
Lossless WebP retains the approved libwebp `exact=1` backend; grayscale JPEG
retains the explicitly labeled SIMD-enabled libjpeg-turbo reference.

The initial JPEG audit finds baseline versus progressive scans, one versus two
quantization tables, and 8×16 versus 8×8 MCUs. Both current controls first differ
at byte 3. The attribution exception does not resolve these coding differences.

An independent scalar C# **development probe**, not yet a production path,
matches the actual submitted Y/Cb/Cr planes for both original 513×259 RGBA8/16
controls on Windows/Linux: **797,202 exact samples per platform**. The instrumented
worker reproduces both frozen complete native JPEG hashes, confirming unchanged
encoding. This check covers flattening, 16-to-8 normalization and 4:4:4 conversion.

The scalar forward transform now matches **823,680 prequantized values** per
platform. Integer quantization also matches **823,680 visible coefficients**
extracted from those native packets, including their edge blocks. Transform
constants are generated from cosine/tangent geometry. Changed-coefficient checks
fail as expected. Phantom blocks, MCU traversal, table generation and entropy
serialization, other sampling and decoding remain open; these stages are still
development tooling.
[Numeric evidence](../benchmarks/jpeg-byte-parity-evidence.json) identifies their
sources, pinned binaries and mirrored raw results.

Tooling in `tools/WarmReference`:

- `compare-jpeg-packets.py ACTUAL REFERENCE` checks complete packets and describes
  their first divergence. `--allow-encoder-attribution` enables only the exception
  above, reporting raw equality separately. Run its six boundary checks with
  `python -B tools/WarmReference/test-jpeg-packet-comparison.py`.
- `ffmpeg-jpeg-stage-dump.c` instruments the existing worker through its public
  frame submission, preserving its operations and options.
- `verify-jpeg-color-stage.ps1 -SourcePath SOURCE -ExpectedYuvPath PLANES -Depth 8`
  checks the managed stage and borrowed input. Use depth 16 for RGBA16LE.
- `verify-jpeg-forward-stage.ps1 -InputYuvPath PLANES -ExpectedDctPath DCT
  -ExpectedCoefficientsPath COEFFICIENTS` checks the transform, quantization and
  deliberately changed coefficients against the pinned native outputs.

Existing quality checks remain during the transition. Retire calibration and
dB analysis only when exact packet/sample gates cover the same required paths;
retain minimal quality-option sanity tests and historical evidence. Resource
limits, shared admission, cancellation, ownership and safe managed execution
remain mandatory throughout.
