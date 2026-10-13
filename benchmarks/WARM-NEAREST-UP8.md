# Warm nearest RGBA8 upscaling

Independent quiet confirmation measures **0.929× FFmpeg**, conservative upper
**0.955×**, at **935.05 µs** and **2,133,091 B/op**. The three-actual screen
measures 0.930× / upper 1.505×. Both guards pass in each run; all twelve
confirmation actuals are retained, with eleven selected results. Stop tuning
this fixed pair.

The existing original 513×259 RGBA8 raster becomes 1027×519. Managed output,
the native worker and normal/scalar FFmpeg CLI outputs agree with exact integer
center coordinates for every RGBA sample, including hidden RGB. Geometry,
source immutability and independently held outputs pass. No alpha tolerance
or compatibility pass applies. The existing RGBA8/16 downsample controls also
pass after adding optional native output dimensions and an output byte bound.
No original performance raster is added.

Timing covers complete public `Resize`, admission, allocation, materialization
and disposal. The native public filter graph includes packed/planar conversion,
separate RGB and replicated-alpha point kernels, merging, output allocation and
release. Graph and geometry contexts are reused; output pixels are independently
owned on every call. Both run on one CPU with normal SIMD and one filter thread.
Startup, I/O, IPC and validation are excluded.

Production inputs are unchanged. Both runs reuse the same verified library and
harness binaries; existing Windows/Linux qualification of 2,794 Release tests
and all three examples through the same actual package remains applicable.
No production rebuild, method audit or package proof was repeated for this
coverage experiment. The [numeric evidence](warm-nearestup8-evidence.json)
retains measurements, allocation counts, native controls, guards and identities.
Full required coverage and current-source uniform parity remain open.
