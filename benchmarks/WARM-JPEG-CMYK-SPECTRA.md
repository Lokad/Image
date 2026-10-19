# Spectrum-aware CMYK materialization

Complete CMYK decode is **47.0% faster for the DC control** and **30.0% faster
for the one-AC control**. The general-transform control has a **3.1% higher
measured mean**. All three remain warm single-CPU parity gaps.

| Complete owned CMYK decode | Managed / FFmpeg µs | Ratio / upper | Managed allocated B/op | Input JPEG / RGBA bytes |
| --- | ---: | ---: | ---: | ---: |
| DC blocks | 539.07 / 379.20 | 1.422 / 1.511 | 1,635,145 | 10,910 / 531,468 |
| One AC per block | 747.62 / 397.33 | 1.882 / 1.979 | 1,635,146 | 17,346 / 531,468 |
| All63 AC per block | 3,478.40 / 3,112.37 | 1.118 / 1.244 | 1,634,921 | 347,676 / 531,468 |

The one production primitive is owned full-resolution CMYK component
materialization. DC-only blocks use the exact frozen C(0) products and original
rounding, rather than nominal DC/8; for example DC740 produces220, not221.
Blocks with at most eight active frequencies evaluate only nonzero terms in
original v/u order, with separate binary64 multiplies/adds, exact final rounding
and clipping. Zero terms can be omitted without changing finite outputs. Safe
AVX-512 handles independent output lanes; scalar execution uses the same order.
More populated blocks retain the original general IDCT. Every accepted block
still charges4,096logical work units before writes. Component allocation, row
copies, coefficient return/liveness, output ownership and cancellation order
remain. No new heap workspace, input copy, coefficient table or retained field.
The common transform and original sample materializer bodies stay unchanged.

The existing10,910-byte DC packet and complete native boundary are retained.
Two named controls were necessary because the new DC rejection/sparse path and
retained general transform are genuinely distinct required algorithms. They use
the SAME original513×259 CMYK projection, unit quantization and Adobe transform0,
adding coefficient4 at the first AC or all63 AC frequencies. This closes that
specific coverage omission without a shape/depth/filter/quality grid. The finite
checklist grows243→245 named paths, with24still unmeasured. Do not add more
spectrum variants without a newly demonstrated omitted algorithm.

DC/one-AC pixels are analytical. General-control expected CMYK is frozen from
the unchanged direct caller-sample/general-IDCT path and independently reproduced
by the qualified predecessor; the device-RGB projection is exact. The general
receipt's explicit qualified-reference flag distinguishes it from analytical
controls; a generic cleanup sentence was corrected after timing. All pixels,
source stability, two surviving independent outputs, opaque alpha/K semantics,
normal native CLI equality and original normal/scalar maximum4/RMSE1.5 pass.
General native maximum2/RMSE0.76901 remains within that original gate. Native
still decodes all four components, applies black/color conversion, allocates and
packs owned RGBA and releases planes/output inside every complete operation.
ICC conversion stays off and EXIF reorientation opt-in. No baseline adjustment.

An immutable DC-only prototype first improved DC46% but showed a4.5% higher
one-AC mean. The accepted sparse extension improves DC47% and one-AC30% against
the predecessor; that prototype is superseded. General before/current means are
3,374.41/3,478.40µs (+3.1%); no statistical equivalence or general improvement is
claimed. Every one of21actuals across the bounded experiments, both idle-v2
guards and pre/post50ms native brackets is retained. Above-one means require no
long confirmation. Two immutable production candidates per OS, two narrow
control-harness builds, original native/compiled selectors reused. Only the final
accepted production gets full qualification; no extra image/profiling sweep.

Accepted qualification:4,193tests/OS (1,428internal/1,837public/928live FFmpeg),
60affected hardware-disabled checks/OS, and the SAME actual private Windows
package consumer/all three examples on both OS. Unit controls cover quantizers,
sample halves/extremes, every frequency, signed sparse mixtures, dense rejection,
malformed spans, cancellation/work charges and destination guards. Public tests
cover DC/sparse/general output, Adobe conventions, progressive DC and surviving
owners. Package SHA
`18bef84b345a7b854c94961e98c9640814825c17f1b47da8edecdfaf6bf55325`.

The released0.1.0 package/tag remain immutable. Exactly one existing timing row is
replaced and two required controls are added. All70existing passing proofs remain
valid/stopped. Current221numeric rows contain70passes/150gaps/one confidence-open;
the full245-case objective remains open. Next the known fourth coefficient-buffer
allocation is a bounded opportunity, with no new benchmark cases needed.

[Numeric evidence](warm-jpeg-cmyk-spectra-evidence.json) retains prototypes,
predecessor/current source mappings, gates, allocations, timings and both-OS
qualification. All raw archives are on both hosts:

- `h10j-measurements.tar.gz` SHA`16929a508c870681df96dbfe2352f87203e8a0a4fe99059aa958598c652a8395`.

- `h10j-qualified-windows.tar.gz` SHA`7a7d0c97e70115cfa69860230749ff23a7155206cca75ef4a03a5957a2114e87`.

- `h10j-qualified-linux.tar.gz` SHA`0f2eb19ab1bb632b7e531a25c829452cbf7240016407c90e03459d0c7286e180`.
