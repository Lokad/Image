# Certified SIMD Lanczos3 normalization

Four-channel SIMD normalization cuts complete Lanczos3 time **16.6–47.3%**
from the preceding exact SIMD accumulation. All four existing cases remain
gaps against independently confirmed warm single-CPU parity.

| Complete operation | Lokad µs | Reference µs | Ratio / conservative upper | Lokad allocated B/op | Time saved |
| --- | ---: | ---: | ---: | ---: | ---: |
| Reduce8 | 2,235.87 | 491.27 | 4.551 / 4.947 | 146,631 | 16.9% |
| Reduce16 | 2,261.93 | 507.94 | 4.453 / 5.010 | 146,611 | 16.6% |
| Enlarge8 | 8,957.60 | 4,071.92 | 2.200 / 2.356 | 351,912 | 47.3% |
| Enlarge16 | 10,026.21 | 3,943.92 | 2.542 / 2.808 | 4,617,090 | 44.0% |

AVX512DQ/VL converts the existing certified Int64 limb sums to binary64 and
normalizes four channels together. Absolute conversion/combination errors are
bounded by `EN=2^44` for RGB and `ED=2^28` for alpha. For positive actual and
estimated alpha at least `2^59`, clipping to `[0,M]` is `1/d`-Lipschitz in the
numerator and `M/min(d,dHat)`-Lipschitz in the denominator. Thus the clipped
quotient error, including division and bin comparison, is at most
`1572857/17179869184 < 2^-10`. Alpha's separate Q60 normalization also satisfies
that margin. A candidate is accepted only strictly inside its rounding bin by
`2^-10`; uncertain pixels keep the existing exact integer path.

The denominator guards compare against exactly representable `2^59±2^28`.
Below the lower guard, exact rounded alpha is zero and all channels clear.
Unsupported hardware retains the accepted path. No coefficient, accumulation,
scratch, owner, budget, work or cancellation boundary changes. Managed pixels
remain exact; small allocation differences reflect amortized diagnostic overhead.

Five new checks compare independent rational rounding at exact halves, clipping,
alpha thresholds, large opposing limbs, discarded low bits, deterministic
interiors and overshoot. All four protected full frames match on Windows/Linux.
One frozen producer and the original fixtures, native quality proofs, harness
and compiled selectors were reused throughout. Reference remains explicitly
**zimg, FFmpeg's zscale backend**, with complete operation boundaries and quality
gates from the [baseline](WARM-LANCZOS3-BASELINE.md).

Eight scored quiet guards pass. One Enlarge16 preguard refused I/O wait and
launched nothing; its retry passed fresh guards. Both native brackets and
selected/all-actual confidence views are retained. No long confirmation was
run above one, no profile was added and no workload grid was expanded.

Qualification: **4,445 Release tests per OS** (1,632 internal, 1,881 public and
932 live FFmpeg), **44 intrinsics-disabled Lanczos checks per OS**, the same
actual private 0.2.0 package consumer and all three packaged examples on both
systems. Twelve font comparisons retain unchanged production paths. Published
0.1.0 is immutable. Only these four timings refresh; the other 227 numeric rows
retain their identified historical builds. Parity and replacement readiness
remain open.

[Numeric evidence](warm-lanczos3-normalize-evidence.json) retains samples,
allocations, mathematical bounds, identities, failed guard and reuse. Qualified
source: `959b6282703774ba6519f761667d0c8e57c9a8424b9ee4c32fe122468abfce2f`.
Raw `l3n-qualified-windows.tar.gz`:
`9ce953a8cf4760ce3eee4723fba1771e4078eac58340842ea4c971c9a7391a8f`;
`l3n-qualified-linux.tar.gz`:
`77cbe5c6b9f3fb52c62e39d97776dc6231e09fde2a34b8033b101feae564c9ea`.
Unchanged dependencies, fixtures and worker remain in the baseline archive.
Next: investigate exact SIMD horizontal products on Reduce8, with an early
screen to establish whether that primitive provides a substantial win.
