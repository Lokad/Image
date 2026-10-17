# Warm PNG gray16 encode

Historical <=2x checkpoint. Its PNG encode timings are superseded by the current
[chunk checksum round](WARM-PNG-CHUNK-CRC.md), which refreshes all16 affected
selectors and distinguishes independent parity from qualified short gaps.

Gray16 PNG encoding independently confirms **1.150× FFmpeg**, with conservative
upper **1.156×** and **1,051,455 managed bytes allocated per operation**. The
independent repeat averages 5,667.73 µs; all 12 actual results are retained and
both quietness guards pass. Production is unchanged. Stop tuning this fixed pair;
other required material cases and uniform warm parity remain open.

| Run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Three-actual screen | 5,674.61 | 1.156 | 1.283 | Pass |
| Independent twelve-actual confirmation | 5,667.73 | 1.150 | 1.156 | Pass |

The input is the frozen 513×259 RGBA16 grayscale projection: all three RGB words
are original red and alpha is 65535. No original image was added. Both outputs
use gray16, filter None, noninterlaced PNG and compression level 6. IDAT payloads
are 264,834 versus 264,587 bytes, within the existing 5% size gate. Every inflated
scanline and decoded word is exact, including normal/scalar FFmpeg CLI output.
Source and independent held output owners are checked before and after timing.

Ordinary FFmpeg RGBA64-to-gray16 conversion changes grayscale words by up to 253
in both normal and scalar mode and fails the exact reference check before ready.
This attempt has no scored samples and remains archived. The exact baseline uses
FFmpeg's public `extractplanes` filter to select red from the validated neutral
RGB input, followed by GRAY16LE-to-GRAY16BE conversion. This preserves every word.
The CLI oracle uses the same filter sequence. No pixel tolerance is introduced.

Managed timing covers fresh MemoryStream creation/growth, public EncodePng and
disposal, without a redundant ToArray. Native timing includes the one-thread
filter graph's plane extraction, endian conversion, owned intermediate frame
acquisition/release, encoding and independent packet acquisition/release. The
source, graph, codec context and frame/packet wrappers are reused; intermediate
pixel ownership is acquired and released within each operation. Graph threading
is one with thread type zero, and the scale context's thread count is verified
as one. Startup, I/O, IPC, hashes and validation are excluded. Native allocation
figures remain unmeasured. Runs use one pinned CPU, normal SIMD, tiering off and
workstation nonconcurrent GC; managed iterations target 200 ms and native brackets
50 ms. The screen mean was parsed before the one independent confirmation.

Ten positive native controls cover gray8/gray16 encode/decode and existing
RGBA8/RGBA16 encode, RGB8/RGB16 encode and RGB8/RGB16 decode. Four negative controls
reject nonneutral RGB or nonopaque input at either depth, including low-word-bit
differences, before ready. Forced fresh builds and both actual generated children
match all 595 production method bodies and flags of the same verified actual
Release package. Production/package are unchanged; prior 2,682 Release/
differential tests per OS and all three same-package examples on Windows and
Linux are reused. Only original development tooling links the native filters;
the production package retains its independent safe managed implementation.

The [numeric evidence](warm-pnggray16encode-evidence.json) retains every actual
managed/native sample, allocation evidence, original conversion failure, exact
controls and source/child/native identities. All four actual child files, source,
receipts and actual package are frozen before further preparation or mutation.
The [benchmark summary](../docs/benchmark.md) reports this fixed pair at its
measured source. RGB8/gray8 compression comparisons, JPEG420's upper-bound gap
and full required warm coverage remain open.
