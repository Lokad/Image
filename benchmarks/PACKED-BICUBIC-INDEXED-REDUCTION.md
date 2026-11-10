# Indexed packed bicubic reduction

PIF750 isolates the ordered indexed horizontal dot product in a leaf helper.
Its accumulator remains in a register until return; repeated/clipped offsets,
separate multiply/add, unequal tails, coefficients and pixel contracts remain.
The existing contiguous path, scheduler, certificates and precision repair are
unchanged. Count-sized typed views borrow the current decoded row; no new
capacity, cache/session, owner or retention is introduced.

| Complete 513×259→64×32 public resize,4KiB workspace / labeled zimg | Managed µs | Native µs | Ratio / upper | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 | 428.98 | 366.56 | 1.170 / 1.177 | 113 |
| RGBA16 | 416.24 | 387.97 | 1.073 / 1.086 | 113 |

One contemporary original12-actual confirmation measures450.28/native370.37µs.
The independent candidate428.98/native366.56µs saves **21.30µs /4.73% raw /3.74%
normalized**, with separated raw/normalized filtered/all-actual confidence and
native−1.03%. The earlier458.14µs source-valid result is historical, not a causal
gain baseline. Both cases remain demonstrated gaps; word's refreshed cost does
not establish a causal companion gain/equivalence against its older clock context.
No additional favorable sampling. The promising422.14µs short is retained.

Managed source reads, preparation,4KiB workspace, independent output and disposal
stay timed. Native zimg retains its graph/scratch under the existing declared
halo/conversion/repack/ownership boundary; this is not fresh graph construction.
Quiet single-CPU/idle-v2 guards, source/ownership, exact rational/frozen managed
pixels and original native RGB/alpha max1/RMSE≤0.25 gates pass. TotalGC113B/op
is unchanged. Admission4096B still covers3880/3992/4044B phase payloads; independent
8192/16384B outputs stay charged until disposal, scratchretention0. Existing pool
retention/governed bypass remain separate. Physical/native/JIT-stack peaks are
unmeasured; removing logical stack traffic is not an allocation claim.

[Attribution](PACKED-BYTE-PREPARATION-ATTRIBUTION.md) identifies per-tap512bit
sum spills in the indexed fallback. Its two clipped pairs have16,576shared taps,
2,121,728logical read/write bytes; these counts alone predict no CPU gain.
Actual final byte/word prefix/suffix helpers204/210/204/213B retain separate
Mul/Add, no loop accumulator stack traffic and one final vector store. The
contiguous byte181/178B instructions match the qualified original. Complete
timing includes the new call boundary; no blanket inlining rule is proposed.

The production helper has a new focused scalar-bit oracle over16count/depth/
storage cases, zero/odd/long counts, repeated/reversed/clipped offsets, uint
products aboveInt32Max, poisoned out-of-count values, unchanged guarded input and
zero allocation. Existing full-layout, unequal-edge, loan-retirement and shared
root/refusal/cancellation/recovery coverage remains. Internal marker visibility
allows direct tests; there is no public API or forwarding wrapper.

Final **6154full/live checks per OS**,328internal/729public resize checks in
normal/scalar/AVX2-off/AVX512-off modes, and SAME actual Release package/allthree
consumer examples pass Windows/Linux. The unchanged package consumer exercises
both4KiB reductions, frozen pixels and aggregate/surviving ownership. ISA-specific
kernel bodies run when supported; portable fallback qualification remains.
One immutable main/embedded version perOS, no production compile failure,
benchmark image/settings/harness/native/selector build or repeated package proof.
Unchanged codecs/Lanczos/other editing evidence is reused; this is not a127-row
release refresh or completion of283required paths. Publication stays postponed.

[Evidence](packed-bicubic-indexed-reduction-evidence.json) owns current costs,
proofs and qualification. Private0.2.1 package SHA-256 `d17ee831cd52f025a93cf7b4892afa948f8d80e63e37f582f33a3872d18cfacc`
contains timed DLL `4808293283f6665f8a7a0455127322e5c79847fb449e4cc5544ff1df9ce95f1c`; published packages remain immutable.
Raw `tmp/packed-bicubic-indexed-reduction.tar.xz`, SHA-256
`c132b88f7e08c7beec1560012c196915ccfaa9f1c4de26be8fa0efde5788ba8d`,6,682,584bytes/150members, is mirrored and
member-verified. [Contiguous views](PACKED-BICUBIC-CONTIGUOUS-VIEWS.md) and
[shared scheduling](PACKED-BYTE-WORD-CONSOLIDATION.md) retain useful earlier gains
and adverse history. Larger narrow/cached-wide gaps precede further tuning here.
