# Current resize confidence

Two existing RGBA8 controls are measured on the unchanged actual 0.2.1 development
package DLL. After the [stride trial](RASTER-STRIDE-PREPARATION-TRIAL.md), separated
baseline jobs were uncertain and native clocks moved materially. The existing
counterbalanced driver supplies twelve adjacent pairs of 64 complete operations,
alternating managed/native order, with twelve native batches before and after.
Every actual remains. Bounds retain the worse paired-log and all-actual/native-
bracket confidence; the upper-outlier diagnostic cannot improve the final gate.

| Existing complete 513×259 resize | Managed / zimg µs | Ratio range | Direct managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| TightDown8 → 256×129, input+output+4 KiB | 466.97 / 421.51 | 1.004–1.172 | 112 | Gap |
| Wide8 → 128×64, default workspace | 371.91 / 377.37 | 0.953–1.012 | 198,350 | Uncertain |

This resolves the next action, not an overall speedup: investigate the bounded
reduction gap; do not tune Wide8 as a demonstrated gap. Earlier source-valid
captures remain history, including adverse and uncertain results. No further
sampling is taken merely to obtain favorable bounds. Current resize scope is
73 controls: 63 pass / one gap / nine uncertain. Stop passing controls.

Managed timing includes public Resize, preparation, filtering/exact repair,
independent output allocation/materialization/disposal and delegate dispatch.
The labeled FFmpeg zscale backend retains its zimg graph/scratch; native timing
includes required packed/planar normalization, halo, filtering, output packing
and independent pixel allocation/release, excluding IPC. This is not an equivalent
one-shot comparison; the separate qualified one-shot/prepared views remain.
Setup, fixture decoding, JIT and validation are outside both clocks. Native time
in the table is the fastest complete before/paired/after bracket mean, not an
average that hides clock shifts. Bounds include all three brackets.

Original inputs/settings/reference identities, exact rational/managed samples,
approved native RGB/alpha bounds, immutable source and independently owned output
all pass. CPU0, one thread, optimized JIT, workstation nonconcurrent GC and idle-v2
before/after guards remain. All output checks are reused intact from the frozen
harness, with current source/DLL/package pins verified.

Allocations use direct complete-operation counters and differ slightly from BDN
overhead accounting; no allocation saving is claimed on unchanged code. Retained
state and admission capacity remain unchanged, with no new managed workspace,
pool or owner. Physical/native/JIT peaks are unmeasured. There are zero production,
embedded, public-test, harness, driver, native or fixture builds and no broad or
package qualification repeat. The exact same qualified source/modules/package
reuse Windows/Linux 6,511 full/live checks and actual-package/all-three-example
consumer evidence from [bilinear metadata](BILINEAR-OPERATION-METADATA.md).

[Evidence](resize-current-adjacent-confidence-evidence.json) retains every pair,
bracket identity, source/pixel/ownership gate and original boundary. Raw
`resize-current-adjacent-confidence.tar.xz`, SHA256
`549cefa20ea4aefeb3516bd2cc537bd502fad0f25f689e28e0450fd5f07047b6`
(45,728 B / 33 members), is mirrored and member-verified; prior production and
trial archives remain dependencies. All 289 material paths remain required.
Publication is postponed. Next assess shared coefficient preparation and its
output-backed storage/tail tradeoff against the existing bounded-byte attribution;
no instruction/sample count alone establishes a useful design.
