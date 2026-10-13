# Warm nearest RGBA16 upscaling

Independent quiet confirmation measures **1.051× FFmpeg**, conservative upper
**1.095×**, at **1,071.13 µs** and **4,265,431 B/op**. All twelve actuals are
retained, with eleven selected results; both quietness guards pass. The
three-actual screen measures 1.089× / upper 2.153×. Stop tuning this fixed pair.

The existing original 513×259 RGBA16 raster becomes 1027×519. Managed output,
the native worker and normal/scalar FFmpeg CLI outputs agree with exact integer
center coordinates for every full-precision RGBA sample, including hidden RGB.
Geometry, source immutability and independently held outputs pass. No alpha
tolerance, compatibility pass or original performance raster is added.

Timing covers complete public `Resize`, admission, allocation, materialization
and disposal. The native public filter graph includes packed/planar conversion,
separate RGB and replicated-alpha point kernels, merging, output allocation and
release. Graph and geometry contexts are reused; output pixels are independently
owned on every call. Both run on one CPU with normal SIMD and one filter thread.
Startup, I/O, IPC and validation are excluded.

Production and native worker inputs are unchanged. Both runs reuse the same
verified library and harness binaries. Existing Windows/Linux qualification of
2,794 Release tests and all three examples through the same actual package is
reused. No full qualification, method audit or package proof was repeated.
One preparation guard failed because a compiler server remained alive; no
benchmark started. That failure is retained, and the server was stopped before
the screen reused the same prepared binaries.

The [numeric evidence](warm-nearestup16-evidence.json) retains measurements,
allocation counts, native controls, guards and immutable build identities.
Full required coverage and current-source uniform parity remain open.
