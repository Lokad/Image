# Prepared PNG match probes

Prepare stable target prefix/tail samples once per position or best length.
Greedy and lazy matching share one bounded ring-safe adjacent read. No arrays,
owners or admission changes are added. Chain/probe/tie/parse decisions and all
complete packets remain exact.

| Equivalent complete one-shot control | Before µs | Current µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 Optimal6 | 20,504.02 | 19,527.95 | 17,268.14 | 1.131 / 1.144 | 932,516 |
| RGBA8 Fastest1 | 6,093.28 | 6,094.99 | 4,338.76 | 1.405 / 1.431 | 951,212 |

Optimal saves976.06µs/4.76% raw and4.69% reference-normalized, with separated
filtered/all-sample and normalized confidence. Fastest managed time is flat;
its adverse2.21% normalized point has overlapping confidence and a2.19% faster
reference clock. No Fastest gain or causal regression claim. Both CPU gaps remain.

Two short screens precede two independent12-actual confirmations, bracketed by
12native samples each side with passing quiet guards. Both sides include fresh
codec preparation, normalization, independent packet ownership and disposal.
Sources, settings and exact210,973/236,828-byte packets are unchanged. Unchanged
[qualified PNG before evidence](DEFLATE-RELATIVE-LINKS.md) is reused; unrelated
codec changes do not invalidate those paths.

One bounded existing RGBA16 Optimal before/candidate screen gives25,723.97→
24,820.81µs, a3.51% raw point, unchanged3,420,516B/op and exact963,202-byte packets.
This3-actual generalization is not a broader confirmed speedup or equivalent
one-shot native comparison: that worker retains native codec state. Its first
setup stopped before timing on a stale native source identity. The old worker,
failure and guards remain; one required native-only rebuild from pinned sources
precedes the first valid pair. No gate bypass, retuning or scored resampling.

Windows/Linux each pass5,456full/live checks and577internal+211public affected
checks in normal/scalar modes. Frozen ring/overlap packets cover levels1/6/9,
logical origin0/2^40 and three delivery chunks. Shared refusal/cancellation,
exception cleanup, async lifetimes and independent output behavior pass. One
immutable core/internal build perOS; public/differential/harness/selectors reused.
The SAMEactual0.2.1Release package and three examples pass bothOS. Published
packages are unchanged; publication is postponed.

Allocation is unchanged. Deflate workspace allowance remains384KiB; retained
encoder scratch is zero. Complete-operation physical/admitted peaks and native
allocation are unmeasured. Other affected PNG encoding costs remain historical;
stored encoding, decoders, inspection, other codecs and editing paths reuse proof.

[Evidence](png-prepared-probes-evidence.json) retains every actual, bracket,
failure, source/runtime pin, scope and qualification. Raw png-prepared-probes.tar.xz:
20,971,392B /4177members, SHA256
a99e642670a8d9462e89e40663898289f1abdb03a6bffbd4bbd995eab90b4263, mirrored/member-verified.
Before dependencies: webp-cache-prefix-aggregation.tar.xz (SHA256
34e5bbd505797c124e14330bdfaa3e1cb0992c3c6f04a7fc9861d7b10e28cd06) and
deflate-relative-links.tar.xz (SHA256
78f41a7b55cf0b1594cf7d9e0c61c20a9e290e4cc3ef02258b242eb34e714890).
