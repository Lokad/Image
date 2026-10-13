# Warm PNG gray-alpha8 encode comparison

The gray-alpha8 encode case preserves every sample, including original alpha and
279 zero-alpha pixels with nonzero hidden RGB, but remains **unscored**. Both
subjects use compression level 6, filter None and noninterlaced gray-alpha8.
Lokad.Image emits 126,485 IDAT bytes versus the native encoder's 119,940, a 5.17%
difference relative to the larger payload. The existing 5% size gate stops
preparation before scored workload samples. No timing, allocation or parity claim
is made.

The input is the frozen original 513×259 RGBA8 grayscale projection: all RGB
samples are original red and alpha is unchanged. No original image was added.
Ordinary FFmpeg RGBA-to-YA8 conversion changes 66,083 alpha samples by one in
both normal and scalar mode. Gray and hidden RGB remain exact, but the alpha
check fails before ready. A 16-bit intermediate also changes gray samples and
hidden RGB, so it is rejected. Both attempts remain archived without timing;
no pixel tolerance is introduced.

The exact reference uses original development adapter code to pack red and alpha
losslessly into YA8, inside every operation on the pinned CPU, followed by
FFmpeg's PNG encoder and independent packet acquisition/release. Neutral RGB is
validated before ready; alpha retains its complete original range. Source,
prepared frame, codec context and wrappers are reused. Timed input remains RGBA8.
Normal/scalar CLI checks independently encode the identical canonical YA8 bytes
outside timing. This declares the storage packing explicitly and preserves the
complete source-to-packet workload boundary.

Managed encoding includes fresh MemoryStream creation/growth, public EncodePng
and disposal. Startup, I/O, IPC, hashes and validation are excluded from the
proposed boundary. Exact scanlines, samples, alpha, hidden RGB, source and held
owners pass. Twelve positive and five negative native controls cover this case
and existing RGBA/RGB/gray encode/decode, rejecting nonneutral RGB and nonopaque
inputs where opacity is required. The new mode accepts the original varied alpha.
Forced fresh builds and the actual generated child match all 595 production
method bodies and flags of the same verified actual package. Production is
unchanged; prior 2,682 Release/differential tests per OS and all three same-package
examples on Windows and Linux are reused.

Python zlib level 6 reproduces the exact native IDAT size on the identical
scanlines. A single .NET write produces 126,424 bytes versus 126,485 with separate
filter-byte/row writes. The 61-byte saving does not clear the size gate and is not
a useful batching target. This audit contains no performance measurements.

The [numeric evidence](pnggrayalpha8encode-boundary-evidence.json) retains both
rejected conversion paths, the compression gate failure, exact controls and
source/native/actual child identities. Source, raw evidence, actual package and
all four generated child files are frozen before further preparation or mutation.
This comparison, RGB8/gray8 compression comparisons, JPEG420's upper-bound gap
and full required warm parity remain open.
