# Warm PNG gray-alpha16 encode

Gray-alpha16 PNG encoding independently confirms **0.903× FFmpeg**, with
conservative upper **0.915×** and **1,052,368 managed bytes allocated per
operation**. The independent repeat averages 9,316.78 µs; all 12 actual results
are retained and both quietness guards pass. Production is unchanged. Stop tuning
this fixed pair; other material cases and uniform warm parity remain open.

| Run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Three-actual screen | 9,353.81 | 0.931 | 1.058 | Pass |
| Independent twelve-actual confirmation | 9,316.78 | 0.903 | 0.915 | Pass |

The input is the existing frozen 513×259 grayscale RGBA16 projection. All three
RGB words are original red; original alpha and hidden RGB remain unchanged,
including two zero-alpha pixels with nonzero hidden RGB. No original image was
added. Both outputs use gray-alpha16, filter None, noninterlaced PNG and
compression level 6. IDAT payloads are 505,342 versus 505,333 bytes, within the
existing 5% size gate. Every scanline and decoded word, alpha, hidden RGB, source
and independent held output owner is exact.

Managed timing covers fresh MemoryStream creation/growth, public EncodePng and
disposal, without a redundant ToArray. The original native adapter losslessly
extracts red and alpha from resident RGBA64LE and packs both complete words into
a reused YA16BE frame inside every operation, then invokes FFmpeg's PNG encoder
and acquires/releases independent packets. No colorspace conversion or alpha
rounding is used. Source, prepared frame, codec context and wrappers are reused.
The existing RGBA16 swap branch explicitly excludes this mode so it cannot write
RGBA-sized rows into YA16 storage. Normal/scalar CLI checks independently encode
the identical canonical YA16BE bytes outside timing; native timed input remains
RGBA64LE. Startup, I/O, IPC, hashes and validation are excluded. Native allocations
are unmeasured.

Sequential Release runs use one pinned CPU, one codec thread, thread type zero,
normal SIMD, tiering off and workstation nonconcurrent GC. Packing executes on
that CPU; the native colorspace conversion thread count is zero. Managed
iterations target 200 ms and native brackets 50 ms. The short mean was parsed
before the one independent confirmation. All four actual child files, source,
receipts and actual package are frozen before subsequent preparation or mutation.

Fourteen positive and six negative native controls cover RGBA/RGB/gray/gray-alpha
encode/decode at both depths. The new mode accepts varied alpha and rejects
nonneutral RGB, including low-word-bit differences. Forced fresh builds and both
actual generated children match all 595 production method bodies and flags of
the same verified actual Release package. Production/package are unchanged;
prior 2,682 Release/differential tests per OS and all three same-package examples
on Windows and Linux are reused. Packing is original development tooling; the
production library remains independently implemented safe managed code.

The [numeric evidence](warm-pnggrayalpha16encode-evidence.json) retains every
actual managed/native sample, allocation evidence, exact controls and source/
child/native identities. The [benchmark summary](../docs/benchmark.md) reports
this fixed pair at its measured source. RGB8/gray8/gray-alpha8 compression
comparisons, JPEG420's upper-bound gap and full required warm coverage remain
open.
