# Optional compact color JPEG quantization

Opt-in `JpegQuantizationPolicy.CompactColor` produces **48,701 bytes versus 52,093**
for FFmpeg on the fixed caller-RGB420 control. The source-PSNR difference is
**0.017274 dB**, inside the existing0.25dB gate. This is34.2% smaller than the
unchanged74,040-byte Original packet and6.5% smaller than the reference. These
claims concern this procedural513×259RGB420 input and frozen settings only.

Warm parity remains open. Both three-actual screens exceed a1× point mean, so
there is no longer confirmation or independently passing timing claim.

| Complete sample encode | Managed / reference µs | Ratio / upper | Managed allocated B/op | Encoded bytes, managed / reference |
| --- | ---: | ---: | ---: | ---: |
| RGB8 → JPEG420, CompactColor | 1,774.21 / 1,576.12 | 1.126 / 1.362 | 162,110 | 48,701 / 52,093 |
| Gray8 → one component, Original | 1,251.71 / 1,079.34 | 1.160 / 1.394 | 155,026 | 58,823 / 58,910 |

The [Original RGB420 control](WARM-JPEG-CALLER-MCU.md) retains its separate timing
gap and byte-identical packet; selecting a new option does not close that case.
Grayscale keeps the same packet, source quality and allocation scale, with an
essentially unchanged point mean. RGB uses the same FFmpeg qscale5 reference;
gray uses the approved SIMD-enabled libjpeg-turbo reference at69. Managed quality
stays69. Equal numerical qualities are not the matching rule.

CompactColor adds the integer `(16*scale+50)/100` to each Original quantizer and
clamps it to255, using the existing quality scale. This independently chosen
frequency-independent offset reduces excessive low-frequency precision without
importing a reference matrix or searching image statistics. Original remains the
default. Grayscale always uses Original, and quality100 adds zero. Coefficient
rounding, sampling, once-only flattening, restart markers and all stream boundaries
keep their contracts. There is no new major storage: existing quantizer arrays,
scratch, event capacity, admission owners and work/cancellation boundaries remain.

The actual source encode improves on the earlier
[coefficient feasibility experiment](WARM-JPEG-PRECISION-AUDIT.md). Native source
PSNR is15.239708 for the managed packet versus15.256982dB for the reference.
Managed/native decode agreement is maximum3/RMSE0.593673; alpha is exact. Normal
and scalar native output match, and managed scalar pixels match exactly. Both
platforms produce the same compact and Original control hashes. No source image,
reference input/settings, quality search or workload grid changes.

Two short jobs retain all six actuals, equivalent complete operation boundaries,
native pre/post brackets and all four passing versioned idle guards. Every call
borrows the same packed samples, performs normalization, transform, quantization
and entropy coding, acquires a fresh owned output and releases it. No RGBA staging
or flattening is timed on either side. Startup, validation and disk/IPC are excluded.
The production build is frozen once perOS and reused; there is one harness build,
zero selector/native rebuilds and no timed retries or post-freeze production rebuild.

All **4,534 tests perOS** pass:1,682internal,1,920public and932liveFFmpeg. The twelve
new API checks pass normally and without intrinsics on both OSes. They cover
policy defaults/endpoints/clamping, unchanged grayscale, padded borrowed RGB,
RGBA8/16, fixed/adaptive entropy, restarts, async output, undefined options,
shared refusal/cancellation cleanup, later success and surviving outputs. The
**same actual private0.2.0 package** consumer and all three usage examples pass
on Windows and Linux, including the new option through async/shared-root APIs.
Published0.1.0 stays immutable.

Exactly one new material algorithm case is added; caller-gray evidence refreshes.
The finite checklist is255cases:239numeric+16untimed,47current/192historical,
72independent passes/166mean gaps/1confidence-open,125independent observations/
114qualified shorts. Other codec/decoder/edit/text/inspection paths and unchanged
Original packets retain their identified evidence. Size/quality acceptance for
this optional RGB420 control does not establish photographic/diagram efficiency,
full warm parity or replacement readiness.

[Numeric evidence](warm-jpeg-compact-color-evidence.json) includes actuals,
allocations, quality, boundaries, guards, identities and mirrored raw archive
hashes for `c37-measurements.tar.gz` and both `c37-qualified-*.tar.gz` archives.
Private package SHA256:
`f651a4393dcc27bb12b6d11f0e9546795c84463f1aafeafc0e3b35bbd839afc5`.
