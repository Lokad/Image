# VP8 whole-frame probability audit

Whole-frame coefficient probabilities recover useful bytes but still miss both
required size ceilings. This is an **untimed tooling experiment**, with no
production build or accepted change.

| Fixed opaque RGBA8 control | Current / replay bytes | Required ceiling | Coefficient partition, current / replay | Probability updates, current / replay |
| --- | ---: | ---: | ---: | ---: |
| Photograph, managed78/native75 | 21,432 / 20,138 | 17,688 | 17,748 / 16,357 | 13 / 66 |
| Text/diagram, managed75/native75 | 8,288 / 7,684 | 7,100 | 6,796 / 6,070 | 55 / 120 |

The original owned arithmetic reader/writer first reproduces both current header
and coefficient partitions exactly. It then counts branches across the whole
frame and applies the encoder's existing empirical-probability/signaling-cost
selection. Modes, quantizers, coefficients, category/sign probabilities, inputs,
quality settings and effort4 remain unchanged. Header partitions increase from
3,654/1,462 to3,751/1,584 bytes. Normal/scalar native YUV and normal/hardware-off
managed RGBA remain byte-identical to the original packets, preserving all frozen
source-quality and alpha metrics. The controls remain the two513×259 inputs.

Estimated ideal coefficient-tree savings before signaling are about1,462/835
bytes, below the3,744/1,188-byte gaps. These entropy estimates do not prove a
universally optimal arithmetic model; the actual whole-frame replay establishes
that this tested model alone misses both gates. Coefficient and prediction
decisions remain material opportunities. The earlier opaque16 full-cache timing
failure is reused, without another run or an inferred current performance claim.

No quality search, new input, production/harness/native/selector build, timing,
profile, full suite or package proof follows the failed size gates. The qualified
SIMD DCT producer/package and its seven current results remain unchanged.
[Numeric evidence](vp8-full-model-audit-evidence.json) retains sizes, bit estimates,
packet identities and independent pixel checks. Raw scripts, partitions, packets,
proof and helper logs are retained on both hosts in `c41-full-model-audit.tar.gz`,
SHA256 `d1b8cc71953040b8a2aae0da3f52a030942ee7220ff26359792c1f732f65689d`.
