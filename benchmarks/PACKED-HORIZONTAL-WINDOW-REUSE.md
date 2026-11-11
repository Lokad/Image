# Bounded bicubic horizontal window reuse

The existing complete RGBA8 513×259→64×32 Resize saves **14.21 µs /
3.76% raw / 3.68% reference-normalized** against the contemporary
immutable predecessor. Filtered/all-actual raw and normalized confidence intervals
separate; native time moves -0.08%. Byte parity passes and stops.
The word comparison remains **uncertain** across its retained timing views.

| Complete operation / declared sampling | Lokad / zimg µs | Ratio / conservative upper | GC B/op | Current interpretation |
| --- | ---: | ---: | ---: | --- |
| RGBA8, 12-actual global brackets | 364.04 / 372.99 | 0.976 / 0.982 | 113 | pass/stop |
| RGBA16, 12-actual global brackets | 386.57 / 378.04 | 1.023 / 1.027 | 113 | uncertain |
| RGBA16, 12 adjacent pairs ×64 operations | 397.04 / 410.52 | 0.967 / 0.979 | 112 direct | scoped pass; broader word remains open |

These are complete public Resize operations, including geometry/preparation,
every selected source sample, exact certification/repair, independently owned
output and disposal. The explicitly labeled zimg 3.0.6 backend retains graph and
scratch as before. Native float/sample bounds, normalization/packing/ownership,
frozen inputs/settings/full-frame hashes and one-CPU idle-v2 guards are unchanged.
No new images, geometry/depth/settings grid or material public path is introduced.

Four overlapping 32-tap/+8 windows previously reread 128 decoded pixels. Seven
fixed ascending source bands read their 56-pixel union once, updating only active
columns. Each of four Vector256 sums retains its original t0..t31 Add(Multiply).
Existing 33-slot column-major weights, offsets, counts, normalizers and exact
repair are unchanged. No coefficient transpose, copied matrix, delayed-window
array or new working storage is needed. The producer writes directly into the
existing borrowed cache, avoiding two temporary 64 B return transfers.

Existing contiguous-pair proofs and one per-strip check qualify four equal 32-tap
windows at offsets 0/8/16/24. Unequal/count33/clipped/short/shifted/odd/suffix and
portable paths keep their original bodies. The same producer serves decoded
byte/word prefixes. Canonical eligible row groups are 1,554/2,331, not phase
times or a gain forecast. This revises the earlier unselected direct-cache pair
trial: it removes repeated source reads as well as return transfers.

The word short screen 356.07/native 387.42 µs was optimistic. Independent global
confirmation gives 386.57/378.04, upper 1.027. Its raw 49.31 µs difference
includes native −9.18%; normalized 2.34% intervals overlap. No causal word gain is
claimed. The actual numeric instructions match between both hosts: 565 B, 16 B stack
reservation, no vector spills/FMA. Code size alone establishes no speedup.

One bounded diagnostic changes adjacency and uses the same 64 complete operations
on each side, alternating order. All 12 pairs and before/paired/after native
brackets contribute to the conservative bound. It passes its scoped view, but
does not erase the uncertain long-batch observation or establish uniform warm
word parity. The existing owned driver changes only its harness allowlist;
no production rebuild, reference-setting change or favorable repeat follows.
Both views remain explicit; broader word confirmation stays open.

Total GC 113 B/op and direct diagnostic 112 B/op are different instrumentation
boundaries, not an allocation gain. Working admission remains 4 KiB, with the same
3,880/3,992/4,044 B explicit phase payloads. Independently owned results retain
8,192/16,384 B until disposal; operation scratch retention is zero. No new array,
owner, pool, reservation or retained capacity is added. Physical/native/JIT
peaks remain unmeasured. Existing default idle-pool/governed-bypass behavior stays.

Sixteen focused cases cover exact ordered-double bits at both depths/low alpha,
poisoned coefficient padding and guards, geometry rejection, zero allocation,
pre-cancellation and word precision-decline cleanup/public replay. Original
prefix/suffix, odd/clipped/poisoned result loans, root refusal/cancellation/recovery
and surviving-owner checks remain. Windows/Linux each pass **6,417 full/live checks**,
508 internal/789 public affected checks per normal/scalar/AVX2-off/AVX512-off mode,
including all three named storage suites. The SAME actual Release-package
consumer/all three usage examples pass both OSes. One core/embedded build per OS,
one diagnostic driver build, zero production/test compilation failures and one
full/package milestone; unchanged codec/text ISA evidence is reused.

Package `86c09ae78a81b22d64291c7bf94f5cb5e68c873a5de69bc265b98b540cb1f4e6` contains timed DLL `be95c47fbc66c5c58c83ae9f5b38d7ada81b8485055913caf689ddf04783267c`.
Published releases remain immutable; private 0.2.1 qualification is not publication.
[Evidence](packed-horizontal-window-reuse-evidence.json). Raw `packed-horizontal-window-reuse.tar.xz`:
2,273,912 B / 165 members, SHA256 `6413c4d2d752a1777f7b4273aa0bae2ac99dd0a6bda9438d1ff468d180dacf0a`,
mirrored/member-verified with retained predecessor/profile/reference dependencies.
[Earlier byte preparation](PACKED-BICUBIC-FOUR-PIXEL-PREPARATION.md) and
[word attribution/direct-cache trial](PACKED-WORD-CURRENT-ATTRIBUTION.md) remain
history. This report owns current packed costs. All 289 material paths remain
required; publication is postponed. Next: a demonstrated independent bounded
byte gap, with uncertainty resolved before any further word tuning.
