# Four-pixel RGBA8 bilinear upsampling

The fixed two-by-two kernel independently confirms **1.764× FFmpeg**, conservative
upper **1.787×**, at **3,797.55 µs** and **2,235,599 B/op**. Both quietness guards
pass. This reduces the previous 6,529.25-µs baseline mean **41.8%**; allocations
increase by 118 B/op, less than 0.006%. Stop tuning this fixed pair.

The same original 513×259 transparent RGBA8 raster becomes 1027×519. Every
managed pixel still matches the exact rational reference, including alpha and
zero-alpha RGB; source, geometry and independent held outputs remain exact.
The approved native alpha bound and timed writable-output/zero-alpha clearing
pass are unchanged. The [strict refusal and baseline](WARM-BILINEAR-UP8.md)
remain retained. Native conversion, premultiplied filtering and independent
output acquisition/release stay inside timing on one CPU/filter thread.

Four output pixels share a SIMD batch. Exact integer triangle coefficients and
64-bit premultiplied sums replace general support traversal; existing spare
geometry slots prepare the bounded input reads once. Output-array admission
keeps sums below 2^47. A conservative rounding-boundary check retains the
original binary64 calculation and exact tiny-alpha work/memory handling where
needed. No additional workspace, output cache, unsafe code or native production
dependency is introduced. Other depths/filters, mixed scaling, tight-memory
fallbacks and unsupported hardware retain their existing paths.

Row preparation, a direct integer prototype and a single-pixel coefficient
kernel establish no useful gain and are rejected. Their original builds, raw
actuals and native brackets remain frozen. An initial confirmation launcher used
a stale VP8 job; receipt validation rejected it and its timings remain unscored.
The corrected confirmation uses the exact candidate library and benchmark DLLs
from the short screen. All twelve actuals are retained, including one excluded
from the filtered result.

Windows and Linux each pass **2,788 Release tests**, including **896 live FFmpeg
comparisons**, and all three examples through the **same actual dependency-free
Release package**. Focused normal/scalar checks cover exact retained pixel hashes,
edges, budget fallbacks and input edits while previous outputs remain alive.
The [numeric evidence](warm-bilinearup8-batch-evidence.json) records the immutable
builds, qualification and comparisons. Remaining material workloads and final
current-source uniform parity remain open.
