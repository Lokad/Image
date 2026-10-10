# Warm single-CPU JPEG byte-output round

This round changes one primitive: LimitedWriteStream.WriteByte. A safe
one-byte stack span replaces the inherited per-byte heap array, while
retaining the same span write and admission checks. It measures the existing
513x259 RGBA8-to-4:4:4 pair after the accepted
[RGB extraction round](WARM-JPEG-RGB.md). JPEG remains above 2x.

The [original numeric evidence](warm-jpeg-byte-evidence.json) retains actual
managed samples/statistics, native elapsed/count brackets, source/tool/DLL
identities, quality, output sizes, allocations and quietness. Regenerate this
document with `python benchmarks/render-warm-jpeg-byte.py`.

| Phase | Repeat | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | --- | ---: | ---: | ---: | ---: |
| baseline | 1 | 17641.77 | 3829.27 / 3791.85 | 4.65x | 10.11x |
| baseline | 2 | 17662.73 | 3776.04 / 3779.35 | 4.68x | 5.09x |
| byteOutput | 1 | 16349.22 | 3784.41 / 3783.31 | 4.32x | 4.91x |
| byteOutput | 2 | 16281.13 | 3794.93 / 3783.91 | 4.30x | 4.71x |

Measured 2026-10-10, AMD EPYC 9V45, Linux Ubuntu 24.04.4 LTS (Noble Numbat),
SDK 10.0.204, runtime .NET 10.0.8 (10.0.8, 10.0.826.23019). Both subjects
use CPU 0 only and one codec/conversion thread. Normal native SIMD is enabled;
managed optimized JIT has tiering disabled, with workstation GC and background
GC disabled. Two independent short repeats guide priority and retain raw
uncertainty. This round does not certify a passing case or uniform parity.

Both intervals include resident original RGBA8, exact single-round alpha
flattening on RGB 0x204060, full-range BT.601 conversion, 4:4:4 encoding and
complete independent output acquisition/release. Native contexts, scratch and
wrappers are reused; every pixel is reprocessed. Fresh managed MemoryStream
creation, growth and disposal are timed. Startup, IPC, I/O, hashing, validation
and quality enumeration are excluded from both intervals.

One existing raster and frozen qscale 5 / quality 69 pair are reused.
Its RGB source-error gap remains 0.08244 dB, within the unchanged 0.25 dB band.
The original 7-setting preparation remains; no extra quality search or
benchmark raster is added. Setup/cleanup freshly check selected bitstreams,
geometry, decoder agreement, alpha, sampling, source integrity and ownership.
Output remains 324,002 managed / 133,041 native bytes;
managed fixed Huffman and native optimal Huffman effort remain explicit.

Managed BDN allocation changes from 11,406,778 to
1,051,673 bytes per operation. Color matrix, sampling, DCT,
entropy work and output bytes are unchanged. Every write retains cancellation
and quota checks, span dispatch, successful-write accounting and caller stream
ownership. No output batching, image cache or row/block buffer is added.

Four new original controls verify mixed byte/span quota boundaries, cancellation
precedence, failed caller writes and zero per-byte heap allocation with a span
sink. Windows/Linux full Release/live suites pass 1,822 tests (242 internal,
686 public, 894 FFmpeg comparisons). Linux additionally passes 26 internal and
70 public JPEG/output controls in each of normal, AVX-512 disabled, AVX disabled
and all intrinsics disabled modes. The same actual dependency-free Release
package and all three usage examples pass both OS:
SHA256 `188778503f31bddf92319908d8857bfe8654da22e7f04a52909771db7048ce11`.

Source-byte and pre-run quietness checks pass. Original post-run rolling-load
failures and later passing cooldown observations remain in the evidence; no
threshold is weakened and hypervisor isolation is not inferred. Other JPEG
paths, PNG, WebP and remaining editing operations still need their own evidence.

Reproduce sequentially on the quiet dedicated Linux VM using the
[native worker instructions](../tools/WarmReference/README.md):

```powershell
./tools/WarmReference/capture-source.ps1
bash tools/WarmReference/build-jpeg-linux.sh
dotnet build Lokad.Image.slnx -c Release --no-restore --tl:off -m:1
# Reuse and freshly validate the frozen preparation; do not enumerate quality.
./bench.ps1 -Smoke -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild
# Repeat after a passing cooldown.
./bench.ps1 -DedicatedVm -Warm -WarmFamily Jpeg -Cpu 0 -NoBuild
```
