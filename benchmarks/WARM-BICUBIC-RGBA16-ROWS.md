# Warm RGBA16 bicubic downsampling with row reuse

Independent quiet confirmation measures **1.313× FFmpeg's scaler backend**,
conservative upper **1.320×**, at **745.92 µs** and **464,342 B/op**. Both guards
pass, and all twelve actuals remain selected and retained. The 8,416.94-µs /
14.675× scalar baseline falls **91.1%**. Stop tuning this fixed pair.

The same original 513×259 RGBA16 raster becomes 256×129. Managed pixels remain
exact against the independent rational reference. The labeled **zimg, FFmpeg
zscale backend** baseline includes timed full-word source deinterleaving,
four-pixel clamped edge padding, filtering, independent planar/packed allocation,
repacking and release. Padding adds 4.697% input area. Normal native RGB/alpha
maximum errors are 1, with RMSE 0.049116 / 0.040061; scalar mode also stays
within the explicit maximum-1 / RMSE-0.25 bounds. Zero-alpha RGB, source and
independent live outputs pass. No original raster is added.

One production primitive extends the existing row-reuse kernel to full UInt16
samples. Integer premultiplication produces unsigned 32-bit products, then
converts them exactly to binary64 without sign extension. The same optional
sixteen-row cache retains unrounded horizontal intermediates. Word-specific
precision bounds use a conservative reciprocal of the lower power of two of
alpha; ambiguous thresholds and tiny alpha retain original arithmetic.
Full-word unsigned output packing preserves samples above 32767. Workspace
remains **716×output width + 32×source width + 128 bytes**, 199,712 bytes here,
admitted and released within the operation; tight budgets retain the old path.
Allocations increase about 200 KB/op from 264,320 B/op.

Because the helper is shared, one RGBA8 control is independently confirmed on
the **same immutable production/harness build**: **1.713× / upper 1.728×**, at
711.85 µs and **332,177 B/op**. This is about 9.4% more time than its previous
650.72-µs confirmation and remains below 2×. Its noisy three-actual screen and
every confirmation actual remain retained. Stop tuning both passing pairs.

Windows and Linux each pass **2,807 Release tests**, including **896 live FFmpeg
comparisons**, and all three examples through the **same actual dependency-free
Release package**. Seven new public cases cover full-word products/packing,
support and workspace boundaries, tiny/zero alpha, source edits and held output
independence. The 141 focused resize checks pass with normal/scalar code on both
platforms. The [numeric evidence](warm-bicubic16-rows-evidence.json) retains the
baseline, screen, independent confirmations, native controls and qualification.
Required material coverage and current-source uniform parity remain open.
