# Streaming PNG pixel validation

The existing row inflater now validates common nonindexed, noninterlaced PNG in
one Deflate walk, without a raster or second native inflation. The operation
admits 64 KiB of history before allocation through its existing pool/root rules.
Filters, row counts, Deflate validity, CRC and Adler remain checked. Indexed/
Adam7 and incomplete-tree or uncertain acceptance cases retain the original
reader; history is released before correctness replay. Admission refusal stays
terminal. Actual decoding still reconstructs exact pixels with the same engine.

The fixed 513×259 PNG16 span control independently measures **2,933.20 µs**,
**42.4% less elapsed**, versus the preceding confirmed 5,090.51 µs. Its bounded
repeat passes at **0.990× / upper0.99966**; the earlier independent upper1.00108
is retained. This margin is narrow. Async confirms0.833× / upper0.847. Stop these
two passing cases unchanged. PNG8 remains a1.355× short gap; indexed Adam7 is
unchanged at1.345× short. The full267-path ≤1× objective remains open.

Warm default-null-budget allocation falls from17,008 to**240 B/op** for PNG16
span and to8,544 B/op for async. Caller-budget calls bypass pools: their fresh
managed allocation increases while admitted peaks decrease. The
[governed storage table](PIXEL-VALIDATION-STORAGE.md) reports those separate costs.
[Evidence](warm-png-streaming-validation-evidence.json) retains every actual,
native bracket, quiet guard, descriptor/input/ownership proof and producer.

Both platforms pass5,041 Release checks each, including947 live comparisons,
plus137 internal and88 public scalar checks. Maximum/overlapping history distances
cross64KiB slides; both depths, cancellation modes, all legal PNG color/filter/
Adam7 fixtures, late row/checksum errors and shared cleanup/recovery pass. The
SAME actual private0.2.1 package and three usage examples pass both systems.
One production build per OS and unchanged native/harness/selectors serve the round.

The [two-walk baseline/profile](png-validation-current-evidence.json) motivated
this change; its CPU attribution is superseded for the fused path. An inverse-
filter skip and [two SIMD classifiers](png-literal8-scanners-deferred-evidence.json)
had no gain and are removed. Keep those families closed. Cited mirrored raw
masters retain source, binaries, scripts, all observations and reproducers.
