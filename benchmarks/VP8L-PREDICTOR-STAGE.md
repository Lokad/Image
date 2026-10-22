# VP8L predictor stage

An independent source-only model now matches the predictor map and residual
samples for the existing513×259 RGBA8 control: libwebp1.6.0, method4,
quality75, exact=1. This is an encoder stage proof; production still emits a
different complete packet, and other controls remain open.

The model computes all2,145 modes in8×8 tiles from source pixels after subtract-green.
Costs combine local and accumulated residual distributions, deterministic ties,
small-residual preference and neighboring-mode preference. Fixed-point logarithms
are generated mathematically; upstream implementation tables are not inputs.
All531,468 residual bytes match the values decoded independently from the frozen
reference packet after undoing its cross-color transform.

The same Windows-built model passes Windows/Linux comparisons, source preservation,
independent output ownership and negative mutations. No production/native rebuild,
native encode, package requalification or performance run accompanies this stage.
Resource admission and complete encoder qualification await production integration.
Automatic tile-size analysis and broader effort/settings coverage are unqualified.

Run `tools/WarmReference/verify-vp8l-predictor-stage.ps1` with `-SourcePath`,
`-NativeModesPath` and `-NativeResidualGzipPath`; optional `-AssemblyPath` reuses
an immutable stage build. [Numeric evidence](vp8l-byte-parity-evidence.json) pins
inputs, hashes, exact scope and raw records. Next: source-derived cross-color
coefficients and residuals, then shared auxiliary/main-image entropy coding.
