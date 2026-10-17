# Warm resident JPEG identification

Historical checkpoint: current timing is superseded by
[bounded entropy scanning](WARM-JPEG-ENTROPY.md). The approved reference,
packet, complete operation boundary and correctness policy remain unchanged.

Independent quiet confirmation measures **1.840x**, conservative upper
**1.860x**, at **2.23 us /88 B/op** against **1.21 us**.
All twelve actuals and both five-second quiet guards pass. The original valid
short measured33.725x/40.98us; the accepted change cuts managed time94.6%.
No longer job was run for that above-two screen. Stop this case.

One existing52,093-byte JPEG420 packet from the original513x259 raster projection
is inspected at default limits. Both operations validate full original marker,
scan-header, stuffing, EXIF and EOI/EOF structure and return fresh independently
owned exact six-field metadata, without coefficient/pixel decoding. Startup,
I/O, IPC, hashing/preparation/validation are excluded; one CPU, normal SIMD,
nonconcurrent workstation GC. No image/profile/quality grid is added.

The reference is **FFmpeg MJPEG discard-all metadata extraction with an original
complete structural/EXIF scan and timed canonical SOF2 header projection**.
Direct decoder inspection applies stronger DQT/sampling/encoded-size checks
than structural Identify. A17-byte grayscale8 metadata header preserves the
validated width/height/precision after the complete original-packet scan;
orientation comes from its bounded EXIF inspection. Projection payload
acquisition, codec reset/header submission, owned AVCodecParameters extraction,
materialization and release are timed; codec/frame/packet wrappers are reused.
No original pixels or quantizers are rewritten in production. A pixel-buffer
callback rejects every request; observed requests remain zero.

A six-second profile puts5978/5984 samples in the scalar structural scan. The
metadata-only primitive uses safe Span.IndexOf SIMD marker searches, bounded
by remaining logical work and4KiB cancellation intervals. It publishes only
admitted non-marker work and keeps marker length/validation ordering. Eighteen
normal/scalar differential tests match old scalar metadata, consumed work and
failure ordering across small budgets, tails and malformed markers. The pixel
decoder's original ContainerHeaders preflight stays unchanged. PNG/WebP Identify,
pixel codecs, edits and resizes retain their executed paths; their proofs reuse.

Twenty-one same-packet controls cover opaque entropy, ignored DQT/sampling
fields, changed width/orientation, multiple scans and malformed structure.
Resident-span controls also check exact input/working limits, work rejection,
cancellation, source stability and independent descriptors. The candidate
reuses the native worker, fixture, harness and generated short driver; no
passing case is retuned.

Fresh **3,535 Release tests per OS** (1,251 internal,1,386 public,898 live FFmpeg)
and consumer/all three examples through the **same actual Release package on
Windows/Linux** pass. Production remains safe managed dependency-free C#/.NET10.
[Numeric evidence](warm-jpeg-identify-evidence.json) retains all confidence,
controls, raw/source hashes, baseline/profile, precise invalidation and package
proof. Full required warm coverage remains open for C03 resident WebP Identify
and C04 the existing PNG through a fresh Stream.
