# RGBA16 bilinear upsampling with horizontal row reuse

This case independently confirms **1.331× FFmpeg**, conservative upper **1.416×**,
at **2,550.27 µs** and **4,498,974 B/op**. Both quietness guards pass, and all
twelve actuals remain retained. The previous 5,352.52-µs / 2.624× baseline falls
**52.4%**; allocations increase 131,582 B/op, about 3.0%. Stop tuning this pair.

The same original 513×259 RGBA16 raster becomes 1027×519. Every managed sample
still matches the exact rational reference. Source, geometry and independent
held outputs remain exact. Native alpha maximum error is 1/RMSE 0.042395, within
the existing approved bound; RGB maximum is 1/RMSE 0.046274. Zero-alpha RGB is
zero. Native CLI normal/scalar outputs agree exactly. This RGBA16 baseline adds
no RGB-clearing compatibility pass. Complete public resize and native conversion,
premultiplied filtering, output acquisition/release remain timed on one CPU.

One six-second profile attributes 97.6% of 5,403 samples to the paired-row loop,
with both guards passing. A direct four-pixel extension improves the screen by
only 5.6% and remains 2.484×; it is rejected. The accepted kernel instead retains
unrounded, exact 64-bit horizontal intermediates in at most four operation-local
rows and reuses them across vertical outputs. Unsigned full-word premultiplication
avoids sign extension. Conservative precision checks retain original binary64
rounding and tiny-alpha handling near ambiguous boundaries.

Optional workspace is **128×output width + 64 bytes**: 131,520 bytes here,
admitted before allocation and released on every exit. Tight budgets retain
the previous path. No output is cached or reused across operations. Existing
RGBA8, other filters, mixed scaling and unsupported hardware remain unchanged.

Windows and Linux each pass **2,794 Release tests**, including **896 live FFmpeg
comparisons**, and all three examples through the **same actual dependency-free
Release package**. Six new public cases cover full-word products, edges, budget
boundaries, input edits and independent outputs. The
[numeric evidence](warm-bilinearup16-rows-evidence.json) retains qualification,
baseline/profile, the rejected trial and immutable screen/confirmation builds.
Remaining material workloads and final current-source uniform parity stay open.
