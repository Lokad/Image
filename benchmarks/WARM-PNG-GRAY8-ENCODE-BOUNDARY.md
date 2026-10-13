# Warm PNG gray8 encode comparison

The new opaque grayscale RGBA8-to-gray8 PNG case passes exact sample, scanline and
ownership checks but remains **unscored**. Both subjects use compression level 6,
filter None and noninterlaced gray8. Lokad.Image emits 86,745 IDAT bytes versus
FFmpeg's 72,304, a 16.65% difference relative to the larger payload. The existing
5% size gate stops correctness preparation before scored workload samples.
No timing, allocation or parity claim is made.

The input is the existing original 513×259 RGBA8 raster with all three RGB samples
set to original red and alpha set to 255. It is identical to the frozen gray8
decode projection; no original image was added. The native adapter's explicit
gray encode operation includes one-thread full-range RGBA-to-GRAY8 conversion
inside each operation, followed by encoding and independent packet acquisition/
release. Source, converted frame, codec context and wrappers are reused. Managed
encoding includes fresh MemoryStream creation/growth, public EncodePng and
disposal. Startup, I/O, IPC, hashing and validation are excluded from the proposed
timing boundary.

Untimed checks verify identical inflated scanlines, every decoded sample,
unchanged source and independent held outputs. Normal/scalar FFmpeg CLI outputs
are exact. Eight native controls cover the new gray encode/decode and existing
RGBA8/RGBA16 encode, RGB8/RGB16 encode and RGB8/RGB16 decode. Two negative controls
reject nonneutral RGB and nonopaque input before the worker becomes ready.
Forced fresh builds and the actual generated child match all 595 production
method bodies and flags of the same verified actual package. Production is
unchanged; prior 2,682 Release/differential tests per OS and all three same-package
examples on Windows and Linux are reused.

A focused audit reproduces FFmpeg's payload size using Python zlib level 6 on the
identical scanlines. In .NET, a single write produces 86,711 bytes versus 86,745
with separate filter-byte/row writes. Batching saves only 34 bytes and does not
resolve the compression comparison; it is not a useful optimization target.
These checks include no timing and do not establish a cause beyond the observed
implementation difference.

The [numeric evidence](pnggray8encode-boundary-evidence.json) retains the original
preparation failure, exact controls, compression audit and source/native/actual
child identities. Owned source, the actual package and all four generated child
files are frozen in an immutable evidence archive before further preparation or
mutation. This pair, RGB8 encode's comparison, JPEG420's upper-bound gap and the
full required warm scope remain open.
