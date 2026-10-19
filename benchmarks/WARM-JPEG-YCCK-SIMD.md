# Exact SIMD YCCK420 color rows

YCCK420 decoding takes **76.1% less elapsed time**, 4,761.24→1,135.97µs.
The short-screen ratio falls from6.733x to **1.604x**, upper1.636x. It remains
a parity gap; no independent passing claim or long repeat. Allocations remain
about1.47MB/op. Four shared-dispatch CMYK controls are refreshed below.

| Complete resident JPEG → owned RGBA8 | Lokad / FFmpeg µs | Ratio / upper | Lokad allocated B/op | JPEG / RGBA bytes |
| --- | ---: | ---: | ---: | ---: |
| YCCK420/full K | 1,135.97 / 708.06 | 1.604 / 1.636 | 1,470,246 | 6,704 / 531,468 |
| CMYK DC | 525.44 / 377.75 | 1.391 / 1.473 | 1,085,667 | 10,910 / 531,468 |
| CMYK horizontal one-AC | 674.71 / 389.55 | 1.732 / 1.783 | 1,085,662 | 17,346 / 531,468 |
| CMYK all63 AC | 3,463.08 / 3,110.84 | 1.113 / 1.197 | 1,085,522 | 347,676 / 531,468 |
| CMYK 2D sparse | 792.76 / 437.47 | 1.812 / 1.869 | 1,085,656 | 22,708 / 531,468 |

A six-second CPU0 profile of the previously accepted producer has5,689samples:
86.6% in scalar interpolation, color/black conversion and rounding. This supports
one substantial row primitive rather than tuning entropy or transforms. Safe
AVX-512 binary64 arithmetic reuses exact integer-quarter chroma interpolation,
keeps every original matrix/complement/multiply/divide operation, rounds only
the final RGB samples and packs opaque RGBA. Scalar borders/tails and unsupported
hardware retain their original pixels. No FMA, reciprocal division, lower precision,
canonical8bitCMYK intermediate, extra heap workspace or full-frame copy.

Selection is limited to Adobe-inverted YCCK H2V2, half-resolution Cb/Cr and full K.
The original component materializer and4,096block/32pixel work charges remain.
Work/cancellation precedes each output row; admission, exception precedence,
source and independent output lifetimes are unchanged. Existing four CMYK gaps
refresh because the shared RenderCmyk body changed; their small timing differences
are not claimed as gains. Direct caller samples and all218other numeric paths
retain their evidence. All70passing cases stay stopped; no new image/settings grid.

Reuse the frozen packets, exact expected arrays, approved harness, selector, CLI
outputs and native workers. Both sides include complete component decode,
color/black normalization, fresh ownedRGBA and release on one CPU/thread. The
[approved native-only bound](WARM-JPEG-YCCK420.md) stays tied to the fixed YCCK420
packet: RGB<=16 at x/y mod16=0or15, <=4 elsewhere, RMSE<=1.5, exact alpha255.
Observed max16/RMSE1.3489149162; exact managed pixels remain required. CMYK gates
stay max4/RMSE1.5. ICC is off and EXIF reorientation opt-in.

All15actuals/outliers, idle-v2 guards and50ms native pre/post brackets are retained.
One producer/OS and one thin diagnostic driver build; zero harness/native/selector
rebuilds. Accepted qualification passes4,202tests/OS (1,433internal,1,841public,
928live FFmpeg),66affected hardware-off checks/OS and the SAME actual private
Windows package consumer/all three examples on both OS. New tests cover fractional
chroma/black, all input-byte values, saturation, guarded outputs/source preservation,
odd/even borders, progressive packets and surviving independent owners. Published
0.1.0 remains immutable.

Exactly five rows change. Finite scope remains246=223numeric+23unmeasured,
70passes/152mean gaps/one confidence-open. CMYK2D sparse1.812x is now the largest
measured gap; its bounded prefix-reuse trial remains rejected. Parity remains open.

[Numeric evidence](warm-jpeg-ycck-simd-evidence.json). Raw on both hosts:

- `h10o-profile.tar.gz` SHA`f6a6708625f7d642f799db460cc8c539b442ad6d5ced092131679dc024f84f2b`.

- `h10o-measurements.tar.gz` SHA`f0cbd97ed3197dd7e22f622d1745762f3d7e40cdfbffd84c1e70a07946a3de16`.

- `h10o-qualified-windows.tar.gz` SHA`63ca4c9a1b8a198bb2d9e485240280109f068da56d4c4be6b952425ded881b11`.

- `h10o-qualified-linux.tar.gz` SHA`eb57f01339baf4ffe781fafaef2ea01118091473f233860a3e49defd78b73c4b`.
