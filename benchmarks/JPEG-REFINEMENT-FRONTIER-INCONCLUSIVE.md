# JPEG refinement frontier investigation

Neither completed stage establishes a useful complete-decode gain. Production
and the actual package are restored exactly; five actual-reader behavioral
controls remain. No independent confirmation or favorable resampling follows.

| Quiet one-CPU screen, three actuals | Managed µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| 233 baseline, Live444 | 4,676.88 | 3,359.61 | 1.392 / 1.474 | 984,212 |
| 233 candidate, Live444 | 4,671.97 | 3,355.66 | 1.392 / 1.535 | 984,212 |
| 233 baseline, Ordinary444 | 4,411.63 | 3,356.38 | 1.314 / 1.378 | 984,212 |
| 233 candidate, Ordinary444 | 4,369.63 | 3,357.71 | 1.301 / 1.384 | 984,212 |
| 234 candidate, Live444 | 4,729.13 | 3,358.00 | 1.408 / 1.527 | 984,212 |
| 234 candidate, Ordinary444 | 4,380.22 | 3,354.66 | 1.306 / 1.444 | 984,212 |

Stage233 uses the PDEP target to validate a zero run, removing redundant prefix
counting/subtraction. Stage234 continues the design by locating a coefficient
only for a set correction bit, with no byref across slow reader calls. The first
stage is flat (0.105%raw, no normalized lead); the completed stage is1.12%raw/
1.17%normalized adverse by point. Wide intervals establish no causal regression.
The ordinary444 control is unchanged. Stage234 reuses the original before,
not the intermediate implementation. All actuals/brackets remain in the evidence.

Both use the existing132,961B progressive444 packet and complete public Decode
to fresh independent531,468B RGBA8, including admission, reconstruction/conversion,
output allocation and disposal. FFmpeg retains decoder/frame/packet wrappers;
complete software decoding/conversion/output release stays timed. Live-token
creation/disposal is outside timing; native has no cancellation setting. Frozen
managed pixels, approved native bounds, source/output ownership and both quiet
guards pass. Expected-frame metadata is restored in finally; no new image/grid.

The2,097,152case16-bit population/run proof plus12,800shifted/clipped cases retains
target/prefix/correction/failure identities. Current JPEG sources exactly match
[JIP174 attribution](JPEG-REFINEMENT-STATE-ATTRIBUTION.md#current-instruction-attribution),
so no new profile is taken. The completed method's setup JIT listing is2,535B
versus the earlier2,662B; smaller generated code is not a CPU-gain claim. Every
code/field/correction-bit checkpoint, immediate update, overflow/error precedence
and logical cursor remain. No storage, owner, cache or atomic bit group is added.

Each stage passes369internal/331public checks in normal/scalar modes on bothOSes.
An invalid all-ones Huffman code in the new test is corrected to canonical00/01;
the successful core is reused. After exact production restoration,42refinement/
cancellation checks pass perOS/mode. Five retained theory cases add3000independent
bitwise-oracle comparisons across run/insertion, clipped bands, dense/sparse zeros,
stuffing/truncation, overflow and the surviving logical tail. Source/core/PDB/XML
and actual package identities match the qualified baseline. Reuse5,549full/live
checks and SAME actual package/allthree examples; no broad/package repeat.
No new admitted/physical peak or native allocation claim. Retained scratch stays0.

[Evidence](jpeg-refinement-frontier-inconclusive-evidence.json) retains both
stages, original-baseline comparison, proof, failures, codegen and restoration.
Mirrored/member-verified `jpeg-refinement-frontier-inconclusive.tar.xz`:
3,098,656B /242members, SHA256
`a03d18cb828cb7660d81ef65454b03995610be57b0dc85b05484aafb201006b8`.
Its manifest pins qualified-package, original input/reference/runtime and
instruction archives. No family is closed; publication remains postponed.
