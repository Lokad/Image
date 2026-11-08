# Paired JPEG component preparation study

Three coherent preparation/execution stages preserve exact pixels but establish
no useful complete-decode gain. Qualified production, embedded tests and public
bindings are restored byte for byte. The paired implementation and its dedicated
tests are removed. The existing actual package and current costs remain unchanged.

| Three-actual live progressive444 screen | Managed µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Qualified original | 3554.56 | 3353.28 | 1.060 / 1.126 | 169,664 |
| A: scalar fusion + paired inverse | 3662.30 | 3351.82 | 1.093 / 1.157 | 169,664 |
| B: prepared scales + SIMD dequantization | 3668.15 | 3353.17 | 1.094 / 1.135 | 169,664 |
| C: vector layout conversion + paired inverse | 3546.26 | 3354.35 | 1.057 / 1.125 | 169,664 |

A reuses the two chroma components' admitted 256 B Approximation buffers for
short input/row storage and executes both inverse matrices in 512-bit halves.
Its fused conversion trades the natural Int32 copy/SIMD dequantizer for scalar
products. B retains SIMD dequantization and prepares zigzag scales once in retired
luma scratch, reusing row storage for transient dequantized coefficients.
C removes those plans and scalar scatter loops with format-derived vector
permutations, narrowing and natural quantizers; four fixed index vectors add
256 B of immutable format state. Every per-image preparation remains timed.
These are staged architectural revisions, not unchanged favorable resampling.

A/B have adverse point results. C is only 0.23% lower than the contemporary
original, with broad short confidence. This establishes neither a useful gain
nor statistical regression/equivalence. There is no independent confirmation,
ordinary444 expansion, broad suite or new package run after these screens.
The outcome closes these implementations, not all revised inverse-transform work.

The actual frozen component grid is 65×34, including padded blocks: 2,210 blocks
per component and 2,210 chroma pairs. Of these, 2,145 pairs have two non-DC blocks,
65 have two DC blocks and none are mixed. Paired AC rows cover 97.19% of 32,348
row transforms. Those applicability counts did not predict elapsed savings.
DC/two-low-row shortcuts, original overflow, full-width DC saturation, AC wrapping,
signed-word bias, clipping, cancellation and independent ownership remain exact.

Each stage passes 437 internal / 375 public checks per normal/scalar mode on both
systems, including basis/dense/extreme, independent shortcuts, destination guards
and public admission/async coverage. One initial cancellation test canceled
before constructing OperationContext; its setup was corrected and only embedded
tests rebuilt. There was no production numerical failure. Three production and
four embedded-test versions per OS, zero new worker/native/fixture builds or
profiles; unchanged external assemblies were rebound.

The exact same original 132,961 B progressive444 packet produces the exact
531,468 B independent frame on every stage. Quiet idle-v2 guards pass, CPU0 only,
Release, tiering off and nonconcurrent workstation GC. Complete public parsing,
preparation, live-token entropy, inverse, conversion, output ownership and disposal
stay timed. FFmpeg retains its existing codec/input/conversion contexts; its
cancellation boundary remains asymmetric. Original/reference settings are frozen.
This is a complete-operation comparison, not an inverse-engine microbenchmark.

No new owned capacity or retained operation scratch; allocation stays 169,664 B/op.
Physical/native peaks are unmeasured. C's 256 B of format vectors is removed with
the experiment. Exact restoration returns Windows69e4a0bc/Linux6d0398a1 and
packageb43fbef4; the prior 5,964 full/live checks per OS, scalar/ISA and SAME actual
package/all-three-example qualification remain valid. No redundant rebuild,
package proof, all-path parity or release-performance claim follows restoration.

[Compact evidence](jpeg-paired-component-preparation-evidence.json) includes
the restoration records. Raw jpeg-paired-component-preparation-unselected.tar.xz,
SHA25697d744e96cccef6f09c8a5fc2c7c932fac3de3affe8105c0df17351c72c78ec0,
16,484,972 B / 351 members, is mirrored and member-verified. It captures all three
stages before restoration: original/candidate sources/modules, logs, guards,
receipts, manifests and scripts. Dependency: qualified MBQ635 rawc376dfbd and
JPA638 applicability raw6397e253 are retained separately.

Reproduce with the raw JPC639/JPS640/JVP641 prepare/build/focused/measure scripts
on commit4b398c90. JPC639's corrected-focus script records the test-only repair;
each stage's measure tools select the intended Windows-produced artifact family.
The retained restore script verifies preservation before replacing all original
sources/modules, removing the new pair files and rebinding unchanged consumers.
Rejected-stage data is not a benchmark-renderer dependency. Continue independent
tight-byte resizing work; compression correctness remains ahead of CPU gains.
