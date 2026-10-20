# Lossless WebP coding-cost eligibility

The existing opaque control falls from **170,156 to123,602 B**, a27.36% reduction,
closing96.47% of its previous excess over the frozen121,896 B reference. It remains
1.40% larger. The primary photograph/diagram controls retain174,748/7,636 B against
160,032/7,234 B; their required size gates remain open. All are the existing513×259
inputs with libwebp `exact=1` (FFmpeg's encoder backend), method4/quality75.

All six affected complete operations independently pass single-CPU warm parity.
Bounds retain selected/all actuals and native brackets, with the versioned quiet
VM guard. Times are microseconds; allocations are managed bytes per operation.
Native allocations are unmeasured.

| Existing case | Managed / reference µs | Ratio / upper | Allocated B/op | Managed / reference packet B |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 49,621.60 / 69,789.18 | 0.711 / 0.720 | 9,669,728 | 123,602 / 121,896 |
| DiagramLossless8 | 6,715.35 / 10,254.85 | 0.655 / 0.669 | 107,568 | 7,636 / 7,234 |
| PhotographLossless8 | 65,462.97 / 82,409.48 | 0.794 / 0.802 | 9,912,952 | 174,748 / 160,032 |
| Encode8 | 57,489.75 / 121,784.50 | 0.472 / 0.481 | 10,286,728 | 145,648 / 183,812 |
| Encode16 | 66,156.61 / 122,061.47 | 0.542 / 0.550 | 10,286,320 | 145,648 / 183,812 |
| WebpLossless8AsyncNonSeekable | 58,259.63 / 120,897.37 | 0.482 / 0.487 | 10,286,704 | 145,648 / 183,812 |

The raw weighted body cost is now calculated from the frequencies already gathered.
Prediction remains eligible for literal-heavy inputs and additionally when cached
body cost exceeds8bits/pixel. Low literal count alone does not imply cheap coding.
Existing LEFT prediction, cache, spatial groups and complete-packet cost selection
are unchanged. No predictor, group model, cache-size or quality grid is added.

The newly eligible opaque case spends more work and allocation for smaller output:
9,669,728 B/op, versus historical614,408 B/op. Its old timing does not qualify this
path. The unchanged major-buffer admission covers actual capacity, replacement
overlap and retained ownership; refusal is terminal. Exact hidden RGB/alpha,
16-to-8 normalization, borrowed inputs/root, cancellation, cleanup and recovery pass.
Other controls retain their packets and allocation counts. Historical elapsed
numbers are not a controlled estimate of this change's performance effect.

Both platforms pass4,576 tests each (including932 live FFmpeg), plus24 internal
and117 public affected scalar checks. One core and two test builds per OS are
reused through12 scores/90 actuals/24 passing guards and full qualification; no
harness, selector, native-worker or differential-test rebuild. The same actual
private0.2.0 package and all three usage examples pass on both OSes. Published0.1.0
is unchanged. This follows the [untimed eligibility audit](WARM-VP8L-OPAQUE-COST-AUDIT.md).

Exactly six encoder observations refresh. Other relevant production paths retain
their evidence;31 C43 metadata timings remain open. No workload is added. Remaining
compression gates, full finite warm parity and replacement readiness remain open.

[Numeric evidence](warm-vp8l-code-cost-evidence.json) retains every actual, guard,
allocation, producer and verification result. Mirrored raw archives:

- `c49-qualified-windows.tar.gz`: `8c2d19ccbc6efa5890611e92b5a71cd1b4d67ce013d5835d7f28c90173592948`

- `c49-qualified-linux.tar.gz`: `906757cb334af5e437397614bbf058818ee8b94a5aabda42e2aeeb91f673d7e9`

- `c49-measurements.tar.gz`: `9741ae227ad5974a359ff3c2c2fd029d5ee050d254616ece977f83d7656a28af`

Actual private package SHA256: `8f025d811cbb4457afe8a2ab87213284ad5375b97a3b1b3205e0135e771ded48`.
