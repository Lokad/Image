# Font vector reduction trial

**Inconclusive, unaccepted:** complete owned-font loads measure
**16.662238 → 16.675965 µs**, ratio interval **0.978612–1.023541**,
**176 B/op** unchanged. All three reversed alternating 16,384-operation pairs
remain. No useful gain or causal regression is established; stop without
confirmation, standalone-reference refresh, production or broad/package changes.

FHI1172 reuses all 780 header leaves from the existing FAT1165 trace. The original
capture contains a map but no machine bytes. A fresh untimed original-worker
codegen check on the same immutable DLL produces 1,408 B; every sampled relative
address matches an instruction boundary. This supports an instruction lead,
not identical cross-capture bytes, stall attribution or a removable-time estimate.
Older 1,401 B candidate codegen is explicitly excluded from this reconciliation.

The isolated prototype replaces `MoveMask` plus scalar masking with integer
`TestZ`, observing the same xmin/ymin words. Compare results are zero/all-one
words, making the selected-lane predicates equivalent. Actual generated code
uses `vptest`, removes the YMM-to-GP mask and occupies 1,402 B, plus a 32 B read-only
constant. Loads, signed comparisons, eager checks, scalar/cancelable paths,
first-error order, admission and ownership remain unchanged.

Each OS passes **16,384 ordered-oracle cases per normal, AVX2-off and scalar
mode**, plus **166 public font/text/admission checks per normal/scalar mode**.
Original complete-load source, metrics, independent surviving faces and cleanup
checks pass. One isolated Windows core build; zero VM, canonical, embedded/public,
existing harness/reference/fixture/package builds. Unchanged production reuses
6,776 full/live checks and the same actual package/three examples each OS.

Two persistent default-loader processes warm 16,384 operations each, then alternate
complete load/dispose batches on CPU0 with natural nonconcurrent GC, two batch
clocks and passing versioned quiet guards. This paired-DLL screen is not FreeType
parity or an overall text-processing claim. No owned/retained capacity is added;
physical/native/JIT peaks are unmeasured. Eager/lazy validation differences remain.

[Evidence](font-vector-reduction-trial-evidence.json) pins the proof, original
trace reconciliation, exact modules, all actuals and qualification reuse.
Mirrored/member-verified raw `font-vector-reduction-trial.tar.xz`:
**1,298,828 B / 74 members**, SHA256
`b73ea0fa17840a2d6e715227c0232cfb08cb2907f64544a22b69bc191cdf1aa4`.
Archived `fhi1172-*` / `fzt1173-*` recipes and dependencies reproduce the round.
Keep current costs and reduction; this rejected trial is not a renderer input.
