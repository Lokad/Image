# Shared image-storage reuse

Retain the verified allocation reduction. The final controlled DLL comparison
does **not** establish a CPU gain: 3,170.933→3,165.833 µs, interval
0.917117–1.083007, with **338,156.20→8,889.94 B/op** (97.4% less allocation).
Earlier prototype CPU gains remain scoped leads, not the accepted final effect.

The existing shared image/font pool now retains four exact-size arrays within
its unchanged **4 MiB total byte ceiling**. Codec-local PNG and asynchronous
encoded-output pools keep two slots and their original policies. Nearest resize
uses the existing fully overwritten result-owner path; every sample is still
computed before publication. No pixel cache, live aliasing, public API or numerical
change. Borrowed caller budgets bypass idle storage; admission, cancellation,
failure cleanup and independently surviving outputs remain explicit.

| Complete current operation | Lokad / reference µs | Conservative interval | Lokad GC B/op |
| --- | ---: | ---: | ---: |
| PNG16 decode/orient/crop/nearest fit; retained FFmpeg decoder/graph | 2,768.861 / 2,939.761 | 0.917058–0.950634, pass | 8,888.16 |
| PNG8 decode/orient/crop/nearest fit; retained FFmpeg decoder/graph | 1,185.833 / 1,481.925 | 0.792432–0.804558, pass | 4,792.08 |
| Owned font load; FreeType 2.13.2 copy/open/retain/dispose | 18.3916 / 13.8037 | 1.194370–1.377474, scoped gap | 176 |
| Complete constrained-fit16 text one-shot; ImageSharp public integration | 1,108.448 / 6,208.691 | 0.159711–0.194348, pass | 306,164.17 |
| Prepare + eight constrained-fit16 draws; ImageSharp public integration | 1,259.794 / 6,835.318 | 0.163796–0.207305, pass | 315,890.25 |
| Owned vertical flip8; FFmpeg owned output | 8.692856 / 8.329975 | 0.916851–1.101581, uncertain | 56 |
| Owned vertical flip16; FFmpeg owned output | 17.657861 / 17.586334 | 0.991207–1.010163, uncertain | 56 |

The original complete PNG operations retain every intermediate, normalized input,
independent result and disposal inside timing. Native decoder/graph reuse is an
explicit repeated-operation boundary; it does not replace the historical
[equivalent one-shot measurement](PNG-COMPOSITION-ONE-SHOT.md). All frozen input,
full-byte/full-word, alpha/hidden-RGB, source/stream/EOF and ownership gates pass.
Twelve adjacent pairs and all three native statistical groups remain, on CPU0
with natural nonconcurrent GC and passing idle-v2 guards. No timing-only favorable
restarts or pixel-kernel tuning.

The complete text workloads use stock **ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 /
ImageSharp 3.1.12**, including every public layout call, all 30 fitting trials,
fresh explicitly cleared RGBA16 canvases, independent outputs and disposal.
They are public-API integrations, not FFmpeg or bare shaping comparisons. Font
validation remains eager versus lazy, and prepared state differs. Preparation
alone is **1,114.504 / 6,116.033 µs**, a scoped diagnostic. The directly measured
eight-draw batch amortizes to **157.474 / 854.415 µs per draw**. Reference
one-shot/batch allocation is **9,256,953 / 9,494,633 B**. Do not sum stage timings
or infer a causal improvement from unrelated historical clocks.

| Original direct nearest control | Lokad / FFmpeg µs | Interval / eligibility | Lokad GC B/op |
| --- | ---: | ---: | ---: |
| RGBA8 513×259→256×129 | 26.193 / 236.163 | 0.037541–0.184516, pass | 112 |
| RGBA16 513×259→256×129 | 18.306 / 169.966 | 0.104191–0.110198, pass | 112 |
| RGBA8 513×259→1027×519 | 382.031 / 1,043.147 | 0.330434–0.403801, pass | 112 |
| RGBA16 513×259→1027×519 | 1,054.276 / 990.953 | 0.971290–1.081140, **unqualified** | 4,264,364 |

All four input/output hashes match the original required controls; native CLI
and scalar CLI samples, alpha, hidden RGB, source and independent outputs are
exact. The last capture completed all twelve actuals, but its post-run CPU0 I/O
wait was **1.4%**, above the **1%** guard. Its complete samples and failed guard
are retained; it proves neither a current pass nor a gap. No favorable retry.
Its 4,264,104-byte output exceeds the unchanged 4 MiB idle cap; the cap was not
enlarged to accommodate it. These four cases already belonged to the finite
289-path scope, with no new fixture or settings grid.

Allocation, retention and capacity are distinct. Source-derived word-pipeline
idle payload increases **2,125,872→2,455,088 B**; its four simultaneously live image
payloads still total **2,455,088 B**. The earlier byte diagnostic measured
**1,062,936→1,227,544 B** idle payload. Full-fit prepared storage remains **4,160 B**,
governed peak **822,460 B**. Approximate warmed full-GC prepared heap deltas remain
**4,336 / 22,504 B**, excluding unchanged idle pools. Physical/native/JIT peaks
are unmeasured. No allocation reduction is presented as proof of CPU savings.

Both OSes pass **6,760 full/live checks** (3,304 internal, 2,508 public, 948 live),
plus **86 internal /441 public checks per normal/scalar mode**. The three named
resize-storage suites are explicitly included. Durable regressions cover dirty
nearest output, exact words, live independent owners, cancellation, admission,
asynchronous survival, four-slot byte caps and concurrent leases. Actual-pool
probes cover two/four-slot invariants; the original two-slot policy trace remains
identical. One canonical core/embedded build per OS, no public/original-harness/
reference/fixture rebuild, one outer nearest helper, one NoBuild Release pack.
Full-suite completion records corrected an expected-count assertion from the
already passing logs; no suite was repeated. The SAME actual timed Linux package
passes its consumer and all three usage examples on both OSes, without rebuilding
the timed DLL. Published 0.2.0 remains unchanged; publication is postponed.

Precisely affected cost evidence: default owned image outputs reaching
`PixelStoragePool.Shared` (clone/crop/orientation/turn/resize and default decoder
results), owned font load/preparation, private PNG compressed/inflated workspace,
and asynchronous `EncodedOutput`. The original two-slot policy is unchanged, but
its implementation changed. Costs refreshed above qualify only those operations;
other affected table entries retain their identified **historical artifact costs
pending the final refresh**, not current-DLL timing proof. Caller JPEG sample
buffers, header/probe boundaries, synchronous encoders not reaching these pools,
borrowed ordinary/prepared text, authored layout/rendering, compositing and
rectangle-copy timers retain their unchanged-path evidence. Encoder bitmath,
declared exact packets, decoder pixels and exact resize arithmetic are unchanged.
No passing kernel is retuned and no whole-library performance claim follows.

[Evidence](composition-storage-reuse-evidence.json) pins source overlays, both
modules, current package, every measurement/guard, qualification and retained
dependencies. Mirrored/member-verified raw `composition-storage-reuse-qualified.tar.xz`:
SHA256 `fb1fdff9501ec39bac0531af689a88a3299b22b9a9a843572c266fc2db14d117`,
34,553,720 B /708 members. It preserves executable
`psr1151-*` recipes, original/candidate code and raw samples. Reconstruct from the
recorded base commit plus archived source overlay and reference dependencies;
use the recorded commands, not a fixture-specific output shortcut. Earlier
[prototype](PNG-COMPOSITION-STORAGE-PROTOTYPE.md), [allocation attribution](PNG-COMPOSITION-STORAGE.md)
and [precision capture](PNG-WORD-COMPOSITION-PRECISION.md) remain retained history.

Continue ordinary-text uncertainty, meaningful font costs and copy gaps by useful
absolute savings and completion of named coverage. Keep the two flip uncertainties
and unqualified nearest16 visible; do not replay unaffected authored passes or
nanosecond probes. The final immutable-package benchmark refresh and full 289-path
objective remain outstanding.
