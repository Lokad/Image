# Lossless WebP uncached literal models

The fixed opaque photograph falls from **169,808 to 164,008 bytes**, saving
**5,800 bytes (3.42%)** with exact RGBA. It remains **3,976 bytes above** its
160,032-byte reference. Diagram 7,636/7,234 and procedural opaque 123,602/121,896
bytes are unchanged; these three complete-packet gates remain open. Alpha8/16
and async retain passing 145,648/183,812-byte packets. These controls establish
no broader compression claim.

One uncached alternative reuses the selected predictor/color transforms and copy
parse. It refits the complete literal distribution using the format's 280-symbol
green alphabet and the existing spatial grouping pass, then compares complete
packets. Count and write paths share the selected cache mode. The C96 byte-only
prototype and actual managed photograph packets match exactly. The diagram keeps
its smaller predecessor; no reference, settings or quality search is involved.

Both alternatives borrow the same admitted predictor and color owners. Losing
coding tables/groups release promptly without releasing those shared maps.
Substantial replacement capacities and overlap use the caller's shared root;
refusal, cancellation and work exhaustion remain terminal. No additional frame
copy or per-pixel decision map is introduced.

The six existing complete operations preserve original 513×259 inputs/settings:
effort4, **libwebp exact=1, FFmpeg's encoder backend**, quality75/method4, unchanged
reference packet hashes, normalization, hidden RGB and independent output ownership.
Normal/scalar managed and FFmpeg pixels are exact. These are **three-actual quiet-VM
cost screens**, with selected/all-actual bounds retained, not independent CPU-parity
passes. Native allocations are unmeasured.

| Existing operation | Packet B managed/reference | Managed/reference µs | Ratio/upper | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| EncodeOpaque8 | 123,602/121,896 | 221,275.53/70,433.20 | 3.142/4.256 | 36,142,008 |
| DiagramLossless8 | 7,636/7,234 | 8,027.39/9,858.36 | 0.814/1.376 | 107,568 |
| PhotographLossless8 | 164,008/160,032 | 329,395.62/83,543.00 | 3.943/5.057 | 36,327,472 |
| Encode8 | 145,648/183,812 | 238,853.98/122,342.32 | 1.952/2.290 | 37,808,768 |
| Encode16 | 145,648/183,812 | 266,759.72/121,993.01 | 2.187/2.505 | 37,807,136 |
| WebpLossless8AsyncNonSeekable | 145,648/183,812 | 237,872.92/121,571.51 | 1.957/2.304 | 37,809,152 |

Photograph allocation rises from 28,606,536 to 36,327,472 B/op; the extra candidate
also costs resources when the previous packet wins. The verified compression gain
is accepted under compression-first ordering. These elapsed observations are not
a controlled estimate of the speed change. CPU parity remains deferred; no
independent timing repeat or speed-only optimization was performed.

Both OSes pass **4,792 tests each**, including 936 live FFmpeg comparisons, plus
57 internal/117 public affected scalar checks. Nine new tests cover the uncached
alphabet/flag, hidden RGB and 16-bit normalization, inherited-owner transfer and
actual retained charges, pre-admission local guards, terminal refusal/cancellation/
work exhaustion, recovery, exact output limits and independent outputs.

One immutable production/internal-test build per OS is reused throughout.
Unchanged public/differential tests, harness, native workers and selectors are
reused without builds. The same actual private 0.2.0 package/all three examples
pass on both OSes; published 0.1.0 remains unchanged. All seven lossy gates stay
closed with identical primary VP8 packets/quality anchors and unchanged separate
ALPH code. No workload is added; other valid evidence is reused. The
[residual-color round](WARM-VP8L-RESIDUAL-COLOR.md) is historical.

[Numeric evidence](vp8l-no-cache-evidence.json) retains every actual, allocation,
guard, producer, quality result and qualification. Mirrored raw:

- `c97-qualified-windows.tar.gz`: `959cdc1a7a0abb5184ba4fc817e7ba09238b2ad2fdbe3c8b6771afd68bd8827e`
- `c97-qualified-linux.tar.gz`: `2cf9b1b54cfa5e92431e66a26fc55d194de894023c97f1d6ba202d366abb7fd1`

Actual private package SHA256: `f74bc3565e74cb7d2a2c2612074079e3dd4375706944408bc0bcfcb987b05acf`.
