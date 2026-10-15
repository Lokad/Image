# PNG16 SmallestSize encoding

Complete RGBA16 SmallestSize encoding independently confirms **0.970× FFmpeg /
upper 0.985×**, at **2,103,522 managed B/op**. Both backends use numeric level9;
all words, alpha and hidden RGB remain exact. Production is unchanged.

| Fixed workload / harness stage | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Initial short, before readiness compatibility fix | 19,744.32 | 19,946.62 | 0.990 / 2.206 | 2,103,522 |
| Final short | 19,715.19 | 19,758.05 | 0.998 / 1.010 | 2,103,522 |
| Independent twelve-actual confirmation | 19,689.04 | 20,296.44 | 0.970 / 0.985 | 2,103,522 |

One existing original 513×259 RGBA16 raster exercises the supported
`CompressionLevel.SmallestSize` branch, with filter None and straight RGBA.
Managed/native IDAT payloads are **960,425 / 960,316 bytes**, differing **0.011%**
within the unchanged **5% gate**. Complete packets are 960,650 / 963,202 bytes;
container/chunk overhead is included in timing. No image, depth, quality or
effort grid is introduced.

Both subjects time complete encoding into independent owned output and release.
Managed fresh MemoryStream creation, growth, full word packing, public encoding
and disposal are included, without a redundant ToArray. Native source/context/
packet wrappers and the prepared endian buffer are reused; full LE-to-BE sample
conversion, encoding, real packet acquisition and release are timed. No output
is cached. Startup, I/O, IPC, fixture creation and validation are excluded.
Native allocations are **unmeasured**; managed figures include harness overhead.

The original public-API adapter adds only a scoped RGBA16 level9 operation;
older operation levels remain fixed. Exact analytical pixels, normal/scalar CLI
packets, alpha/hidden RGB, source preservation and independent owners pass before
timing. Sequential Release runs use one pinned CPU, one codec thread, normal SIMD,
disabled tiering and workstation nonconcurrent GC. Every quiet guard passes.
The final screen and repeat share one immutable production/harness build. All
twelve confirmation actuals are retained; BDN selects eleven. The upper includes
selected and all-actual intervals against the faster native bracket's lower bound.

A harness correction scopes newer generic-worker readiness fields to that worker;
separate Fastest/stored workers retain their existing schemas and checks. All
three existing RGBA16 encoder modes pass correctness-only smoke with exact output.
The first smoke attempt loaded stale cached child DLLs and was rejected by result/
receipt matching. Its wrong-version data is retained **unscored**. Verified current
DLLs were preloaded and the same generated child was rerun without rebuilding.
The initial valid SmallestSize screen remains separately retained. Two harness
builds reflect different inputs; the native adapter is built once.

Only two benchmark inputs and the original native adapter change. Existing
**3,432 Release tests per OS**, including **898 live comparisons**, and the same
actual package's consumer plus all three examples on Windows/Linux are reused
from the [JPEG qualification](WARM-JPEG-BASELINE-AC-RUN.md). No production rebuild,
full suite, package proof, method audit or profile is repeated.

[Numeric evidence](warm-png-smallest16-evidence.json) retains every actual,
allocation, effort/size gate, guard, source/assembly/job/native identity, both
smoke outcomes and qualification reuse. Stop this confirmed pair; full required
warm coverage stays open.
