# Bilinear resizing on the current package

Two of the three original controls affected by shared image storage pass on
package `d873438f`, containing Linux DLL `dc8c9a94`. The RGBA8 reduction
remains uncertain. The same three 513×259 inputs and output geometries are used.

| Complete operation | Lokad.Image µs | FFmpeg µs | Conservative ratio interval | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8 enlargement to 1027×519 | 1,967.04 | 1,994.67 | 0.883261–0.993357 | 102,891 | Pass |
| RGBA8 reduction to 256×129 | 384.01 | 387.09 | 0.976486–1.007839 | 8,473 | Uncertain |
| RGBA16 reduction to 256×129 | 412.89 | 419.04 | 0.979734–0.991838 | 8,473 | Pass |

Lokad.Image prepares geometry and workspace and disposes an independent result
inside each call. FFmpeg retains its filter graph and geometry across calls.
Each native call includes packed/planar conversion, the zscale triangle filter
with straight-alpha handling, and output acquisition and release. The RGBA8
enlargement includes native zero-alpha normalization. Exact managed pixels,
the original native RGB and alpha bounds, scalar and normal FFmpeg output,
source preservation and independent output checks pass.

The private driver reuses the original generated loops and validation actions
with explicit natural GC. Each case retains all twelve managed actuals and both
twelve-sample native brackets. Native batches have the original 32, 256 and 128
operations, respectively. Both versioned quiet guards pass on CPU 0. The
conservative interval takes the outer bounds across filtered and all-actual
managed estimates and both native brackets. The wide enlargement interval
retains a higher-cost native bracket after the managed actuals.

GC allocation is separate from the owned result payloads of 2,132,052 B,
132,096 B and 264,192 B. Those results fit the existing 4 MiB idle limit.
The RGBA16 enlargement's 4,264,104 B result bypasses that limit and retains
its [passing partitioned evidence](BILINEAR-PARTITIONED-REFERENCE.md).
Its [fresh-graph comparison](BILINEAR-EQUIVALENT-ONE-SHOT.md) has a separate
operation boundary.

One tooling build reused three frozen runtimes and the pinned FFmpeg worker.
The first two launches stopped before sampling because the replay needed the
original worker source/build-script bytes and an unused ImageProbe presence
check. Both startup logs are retained. The replay now binds those original
source hashes and the existing ImageProbe artifact. Production and package
qualification are reused. No production speedup follows from differences across
captures. Stop the passing cases and keep the RGBA8 reduction open pending
evidence that explains the variation or an affected production change.

[Evidence](bilinear-current-package-evidence.json) names the frozen receipts,
samples, runtime and tool hashes, and mirrored archives. The 28-member qualified
archive includes the exact driver and VM recipe. All archive members were
hash-verified on both hosts. The [previous costs](RELEASED-IMAGE-STORAGE.md)
remain available under their original producer.
