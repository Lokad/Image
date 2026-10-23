# Packed grayscale PNG expansion

One noninterlaced opaque gray1 PNG, filter None and zlib level 6, derives from
the existing original 513x259 RGBA8 raster: red >=128 becomes white, otherwise
black; RGB equal gray and alpha is 255. Original development-only code packs
MSB-first samples and zeroes unused low bits. No original image was added.
Encoded depth is 1; both subjects produce independent owned RGBA8 at depth 8.
Native timing includes one-thread MONOBLACK-to-RGBA unpacking/conversion, output
allocation and release; contexts/frame wrappers are reused. Startup, I/O, IPC,
authoring and validation are excluded. Native SIMD/scalar pixels and owners are exact.

| Quiet run | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Generic unpacking screen | 2,521.82 | 302.99 | 8.323 / 8.955 | 537,657 |
| Direct packed-row screen | 294.13 | 302.97 | 0.971 / 1.511 | 537,698 |
| Independent row confirmation | 299.69 | 305.79 | 0.980 / 1.001 | 537,700 |

The screened managed mean falls 88.3%. This fixed gray1 pair **confirms below 2x**
in one independent twelve-actual repeat, retaining every actual (10 filtered results).
All scored runs pass both unchanged quiet guards. An earlier precheck fails at
CPU0 I/O wait 1.002% >1%; it launches no statistical child and is retained unchanged.
One fresh check reuses the same verified prepared binaries; no preparation/full-QA
repeat. The upper uses the larger filtered/all-actual bound. Allocations are raw
GC bytes/op rounded up, including harness overhead; native totals are not measured.
Depths 2/4 are correctness-tested but have no performance confirmation yet.
Other material warm coverage remains open.

A separate six-second profile attributes 49.2% to generic WritePixel expansion,
29.5% to stride access, 9.9% to pixel access and 3.9% to channel dispatch. Both
profile idle guards pass; profiling makes no timing claim. One production primitive
expands dense packed gray1/2/4 rows without transparency. Bounded one/two/four-byte
loads, AVX2 variable shifts, masks and exact integer scales 255/85/17 produce eight
RGBA8 pixels with opaque alpha. Scalar tails ignore unused padding. Dense Adam7
passes qualify; sparse/transparency mapping and work/memory/cancellation contracts
stay unchanged. No workspace or source cache is introduced.

Fifty-four controls cover packed depths 1/2/4, all packed-byte patterns, sample/bit
order, scaling, unaligned guards, tails/padding and cancellation boundaries.
Each OS passes 2,587 Release tests: 837 internal, 854 public and 896 live FFmpeg.
The row controls and eight authored PNG tests covering 72 original profiles also
pass with SIMD disabled. The same actual Release package passes all three usage
examples on both OSes; all 581 production methods and flags match fresh/timed
assemblies to that package. Existing RGBA8/16, RGB8/16, gray8/16, gray-alpha8/16
and palette8 native-worker controls also pass.

Before this round, an independent RFC 1951 structural audit validates all 133,126
filtered bytes of the existing palette8 input against BCL inflation and expected
indices. Its five blocks contain 66,511 literals: 53,964 eight-bit and 12,547
nine-bit codes. Four consecutive eight-bit groups cover 46.5% of literals.
Multiplying that coverage by the current 23.5% literal-framing profile estimates
about 11% decode work under **uniform per-literal cost**, before classification
overhead. This is a rough opportunity estimate, not a timing measurement or a
strict speedup bound. No production framing experiment or benchmark was launched;
the substantially larger packed-expansion gap was prioritized. Palette8 remains open.

The [numeric evidence](warm-pnggray1-row-evidence.json) retains every actual,
bracket, source/input/derived-raster hash, four child identities and raw/QA/profile
archive identity, the failed precheck and the palette structural audit. No upstream
source, binary or corpus enters tracked files. Stop tuning gray1; next establish
one gray4 projection/native boundary for the same implemented kernel and raster.
See the [coverage audit](WARM-COVERAGE.md) for remaining work.

```powershell
./bench.ps1 -Smoke -Warm -WarmFamily Png -WarmCases DecodeGray1 -Cpu 0 -NoBuild
./bench.ps1 -DedicatedVm -Warm -WarmFamily Png -WarmCases DecodeGray1 -Cpu 0 -NoBuild -PrepareWarm
./bench.ps1 -DedicatedVm -Warm -WarmConfirm -WarmFamily Png -WarmCases DecodeGray1 -Cpu 0 -NoBuild -PrepareWarmConfirm
```

Build the development-only native worker with `tools/WarmReference/build-png-linux.sh`
first. The full required warm target remains open.
