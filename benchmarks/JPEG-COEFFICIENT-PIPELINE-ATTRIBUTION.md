# JPEG coefficient pipeline attribution

JSA727 is a read-only audit of the qualified quarter-interpolation package.
No production, harness, fixture or package rebuild; existing costs and
qualification remain valid. This supplies a next architectural hypothesis,
not a speed claim or production acceptance.

One two-second CPU0 capture of complete sparse-CMYK decode/disposal has 1,993
sample locations: inverse AVX2 580, plain entropy reader 304, baseline scheduling
210, dequantization 160, component packing 129 and CMYK conversion 73. CPU1
samples; both idle-v2 guards and exact native/managed frame, input and ownership
checks pass. Instrumented elapsed is discarded. Inlining and sampling skid mean
these counts are neither phase times nor removable percentages. The earlier
packing capture predates the accepted paired packer; it is historical.

## Next design: baseline entropy to inverse

The baseline path clears 64 Int32 coefficients, stores raw amplitudes, then reads
and dequantizes all 64 into a separate short block before IDCT. A proposed short
coefficient destination would apply each quantizer at the validated token commit.
This removes a dense conversion pass and halves the coefficient clear, while
sharing the existing grammar and inverse arithmetic. Progressive/refinement
coefficients keep their actual Int32 representation.

An independent canonical T.81 model reads two existing frozen packets, verifies
their final padding/EOI and compares every proposed dequantized word:

| Existing control | Blocks | Dense product lanes | Proposed token products | Equal words |
| --- | ---: | ---: | ---: | ---: |
| Sparse CMYK, 22,708 B | 8,580 | 549,120 | 25,740 | 549,120 |
| Direct RGB444, 133,057 B | 6,630 | 424,320 | 228,205 | 424,320 |

These are modeled data operations, not physical traffic, elapsed savings or
general content distributions. The initial model reused the RGB MCU-count
assertion for CMYK; the assertion was corrected to each encoded sampling grid.
No pixel or arithmetic failure occurred. DC retains its full predictor/product,
level shift and saturation; AC retains modulo-word multiplication. Existing
full-domain quantizer/word/extreme tests remain necessary.

The external reference also fuses dequantization into baseline entropy output.
That strategy is insight only; no source or tables are copied. Lokad.Image's own
validated grammar and exact numerical contract remain authoritative.

## Transition and risks

Prototype one bounded coefficient destination with the two representations
needed by actual callers. Preserve all logical polls, prefix/amplitude failure
boundaries, reader cursor and partial-miss state, restart/stuffing handling,
terminal refusal, output ownership and late color interpretation. Share the
inverse core; remove temporary duplicate staging at the completed stage.

Per-token products and generic destination checks may outweigh dense SIMD on
less sparse data. Use complete sparse-CMYK and existing direct-RGB generalization
screens before accepting a design. Earlier paired progressive-component and
512-bit row studies did not establish useful gains; this proposes a different
baseline storage boundary, without another paired matrix or numerical change.
Judge elapsed time, allocation/retention and simplification together. There is
no automatic merge authorization from sample counts or this model.

[Evidence](jpeg-coefficient-pipeline-attribution-evidence.json) retains full
identities, source scope, counts, proposal and unchanged 6,133 checks/OS plus
scalar/ISA/SAME-package qualification. Raw
`tmp/jpeg-coefficient-pipeline-attribution.tar.xz`, 290,960 B / 29 members,
SHA256 `4dd56c4370c14d544fa1d5b033ee8735633a7011b684d2cffdb0a6bce28ad7fc`,
is mirrored and member-verified. It includes the trace, map/JIT, scripts,
canonical model, frozen packets and original receipts; the qualified package
archive supplies unchanged runtime dependencies. This diagnostic does not add
a benchmark-renderer dependency or alter the 283-path objective.
