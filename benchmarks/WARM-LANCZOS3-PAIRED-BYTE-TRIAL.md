# Exact paired-integer Lanczos3 byte trial

Neither variant supplies a material complete-operation improvement. The first
is parked at about 3.5% lower point mean; preparing packed coefficients once is
effectively neutral. Accepted source, core, internal tests and symbols are restored
exactly on both OSes, without a restoration rebuild.

| Existing Reduce8 operation | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| VariantA | 732.36 | 489.96 | 1.495 / 1.618 | 142,598 |
| VariantB | 758.12 | 490.64 | 1.545 / 1.848 | 191,578 |
| Accepted, retained | 758.75 | 490.07 | 1.548 / 1.646 | 138,466 |

One 4,136-byte source row stores exact premultiplied RGBA8 centered at32767/128.
Q30 normalization and absolute-weight bounds keep signed coefficient halves in
Int16. Each paired multiply-add fits Int32, then widens before accumulation.
Recombination and one normalized bias recover the exact original Int64 sums.
The original double conversion, vertical certificate and exact fallback remain.
An independent proof checks1,280windows/5,120channels/69,568pair lanes, all tail
phases and original256Q30windows. No new floating error or pixel tolerance.

VariantB prepares packed factors once per operation, replacing repeated packing
inside every row. It admits the full array capacity, including unused tail slots.
Both variants preselect local work and combined overlapping temporary/output
storage, reserve before allocation and retain independent result owners. Shared
refusal is terminal; cancellation cleans up and subsequent operations succeed.

Focused controls run60/61cases in normal, all-intrinsics-off and AVX512F-off modes
per OS. Actual accelerated Linux rows equal independent integer sums; hardware
without the required instructions keeps the existing paths. Original full-frame
managed bytes and native normal/scalar quality controls remain exact/bounded.

Each immutable variant reuses the same existing fixture, native worker, harness
and selector. Both three-actual shorts retain native3+3 brackets, all four quiet
guards and selected/all-actual confidence views. Reference remains
**zimg, FFmpeg's zscale backend**, with the
[same complete boundary](WARM-LANCZOS3-BASELINE.md). No extra image, settings grid,
diagnostic, longer repeat above one or full/package qualification for parked code.

Accepted4,484tests/OS and the same actual private0.2.0 package/consumer examples
remain valid. Published0.1.0, all selected rows and finite coverage counts remain
unchanged. Sources, actual binaries/symbols, tests, samples and proofs are retained
in four mirrored verified archives. [Evidence](warm-lanczos3-paired-byte-trial-evidence.json)
contains their hashes, capacities and identities. Next: one current-cost diagnostic
of the largest current JPEG gap, the existing sparse-2D CMYK decode. Uniform warm
parity and replacement readiness remain open.
