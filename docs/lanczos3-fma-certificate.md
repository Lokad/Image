# Exact Lanczos3 pixels with FMA accumulation

The accelerated kernel keeps the Q30 coefficient plans. Byte rows use exact
integer filtering and convert to double in place. Word rows reuse exact UInt32
premultiplied samples and form double rows with four FMA chains. Vertical FMA
proposes pixels; acceptance requires an error interval strictly inside all four
clipped rounding bins. Uncertain pixels use the original signed Int64/Int128
recomputation and half-up rule. Final managed samples remain exact.

Let `M=65535`, `B=2^31`, `Q=2^60`, with largest horizontal/vertical counts `h,n`.
The plan checks that each axis has `sum(abs(weights)) <= B`.

- Exact horizontal RGB prefixes have magnitude at most `M²B < 2^63`;
  alpha prefixes are at most `MB < 2^47`. Byte conversion contributes at most
  `512` RGB error. Each word FMA and the three recombination additions round by
  at most `512`, giving RGB error at most `(h+3)·512`. Word alpha remains an
  exactly represented integer. These errors fit the horizontal headroom even
  for `Int32.MaxValue` counts.
- Exact vertical RGB and alpha prefixes are below `2^94` and `2^78`.
  Their respective headrooms exceed `2^78` and equal `2^62`. Propagated conversion
  error and every preceding FMA error remain within those headrooms, including
  `Int32.MaxValue` tap counts. Thus each FMA rounds by at most `2^41` RGB or
  `2^25` alpha, establishing total bounds
  `E_RGB=(k+2n)·2^40` and `E_A=n·2^25`, where `k=1` for byte rows and `k=h+3`
  for word rows.
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

The admitted horizontal cache is never copied or exposed. Byte execution uses
successive Int64/double views; word execution writes double directly and reuses
the existing UInt32 product row. No extra capacity is needed. Row/sum storage is
reserved before allocation: one-shot operations release it after actual users
finish, while prepared plans retain it until disposal. Returned images have an
independent owner. Insufficient local workspace selects
the integer kernel before admission; refusal, cancellation and allocation failure
terminate and clean up without retry or waiting for capacity.

Hardware without the required AVX512F/DQ/VL, AVX2 and FMA support uses the integer
kernel. Focused tests check actual conversion/FMA envelopes against BigInteger,
full unsigned products, exact alpha, guard storage and zero kernel allocation,
half ties, clipping, negative/tiny alpha, long support, cancellation, shared-root
refusal/retained results and tight local limits. The four original full frames
remain separate exact-pixel checks. See the
[benchmark state](benchmark.md) for measured scope and remaining parity gaps.
