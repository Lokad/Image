# Rejected Deflate token-loop experiment

The independently implemented framing walk validates Deflate before BCL inflation.
This trial kept literals and assigned short-code matches in the same local bit
cache, reducing returns to the length/distance dispatcher. Long or unassigned
codes, cancelable operations, tight work budgets, short tails and format errors
retained the original per-field path. No workspace or admission changed.
FFmpeg's PNG parser inflates rows through zlib; this informed the investigation
of repeated parsing work. No external implementation was copied.

One quiet three-actual screen used the existing palette8 + tRNS input and the
same resident-input to independently owned RGBA8 decode boundary.

| Screen | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Accepted palette8 row | 992.81 | 365.69 | 2.715 / 2.954 | 621,315 |
| Experimental token loop | 951.20 | 365.43 | 2.603 / 2.815 | 621,317 |

The mean is only 4.2% lower and remains above 2x. The trial was **reverted**;
no twelve-actual confirmation was launched. Both unchanged quiet guards pass and
all three actuals are retained. The upper uses the larger filtered/all-actual
bound. This screen does not establish a durable small speedup. The summary keeps
the accepted production result. Allocations include harness overhead; native
allocation totals are unmeasured.

Candidate and restored builds pass 102 focused framing tests on each OS. Four
new original-stream controls compare every tight budget, output boundary and
truncation against the cancelable per-symbol path, including repeated matches,
overlapping history, length/distance extras and a distance beyond history.
Exception type/message, remaining work and released workspace agree.
Forced restored builds match all 581 method bodies and flags to the accepted
actual package on both OSes. The accepted full-suite/package checks are reused.

The [numeric evidence](warm-png-token-run-rejected-evidence.json) retains source,
raw receipts, four timed child identities, native brackets and restoration proof.
No additional original performance image was introduced. The next coverage screen
uses one packed palette4 projection to assess the remaining generic expansion
path. The required uniform warm target remains open.
