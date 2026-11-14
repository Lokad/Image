# Current VP8 photograph attribution

The longer-batch complete comparison now demonstrates a small photograph gap.
One **two-second trace of the actual immutable package DLL** supplies current
attribution. CPU0 executes; CPU1 collects. Complete frozen513×259/Q75/method4
encoding, exact17,688Bpacket, source/settings/ownership and idle-v2 guards pass.
**Profiled elapsed is discarded; locations/skid are not component times or gains.**

| Current sampled location | Samples / 1968 total |
| --- | ---: |
| Luma4, including inlined quantization/rates/reconstruction | 518 |
| Coefficient emission | 161 |
| Luma16 | 108 |
| Small candidate prediction | 77 |
| Forward transform | 77 |
| Token statistics | 73 |

Actual Luma4 is6,630B; coefficient emission3,967B. Luma4's IG71quantizer/validation
has84samples, IG72index preparation26, IG76price summation56, IG107reconstruction62.
The quantizer reloads five fixed matrix vectors from the parent frame per mode;
the scalar sum reloads its price pointer inside the coefficient loop.
These are concrete repeated loads/register-pressure leads, not removable percentages.
Emission samples are distributed; no isolated emitter gain is established.

The old attribution predates four changed files: macroblock decisions, block
quantization and both rate-plan files. A bounded six-file comparison identifies
those invalidations; quantization-plan and reconstruction files remain unchanged.
Do not reuse the old hotspot ranking. Old quantizer/scoring and pair/packed
quantization outcomes remain leads/adverse history, not bans or replay approval.

**Next hypothesis:** separate nonuniform, fused, uncancelable luma4 candidate
preparation from ordered winner scoring after the existing batched transforms.
Prepare exact quantized values/rates for the same ten visited candidates in a
small loop with fewer live reconstruction variables; preserve all validation,
mode order, strict ties, candidate independence and selected levels/pixels.
Keep scalar, live-token, trellis and uniform behavior. Account for extra bounded
stack storage inside the existing16KiB allowance; add no heap/pool/session.
Inspect codegen and complete paired photograph/alpha costs before acceptance.
This proposed phase split is distinct from replaying the former single-mode
byproduct handoff. Additional stores/calls may outweigh reduced spills.

[Evidence](current-vp8-photo-attribution-evidence.json) records actual code,
mapped instructions, source audit, producer identities and both opaque captures.
Raw vp8-opaque-long-batch-attribution.tar.xz is mirrored/member-verified:
**176772B/53members**, SHA256
a2d294d298ed51742daaf5d0e15e5cdb9ebdc0aa9515bcfd5ccfd80b0aba1265.
No builds or broad/package checks repeat. Full289-path parity remains open;
publication is postponed.
