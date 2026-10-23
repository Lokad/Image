# Dense gray-alpha8 PNG expansion

One noninterlaced gray-alpha8 PNG, filter None and zlib level 6, derives from the
existing original 513x259 RGBA8 raster: red becomes gray, green/blue equal red,
and alpha stays unchanged, including hidden RGB. No original image was added.
Both subjects decode the identical resident bitstream into independent owned RGBA8.
Native timing includes one-thread YA8-to-RGBA conversion, output allocation and
release; contexts/frame wrappers are reused. Startup, I/O, IPC, preparation and
validation are excluded.

| Quiet run | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Generic expansion screen | 3,827.40 | 1,139.77 | 3.358 / 4.036 | 660,975 |
| Direct row screen | 1,569.90 | 1,136.26 | 1.382 / 1.437 | 661,014 |
| Independent row confirmation | 1,574.21 | 1,147.07 | 1.372 / 1.409 | 661,014 |

The screened mean falls 59.0%. This fixed pair **confirms below 2x** with all
12 actuals retained and both unchanged quiet guards passing. Both scored screens
also pass both guards. The upper uses the larger filtered/all-actual bound.
Allocations are raw GC bytes/op rounded up, including harness overhead. Native
allocation totals are not measured. Other required warm coverage remains open.

A six-second diagnostic profile attributes 30.1% to generic WritePixel expansion,
20.0% to stride access, 6.6% to pixel access and 2.8% to channel dispatch. Its
post-profile quiet guard fails at 1.2% aggregate I/O wait (threshold 1%); the failure
is retained unchanged and the profile is diagnostic only, with no timing claim.
No profile repeat was performed.

One production primitive expands dense gray-alpha8 rows directly. Bounded
16-byte loads and AVX2 byte shuffles produce eight RGBA8 pixels, retaining every
alpha and hidden RGB sample. Scalar tails/other hardware and sparse passes keep
their mapping. Dense Adam7 passes also qualify. Arithmetic, source pixels,
work/memory admission and cancellation stay unchanged; no workspace is introduced.

Fourteen new controls cover all 65,536 gray/alpha combinations, unaligned source
and destination guards, vector tails, widths around the cancellation boundary,
and cancellation before writes. Each OS passes 2,476 Release tests: 726 internal,
854 public and 896 live FFmpeg. The row controls and eight authored PNG tests
covering the existing 72 original profiles also pass with SIMD disabled.
The same actual Release package passes all three usage examples on both OSes.
All 576 production methods and flags match fresh/timed assemblies to that package.
Managed and native normal/scalar pixels, alpha, source and independent owners are
exact; existing RGBA8/16 native-worker controls also pass.

The [numeric evidence](warm-pnggrayalpha8-row-evidence.json) retains every actual,
bracket, source/input/derived-raster hash, four actual child identities and raw/QA
archive identity, including the failed post-profile guard. No upstream source,
binary or corpus enters tracked files. Stop tuning this fixed pair; next screen
one RGB8 expansion profile from the existing raster.

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Png -WarmCases DecodeGrayAlpha8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases DecodeGrayAlpha8 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmConfirm -WarmFamily Png -WarmCases DecodeGrayAlpha8 -Cpu 0 -NoBuild -PrepareWarmConfirm
```

Build the development-only native worker with `tools/WarmReference/build-png-linux.sh`
first. See the [material coverage audit](WARM-COVERAGE.md) for remaining scope.
