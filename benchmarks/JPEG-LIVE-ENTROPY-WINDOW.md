# Shared live JPEG entropy window

Current JPEG read costs are superseded by [paired AC windows](JPEG-PAIRED-AC-WINDOW.md); this report retains the live-window stage and its evidence.

DC and AC decoding now share one live-token window and canonical cursor
publication. Short EOB codes complete inside that window. AC continuation uses
the same kernel; stuffing, long codes and syntax failures retain scalar traversal.
Every required block, prefix and nonzero-amplitude cancellation checkpoint remains.
There is no new table, buffer, cache, owner, session or retained state.

| Complete live-token native444 decode, independent twelve | Lokad µs | FFmpeg µs | Ratio / upper | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Contemporary original | 1,908.29 | 1,714.55 | 1.113 / 1.123 | 10,088 |
| Accepted | 1,769.84 | 1,720.57 | 1.029 / 1.042 | 10,088 |

The stage saves **138.44 µs / 7.25% raw / 7.58% reference-normalized**.
Filtered and all-actual raw/normalized intervals separate; native moves +0.35%.
The remaining **49 µs gap** stays open. The preceding three-actual screen is
retained separately. No favorable repeats or broader JPEG speedup claim follow.

Ordinary RGB generalization measures 1,650.10 → 1,644.16 µs, with overlapping
intervals in every view. Sparse CMYK measures 526.72 → 527.63 µs, also overlapping
in raw time. Its reference moves 445.64 → 438.27 µs (−1.65%), and normalized loss
of 1.86% separates in both views. Preserve that adverse normalized result:
neither control establishes a causal raw gain, regression or equivalence.
The sparse candidate confirmation is reused from the affected-cost refresh.

All **23 affected baseline costs** refresh because dispatch changed, even though
the ordinary fused kernel body remains exact. Six unchanged progressive costs
reuse valid evidence. JPEG remains **14 pass / 13 gaps / 2 uncertain**; passing
cases stop and uncertainty is not an optimization target. Ten displayed rows
refresh; 115 reuse unchanged paths.

The primary control keeps its frozen 513×259, 133,041-byte packet and exact
531,468-byte managed/native RGBA result. Complete input delivery, parse/table
preparation, entropy, IDCT, color conversion, independent output and disposal
remain timed on one CPU. Native codec/conversion/input/frame contexts retain
their declared preparation; this is not a fresh-context comparison.

JLB546's two-second current-library profile locates 922 of 1,996 samples in the
AC window, with 79 in short Huffman reading and 50 each in continuation/bit
reading. Samples are leads, not phase times or projected gains. Instrumented
elapsed is discarded. Two setup attempts stopped before actuals because the
existing metadata projection was not applied; both failed logs and the corrected
wrapper are retained. No production defect or relaxed pixel gate was involved.

The audited private-reader failure boundary from
[normal publication](JPEG-LIVE-NORMAL-PUBLICATION.md) remains: failed state has no
public observer or resumption, while normal/fallback cursors and outer cleanup
stay exact. Windows/Linux each pass **5,879 full/live checks**, plus **403 internal /
375 public** per normal/scalar mode. Existing scalar grammar checks cover cached
phases, stuffing, truncation, partial writes and following reads. New witnesses
exercise actual fused DC/EOB execution across all eight phases and twelve DC
categories, plus cancellation before speculative DC publication.

The **same actual Release package/all three examples** passes both OSes,
including admitted live baseline JPEG sync/async ownership. One immutable main
and embedded build per OS serves development, measurement and qualification.
Unchanged public/harness/native/selector/fixture builds are reused.

GC above is measured separately from capacity. The unchanged native444 coarse
admission peak of 541,253 B and retained output of 531,468 B reuse
[ownership evidence](JPEG-FULL-GRID-OUTPUT.md); physical/native peaks are
unmeasured. Shared roots, terminal refusal, asynchronous lifetimes, exact packets
and managed pixels remain required.

[Evidence](jpeg-live-entropy-window-evidence.json) pins source proofs, all costs,
comparisons and same-package qualification. Mirrored/member-verified raw
jpeg-live-entropy-window-qualified.tar.xz, SHA256
e60ed31cde1a103805c8d5182fabea667d6b2b66c04eb866d7b10f1579367bdf,
8,469,936 bytes / 447 members, retains sources/modules, scripts, actuals, guards,
setup failures and package logs with pinned providers. This is not a full
125-row actual-package release refresh or completion of all 283 material paths.
Publication remains postponed.
