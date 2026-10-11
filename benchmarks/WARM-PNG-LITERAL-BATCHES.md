# Warm PNG literal-batch trials

All batching variants are discarded. Partial literal runs improve PNG16,
but regress PNG8; restricting the attempt to an already decoded literal
reduces both effects. Retained production remains unchanged: PNG16 is
2.22x / 2.22x and PNG8 is 2.11x FFmpeg. Uniform warm parity remains open.

The [numeric evidence](warm-png-literal-batches-evidence.json) preserves
original samples, confidence bounds, allocations, identities, quietness
failures and immutable raw/source archive hashes. Regenerate this report
with `python benchmarks/render-warm-png-literal-batches.py`.

| Variant | Case | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | --- | ---: | ---: | ---: | ---: |
| Retained Work | Decode16 | 6482.70 | 2919.78 / 2935.67 | 2.22x | 2.44x |
| Retained Work | Decode8 | 2994.19 | 1418.29 / 1422.28 | 2.11x | 2.32x |
| Retained Work | Decode16 | 6469.95 | 2919.63 / 2916.54 | 2.22x | 2.39x |
| Rejected eight-only | Decode16 | 6471.33 | 2924.68 / 2921.18 | 2.22x | 2.53x |
| Rejected partial | Decode16 | 6002.32 | 2921.66 / 2921.21 | 2.05x | 2.21x |
| Rejected partial | Decode8 | 3146.47 | 1417.41 / 1421.99 | 2.22x | 2.28x |
| Rejected partial | Decode16 | 5982.00 | 2916.16 / 2921.69 | 2.05x | 2.15x |
| Rejected partial | Decode8 | 3140.75 | 1417.93 / 1417.25 | 2.22x | 2.37x |
| Rejected after-literal | Decode8 | 3068.82 | 1414.27 / 1418.87 | 2.17x | 2.44x |
| Rejected after-literal | Decode16 | 6297.99 | 2917.22 / 2922.63 | 2.16x | 2.20x |

Eight-only batching has no useful gain and receives no repeat. Partial
batching repeats PNG16 at 2.05x / 2.05x, about 7% lower managed time, but
PNG8 repeats at 2.22x / 2.22x, about 5% higher managed time. Starting the
batch after an ordinary literal screens at 2.17x PNG8 and 2.16x PNG16:
the PNG8 penalty persists, so that variant receives no repeat. Every
candidate keeps the same input and exact owned output identities.

The independent RFC 1951 framing walk probes local reader state and commits
only valid literal runs with fully admitted exact bit work, sufficient output
and a noncancelable token. Short inputs, unassigned or long codes, end codes,
matches, tight budgets and cancellable operations keep the original path.
These safeguards pass correctness checks; they do not justify retaining a
performance tradeoff. All trial production code is restored.

Two preceding inlining diagnostics receive no statistical timing. Inlining
the full canonical decoder removes its calls, but leaves seven Consume
calls. Also inlining Consume removes those calls but leaves other reader
calls and stack-backed state. Optimized Validate code grows from the retained
9,380 bytes to 10,759 / 11,485 bytes. Both diagnostics pass pre/post quietness
and are discarded. They provide no new CPU-cost attribution.

Only the two existing original 513x259 RGBA8/16 rasters are timed. Complete
public decode and independent output acquisition/release remain inside the
boundary, including native conversion and reset. Context/scratch reuse is
declared; startup, IPC, I/O, generation, hashing and validation are excluded.
Sequential Release jobs use CPU 0, one codec/conversion thread, normal SIMD,
.NET 10.0.8 / SDK 10.0.204, disabled tiering and non-concurrent workstation
GC on the dedicated AMD EPYC 9V45 VM. Actual statistical prechecks pass;
postchecks fail and remain recorded. One failed after-literal precheck
collects no samples. No thresholds are relaxed and no <=2x claim is made.

Four new reserved-code cases retain exact work and failure precedence after
valid literal runs, atomic failing-prefix admission and scratch release,
with both default and cancellable tokens. They pass against the original
production before the trials. All 2,032 tests pass on restored production
on Windows and Linux: 452 internal, 686 public and 894 live FFmpeg.

Restored production bytes match the accepted Work checkpoint. All production
IL and implementation flags match the same previously verified actual
dependency-free Release package, which passed all three usage examples on
both platforms. Its original controls also match the retained statistical
library; rejected candidate libraries remain separate. Package SHA256:
`3ad41ce5d341a39cde352ffd93e7a2555794c18c2794aad6866c60be9b1a40c2`.
