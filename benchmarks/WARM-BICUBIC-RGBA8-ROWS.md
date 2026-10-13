# Warm RGBA8 bicubic downsampling with row reuse

Independent quiet confirmation measures **1.554× FFmpeg's scaler backend**,
conservative upper **1.593×**, at **650.72 µs** and **332,178 B/op**. Both guards
pass; all twelve actuals are retained, with eleven selected results. The
9,257.98-µs / 22.175× baseline falls **93.0%**. Stop tuning this fixed pair.

The existing original 513×259 RGBA8 raster becomes 256×129. Managed pixels remain
exact against the independent rational reference, including alpha and zero-alpha
RGB. Source, geometry and independently held outputs pass. The explicitly
adjusted native reference uses **zimg, FFmpeg's zscale backend**, with B=0/C=0.5
and a **timed four-pixel clamped halo**: 4.697% more input area. Complete source
deinterleaving, filtering, planar/packed output allocation, repacking and release
are timed. Graph and scratch contexts are reused; every call transforms every
sample. This backend comparison is distinct from the unmatched ordinary CLI
boundary. One normal-SIMD alpha sample and one RGB sample differ by one level;
RMSE is 0.005503 and 0.003177 respectively, within the explicit maximum-1 /
RMSE-0.25 native bounds. Scalar backend output is exact. No image is added.

The new safe managed path prepares geometry once, premultiplies source bytes
exactly with integer intrinsics and reuses unrounded horizontal intermediates in
sixteen operation-local rows. Two independent horizontal accumulators avoid
wide-vector construction that spilled repeatedly on the measured JIT. Source
decoding and rounding-distance checks operate in batches. Conservative guards
retain original binary64 accumulation and exact tiny-alpha arithmetic at
ambiguous quantization boundaries. Tight budgets and unsupported hardware retain
the previous path, with the same work charges and bounded cancellation.

Optional workspace is **716×output width + 32×source width + 128 bytes**,
199,712 bytes here, admitted before allocation and released on every exit.
Allocations rise about 200 KB/op from 132,224 B/op. This buys the measured
93.0% time reduction without caching output or retaining state between calls.
The native adapter allocates 264,192 output pixel bytes per call; this describes
known planar/packed payload, not a complete native allocation measurement.

An initial row-reuse screen remains 4.203×; independent accumulators and batched
decoding reach 2.124×. Both remain retained intermediate trials without twelve-
actual confirmation. One six-second profile of the row-reuse trial, with both
guards passing, and its generated code identify the repeated stack traffic.
An earlier baseline profile's post guard fails for 1.201% aggregate I/O wait;
that failure is retained and is not certified. Neither affects the quiet scored
baseline or accepted screen/confirmation.

Windows and Linux each pass **2,800 Release tests**, including **896 live FFmpeg
comparisons**, and all three examples through the **same actual dependency-free
Release package**. Six new public cases cover support boundaries, negative
lobes, tiny/zero alpha, exact cache admission and independent retained outputs.
The 134 focused resize checks pass with normal/scalar code on both platforms.
The [numeric evidence](warm-bicubic8-rows-evidence.json) retains raw measurements,
guards, build identities, trials and package qualification. Required material
coverage and current-source uniform parity remain open.
