# VP8 intra16/Y2 compression milestone

Admitted mixed 4x4/16x16 luma prediction and grouped DC coding are **accepted as an
incremental compression gain**. The fixed diagram packet shrinks 7.8%, photograph
1.1%; both retain the unchanged matched-reconstruction-quality gate. Both complete
packet ceilings remain open. CPU parity is deferred while compression gates are open.

| Frozen opaque513×259 control | Previous → current / reference B | Source-PSNR gap dB | Complete managed / reference µs | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Photograph8 | 21,432 → 21,202 / 17,688 | 0.021969 | 11,184.03 / 9,709.76 | 601,419 |
| Diagram8 | 8,288 → 7,642 / 7,100 | 0.214030 | 6,279.50 / 5,829.60 | 487,843 |

Managed quality78/75, native75 and effort4 remain fixed. Equal numerical qualities
are not the matching rule. Acceptance retains source-PSNR absolute difference≤0.25dB,
exact alpha, native/public decoder bounds, source immutability and independent
outputs. Complete packet targets remain17,688/7,100 B. No other content, quality,
sampling or image-size claim follows. Lossless encoding and JPEG defaults are unchanged.

The [original C39 screen](WARM-VP8-INTRA16-SCREEN.md) rejected these byte-saving
choices under the earlier CPU ordering. Current integration preserves the accepted
safe SIMD transform and reproduces those packets exactly. Normal/scalar FFmpeg
reconstruction also agrees exactly for each current packet. At efforts4..6 and
q>0, a source-distortion/estimated-rate decision selects staged intra4 or intra16
reconstruction. Only one macroblock is staged. The signed Y2 high-byte cache and
row contexts are reserved before allocation through the same borrowed root;
constrained local budgets keep the existing branch before optional allocation.
Shared refusal is terminal, without retry or lower-memory fallback.

Both platforms pass **4,600 tests**, including932 live FFmpeg comparisons, plus
135 affected internal and2 public scalar checks per OS. Independent Walsh-matrix,
cache signed-range/AC replay and refusal/cancellation/ownership regressions cover
the new path. The same actual private0.2.0 package and all three examples pass on
Windows/Linux. One core/internal/public test build per OS; no producer rebuild
after freeze and no differential/reference/harness/native/selector build.

Two sequential three-actual **cost screens**, with idle-v2 guards and native pre/post
brackets, run on the dedicated EPYC9V45 VM at one pinned CPU, normal SIMD,
.NET10.0.8, tiering off and nonconcurrent workstation GC. Complete operations
include admission, source processing, fresh output growth and independent ownership;
input preparation, validation, IPC and startup are excluded on both sides. Native
allocations are unmeasured. No independent timing confirmation is requested or
implied. The retained initial catalogue-identity setup refusal occurs before
measurements and is unscored; one uniquely named preparation retry succeeds.

Exactly the two content VP8 encoding rows change. Other five existing VP8 encode
rows use Quality100/q0, preserving their prior path and evidence. VP8L, PNG, JPEG,
editing, inspection, font and frozen decoder proofs remain valid in their named
scope; their tests do not need to be repeated for another unchanged experiment.
[PNG packet acceptance](PNG-COMPLETE-PACKET-AUDIT.md) is reconciled separately.

[Numeric evidence](vp8-intra16-compression-evidence.json) retains all actuals,
quality checks, identities, allocation bytes and guards. Raw archives retained
on both hosts:

- `c53-qualified-windows.tar.gz`: `c8ab5de1e8433c40064381a54e118076f1bb381314fd994285b57851e35c2123`

- `c53-qualified-linux.tar.gz`: `937d1d87e258f4822eb4fa385a9fa61e84c432ef45d10b5542f92a7c568d3f28`

Actual private package SHA256: `a8f33947ba8dbf19a32045997258a1a0d682f9abbf2accc2cd5fd34eb402a0ec`.
