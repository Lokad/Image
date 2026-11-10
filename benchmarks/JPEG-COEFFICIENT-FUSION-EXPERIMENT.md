# Baseline JPEG coefficient fusion study

JCF728/JCR729 are unselected. Both preserve the tested exact pixels and reader
state, but the completed stages establish no useful complete-operation gain.
Qualified sources, main/embedded modules, consumer bindings and the Linux source
manifest are restored byte for byte. The prototype helper and dedicated tests
are removed. Current costs, package and qualification remain unchanged.

| Three-actual complete decode screen | Lokad µs | FFmpeg µs | Allocated B/op |
| --- | ---: | ---: | ---: |
| Sparse CMYK, qualified original | 525.91 | 477.17 | 20,944 |
| A: dequantized destination passed by value | 564.72 | 472.10 | 20,944 |
| B: separate scoped-span call boundary | 517.12 | 478.24 | 20,944 |
| Direct RGB444, qualified original | 1,643.62 | 1,567.51 | 42,936 |
| Direct RGB444, B | 1,661.25 | 1,564.56 | 42,936 |

A has an adverse 38.81µs point. Generated code copies its 32-byte destination
descriptor through the reader call. B removes that struct ABI by passing ordinary
scoped spans into the same grammar. Its sparse lead is only8.79µs/1.67% raw,
while RGB has an adverse17.62µs/1.07% point. Broad short intervals overlap;
neither causal gain, regression nor equivalence is established. No independent
confirmation, broad suite or package repeat follows. The added generic grammar
and code-generation complexity is not justified by this completed-stage evidence.
This rejects these implementations, not every possible fusion design.

The [preceding model](JPEG-COEFFICIENT-PIPELINE-ATTRIBUTION.md) correctly identifies
repeated data work, but does not predict its elapsed cost. Both stages commit
dequantized shorts at validated baseline token stores instead of clearing natural
Int32s and performing a dense SIMD conversion. DC keeps its full product, bias
and saturation; AC keeps word wrapping. The original numerical inverse body is
source-identical. Real persistent/validation Int32 paths share the grammar;
progressive/refinement storage remains unchanged. No numerical policy, encoder,
metadata, output publication or ownership boundary changes.

Both stages pass456 internal/375 public checks per normal/scalar mode on
Windows/Linux. New tests cover full word/quantizer and wide DC boundaries plus
ordinary/live partial commits, truncations, failures, cursor and following reads.
Existing natural-mode pairs, long codes, refusal/cancellation and public ownership
checks remain. The frozen22,708B CMYK and133,057B RGB packets and their full
independently owned531,468B output frames retain all original native/pixel gates.

There are two successful main/embedded versions per OS and zero harness, native,
selector or fixture builds. Three Windows internal compile attempts produce no
executable: a missing generic declaration and two readonly-reference lifetime
attempts. B retains safe ordinary scoped spans; no borrowing restriction is relaxed.

One completed RGB-original screen fails its **after** idle guard (aggregate busy
.103, CPU0 .046, no overlapping jobs). Its actuals and native validation are
retained but unscored. One immediate guarded original-only recheck uses unchanged
inputs/settings/cadence; no candidate favorable repeat or load-average cooldown.
All statistical work is sequential, CPU0 only, Release on the quiet VM. Complete
parsing, entropy, inverse, normalization, independent output and disposal stay
timed; FFmpeg retains its documented contexts. JIT inspection elapsed is discarded.

Coefficient clearing falls256→128B per block and the conversion pass disappears,
but actual physical/admitted stack peaks are unproven after changed frames and
descriptors. There is no new owned capacity or retained state; total GC is
unchanged. Result lifetime, reservations, shared root, refusal and cleanup remain.
Exact restoration reuses6,133 full/live checks per OS, scalar/ISA and the same
actual Release package/allthree examples. No redundant qualification follows.

[Evidence](jpeg-coefficient-fusion-experiment-evidence.json) retains stages,
samples, source/module identities, guards, tests and restoration. Mirrored,
member-verified raw `tmp/jpeg-coefficient-fusion-unselected.tar.xz`:
7,883,924B /247 members, SHA256
`bf88043ad3ca57cf2f43ef042b2a3845ed3fb73a0c235b63d7d1f7c188a91df8`.
It contains original/candidate source and modules, logs and reproduction scripts;
qualified dependency archives retain unchanged runtime/reference inputs. This
study supplies no displayed cost and is not a renderer dependency.

Next reconcile the larger tightly bounded, wider-support resizing controls and
their actual current branches before another prototype. No new shape grid;
passing cases stop, publication stays postponed and all283 paths remain required.
