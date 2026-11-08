# Earlier original PNG Fastest costs

Superseded by [streaming token frequencies](PNG-STREAMING-TOKEN-FREQUENCIES.md).
These measurements remain the unchanged-original comparison evidence.

2026-11-08. These are the unchanged production implementation after exact
restoration to [JFW547](JPEG-LIVE-ENTROPY-WINDOW.md), not the unselected prefix
experiment. [Machine evidence](current-png-fastest-costs-evidence.json) owns three
existing original controls; twelve other encoding costs retain unchanged evidence.

| Complete one-shot control | Lokad / native µs | Ratio upper | GC B/op | Status |
| --- | ---: | ---: | ---: | --- |
| RGBA8 Fastest1 | 4,540.44 / 4,332.94 | 1.065 | 820,916 | Gap |
| RGBA16 Fastest1 | 18,513.54 / 18,638.38 | 0.999 | 2,437,660 | Pass; stop |
| RGBA16 Fastest1, live token | 18,830.06 / 18,588.37 | 1.018 | 2,437,660 | Gap |

The live control uses the conservative two-launch envelope; the other original
launch is 18,795.73 / 18,624.81 µs, upper 1.014. Every launch retains twelve actuals,
both native brackets and idle-v2 checks. This resolves prior uncertainty without
claiming a causal improvement over older measurements. All fifteen encoding
controls now total **10 pass, 3 gap, 2 uncertain**; forward-word Fastest and word
Smallest remain uncertain. Do not substitute the experiment's candidate costs.

Frozen 513×259 inputs, filter None, level 1 and native settings are unchanged.
Both sides time fresh codec/zlib/working-storage preparation, encoding and
independently owned complete output. Byte/word packets remain exactly identical
to the reference: 236,828 / 968,216 bytes. Managed pixel and full 16-bit samples,
hidden RGB, cancellation, bounded resources and ownership contracts remain exact.

Codec admission stays 384 KiB with zero retained operation scratch. Total measured
allocations are above; no physical/native peak was measured. Unchanged production
reuses 5,879 full/live checks per OS plus affected scalar proof. The same actual
0.2.1 package, SHA-256 `cdbaa146508f2300b88b24dd1195582bf3eeabcc3358fd9b9dc80d330bb3fc8f`,
passes all three examples and the newly retained Fastest sync/async ownership and
frozen-packet checks on Windows/Linux. No production/package rebuild was needed.

Current evidence and the separately [unselected study](PNG-PREFIX-PREPARATION-EXPERIMENT.md)
share a verified raw archive, identified in the machine evidence. No rejected-study
report is required by the benchmark renderer. Next: attribute the original live
word gap of approximately 242 µs; stop the passing ordinary word control.
