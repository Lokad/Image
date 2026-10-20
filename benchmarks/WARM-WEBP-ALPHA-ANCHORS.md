# Older alpha match anchors under the work limit

The existing32M-work packet shrinks **111,094 → 104,500 B (5.9%)**, preserving
exact prior VP8, decoded RGBA, source quality and alpha. ALPH falls
**28,333 → 21,740 B**, below the24,588 B reference component. The complete
**98,590 B** packet ceiling remains open by5,910 B.

| Existing control | Managed / reference packet B | Managed / reference µs | Managed allocated B/op | Short ratio / upper |
| --- | ---: | ---: | ---: | ---: |
| EncodeBudget8 | 104,500 / 98,590 | 15,008.23 / 53,686.75 | 2,012,156 | 0.280 / 0.326 |

One token-start anchor per hash preserves an older occurrence while the existing
rolling entry tracks positions inside copies. A probe at the longest-match
boundary rejects candidates that cannot improve it; all chosen prefix samples
remain checked exactly. There is one matching pass and adaptive token replay,
with no filtered alpha plane or extra RGBA frame. Actual31,970,409 units fit32M.

The borrowed root admits both histories (131,136 B) and tokens before allocation.
Histories end after matching; old token and new encoded capacities overlap under
admission. Staging stays charged through actual output. Independent smaller local
limits select the original history before shared admission; refusal, actual work
exhaustion or cancellation never retries it. Every read, update, probe, comparison,
initialization and existing writer/copy charge remains capped.

Both OSes pass4,682 tests and210internal+8public+2diff scalar checks. Eight new
cases cover owner/peak/aggregate/refusal, local decline, cancellation, cleanup and
subsequent success. Windows/Linux normal/scalar packets and managed pixels agree.
The same actual private0.2.0 package/all three examples pass bothOS. One successful
frozen core/internal build perOS is reused; unchanged public/diff/reference/harness/
native/selectors require no build. An initial naming compile error is retained.

One guarded three-actual job records costs; no independent repeat or CPU-parity
claim. Allocation rises from the preceding1,806,692 to2,012,156 B/op;
this cost does not veto the byte gain. Native allocations are unmeasured. Four
unrestricted records are exactly unchanged, primary opaque controls bypass alpha,
and six other lossy encode cost/quality records retain their C70 scope. No input,
reference, numerical setting or quality gate changes. Five packet gates remain
open; a component pass does not establish whole-codec compression parity.

[Structured evidence](webp-alpha-anchors-evidence.json) retains immutable raw,
source/producer/package, quality and cost proofs. The preceding wider-history
prototype reproduced all old tokens and was rejected for zero compression gain,
without production build/timing. The adopted anchor prototype predicted these
exact packet/work results before production qualification.
