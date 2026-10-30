# Exact paired JPEG inverse columns

Current costs are superseded by [SIMD dequantization](WARM-JPEG-DEQUANTIZATION.md).
Original evidence and accepted behavior remain retained.

The existing JPEG420 complete decode takes **2.51% less elapsed** in an independent
comparison, or **2.44% using all actual samples**, with unchanged allocation and
exact pixels. **1.285× / upper 1.304×** remains a gap to warm single-CPU parity.

| Quiet run | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Baseline, independent | 1,331.273 | 1,008.981 | 1.319 / 1.328 | 757,319 |
| Candidate, short | 1,326.242 | 1,008.112 | 1.316 / 1.548 | 757,319 |
| Candidate, independent | 1,297.809 | 1,009.854 | 1.285 / 1.304 | 757,319 |

Both confirmations retain twelve actual/ten filtered samples and separated
filtered and all-actual managed intervals. Native brackets and idle-v2 guards
pass. The short screen was 6.89% below the preceding applicable JCC1 confirmation;
the fresh independent baseline is lower, so that is not the final speedup claim.
No variants or additional confirmations were run.

One frozen 513×259 baseline420 JPEG remains 52,093 bytes. Complete public decode,
normalization, independently owned RGBA8 output and release are included on both
subjects. All 531,468 output bytes and opaque alpha match normal/scalar FFmpeg.
Startup, concurrency, IPC and validation are outside timing. This is one named
control, without a general JPEG speed claim.

Cosine symmetry shares even-frequency sums between mirrored column outputs and
negates odd sums. DC/quarter-turn pairing reduces column vector multiplications
from 64 to 26. Generated Q14 weights, row evaluation, dequantization, signed-word
column bias, modulo-Int32 arithmetic, rounding and saturation remain exact. The
same AVX2 gate and portable fallback apply. Explicit stack scratch remains 256
bytes, with no new owned storage, admission or cancellation boundary.

Both platforms pass 5,056 full checks, including 947 live comparisons, 39 focused
normal/scalar checks and 293 scalar public JPEG checks. A private owned-source
oracle compares 49,920 blocks and 3,194,880 output samples per platform/mode,
including basis spectra, integer/quantizer extremes, DC shortcuts and guards.
The same actual private 0.2.1 package and all three consumer examples pass both
platforms. One production build per platform is reused; harnesses, selectors and
native workers are cached.

[Evidence](warm-jpeg-paired-columns-evidence.json) retains samples, exact identities,
qualification and the mirrored member-verified raw master. JPEG inverse-render
costs are affected; only this control is refreshed. Validation skips this stage,
and unchanged encoder/other-codec/editing/text evidence remains applicable.
The unchanged baseline-AC entropy path is the next substantial lead; paired-column
variants are closed. The full 267-path parity objective remains open.
