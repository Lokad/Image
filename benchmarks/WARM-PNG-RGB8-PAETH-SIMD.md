# SIMD PNG RGB8 Paeth reconstruction

Complete RGB8 Paeth decoding improves **19.2%**, with unchanged **3,448 B/op**.
It independently passes warm one-CPU parity and stops. Filtered/unfiltered raw
99.9%confidence intervals and conservative reference-normalized envelopes both
separate from the prior independent confirmation.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Prior RGB8 Paeth | 2,860.610 | 1.085 / 1.104 | 3,448 |
| Current RGB8 Paeth | 2,311.766 | 0.880 / 0.882 | 3,448 |

Three independent short-lane recurrences compute the exact PNG 9.4 predictor,
including tie order and byte overflow. Bounded four-byte reads supply three
channels; stores touch only those three bytes. Scalar tails and portable,
overlapping-row and truncated-neighbor fallbacks remain. No buffer, cache,
owner, unsafe code or memory requirement is added. Cancellation checks cover
long rows at intervals of at most 65,535 bytes.

Only the RGB8 Paeth SIMD branch selects the new kernel. Other generic branches
encounter one negative dispatch predicate and retain their algorithms/storage;
existing None/Up/Sub/Paeth4/6/8/Average fast returns precede it. Encoders, other
codecs, editing, admission and retained ownership are unchanged. Preserve their
applicable evidence rather than rebuilding or retiming unrelated cases.

A fresh three-actual original/candidate screen passes the strict5%gate. One
candidate12actual confirmation reuses the PPH1original12, giving19.186%filtered /
19.174%unfiltered and18.917%normalized point improvement. Original packets,
native settings, complete decode/normalization/owned output/disposal, exactRGBA/
alpha/hiddenRGB and source/owner gates remain. All raw/brackets/unfiltered data
and idle-v2 guards are retained. Native allocations are unmeasured.

All 5,107 checks/OS pass (2,024 internal, 2,135 public, 948 live), plus 24 kernel
and 231 public checks normal/scalar per OS. Thirteen new cases cover all 256³ predictor triples,
ties/overflow, wider-load safety, tails/unaligned/first rows, overlap, truncation
and cancellation. The same actual Release package and three examples pass both OSes.
One core build/OS, zero harness/native/selector builds; unchanged binaries reused
throughout. Published releases remain immutable. At this checkpoint, 21 existing
PNG costs needed refresh; this scoped pass does not complete the 268-path objective.

[Evidence](warm-png-rgb8-paeth-simd-evidence.json) and
[prior source-layout costs](CURRENT-PNG-PAETH-SOURCE-COSTS.md).
Mirrored/member-verified archive png-rgb8-paeth-simd-qualified.tar.xz, 135members,
SHA256 29a90db3cdf985ad7b594dd4aafbf24c75a47b1f1f322825b43824b2290d858b; reproduction dependencies are pinned by hash.
