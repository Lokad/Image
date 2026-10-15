# Tight-memory RGBA8 vertical bicubic enlargement

The original **513×259 → 513×519 RGBA8** workload falls from **11.445×**
to an independently confirmed **1.748×** against **zimg, FFmpeg's zscale
backend**: **3,034.06 µs / upper 1.764× / 1,280,402 B/op**, an **84.8%**
managed mean reduction. All twelve actuals are retained; ten are selected.
Both versioned quiet guards pass. The working limit is **1,600,552 bytes**:
source plus independently owned output plus **4 KiB**. Native memory is neither
constrained nor measured. One fixed pair is confirmed; uniform parity remains open.

One primitive, `EnlargingColumnBicubicResize`, now admits enlargement with an
unchanged axis. The existing eight-column/five-row strip cache, **2,416-byte**
fixed arrays, full coefficients, exact byte products, unrounded intermediates,
precision guards, original-order uncertainty replay and deferred tiny-alpha
replay remain unchanged. Tiny pixels resolve after the non-inlined core releases
its 4-KiB admission. Complete work, bounded cancellation and source/output owner
independence remain. No heap workspace, persistent state or output cache is added.
The existing default-memory cache still executes first.

Every managed byte matches the frozen original output, retaining the already
qualified **one independently certified exact rational blue half** allowed by
SPEC.md. No managed tolerance or native gate changes. Native normal/scalar and
negative controls use maximum-one RGB/alpha error and RMSE at most **0.25**.
Complete packing, a four-pixel timed clamped halo (**4.697%** additional input
area), premultiplied float filtering, independent allocation, unrounded-alpha
unpremultiplication, quantization, zero-alpha RGB clearing and release are timed.
This is the labeled backend, not FFmpeg CLI.

The existing tight two-axis enlargement passes its regression mean screen on
the same frozen production/harness: **1.863× / three-sample upper 2.011×**,
**5,661.64 µs / 2,493,057 B/op**. This is a regression screen, not independent
certification; its original confirmed evidence is retained. Default-memory
execution and its inputs remain unchanged.

Windows and Linux each pass **3,250 Release tests**, including **896 live FFmpeg
comparisons**, plus **599 focused checks** in normal/scalar modes. The consumer
and all three examples pass through the **same actual dependency-free Release
package** on both platforms. Qualification runs once per platform after passing
performance means, and is recorded in the
[numeric evidence](warm-bicubictightverticalup8-deferred-evidence.json).
Ten new public cases cover frozen bytes, 4095/4096-byte scratch, both identity
axes, one-dimensional/odd tails, low/tiny alpha, source changes, retained owners,
work and cancellation. Only changed inputs build; verified children and cached
restore are reused. No new original raster, profile, rejected production trial
or preparation failure is added. Stop this confirmed pair; other material
required/current-source coverage remains open.
