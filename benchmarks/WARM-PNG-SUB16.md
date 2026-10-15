# PNG16 Sub reconstruction

Complete RGBA16 Sub decoding independently confirms **0.482x FFmpeg /
conservative upper 0.493x**, at unchanged **8,552 managed B/op**. The mean falls
**76.5%** from the original 2.052x screen. Every word, alpha and hidden RGB sample
is exact; no native comparison allowance changes.

| Complete decode | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Original RGBA16 Sub | 1,521.69 | 741.65 | 2.052 / 2.409 | 8,552 | Above two; no twelve-sample repeat |
| Prefix sum, short | 380.74 | 755.70 | 0.504 / 1.586 | 8,552 | Quiet screen |
| Prefix sum, independent twelve | 357.99 | 743.02 | 0.482 / 0.493 | 8,552 | Confirmed |
| RGBA8 Sub | 1,328.83 | 673.42 | 1.973 / 2.211 | 4,456 | Regression screen; prior confirmed |
| RGBA16 Paeth | 5,749.92 | 5,769.74 | 0.997 / 1.264 | 8,552 | Regression screen; prior confirmed |

One six-second profile attributes **81.7%** of CPU samples to row reconstruction.
An independent safe Vector128 primitive computes eight byte-prefix sums modulo 256:
each vector reconstructs two pixels and carries the upper pixel into the next
vector. Original scalar tails and 65,536-byte cancellation checkpoints remain.
Short previous rows retain the original partial-update exception path; overlapping
rows, pre-cancellation/token identity, empty rows, alignment, tails and overflow
match independent scalar controls. No workspace, allocation, unsafe code,
dependency, native binding, upstream implementation or output cache is added.

The new input reuses the original deterministic 513x259 RGBA16 raster. Independent
authoring follows [PNG section 9.2, Sub](https://www.w3.org/TR/png-3/),
subtracting the byte eight positions earlier after full word-to-big-endian
packing. Every row is Sub; compression remains BCL Optimal level6. Byte authoring
retains its original packet exactly. Only two original rasters and three workload
types are used, without image, geometry, compression or strategy enumeration.

Both subjects time complete identical-input decoding, all reconstruction/endian
conversion, independent output acquisition and release. Native wrappers/encoded
packet are reused; there is no decoded-output cache. Normal/scalar native CLI
pixels, source preservation, geometry/orientation and two live outputs pass.
Sequential Release runs use one pinned CPU, one codec/conversion thread, normal
SIMD, disabled tiering and workstation nonconcurrent GC. Every quiet guard passes.
All twelve confirmation actuals are retained; BenchmarkDotNet selects ten. The
upper includes both selected and all-actual intervals against the faster native
bracket's lower bound. Startup, I/O, IPC, generation and validation are excluded.
Native allocations are **unmeasured**.

Both regression means pass before full **3,383 Release tests per OS**, including
**898 live comparisons**, plus 165 focused normal/scalar checks per OS. One actual
Release package passes its consumer and all three examples on Windows and Linux.
Measured and qualified inputs are identical. The first Windows default-tiering
suite reported 808 bytes in an unchanged writer's zero-allocation test; its compiled
focused retry and full qualification with tiering disabled pass. That failed log
is retained; it is not silently counted as a pass.

[Numeric evidence](warm-pngsub16-evidence.json) retains every actual, guard,
profile, assembly/job identity, source inventory, package proof and raw archive
hash. Stop this confirmed pair; other material workloads and unmatched policies
remain open.
