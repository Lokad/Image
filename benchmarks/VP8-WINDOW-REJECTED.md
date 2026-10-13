# Rejected VP8 arithmetic-window integration

The wider reader's quiet three-actual screen averages **9,821.90 µs**, **2.052×
FFmpeg**, upper **2.115×**, with unchanged **760,548 B/op**. The previous
independent baseline is 9,504.58 µs / 1.983× / upper 2.004×. This candidate is
3.3% slower than the retained baseline and is rejected. Production and tests are
restored; opaque VP8 remains open. No twelve-actual candidate confirmation or
redundant full validation is run.

The [correctness prototype](VP8-WINDOW-PROTOTYPE.md) establishes exact arithmetic
behavior, not speed. Its integration stores the logical interval and up to 24
future bits in one word, refills in bounded batches and shifts the word during
normalization. Short partitions/spans retain the original failure recurrence.
Only the reader constructor and `Read` bodies change; one `Fill` helper is added.
The other 628 existing bodies and all 630 existing implementation flags match
the actual tested package. The 20-to-24-byte state fits the existing fixed
8,192-byte header/model/transform allowance; memory admission stays unchanged.

Forty-two focused arithmetic, fixture and admission tests pass on both Windows
and Linux, with intrinsics enabled and disabled, including four new long-sequence
checks with changing span boundaries. Full logical cursors, work and failures
still match the scalar recurrence. Both original opaque/alpha pixel hashes,
source bytes and held-output independence remain exact in all four modes.

Timing uses the same frozen packet, complete public decode/output/disposal,
one pinned CPU, normal SIMD and existing FFmpeg worker. Both quietness guards
pass. All three actuals, native brackets, source, actual candidate package and
four generated child files are frozen before restoration. Extra guards, cursor
updates and refill costs are possible explanations; this round does not isolate
their individual contributions.

Fresh restored builds on both OSes match all 630 production bodies and flags
of the previously validated actual package. Its 2,784 tests and all three
package examples per OS remain authoritative. The
[numeric evidence](vp8-window-rejected-evidence.json) retains the candidate,
method delta and restoration proofs. Keep the prototype as a correctness
reference, but do not repeat this representation unchanged. Cover one remaining
material workload and prioritize a measured substantial gap before small
upper-bound margins.
