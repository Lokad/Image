# Dense indexed-palette8 PNG expansion

One noninterlaced indexed PNG, depth 8, filter None and zlib level 6, derives from
the existing original 513x259 RGBA8 raster. Its red byte becomes index i. The
original 256-entry palette is R=i, G=255-i, B=(73*i+19)%256, A=i; index zero
retains nonzero hidden RGB. This is an explicit format projection, with no new
original image. Original development-only code authors IHDR/PLTE/tRNS/IDAT/IEND
because the production encoder has no palette option.

Both subjects decode the identical resident bitstream into independent owned RGBA8.
Native timing includes one-thread PAL8-to-RGBA conversion, output allocation and
release; contexts/frame wrappers are reused. Startup, I/O, IPC, authoring and
validation are excluded. Normal/scalar native pixels, all alpha/hidden RGB,
source integrity and independent owners are exact.

| Quiet screen | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Generic palette expansion | 2,894.70 | 366.65 | 7.895 / 9.063 | 621,277 |
| Direct packed row writes | 992.81 | 365.69 | 2.715 / 2.954 | 621,315 |

The screened mean falls 65.7%. This fixed pair remains **above 2x / open**;
no twelve-actual repeat was run. Both screens pass both unchanged quiet guards
and retain all three actuals. The upper uses the larger filtered/all-actual bound.
Allocations are raw GC bytes/op rounded up, including harness overhead; native
allocation totals are not measured. Other required warm coverage remains open.

A six-second baseline profile attributes 30.0% to generic WritePixel expansion,
26.0% to stride access, 10.2% to pixel access and 4.1% to channel dispatch.
One production primitive captures the existing palette/tRNS and output row spans,
validates each index before writing it, then packs the selected RGB/alpha into one
32-bit row write. Missing alpha entries are opaque; zero alpha retains RGB.
No palette workspace, sample cache, arithmetic or admission change is introduced.
Dense Adam7 passes qualify; packed/sparse mapping and error precedence remain
unchanged, with cancellation at most every 4096 pixels.

The updated six-second profile puts BCL inflation at 36.8%, ReadLiteralRun at
23.5%, framing validation at 10.2% and direct palette lookup at 20.1%. Both
profiles pass their original idle guards and make no timing claim. Palette8 is
the largest measured matched gap; investigate one substantial framing algorithm
change on this same narrow pair. Preserve declared-window/output-count checks,
work/memory admission, tails, cancellation and error precedence. Earlier wider
refills, variable-length literal groups and small classifiers did not provide
useful parity gains; do not blindly repeat them.

Sixteen new controls cover all indices/channels, full/missing/partial alpha,
hidden RGB, unaligned guards, cancellation boundaries and valid-prefix writes
before an invalid index. Each OS passes 2,533 Release tests: 783 internal,
854 public and 896 live FFmpeg. The row controls and eight authored PNG tests
covering 72 original profiles also pass with SIMD disabled. The same actual
Release package passes all three usage examples on both OSes; all 580 production
methods and flags match fresh/timed assemblies to that package. Legacy RGBA8/16,
RGB8/16, gray8/16 and gray-alpha8/16 native-worker controls also pass.

The [numeric evidence](warm-pngpalette8-row-evidence.json) retains every actual,
bracket, source/input/derived-raster hash, four actual child identities and raw/
QA/profile archive identities. No upstream source, binary or corpus enters
tracked files. See the [material coverage audit](WARM-COVERAGE.md) for remaining work.

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Png -WarmCases DecodePalette8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases DecodePalette8 -Cpu 0 -NoBuild -PrepareWarm
```

Build the development-only native worker with `tools/WarmReference/build-png-linux.sh`
first. The full required warm target remains open.
