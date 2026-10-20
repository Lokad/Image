# VP8 source-only spatial quantization screen

This policy is **rejected**. Source-gradient clustering makes smaller packets but
misses both size ceilings and the unchanged0.25dB source-quality gate. The qualified
encoder and build are restored exactly. No timing, scalar/full-suite or package
repeat follows this failed two-control screen.

| Fixed opaque513×259 control | Candidate / reference bytes | Source PSNR gap dB | Quality gate |
| --- | ---: | ---: | --- |
| Photograph8 | 20,284 / 17,688 | 0.589574 | Fail |
| Diagram8 | 7,868 / 7,100 | 0.577088 | Fail |

The fixed policy clusters log source gradients into four groups, then scales the
base AC step by a bounded square-root contrast ratio. Its map is entirely source-
derived; no native assignments are copied. Quality78/75, native75/effort4, source
rasters, normalization and references remain frozen. The [preceding correlation](WARM-VP8-ACTIVITY-AUDIT.md)
is useful diagnostic evidence, not a sufficient rate/quality policy.

Native/public decoding, exact opaque alpha, deterministic independent outputs and
unchanged sources pass the normal decoder checks. This does not qualify the
candidate's resource/performance paths. One Linux core build and the prepared
worker/fixtures/quality checks suffice; zero Windows/core-selector/native/harness
builds or scored jobs. Baseline sources and incremental outputs are restored
without rebuilding the unchanged accepted production library.

Current production remains21,432/8,288 B against required17,688/7,100 B. The matched-
quality, complete-operation, memory and independent warm-parity requirements all
remain. Do not lower quality, change reference settings or sweep this policy's
parameters to turn the failed screen into a comparison.

[Numeric evidence](vp8-spatial-quantization-screen-evidence.json) records the complete
quality observations and candidate/source identities. Mirrored source, candidate,
packets, script and rollback proof: `c52-spatial-quantization-screen.tar.gz`, SHA256 `6bdfd61cb364883a9cc37b8fe7f629dc7d09447d19f36e4597fd54568bbaecc1`.
