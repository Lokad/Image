# Constant-DC JPEG trial and current profile

The constant-DC trial saves only **1.2%** of complete sparse-2D decode time.
It is parked; the qualified implementation is restored exactly on both OSes.

| Existing CMYK sparse-2D → owned RGBA8 | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Qualified integer baseline | 714.01 | 441.52 | 1.617 / 1.693 | 1,085,682 |
| Parked constant-DC trial | 705.14 | 443.11 | 1.591 / 1.607 | 1,085,682 |

Initializing the constant8192 DC weight once removes four loads/widens/products/
adds per nonzero-DC block. Original integer sums, strict certificates and every managed
pixel remain exact;10sparse/horizontal and30public CMYK/YCCK checks pass normally
and scalar per OS. One quiet short passes both guards, all native/scalar quality
gates and independent output ownership. It reuses the22,708B JPEG and531,468B
RGBA boundary. The observed gain is too small to prioritize while material gaps
remain. No full suite, package proof, new raster, settings grid or long repeat
was run for rejected code.

One one-second profile of the **restored current producer** retains954samples,
passing idle-v2 guards and exact setup controls. Entropy traversal accounts for
40.8%; the integer inverse kernel accounts for17.5%. Samples identify
13% in the buffered ordinary AC reader,6.4% in short Huffman reads and5.3% in
fused single-symbol reads. These are profile shares, not comparative timings.

Next: let the existing buffered noncancelable AC reader complete short EOB codes,
avoiding repeated fallback prefix probes. Preserve exact work, returned prefetch
bytes, marker/error precedence and cancellation routing. Any accepted change
would invalidate20existing noncancelable baseline JPEG decode rows; reconcile
those named paths without expanding the matrix. Progressive/cancelable readers,
inspection, encoders and other codecs retain their paths.

The selected benchmark rows remain the accepted integer results. The
[4,467-test/OS and actual-package qualification](WARM-JPEG-HORIZONTAL-INTEGER.md)
is reused after exact restoration. Full254-case parity and release readiness
remain open. [Evidence](warm-jpeg-dc-trial-evidence.json) retains statistics,
identities, profile shares and mirrored raw archive hashes.
