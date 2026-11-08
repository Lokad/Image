# Current grayscale JPEG attribution

This pre-bank capture led to [prepared Huffman replay](JPEG-PREPARED-HUFFMAN-BANK.md).
Its replay locations/shares and former costs are historical; that report owns
current comparisons. Unchanged preparation locations remain leads, not phase costs.

JGA620 profiles the qualified VPR619 module without changing or rebuilding it.
The target is complete **RGBA8→grayscale JPEG**, including flattening and
normalization: then source-valid **1,660.52/1,522.24µs**, upper1.103, versus the
explicitly labeled SIMD-enabled libjpeg-turbo reference. Caller-GRAY8 is a separate
**1,218.17/1,136.37µs** boundary. Their difference is not precise normalization
attribution. Both retain exact 58,910-byte packets.

One two-second diagnostic capture uses the existing complete public-API harness,
frozen inputs/settings and exact packet/quality/ownership gates. Both idle-v2
guards pass; operationCPU0/samplerCPU1, one CPU, tiering off. It collects1,997
own samples,1,954 mapped to managed methods. These are sample locations, subject
to inlining/skid, **not isolated phase costs, parity timing or predicted savings**.

| Current method | Own samples |
| --- | ---: |
| Entropy record replay | 443 |
| Binary64 grayscale row conversion | 230 |
| Coefficient recording | 224 |
| Entropy append / pair entry / byte emission | 193 /187 /94 |
| Block preparation / RGBA8 normalization | 149 /123 |

Actual JIT output confirms that replay follows every record through table family,
symbol and separate length/code arrays before calling the writer. This supports
investigating a bounded prepared Huffman symbol bank and coherent replay boundary.
Establish its storage/ownership and production call preconditions before changing
code; preserve exact bits, per-record cancellation, MCU boundaries, stuffing,
flushes and failure cleanup. Avoid retaining test-only production implementations.

The ordinary eligible grayscale encoder already records coefficients/histograms
once and replays them after optimal tables exist. It does **not** run two ordinary
transform passes. Tight/restart paths intentionally recompute. AVX2 transforms,
once-per-operation reciprocals and eight-pixel row batching already exist.
Block preparation is an independent lead if replay redesign is unhelpful.

No new workload, setting, build, performance claim, qualification repeat or package
proof occurs. VPR619's5,939checks/OS and actual package/three examples remain valid.
This report adds no benchmark-renderer dependency. Full283-path parity remains
open; publication is postponed.

[Evidence](jpeg-grayscale-current-attribution-evidence.json) pins the current
module/source, samples/JIT, reused harness/reference, exact gates and next design.
Raw `jpeg-grayscale-current-attribution.tar.xz`, SHA256
`5685820ec4424e93e81c3445f8bf409a6956498de9e075d88a119b0535415aa9`,
97,292B/25members, is mirrored/member-verified with retained input dependencies.
A final dependency-field naming mismatch is recovered from the completed archive;
no profile or measurement is replayed.
