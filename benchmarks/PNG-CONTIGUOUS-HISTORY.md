# Contiguous PNG history

The existing 64 KiB byte history now shares the dictionary's relative coordinate
system. Each origin advance copies the upper 32 KiB down in place. Greedy/lazy
search follows bounded Int32 relative links, avoiding repeated absolute-position
reconstruction and ring-split reads. Long comparisons and retained stored-block
output use contiguous spans. Logical stream/block/deferred-match positions remain
wide; keys, candidate order, tie breaking and complete packets remain unchanged.
The old ring comparison helper is removed; tests exercise the production helper.

| Equivalent complete one-shot control | Before µs | Current µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 Optimal6 | 21,218.89 | 20,087.00 | 18,665.52 | 1.076 / 1.083 | 932,516 |
| RGBA8 Fastest1 | 6,204.10 | 5,911.28 | 4,366.42 | 1.354 / 1.432 | 951,212 |

Optimal saves **1,131.88 µs / 5.33% raw**, **5.60% reference-normalized**, with
separated filtered/all-sample raw and normalized confidence. Fastest points improve
4.72% raw / 5.30% normalized, but intervals overlap: no confirmed Fastest gain or
causal regression claim. Native clocks shift +0.29% / +0.61%. Both CPU gaps remain.

Four short before/candidate jobs precede four independent twelve-actual jobs on
the SAME immutable builds. Every actual contains four complete operations; native
brackets and quiet guards pass. Both sides include fresh codec/zlib preparation,
normalization, independently owned packet output and disposal. All copy/sliding
cost stays timed. Original input/settings and exact 210,973 / 236,828 B packets
remain; startup, I/O and correctness checks are excluded equally. No retuning,
new control shapes or favorable resampling.

The extra 32 KiB copy per slide is roughly another traversal of supplied input.
It adds no array, owner, cache, session or retained capacity. Allocation is
unchanged; Deflate's conservative workspace allowance remains **384 KiB**, retained
encoder scratch zero. Complete-operation admitted/physical peaks and native
allocation are unmeasured; B/op is not peak live storage.

One bounded existing RGBA16 Optimal pair gives 24,791.07→24,337.72 µs (1.83% raw
point), unchanged 3,420,516 B/op and exact 963,202 B packets. Its three actuals
are a generalization screen, not a confirmed broader speedup. That worker retains
native codec state; it does not establish equivalent one-shot comparison.

Current attribution finds 88.53% of 715 identifiable samples in lazy processing.
It supports investigating the complete search/addressing loop; sample fractions
are not component times or mask-specific available savings. All profiled durations
are discarded. [Earlier prepared probes](PNG-PREPARED-PROBES.md) retain superseded
costs and their original evidence; current before/candidate runs are fresh.

Windows/Linux each pass **5,466 full/live checks**, plus 577 internal / 211 public
affected checks in each normal/scalar mode. Exact packets cover levels 1/6/9,
logical origins 0/2^40 and three delivery chunks. Production comparison tests cover
contiguous lower/upper bounds, every mismatch lane, short limits and overlap.
Shared refusal, cancellation/writer failure after real slides, asynchronous storage
and surviving output ownership pass. One production/internal build per OS, zero
public/differential/harness/native/selector rebuilds. The SAME actual 0.2.1 Release
package and three examples pass on both OSes, including complete sync/async PNG
packets and working-limit admission after slides. Published releases are unchanged;
publication is postponed.

[Evidence](png-contiguous-history-evidence.json) records all actuals, brackets,
source/producer identities, generalization and package proof. Its raw archive is
mirrored and member-verified. Other affected compressed-encoding costs remain
historical until refreshed; stored-only encoding, decoding/inspection, other codecs
and editing/text reuse their applicable unchanged evidence.
