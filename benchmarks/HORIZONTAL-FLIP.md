# Horizontal flip — 2026-10-09

Safe span-based SIMD and whole-pixel row reversal reduce the warm public
horizontal-flip operation by 15.11x for RGBA8 and 6.75x for RGBA16 in this narrow
VM measurement. This includes result allocation and disposal. The change removes
the generic per-pixel coordinate delegate and span copies, while retaining exact
channels, hidden RGB, orientation, output ownership, resource accounting and
cancellation checkpoints. Vector256/Vector128 paths have a scalar fallback.

| Precision | Before µs | After µs | Warm speedup |
| --- | ---: | ---: | ---: |
| RGBA8 | 556.76 | 36.86 | 15.11x |
| RGBA16 | 560.92 | 83.06 | 6.75x |

The equivalent FFmpeg comparison streams 32 repeats of each source from one raw
file and writes every edited frame, with one process per subject. It includes
startup/JIT, raw transport and filesystem work. These are complete batch means;
divide by 32 for per-frame figures. They do not establish isolated native kernel
throughput or warmed persistent-service throughput.

| Precision | Before managed ms | After managed ms | After FFmpeg ms | Current batch ratio |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 | 267.59 | 243.56 | 195.97 | 1.24x |
| RGBA16 | 462.94 | 460.75 | 387.85 | 1.19x |

The RGBA8 managed batch mean falls by 9.0%. The RGBA16 change is only 0.5%,
within observed variability; no RGBA16 batch improvement is established.
The original RGBA8 native control has an 18.1% relative standard deviation and
is retained as variable evidence, rather than used to claim a parity-ratio gain.
Current native relative standard deviations are 3.3%/6.3% at depths 8/16. The
RGBA16 warm candidate has 6.1% relative standard deviation. These Short runs guide
this primitive checkpoint; broader claims or small differences need repeats.
Raw-file transport remains a material investigation target outside the flip.

Only two original 513x259 rasters are used: the repository's asymmetric-v1
formula at depths 8/16 with alpha set to zero at both first-row ends. Setup and
cleanup validate independent source-to-destination placement, every channel and
all 32 ordered batch frames. Baseline/candidate outputs are identical. There is
no encoder calibration, extra content corpus or broader parameter sweep.

Two sequential Release BenchmarkDotNet 0.15.6 Short jobs ran on the dedicated
Linux VM: one baseline and one candidate, each with six cases, one launch,
three warmups and three measured iterations. The machine presents four AMD EPYC
9V45 vCPUs and about 15.6 GiB RAM, Ubuntu 24.04.4 x64, SDK 10.0.204/runtime 10.0.8,
and the pinned FFmpeg n8.1 with normal CPU dispatch and one codec/filter thread.
Pre-run quiet samples pass the fixed CPU/I/O/steal/load thresholds. Immediate
post samples fail the rolling load threshold; later cooldown samples pass.
All observations, including failures, remain in the
[original numeric snapshot](results/horizontal-flip-2026-10-09.json), together
with individual measurements, statistics, allocation counters, exact-output
hashes and separate untimed child-memory counters. Source inventories identify
the measured bytes; the candidate was measured before its implementation commit.
Worker and ImageProbe library identities are recorded separately because they
are distinct builds of the same source, serving distinct measurement boundaries.

Correctness validation passes on Windows and Linux, including live FFmpeg
comparisons, whole-pixel/vector/chunk boundaries, outstanding orientation,
ownership, exact memory/work limits and cancellation. Targeted transforms also
pass with AVX disabled and with all hardware intrinsics disabled. The same
actual Release NuGet package passes on both platforms with zero dependencies.
Local Dry runs verify all six cases and exact outputs; their timings are not
performance evidence.

Reproduce the narrow job on a quiet dedicated VM after restoring dependencies:

```powershell
dotnet build benchmarks/Lokad.Image.Benchmarks -c Release --no-restore --tl:off -m:1
./bench.ps1 -DedicatedVm -HorizontalFlip -NoBuild
```

Baseline code and the identical harness are at revision `77703b9`; the optimized
source is tracked with this report. Run subjects sequentially and preserve each
run's full JSON and validation receipts. Use `-Smoke` instead of `-DedicatedVm`
for an execution check. The tables can be regenerated without measurement:

```powershell
python benchmarks/results/render-horizontal-flip.py
```
