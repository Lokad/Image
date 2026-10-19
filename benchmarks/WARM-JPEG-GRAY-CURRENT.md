# Current JPEG decode reconciliation

This grayscale result remains current. The subsequent [input reconciliation](WARM-INPUT-RECONCILIATION.md) refreshes the other22 prior rows without rerunning grayscale.

The baseline grayscale complete decode has a current qualified short result of
**1.426×** FFmpeg, conservative upper**1.459×**, with
**672,186 allocated B/op**. This resolves stale evidence after
the compatibility changes; the demonstrated gap remains open. No production
optimization or long repeat follows this clearly slower mean.

| Complete operation | Managed / FFmpeg µs | Ratio / upper | Managed allocated B/op | Verification |
| --- | ---: | ---: | ---: | --- |
| Baseline grayscale JPEG → owned RGBA8 | 973.44 / 682.79 | 1.426 / 1.459 | 672,186 | Qualified short gap |

`WarmJpegDecode/OwnedDecodeGray` reuses the original 513×259, 147,825-byte grayscale
packet (SHAa53c7643717578453750459a7ecc7edb84c4d95366bde811a4f82db242960859).
Both subjects decode every pixel and allocate/release independently owned
531,468-byte RGBA8 output. Native includes gray expansion, state reset and output
ownership; wrappers/private scratch may be reused after release. Startup, I/O,
IPC, preparation/hashing and validation are excluded. Source bytes, managed and
normal/scalar native pixels, reference worker and full boundary match the frozen
record. RGB max4/RMSE1.5 and opaque alpha gates pass.

Three200ms actuals,50ms native pre/post brackets and both idle-v2 guards pass,
one pinned CPU/one codec thread. All actuals/outliers remain in
[numeric evidence](warm-jpeg-gray-current-evidence.json). The already qualified
immutable Linux producer8b876551... and da41b879... benchmark harness are reused,
with the generic JPEG selector from `parity-p55-qualified-evidence.tar.gz`
(bf711b3050c8534c3b89c73f3b5a6cc28c5355bf0812a6364e994827d3930250).
No source synchronization, production/harness/native/selector rebuild or repeated
package proof was needed. Full4,167tests/OS,183affected intrinsics-disabled checks
and same actual private package consumer/all3examples on both OS remain valid.

At this checkpoint, exactly one of the17 prior JPEG rows became current;16JPEG and6additional
Stream rows remain prior production. All other comparisons and29unmeasured
checklist boundaries are unchanged. Full warm parity remains open.
Raw both hosts: `h10a-measurements.tar.gz`
86a83f7bf5782903e85586ec7d0dda2fac21a829fc54659a2c7525c5382b31c5.
