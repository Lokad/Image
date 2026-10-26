# Exact paired JPEG entropy append

The existing 513×259 resident caller-RGB420 control retains Q85/native qscale5,
frozen samples/settings and complete conversion, encoding, fresh independent
output acquisition/release on one CPU. No RGBA staging or extra controls are added.

| Quiet run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| short | 2,428.05 | 1,577.97 | 1.539 / 1.833 | 373,413 |
| confirm | 2,432.58 | 1,576.20 | 1.543 / 1.556 | 373,413 |

The independent run uses 6.7% less elapsed than the preceding quiet
[short screen](WARM-CURRENT-ENCODER-ANCHORS.md); this is not a paired independent
baseline gain claim. Both idle-v2 guards pass. All 12 actual confirmation samples
are retained, including the filtered outlier; conservative bounds cover both
native brackets and filtered/unfiltered managed samples. Complete packets remain
52,091/52,093 B, exact except approved COM attribution, with identical reconstructed
pixels and unchanged 373,413 B/op. Warm parity is still open.

The [profile](WARM-JPEG-CALLER420-PROFILE.md) identified repeated entropy appends.
Each record now appends its Huffman code and amplitude together. A 64-bit
accumulator safely holds seven pending plus up to 32 new bits; no-amplitude
records use one ordinary write. Validation, byte stuffing, padding, restarts and
first 256/subsequent 512-byte flushes remain. Cooperative cancellation checks
each bounded pair and buffer admission/flush. The same admitted lazy buffer,
terminal refusal, explicit cleanup and borrowed caller ownership remain.

Independent bit packing covers every width pair and pending tail, zero/full-bit
patterns, stuffing, flush boundaries and restarts. Existing frozen 946,249-record
and native complete-packet checks remain exact. Both OS pass 4,919 Release checks
each, including 947 live comparisons, plus 317 internal/291 public scalar JPEG
checks. The SAME actual private 0.2.0 package and all three examples pass both OS.
Published 0.1.0 stays unchanged.

[Evidence](warm-jpeg-paired-entropy-evidence.json) records raw samples, producer
and actual-package identities, qualification and reuse. One immutable build serves
the round; native/benchmark/generated workers are reused. Shared JPEG encoding
cost evidence becomes historical except the reconciled caller-RGB420, JPEG444
forward-only and two grayscale boundaries described here.
JPEG decoding and other codecs/editing/text are unchanged. Stop append tuning;
full 253-path parity and replacement readiness remain open.


Three existing boundaries are subsequently reconciled on the same qualified
immutable build, without rebuilding or repeating full/package qualification:

| Current quiet short | Lokad µs | Reference µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 JPEG444, forward-only / FFmpeg | 4,599.19 | 3,783.31 | 1.216 / 1.414 | 820,123 |
| RGBA8 to gray / SIMD libjpeg-turbo | 1,585.89 | 1,405.11 | 1.129 / 1.256 | 155,436 |
| Caller GRAY8 / SIMD libjpeg-turbo | 1,162.42 | 1,077.77 | 1.079 / 1.416 | 155,438 |

These are current screens, without independent confirmations, parity passes or
cross-producer speedup claims. Both guards and exact packets pass. JPEG444 retains
133,039/133,041 B except approved COM. Grayscale retains the raw-exact 58,910-byte
packet, Q69/owned quantizers and explicitly approved SIMD libjpeg-turbo reference;
RGBA normalization flattens on0x204060, while caller GRAY8 borrows resident samples
without RGBA staging on either subject. Complete input delivery/normalization as
applicable, encoding, fresh output acquisition/release and one CPU remain timed.
Existing inputs/settings/native workers and quality/ownership gates are reused.
