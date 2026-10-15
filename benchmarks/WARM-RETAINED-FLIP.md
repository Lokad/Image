# Horizontal flip with two live outputs

Two-result RGBA8 bursts independently confirm **1.447× FFmpeg / conservative
upper 1.484×**, with **128 allocated B per image**. The original screen was
**2.899×**, allocating **266,009 B per image**. Managed elapsed time falls **51%**
and allocation falls **99.95%**, with exact pixels and ownership preserved.
This fixed boundary clears the target; uniform current-source coverage remains open.

| Measurement, per image | Lokad.Image µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Original single-slot pool | 27.28 | 2.899 / 3.157 | 266,009 |
| Two-slot screen | 12.37 | 1.386 / 1.587 | 128 |
| Independent confirmation | 13.26 | 1.447 / 1.484 | 128 |

All twelve confirmation actuals are retained and selected. Both versioned quiet
guards pass for every retained run. The bound includes the more conservative
filtered/all-actual interval against the fastest native bracket's lower bound.
This is a separate ownership boundary from immediate single-result release.

Each timed unit calls the public horizontal flip twice on the same authored
513×259 RGBA8 source, holds both independently owned complete outputs, then
releases both. FFmpeg's public hflip graph performs the same two transforms and
holds both frames before either unref. BenchmarkDotNet declares
`OperationsPerInvoke=2`; native raw pair counters are retained and multiplied by
two for published image-operation counts. Elapsed time is unchanged. Generated
job configuration and every raw/normalized native sample are verified, so the
table consistently reports **time and allocation per image**.

The old pool kept one idle array, requiring another image-sized allocation in
each burst. One production primitive now retains **at most two exact-size idle
arrays**, with a short lock protecting both slots and the unchanged **combined
4 MiB pixel-storage ceiling**. The separate PNG IDAT pool uses the same primitive
within its unchanged **combined 1 MiB ceiling**. Only released arrays are retained;
sources and result owners are not cached. Eligible size misses discard idle
entries; oversized requests bypass the cache. Duplicate idle returns cannot
create aliased leases. Input/result admission, complete overwrite, failure cleanup,
cancellation and live-owner isolation remain unchanged.

Managed and native validation checks both whole images, hidden RGB, alpha and
source preservation while the results are live. Mutating one live result must
leave its sibling and source unchanged. The original source raster is reused;
there is no additional original image, profile grid or codec/depth conversion.
Native row alignment/padding is recorded, with no timed packing copy.

Sequential Release runs use one pinned CPU, one filter thread, normal SIMD,
tiering off and workstation nonconcurrent GC. Complete transform, output storage
acquisition/materialization and release are timed. Native graph/source/frame
wrappers are prepared and reused. Startup, IPC, file transport, hashing and
validation are excluded. Native allocation is unmeasured; managed figures
include harness overhead.

Before full qualification, shared-path short regression means pass at **1.239×**
for single-output RGBA8 flip (**128 B/op**) and **1.855×** for RGBA8 PNG decoding.
These three-actual controls do not replace their earlier independent certificates.
Exactly three workload types and the existing two original raster recipes suffice.

Windows and Linux each pass **3,289 Release tests**, including **896 live FFmpeg
comparisons**, plus **51 focused checks per mode per OS** with normal intrinsics
and intrinsics disabled. Six new internal cases cover combined ceilings and
duplicate returns; existing concurrent lease checks pass. Two public cases cover
stale-buffer overwrite, exact admission while another output is live, hidden
samples and owner independence at both depths. The consumer and all three examples
pass through the **same actual dependency-free Release package** on both systems.
The fully qualified Linux DLL equals the timed DLL.

Screen, confirmation and regression controls reuse the same immutable candidate
library/harness and prepared children. Initial reference-assembly resolution and
filter-classification refusals occur before timing and are retained. The driver
is corrected against verified compiled assemblies; confirmation preparation uses
`ProduceReferenceAssembly=false` to avoid stale reference metadata. Existing
single-output native binaries are preserved; the original two-result worker is
development-only. No upstream sources or tables are copied.

The [numeric evidence](warm-retained-pool2-evidence.json) retains complete samples,
raw pair/image accounting, assemblies, source and package qualification. Stop
this fixed pair; remaining material public/codec/current-source coverage stays open.
