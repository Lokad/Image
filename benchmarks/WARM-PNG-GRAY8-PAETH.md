# Grayscale8 Paeth decoding

Complete grayscale8 PNG decoding with the Paeth filter independently confirms
**1.008× FFmpeg / upper 1.014×**, at **1,384 managed B/op**. Every projected pixel
is exact. Production is unchanged; no speedup is claimed.

| Same immutable build | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 1,037.83 | 1,032.65 | 1.005 / 1.026 | 1,384 |
| Independent twelve-actual confirmation | 1,038.34 | 1,029.60 | 1.008 / 1.014 | 1,384 |

The existing original 513×259 byte raster's red-channel grayscale projection
is reused, with RGB equal to original red and alpha255. Original authoring follows
[PNG](https://www.w3.org/TR/png-3/): color0/depth8, noninterlaced, every row Paeth,
BCL Optimal6 and one IDAT. This exercises the predictor's byte-to-byte dependency.
Every row/predictor byte, chunk CRC and header is checked. No new original image
or setting/shape grid is added; a separate frozen directory preserves earlier inputs.

Both subjects time complete identical-packet decoding, framing/inflation,
prediction, gray expansion and independent owned output acquisition/release.
Native codec/input packet/frame wrappers are reused; full GRAY8-to-owned-RGBA
conversion repeats inside each operation on one conversion thread. No decoded
output is cached. Startup, I/O, IPC, authoring and validation are excluded.
Native allocations are **unmeasured**; managed figures include harness overhead.

Analytical and normal/scalar native RGBA, opaque alpha, geometry, source
preservation and two simultaneously live independent owners pass. Sequential
Release runs use CPU0, one codec/conversion thread, normal SIMD, disabled tiering
and workstation nonconcurrent GC. Both versioned quiet guards pass on each run.
All twelve confirmation actuals are retained; BenchmarkDotNet selects eleven.
The upper includes selected/all-actual intervals against the faster native
bracket's lower bound. Both runs reuse one immutable core, harness and native worker.

Only two benchmark authoring inputs change. One harness build and cached generated
jobs suffice, with verified current DLLs preloaded into each child. Existing
**3,457 Release tests per OS**, including **898 live comparisons**, and the same
actual Release package's consumer plus all three examples on Windows/Linux are
reused from the [short-palette qualification](WARM-PNG-SHORT-PALETTE.md).
No production/native rebuild, full suite, package proof, method audit or profile
is repeated because this fixed case already clears the timing target.

[Numeric evidence](warm-png-gray8-paeth-evidence.json) retains every actual,
allocation, guard, fixture/source/assembly/job/native identity and qualification
reuse. Stop this confirmed pair; full required warm coverage remains open.
