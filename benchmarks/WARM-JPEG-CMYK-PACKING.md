# Direct caller CMYK row packing

The complete operation takes **64.1% less elapsed time**,2,817.13→1,010.51µs.
The short gap falls from8.522x to **3.057x**, upper3.169x. It remains the largest
measured gap; no passing claim or long repeat. Allocations remain about1.635MB/op.

| Resident JPEG → fresh caller CMYK8 | Lokad / FFmpeg µs | Ratio / upper | Lokad allocated B/op | JPEG / CMYK bytes |
| --- | ---: | ---: | ---: | ---: |
| Original canonical CMYK projection | 1,010.51 / 330.50 | 3.057 / 3.169 | 1,635,459 | 10,895 / 531,468 |

The existing profile locates66.0% in scalar sample extraction/interpolation/
rounding. For full-resolution CMYK these are already final byte samples:
interpolation is identity, optional inversion is255-sample and rounding is exact.
Pack four rows directly with safe AVX2 unpack/permutation and optional XOR;
scalar tails/unsupported hardware use the same byte mapping. This changes one
primitive, with no matrix reassociation, reduced precision or heap workspace.
Original32work/pixel is charged before each row is written. Materialization,
IDCT, pool caps, admission, cancellation/error precedence and owners are unchanged.
Non-normal orientation, subsampling, YCCK and other output formats retain their
general conversion. All four ink channels, source and padding remain exact.

Reuse the SAME untagged packet, native worker, scalar/normal CLI samples, compiled
selector and frozen harness from the [initial measurement](WARM-JPEG-CALLER-CMYK.md).
Both include fresh zeroed caller acquisition, complete decode, necessary raw
component packing/normalization and release on one CPU/thread. No RGBA intermediary,
ICC conversion, EXIF reorientation, header patch or native-packet change. All
three200ms actuals/outliers,50ms native pre/post brackets and idle-v2 guards remain.
No new fixture, worker/harness/selector build or profile.

One immutable producer/OS passes4,208tests/OS (1,435internal,1,845public,928live
FFmpeg),72affected hardware-off checks/OS and the SAME actual private Windows
package consumer/all three examples on both OS. New tests cover every sample byte,
four-plane order, inversion, unaligned spans/vector tails, baseline/progressive
packets, padding and surviving caller buffers. Released0.1.0 stays immutable.
Exactly one numeric row changes; all223others and70passing confirmations are reused.

This selector exposes one concrete coverage omission: the retained caller YCCK→RGB
matrix/black algorithm in WriteCmykSamples is distinct from both optimized raw
caller CMYK and owned YCCK RGBA. Add exactly `JpegSamplesDecode/Ycck420RgbResident`,
UNMEASURED, using the existing YCCK420 packet and its analytical RGB. Resolve the
equivalent complete native RGB24 boundary first; keep its approved fixed-fixture
native bound. No new image/shape/depth/filter/quality grid. Finite scope is now
247=224numeric+23unmeasured,70passes/153mean gaps/one confidence-open. Parity stays open.

[Numeric evidence](warm-jpeg-cmyk-packing-evidence.json). Raw on both hosts:

- `h10r-measurements.tar.gz` SHA`a59ff9e797fa444f76ef7b36a35660637c07f3f4583490e568282b6ed5ff4ec1`.

- `h10r-qualified-windows.tar.gz` SHA`8e83b43c0d407af192f513e365681b51cf9985cb94d1ba1ef785f4abbd44c8f7`.

- `h10r-qualified-linux.tar.gz` SHA`f4b9af71b1718d152f88141d8386fdaac0fb1528e6655a0546ad7c16b2dca655`.
