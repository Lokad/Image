# Packed gray4 coverage

One opaque gray4 PNG, noninterlaced/filter None/zlib level 6, derives from the
existing original 513x259 RGBA8 raster: index=red>>4, RGB=index*17, alpha=255.
Original development-only authoring packs samples MSB-first and clears unused
low bits. No original image was added. Coded precision is 4; output is RGBA8.

Both subjects decode identical resident PNG bytes into independent owned output.
Native timing includes decoder unpacking to GRAY8, one-thread GRAY8-to-RGBA
conversion, allocation and release; contexts/frame wrappers are reused. Startup,
I/O, IPC, authoring and validation are excluded. Native SIMD/scalar pixels,
precision, source, tail/padding and held owners are exact.

| Quiet run | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual coverage screen | 396.94 | 150.87 | 2.631 / 3.134 | 545,305 |
| Retained twelve-actual repeat | 420.42 | 159.85 | 2.630 / 2.775 | 545,302 |

This fixed pair remains **above 2x / open**. Both runs pass both unchanged idle
guards; all actuals are retained (12 actual/12 filtered in the repeat). The repeat
was already launched before the above-2x screen ratio was interpreted. It is
retained, with no further repeat. Upper takes the larger filtered/all-actual bound.
Allocation is raw GC bytes/op rounded up, including harness overhead; native
totals are not measured. Other material required warm coverage remains open.

Production is unchanged from the [packed-row implementation](WARM-PNG-GRAY1-ROW.md).
All 581 fresh/timed method bodies and flags match the same accepted actual package.
Its original 2,587 tests per OS, scalar controls and all three package examples
remain the production validation; no full suite/pack was repeated for this harness
and coverage change. Development native-worker controls pass for RGBA8/16,
RGB8/16, gray8/16, gray-alpha8/16, palette8 and gray1.

One six-second diagnostic profile attributes 40.1% to native BCL inflation,
18.2% to framing validation, 8.6% to ReadLiteralRun and 14.2% to packed unpacking.
Both profile idle guards pass; profiling makes no timing claim. Optimizing the
unpacking share alone cannot close this measured gap. Prioritize substantial
framing/data-flow opportunities against the larger palette8 gap, preserving
work/memory admission, precision, checks and error precedence.

The [numeric evidence](warm-pnggray4-evidence.json) retains all actuals/brackets,
source/input/derived-raster hashes, four child identities, raw/profile/control
archives and exact package proof. No upstream source, binary or corpus enters
tracked files. Gray1 remains confirmed; gray2 and packed palette remain unmeasured.
See the [coverage audit](WARM-COVERAGE.md) for remaining work.

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Png -WarmCases DecodeGray4 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases DecodeGray4 -Cpu 0 -NoBuild -PrepareWarm
```

Build the development-only native worker with `tools/WarmReference/build-png-linux.sh`
first. The full uniform warm target remains open.
