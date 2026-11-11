# Bounded wide-row overlap trial

**Inconclusive; no production change retained.** A promising short screen did
not survive independent confirmation with a contemporary immutable predecessor.
The complete 513×259→128×64 RGBA8 control still has a demonstrated gap.

| Complete Resize, 12 actual iterations | Lokad / zimg µs | Ratio / conservative upper | GC B/op |
| --- | ---: | ---: | ---: |
| Qualified predecessor | 398.03 / 376.05 | 1.058 / 1.078 | 113 |
| Attempted row-loan activation | 392.23 / 372.22 | 1.054 / 1.068 | 113 |

Raw improvement is 1.46%; reference-normalized improvement is
0.44%, with native -1.02%. Filtered, all-actual and normalized confidence
intervals overlap. Retain all actuals and both screens; no causal gain or adverse
effect is established. Do not repeat unchanged sampling for a favorable result.

Preparation already proved contiguous 16-tap/+4 pairs for 17-slot records, but
the 16-column byte row-loan executor ignored them. The attempted one-guard change
qualified by tap capacity instead of column count, reusing the existing exact
20-pixel pair kernel rather than 32 logical pixel loads. The 20-slot three-column
mode remained excluded because its slot16 is live data, not a prepared proof.
Geometry, per-lane ordered Add(Multiply), repair, source/tail loans, cancellation
and ownership stayed verbatim; no buffer, helper, owner, pool or capacity was added.
The canonical model counts 10,560 eligible pair invocations; these are logical
counts, not phase costs or a speed forecast. Generated row-loan code is 6,617 B;
vector stack accesses remain, so fewer source loads alone do not prove lower cost.

Both sides time complete public Resize, including preparation, admitted working
storage, independent output and disposal. The labeled zimg3.0.6 reference retains
its graph/scratch, as in the existing comparison; this does not prove one-shot
setup parity. Frozen input/settings, exact managed output, native RGB≤1/alpha
exact bounds, source immutability, 4 KiB admission and quiet single-CPU0 guards
all pass. Total allocation stays113B/op; no new retained scratch or owned capacity.
Physical JIT peak was not measured.

Windows/Linux normal and scalar checks pass508internal/789public cases, including
actual poisoned row-loan/tail phases, cancellation, refusal and survivors. One
core/embedded build perOS; no harness/native/fixture rebuild or new workload grid.
Production source and qualified core/embedded/public modules were restored exactly.
The unchanged [current qualification](PACKED-HORIZONTAL-WINDOW-REUSE.md) and actual
package remain valid; broad/package checks were not repeated for the abandoned
candidate. Passing word and unrelated paths were not retuned.

The next independent item is attribution of the existing bounded narrow-byte
gap. Revisit wide overlap only with a materially revised design or attribution;
this trial is not a permanent ban on the strategy.

[Structured evidence](wide-row-loan-overlap-trial-evidence.json) records identities,
statistics, source reconstruction, checks and the mirrored raw archive:
`tmp/wide-row-loan-overlap-trial.tar.xz` — SHA256 `f65de0c9182c1a4440981608604c22a96cf312ef97bba62bfbb3256486edd8ee` (1,388,768B, 82members).
Every archive member is verified locally and on the VM. It retains attempted and
restored sources, immutable candidate modules, receipts, all actuals/native
brackets, guards, JIT output and build/run/restore recipes. Dependencies retain
the qualified predecessor, reference harness and frozen control provenance.
