# Dense RGB16 PNG row expansion

One noninterlaced RGB16 PNG, filter None and zlib level 6, exercises sample
expansion missing from the confirmed RGBA profiles. It derives from the existing
original 513x259 RGBA16 raster: RGB stays unchanged and alpha becomes 65535.
Both subjects decode the identical resident bitstream into independent owned
RGBA16. Native timing includes one-thread FFmpeg RGB48BE-to-RGBA64LE conversion,
output allocation and release; contexts and frame wrappers are reused. Startup,
I/O, IPC, input preparation and validation are excluded.

| Quiet screen | Lokad.Image ms | FFmpeg ms | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Generic sample expansion | 7.057 | 2.229 | 3.167 / 3.506 | 1,807,644 |
| Dense RGB16 row expansion | 4.485 | 2.226 | 2.014 / 2.187 | 1,807,712 |

The screened mean falls 36.5%. This case remains **open**: mean and conservative
upper exceed 2x, and no independent twelve-actual confirmation was run. Both
scored screens pass the unchanged five-second aggregate/CPU0 quiet guards. The
candidate's first precheck failed aggregate I/O wait at 1.8%; it launched no timing
child and was frozen before one bounded fresh check. Failed checks stay failures.
FFmpeg allocation totals are not measured. All three actuals per screen are
retained; the reported upper takes the larger filtered/all-actual bound.

A separate six-second profile attributes 36.9% of baseline samples to generic
pixel expansion and repeated pixel-buffer/stride access. One production primitive
now expands opaque dense RGB16 rows directly. Bounded AVX2 byte shuffles expand
24 input bytes into four RGBA16 pixels, swap sample byte order and insert opaque
alpha. Other hardware and tails use a scalar path. Transparency and sparse
Adam7 retain their existing paths. No workspace, admission, work charges, error
precedence or bounded cancellation contract changes.

Each OS passes 2,421 Release tests: 671 internal, 854 public and 896 live FFmpeg.
Eleven new row controls also pass with SIMD disabled, along with eight authored
PNG tests covering the existing 72 original profiles. The same actual Release
package passes all three usage examples on Windows and Linux. All 572 production
methods and implementation flags match fresh and timed assemblies to that
package. Normal/scalar FFmpeg pixels, full samples, source integrity and live
independent outputs are exact. Existing native RGBA8/16 controls also pass.

The [numeric evidence](warm-pngrgb16-row-evidence.json) records raw actuals,
native brackets, all four timed-child identities, source snapshots, failed
precheck, profile, QA and actual-package identities. No reference source, binary
or corpus is included in tracked files. The [material coverage audit](WARM-COVERAGE.md)
keeps other required paths open.

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Png -WarmCases DecodeRgb16 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases DecodeRgb16 -Cpu 0 -NoBuild -PrepareWarm
```

Build the development-only native worker with `tools/WarmReference/build-png-linux.sh`
first. Statistical replay requires new quiet pre/post checks on the dedicated VM.
