# Warm JPEG encoding from RGBA16

An eight-pixel RGBA16 component-row kernel reduces the screened mean **38.4%**,
from **3.107× to 1.906× FFmpeg**. The independent quiet repeat confirms **1.913×**,
conservative upper **1.922×**, averaging 7.373 ms with **1,051,739 managed bytes
allocated per operation**. All 12 actual results are retained; both quietness
guards pass. Stop tuning this fixed case; uniform warm parity remains open.

| Run | Lokad µs | Ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Scalar-path three-actual baseline | 11,906.69 | 3.107 | 3.305 | Pass |
| Component-row three-actual screen | 7,336.26 | 1.906 | 2.092 | Pass |
| Independent twelve-actual confirmation | 7,372.57 | 1.913 | 1.922 | Pass |

Both subjects encode the existing original 513×259 RGBA16 raster to opaque
4:4:4 JPEG. Full 16-bit alpha blending onto RGB 0x204060 and normalization to
8-bit use one rounding after blending, inside every timed operation. The
original native adapter verifies every normalized sample against the exact
integer reference before Ready and after measurement; a corrupted reference
fails explicitly. Conversion to full-range BT.601, complete encoding and owned
output acquisition/release are included. No original image was added.

Native qscale 5 and managed quality 69 match at **0.090625 dB**, within the
unchanged 0.25 dB RGB source-error band. Only this one owned setting is evaluated.
Lokad.Image emits **323,990 bytes**, versus FFmpeg's **133,043 bytes**; managed
fixed Huffman and native optimal Huffman coding remain explicit. This establishes
the declared source-quality timing comparison, not compression efficiency parity.
Encoded bytes, geometry, sampling, opaque alpha, decoder agreement, source and
held owners remain unchanged. Existing 8-bit native 4:4:4/4:2:2/4:2:0 bytes are
also unchanged.

One six-second baseline profile attributes 47.2% of samples to Encode, 14.2% to
ReadRgb and 4.7% to stride checks. The new safe AVX-512F/DQ row path acquires eight
complete pixels together. Integer products/sums below 2^40 are exact in binary64;
division error is smaller than the distance to the nearest integer quotient,
so flooring reproduces exact integer normalization. The existing color matrix
and rounding remain unchanged. Scalar fallback and edges retain their paths;
no extra image workspace, pixel cache, work charge or cancellation change is added.
Only the existing Encode routing changes, with two new row methods; the other
594 existing production bodies and every existing implementation flag match.

Managed timing includes fresh MemoryStream growth, encoding and disposal. Native
contexts, scratch frames and wrappers are reused while all pixel processing and
encoded output are recomputed. Startup, I/O, IPC, fixture preparation and checks
are excluded. Native allocations are unmeasured. Sequential Release runs use
one pinned CPU, one codec/conversion thread, normal SIMD, tiering off and
workstation nonconcurrent GC. Each three-actual mean was parsed before proceeding.

Both OSes pass **2,695 Release tests** (921 internal, 878 public, 896 live
differential), **20 focused row checks** with intrinsics enabled and disabled,
and all three usage examples against the **same actual Release package**. The
six new tests cover every 16-bit alpha, independent channels, guards, opaque
extremes, invalid geometry and the existing transparency error. Fresh builds
and actual generated children match all 597 package method bodies and flags.

The [numeric evidence](warm-jpeg16-component-row-evidence.json) retains the
baseline, profile, screen, confirmation, allocations and exact identities.
All four actual child files, source, raw evidence and package were frozen before
subsequent preparation or mutation. The [benchmark summary](../docs/benchmark.md)
reports this fixed pair at its measured source; subsampled RGBA16 and other
material workload coverage remain open.
