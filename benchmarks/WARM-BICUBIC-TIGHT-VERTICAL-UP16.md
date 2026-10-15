# Tight-memory RGBA16 vertical bicubic enlargement

The original **513×259 → 513×519 RGBA16** workload falls from **12.065×**
to an independently confirmed **1.618×** against **zimg, FFmpeg's zscale
backend**: **2,688.28 µs / upper 1.636× / 2,133,096 B/op**, an **86.6%**
managed mean reduction. All twelve actuals are retained; eleven are selected.
Both versioned quiet guards pass. The working limit is **3,197,008 bytes**:
source plus independently owned output plus **4 KiB**. Native memory is neither
constrained nor measured. This confirms one fixed pair; uniform parity remains open.

One primitive, `EnlargingWordColumnBicubicResize`, now admits enlargement with
an unchanged axis. Admission alone leaves whole-image tiny-alpha replay and
does not improve performance; that rejected screen is retained. The accepted
kernel marks only tiny-alpha pixels in private unfinished output, finishes its
strips, releases the non-inlined frame and replays those pixels. The impossible
completed-pixel marker adds no workspace or heap cache. The final scan, original
color arithmetic, exact rational alpha rounding and additional work are timed.
The **2,416-byte** fixed arrays stay within the unchanged **4-KiB** admission.
Full coefficients, exact unsigned products, unrounded intermediates, tap order,
depth-scaled precision guards, ordinary replay, cancellation and owners remain.

Every managed word matches the original frozen output and the independent
rational reference. The existing cache still executes first. The existing
two-axis tight enlargement passes its regression screen on the same frozen
production and harness at **1.729× / upper 1.879×**, **4,968.27 µs /
4,264,616 B/op**. Its three samples are a regression screen, not a new independent
certification. Default-memory execution and inputs remain unchanged.

The comparison retains the approved native adjustment solely for RGBA16
**513×259 → 513×519**: RGB error at most **2** only where reference alpha is
at most **16**, RGB error at most **1** elsewhere, alpha error at most **1**,
and RGB/alpha RMSE at most **0.25**. Managed pixels remain exact. The native
boundary times complete packing, four-pixel clamped halo (**4.697%** additional
input area), premultiplied float filtering, independent allocation, unrounded-alpha
unpremultiplication, quantization, zero-alpha RGB clearing and release. This is
the labeled backend, not FFmpeg CLI. Normal/scalar and negative controls retain
the exact geometry and low-alpha restrictions.

Windows and Linux each pass **3,240 Release tests**, including **896 live FFmpeg
comparisons**, plus **589 focused checks** in normal/scalar modes. The consumer
and all three examples pass through the **same actual dependency-free Release
package** on both platforms. Full qualification runs once per platform after
passing performance means. Results are retained in the
[numeric evidence](warm-bicubictightverticalup16-deferred-evidence.json).
Ten new public cases cover frozen pixels, 4095/4096-byte scratch, both identity
axes, one-dimensional and odd tails, low/tiny alpha, source changes, retained
owners, work and cancellation. Only changed inputs build; verified children,
cached restore and unchanged production qualification are reused during experiments.
An initial registration failure and corrected test setup are retained and unscored.
No new original raster or profile is added. Stop this confirmed pair; other
material required/current-source coverage remains open.
