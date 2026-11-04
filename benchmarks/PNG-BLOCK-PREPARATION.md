# Operation-owned PNG block preparation

Canonical code assignment now builds width counts/start codes and assigns in
symbol order, replacing fifteen alphabet scans and per-bit reversal. The existing
tree workspace also owns 335 frequency ints and 320 code ints reused by every
block. Code-length codes finish before literal codes borrow the same prefix;
literal/distance codes coexist through token emission. Independent length arrays,
tree ordering, block selection, packets and output publication are preserved.
Borrowed code prefixes and block frequencies clear before reuse. No new owner,
pool, session, cache, wrapper or production dependency.

At identical short batching, complete RGBA16 Smallest9 allocation falls
**148,320 B (5.7%)** and RGBA8 Fastest1 falls **19,936 B (2.4%)**. Stage1's code
construction kept the prior allocation path; stage2 removes recurring block
arrays. Word/byte shorts batch 8/48 operations; current confirmations batch 4.
Small B/op differences reflect harness amortization, not additional savings.
This is a storage/preparation milestone, without a CPU gain/equivalence or causal
regression claim. Stage1 alone had no useful complete-operation CPU lead; its
adverse/noisy screens remain, without unchanged confirmation or favorable replay.

| Existing complete 513×259 one-shot control | Managed µs | FFmpeg µs | Ratio / upper | Allocated B/op | Exact packet B, both | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| RGBA16 Smallest9 | 23,814.57 | 22,718.51 | 1.048 / 1.065 | 2,437,652 | 963,202 | gap |
| RGBA8 Smallest9 | 19,417.24 | 18,696.87 | 1.039 / 1.049 | 820,444 | 210,973 | gap |
| RGBA16 Optimal6 | 23,400.62 | 22,554.07 | 1.038 / 1.047 | 2,437,652 | 963,202 | gap |
| RGBA8 Optimal6 | 17,723.94 | 17,294.12 | 1.025 / 1.045 | 820,444 | 210,973 | uncertain |
| RGBA16 Fastest1 | 21,689.76 | 20,151.55 | 1.076 / 1.085 | 2,437,652 | 968,216 | gap |
| RGBA8 Fastest1 | 5,940.17 | 4,734.88 | 1.255 / 1.276 | 820,908 | 236,828 | gap |

All six existing costs use twelve actuals on one immutable selected producer.
Filtered/all-actual confidence and both native brackets are retained, including
exclusions. Native clocks differ from earlier captures; do not infer CPU gains
from the raw points or allocation savings. Equivalent fresh codec/zlib/workspace/
packet preparation, source processing, independent output and disposal stay
timed. Word native endian-frame allocation/conversion/release is included.
Exact complete packets, samples/alpha/hidden RGB, frozen inputs/settings, quiet
guards and ownership gates pass. No new workload grid or broader speed claim.

Two arrays add **2,620 B** operation-retained payload; known fixed arrays total
281,504 B. Existing **384 KiB** admission is unchanged and precedes allocation;
retained scratch is zero after disposal. Reused code storage replaces transient
arrays at block boundaries. These capacities are not complete physical/admitted
peak measurements; native allocation is unmeasured. The existing conservative
32 KiB tree/code/stack allowance covers the expanded tree workspace; no budget
snapshot, fallback or independent allowance is introduced.

Both OSes pass **5,607 full/live checks** (2,372 internal, 2,287 public, 948 live),
plus 580 internal/211 public affected checks per normal/scalar mode. Four new
canonical controls cover the RFC1951 example, all widths, sparse/mixed trees,
independent binary-interval oracles and dirty borrowed views/bounds. Existing
frozen tree/code hashes, delivery/slide/overlap, cancellation/refusal/recovery,
packet and output ownership checks remain. Obsolete allocating code generation
is removed; production callers borrow actual workspace views.

The SAME actual Release package/all three examples pass Windows/Linux, reusing
the packaged exact PNG packet/384 KiB boundary, async surviving output, full-word
color packing and shared-admission consumers. Two core/internal versions per OS,
zero public/harness/native/selector/audit builds, one broad/package milestone.
Other families reuse unchanged-path proof; published releases are immutable and
publication remains postponed.

[Evidence](png-block-preparation-evidence.json) retains six current costs, both
stages, same-batch allocation comparisons, sources/producers and qualification.
Mirrored/member-verified raw `png-block-preparation-qualified.tar.xz`: 8,242,468 B /
363 members, SHA256 `85f6f2f28169e5bb79a3f7cfc9a28e17389a03a3b7bd5535731449e2f7f521db`.
Original/stage sources, immutable binaries/package, all observations and exact
reproduction remain; qualified baseline/reference archives are pinned.
[Earlier tree reuse](PNG-TREE-WORKSPACE.md) and [token output](PNG-TOKEN-OUTPUT.md)
retain their scoped history. Full 282-path CPU parity is open.
