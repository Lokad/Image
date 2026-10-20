# VP8 coefficient-rate audit

Both candidates are **rejected**. The mathematical rate/distortion model saves
photograph bytes but fails matched reconstruction quality. Requiring actual
block error not to worsen restores quality, with immaterial size savings.

| Frozen effort4 control | Model bytes / PSNR gap dB | Reconstruction-guard bytes / PSNR gap dB | Required ceiling |
| --- | ---: | ---: | ---: |
| Photograph, managed78/native75 | 16,130 / 0.707970 | 21,316 / 0.010711 | 17,688 |
| Text/diagram, managed75/native75 | 7,206 / 0.555650 | 8,230 / 0.231759 | 7,100 |

The existing quality gate is **≤0.25dB** from the frozen native source PSNR,
jointly with exact alpha, decoder agreement, size, resources and complete-operation
warm parity. Inputs remain the two513×259 opaque controls. No quality, effort or
reference setting is retuned. Current production stays21,432/8,288 bytes.

The independently derived bounded coefficient search considers zero and the
nearest bounding magnitudes, preserving sign. It accounts for the previous
0/1/2 token context and canonical end-of-block signaling. Exhaustive enumeration
checks36 short mathematical sequences;144 further comparisons never worsen the
declared model objective. Actual integer inversion, clipping and subsequent
prediction require independent source-quality checks; the first candidate
demonstrates why the coefficient objective alone cannot establish quality.

The second candidate compares reconstructed luma error with original rounding
for the same prediction and rejects worsening blocks. It passes source quality
but saves only116/58 bytes. That local check is not a global image-quality proof.
Normal/scalar focused suites pass16/17 checks for the two variants, respectively,
including original-coefficient equivalence and exhaustive search. Both preserve
the source, independent outputs, alpha and native/public decoder bounds.

Two immutable Linux core/internal-test builds; no Windows, harness, native or
selector build. Failed size/quality gates receive no timing, allocation claim,
full suite or package proof. Candidate code, tests, packets and logs are retained;
qualified C40 source, incremental outputs, test artifacts, runtime and actual
package are restored exactly without rebuilding.

[Numeric evidence](vp8-coefficient-rate-audit-evidence.json) retains every quality
result and identity. Raw archives on both hosts:
`c42-first-candidate.tar.gz`
`27b132f2f1b13e7cd1c6920521c899381f9232b67c5ed1843aa5d797526767a7`;
`c42-reconstruction-candidate.tar.gz`
`d771621bcd85ef8401ceab048403f556a319cce713f5906ce9d50082e81e7c1d`;
`c42-coefficient-math-proof.tar.gz`
`c9d5cf0cd5c4bdac01f1f64604ae113d7bb851fcf5ba7e44ddeed8f3574fe36e`.
