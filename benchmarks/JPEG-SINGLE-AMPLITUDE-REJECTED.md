# Rejected JPEG single-coefficient lookup

Predecoding small AC amplitudes in existing admitted Huffman storage misses the
4.1% screen threshold on the existing 513×259 direct-RGB444 complete decode.
Reject the candidate; current production and qualified costs remain unchanged.

| Complete decode | Lokad µs | Ratio / upper | Managed B/op |
|---|---:|---:|---:|
| Qualified original, independent12 | 2,098.407 | 1.449 / 1.458 | 966,367 |
| Rejected candidate, short3 | 2,079.179 | 1.434 / 1.585 | 966,367 |

Point gains are **0.92% filtered /
0.98% all-actual /
1.10% native-normalized**; allocation is unchanged.
One active short, no independent confirmation or sign-width/table/prefix/layout/
inlining variants. These short results establish no speedup or parity claim.

Canonical short prefixes occupy [0,L); real nine/ten-bit entries start at 4L in
the existing long-code table. Single signed-four-bit amplitudes use untouched
slots with a zero low length tag, so the original long fallback ignores them.
The finite 1,248-case packing proof and all 256 coverage bounds pass. The frozen
control covers 93.279% of symbols; coverage alone did not produce a material
complete-operation gain. Earlier preparation designs were superseded before
implementation; the final zero-tag proof describes this sole candidate.

Original 133,057-byte input, settings, exact managed/native/scalar pixels, source,
independent output ownership, all actuals/native brackets and both idle-v2 guards
pass. Full decode/normalization/fresh output and release remain timed on one CPU.
Native allocation is unmeasured; B/op includes harness costs.

No capacity, field, owner, charge or allocation is added. Fifty internal and all
388 public checks pass normal/scalar Linux, including unchanged admission and
cancellation requirements. One core/internal build; zero other builds or profiles.
All source/core/PDB/XML/linked tests restore byte-exactly. Restored 47 internal/
388 public checks pass normal/scalar; three tests dedicated to the removed
implementation are removed. Windows production/package stay unchanged. Applicable
full Windows/Linux and same actual Release-package/three-example proof are reused.
No repeated full/package run follows rejection.

[Evidence](jpeg-single-amplitude-rejected-evidence.json) retains samples, proof,
counts, costs and restoration. Mirrored/member-verified `jpeg-single-amplitude-rejected.tar.xz`:
1,824,892 B / 110 members, SHA256
`f4f8687e4213e9dbb9e423693c5456ba656c4035354f0ce6164dd06a9ca44375`; J4N31/DRGB31 dependencies remain retained.
The full 268-path objective stays open; close this primitive.
