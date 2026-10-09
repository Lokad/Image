# Warm single-CPU JPEG RGB extraction round

This round changes one JPEG input primitive: RGBA extraction and flattening.
It measures the existing 513x259 RGBA8-to-4:4:4 encode pair, after the accepted
[SIMD DCT round](WARM-JPEG-DCT.md). JPEG remains above 2x and uniform warm
parity is not established.

The [original numeric evidence](warm-jpeg-rgb-evidence.json) retains managed
statistics/actual samples, native elapsed/count brackets, source/tool/DLL
identities, quality, sizes, allocations and quietness. Regenerate this document
with `python benchmarks/render-warm-jpeg-rgb.py`.

| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | --- | ---: | ---: | ---: | ---: |
| baseline | 1 | 24293.01 | 4037.69 / 4059.64 | 6.02x | 11.59x |
| baseline | 2 | 23418.43 | 3775.92 / 3792.43 | 6.20x | 7.13x |
| rgb | 1 | 17641.77 | 3829.27 / 3791.85 | 4.65x | 10.11x |
| rgb | 2 | 17662.73 | 3776.04 / 3779.35 | 4.68x | 5.09x |

Measured 2026-10-09, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). Both subjects
use CPU 0 only and one codec/conversion thread, with normal native SIMD.
Managed optimized JIT has tiering disabled; workstation GC and background
GC disabled. Independent short repeats guide priority, with raw uncertainty;
no passing case is certified by this round.

Resident original RGBA8 becomes complete independent JPEG bytes. Both subjects
time single-round alpha flattening on RGB 0x204060, full-range BT.601, 4:4:4
encoding and result acquisition/release. Native contexts/scratch/wrappers are
reused but every image is processed again. Managed MemoryStream creation,
growth and disposal are included. Startup, IPC, I/O, validation and quality
search are excluded from both scored intervals.

One existing raster and the frozen qscale 5 / quality 69 pair are reused.
Its RGB source-error gap remains 0.08244 dB (fixed band 0.25 dB).
The original 7-setting preparation is preserved; no quality search or new
benchmark raster is added. Setup/cleanup freshly check selected bitstreams,
decoder agreement, opaque alpha, geometry, sampling, unchanged input and
independent live storage. Exact bitstreams and quality metrics are unchanged.

Managed output remains 324,002 bytes; native 133,041 bytes.
Managed fixed Huffman / native optimal Huffman effort is explicit.
BenchmarkDotNet reports 11,406,778 allocated bytes per operation,
151 bytes above the prior round out of 11.4 MB. A separate direct allocation
check runs 16 complete public encode/dispose calls per library: both have
the same per-call allocation range, and the candidate total is lower by 248
bytes. The raw counts and DLL/output identities are retained; there is no
observed allocation growth in this check. Output byte writes retain their
separate allocation cost. No image cache or row/block buffer is introduced.

Separate accepted-DCT CPU sampling attributes 28.58%
of encode CPU to RGB reading/flattening, including row access. ReadRgb now
acquires one bounded four-channel row span rather than making four Sample
calls. At 8 bits, cancelling the common factor 255 in the specified blend
gives the exact normalized integer numerator divided by 255 with +127 nearest
rounding. The odd denominator has no half ties. The 16-bit once-only blend/
depth reduction remains unchanged. Color matrix, sampling, DCT, entropy, work/
memory/output admission and cancellation are unchanged.

The independent rational-residual test covers 524,288 value/alpha/background
combinations. All 160 frozen original managed JPEG bitstreams reproduce across
five patterns, four sampling modes, two depths and four qualities. These are
correctness checks, not extra timed profiles. Windows/Linux full Release/live
suites pass 1,818 tests (238 internal, 686 public, 894 FFmpeg comparisons).
Linux additionally passes 22 internal and 70 public JPEG controls in each of
normal, AVX-512 disabled, AVX disabled and all intrinsics disabled modes.
The same actual dependency-free Release package and all three usage examples
pass both OS; package SHA256 `4b7200f1b7ccf1c1ebf167207ccbe828debb2adb6d4443d04d785034edf7cbd2`.

Pre-run source-byte and quietness checks pass. Preserve original post-run
rolling-load failures and final passing cooldown checks without weakening
thresholds; these observations do not prove hypervisor isolation. Other JPEG
depth/sampling warm paths, decoding, WebP and remaining edits still need evidence.

Reproduce sequentially on the dedicated quiet Linux VM, using the
[native worker instructions](../tools/WarmReference/README.md):

```powershell
./tools/WarmReference/capture-source.ps1
bash tools/WarmReference/build-jpeg-linux.sh
dotnet build Lokad.Image.slnx -c Release --no-restore --tl:off -m:1
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-jpeg 0 5
./bench.ps1 -Smoke -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild
# Repeat after a passing cooldown; revalidate the same frozen pair.
./bench.ps1 -DedicatedVm -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild
```
