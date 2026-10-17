# Warm PNG RGBA8 NoCompression encode

Historical <=2x checkpoint. Its PNG encode timings are superseded by the current
[chunk checksum round](WARM-PNG-CHUNK-CRC.md), which refreshes all16 affected
selectors and distinguishes independent parity from qualified short gaps.

Bulk RGBA8 row copies reduce the short-screen encode time by **83.7%**. The
independent confirmation is **1.883× FFmpeg**, conservative upper **1.933×**, with
**2,102,472 managed bytes allocated per operation**. Both quietness guards pass;
all 12 actual results are retained. This fixed case now meets the target; stop
tuning it. Other required material cases and uniform warm parity remain open.

| Implementation/run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Per-component access baseline | 2,182.89 | 11.526 | 11.872 | Pass |
| Bulk row copy, three-actual screen | 355.53 | 1.874 | 1.928 | Pass |
| Independent twelve-actual confirmation | 357.17 | 1.883 | 1.933 | Pass |

A six-second baseline CPU profile attributes 39.1% of samples to PNG Encode,
36.2% to Stride and 13.0% to Pixels. Repeated component access dominates the
stored-Deflate path. One existing production method, PngCodec.Encode, now copies
RGBA8 rows in bulk in groups of at most 4,096 pixels. All other 594 method bodies
and all implementation flags remain equivalent with metadata references resolved.
No method, workspace, dependency, unsafe code or native binding is added.

The optimization preserves every output byte, including hidden RGB and alpha.
The same row buffer, reservations, work charge, None filter, zlib write sequence
and chunk order remain. Cancellation checks precede each 4,096-pixel group;
other color types and 16-bit packing keep their existing paths. Seven new public
checks cover row/tail boundaries, exact inflated scanlines, hidden samples,
unchanged source and header cancellation. All 98 focused PNG checks pass with
intrinsics enabled and disabled on both OSes. All 2,689 Release/differential tests
per OS pass: 915 internal, 878 public and 896 live FFmpeg. All three examples use
the same actual dependency-free Release package on Windows and Linux. Fresh
builds and actual timed children match all 595 package method bodies and flags.

One unchanged original 513×259 RGBA8 raster is used. Both encoders use RGBA8,
filter None, noninterlaced PNG and NoCompression/level 0. Independent structural
checks require only stored Deflate blocks, valid length/complement pairs and
exact full scanlines. Malformed block type, length, sample and truncation controls
fail explicitly. IDAT payloads are identical at 531,818 bytes; the frozen managed
PNG remains byte-for-byte unchanged. Normal/scalar native CLI, samples, source
and independent held outputs are exact. Existing native codec controls pass.
The native worker is unchanged between baseline and candidate.

Managed timing includes fresh MemoryStream creation/growth, public EncodePng and
disposal without a redundant ToArray. Native timing includes complete independent
packet acquisition/encoding/release; source, codec context and wrappers are
reused. Startup, I/O, IPC, preparation, hashes and validation are excluded. Native
allocation figures remain unmeasured. Sequential Release runs use CPU0, one
codec thread, thread type zero, normal SIMD, tiering off and workstation
nonconcurrent GC. Native brackets target 50 ms and managed iterations 200 ms.
The candidate mean was parsed before the single independent confirmation.

The [numeric evidence](warm-pngstore8copy-evidence.json) retains baseline/screen/
confirmation, every actual managed/native sample, allocations, profile identity,
package verification and exact source/child/native identities. All four actual
child files, source, receipts and actual package are frozen before later
preparation or mutation. This is the retained fixed-case result at its measured
source; the [summary](../docs/benchmark.md) does not certify all workloads at HEAD.
