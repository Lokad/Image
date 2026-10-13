# Warm JPEG 4:2:2 encoding

JPEG 4:2:2 encoding independently confirms **1.963× FFmpeg**, conservative upper
**1.990×**, with **527,307 managed bytes allocated per operation**. The repeat
averages 4.633 ms; all 12 actual results are retained and both quietness guards
pass. Production and package are unchanged. Stop tuning this fixed case; uniform
warm parity remains open.

| Run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Three-actual screen | 4,654.37 | 1.976 | 2.202 | Pass |
| Independent twelve-actual confirmation | 4,632.95 | 1.963 | 1.990 | Pass |

Both subjects encode the existing original 513×259 RGBA8 raster, flatten alpha
onto RGB 0x204060 with single rounding, convert to full-range BT.601 and produce
complete independently owned 4:2:2 JPEG bytes. FFmpeg qscale 5 and managed quality
65 match within the unchanged 0.25 dB RGB source-error band: gap 0.245991 dB.
The initial managed quality 69 misses at 0.293371 dB and remains untimed, with
its source, bitstreams and rejection retained. Only these two settings were
checked on the same raster; no quality sweep or original image was added.

Lokad.Image emits **181,958 bytes**, versus FFmpeg's **71,168 bytes**. Managed
fixed Huffman tables and FFmpeg optimal Huffman coding differ; these results
establish the declared source-quality timing comparison, not compression
efficiency parity. Geometry, relative sampling factors, opaque output alpha,
decoder agreement, deterministic bitstreams, source integrity and independent
held owners pass before and after timing.

Managed timing includes fresh MemoryStream creation, growth, complete encoding
and disposal. Native timing includes flattening, color conversion, encoding and
owned packet acquisition/release; contexts, scratch frames and wrappers are
reused, while transformed pixels and encoded output are recomputed each operation.
Startup, I/O, IPC, fixture generation and validation are excluded. Native
allocations are unmeasured. Sequential Release runs use one pinned CPU, one codec
and conversion thread, normal SIMD, tiering off and workstation nonconcurrent GC.
The screen mean was parsed before the single independent confirmation.

Only the development harness and original public-API FFmpeg adapter change.
Existing 4:4:4 and 4:2:0 native bitstreams remain byte-identical. Forced fresh
builds and both actual generated children match all 595 production method bodies
and flags of the verified actual Release package. Prior 2,689 Release/differential
tests per OS, 98 focused PNG normal/scalar checks per OS and all three same-package
examples on Windows and Linux are reused for unchanged production.

The [numeric evidence](warm-jpeg422encode-evidence.json) retains every actual
sample, allocations, rejected anchor and exact source/child/native identities.
All four actual child files, source, receipts and package were frozen before
further preparation or mutation. The [benchmark summary](../docs/benchmark.md)
reports this fixed pair at its measured source; required scope stays open.
