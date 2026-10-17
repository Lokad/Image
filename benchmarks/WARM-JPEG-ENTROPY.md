# JPEG entropy scanning for metadata inspection

Updated 2026-10-17. The existing resident JPEG Identify case independently meets
warm single-CPU parity. Managed time falls **72.9%**, with unchanged metadata,
complete structural validation, logical work and output ownership. Stop this
case; parity across the fixed 207-case checklist remains open.

| Existing operation | Managed µs | Reference µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Resident JPEG Identify | 0.604 | 1.211 | 0.499 / 0.506 | 88 | Parity confirmed |

The input remains the same 52,093-byte JPEG420 packet from the original 513×259
raster. Complete public Identify includes admission, the original marker,
scan-header, stuffing, EXIF and EOI/EOF scan and fresh owned ImageInfo. Entropy
symbols and pixels are not decoded. The approved reference remains **FFmpeg
MJPEG discard-all plus complete original structural inspection, timed canonical
17-byte SOF2 projection and owned AVCodecParameters acquisition/materialization/
release**. Native contexts are reused; pixel-buffer requests stay zero. Native
allocation is unmeasured. No input, settings, quality gate or workload was added.

One six-second profile has 75.2% of 5,886 samples in unresolved CoreLib code;
the saved JIT confirms a Span.IndexOf call. This is attribution evidence, not a
wall-time result. The new primitive scans ordinary bytes and stuffed FF00 pairs
together within an already admitted ≤4 KiB raw span. Safe Vector512 masks count
exact logical work and stop before actual markers; scalar tails and other
hardware share those semantics. Cancelable scans retain their original source
and polling. There is no new allocation, workspace or admission requirement.

Removing one branch, the helper and three imports restores the **entire** previous
JpegIdentifier file byte for byte. The pixel decoder's ContainerHeaders preflight
is unchanged. **206 comparisons and all 57 prior passing proofs are reused.**
JPEG Stream Identify shares this scan; no separate Stream timing is claimed
because that operation has no row in the fixed checklist.

One three-actual screen precedes one independent twelve-actual confirmation on
the same production/harness/native/fixture build. All actuals, native pre/post
brackets and both exclusive-vm-idle-v2 guards remain retained. The confirmation
keeps its ten selected results and all twelve actuals in the conservative bound.
An initial setup refusal before timing required refreshing the existing 21
equivalent-boundary controls for the new DLL; every input/result stays identical.
No selector, harness or native rebuild was needed during measurement.

All **3,648 Release tests per OS** pass (1,342 internal, 1,408 public, 898 live
FFmpeg), plus 20 scalar scanner checks per OS. Two new authored facts cover dense
stuffing, vector/search edges, truncated tails, real markers and exact error/work
ordering against the independent scalar scan. The same actual private Release
package passes its consumer and all three examples on Windows/Linux; symbols
match, and Linux production bytes are unchanged through qualification. Unaffected
scalar proofs and the published 0.1.0 package/tag are preserved.

[Numeric evidence](warm-jpeg-entropy-evidence.json) retains statistics, receipts,
identities, reuse proofs and qualification. Main raw archive
`parity-p24-qualified-evidence.tar.gz`, SHA256
`00089a2c9ec2eb349141ac5e01dbdd1ba38531638fe8805177029e80e2b59200`.
