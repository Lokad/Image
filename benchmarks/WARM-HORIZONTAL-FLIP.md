# Warm single-CPU horizontal flip

The two measured 513x259 horizontal-flip cases meet the 2× FFmpeg target after
bounded output-storage reuse. This result covers RGBA8/RGBA16 with sequential
disposal on the dedicated reference VM. Other dimensions, retained-output
patterns, editing operations and codecs remain open.

Each confirmation uses twelve requested managed iterations and twelve native
samples before and after them, with an independent second launch. The numeric
[snapshot](results/warm-horizontal-flip-2026-10-09.json) preserves original managed
statistics, all actual measurement counts/times, native counters/times, exact
checks, component identities and quietness observations.

| Case | Before managed µs | After managed µs | Worst mean ratio to FFmpeg | Conservative ratio upper |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 | 180.20–180.92 | 11.79–12.05 | 1.25× | 1.28× |
| RGBA16 | 352.90–354.90 | 21.68–22.00 | 0.96× | 1.06× |

Ratios use the faster native pre/post mean for each launch, and the table takes
the worst launch. The conservative estimate divides the managed 99.9% Student-t
upper mean by the lower native bracket mean interval. Native intervals use the
managed critical value, with no fewer native samples; managed outlier removal
can make that choice more conservative. These per-mean intervals do not imply
a simultaneous guarantee for every input or machine. All requested actual
measurements are retained alongside BenchmarkDotNet's resulting statistics.

The three-sample screens had conservative bounds near 2.03×, so they remained
inconclusive and triggered longer samples on the same two inputs. No additional
images or profile sweep was used.

## Operation and runtime boundary

Both sides consume already-resident packed RGBA samples, materialize independent
owned pixel results, and release them within timing. Native samples use the
worker's CLOCK_MONOTONIC intervals, including real hflip graph execution and
result release. Managed samples include the public call, admission, metadata,
output acquisition, complete pixel writes and disposal. Launch, IPC, file I/O
and hashes are outside both intervals. No encoded/decoded result is cached.

The original development adapter uses public FFmpeg filter APIs with a persistent
graph, source and frame wrappers. FFmpeg may reuse released storage. Its padded
RGBA rows are exposed directly; recorded native strides differ from the
library's tight rows. There is no unnecessary native packing copy.

Both subjects and their runtime threads are pinned to CPU 0, one logical CPU.
FFmpeg filter threads are one, with other graph threading disabled. Managed
workers use processor count one, workstation GC, background GC disabled and
tiered compilation disabled. Optimized JIT code is prepared before measurement.
The machine runs Ubuntu 24.04.4 x64 on AMD EPYC 9V45, SDK 10.0.204/runtime 10.0.8,
BenchmarkDotNet 0.15.6 and the pinned source-built FFmpeg n8.1.

Before-run quiet checks passed. Immediate post-run checks retained elevated
rolling load from the preceding work; failed I/O-wait observations and later
passing cooldown observations are preserved too. Samples ran sequentially,
without concurrent builds or other benchmarks. Quiet checks cannot establish
future hypervisor isolation.

## Changed primitive and memory

Horizontal flip now acquires an exact-size byte array from a single idle slot.
It retains at most 4 MiB, never larger arrays or live owners. Every sample is
overwritten before the result is returned, including alpha and hidden RGB.
Live outputs keep separate storage. Atomic, idempotent disposal returns each
lease once; exceptions release private partial results. The SIMD pixel loop is
unchanged from the earlier round.

Warm allocation falls from about 532 kB/1.06 MB per operation to 128 bytes at
both depths; the confirmation measurements report no GC collections. Source
plus output byte admission and four work units per pixel are unchanged. Idle
cache memory is separately bounded, outside per-operation admission. Buffer
creation remains zero initialized. Row spans must cease being used before
their owner is disposed; released storage can be reused.

The bounded cache does not accelerate arrays above 4 MiB. That branch and
patterns that retain many live outputs need their own warm measurements before
any broader parity claim. The earlier raw-file ratios include startup/transport
and do not certify this warm target.

## Validation and reproduction

Windows and Linux each pass 1,679 Release tests with live FFmpeg: 102 internal,
683 public API and 894 differential. Each platform also passes 65 focused
transform tests with AVX disabled, then with all hardware intrinsics disabled.
Tests cover exact-size retention limits, concurrent leases, repeated disposal,
dirty reused storage, partial-operation failure, live output independence,
precision, orientation, hidden RGB and unchanged admission boundaries.
The native counter/exact checks and ten protocol/affinity negative controls pass.
The same actual dependency-free Release package is verified on both platforms,
including all three usage-guide examples. Its SHA256 is
`56c2770b2dfcc5ac6b82f1a01227af972c2480c5484285e55e3a86558ad75d24`.

After provisioning the pinned ignored native tools on the dedicated Linux VM:

```bash
source external/linux-x64/environment.sh
bash tools/WarmReference/build-linux.sh
python3 tools/WarmReference/verify-linux.py --cpu 0
```

Then, in PowerShell:

```powershell
dotnet build benchmarks/Lokad.Image.Benchmarks -c Release --tl:off
./tools/WarmReference/capture-source.ps1
./bench.ps1 -Smoke -Warm -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -Cpu 0 -NoBuild
# Extend only these cases when uncertainty requires it; repeat independently.
./bench.ps1 -DedicatedVm -Warm -WarmConfirm -Cpu 0 -NoBuild
```

Capture and review quietness around statistical runs, with cooldown between
launches. Benchmarks remain development tools; no native assets or dependencies
enter Lokad.Image. Regenerate this report with
`python benchmarks/render-warm-horizontal-flip.py`.
