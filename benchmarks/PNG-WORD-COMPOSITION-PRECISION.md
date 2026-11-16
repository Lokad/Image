# PNG16 composition precision

The original retained-native comparison remains **uncertain** on the current
immutable actual-package DLL: **3,084.462 / FFmpeg 2,985.741 µs**, ratio **1.033064**,
conservative interval **0.920724–1.103471**, **338,160 B/op**. No production change,
CPU gain or demonstrated CPU gap follows. The equivalent
[fresh-state one-shot comparison](PNG-COMPOSITION-ONE-SHOT.md) remains passing and
stopped; its native preparation boundary differs and does not retire this control.

One prespecified capture extends equal batches 256→1,024 complete operations,
retaining twelve alternating pairs, twelve native batches before/after and four
warm batches on each side. The old four×256 warmup was already substantial: this
is not a startup correction. Every actual, the paired log confidence and managed
all-actual/diagnostic confidence against the fastest native bracket remain.
Unfiltered all-actuals always constrain acceptance. No forced GC, sample exclusion,
settings/fixture change or identical favorable retry. Natural nonconcurrent GC,
CPU0 and idle-v2 guards pass. The larger capture does not improve confidence;
that adverse result stays visible rather than authorizing another capture.

| Original complete public boundary | Managed / FFmpeg µs | Conservative interval | Managed B/op |
| --- | ---: | ---: | ---: |
| Earlier twelve×256 actuals | 2,871.463 / 2,943.036 | 0.928688–1.021958 | 338,160 |
| Current twelve×1,024 actuals | 3,084.462 / 2,985.741 | 0.920724–1.103471 | 338,160 |

GC counts are read outside the same two batch clocks. Each managed actual records
30–32 collections at generations 0, 1 and 2: **378 of each over 12,288 operations**.
Actual allocation is 338,159.266–338,159.750 B/op. Managed actuals span
2,891.960–3,123.958 µs; native bracket means 3,026.049/3,148.443/2,985.741 µs also
move. Collection counts do not isolate pause time, explain the historical slow
batch, or justify filtering. No cross-run slowdown or GC-exclusive causal claim.
The concrete next diagnostic is substantial allocation/storage lifetimes, before
any further timing or production optimization.

The frozen 960,565-byte PNG, every full-word sample, hidden RGB, alpha, source,
stream lifetime/EOF and two-live-output independence remain exact. The operation
still performs public Decode(Stream) → AutoOrient(Normal) → centered 256×129 crop
→ nearest fit inside 128×64, producing independently owned 127×64 pixels. All
managed intermediate owners, fresh stream/acquisition and disposal stay inside.
FFmpeg reacquires the input packet through fresh AVIO/scratch, decodes and executes
its retained single-thread graph, converts full-word output and owns/releases the
result. Necessary native decoder/graph preparation remains retained and explicit.
Resident input, startup, IPC, hashing and verification stay outside equally.

Source-derived simultaneous image payload remains **2,455,088 B**, default idle
payload can retain **2,125,872 B** within the unchanged two-slot/4 MiB ceiling.
Borrowed caller roots bypass that idle storage. These logical capacities differ
from GC allocation; this measurement newly establishes neither admitted/physical
peak nor native allocator retention. No owner, array pool, capacity or API changes.

Only the original public-operation orchestrator changes its allowed batch ceiling
and records untimed GC counts. The original compiled benchmark assembly, frozen
inputs, FFmpeg worker/settings and production package are reused. An overbroad
runtime filename filter initially omitted the benchmark module/deps; the host
rejected before runtime/native workers/statistical samples. Linking the unchanged
modules corrected preparation, with zero repeated completed jobs or builds. Both
attempts remain. One Windows orchestrator build; zero production/embedded/public/
original-harness/reference/fixture/package builds or broad qualification repeats.

Reuse Windows/Linux **6,750 full/live checks each**, unchanged PNG normal/scalar/
AVX512-disabled evidence and the SAME actual Release-package consumer/all three
examples. DLL `58822411f81726ca70da4759e8b51be02cbd1dac7a098532fb8c6fe1b8cc121d`,
package `8ca826fc97e74973349a7c86b30d8e9e85dcc3a128315cf6fa6f75c1e08f705e`.
Publication is postponed and all 289 named material paths remain required.

[Evidence](png-word-composition-precision-evidence.json) pins all actuals, GC
counts, source/operation boundaries, exact producer/reference identities and reused
qualification. Mirrored/member-verified `png-word-composition-precision.tar.xz`:
SHA256 `3d3902d6fe9d671b28e457e5ea2dda7cbdda0f92693d2fa98b2dcae047917670`, 7179340 B /176 members. Its `pcm1148-*`
recipes, original/modified orchestrators, immutable runtime, one frozen input,
quality observations, guards and pinned dependencies reproduce this bounded check.
