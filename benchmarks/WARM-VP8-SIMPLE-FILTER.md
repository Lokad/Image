# VP8 simple-filter decoding

Complete opaque VP8 simple-filter decoding independently confirms **1.746×
FFmpeg / upper 1.765×**, at **760,548 managed B/op**. Production is unchanged.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Short screen | 8,744.75 | 4,902.99 | 1.784 / 3.302 | 760,548 |
| Independent confirmation | 8,538.31 | 4,889.76 | 1.746 / 1.765 | 760,548 |

One original 76,128-byte keyframe uses the existing 513×259 byte raster's
unchanged opacity projection. An original development-only public libwebp API
adapter authors the fixture at quality75/method4, simple filter/strength60,
one segment and no concurrency. Two independent encodes are byte-identical.
Original interval-header inspection (RFC6386 sections7/9) proves coded simple
filtering at level8, sharpness0, with no segmentation or deltas; every possible
intra mode therefore has effective level8. This exercises reduced luma edge
filtering with chroma filtering omitted. No new original image or settings grid.
Reference sources, version/archive/tool/input hashes and repeat receipts are
retained; binaries and generated packets stay ignored.

Qualified managed pixels and normal/scalar FFmpeg CLI anchors are pinned before
timing. Managed outputs remain exact, opaque alpha255 and independent owners
are checked, and native RGB agreement preserves max4/RMSE1.5. The source stays
unchanged. Identical-input decoding requires no matched encoder quality or
quality enumeration. libwebp is used only for untimed fixture authoring;
**FFmpeg's software decoder remains the performance baseline**.

Both subjects time complete decode/conversion and independent owned-output
release. Native state reset, centered chroma expansion, limited BT601 conversion
through RGBA16, nondithered RGBA8 reduction and real scratch/output acquisition
are included. Codec/frame/packet/conversion wrappers are reused, processing all
pixels on every operation. No alpha merge or output cache. Startup, I/O, IPC,
generation, hashing and validation are excluded.

Sequential Release runs use CPU0, one codec/conversion thread, normal SIMD,
disabled tiering and workstation nonconcurrent GC. Both pre/post versioned quiet
guards pass. The short screen's mean passes; its three-actual upper is not a
certification. The independent run retains all twelve actuals (eleven selected);
its upper includes selected/all-actual intervals against the faster native
bracket's lower bound. Managed allocations include harness overhead; native
total allocations are unmeasured.

One reference-author build and one changed harness build suffice. Both measured
runs reuse the same verified immutable production core/harness/native decoder
worker. Existing 3,483-tests-per-OS/live/same-actual-package qualification is
reused, with no production/native decoder rebuild, profile or repeated proof.

[Numeric evidence](warm-vp8-simple-filter-evidence.json) retains every actual,
allocation, guard, original fixture provenance and frozen source/build/pixel
identity. Stop this pair; remaining material/current-source coverage stays open.
