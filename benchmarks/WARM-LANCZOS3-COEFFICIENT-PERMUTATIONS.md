# Packed Lanczos3 coefficient lanes

One coefficient-load primitive improves the four existing complete Lanczos3
operations **2.8%–7.2%**, preserving exact pixels.
All remain measured gaps against independently confirmed warm single-CPU parity.

| Complete operation | Lokad µs | Reference µs | Ratio / conservative upper | Lokad allocated B/op | Time saved |
| --- | ---: | ---: | ---: | ---: | ---: |
| Reduce8 | 894.40 | 488.96 | 1.829 / 1.993 | 146,652 | 7.2% |
| Reduce16 | 931.20 | 507.17 | 1.836 / 2.174 | 146,643 | 3.7% |
| Enlarge8 | 5,671.76 | 4,066.82 | 1.395 / 1.504 | 351,910 | 3.1% |
| Enlarge16 | 6,661.47 | 3,928.07 | 1.696 / 1.806 | 4,617,090 | 2.8% |

Each four-tap group reads four signed coefficients together from the admitted
axis plan. Signed widening and two register permutations reproduce the previous
RGBA weight lanes exactly, including negative coefficients. Invariant index
vectors are prepared before the window loops. No expanded table or cache.
Premultiplication, tap/subset order, accumulation, normalization, scratch,
ownership, work and 1024-tap cancellation boundaries are unchanged. Existing
independent BigInteger rows cover 1031-tap support and a three-tap tail; all four
protected full frames remain exact on both OS.

One initial coefficient variant also passed exact controls and a quiet Reduce8
screen. Moving its index constants outside the loop was neutral; no separate
gain is claimed. The selected source is frozen across four scored shorts; only
that producer receives full qualification. Both variants/raw samples are retained.
Fixtures, reference worker, quality proofs, harness and selectors are reused.
All eight selected quiet guards pass. No repeated profile, new raster/settings
grid or long confirmation above one.

Reference is **zimg, FFmpeg's zscale backend**, with the complete equivalent
operation from the [baseline](WARM-LANCZOS3-BASELINE.md). Native pre/post brackets,
selected/all-actual confidence views and output ownership remain unchanged.
Qualification: **4,448 Release tests per OS** (1,635 internal, 1,881 public,
932 live FFmpeg), **47 all-intrinsics-off** and **47 AVX512F-off** Lanczos controls
per OS, same actual private 0.2.0 package consumer and all three examples on
Windows/Linux. Twelve font proofs reuse unchanged paths. Published 0.1.0 is immutable.

All four Lanczos3 rows refresh; other 227 rows retain historical scope. The earlier
U profile identifies the rationale for this change; its fractions do not describe
the changed coefficient loop. Parity/replacement readiness remain open. Next:
resolve current-code uncertainty for the existing CMYK sparse-2D JPEG control
before assuming its older 1.812× measurement needs further optimization.

[Numeric evidence](warm-lanczos3-coefficient-permutations-evidence.json) retains
samples, allocations, identities and bounds. Qualified source:
`a32e087d97d3a1b701033f160cce9d2468d0468afb6a0700179d693b3625953c`. Raw `l3coef-hoist-qualified-windows.tar.gz`:
`b83b54747a5f2dd5a456af2451372ef678cb42a7ac221e1f38bd5fdb7b1758eb`; `l3coef-hoist-qualified-linux.tar.gz`: `1d28bed10c132fb246168f9fc06f2f6398dc53d73db735e4f1b9aeb32bec6bac`.
Initial control `l3coef-first-linux.tar.gz`: `dbc634fe463a45191266c43ad351271666f8126787333504d34afbe91948203e`.
Unchanged dependencies, fixtures and worker remain in the baseline archive.
