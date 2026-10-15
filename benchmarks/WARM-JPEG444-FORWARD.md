# JPEG444 encoding through forward-only output

Complete RGBA8 JPEG444 encoding through a forward-only Stream independently
confirms **1.754x FFmpeg / conservative upper 1.765x**, at **526,051 managed
allocated B/op**. Production is unchanged; uniform current-source coverage
remains open.

| Measurement | Lokad.Image us | FFmpeg us | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 6,672.00 | 3,774.14 | 1.768 / 2.115 | 526,051 |
| Independent twelve-actual confirmation | 6,613.11 | 3,770.32 | 1.754 / 1.765 | 526,051 |

The existing original 513x259 RGBA8 raster uses quality 69/native qscale5 and
background RGB 0x204060. A fresh forward-only wrapper over fresh owned MemoryStream
storage forces the qualified fixed geometric Huffman path. Wrapper creation/
disposal, storage growth, alpha flattening, color conversion, transform,
serialization and owned output release are timed. Native flattening/conversion,
encoding and independent packet acquisition/release are timed; codec/conversion/
frame/packet wrappers may be reused. No output is cached.

An original development-only visitor using public libjpeg APIs proves that the
complete quantization, geometry and coefficient dump equals the qualified
[adaptive JPEG444 anchor](WARM-JPEG444-RGBA8-ADAPTIVE.md), byte for byte.
Each registered decoder's pixels and source errors remain exact. The unchanged
source-PSNR gap is **0.082440 dB**, within **0.25 dB**. Decoder RGB maximum 3 /
RMSE 0.227803 stays inside maximum 4 / RMSE 1.5; opaque alpha, source preservation
and independent outputs pass before/after timing. Both SOF layouts express 4:4:4:
native 1x2/1x2/1x2 and managed 1x1/1x1/1x1. Managed output is 170,978 bytes, native
133,041 bytes. This is source-quality matched timing; size efficiency parity is
not established. No raster, quality sweep or production policy is added.

Both quiet guards pass for each run. All twelve confirmation actuals remain
retained; BenchmarkDotNet selects eleven. The upper uses the larger filtered/
all-actual interval against the faster native bracket's lower bound. Sequential
Release measurements use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Startup, file I/O, IPC,
preparation, coefficient inspection, hashing and validation are excluded.
Native allocations are unmeasured; managed figures include harness overhead.

The screen supervisor referenced an absent metadata flag after successful timing.
Its failed receipt is retained; an explicit correction checks the actual setting
count 1 / order [69] and revalidates packet/pixel/guard/job checks without rerunning
the measurement. Confirmation uses the corrected checks directly.

Only two benchmark inputs change. The same verified immutable production
DLL/harness and existing native worker serve screen and confirmation. Existing
3,308 Release tests per OS, including 896 live FFmpeg comparisons, and consumer
plus three examples through the same actual Release package are reused. No
production/native rebuild, full suite, package proof or whole-library method
audit repeats.

The [numeric evidence](warm-jpeg444stream-evidence.json) retains every sample,
allocation, guard, exact coefficient proof, actual job/assembly, packet and
qualification link. Stop this confirmed pair; remaining material public/codec/
current-source coverage and the grayscale comparator stay open.
