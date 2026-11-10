# Shared certified byte publication

CRR755 shares ordinary-alpha byte publication between bounded narrow/wide bicubic
resizing. Both reuse nearest-even rounding for the uncertainty check and output;
the wide path keeps its dynamic division certificate for small alpha or uncertainty.
The old private narrow publisher and reflective delegate are removed;15 existing
behavioral tests directly call the actual shared publisher. No compatibility wrapper.

| Complete513×259 public resize,4KiB scratch / labeled zimg | Managed µs | Native µs | Ratio / upper | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| TightDown8 | 504.61 | 417.18 | 1.210 / 1.216 | 113 |
| TightWide8 | 419.13 | 349.29 | 1.200 / 1.209 | 113 |
| TightDown16 | 560.37 | 572.84 | 0.978 / 0.985 | 113 |
| TightWide16 | 430.50 | 451.13 | 0.954 / 0.968 | 113 |

One contemporary original wide-byte confirmation measures427.09/native347.58µs.
The independent419.13/native349.29µs result saves **7.96µs/1.86% raw/2.34%
normalized**; filtered/all-actual confidence separates, native+0.492%. Each run
retains all12actuals. The407.67µs screen remains a lead rather than the gain claim.
Narrow byte remains a gap; its point movement establishes no causal improvement,
regression or equivalence against older clock contexts. Both word controls pass
and stop. This is four named controls, not completion of283 required paths.

The final C type guard excludes byte-publication locals during word JIT import.
Three byte execution bodies match measured B exactly after process-address and
stack-cookie normalization, including registers, branches and stack offsets.
Their12-actual evidence is reused with producer906e87b5; these are source-valid
costs, not direct timings of the final package DLL. Changed word bodies are
measured on final1a7c5a22. Prototype B's wide-word upper1.003 contradicted its
prior pass: one original countercheck and code capture found an enlarged frame
520→536B. The explicit type guard restores520B before acceptance. Final word
reference clocks moved, especially415.58→451.13µs on wide word; no causal word
CPU gain, equivalence or nonregression is claimed. No favorable unchanged repeat.

Clipping before rounding makes saturated endpoints unambiguous. With margins
below.25, `.5-abs(clipped-round(clipped))` preserves the old half-integer test;
near its decision boundary Sterbenz subtraction is exact. The same rounded value
then supplies output samples. Independent scalar13,915 boundary checks and the
actual SIMD helper preserve decisions and nearest-even samples, including clips,
half-neighbors and per-channel margins, with no allocation.

The shared reciprocal certificate retains alpha sum≥16 and normalizer≥.5 guards.
Using the existing wider400/2^40 accumulation bound, reciprocal/Newton error<2^-27
and critical quotient bound257 gives RGB error<7.852e-6 and alpha error<2.660e-6,
strictly below the unchanged2^-16 margin. Saturated endpoints remain stable under
the affine quotient-error bound. Tiny-alpha/uncertain wide cases keep the original
division/margin fallback. Coefficients, ordered sums, exact repair and managed
pixel policy remain unchanged; all original native pixel bounds pass.

Complete managed source reads, geometry/preparation, working storage, independent
output and disposal remain timed. Native zimg3.0.6 FFmpeg zscale retains its
established graph/input/scratch and halo/conversion/filter/packing boundary.
No fresh-native-graph comparison is claimed. GC113B/op,4KiB admission, narrow
3232/4080B and wide4068/3776B explicit payloads remain. Scratchretention0 and
independent32768/65536/132096/264192B outputs persist through disposal. Existing
pool retention is separate; physical/native/JIT-stack peaks are unmeasured.
No new array, owner/root, cache/session, dependency or numerical-policy relaxation.

Final **6157full/live checks per OS**,331internal/729public resize checks per
normal/scalar/AVX2-off/AVX512-off mode, and SAME actual Release package/allthree
examples pass Windows/Linux. Hardware kernels run where supported; portable
fallbacks retain qualification. The consumer adds the existing bounded byte control
to its frozen-pixel/aggregate/retained/refusal/cancellation/recovery loop.
Unchanged cached/packed/Lanczos/codec/text bodies reuse valid evidence. Compiler
name collision and15 obsolete-reflection failures are retained; test-only repair
rebuilt the embedded test assembly without rebuilding the immutable core. Prototype
B Windows qualification remains historical; final C is qualified on both OSes.

[Evidence](bicubic-shared-byte-publication-evidence.json) owns costs, exact bounds,
source/code identities, transition and qualification. Actual private0.2.1 package
SHA256 `ce0a5df2a1c489e15f524974f4022419cebdef7e8ac42174153efa0ca204379a` contains DLL `1a7c5a22318c1260f7c682e44f02f09b7b5e9087b6c0ab149b8bcd569cc82370`.
Mirrored/member-verified raw `tmp/bicubic-shared-byte-publication.tar.xz`, SHA256
`6a8646614d09f395b80ff740adce836fed1b2f5315ad38a02a0c659363421224`,13,258,500bytes/418members, preserves all phases,
failures and platform-specific raw variants. [Wide overlap](BICUBIC-WIDE-OVERLAP.md)
retains earlier gains; [vertical traversal](COLUMN-VERTICAL-QUAD-STUDY.md) retains
its discarded implementations. This is no127-row release refresh. Published
packages remain immutable and publication is postponed.
