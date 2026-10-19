# Exact AVX2 byte horizontal products

The selected byte helper cuts complete Reduce8 time **13.3%**. All four
existing cases refresh on the same producer, with **5.3–13.3%** elapsed savings;
all remain gaps against independently confirmed warm single-CPU parity.

| Complete operation | Lokad µs | Reference µs | Ratio / conservative upper | Lokad allocated B/op | Time saved |
| --- | ---: | ---: | ---: | ---: | ---: |
| Reduce8 | 1,367.60 | 489.21 | 2.796 / 3.043 | 146,645 | 13.3% |
| Reduce16 | 1,482.23 | 506.91 | 2.924 / 3.429 | 146,623 | 5.3% |
| Enlarge8 | 7,575.00 | 4,078.05 | 1.858 / 2.013 | 351,912 | 5.5% |
| Enlarge16 | 8,312.18 | 3,921.24 | 2.120 / 2.180 | 4,617,090 | 6.6% |

Byte premultiplication is at most `255^2=65025`, so signed 32×32→64 AVX2
products retain every bit. The existing absolute axis-weight bound keeps all
prefixes below `65025*2^31 < 2^47`. A separate typed helper removes the depth
branch from this loop and needs no AVX512DQ/VL. The accepted word arithmetic,
coefficients, vertical accumulation and normalization remain unchanged; word
rows above are fresh controls of the shared entry and generated code.

Working storage, owners, budgets, work charges and polling boundaries remain
unchanged. No array or cache was added; small allocation differences reflect
harness amortization. Existing independent row/oracle/negative-weight/boundary
controls are reused. All four protected full frames remain exact on both OS.

An earlier eight-lane paired-tap candidate retained exact pixels but regressed
Reduce8 from 1.577 ms to 2.099 ms (**33.1%**). It was rejected after one passing
quiet screen, without expanding its matrix or full qualification. Its frozen
producer, source, samples and receipts remain in the linked numeric evidence.
The rejected DLL/test assemblies do not qualify the restored or selected source.

The selected producer is immutable throughout its four shorts. Original inputs,
native worker/quality proofs, harness and compiled selectors are reused. Reference
remains explicitly **zimg, FFmpeg's zscale backend**, with equivalent complete
boundaries from the [baseline](WARM-LANCZOS3-BASELINE.md). All eight selected quiet
guards pass; both native brackets and selected/all-actual confidence views remain.
No long confirmation was run above one. No new profile or settings grid was added.

Qualification: **4,448 Release tests per OS** (1,635 internal, 1,881 public and
932 live FFmpeg), **47 Lanczos checks with all intrinsics disabled** and another
**47 with AVX512F disabled per OS**, the same actual private 0.2.0 package consumer
and all three packaged examples on both systems. Twelve font comparisons keep
unchanged paths. Published 0.1.0 stays immutable. Other 227 numeric rows retain
their historical builds; parity and replacement readiness remain open.

[Numeric evidence](warm-lanczos3-byte-products-evidence.json) includes all samples,
allocations, identities, bounds, rejected trial and reuse. Qualified source:
`01c63aaf22360bad3e1c0c0127f9cd265c4031b2a04ecf77e56e65596417839c`.
Raw `l3b-qualified-windows.tar.gz`:
`8021a3fefc1276dc8c25bf13901d38ccc60b0e15c8a73982a8b5face2b984b38`;
`l3b-qualified-linux.tar.gz`:
`a8a9e56bb34aeb6acb712d77a8c384bc172321db8150b72344aefffe7a05ba4e`.
Unchanged dependencies, fixtures and worker remain in the baseline archive.
Next: exact unsigned word products with negative-weight correction, targeting
Reduce16 first, with a proof and narrow screen before assuming a gain.
