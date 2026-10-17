# Tight-memory RGBA16 horizontal bicubic enlargement

Historical timing checkpoint: [wider enlargement strips](WARM-BICUBIC-ENLARGEMENT-STRIPS.md)
records seven current qualified short results. Its affected enlargement timings and source/qualification checkpoints are historical; other selected paths retain their applicable evidence.

The original **513×259 → 1027×259 RGBA16** workload falls from **2.086×**
to an independently confirmed **1.606×** against **zimg, FFmpeg's zscale
backend**: **2,313.25 µs / upper 1.614× / 2,129,311 B/op**, a **22.9%**
managed mean reduction. All twelve actuals are retained and selected; both
versioned quiet guards pass. The working limit is **3,194,976 bytes**: source
plus independently owned output plus **4 KiB**. Native memory is neither
constrained nor measured. This confirms one fixed pair; uniform parity stays open.

One primitive, `EnlargingWordColumnBicubicResize`, omits zero-weight vertical
source rows when height is unchanged and the **original binary64 center equals
the integer output row**. The cardinal bicubic kernel then has one unit tap.
Original coordinate evaluation remains; nonintegral centers retain full support,
including when callers raise dimension limits. The **2,416-byte** fixed arrays,
4-KiB admission, original full-support work charge, full horizontal coefficients,
exact unsigned products, unrounded intermediates, precision guards, original
replay, deferred tiny-alpha scratch, cancellation and independent owners remain.
No heap workspace, persistent state or output cache is added.

Every managed word matches the frozen original output and independent rational
reference. Native normal/scalar and negative controls retain maximum-one RGB/
alpha error and RMSE at most **0.25**. Complete packing, a four-pixel timed
clamped halo (**4.697%** additional input area), premultiplied float filtering,
independent allocation, unrounded-alpha unpremultiplication, quantization,
zero-alpha RGB clearing and release are timed. This is the labeled backend,
not FFmpeg CLI; no baseline adjustment is added.

The existing tight two-axis enlargement passes its regression mean screen on
the same frozen production/harness at **1.697×**, **4,875.12 µs / 4,264,616 B/op**.
Its three-sample upper **2.623×** is not independent certification. The byte
horizontal enlargement check remains **open at 2.064× / upper 2.213×**,
**3,115.06 µs / 1,178,393 B/op**; no twelve-sample confirmation is scored above two.

Windows and Linux each pass **3,252 Release tests**, including **896 live FFmpeg
comparisons**, plus **601 focused checks** in normal/scalar modes. The consumer
and all three examples pass through the **same actual dependency-free Release
package** on both platforms. Native controls and qualification are retained in
the [numeric evidence](warm-bicubictighthorizontalup16-cardinal-evidence.json).
Two new frozen horizontal cases extend the existing both-axis/identity-axis,
odd-tail, tiny-alpha, scratch, work, cancellation and owner tests. The harness
builds once for both new cases; verified generated drivers are reused. An initial
word confirmation was launched prematurely and stopped; its logs remain unscored.
A preliminary shortcut was superseded by the original-coordinate equality guard;
its short and confirmation archives remain. Full qualification runs only for
the accepted guarded implementation. No new original raster or profile is added.
Stop this confirmed word pair; prioritize the open byte gap next.
