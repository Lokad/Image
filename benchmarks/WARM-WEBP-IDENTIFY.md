# Warm resident WebP identification

Independent quiet confirmation measures **0.0040x**, conservative upper
**0.0045x**, at **0.018 us /88 B/op**, against **4.54 us**.
All twelve actuals and both five-second quiet guards pass. The short mean was
read before confirmation. No production optimization is needed; stop this case.

This is one existing533,100-byte VP8L WebP from the original513x259 byte raster,
resident span/default limits to fresh independently owned six-field ImageInfo.
Both subjects validate complete RIFF length/chunk bounds/padding, image headers,
VP8X feature/chunk agreement and bounded EXIF orientation without entropy/pixels.
No image, shape, depth, filter or quality grid is added.

The reference is explicitly **FFmpeg public WebP parser with an original complete
RIFF/VP8X/VP8L/VP8/EXIF compatibility adapter**. The public parser frames packets
and does not supply the required metadata/validation; the decoder has no lossless
header-only discard path. The parser's required input buffering and EOF flush,
structural/metadata inspection and fresh AVCodecParameters descriptor acquisition,
materialization and release are timed. Parser/context and original input are
reused. This parser reference explains its additional warm copy cost; the result
is specific to this metadata operation, not a pixel-codec speed comparison.
Startup, I/O, IPC, preparation/hashing and validation stay outside timing.

All21 same-packet controls agree, including opaque payload acceptance, changed
width/EXIF orientation, malformed length/padding/feature flags and animation
rejection. Direct span controls verify exact admission, work rejection,
cancellation, source stability and independent descriptors. One CPU/normal SIMD,
nonconcurrent workstation GC and equivalent complete ownership are retained.

Only selector/setup/tooling inputs changed. Every production input hash matches
the existing qualified build, and prior resident Identify's measured body is
identical. Reuse C01/C02 and all B01-B15 confirmations. The current3,535 Release
tests per OS and consumer/all three examples through the same actual Release
package on Windows/Linux remain valid; no repeated build/suite/package proof.
One untimed generated restore stalled on network access and was replaced with
cached offline restore before any confirmation measurement; both logs are kept.
[Numeric evidence](warm-webp-identify-evidence.json) retains original actuals,
confidence bounds, guards, owners/controls, binary/source hashes and qualification
reuse. Only C04 fresh-Stream PNG Identify remains on the finite required checklist.
