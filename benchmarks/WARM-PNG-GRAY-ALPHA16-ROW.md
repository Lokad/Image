# Dense gray-alpha16 PNG expansion

One noninterlaced gray-alpha16 PNG, filter None and zlib level 6, derives from the
existing original 513x259 RGBA16 raster: red becomes gray, green/blue equal red,
and alpha stays unchanged, including hidden RGB. No original image was added.
Both subjects decode the identical resident bitstream into independent owned RGBA16.
Native timing includes one-thread YA16BE-to-RGBA64LE conversion, output allocation
and release; contexts/frame wrappers are reused. Full color/alpha samples are
exact, including low bytes. Startup, I/O, IPC, preparation and validation are excluded.

| Quiet run | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Generic expansion screen | 5,608.22 | 1,801.02 | 3.114 / 3.581 | 1,573,632 |
| Direct row screen | 3,459.13 | 1,910.33 | 1.811 / 3.474 | 1,573,649 |
| Independent row confirmation | 3,195.37 | 1,798.83 | 1.776 / 1.785 | 1,573,649 |

The screened managed mean falls 38.3%. The short screen has wide three-sample
native uncertainty and is not certified. This fixed pair **confirms below 2x**
in one independent twelve-actual repeat, retaining all actuals (11 filtered results).
Every scored run passes both unchanged quiet guards. The upper uses the larger
filtered/all-actual bound. No guard failure or retry occurs in this round.
Allocations are raw GC bytes/op rounded up, including harness overhead; native
allocation totals are not measured. Other material warm scope remains open.

A separate six-second profile attributes 24.4% to generic WritePixel expansion,
13.4% to stride access, 3.9% to pixel access and 1.9% to channel dispatch. BCL
inflation takes 25.2% and literal framing 20.6%. Both profile idle guards pass;
profiling makes no timing claim. One production primitive expands dense
gray-alpha16 rows directly. Bounded 16-byte loads and AVX2 byte shuffles produce
four native-endian RGBA16 pixels, replicating complete gray and retaining alpha,
including RGB under zero alpha. Scalar tails/other hardware and sparse mapping
stay unchanged; dense Adam7 passes qualify. Source samples, arithmetic,
work/memory admission and cancellation remain unchanged; no workspace is introduced.

Fourteen new controls cover every value in each 16-bit sample, hidden RGB under
zero alpha, byte order/low-byte precision, unaligned guards, vector tails and
cancellation boundaries. Each OS passes 2,517 Release tests: 767 internal,
854 public and 896 live FFmpeg. The row controls and eight authored PNG tests
covering 72 original profiles also pass with SIMD disabled. The same actual
Release package passes all three usage examples on both OSes; all 579 production
methods and flags match fresh/timed assemblies to that package. Managed and native
normal/scalar pixels, source integrity and independent owners are exact. Legacy
RGBA8/16, RGB8/16, gray8/16 and gray-alpha8 native-worker controls also pass.

The [numeric evidence](warm-pnggrayalpha16-row-evidence.json) retains all actuals,
brackets, source/input/derived-raster hashes, four actual child identities and
raw/QA/profile archive identities. No upstream source, binary or corpus enters
tracked files. Stop tuning this fixed pair; next screen one indexed-palette8
projection from the existing RGBA8 raster. See the [coverage audit](WARM-COVERAGE.md)
for remaining work.

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Png -WarmCases DecodeGrayAlpha16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases DecodeGrayAlpha16 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmConfirm -WarmFamily Png -WarmCases DecodeGrayAlpha16 -Cpu 0 -NoBuild -PrepareWarmConfirm
```

Build the development-only native worker with `tools/WarmReference/build-png-linux.sh`
first. The full required warm target remains open.
