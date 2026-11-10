# Packed bicubic byte preparation

Complete RGBA8 bicubic 513×259→64×32 with 4 KiB admission saves
**13.99 µs / 3.33% raw**, 419.58→405.59 µs. Filtered/all-actual raw and
reference-normalized confidence intervals separate. The normalized point gain
is 11.11%, with native movement **+8.75%**; it is not the raw elapsed saving.

| Complete frozen control | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8, 4 KiB | 405.59 | 398.21 | 1.019 / 1.028 | 113 | gap |
| RGBA16, 4 KiB | 422.81 | 397.48 | 1.064 / 1.098 | 113 | gap |

One contemporary original and one independent final byte comparison retain
twelve actuals each. The promising 407.71 µs short remains a screen. Refreshed
word costs establish its current status, without a causal companion gain,
regression or equivalence claim. No additional favorable sampling follows.

## Shared preparation and exactness

One byte product helper prepares four RGBA pixels in UInt16 lanes. Every color
product is at most 65,025; alpha retains its original sample. The existing
prefix widens unsigned products exactly to Double; the suffix stores the same
packed products directly. Two-pixel/single tails and full UInt32 word products
remain. Both existing modes use this bulk block within their shared executor.

The [prior attribution](PACKED-BYTE-PREPARATION-ATTRIBUTION.md) models
137,788 two-pixel batches becoming 68,894 four-pixel batches, with 518 single
tails. These are iteration counts, not phase costs or predicted CPU savings.
No profile, model, fixture, harness, native worker or selector rebuild is repeated.

Removing the helper and bulk block reconstructs the original source exactly.
Geometry, ordered horizontal/vertical filtering, coefficients, clipped offsets,
certificates, exact repair, admission, ownership and cancellation are unchanged.
Actual byte code uses UInt16 multiplication/shuffles and unsigned widening.
Word source is unchanged but its generated bodies differ by six bytes, so both
complete costs refresh. Other codec/resize/text evidence remains source-valid.

One new exhaustive test checks every byte color/alpha pair across four distinct
pixel/channel lanes, unsigned products, preserved alpha, guards and zero allocation.
Existing actual prefix/suffix, odd/partial/clipped layouts, poisoned result storage,
tiny-alpha unwind, shared refusal/cancellation/recovery and independent surviving
outputs remain covered. No benchmark input/settings matrix is added.

No owned capacity, owner, pool, session, root or public API is added. Working
admission stays 4 KiB, with existing explicit phase payloads 3,880/3,992/4,044 B.
Total warm allocation stays 113 B/op. Independent byte/word result capacity is
8,192/16,384 B until disposal; scratch retention is zero. Output-pool retention
and governed bypass remain separate. Physical/native/JIT-stack peaks are unmeasured.

## Qualification and operation boundaries

Windows/Linux each pass **6,181 full/live checks**, 350 internal/729 public resize
checks per normal/scalar/disabled-AVX2/AVX512 mode, and the **same actual Release
package/all three usage examples**. One successful main/embedded build per OS
serves the round; broad/package qualification runs once per OS. An initial internal
compile catches a local-name collision before producing an executable; the exact
attempted source and failure are retained. Unchanged JPEG/other ISA evidence is reused.

Complete managed timing includes source reads, geometry/storage preparation,
filtering, independent output and disposal. The explicitly labeled FFmpeg zscale
zimg backend retains graph/input/scratch; halo, packing, filtering and independent
output/release remain timed. This is not equivalent fresh-graph work. Exact managed
rational pixels remain distinct from approved native bounds. Frozen pixels,
source/ownership and quiet idle-v2 single-CPU gates all pass.

[Evidence](packed-bicubic-four-pixel-preparation-evidence.json) records the
comparison, current costs, source/code checks and qualification. Mirrored and
member-verified raw `packed-bicubic-four-pixel-preparation.tar.xz`:
6,829,440 B / 163 members, SHA256
`ea62895c444987b31495ed85de986e508eacb077e57060c5afa79e42db9ee5fa`.
Private 0.2.1 package: 443,912 B, SHA256
`754785736730b2d9aeeed90e8be68fe7df85bea5c48f33b73fcad13b0ba2bbd0`.
Published releases remain immutable; publication is postponed. All 283 material
paths remain required. Both packed cases remain gaps; the full goal is open.
