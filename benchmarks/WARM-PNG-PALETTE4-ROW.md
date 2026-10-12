# Packed palette PNG expansion

One original palette4 PNG projects the existing 513x259 RGBA8 raster: index
j=red>>4, i=j*17; palette R=i, G=255-i, B=(73*i+19)%256 and alpha=i. All sixteen
entries have tRNS, including nonzero hidden RGB at zero alpha. Samples are packed
MSB-first with unused low bits zero. This adds no original performance image.

Native FFmpeg unpacks into PAL8 then converts to independently owned RGBA8 on
one thread. Unpacking, conversion, output allocation/copy/release are inside
every operation; contexts/wrappers are reused. Managed timing covers public
Decode/allocation/disposal. Resident input, preparation, validation and IPC are
outside timing. Native normal/scalar pixels, source and held owners are exact.

The production primitive expands dense palette1/2/4 rows directly from packed
indices and existing palette/tRNS spans. Palette4 with all sixteen colors and
absent/full alpha uses eight-byte SSSE3 groups to produce sixteen RGBA pixels.
Partial palettes/alpha, tails and other hardware retain scalar index validation.
Dense Adam7 passes qualify; sparse mapping is unchanged. No workspace, work
admission or cancellation interval changes. No upstream implementation is copied.

| Screen | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Generic packed expansion | 2544.92 | 141.51 | 17.984 / 29.793 | 545,377 |
| Direct packed expansion | 395.24 | 146.14 | 2.704 / 4.328 | 545,417 |

The screened mean falls 84.5%, but **2.704x / upper 4.328x remains open**.
Both unchanged quiet guards pass; all three actuals are retained and the upper
uses the larger filtered/all-actual bound. No twelve-actual confirmation was
launched after parsing the above-2x mean. Allocations include harness overhead;
native allocation totals are unmeasured. Palette1/2 are correctness-tested,
but remain unmeasured; this result does not establish full palette coverage.

A six-second diagnostic profile has 3,657 samples: BCL inflation 38.4%, framing
Validate 19.0%, literal framing 8.1%, canonical decoding 2.6%, and packed palette
expansion 13.5%. Both quiet guards pass and all 582 methods/flags match the actual
package before profiling. Row expansion alone cannot close the remaining gap.
The [token-loop trial](WARM-PNG-TOKEN-RUN-REJECTED.md) on palette8 improved only
4.2% and was reverted. Prioritize substantial inflation/framing data flow.

Each OS passes 2,645 Release tests (895 internal, 854 public, 896 live FFmpeg),
54 new packed-palette controls with SIMD enabled and disabled, and eight scalar
authored PNG tests covering 72 original profiles. The same actual dependency-free
Release package and all three usage examples pass on Windows and Linux. Exact
bit order, arbitrary packed bytes, alpha/hidden RGB, padding, unaligned guards,
partial palettes/alpha, first-invalid-index writes and cancellation are covered.

The [numeric evidence](warm-pngpalette4-row-evidence.json) retains every actual,
bracket, measured source, input/raster hash, four child identities and raw/QA/
profile archives. Passing fixed pairs remain stopped. The required uniform warm
target and other [material coverage](WARM-COVERAGE.md) remain open.
