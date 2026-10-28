# VP8 quantization caller invariants

The fixed photograph/effort4 control falls from 14,934.943 to 13,899.636µs in fresh
independent twelve-actual confirmations: 6.93% less filtered elapsed,6.90% with
all actuals. Both confidence-interval forms separate; faster-native-bracket drift
is 0.082%. Allocation stays 809,072B/op and the exact 17,688-byte packet is unchanged.
The resulting 1.433× / upper 1.438 remains a CPU gap.

| Existing 513×259 control | Lokad µs | Ratio / upper | Allocated B/op | Both packet bytes | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Photograph effort4 | 13,899.636 | 1.433 / 1.438 | 809,072 | 17,688 | Confirmed gap |
| Diagram effort2 | 3,331.686 | 1.328 / 1.463 | 741,375 | 7,330 | Screen gap |
| Diagram effort4 | 7,750.982 | 1.331 / 1.478 | 741,137 | 7,100 | Screen gap |
| Diagram effort6 | 13,710.684 | 1.394 / 1.740 | 754,344 | 6,818 | Screen gap |
| RGBA8 + alpha | 70,231.817 | 1.308 / 1.382 | 21,353,208 | 98,590 | Screen gap |
| Opaque RGBA16 | 20,535.936 | 1.318 / 1.375 | 1,201,552 | 72,380 | Screen gap |

All affected displayed costs are current. Companion allocations/packets are
unchanged; unmatched short screens establish no causal gain or regression.
Complete warm one-CPU operations retain normalization, public admission/encoding,
fresh independent output and release. Inputs, settings and FFmpeg/libwebp workers
are frozen. Exact packets, reconstructed pixels, alpha, source/owners and both
idle-v2 guards pass. No image/settings grid, new buffer or capacity change.

All ten ordinary quantizer call sites use separate block buffers and live generated
plans. Steps 4–440, bounded sharpening/bias and the 16,320 coefficient domain give
scaled numerators ≤534,832,640 and reconstruction magnitude ≤16,492. The retained
17,928-policy overapproximation bounds every smaller coefficient by monotonicity.
Private repeated checks and the unreachable malformed-matrix overflow fallback
can go. Quantized-level arithmetic, zigzag, cap and scalar checked math remain
identical, as do all public bounds/cancellation/admission and ownership. The change
removes 56 net production and 78 test lines. Nine rows solely testing unreachable
private invalid-range/alias/overflow-prefix behavior go; native hashes, signed
packing endpoints, 73,504 arithmetic blocks, tails and no-allocation coverage stay.

The six-second diagnostic captures 5,915 samples. All quantization instruction
addresses map to same-process 516/1,098/397-byte bodies, respectively 178/366/309
samples. Shares are diagnostic, not method times or predicted gains. No host,
harness, native worker or selector build is needed. An initial stale-manifest
guard failure precedes target launch; metadata synchronization corrects it. A
parser fix recovers the original samples without a repeated diagnostic.

The first short is 13,831.016µs,3.02% below retained older-producer evidence, so
its original 5% triage gate is explicitly not passed. The plan permits one unchanged
build's independent pair for the substantive simplification, requiring ≥2% in
both views and separate intervals. That fresh pair exceeds 5% as well. All samples,
including one native short-sample excursion that widens its upper, remain retained;
there are no candidate variants or repeated short screens. The final upper ≤1×
objective and byte/quality/confidence gates do not change.

Windows/Linux each pass 5,065 checks (1,994 internal, 2,123 public, 948 live), 41
focused cases normal/scalar and 27 public scalar packet/admission/async cases.
The SAME actual private 0.2.1 package and three examples pass both platforms,
including frozen method 0–6 sync/async controls and shared ownership. Published
packages remain immutable. Other affected named VP8 boundaries need applicable
current costs before CPU-pass claims. Other codecs, decoding, editing/text and
passing resize paths retain unchanged applicable evidence. No new parity pass.

[Evidence](warm-vp8-quantization-caller-invariants-evidence.json) pins samples,
source, range proof, qualification and unchanged native history. Mirrored,
member-verified vp8-quantization-caller-invariants-qualified.tar.xz: 267 members,
SHA256 a531df0a3acf92762c95793cfc513a33d9598cdefcfbe73424b7da8b88ef1ad8.
The full 268-path objective remains open.
