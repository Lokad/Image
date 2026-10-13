# Rejected VP8 normalization trial

Bulk arithmetic normalization does not establish a useful gain: its quiet
three-actual screen averages **9,669.12 µs**, **2.016× FFmpeg**, upper **2.320×**,
with unchanged **760,548 B/op**. The prior independent baseline remains
9,504.58 µs / 1.983× / upper 2.004×. The trial is rejected; all original
production and tests are restored. No twelve-actual trial or redundant full
validation is run for the rejected code. Opaque VP8 remains open.

One six-second profile records 5,863 samples, with 42.1% in coefficient-token
decisions and 18.0% in coefficient decoding. Its post-profile quietness guard
fails, so it is retained as diagnostic evidence only. It suggests investigating
arithmetic decoding; it does not support a timing or parity claim.

The candidate computes the exact normalization width with leading-zero count
and consumes bits from one or two bytes. Short partitions/spans retain the
original recurrence and failure precedence. Only `Vp8BooleanReader.Read` changes;
the other 629 bodies and all existing flags match. Forty-two focused arithmetic,
fixture and admission checks pass with intrinsics enabled and disabled on both
OSes, including four new full-cursor/work sequence checks. Existing opaque and
alpha decoded pixels remain exact. Larger inlined code and additional guards
are possible costs; this round does not isolate their contributions.

The identical frozen opaque packet, single pinned CPU, normal SIMD, optimized
JIT and complete public operation remain fixed. Both screen quietness guards
pass. Source, actual trial package, all four child files and every actual are
frozen before restoring the original bytes. A supplemental archive retains
candidate logs omitted by the initial archive's inherited filename filter;
the first archive remains unchanged. An inherited smoke-provenance boolean is
explicitly corrected by the actual method/package proofs in the evidence.

Fresh Windows/Linux builds after restoration match all 630 bodies and flags in
the tested package. Its 2,784 tests and three package examples per OS remain the
validation authority. The [numeric evidence](vp8-normalization-rejected-evidence.json)
retains the profile, rejected measurement, package delta and restoration proofs.
Next, prototype a wider arithmetic window while preserving cursor, work,
cancellation, malformed-input and sample contracts. No speed claim is made for
that unimplemented strategy.
