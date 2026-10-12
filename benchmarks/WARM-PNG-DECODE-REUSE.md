# PNG complete-overwrite storage

PNG decode now uses the existing bounded pixel-storage policy for operations that
write every output sample. One released exact-size array, at most 4 MiB, can be
leased again. Every color/depth/Adam7 path reconstructs the complete raster before
publishing its owner; failures dispose the private result. Live outputs never
share storage. Pixel admission remains exact and unchanged, including tight-limit
fallback behavior. New ImageBuffer.Create results remain zero initialized.
No framing, inflation, filter arithmetic or failure ordering changed.

Two existing resident PNG inputs retain the same complete public Decode/disposal
and native independently owned RGBA acquisition/release boundaries. FFmpeg
conversion/codec threads remain one, normal SIMD, CPU0; native wrappers/contexts
are reused. Both rasters are projections of the same original RGBA8 image, with
mixed alpha and hidden RGB. No new original performance image is added.

| Case / observation | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Palette8 prior | 992.81 | 365.69 | 2.715 / 2.954 | 621,315 |
| Palette8 storage screen | 886.34 | 364.35 | 2.433 / 2.634 | 89,564 |
| Palette4 prior | 395.24 | 146.14 | 2.704 / 4.328 | 545,417 |
| Palette4 storage screen | 231.75 | 142.52 | 1.626 / 3.078 | 13,656 |
| Palette4 independent confirmation | 214.75 | 136.64 | 1.572 / 1.617 | 13,656 |

Palette8 screen time falls 10.7% and allocations 85.6%; **2.433x / upper 2.634x
remains open**. No longer repeat was launched. Palette4 screen time falls 41.4%
and allocations 97.5%. Its independent confirmation is **1.572x / upper 1.617x**,
with 12 actuals / 10 filtered results. All actuals remain retained and the upper
uses the larger filtered/all-actual bound. Every unchanged quiet guard passes.
Stop tuning this confirmed palette4 pair. Native allocation totals are unmeasured;
managed figures include harness overhead.

Each OS passes 2,646 Release tests (895 internal, 855 public, 896 live FFmpeg),
plus 32 focused PNG public controls with SIMD enabled and disabled. All 72 frozen
PNG color/depth/interlace profiles reconstruct every sample after deliberately
poisoned reuse, and simultaneous held owners stay independent. A partial decode
failure also leaves the next valid owners fully reconstructed. The same actual
dependency-free Release package and all three usage examples pass on both OSes;
all 582 fresh/timed method bodies and flags match that package.

The preceding BCL-only diagnostic compared split filter/data reads with a complete
258-byte scanline: 86.78 versus 80.55 microseconds, only 7.2% improvement. Every
filtered byte agrees and both quiet guards pass. This uses reused scanline storage
and excludes PNG framing, reconstruction and output ownership; it is neither a
FFmpeg comparison nor an end-to-end parity result. No read-pattern change was
implemented. Source inspection of [.NET ReadByte](https://github.com/dotnet/runtime/blob/v10.0.8/src/libraries/System.IO.Compression/src/System/IO/Compression/DeflateZLib/DeflateStream.cs)
and [zlib's fast-loop guard](https://github.com/madler/zlib/blob/v1.3.1/inflate.c#L981)
informed this finite diagnostic. No upstream code is copied into the library.

The [numeric evidence](warm-pngdecode-reuse-evidence.json) retains all actuals,
brackets, input/derived-raster/source hashes, four actual child identities, raw
archives, diagnostic and QA/package identities. Next check the remaining gray4
gap with this shared policy, then prioritize the remaining measured gaps.
Other material profiles, held-owner/fresh Stream boundaries and the full required
uniform warm target remain open. Allocations for retained or first-use owners
remain distinct from the released-storage warm loop reported here.
