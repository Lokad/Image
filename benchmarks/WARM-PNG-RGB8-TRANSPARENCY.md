# Warm PNG RGB8 transparency

One safe RGB8 tRNS row kernel reduces the screened mean **59.9%**, from
**3.524× to 1.408× FFmpeg**. The independent quiet repeat confirms **1.417×**,
conservative upper **1.439×**, averaging 1,779.35 µs with unchanged **11,984
managed bytes allocated per operation**. All 12 actual results are retained
and both quietness guards pass. Stop tuning this fixed pair; uniform required
warm parity remains open.

| Run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Generic mapping three-actual baseline | 4,435.67 | 3.524 | 6.054 | Pass |
| Row-kernel three-actual screen | 1,779.19 | 1.408 | 1.957 | Pass |
| Independent twelve-actual confirmation | 1,779.35 | 1.417 | 1.439 | Pass |

Both subjects decode the same authored 513×259 RGB8 PNG, filter None and
noninterlaced, preserving the existing original raster's RGB samples. The exact
six-byte tRNS key (48,139,231) makes 129 pixels transparent without changing
their hidden RGB; the other 132,738 pixels have alpha 255. Exact chunks/key,
filtered rows, managed samples, normal/scalar FFmpeg pixels, unchanged source
and independent held owners pass before and after measurement. Only the two
existing original performance rasters are used; no profile sweep is added.

The development adapter explicitly accepts FFmpeg's RGBA output for RGB tRNS.
The decoder expands alpha inside each operation and already owns that output;
no redundant swscale conversion or second pixel allocation is performed. The
same updated native worker is used for baseline and candidate. Previous opaque
RGB8 and gray8/16 transparency inputs and outputs remain byte-identical.

One six-second baseline profile attributes 31.6% of samples to WritePixel,
17.1% to stride checks and 5.5% to pixel access. The new bounded AVX2 groups
shuffle eight RGB triplets into RGBA and compare all three complete 16-bit
keys, masking only alpha. A key channel above 255 never matches. Scalar tails
and other hardware preserve the same mapping. Only PNG pass routing changes,
with one new row method; the other 599 existing bodies and every existing
implementation flag match. Admission, work and cancellation cadence are
unchanged, with no additional image workspace.

Managed timing includes public Decode, independent output acquisition and
disposal; released pixel storage may be reused. Native timing includes complete
decode and independently owned frame acquisition/release; resident input,
contexts and wrappers are reused. Startup, I/O, IPC, preparation and checks are
excluded. Native allocations are unmeasured. Sequential Release runs use one
pinned CPU, one codec thread, normal SIMD, tiering off and workstation
nonconcurrent GC. Each three-actual mean was parsed before proceeding.

Both OSes pass **2,749 Release tests** (975 internal, 878 public, 896 live
differential), **33 focused RGB8 row checks** with intrinsics enabled/disabled,
and all three examples through the **same actual Release package**. Twenty new
tests cover every representable key value in each channel, full-key comparisons,
hidden RGB, unaligned guards, tails and cancellation. Fresh builds and actual
generated children match all 601 package method bodies and flags.

The [numeric evidence](warm-pngrgb8trns-row-evidence.json) retains baseline,
profile, candidate, confirmation, allocations, prior native controls and exact
identities. All four actual child files, source, raw evidence and actual package
were frozen before subsequent preparation or mutation. The
[benchmark summary](../docs/benchmark.md) reports this pair at its measured
source; JPEG420 gaps and other required material coverage remain open.
