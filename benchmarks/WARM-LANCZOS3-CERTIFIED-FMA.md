# Certified Lanczos3 FMA accumulation

The four existing complete Lanczos3 operations improve **13.6%–23.8%**
and allocate less, with every original managed output exact. All remain measured
parity gaps; no independent long confirmation is run above the target.

| Complete operation | Lokad µs | Reference µs | Ratio / conservative upper | Lokad allocated B/op | Time saved |
| --- | ---: | ---: | ---: | ---: | ---: |
| Reduce8 | 758.75 | 490.07 | 1.548 / 1.646 | 138,466 | 15.2% |
| Reduce16 | 804.22 | 506.28 | 1.588 / 1.759 | 138,454 | 13.6% |
| Enlarge8 | 4,324.39 | 4,076.28 | 1.061 / 1.225 | 319,046 | 23.8% |
| Enlarge16 | 5,248.30 | 3,935.96 | 1.333 / 1.459 | 4,584,270 | 21.2% |

The selected kernel retains exact Q30 horizontal integer filtering, converts each
cached row in place, and uses SIMD FMA for vertical accumulation. A strict error
interval certifies all four rounded channels. Uncertain pixels use the original
Int64/Int128 computation. The [certificate](../docs/lanczos3-fma-certificate.md)
is included in the package. There is no managed pixel tolerance.

One admitted backing array stores each row successively as Int64 and double;
vertical sums use eight bytes per channel instead of sixteen. Substantial storage
is reserved before allocation and released promptly. Returned images retain their
independent owners. Conservative work includes every possible exact recomputation;
tight local limits or missing hardware select the original kernel before admission.
Shared refusal, cancellation and allocation failure terminate and clean up.

Two full-FMA horizontal trials were rejected: Reduce16 took 1,643.57 and 1,075.97µs
versus the prior qualified 931.20µs. A completed one-second diagnostic of the first
placed 76.9% of 967 samples in horizontal filtering. This motivated retaining the
integer horizontal kernel. These trials receive focused controls, not full/package
qualification; their original sources, actual binaries and samples are archived.

Reference: **zimg, FFmpeg's zscale backend**, with the equivalent complete operation
from the [baseline](WARM-LANCZOS3-BASELINE.md), including packing, normalization,
clamped support and fresh owned output. All eight idle-v2 guards pass. Each short
retains all three actuals and selected/all-actual conservative bounds. Fixtures,
native worker/quality proofs, harness and selectors are reused; no grid expansion.

Qualification: **4,457 Release tests per OS** (1,644 internal, 1,881 public,
932 live FFmpeg), **55 all-intrinsics-off** and **55 AVX512F-off** Lanczos checks
per OS, four exact original frames on both OS, the same actual private 0.2.0 package
consumer and all three examples on Windows/Linux. The extra selection test and
documents leave the scored production DLLs unchanged. Twelve text comparisons
reuse unchanged paths. Published 0.1.0 stays immutable.

All four Lanczos3 rows refresh. Other 233 numeric rows retain their identified
build scope; 226 historical codec/edit rows still require current-admission
qualification. Previous sample fractions describe their original producers only.
The current CMYK sparse-2D decode at 1.829× is now the largest demonstrated gap.
The finite checklist and full parity/replacement objective remain open.

[Numeric evidence](warm-lanczos3-certified-fma-evidence.json) retains samples,
allocations, identities, rejected trials and bounds. Qualified source:
`057785070034925d107c29ce36ae08e0e6a074b9de62af1ed509744fb4f8d32f`. Raw `c4c-qualified-windows.tar.gz`:
`a5353ce851ad8ec901c31ffc144e80b6382ffb9f6af49ef6949286cb9aaefcce`; `c4c-qualified-linux.tar.gz`:
`8f6321dbbc1ac2c1f97e94b51745db990568c743523f543d09f47572a8f123f4`. Unchanged dependencies, fixtures and worker are retained
in their preceding archives. All selected and rejected archives are mirrored.
Final SPEC synchronization repacks the identical production DLL; the same actual
package again passes both consumers and all examples, reusing full/hardware tests.
Document-only raw: `c4c-final-docs-windows.tar.gz`
(`e11f9c7ddf009208e05596d56eaf36d48325233eb8600b27cc24c65033ed35b6`)
and `c4c-final-docs-linux.tar.gz`
(`550c08fbc084fa3104604565a083abb010d831eeb427f2f05ff2c189415a33c7`).
