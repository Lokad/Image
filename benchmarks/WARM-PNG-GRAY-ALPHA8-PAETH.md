# Grayscale-alpha8 Paeth decoding

Complete grayscale-alpha8 PNG decoding independently confirms **1.017× FFmpeg /
upper 1.068×**, at **2,408 managed B/op**. Every pixel, including alpha and hidden
RGB, is exact. Production is unchanged; no speedup is claimed.

| Same immutable build | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 1,964.22 | 2,004.57 | 0.980 / 1.788 | 2,408 |
| Independent twelve-actual confirmation | 2,051.11 | 2,016.66 | 1.017 / 1.068 | 2,408 |

The existing original 513×259 byte raster supplies red-channel grayscale and
unchanged alpha, including hidden RGB. Original authoring follows
[PNG](https://www.w3.org/TR/png-3/): color4/depth8, noninterlaced, every row Paeth,
two-byte predictor dependency, BCL Optimal6 and one IDAT. Header, CRC and every
predictor/sample byte are checked. No original image or settings grid is added.

Both subjects decode the identical packet, including framing/inflation,
prediction, gray-alpha expansion and independent owned output acquisition/release.
Native codec/input packet/frame wrappers are reused; complete YA8-to-owned-RGBA
conversion repeats inside each operation on one conversion thread. No decoded
output is cached. Startup, I/O, IPC, authoring and validation are excluded.
Native allocations are **unmeasured**; managed figures include harness overhead.

Analytical and normal/scalar native RGBA, alpha/hidden RGB, geometry, source
preservation and two simultaneously live independent owners pass. Sequential
Release runs use CPU0, one codec/conversion thread, normal SIMD, disabled tiering
and workstation nonconcurrent GC. All twelve confirmation actuals are selected
and retained. Both versioned quiet guards pass on each run. The upper includes
selected/all-actual intervals against the faster native bracket's lower bound.
Both runs reuse one immutable core, harness and native worker.

The initial untimed predictor validator read source green instead of alpha for
previous samples. It refused preparation before any measurement. The retained
refusal and corrected validator preserve exact checks; two harness builds were
necessary. Only two benchmark authoring inputs change. Cached generated jobs and
verified child DLLs reuse the existing production/native binaries and
**3,457 Release tests per OS**, including **898 live comparisons**, plus the same
actual Release package's consumer and all three examples on Windows/Linux from
the [short-palette qualification](WARM-PNG-SHORT-PALETTE.md).
No production/native rebuild, full suite, package proof, method audit or profile
is repeated.

[Numeric evidence](warm-png-gray-alpha8-paeth-evidence.json) retains all actuals,
allocations, guards, fixture/source/assembly/job/native identities and reused
qualification. Stop this confirmed pair; full required warm coverage remains open.
