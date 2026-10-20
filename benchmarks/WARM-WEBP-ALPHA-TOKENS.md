# WebP alpha tokens under the work limit

The existing 32M-work control shrinks **208,216 → 181,998 B (12.6%)**, preserving
exact prior RGBA, source quality and alpha. ALPH falls **54,552 → 28,333 B**;
the complete **98,590 B** reference target remains open.

| Existing control | Managed / reference packet B | Managed / reference µs | Managed allocated B/op | Short ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| EncodeBudget8 | 181,998 / 98,590 | 20,972.78 / 53,600.83 | 2,295,207 | 0.391 / 0.435 |

One matching pass collects compact tokens and frequencies. Adaptive prefix codes
replay those tokens, with complete copy fields packed into at most 24-bit writer
calls. Actual work is **31,980,594 units** inside 32M; physical calls and every
other performed phase remain charged. No source, setting or quality gate changes.

The borrowed shared root admits token storage and overlapping encoded capacity
before allocation. History, tokens and models end after their actual users;
encoded staging remains charged through output. Smaller local workspace selects
fixed staging before shared admission. Refusal, exhaustion and cancellation are
terminal. Completed padded byte costs choose the smaller candidate without a
second matching pass or filtered plane.

Both OSes pass **4,654 tests**, including ten new overlap, aggregate/every-growth
refusal, cancellation, interrupted-output, work-limit and no-gain cases. Existing
fixed-stage tests declare their smaller local workspace; public 8/16-bit surviving
output tests cover the new default. Scalar checks (182 internal, eight public,
two differential), exact normal/scalar packets and the same actual private 0.2.0
package/all three examples pass. One core/internal build perOS; unchanged public,
differential, reference, harness, native and selector binaries are reused.

The single guarded three-actual cost screen records the figures above; native
allocations are unmeasured and CPU parity is deferred. No rebuild follows freezing,
and no full-suite or independent timing repeat is spent on unchanged inputs.
Four unrestricted records and passing primary content remain unchanged. All five
procedural packet gates stay open; no broader compression claim follows.

[Structured evidence](webp-alpha-tokens-evidence.json) records immutable raw proofs
and the preceding unadopted coefficient-level screen. That screen saved about 1%
and is retained for later; the larger alpha gain took priority without a CPU veto.
