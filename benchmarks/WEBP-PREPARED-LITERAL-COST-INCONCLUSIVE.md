# Prepared WebP trace literal costs

The trace model leaves an entire newly zeroed price plane untouched when it has
at most one nonzero count. One implementation selects that fact once per trace,
skipping red/blue lookups through a shared generic DP loop. This is a property of
immutable prices, independent of input pixels. Cache transitions, integer costs,
strict ties, path/token order, cancellation and exact packets stay unchanged.
No array, frame scan, cache, admission or owned capacity is added.

| Complete existing control, three direct batches | Original µs | Candidate µs | Managed B/op, both |
| --- | ---: | ---: | ---: |
| Lossless photograph | 82,114.88 | 83,803.17 | 6,516,744 |
| Lossless diagram | 7,242.14 | 7,422.63 | 4,362,808 |
| Transparent VP8 + lossless ALPH | 61,696.17 | 62,586.33 | 7,165,064 |
| Authored bars | 772.64 | 766.11 | 896,424 |

These quiet-VM complete-operation screens establish no statistical regression
or useful material gain. The small bars lead does not offset adverse material
control points. All original inputs/settings, exact complete packets, source/
output ownership, cleanup and identical admitted peaks pass. No reference timing
or physical peak is measured. Restore without independent confirmation, favorable
sampling or a permanent exclusion of revised cost-model designs.

Linux passes 394 internal/195 public checks per normal/scalar mode on one main
build, reusing every test/harness/native/selector. Windows retains a source-only
candidate. Originals restore exactly without rebuilding; 40 restored trace checks
pass per Linux mode. Reuse unchanged WSP312's 5,656 full/live checks per OS and the
SAME actual Release-package/all-three-example proof. No current row, broad-suite
claim or package changes. A missing post-check download name is corrected by
retrieving completed logs; no work repeats. Publication remains postponed.

[Evidence](webp-prepared-literal-cost-inconclusive-evidence.json) pins models,
sources/modules, all batches, gates and restoration. Raw
`webp-prepared-literal-cost-inconclusive.tar.xz`: 1,683,088 B / 306 members,
SHA256 `31e6ccbb2ce4582e8f218b64ad970ea2f8334e6caf08d6ebc0717a87c017c3d6`,
mirrored/member-verified. Retained `wcm314-*.py` and its pinned WSP312 dependency
provide reproduction. The full 282-path objective stays open.
