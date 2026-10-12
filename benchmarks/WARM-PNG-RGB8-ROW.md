# Dense RGB8 PNG expansion

One noninterlaced RGB8 PNG, filter None and zlib level 6, derives from the existing
original 513x259 RGBA8 raster: RGB stays unchanged and alpha becomes 255.
No original image was added. Both subjects decode the identical resident bitstream
into independent owned RGBA8. Native timing includes one-thread RGB24-to-RGBA
conversion, output allocation and release; contexts/frame wrappers are reused.
Startup, I/O, IPC, preparation and validation are excluded.

| Quiet run | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Generic expansion screen | 4,472.73 | 1,513.05 | 2.956 / 3.129 | 727,254 |
| Direct row screen | 2,149.75 | 1,525.10 | 1.410 / 1.983 | 727,305 |
| Independent row confirmation | 2,139.12 | 1,510.62 | 1.416 / 1.440 | 727,305 |

The screened mean falls 51.9%. This fixed pair **confirms below 2x** with all
12 actuals retained (11 filtered results) and both unchanged quiet guards passing.
Both scored screens also pass both guards. The upper uses the larger filtered/
all-actual bound. Allocations are raw GC bytes/op rounded up, including harness
overhead; native allocation totals are not measured. Other material warm scope
remains open.

A separate six-second profile attributes 29.5% to generic WritePixel expansion,
16.9% to stride access, 5.7% to pixel access and 2.3% to channel dispatch. Both
profile idle guards pass; the profile is diagnostic and makes no timing claim.
One production primitive expands dense RGB8 rows directly. Three bounded
8-byte reads and AVX2 byte shuffles produce eight RGBA8 pixels with opaque alpha.
Scalar tails/other hardware, transparency and sparse passes retain their mapping;
dense Adam7 passes qualify. Source samples, arithmetic, work/memory admission and
cancellation stay unchanged. No workspace is introduced.

Thirteen new controls cover every byte value in each channel, distinct channel/
lane order, unaligned source/destination guards, vector tails, cancellation-boundary
widths and cancellation before writes. Each OS passes 2,489 Release tests:
739 internal, 854 public and 896 live FFmpeg. The row controls and eight authored
PNG tests covering 72 original profiles also pass with SIMD disabled. The same
actual Release package passes all three usage examples on both OSes; all 577
production methods and flags match fresh/timed assemblies to that package.
Managed and native normal/scalar pixels, source integrity and independent owners
are exact. Legacy RGBA8/16, RGB16, gray8 and gray-alpha8 worker controls also pass.

The [numeric evidence](warm-pngrgb8-row-evidence.json) retains all actuals, brackets,
source/input/derived-raster hashes, four actual child identities and raw/QA/profile
archive identities. No upstream source, binary or corpus enters tracked files.
Stop tuning this fixed pair; next screen one gray16 projection from the existing
RGBA16 raster. See the [coverage audit](WARM-COVERAGE.md) for remaining work.

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Png -WarmCases DecodeRgb8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases DecodeRgb8 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmConfirm -WarmFamily Png -WarmCases DecodeRgb8 -Cpu 0 -NoBuild -PrepareWarmConfirm
```

Build the development-only native worker with `tools/WarmReference/build-png-linux.sh`
first. The full required warm target remains open.
