# Fresh JPEG Stream decoding

Complete fresh-stream JPEG decoding independently confirms **1.891x FFmpeg /
conservative upper 1.902x**, at **1,813,266 managed allocated B/op**.
Production is unchanged; uniform current-source coverage remains open.

| Measurement | Lokad.Image us | FFmpeg us | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 1,882.05 | 1,001.52 | 1.879 / 1.940 | 1,813,266 |
| Independent twelve-actual confirmation | 1,886.05 | 997.23 | 1.891 / 1.902 | 1,813,266 |

Both subjects reacquire the existing58,012-byte JPEG420 restart packet through
fresh seekable memory streams. The managed MemoryStream does not expose its
buffer; public `Decode(Stream)` performs all default buffering, growth and final
copies. Native fresh AVIO uses an8192-byte read callback, length query, complete
owned packet acquisition and an explicit EOF check, without demuxing. Input
acquisition, all copies, full decode/chroma expansion/conversion and independent
output release are timed. Native decoder/conversion/frame/packet wrappers may
be reused; encoded packet storage is acquired and released every operation.

The encoded input and each decoder's pixels exactly match the
[resident restart anchor](WARM-JPEG-RESTART-DECODE.md). Strict DRI33/sixteen-RST
framing and negative controls pass. Managed caller lifetime, EOF, unchanged
source and two independent outputs are checked before/after timing. Native
pixels equal the public CLI; scalar/normal decoder agreement remains maximum3 /
RMSE0.579363, within unchanged maximum4/RMSE1.5, with exact opaque alpha. No new
image, encoding, quality search or comparison allowance is introduced.

All twelve confirmation actuals are retained and selected. The upper uses the
larger filtered/all-actual interval against the faster native bracket's lower
bound. Every versioned quiet guard passes. Sequential Release runs use one
pinned CPU, one codec/conversion thread, normal SIMD, disabled tiering and
workstation nonconcurrent GC. Startup, file I/O, IPC, preparation, hashing and
validation are excluded; no decoded output is cached. Native allocations are
unmeasured; managed figures include harness overhead.

The distinct original adapter preserves existing resident-JPEG and PNG-stream
workers. An initial sampling-selector setup refusal remains unscored; the
corrected gate requires4:2:0 explicitly. Screen and confirmation reuse the same
verified immutable production DLL/harness. Only two benchmark files and an
original adapter/build script change. Existing3,308 Release tests per OS,
including896 live FFmpeg comparisons, and consumer plus three examples through
the same actual Release package are reused. No production rebuild, full suite,
package proof or whole-library method audit repeats.

The [numeric evidence](warm-jpegstream-evidence.json) retains every sample,
allocation, guard, actual job/assembly, encoded input and qualification link.
Stop this confirmed pair; remaining material public/codec/current-source
coverage stays open.
