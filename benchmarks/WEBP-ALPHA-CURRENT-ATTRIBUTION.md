# Current transparent WebP attribution

One two-second capture reuses the existing 513×259 transparent Q75/effort4 control,
worker, harness and selector with the qualified current DLL. Both quiet guards,
frozen settings/source/output ownership and exact 98,590-byte packet pass. No
build or production change; profiled elapsed is discarded.

| Exclusive location | Samples, out of 1,944 |
| --- | ---: |
| VP8L matcher | 285 |
| VP8 coefficient coding | 126 |
| Green predictor histogram | 91 |
| VP8 intra4 decisions | 82 |
| Cache history | 75 |
| VP8 token statistics | 67 |
| Prepared population sums | 65 |

The matcher JIT body is 2,819 B. Hot instructions include candidate-chain access,
the best-length pruning probe and inlined scalar prefix comparisons. This broadly
overlaps the retained [matching preparation](WEBP-MATCHING-PREPARATION-INCONCLUSIVE.md),
[prefix](WEBP-MATCH-PREFIX-INCONCLUSIVE.md) and
[compact matching](WEBP-COMPACT-MATCHING-INCONCLUSIVE.md) investigations. Counts,
instruction skid and code size are attribution, not method times or predicted
savings. They do not justify replaying those unchanged implementations.

No candidate is accepted or family closed. A materially different repeated-work
or workspace design remains eligible with concrete evidence. Meanwhile, reconcile
the already named uncertain Lanczos enlargement one-shot/prepared comparisons.
All current costs and the selected build's 5,543 full/live checks per OS and
same actual package/three-example qualification remain valid; no broad repeat.

[Evidence](webp-current-alpha-execution-attribution-evidence.json) pins selected
samples, exact-source JIT, producers, output gates and prior investigations. Raw
`webp-current-alpha-execution-attribution.tar.xz`: 261,412 B / 22 members, SHA256
`a9ce200e792628392960adcd4190c65f1aa4cf9709b7c448f4a17fe8cb0e4e89`,
mirrored/member-verified. The trace, map, complete analysis, JIT, guard logs,
current owned sources and reproduction scripts are retained, with qualified-core
and existing reference/runtime archives pinned. Publication remains postponed.
