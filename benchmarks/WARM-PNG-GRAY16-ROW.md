# Dense gray16 PNG expansion

One noninterlaced gray16 PNG, filter None and zlib level 6, derives from the
existing original 513x259 RGBA16 raster: red becomes gray, green/blue equal red,
and alpha becomes 65535. No original image was added. Both subjects decode the
identical resident bitstream into independent owned RGBA16. Native timing includes
one-thread GRAY16BE-to-RGBA64LE conversion, output allocation and release; contexts/
frame wrappers are reused. Full samples are exact, including low bytes. Startup,
I/O, IPC, preparation and validation are excluded.

| Quiet run | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Generic expansion screen | 4,025.80 | 958.76 | 4.199 / 4.501 | 1,330,995 |
| Direct row screen | 1,740.13 | 958.52 | 1.815 / 1.943 | 1,331,046 |
| Independent quiet row confirmation | 1,743.26 | 960.42 | 1.815 / 1.837 | 1,331,046 |

The screened mean falls 56.8%. This fixed pair **confirms below 2x** with all
12 actuals retained (10 filtered results) and both unchanged quiet guards passing.
Both scored screens also pass both guards. The first twelve-actual confirmation's
post-run guard fails at aggregate I/O wait 1.35% and CPU0 I/O wait 2.8%, above the
unchanged 1% limit. It is retained unchanged and unscored. One fresh attempt reuses
the same verified prepared binaries and passes; no repeat preparation or full QA
was performed. The upper uses the larger filtered/all-actual bound. Allocations
are raw GC bytes/op rounded up, including harness overhead; native allocation
totals are not measured. Other material warm scope remains open.

A separate six-second profile attributes 31.6% to generic WritePixel expansion,
19.0% to stride access, 6.8% to pixel access and 3.0% to channel dispatch. Both
profile idle guards pass; profiling makes no timing claim. One production
primitive expands dense gray16 rows directly. Bounded eight-byte loads, AVX2
broadcast and byte shuffles produce four native-endian RGBA16 pixels, replicating
the complete gray sample and inserting alpha 65535. Scalar tails/other hardware,
transparency and sparse passes retain their mapping; dense Adam7 passes qualify.
Source samples, arithmetic, work/memory admission and cancellation stay unchanged.
No workspace is introduced.

Fourteen new controls cover all 65,536 gray values, byte order/low-byte precision,
unaligned source/destination guards, vector tails, cancellation-boundary widths
and cancellation before writes. Each OS passes 2,503 Release tests: 753 internal,
854 public and 896 live FFmpeg. The row controls and eight authored PNG tests
covering 72 original profiles also pass with SIMD disabled. The same actual
Release package passes all three usage examples on both OSes; all 578 production
methods and flags match fresh/timed assemblies to that package. Managed and native
normal/scalar pixels, source integrity and independent owners are exact. Existing
RGBA8/16, RGB8/16, gray8 and gray-alpha8 native-worker controls also pass.

The [numeric evidence](warm-pnggray16-row-evidence.json) retains all actuals,
brackets, source/input/derived-raster hashes, four actual child identities and
raw/QA/profile archive identities, including the failed confirmation. No upstream
source, binary or corpus enters tracked files. Stop tuning this fixed pair;
next screen one gray-alpha16 projection from the existing RGBA16 raster.
See the [coverage audit](WARM-COVERAGE.md) for remaining work.

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Png -WarmCases DecodeGray16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases DecodeGray16 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmConfirm -WarmFamily Png -WarmCases DecodeGray16 -Cpu 0 -NoBuild -PrepareWarmConfirm
```

Build the development-only native worker with `tools/WarmReference/build-png-linux.sh`
first. The full required warm target remains open.
