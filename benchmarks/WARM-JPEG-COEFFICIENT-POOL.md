# Bounded JPEG coefficient reuse

Private exact-size coefficient leases cut full-resolution direct-RGB decode time
**10.4%**, native-input4:4:4 **12.6%**, and its live-token case **12.8%**.
Their measured allocations fall **63.7%**, from about2.66MB to966,317B/op.
These are qualified improvements; all16 named cases remain above parity.

Every operation still parses and decodes the complete input, reconstructs every
sample and returns an independent output. The three-slot pool retains at most
**4MiB of idle payload**, separately from per-operation allocations. Leases have
the exact original lengths and are completely zeroed before parsing. Original
working-memory/work admission, entropy and arithmetic order, pixels, cancellation
and failure precedence remain. Storage returns after its last use or failure;
contention falls back without waiting. No input, descriptor or decoded-value cache.
The existing full-resolution packet uses1×2 factors for all three components,
so its coded padding requires three141,440-element arrays,1,697,280payload bytes.

| Existing selector | Lokad / reference µs | Ratio / upper | Managed B/op | Managed time improvement |
| --- | ---: | ---: | ---: | ---: |
| StreamDecode420Restart | 1,583.21 / 993.36 | 1.594 / 1.773 | 951,078 | +2.72% |
| OwnedDecodeGray | 978.38 / 683.31 | 1.432 / 1.479 | 672,186 | -4.34% |
| NativeDecode420 | 1,413.41 / 1,011.48 | 1.397 / 1.476 | 757,343 | +2.92% |
| NativeDecode420Restart | 1,569.26 / 1,015.25 | 1.546 / 1.681 | 761,718 | +4.34% |
| NativeDecode422 | 1,881.70 / 1,350.64 | 1.393 / 1.576 | 829,141 | -8.73% |
| Decode8 | 2,470.91 / 1,899.87 | 1.301 / 1.386 | 948,036 | +5.79% |
| NativeDecode8 | 2,314.44 / 1,721.50 | 1.344 / 1.424 | 966,317 | +12.62% |
| DirectRgb444 | 2,204.97 / 1,455.09 | 1.515 / 1.686 | 966,317 | +10.40% |
| DirectRgb422 | 1,472.32 / 1,095.97 | 1.343 / 1.462 | 829,148 | +1.56% |
| DirectRgb420 | 1,078.10 / 743.66 | 1.450 / 1.471 | 757,346 | +4.69% |
| NativeDecodeCancelable8 | 2,410.34 / 1,716.79 | 1.404 / 1.596 | 966,317 | +12.83% |
| ProgressiveGray | 2,604.90 / 1,714.18 | 1.520 / 1.692 | 683,138 | +2.28% |
| Progressive420 | 2,633.05 / 1,950.32 | 1.350 / 1.411 | 775,330 | +1.43% |
| ProgressiveCancelable444 | 4,967.25 / 3,365.13 | 1.476 / 1.585 | 984,298 | +4.60% |
| Progressive422 | 3,315.96 / 2,247.91 | 1.475 / 1.539 | 847,126 | -0.99% |
| Progressive444 | 5,130.75 / 3,364.53 | 1.525 / 1.590 | 984,298 | +3.42% |

Three actuals at200ms and native50ms brackets retain every actual and both
`exclusive-vm-idle-v2` guards. B/op includes harness costs; native allocations are
unmeasured. There is no routine twelve-actual confirmation while means remain
materially above1. The same original rasters, packets, workers/build pins and
complete operation boundaries are reused. Native RGBmaximum4/RMSE1.5 and exact
alpha, frozen managed pixels, source preservation and independent owners pass.
Stream input includes acquisition/reads/EOF/release; the approved directRGB422
centered correction and RGB420 component interpretation remain timed.

The initial NativeDecode422 screen was8.7% slower than its old managed mean, so
the batch stopped. One bounded baseline/candidate replay reused verified binaries
and exactly restored each source tree without rebuilding:

| Same4:2:2 case, paired short | Lokad µs | Reference µs | Ratio / upper |
| --- | ---: | ---: | ---: |
| Qualified baseline | 1,739.27 | 1,347.32 | 1.291 / 1.365 |
| Candidate replay | 1,728.00 | 1,348.12 | 1.282 / 1.540 |

The replay was essentially flat,0.65% less managed time, and did not reproduce
a material regression. The **initial slower1.393x screen remains selected**;
both replays and every slow actual remain in the evidence. The slower grayscale
and progressive422 screens are likewise retained. This is not a repeat-until-pass
or a uniform speedup claim.

Only the16 existing JPEGdecode timings change. Their old proofs all exceeded1;
the other191 comparisons and all60 passing proofs retain applicable paths.
JPEG inspection, encoding and other codecs/edits do not execute the new pool.
No coverage, settings or input grid was added. One producer build perOS, zero
harness/selector/native builds during18 short jobs, and no new profile.

Qualification passes **3,711 Release tests perOS**:1,377internal,1,436public and
898live FFmpeg, plus3 scalar pool and46 scalar admission/lifecycle checks perOS.
Three new internal checks cover zeroing, live/concurrent leases, exact sizes,
shape changes and the byte ceiling; one public check covers entropy/transform
failure followed by valid dense/sparse decodes and independent outputs. The same
actual Release package's consumer and all three examples pass on Windows/Linux.
The published0.1.0 package/tag are preserved. [Numeric evidence](warm-jpeg-coefficient-pool-evidence.json)
pins sources, allocations, all measurements, conservative uncertainty, raw archives
and the actual package. The full207-case <=1x objective remains open.
