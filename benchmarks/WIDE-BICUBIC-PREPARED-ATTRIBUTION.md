# Bounded wide bicubic: prepared source addressing

[Contiguous source views](CONTIGUOUS-WIDE-SOURCE-VIEWS.md) now record the
implementation and complete results. This report retains the prior attribution.

The existing complete 513×259→128×64 RGBA8/4KiB Resize retains its source-valid
371.62/zimg351.70µs, upper1.069 demonstrated gap. WPA836 profiles the actual
[qualified package](PREPARED-DEEP-PAIR-CONVERSIONS.md); the wide executor and its
numerical helpers are unchanged. No production change or new timing claim.

One two-second CPU0 capture with CPU1 sampler has 1,998 locations: row-loan
1,124, tail710, geometry Build68 and Kernel45. Current row/tail bodies are
6,204/6,512B. The row-loan indexed horizontal loop contains 509 locations and
still loads two offset bytes, checks two decoded indices and shifts their addresses
per tap. Its independent AVX256 multiply/add chains already fold coefficient
broadcasts into multiplies. Inlining and sampling skid prevent treating locations
as phase costs, removable percentages or predicted gains; instrumented elapsed
is discarded. Frozen exact pixels/native bounds/source/ownership/idle-v2 gates pass.

The next hypothesis consumes the existing once-per-tile contiguous-window proof
through two prebounded source spans, retaining the original t0…t15 multiply/add
order. It removes repeated offset metadata and source-index checks. Both columns
still load their own samples; no AVX512 shuffle/rolling window is introduced.
The [earlier overlap activation](WIDE-ROW-LOAN-OVERLAP-TRIAL.md) remains inconclusive
and is not replayed. The source-only model covers 60 pairs/10,560 invocations,
3,840 exact portable double comparisons and 337,920 logically avoidable offset
loads/checks. These counts are not CPU evidence or authorization to merge.

Keep indexed/clamped/odd/count17 and other modes as real production paths;
geometry, vertical scheduling, certification/repair, cancellation and admission
remain unchanged. No new buffer, coefficient layout, owner, pool, reservation,
retention or public API. Span setup, code size and register pressure could erase
the benefit. Actual SIMD/order/poison/ownership checks and the complete existing
short screen precede independent confirmation; reconcile affected costs before
an accepted-stage Windows/Linux/scalar/ISA/SAME-package milestone. Passing word
and unrelated controls remain stopped; no workload grid is added.

Current full/package qualification is reused. No production/embedded/harness/
native/fixture build, repeated broad qualification or displayed-cost change.
[Evidence](wide-bicubic-prepared-attribution-evidence.json) and mirrored/member-verified
raw `wide-bicubic-prepared-attribution.tar.xz` — 88,500B/21members,
SHA256 `f9fa9b0ba7d9933db351dd6620ea0308ac13907bfc1c321c507122d934591a25` —
retain the profile, identities, generated code, model and reproduction. Dependencies
retain the actual modules, reference and earlier inconclusive evidence. All289
material paths remain in scope; publication stays postponed.
