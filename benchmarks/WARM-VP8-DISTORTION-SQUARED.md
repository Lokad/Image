# Exact squared groups for distortion-only VP8 search

The effort2 diagram independently measures 7.85 ms: 3.128× reference time
(upper 3.148×), 741,257 B/op and an exact 7,330-byte packet. Elapsed time is
13.20% below its preceding quiet short; these are not a paired baseline gain.
It remains the largest measured gap, ahead of RDO/trellis controls near1.8×.

| Quiet one-CPU run | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Output bytes |
| --- | ---: | ---: | ---: | ---: | ---: |
| DiagramLossyDistortion8 short | 7,924.02 | 2,512.48 | 3.154 / 3.348 | 741,257 | 7,330 |
| DiagramLossyDistortion8 confirm | 7,848.72 | 2,509.34 | 3.128 / 3.148 | 741,257 | 7,330 |
| PhotographLossy8 short | 17,948.88 | 9,710.55 | 1.848 / 1.969 | 809,056 | 17,688 |
| DiagramLossy8 short | 10,276.48 | 5,837.16 | 1.761 / 1.807 | 741,121 | 7,100 |
| vp8 short | 76,876.39 | 53,507.96 | 1.437 / 1.514 | 23,258,168 | 98,590 |
| DiagramLossyTrellis8 short | 17,452.62 | 9,806.85 | 1.780 / 2.621 | 754,328 | 6,818 |
| EncodeOpaque16 short | 26,830.90 | 15,618.90 | 1.718 / 1.783 | 1,201,536 | 72,380 |

All idle-v2 guards pass. Confirmation retains12actual/10filtered samples and the
conservative bound over both sets. Other rows are shorts; the photograph reuses
the exact candidate/harness/worker/input observation from its earlier screen.
The trellis upper2.621× remains uncertain, without a passing/gain claim. Complete
packets, registered reconstructed samples/alpha, source and independent ownership
remain exact. Complete normalization, packet growth/acquisition and release are
timed; startup, IPC, validation and ToArray are not.

The original squared-metric candidate was marginal on RDO photography. The newly
identified distortion-only search path exposed a materially larger gap, so the
same already-built candidate was rescreened once there. No variant or production
rebuild was needed. Its AVX2 kernel packs complete16byte groups, widens differences
and adds adjacent squares exactly. Existing width4/16callers use it; other widths,
tail rows and portable execution retain scalar code. Identical4rowpacking is shared
with texture spectrum. Differences are bounded by255; the full16×16sum is at most
16,646,400. Validation, all integer scores/ties, owners, cancellation and lifetimes
remain. No quantizer, quality, mode policy or packet changes.

Both systems pass4,944Release checks (1,962internal/2,035public/947live) and623scalar
WebP checks. Reused candidate focus includes native metrics/macroblock/frame
oracles; the retained rectangle contract covers2,304layouts and6,912assertions,
including strides, exact tails, overlappingread-onlyinputs, symmetry and guards.
The same actual private0.2.0package and all three examples pass both systems.
Existing production binaries were reused; only missing tooling/linked tests were
built. Published0.1.0is unchanged.

Confirmation preparation initially exposed a launcher/child assembly mismatch,
then missing redirected reference/assets files when reference builds were
suppressed. These zero-benchmark failures are retained. Standard generated-project
restore/build with a binary-reference harness fixed preparation without production
compilation; the measured harness bytes were reused for confirmation.
[Evidence](warm-vp8-distortion-squared-evidence.json) retains all identities,
reuse, failures, samples and the mirrored, member-verified raw archive. Unchanged
decoder/other-codec/edit/text proof carries forward. Full255case warm parity and
replacement readiness remain open; prioritize this3.128×path before smaller gaps.
