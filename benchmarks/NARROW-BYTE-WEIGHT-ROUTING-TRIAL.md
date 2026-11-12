# Narrow byte coefficient-routing trial

NWR842 is **unselected**. Two direct variable permutations replace three fixed
coefficient-routing shuffles per group, but the complete operation shows no useful gain.

| Existing513×259→256×129 RGBA8 Resize /4KiB | Lokad / zimg µs | Ratio / upper | GC B/op |
| --- | ---: | ---: | ---: |
| Qualified immutable baseline | 430.01 / 418.45 | 1.028 / 1.106 | 113 |
| Direct coefficient routing | 430.61 / 418.97 | 1.028 / 1.170 | 113 |

Each screen retains three actuals and both native brackets. Candidate raw cost
grows0.14%,reference-normalized cost0.014%,native+0.125%; all confidence overlaps.
These screens establish neither a gain nor a regression. No independent
confirmation, word recapture or broad/package repeat follows this result.

The change selects defined lower coefficient lanes0..3 directly and preserves
compact preparation, every ordered Add(Multiply), source shuffles, all caller
bodies, storage loans, row/tail scheduling, exact repair, ownership and
cancellation. The whole original file reconstructs. Generated row/tail bodies
grow6,616→6,627B /6,562→6,573B despite removing one instruction per group;
constant indices and variable-permutation costs remain included. Fewer source
instructions are not CPU-gain evidence. No new capacity, owner, pool or API.

Windows/Linux each pass528internal/791public affected checks in normal/scalar
modes, reusing the existing exact-bit/high/zero/tiny-alpha/noallocation and
actual row/tail/generalization/refusal/cancellation/surviving-owner cases.
One core/embedded build perOS,zero public/harness/native/fixture builds and no
added test. Original source and qualifiedc345/b96core/embedded/public modules
are restored exactly; the actual package016a1ec5 and its
[6,449full/live checks perOS/SAMEpackage/all3examples](PNG-STORED-WORKSPACE.md)
remain unchanged. No current benchmark cost is replaced.

[Evidence](narrow-byte-weight-routing-trial-evidence.json) retains both complete
screens, attempted source/modules, generated code, proofs and restoration.
Mirrored/member-verified raw`narrow-byte-weight-routing-trial.tar.xz`,SHA256
`1cf8af7e9ef10263a31fef0d5b8f697875dab971f12035e7d43bd28ce8451fd3`
(1,380,800B/56members), links baseline/reference dependencies. The expanded
coefficient-storage alternative remains a design lead with an explicit storage/
tail-traffic tradeoff; this result does not ban a revised strategy.
Continue the existing Gray8 PNG uncertainty before assuming it needs tuning.
All289paths and publication postponement remain.
