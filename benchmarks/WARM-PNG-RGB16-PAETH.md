# RGB16 Paeth decoding

The current RGB16 Paeth timing is superseded by [six-byte SIMD reconstruction](WARM-PAETH-SIX-CHANNEL.md), which independently confirms parity. The retained measurements below are historical.

Complete RGB16 PNG decoding independently confirms **1.904× FFmpeg /
upper1.928×**, at **6,504 managed B/op**. Every 16-bit pixel is exact.
Production is unchanged; no speedup is claimed.

| Same immutable build | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Three-actual screen | 8,599.57 | 4,462.93 | 1.927 / 2.689 | 6,504 |
| Independent twelve-actual confirmation | 8,510.46 | 4,469.96 | 1.904 / 1.928 | 6,504 |

The existing original 513×259 word raster supplies unchanged RGB and alpha65535.
Original authoring follows [PNG](https://www.w3.org/TR/png-3/):
color2/depth16, big-endian RGB samples, noninterlaced, every row Paeth,
six-byte predictor dependency, BCL Optimal6 and one IDAT. Every header, CRC,
predictor and sample byte is checked. Existing authoring already supports this
layout; only one benchmark selector and its depth-dependent frozen path change.
No original image or settings grid is added.

Both subjects decode the identical packet, including framing/inflation,
prediction, RGB expansion and independent owned output acquisition/release.
Native wrappers are reused; complete RGB48BE-to-owned-RGBA64LE conversion and
the second output allocation repeat inside each operation on one conversion
thread. No decoded output is cached. Startup, I/O, IPC, authoring and validation
are excluded. Native allocations are **unmeasured**; managed figures include
harness overhead.

Exact analytical and normal/scalar native RGB/opaque alpha, geometry, source
preservation and two simultaneously live independent owners pass. Sequential
Release runs use CPU0, one codec/conversion thread, normal SIMD, disabled tiering
and workstation nonconcurrent GC. Both versioned quiet guards pass on each run.
All twelve confirmation actuals are selected and retained. The upper includes
selected/all-actual intervals against the faster native bracket's lower bound.
Both runs reuse one immutable core, harness and native worker.

One harness build and cached generated jobs suffice, with verified DLLs preloaded
into each child. Production/native binaries and **3,457 Release tests per OS**,
including **898 live comparisons**, plus the same actual Release package's
consumer and all three examples on Windows/Linux are reused from the
[short-palette qualification](WARM-PNG-SHORT-PALETTE.md).
No production/native rebuild, full suite, package proof, method audit or profile
is repeated because the independent comparison clears the target.

[Numeric evidence](warm-png-rgb16-paeth-evidence.json) retains all actuals,
allocations, guards, fixture/source/assembly/job/native identities and reused
qualification. Stop this confirmed pair; full required warm coverage remains open.
