# Paired Lanczos output rows

Fast byte reduction traverses two output windows together. A cached horizontal
row feeds both independent vertical sums before its ring slot can be reused.
Each output retains its original nonzero coefficient/FMA order and exact
certification/repair. The existing horizontal producers stay unchanged; a
four-row producer is unnecessary after confirmed parity and is not implemented.

The existing complete prepared 513×259→256×129 RGBA8 operation independently saves
**79.62 µs / 14.68% raw / 7.40% reference-normalized**. Filtered/all-actual raw and
conservative normalized intervals separate. Native time moves **−7.87%**, so these
views remain separate. Candidate **462.72/zimg489.40 µs, upper0.950** passes and
stops. No wider shape/content or companion causal-gain claim is made.

| Existing complete operation | Managed µs | zimg µs | Ratio / upper | GC B/op | Retained plan B |
|---|---:|---:|---:|---:|---:|
| Prepared RGBA8 reduction | 462.72 | 489.40 | 0.945 / 0.950 | 138 | 139,800 |
| One-shot RGBA8 reduction | 591.47 | 1580.94 | 0.374 / 0.376 | 138,650 | 0 |

Preparation and working rows are retained on both prepared sides. Source reads,
row-ID reset, exact execution, independent output and disposal remain timed.
One-shot includes axes/workspace/graph/input preparation and ownership on both
sides. zimg is FFmpeg's labeled zscale backend with float intermediates; managed
Q30/rational pixels and the original native gates remain unchanged. Every actual,
both reference brackets, source/output hashes and quiet CPU0 guards are retained.

One sum row borrows **8,192 B** from eight unpublished output rows. Only preceding
output pairs are published while this view is live; the ordinary eight/nine-row
tail overwrites it after retirement. Tiny results, long supports, scalar, word
and height enlargement paths retain their existing execution. No array, reservation,
owner or retained capacity is added. The Linux prepared plan stays **139,800 B**;
one independently owned output is **132,096 B**, giving **271,896 B** combined
logical owned capacity, excluding borrowed input. Repeated GC stays **138 B/op**.
Physical/native/JIT peaks are unmeasured. Admission refusal remains terminal.

Thirteen added controls verify exact integer complete frames, odd/even and
unaligned prefix/tails, cache wrapping/long supports, low alpha, independent
scalar FMA bits/canaries, shared refusal/cancellation cleanup and surviving owners.
Focused checks pass 130 internal / 15 public normal/scalar on Windows, and additionally
AVX512-off/AVX2-off on Linux; the three named resize-storage suites are included.
Each OS passes **6,551 full/live checks**. The SAME actual Release package and all
three examples pass both OSes, including frozen paired-output pixels, shared
refusal, cancellation/recovery and independent surviving lifetimes.

The package contains the exact timed Linux DLL; Windows source builds qualify
separately. One production build per OS, with one extra Linux internal-only build
for stronger final tests; zero harness/native/fixture builds and one no-build pack.
No production rebuild after timing. Unchanged six Lanczos views and unrelated
families reuse source-valid evidence. Current Lanczos scope is **six passes / one
word-reduction gap / one word-enlargement uncertainty**. Published packages remain
immutable; publication and the full 289-path objective remain open.

A completed short baseline initially failed metadata postprocessing because its
mode was compared with a confirmation record. It was recovered without retiming.
The VM parser lacked its BDN dependency; the existing verified local parser
processed those same logs. Both failures and the captures remain recorded.

[Evidence](lanczos-paired-output-evidence.json) pins every cost, source/reference
identity, storage proof and qualification. Mirrored/member-verified raw
`lanczos-paired-output-qualified.tar.xz` contains 137 members / 2,081,032 B, SHA256
`cc4078cf9ba6b61db8dae5af25789da5c9cff1e86967d7d21d6a2c6c09d03c84`.
Its released-storage dependency retains unchanged tools/fixtures and prior
qualification. Earlier [full-row product](LANCZOS-BYTE-PRODUCT-PREPARATION.md),
[publication](LANCZOS-ROW-PUBLICATION.md) and [source-tap fusion](LANCZOS-FUSED-VERTICAL-EXPERIMENT.md)
studies remain distinct history; none is blindly replayed here.
