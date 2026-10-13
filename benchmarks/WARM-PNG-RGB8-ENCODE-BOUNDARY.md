# Warm PNG RGB8 encode comparison

The new opaque RGBA8-to-RGB8 PNG case passes exact scanline, pixel and ownership
checks, but remains **unscored**. Both encoders use compression level 6, filter
None and noninterlaced RGB8. Lokad.Image produces 191,758 IDAT bytes versus
FFmpeg's 168,159, a 12.31% difference relative to the larger payload. This exceeds
the existing 5% compression-size gate, so preparation fails explicitly before
any scored workload samples. No timing, allocation or parity claim is made.

The input is the existing original 513×259 RGBA8 raster with alpha set to 255
and every RGB sample unchanged. It is identical to the earlier RGB8 decode
projection; no original performance image was added. The native reference adds
an explicit RGB encode operation with single-thread RGBA-to-RGB24 packing inside
each operation, followed by encoding and independent packet acquisition/release.
Its source, converted frame, codec context and wrappers are reused. Managed
encoding includes fresh MemoryStream creation/growth, public EncodePng and
disposal. Startup, I/O, IPC, hashing and validation are excluded from the proposed
timing boundary.

Untimed checks verify identical inflated scanlines, every decoded pixel,
unchanged input and independent held outputs. Normal and scalar FFmpeg CLI
outputs are exact. The changed adapter also passes existing RGB8/RGB16 decode
and RGBA8/RGBA16 encode controls. All production methods and flags in the forced
fresh build and generated child match the same verified actual Release package.
Production is unchanged; the prior 2,682 Release/differential tests per OS and
all three actual-package usage examples on both OSes are reused.

A focused compression audit reproduces the native payload size with Python
zlib level 6 on the identical scanlines. In .NET, a single write produces 191,638
bytes versus 191,758 with separate filter-byte/row writes. Row batching therefore
recovers only 120 bytes and does not resolve the comparison. This audit includes
no timing and does not establish a cause beyond the measured implementation
difference.

The [numeric evidence](pngrgb8encode-boundary-evidence.json) records the gate
failure, scanline hash, exact controls, native build, actual child identities and
immutable archive identity. The original preparation log, owned source and all
four generated child files are frozen before further preparation or mutation.
The fixed pair, other required material cases and uniform warm parity remain open.
