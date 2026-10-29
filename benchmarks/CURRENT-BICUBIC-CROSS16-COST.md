# Current tight bicubic cross16 cost

Superseded by [output-resident word rows](WARM-BICUBIC-OUTPUT-ROW-CACHE.md);
the unchanged original measurements below remain historical evidence.

The existing 513×259 RGBA16 →256×519 TightCross16 control independently confirms
1,285.382 µs, 1.570× / conservative upper1.580, 1,063,410 allocated B/op.
It remains a CPU gap. Twelve actuals and eleven filtered observations, both native
brackets and passing idle-v2 guards are retained. This cost confirmation makes
no causal gain/regression claim against the earlier unmatched short screen.

Complete warm one-CPU public resizing includes allocation/admission and independent
output ownership. The2,129,944B local limit and frozen FFmpeg zscale/zimg worker,
normalization, inputs/settings and exact managed/native-bound gates are unchanged.
The worker's actual pinned original source is recovered from the retained archive;
later unrelated worker-source changes do not rebuild or alter the reference.

No production, harness, worker or selector builds. Three source pins establish
that CrossWord, BicubicMath and transform dispatch remain unchanged from the
applicable attribution. Current TGV1 full Windows/Linux/live/scalar and the same
actual Release-package/three-example qualification carry forward. No repeated
qualification or workload grid is needed; the full268-path objective remains.

[Evidence](current-bicubic-cross16-cost-evidence.json) pins all samples, source,
reference and qualification reuse. Mirrored/member-verified raw archive:
current-bicubic-tightcross16-confirmation.tar.xz,43members,SHA256
ee1a9dc39d40bae92749a68c768d7710133994736b597a495761c9c0cbc40937.
