# Fourth CMYK coefficient-buffer reuse

Complete owned CMYK decoding allocates **33.6% less managed memory**, about
549 KB fewer bytes per operation. Short-screen elapsed times are similar; all
three controls remain gaps against FFmpeg. No parity or statistical timing win
is claimed.

| Complete owned decode | Managed / FFmpeg µs | Ratio / upper | Managed allocated B/op | JPEG / RGBA bytes |
| --- | ---: | ---: | ---: | ---: |
| DC blocks | 527.03 / 377.57 | 1.396 / 1.443 | 1,085,667 | 10,910 / 531,468 |
| One AC per block | 746.37 / 388.45 | 1.921 / 2.001 | 1,085,660 | 17,346 / 531,468 |
| All63 AC per block | 3,463.78 / 3,122.44 | 1.109 / 1.124 | 1,085,522 | 347,676 / 531,468 |

The private exact-size coefficient pool starts with three slots. Owned
full-resolution CMYK can grow it once to four under the same nonblocking lock,
before returning the current frame's coefficient planes. Four subsequent leases
reuse all four buffers. The combined retained-byte ceiling remains4MiB; live
planes never enter the pool, every lease is zeroed, and cross-shape eviction and
duplicate-return checks stay unchanged. Work/live-memory admission, cancellation,
failure cleanup and caller/output ownership are unchanged. Fresh ordinary hosts
retain their original three-slot path; mixed/concurrent histories have correctness
coverage, without a mixed-history performance claim.

Reuse the original raster, three authored packets, exact qualified pixels,
normal/scalar native gates, two surviving outputs and complete owned RGBA boundary
from [the spectrum round](WARM-JPEG-CMYK-SPECTRA.md). Native still includes all
four components, black conversion, owned RGBA normalization and release. K is
never alpha; ICC remains off and EXIF reorientation opt-in. Packet sizes and
quality are unchanged. No fixture, harness, native worker or selector rebuild,
new benchmark case, profile or settings grid. One immutable candidate per OS,
three200ms actuals per case, both idle-v2 guards and50ms native pre/post brackets
are retained. Above-one means do not warrant long confirmations.

Accepted qualification:4,196tests/OS (1,431internal/1,837public/928live FFmpeg),
55affected hardware-disabled checks/OS, and the SAME actual Windows private
package consumer/all three examples on both OS. New pool tests prove four
distinct zeroed leases, the original byte cap, eviction and concurrent ownership.
Released0.1.0 package/tag remain immutable.

Exactly three numeric rows change. All70unaffected passing proofs remain valid
and stopped. The finite245-case objective stays open:221numeric rows,
24unmeasured;70passes/150mean gaps/one confidence-open. The largest measured gap
is the1.921x one-AC CMYK control. No general-photo/progressive/mixed-host speed
claim follows from these representative spectra.

[Numeric evidence](warm-jpeg-coefficient-reuse-evidence.json) includes allocations,
all actuals/outliers, exact source/build identities, gates and both-OS qualification.
Raw archives are retained on both hosts:

- `h10k-measurements.tar.gz` SHA`840d66436192fd80a74224a10d8e3c93e76389494a129d16137bd3bd762ebd1d`.

- `h10k-qualified-windows.tar.gz` SHA`49f67da192fb2004d0a080542644e39876b90eec69bf68ecc0018e1a5851e66f`.

- `h10k-qualified-linux.tar.gz` SHA`d664fa80f7d08083b90903ab87c4b280f39905552f76e3276edde2bd5bae4893`.
