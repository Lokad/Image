# WebP matching and luma reuse investigations

The current transparent-encode profile assigns 118/772 exclusive samples to
matching-index construction and only 8/772 to prefix comparison. Together with
the [earlier prefix trial](WEBP-MATCH-PREFIX-INCONCLUSIVE.md), this favors
investigating short hash-chain traversal rather than another SIMD prefix loop.
Samples are attribution, not elapsed-time estimates.

Three private count audits reuse four frozen complete encodes, each with one
warmup and one observed operation. Each audit compares every word of all 17
matching indexes with a separately compiled unchanged matcher, then verifies
complete packets, unchanged sources and shared-root cleanup. No timing or
allocation claim comes from these instrumented runs.

| Full-frame matching call, 513×259 | Original candidate visits | 1 MiB class heads | 64 KiB class heads |
| --- | ---: | ---: | ---: |
| Photograph VP8L | 959,929 | 207,003 | 585,655 |
| Opaque VP8L | 902,452 | 260,343 | 754,749 |
| Original VP8L | 267,958 | 126,694 | 241,460 |
| Transparent VP8, first alpha pass | 712,821 | 293,287 | 436,116 |
| Transparent VP8, second alpha pass | 627,401 | 378,437 | 378,437 |

Exact three-pixel class links skip only candidates unable to improve an existing
length of at least two. Original chain ranks preserve search-depth eligibility,
window, order and ties. Hash collisions and constant triples using run-length
keys retain the original traversal. The diagnostic arrays are not a production
admission design.

A production prototype uses the smaller table and borrows two already-admitted
words per pixel from the empty token owner after native pair heads finish.
Only the separate 64 KiB table adds allocation; it is reserved before allocation
and remains charged through actual users. There is no pool or output alias.

| Complete public encode, three quiet-VM batches | Qualified before µs | Candidate µs | Raw point | Added allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Lossless photograph | 91,004.13 | 91,833.81 | −0.91% | 65,560 |
| Lossless text/diagram | 7,496.99 | 7,721.43 | −2.99% | 65,560 |
| Transparent VP8 | 64,466.99 | 65,553.26 | −1.68% | 131,120 |
| Existing authored bars | 798.57 | 812.29 | −1.72% | 0 |

This unchanged screen includes all public admission, preparation, coding and
owned stream output. CPU0, tiering disabled, sequential execution and both idle
guards pass. Complete packets are exact. Total allocations increase; complete
admitted peaks remain 3,470,059 / 3,386,043 / 3,847,888 / 312,716 B respectively
because another phase sets each peak. Operation scratch retention remains zero;
physical/native heap peaks are unmeasured. Fewer visits do not establish a gain.

The screen shows no useful complete-operation improvement and establishes no
causal regression. Restore this implementation without favorable resampling or
independent confirmation. This does not prohibit a materially different design.
The larger diagnostic table is not a measured production candidate.

The prototype passes 315 focused checks in both normal/scalar modes on each OS,
including exact indexes, packets, admission refusal and cancellation cleanup.
Initial test dimensions and old fixed peak expectations were corrected without
rebuilding the production DLL; failed setup logs remain. Table-specific tests
are removed with the implementation. Eight useful actual-encoder cases remain:
token-capacity thresholds, quality/window boundaries, low effort, long runs,
collisions, unchanged input and independently surviving indexes. These and eight
existing handoff tests pass after restoration: 16 normal/scalar checks per OS.

Qualified WSP152 source and immutable DLLs are restored without production or
package rebuilds. Reuse its 5,507 full/live checks per OS and same actual Release
package/three-example proof on both platforms. Current comparison tables remain
unchanged. Next investigate repeated VP8 luma prediction/decision preparation,
using the existing profile rather than repeating this screen.

[Evidence](webp-chain-class-skip-inconclusive-evidence.json) pins identities,
counts, observations and restoration. Both raw archives are mirrored and every
member verified:

- `webp-chain-class-skip-inconclusive.tar.xz`: 1,861,940 B / 135 members;
  SHA256 `df1b8094860c79cc6c911c2e216729ac754d5f3c6688d3e5721116f654f8f7c4`.
- `webp-chain-class-skip-restoration.tar.xz`: 822,740 B / 20 members;
  SHA256 `ccf37dcc9dcccbe430effa87a4b50fb203fb905fd7eed6402787015975dcf268`.

Dependencies are the retained [WSP152 qualification](WEBP-SCORING-PREPARATION.md)
and transparent-profile archives, with full hashes in the evidence. No upstream
source, implementation tables or fixtures were copied.

An independent follow-up counts exact duplicate **nonuniform** luma predictions
on the existing photograph, diagram and transparent VP8 controls. It observes
one operation after one warmup per control: six complete frozen packets, unchanged
sources, shared-root cleanup and both quiet guards pass. Existing uniform reuse
is excluded from the proposed savings.

| Duplicate work among batched nonuniform trials | Quantization/rate | Reconstruction/SSD | Texture |
| --- | ---: | ---: | ---: |
| Photograph | 4,222 / 88,310 | 639 / 47,820 | 79 / 18,085 |
| Text/diagram | 8,400 / 34,610 | 911 / 9,971 | 175 / 5,871 |
| Transparent VP8 | 1,047 / 89,720 | 338 / 60,861 | 8 / 14,085 |

These counts establish no CPU or allocation gain. Only 1.17% of the expensive
transparent control's nonuniform quantization trials repeat; this does not
justify adding a per-block prediction cache for that gap. Diagram reuse remains
a lead. No production prototype or timing confirmation follows this audit.

Raw `vp8-luma-preparation-reuse-audit.tar.xz`: 442,844 B / 29 members, SHA256
`8f046e2f66d0917ca72161f883d1542612a39728e4df91b6d8a6745aabda01f1`,
mirrored/member-verified. The evidence includes the private source projection,
original decisions, counters, frozen packets and corrected collector build.
Production DLL/package identities and their qualification remain unchanged.

Next investigate shared coefficient syntax preparation for VP8 statistics and
emission. The current profile attributes 7.64% / 3.50% exclusive samples to those
methods; that is not wholly removable cost. The managed paths separately walk
selected coefficients. The external reference records bit instructions during
statistics and emits them later with final probabilities. This suggests a
bounded operation-owned stream, with instruction volume, live storage and exact
probability policies established before production changes. Source inspection
provides insight only; no upstream implementation is imported.
