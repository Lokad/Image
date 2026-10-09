# Warm single-CPU JPEG forward-DCT round

This round measures one complete RGBA8-to-4:4:4 JPEG operation on the existing
original 513x259 raster. It establishes a narrow improvement; JPEG remains
above 2x and uniform warm parity is not established.

The [original numeric evidence](warm-jpeg-dct-evidence.json) retains managed
statistics and actual samples, native elapsed/count samples, bitstream and
tool identities, quality, output sizes, allocations and quietness observations.
Reproduce this document with `python benchmarks/render-warm-jpeg-dct.py`.

| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | --- | ---: | ---: | ---: | ---: |
| baseline | 1 | 34979.80 | 3843.08 / 3845.14 | 9.10x | 17.45x |
| baseline | 2 | 35935.22 | 3779.40 / 3983.66 | 9.51x | 13.04x |
| simd | 1 | 24293.01 | 4037.69 / 4059.64 | 6.02x | 11.59x |
| simd | 2 | 23418.43 | 3775.92 / 3792.43 | 6.20x | 7.13x |

Measured on 2026-10-09: AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). Both subjects
use logical CPU 0 only, one codec/conversion thread and normal native CPU
dispatch. Managed workers use optimized JIT with tiering disabled, workstation
GC and background GC disabled. Three 200 ms samples per subject guide priority;
independent repeats retain uncertainty and cannot certify a passing case.

The operation starts with resident RGBA8 and returns complete independent JPEG
bytes. Both subjects include specified single-round alpha flattening onto
RGB 0x204060, full-range BT.601 conversion, 4:4:4 encoding and output release.
Native contexts/scratch and wrappers are reused but all source processing
executes again each time. Managed MemoryStream creation/growth/disposal is
included. Worker launch, IPC, I/O, validation and quality search are excluded.

One native anchor (qscale 5) matches owned quality 69 within
0.08244 dB, against the unchanged 0.25 dB band. Preparation
sampled 7 managed settings on one original raster; benchmarks only
revalidate the selected output. Source-error, decoder-agreement and opaque-alpha
checks pass before and after. The SIMD outputs preserve the exact original
JPEG bytes and all quality metrics; no recalibration or reduced format work.

Managed output is 324,002 bytes; native output is 133,041 bytes.
The managed fixed canonical Huffman tables and native optimal Huffman effort
are declared. This speed improvement does not improve encoded size.
Managed allocation remains 11,406,627 bytes per operation; that separate
allocation cost is retained rather than hidden in a reused output cache.

Separate baseline CPU sampling attributes 36.18%
of workload CPU to forward DCT. Only that production primitive changes:
AVX-512 evaluates eight independent frequencies, AVX four, with a portable
scalar fallback. Every coefficient keeps the original frozen binary64 basis,
two separate multiplications and y/x accumulation order. No FMA, sum
reassociation, new table, block buffer or image cache. Logical work remains
4096 units per block, admitted with cancellation before writing coefficients.
The measured candidate records actual managed SIMD availability.

Windows and Linux pass 1,817 Release/live tests (237 internal, 686 public,
894 FFmpeg comparisons). Thirteen original controls pin all coefficient bits,
input/output guards, exact work, cancellation and invalid lengths. Linux
also passes 21 internal and 70 public JPEG controls in each of four modes:
normal, AVX-512 disabled, AVX disabled and all intrinsics disabled. The same
actual dependency-free Release package and three usage examples pass on both
platforms: SHA256 `7e51d2b9848af1f81291126af264841a4d7b1f466a7b68fcbd17f22fa33b8cd8`.

Every statistical run passes its source-byte and pre-run quietness checks.
Post-run rolling-load failures and later cooldown observations remain in the
evidence; the final cooldown passes without changing thresholds. These checks
do not establish hypervisor isolation. Other JPEG sampling modes/depths,
decoding, WebP and remaining editing paths still need their own warm evidence.

Reproduce on the dedicated quiet Linux environment after provisioning the
pinned development-only native tools:

```powershell
./tools/WarmReference/capture-source.ps1
bash tools/WarmReference/build-jpeg-linux.sh
python3 tools/WarmReference/verify-jpeg-linux.py --cpu 0
dotnet build Lokad.Image.slnx -c Release --no-restore --tl:off -m:1
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-jpeg 0 5
./bench.ps1 -Smoke -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild
# Repeat sequentially after a passing cooldown.
./bench.ps1 -DedicatedVm -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild
```
