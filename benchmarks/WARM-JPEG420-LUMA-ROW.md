# JPEG 4:2:0 luma row coverage

One existing original 513x259 RGBA8 raster is flattened once onto RGB 0x204060
and encoded to independently owned JPEG bytes. Both subjects include conversion,
encoding, output acquisition and release; managed MemoryStream creation/growth
is included. Native context/conversion/frame/packet wrappers are reused, with
one CPU and one codec/conversion thread. Startup, I/O, IPC and validation are
excluded. No source image or quality sweep was added.

Native qscale 5 and managed quality 69 are fixed. Their source-PSNR gap is
0.106110 dB inside the unchanged 0.25 dB band; opaque alpha and bounded decoder
agreement pass. Preparation evaluates exactly one managed setting. This measures
the library's default sampling mode, not its default quality number.

| Quiet screen | Lokad.Image ms | FFmpeg ms | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Original 4:2:0 path | 7.656 | 1.820 | 4.206 / 4.489 | 527,251 |
| One-to-one luma row SIMD | 6.258 | 1.808 | 3.462 / 3.731 | 527,251 |

The screened mean falls 18.3%; this case remains **above 2x / open**. Both screens
pass both quiet guards, retain all three actuals, and use the larger filtered/
all-actual conservative bound. No longer repeat was run. Native allocations are
not measured. Compression efficiency parity is not established.

A separate six-second profile attributes 21.1% to ReadRgb, 7.8% to stride access
and 2.4% to pixel-view access; 46.5% falls in Encode, including inlined sampling.
The existing SIMD row kernel was limited to whole-image 4:4:4. Qualifying the
actual component sampling factors admits one-to-one luma in 4:2:0/4:2:2. Chroma,
right-edge padding, other depths/hardware, arithmetic, work and memory admission
remain unchanged. Managed and native encoded bytes are unchanged from preparation.

Each OS passes 2,429 Release tests and 14 scalar component-row controls. All
three usage examples pass through the same actual package on both OSes. All 572
production methods and implementation flags match fresh/timed assemblies to that
package. The [numeric evidence](warm-jpeg420-luma-row-evidence.json) contains raw
actuals, brackets, child/source identities, profile and QA archives. A private
verification typo failed after a successful preparation smoke; it was corrected
without rerunning the build/smoke and launched no statistical child.

```powershell
# Linux preparation, outside timing:
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --prepare-warm-jpeg420 0
./bench.ps1 -Smoke -Warm -WarmFamily Jpeg -WarmCases Encode420_8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Jpeg -WarmCases Encode420_8 -Cpu 0 -NoBuild -PrepareWarm
```

Build the development-only worker with `tools/WarmReference/build-jpeg-linux.sh`
first. Next, optimize subsampled chroma conversion/dataflow one primitive at a
time; this is the largest currently measured matched gap.
