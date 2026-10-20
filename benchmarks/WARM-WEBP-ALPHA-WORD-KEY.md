# Four-residual alpha match keys

The existing32M-work WebP control shrinks **104,500 → 102,006 B (2.4%)**,
with identical prior VP8, native/public decoded RGBA, source quality and alpha.
The complete **98,590 B** reference ceiling remains open by **3,416 B**.
ALPH decreases **21,740 → 19,246 B**, below its24,588 B reference component;
this component pass does not establish complete-packet or general WebP parity.

| Existing control | Managed / reference packet B | Managed / reference µs | Managed allocated B/op | Short ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| EncodeBudget8 | 102,006 / 98,590 | 15,047.12 / 53,488.81 | 1,997,180 | 0.281 / 0.334 |

Only the admitted two-history alpha matcher changes from a three-residual to a
four-residual key. The same16,384 buckets, recent/token-start histories and exact
prefix checks remain. A rolling four-sample window acquires each residual once,
without a filtered plane, extra frame, pass or cache. UInt32 arithmetic fits all
four byte terms and masks before signed conversion. Tighter local paths and the
unrestricted matcher are unchanged. All actual work remains charged: **31,947,691**
units fit the unchanged32M cap, versus31,970,409 previously.

Both histories/tokens are admitted through the borrowed root before allocation;
token and replacement encoded capacity overlap under admission. Owners end after
their actual users. Shared refusal, cancellation and actual work exhaustion stop
without retry, with cleanup and subsequent success verified. Both OSes pass
**4,692 tests** and216 internal+12 public+4 differential scalar checks. Four new
cases cover maximum unsigned keys and short tails; existing admission, replacement,
refusal and cancellation checks pass. Windows/Linux normal/scalar packets agree.
The same actual private0.2.0 package/all three examples pass both OSes.

One frozen core/internal-test build perOS; unchanged public/differential/harness/
native/selector binaries are reused. The one short guarded three-actual cost run
records **1,997,180 B/op**, versus2,012,156 before; native allocations
are unmeasured. No CPU-parity claim, independent timing repeat, quality search,
reference/settings/input change or workload expansion. Managed100/native75 and
effort4 remain fixed; unchanged reconstruction quality establishes the comparison,
not equal numerical quality. Four unrestricted full records are exactly C73 and
primary opaque paths bypass alpha, retaining their six scoped cost/quality proofs.
All five procedural complete-packet gates remain open.

[Structured evidence](webp-alpha-word-key-evidence.json) identifies original/current
packets, work, quality, source/producer/package, allocation and immutable raw proofs.
The bounded prototype reproduced the former tokens exactly, then predicted the
accepted packet/work result before the production build. Unrelated codec, editing,
text and inspection confirmations retain their unchanged scope.
