# Exact SIMD caller YCCK420 RGB rows

Caller RGB24 takes **80.2% less elapsed time**, 4,532.59→898.47µs, reducing the
short gap from 7.504x to **1.488x / upper 1.550x**. Owned YCCK RGBA improves
**20.3%** to **1.163x**. Both remain parity gaps; no long repeat above one.
The caller-CMYK rejection control is refreshed, without a claimed timing gain.

| Complete resident JPEG operation | Lokad / FFmpeg µs | Ratio / upper | Lokad allocated B/op | JPEG / output bytes |
| --- | ---: | ---: | ---: | ---: |
| YCCK420 → caller RGB24 | 898.47 / 603.97 | 1.488 / 1.550 | 1,337,490 | 6,704 / 398,601 |
| YCCK420 → owned RGBA8 | 824.27 / 708.72 | 1.163 / 1.175 | 1,470,256 | 6,704 / 531,468 |
| Untagged → caller CMYK8 | 526.74 / 330.84 | 1.592 / 1.669 | 1,635,479 | 10,895 / 531,468 |

The preceding profile located93.2% in scalar interpolation, color/black conversion,
rounding and caller dispatch. One primitive now shares exact AVX-512 binary64
color arithmetic between caller RGB and owned RGBA. It retains the original
centered quarter interpolation, every matrix/complement/multiply/divide operation
and final-only rounding. Safe RGB24 packing writes caller bytes directly, without
an RGBA raster. Owned output adds opaque alpha after the same color calculation.

Selection requires full Y/K, half-width/height Cb/Cr, inverted YCCK and normal
orientation. Scalar borders/tails and other layouts/options retain their exact
general path; hardware-off behavior remains safe managed code. Original work
charges precede writes, with unchanged admission, limits, cancellation, source
and surviving output ownership. No precision reduction, FMA/reassociation,
reciprocal division, heap workspace, encoded-input copy or pool-retention change.

Same original packet, native workers, frozen normal/scalar CLI, analytical samples,
compiled selector and harness. Both sides time complete fresh zeroed output,
all four component decodes, required normalization/black/packing and release.
The approved native-only fixed-packet bound remains16 only at authored x/y mod16
=0or15 transitions,4 elsewhere,RGB RMSE≤1.5; owned alpha is exact. Managed RGB
remains exact. Native directRGB24 still exactly matches approved RGBA colors.
ICC conversion stays off and EXIF reorientation opt-in.

Three short screens retain all nine actuals/outliers,50ms native pre/post brackets
and idle-v2 guards. One immutable producer/OS, no new profile, harness, selector,
worker or fixture rebuild. Accepted qualification:4,209tests/OS (1,436internal,
1,845public,928live FFmpeg),96hardware-off checks/OS and the SAME actual private
Windows package consumer/all three examples on both OS. Extended tests cover
both RGB/RGBA vector/scalar samples, full binary64 black arithmetic, clipping,
unaligned spans, padding, partial borders, progressive data and surviving owners.
Released0.1.0 remains immutable.

Exactly three numeric rows change: caller RGB, shared owned YCCK and caller CMYK's
changed selector body. All222other rows and70passing proofs retain evidence.
Scope247=225numeric+22unmeasured,70passes/154mean gaps/one confidence-open.
Largest current gap returns to CMYK2D sparse1.812x; its prefix trial stays rejected.

[Numeric evidence](warm-jpeg-ycck-caller-simd-evidence.json). Raw on both hosts:

- `h10v-measurements.tar.gz` SHA`64c9a747993bf39eb07f4cd46f679f4d8e2c56e4c5a5033a84a79e510f5dfaaa`.

- `h10v-qualified-windows.tar.gz` SHA`34c73d5fcf6693c63773f6534894a702a79c97b483b83d3fa3f5e5b0a035de82`.

- `h10v-qualified-linux.tar.gz` SHA`81a3e867a68e75416aec41fc8814f7645bef10f51f7a230ce491399f7424d79b`.
