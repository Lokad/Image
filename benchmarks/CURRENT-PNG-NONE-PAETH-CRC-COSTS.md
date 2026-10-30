# Current PNG None/Paeth decode costs

Two existing 513×259 resident decode controls are reconfirmed on the qualified
GFNI CRC build because the shared CRC change invalidated their older timings.
Both remain **confidence-open**: conservative intervals cross parity. Keep the
one observation each; neither establishes an optimization need or a CPU pass.

| Complete decode | Lokad µs | Ratio | Conservative interval | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| None RGBA16 | 2,908.794 | 1.004 | 0.993–1.014 | 8,568 |
| Paeth RGBA8 | 2,861.646 | 1.006 | 0.992–1.016 | 4,472 |

Each subject decodes the same frozen packet into fresh independently owned RGBA
pixels, including normalization and output release. Every RGBA, alpha and hidden
RGB sample is exact; original source, settings, native worker and ownership gates
pass. Native allocations are unmeasured. These are current costs, without a
causal comparison against earlier producers.

Each job has twelve actuals using the cached four-operation selector. All actuals,
native brackets, filtered/unfiltered confidence and both idle-v2 guards pass and
are retained. There are no builds, new fixtures/settings, profiles or successful
retiming. Production is unchanged; the [CRC qualification](WARM-PNG-GFNI-CRC.md)
applies: 5,193 checks per OS, focused normal/GFNI-disabled/scalar checks and the
same actual Release package, governed async consumer and three examples on both
platforms. The full 268-path warm parity objective remains open.

[Evidence](current-png-none-paeth-crc-costs-evidence.json) records producers and
the mirrored, member-verified raw archive with pinned dependencies.
