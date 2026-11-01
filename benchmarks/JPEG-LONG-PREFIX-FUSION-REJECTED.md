# JPEG long-prefix fusion: rejected

The [current independent confirmation](JPEG-LONG-PREFIX-CURRENT-CONFIRMATION.md)
also rejects this exact candidate: required raw/normalized confidence overlaps.
The earlier short and original threshold decision below remain historical.


The existing DirectRgb444 screen observes **3.26% less elapsed time** (3.32% all
actuals, **3.49% native-normalized**), below the 4.1% continuation threshold.
Reject without an independent confirmation or variants. These short point means
do not establish a causal gain or change accepted production costs.

| Complete original 513×259 decode | Lokad µs | Ratio / upper | Managed B/op |
|---|---:|---:|---:|
| Reused original, independent12 | 2,098.407 | 1.449 / 1.458 | 966,367 |
| Candidate, short3 | 2,030.097 | 1.399 / 1.514 | 966,370 |

The trial borrows the table's existing 9/10-bit canonical lookup inside the fused
baseline AC loop. An 8-bit miss peeks buffered bits and requires the full code and
amplitude before consumption, preserving the original 18-bit refill and logical
cursor. No new read, storage, owner, charge or polling boundary. The frozen input
has 2,427 such symbols out of 226,740 AC entries; counts identify exposure, not CPU
cost. Native drift is +0.24%; the 3 B/op difference is GC reporting granularity.

Candidate **65 internal/388 public** checks pass normal/scalar Linux, including
9/10-bit full-baseline scalar comparisons of phases, partial coefficients,
predictors, cursors, markers, truncation and invalid categories/runs. Exact current
native pixels, source/output ownership, frozen settings and idle-v2 gates pass.
One Linux core/internal build; zero public/harness/native/selector builds or new
performance inputs. Actual operation includes fresh output acquisition and release.

Source, core/PDB/XML and linked test assemblies are restored **byte-exactly**,
without rebuilding. Two trial-only controls leave with the implementation;
restored **63 internal/388 public** checks pass normal/scalar. Windows and actual
package are unchanged. Applicable **5,354 checks/OS**, 948 live comparisons, the
same actual Release package/three examples and green CI remain valid; no full or
package repeat. The full 268-path warm parity objective remains open.

[Evidence](jpeg-long-prefix-fusion-rejected-evidence.json) links the reused original
and retains the decision, scope and restoration. Mirrored/member-verified
jpeg-long-prefix-fusion-rejected.tar.xz, 3,296,500 B/91 members,
SHA256 d3cc1fb7e0b61c46b694f8442ec8079e816cfc81aa0877706377aac08a77de9f, preserves all actuals, guards, original/candidate source,
DLLs, tests, fixtures and cached workers/selectors. Close this prefix-fusion family.
