# Tight-memory RGBA8 horizontal bicubic enlargement

The original **513×259 → 1027×259 RGBA8** workload falls from **2.064×**
to an independently confirmed **1.583×** against **zimg, FFmpeg's zscale
backend**: **2,387.65 µs / upper 1.591× / 1,178,425 B/op**, a **23.4%**
managed mean reduction. All twelve actuals are retained and selected; both
versioned quiet guards pass. The working limit is **1,599,536 bytes**: source
plus independently owned output plus **4 KiB**. Native memory is neither
constrained nor measured. Uniform parity remains open.

One primitive, `EnlargingColumnBicubicResize`, omits zero-weight vertical source
rows only when height is unchanged and the **original binary64 center equals
the integer output row**. The cardinal kernel then has one unit tap. Original
coordinate calculation and nonintegral-center support remain, including for
raised dimension limits. The **2,416-byte** fixed arrays, 4-KiB admission, full
support work charge, full horizontal coefficients, exact byte products, unrounded
intermediates, precision guards, original-order replay, deferred tiny-alpha
scratch, cancellation and independent owners remain. No heap workspace,
persistent state or output cache is added.

Every byte matches the frozen original output and independent rational reference.
Native normal/scalar and negative controls retain maximum-one RGB/alpha error
and RMSE at most **0.25**. Complete packing, a four-pixel timed clamped halo
(**4.697%** additional input area), premultiplied float filtering, independent
allocation, unrounded-alpha unpremultiplication, quantization, zero-alpha RGB
clearing and release are timed. This is the labeled backend, not FFmpeg CLI.
No comparison adjustment is added.

The two existing tight **513×259 → 1027×519** enlargement cases also receive
independent confirmation on the **same frozen production DLL and harness**:
RGBA8 **1.783× / upper 1.795×**, **5,412.68 µs / 2,493,057 B/op**;
RGBA16 **1.773× / upper 1.783×**, **5,097.69 µs / 4,264,616 B/op**.
All twelve actuals are retained for each, with eleven/twelve selected respectively.
Both quiet guards pass for each case. These confirmations resolve the previously
unconfirmed short-run upper bounds; the original and regression evidence remains.
Exactly three workloads and the same two original rasters are used this round.

Windows and Linux each pass **3,254 Release tests**, including **896 live FFmpeg
comparisons**, plus **603 focused checks** in normal/scalar modes. The consumer
and all three examples pass through the **same actual dependency-free Release
package** on both platforms. Native controls and qualification are retained in
the [numeric evidence](warm-bicubictighthorizontalup8-cardinal-evidence.json).
Two new frozen horizontal cases extend existing identity-axis, odd-tail, tiny-alpha,
scratch, work, cancellation and owner checks. The qualified byte baseline and
unchanged harness are reused; only changed production/test inputs build. Verified
generated drivers are restored for their actual compiled cases. Full qualification
runs once per platform after passing performance means. No new profile, original
raster, rejected production trial or preparation failure is added. Stop these
three confirmed pairs; other material required/current-source coverage stays open.
