# Prepared VP8 coefficient pipeline investigation

A coherent three-stage prototype prepares coefficient syntax while collecting
adaptive statistics, then emits it with final probabilities. One traversal serves
statistics and output. Operation-owned admitted pages replace the full coefficient
frame with block-local scratch. Further stages resolve a local probability table
once per emission and execute exact arithmetic in bounded 256-instruction chunks.
Skip policies for efforts 0–2, header retries, source/output ownership and complete
packets remain unchanged. No production dependency, unsafe code or pool is added.

The completed design does not earn retention. One independent comparison uses
twelve fixed interleaved blocks per subject/control, two complete compiled managed
calls per block, and the fastest of six adjacent native samples. All twelve ratios
enter the pinned 99.9% interval, without filtering or favorable resampling. Original
inputs/settings, exact packets, native checks and both quiet guards pass.

| Complete public encode | Before ratio / 99.9% interval | Candidate ratio / interval | Allocated B/op before → after | Complete packet B, both |
| --- | ---: | ---: | ---: | ---: |
| Photograph VP8 | 1.415 / 1.385–1.444 | 1.449 / 1.422–1.476 | 809,072 → 665,448 | 17,688 |
| VP8 with lossless alpha | 1.219 / 1.206–1.231 | 1.239 / 1.220–1.258 | 9,684,744 → 10,845,568 | 98,590 |

Neither raw nor normalized confidence separates. Both mean ratios are adverse;
this establishes no causal regression or CPU gain. Native clock drift is about
−0.97% / −1.21%. The photograph saves 143,624 allocated B/op; alpha adds
1,160,824 B/op. This allocation trade does not justify the design without a useful
elapsed-time gain. The earlier three-actual leads are retained as development
screens, not confirmation. No further sampling follows.

Measured complete-operation admitted peaks on the four-control direct screen are
unchanged: photograph/diagram **lossless** 3,470,059 / 3,386,043 B, transparent VP8
3,847,888 B and authored bars 312,716 B. Only transparent VP8 exercises the changed
pipeline in that screen. Total alpha allocation rises while another phase sets
the complete peak. Operation scratch retention is zero; physical/native heap
peaks and the opaque photograph’s governed peak are unmeasured.

The first stage passes 749 focused checks in normal/scalar modes on each OS.
Later affected emission stages pass 39 and 70 checks per mode/OS, reusing the
unchanged producer proof. These include frozen complete packets for efforts 0–6,
every signed magnitude through 2,047 with varied bands/contexts and both statistics
policies, page/retry ownership, admission refusal, cancellation and asynchronous
output lifetimes. Failed compile attempts, the corrected independent band oracle,
a pre-measurement missing selector and a post-measurement retrieval error remain
in the raw evidence. Completed measurements were not rerun for those setup fixes.

Qualified WSP152 source, core and actual package are restored byte-for-byte.
Prototype-only implementations/tests are removed; no forwarding layer remains.
The restored frozen packet/lifetime suite passes **26 normal/scalar checks per
OS**, without rebuilding. Reuse full/live, unaffected scalar, same actual package
and example qualification from [WSP152](WEBP-SCORING-PREPARATION.md). Current
comparison costs remain unchanged. This implementation’s outcome does not ban a
materially different approach.

[Evidence](webp-prepared-coefficient-pipeline-evidence.json) pins every stage,
producer, observation, confirmation and restoration. Both raw archives are
mirrored and member-verified:

- `webp-prepared-coefficient-pipeline.tar.xz`: 7,534,368 B / 333 members; SHA256
  `2eb33a37482b30b97f3d536cfe6e719afa538b058f31012d226486f48cfeb96a`.
- `webp-prepared-coefficient-restoration.tar.xz`: 4,876 B / 14 members; SHA256
  `a3d029be21cae8b058b3085228ec3f24254f33014a392d5ff42a4c5439091b46`.

Intermediate Windows main DLLs were overwritten by stale test-directory snapshot
copies. Their hashes, sources, compile logs and tested source-embedded Windows
test assemblies remain. All measured Linux DLLs and the final Windows DLL are
retained. No reconstruction build or timing replay was used. Unchanged harness
dependencies and original instruction attribution are pinned in the evidence.

Next reconcile current-code confirmations for the existing VP8L RGBA16
normalization and asynchronous nonseekable-output controls. Their large displayed
gaps are historical after the accepted workspace/scoring changes; establish
current measurements before assuming they need optimization.
