# Dense gray8 PNG expansion

One noninterlaced gray8 PNG, filter None and zlib level 6, exercises the generic
sample-expansion path. It derives from the existing original 513x259 RGBA8 raster:
red becomes gray, green/blue equal red and alpha becomes 255. No original image
was added. Both subjects decode the identical resident bitstream into independent
owned RGBA8. Native timing includes one-thread GRAY8-to-RGBA conversion, output
allocation and release; contexts and frame wrappers are reused. Startup, I/O,
IPC, preparation and validation are excluded.

| Quiet screen | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Generic gray8 expansion | 3,093.11 | 390.50 | 7.921 / 12.431 | 620,215 |
| Direct dense row expansion | 836.63 | 372.62 | 2.245 / 3.003 | 620,246 |

The screened mean falls 73.0%. This case remains **above 2x / open**; no longer
repeat was run. Both scored screens pass both unchanged quiet guards and retain
all three actuals. The upper takes the larger filtered/all-actual bound. Managed
allocations are raw GC bytes/op rounded up, including harness overhead. Native
allocation totals are not measured.

A separate six-second baseline profile attributes 67.6% to generic expansion
and repeated stride/pixel access, plus 3.2% to channel dispatch. One production
primitive expands dense opaque gray8 rows directly. Bounded AVX2 eight-byte loads
replicate samples into RGB and insert opaque alpha; tails and other hardware use
scalar expansion. Dense Adam7 passes also qualify. Transparency and sparse passes,
source pixels, arithmetic, work/memory admission and cancellation stay unchanged.
No workspace or source cache is introduced.

The updated six-second profile puts expansion at 3.6%, native BCL inflation at
46.3%, literal framing at 27.0% and framing validation at 11.2%. Preserve all
framing, declared-window, output-count and error-precedence checks when targeting
that remaining cost. No further expansion micro-tuning is justified.

Each OS passes 2,462 Release tests: 712 internal, 854 public and 896 live FFmpeg.
Thirteen new row controls and eight authored PNG tests also pass with SIMD
disabled; the latter cover the existing 72 original profiles. The same actual
Release package passes all three usage examples on both OSes. All 575 production
methods and flags match fresh/timed assemblies to that package. Managed and native
normal/scalar samples, precision, alpha, source integrity and independent owners
are exact. Existing RGBA8/16 native-worker controls also pass.

The [numeric evidence](warm-pnggray8-row-evidence.json) retains actuals, brackets,
all four child identities, source/input/derived-raster hashes, profiles, QA and
actual-package archives. No reference source, binary or corpus enters tracked files.

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Png -WarmCases DecodeGray8 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases DecodeGray8 -Cpu 0 -NoBuild -PrepareWarm
```

Build the development-only native worker with `tools/WarmReference/build-png-linux.sh`
first. See the [material coverage audit](WARM-COVERAGE.md) for the remaining scope.
