# Lanczos coefficient preparation investigation

Two coherent implementations prepare the same widened/permuted Q30 lanes once
instead of repeating preparation for every horizontal source row. The first
owns lanes in the workspace and forwards spans through row calls. The revised
stage owns them in the existing axis and restores the original call signatures.
Typed byte/word products, exact sums, conversions, certificates and repair stay
unchanged. Both implementations are removed; production restores byte-exactly.

| Existing prepared 513×259→256×129 control | Qualified prior µs | First screen µs | Revised screen µs | Revised zimg µs | Allocated B/op | Prior → candidate retained B |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| RGBA8 | 563.84 | 602.67 | 605.33 | 488.65 | 139 | 139,800 → 237,752 |
| RGBA16 | 558.34 | 669.00 | 635.66 | 511.37 | 139 | 148,040 → 245,992 |

Each candidate control has **three actuals**, all retained, native brackets and
before/after quiet guards. These are development screens, not independent gain
or causal-regression confirmation. Both revisions have adverse managed and
reference-normalized points. The first byte/word raw changes are +6.89%/+19.82%;
the revised changes are +7.36%/+13.85%. The retained-memory cost is 97,952 B per
plan. No candidate merits confirmation from these observations. No repeat is
used to obtain favorable numbers, and no claim closes all future preparation
designs. Bounds checks, memory access and register pressure remain hypotheses,
without a profiling-based allocation of the adverse cost.

Both sides reuse prepared state, read current input and return independent owned
output. Setup/JIT/validation are excluded equally; output allocation/disposal and
the complete public resize stay timed. All original source/output/native-quality,
ownership and producer gates pass. The zimg reference uses float intermediates;
managed Q30/rational pixels remain exact. No numerical-policy change is proposed.
Total allocations, retained capacity and physical peak are distinct: 139 B/op is
unchanged; physical/admitted peaks and native allocations are not remeasured.

Each Linux stage passes 80 internal/15 public checks in normal/scalar modes.
Four new theory cases cover independent BigInteger rows, long supports, tails,
alpha extremes, source immutability, zero allocation, retained admission, terminal
refusal, cancellation cleanup and subsequent reuse. Candidate-only tests leave
with the removed implementations; their source and modules remain in raw history.
No Windows, harness, native-worker, selector, paired-tool or package build occurs.
Two Linux core/internal builds total. Initial postprocessing used a missing local
historical filename; it is corrected against existing public evidence without
repeating a completed job.

All production sources, selected modules and consumers restore to the qualified
[luma4 checkpoint](VP8-LUMA4-PREPARATION.md), including the same actual package.
Its 5,639 full/live checks per OS, affected fallback checks and actual-package/
all-three-example proof remain applicable. No broad requalification follows an
exact restoration. All 125 current comparison rows stay unchanged and the full
282-path objective remains open. Publication stays postponed.

[Evidence](lanczos-coefficient-preparation-evidence.json) retains both source/
producer epochs, original gates, every actual, costs, restoration and workflow.
Reproduce with retained `lcp301*.py` on the quiet single-CPU VM, unchanged prepared
resize harness/workers and frozen source. The revised stage is a materially changed
ownership/call-boundary experiment, not an unchanged measurement retry.

Raw `lanczos-coefficient-preparation-investigation.tar.xz`: 3,575,796 B / 225 members,
SHA256 `d27597f51571d1299bb0e3257388c5f695c9c27ef00a711450918e83add5ad7f`; mirrored and every member verified locally/VM.
Dependencies retain the qualified original/package and unchanged controls,
workers, harness and framework.
