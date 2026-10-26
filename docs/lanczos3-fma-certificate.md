# Exact Lanczos3 pixels with FMA accumulation

The accelerated kernel keeps the existing Q30 coefficient plans and exact
horizontal integer filtering. It converts each cached row to double in place,
uses FMA for vertical accumulation, and accepts a pixel only when an error
interval lies strictly inside all four clipped rounding bins. Uncertain pixels
are recomputed with the original signed Int64/Int128 arithmetic and half-up rule.
RGBA8 and RGBA16 use the same conservative word-sample certificate.

Let `M=65535`, `B=2^31`, `Q=2^60`, and `n` be the largest vertical window count.
The plan checks that each axis has `sum(abs(weights)) <= B`.

- Exact horizontal RGB prefixes have magnitude at most `M²B < 2^63`;
  alpha prefixes are at most `MB < 2^47`. Int64-to-double conversion contributes
  at most `2^9` RGB error and no alpha error.
- Exact vertical RGB and alpha prefixes are below `2^94` and `2^78`.
  Their respective headrooms exceed `2^78` and equal `2^62`. Propagated conversion
  error and every preceding FMA error remain within those headrooms, including
  `Int32.MaxValue` tap counts. Thus each FMA rounds by at most `2^41` RGB or
  `2^25` alpha, establishing total bounds
  `E_RGB=(1+2n)·2^40` and `E_A=n·2^25`.
- If `Ahat < Q/2-E_A`, exact rounded alpha is zero and the result is transparent
  black. Otherwise, `Ahat >= Q/2+E_A` is required before proposing a pixel.
  The uncertain alpha interval takes the exact path.
- Clipping a quotient to `[0,M]` bounds its perturbation by
  `(E_RGB+M·E_A)/(Ahat-E_A)`. Alpha's bound is `E_A/Q`. The error numerator has
  at most 49 significant bits and is exactly representable. An additional `2^-28`
  allowance covers double division and bin comparison: even the theoretical
  maximum radius is below 20,480, with numerical error far below that allowance.
- Nearest-even rounding only proposes a candidate. Acceptance requires strict
  distance from the half-bin boundary exceeding the bound in every channel.
  Half ties and uncertain bins use exact recomputation, preserving half-up pixels.

The horizontal cache has one admitted backing allocation viewed successively as
Int64 and double; it is not copied or exposed. Row/sum capacities are reserved
before allocation and released with the operation. Returned images retain their
independent owner. Insufficient local workspace selects
the integer kernel before admission; refusal, cancellation and allocation failure
terminate and clean up without retry or waiting for capacity.

Hardware without the required AVX512F/DQ/VL, AVX2 and FMA support uses the integer
kernel. Focused tests check actual conversion/FMA envelopes against BigInteger,
half ties, clipping, negative/tiny alpha, long support, cancellation, shared-root
refusal/retained results and tight local limits. The four original full frames
remain the separate exact-pixel qualification gates. See the
[benchmark state](benchmark.md) for measured scope and remaining parity gaps.
