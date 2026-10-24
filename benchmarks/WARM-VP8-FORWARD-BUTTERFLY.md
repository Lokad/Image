# Exact VP8 forward transform factorization

The existing513×259 RGBA8 control retains frozen Q75/method4/reference settings
and the complete98,590-byte packet, including exact source alpha.

| Quiet one-CPU run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|---:|
| short | 202,222.15 | 53,618.19 | 3.772/4.329 | 24,093,944 |
| confirm | 203,322.25 | 53,732.55 | 3.784/3.808 | 24,093,944 |

Independent confirmation saves17.2% against the
[previous245,441.85-µs screen](WARM-VP8L-ENTROPY-CONSTANTS.md), with unchanged
allocations. Both versioned idle guards pass. Complete operations include source
normalization, alpha, encoding, fresh owned output and release. CPU parity remains
open; claims cover this fixed control and settings.

Four-point butterflies replace general integer matrix projections, preserving
all signed rounding and the odd-column adjustment. The64-byte scratch, overlap,
validation, work/admission/cancellation and ownership contracts stay unchanged.
No upstream code/table, heap storage, cache or new dispatch mode is introduced.

Both platforms pass5,058 checks each, including947 live comparisons,42 affected
scalar transform checks and29 scalar reference-packet/public-wrapper checks.
The frozen domain contains4,841,216 exact coefficients;36 native fragments and
13,464 opaque source blocks remain exact. New partial-overlap controls pass.
The same actual private0.2.0 package and all three usage examples pass on both.
The verifier now retires completed generated build outputs as it proceeds,
retaining package/source/input evidence. Immutable timed workers are reused.

[Evidence](warm-vp8-forward-butterfly-evidence.json) records samples, identities,
qualification and unchanged-path reuse. The larger remaining encoder gap is
VP8L; its relevant current confirmation is retained.
