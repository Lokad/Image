# Warm byte PNG Optimal encoding

Historical <=2x checkpoint. Its PNG encode timings are superseded by the current
[chunk checksum round](WARM-PNG-CHUNK-CRC.md), which refreshes all16 affected
selectors and distinguishes independent parity from qualified short gaps.

The three previously unscored default byte PNG encodes now pass the **unchanged
5% IDAT-size gate**, exact sample/hidden-RGB/owner checks, and independent quiet
warm confirmation against **FFmpeg**. Each complete public encode uses a fresh
managed MemoryStream; its creation, growth, encoding and disposal are timed.
The single-thread native operation includes required packing/conversion, encode,
independent packet acquisition and release, with prepared wrappers reused.

- **Gray8:** 3,540.72 µs, **1.802× / upper 1.818×**, **263,664 B/op**.
  Both payloads are **72,304 bytes**.
- **RGB8:** 10,402.09 µs, **1.093× / upper 1.107×**, **527,098 B/op**.
  Payloads are **163,277 / 168,159 bytes**, differing **2.903%**.
- **Gray-alpha8:** 5,924.76 µs, **1.374× / upper 1.382×**, **264,179 B/op**.
  Payloads are **119,967 / 119,940 bytes**, differing **0.0225%**.

Every confirmation retains all twelve actuals; ten/eleven/twelve are selected
respectively. Both versioned quiet guards pass for every screen and confirmation.
Exactly three existing 513×259 projections of the same authored byte raster are
used; the total original performance raster count stays two. Native memory is
not measured; managed figures include harness overhead.

One encoder policy change makes public `Optimal` use the BCL's smallest-size
search for **byte gray, RGB and gray-alpha**. It applies to every input of these
layouts, with no fixture-dependent dispatch. **Managed backend level 9 / FFmpeg
level 6** are explicitly reported. FFmpeg's settings and complete workload stay
unchanged. Other formats and explicitly selected policies retain their previous
compressor selection. Filter None, exact samples, 5% size comparison, work,
admission, bounded chunks and cancellation remain. No workspace/cache/dependency
or native binding is added.

Equal numeric levels did not provide comparable compression: the original
level6/level6 gray, RGB and gray-alpha payload differences were **16.65%, 12.31%
and 5.17%**, and those refusals remain unscored. In a correctness-only audit on
identical gray scanlines, all BCL level6 strategies failed; levels7/8 also failed.
Level9 closes the gap. Row partitioning stays fixed; its prior negligible batching
gain is not retuned. The [runtime uses zlib-ng](https://github.com/dotnet/runtime/blob/v10.0.8/src/native/external/zlib-ng.cmake),
while [FFmpeg initializes zlib](https://github.com/FFmpeg/FFmpeg/blob/n8.1/libavcodec/zlib_wrapper.c).
The backend levels are implementation settings, and their actual output and cost
are verified rather than inferred from matching numbers.

The harness now pins the **original compressed decoder inputs** and stores new
encoder outputs separately. A preparation refusal caught the original attempt
to replace a frozen input; that failure is retained and unscored. Three untimed
decoder controls verify the old hashes, every sample, normal/scalar agreement
and independent owners. No decoder input bytes or historical timings are replaced.

Windows and Linux each pass **3,257 Release tests**, including **896 live FFmpeg
comparisons**, plus **101 focused PNG checks**. The consumer and all three examples
pass through the **same actual dependency-free Release package** on both systems.
Qualification is retained in the [numeric evidence](warm-pngbyte-optimal-evidence.json).
Three new public tests
cover comparable default payloads, every sample, source and held outputs;
existing admission/explicit-policy/cancellation tests pass. Only changed inputs
build; the same verified production/harness and shared generated drivers serve
all measured cases. Full qualification runs once per platform after useful
passing means. Stop these three confirmed pairs; uniform current-source parity
and other material public/codec boundaries remain open.
