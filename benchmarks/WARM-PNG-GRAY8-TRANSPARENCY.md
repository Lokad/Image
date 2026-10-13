# Warm PNG gray8 transparency

One safe gray8 tRNS row kernel reduces the screened mean **79.4%**, from
**3.593× to 0.729× FFmpeg**. The independent quiet repeat confirms **0.739×**,
conservative upper **0.761×**, averaging 563.19 µs with **9,936 managed bytes
allocated per operation**, unchanged from baseline. All 12 actual results are
retained and both quietness guards pass. Stop tuning this fixed pair; uniform
required warm parity remains open.

| Run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Generic mapping three-actual baseline | 2,740.07 | 3.593 | 6.674 | Pass |
| Row-kernel three-actual screen | 564.39 | 0.729 | 1.242 | Pass |
| Independent twelve-actual confirmation | 563.19 | 0.739 | 0.761 | Pass |

Both subjects decode the same authored 513×259 gray8 PNG, filter None and
noninterlaced. It projects the existing original raster's red channel to equal
RGB and sets a two-byte tRNS key of 127. Its 476 matching pixels have zero alpha
and retain hidden RGB 127; the other 132,391 pixels are opaque. The original
RGB samples remain unchanged. Exact chunk lengths/key, filtered rows, managed
samples, normal/scalar FFmpeg pixels, source and held owners pass; no original
image or profile matrix is added.

The first native preparation fails before Ready because the development adapter
expects GRAY8 while FFmpeg exposes YA8 for gray transparency. It remains
untimed. The original adapter now admits explicit gray8/16 tRNS YA formats and
declares transparency expansion inside each operation. Its existing opaque-gray
output remains byte-identical. The scored baseline and candidate use this same
native worker and exact input.

One six-second baseline profile attributes 39.2% to WritePixel, 24.4% to stride
checks and 8.5% to pixel access. The new bounded AVX2 groups widen eight gray
samples, compare the full 16-bit transparency key and preserve RGB while masking
only alpha. Keys above 255 never match. Scalar tails and other hardware preserve
the same mapping. Only pass routing changes, with one new row method; the other
598 existing bodies and every existing implementation flag match. Work,
admission and cancellation cadence are unchanged; no image workspace is added.

Managed timing includes public Decode, independent output acquisition and
disposal; released pixel storage may be reused. Native timing includes decode,
YA8-to-owned-RGBA conversion and frame release; source packet, contexts and
wrappers are reused. Startup, I/O, IPC, preparation and checks are excluded.
Native allocations are unmeasured. Sequential Release runs use one pinned CPU,
one codec/conversion thread, normal SIMD, tiering off and workstation nonconcurrent
GC. Each three-actual mean was parsed before proceeding.

Both OSes pass **2,729 Release tests** (955 internal, 878 public, 896 live
differential), **27 focused gray8 row checks** with intrinsics enabled/disabled,
and all three examples against the **same actual Release package**. Fourteen
new tests cover every representable key, full-key comparisons, hidden RGB,
unaligned guards, tails and cancellation. Fresh builds and actual generated
children match all 600 package method bodies and flags.

The [numeric evidence](warm-pnggray8trns-row-evidence.json) retains the untimed
rejection, baseline, profile, candidate, confirmation, allocations and exact
identities. All four actual child files, source, raw evidence and actual package
were frozen before subsequent preparation or mutation. The
[benchmark summary](../docs/benchmark.md) reports this pair at its measured
source; gray16/RGB transparency and other material coverage remain open.
