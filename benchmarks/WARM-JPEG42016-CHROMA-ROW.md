# Warm RGBA16 JPEG 4:2:0 chroma sampling

One safe RGBA16 chroma-row kernel reduces the screened mean **42.9%**, from
**3.549× to 2.022× FFmpeg**. It averages 3.729 ms and allocates **527,306 managed
bytes per operation**. Both quietness guards pass and all three actual results
are retained. **Parity remains open**: the mean exceeds 2× and its conservative
upper is 2.796×. No twelve-actual confirmation repeats an above-target screen.
Prioritize the next material gap before small further tuning here.

| Run | Lokad µs | Ratio | Conservative upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Scalar-chroma three-actual baseline | 6,530.04 | 3.549 | 3.870 | 527,192 |
| Component-row three-actual screen | 3,729.37 | 2.022 | 2.796 | 527,306 |

Both subjects encode the existing original 513×259 RGBA16 raster to opaque
4:2:0 JPEG. Full 16-bit alpha blending onto RGB 0x204060 and normalization to
8-bit use one rounding after blending, inside every timed operation. The
unchanged native adapter verifies every normalized sample against the exact
integer reference before Ready and after measurement. Conversion to full-range
BT.601, complete encoding and independent owned output acquisition/release are
included. No original image or quality sweep is added: native qscale 5 and
managed quality 69 match at 0.105942 dB within the unchanged 0.25 dB band.

Managed/native packets remain exactly **147,730 / 52,117 bytes**, respectively.
Fixed versus optimal Huffman coding is explicit; this is the declared
source-quality timing comparison, not compression efficiency parity. Geometry,
sampling, opaque alpha, decoder agreement, deterministic bytes, source and held
owners remain unchanged.

One six-second baseline profile attributes 43.4% of samples to Encode, 18.1% to
ReadRgb and 5.9% to stride checks; vectorized luma is only 2.3%. The new
AVX-512F/DQ chroma kernel reuses exact 16-bit normalization for sixteen pixels in
one or two source rows. It preserves the zero-initialized binary64 accumulation
order and averages unrounded components before the single final rounding.
Only existing Encode routing changes, with two new row methods; the other
596 existing bodies and every existing implementation flag match. Scalar
fallback, edge padding, admission, work charges and cancellation remain intact.
No additional image workspace or pixel cache is introduced.

Managed timing includes fresh MemoryStream growth, encoding and disposal. Native
contexts, scratch frames and wrappers are reused while pixel processing and
encoded output are recomputed. Startup, I/O, IPC, preparation and checks are
excluded. Native allocations are unmeasured. Sequential Release runs use one
pinned CPU, one codec/conversion thread, normal SIMD, tiering off and workstation
nonconcurrent GC. Each three-actual mean was parsed before the next action.

Both OSes pass **2,715 Release tests** (941 internal, 878 public, 896 live
differential), **60 focused row checks** with intrinsics enabled and disabled,
and all three usage examples against the **same actual Release package**.
Twenty new tests cover both chroma components and row counts, every 16-bit
alpha, tap order, premature rounding, channel/guard integrity and error cases.
Fresh builds and actual generated children match all 599 package methods/flags.

The [numeric evidence](warm-jpeg42016-chroma-row-evidence.json) retains baseline,
profile, candidate, allocations, validation and exact identities. All four
actual child files, source, raw evidence and package were frozen before further
mutation. The [benchmark summary](../docs/benchmark.md) leaves this case open at
its measured source; uniform required warm parity is not established.
