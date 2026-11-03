# Compact WebP matching: inconclusive

Recycle the forward head table's admitted token storage after its last lookup,
then compare exact green/alpha UInt16 samples during reverse matching. Eligibility
requires every red/blue sample to be constant; full-color inputs retain UInt32
queries. Full-color hashing, chain/search order, pruning probes, lengths, distances,
ties and complete packets remain unchanged. One specialized generic scheduler
shares both widths, with bounded safe BCL compact-prefix comparisons.

| Complete existing control, three actuals | Before ms / native ratio | Candidate ms / native ratio | Allocated B/op, both | Exact packet B |
| --- | ---: | ---: | ---: | ---: |
| VP8 + lossless alpha | 68.813 / 1.2080 | 69.601 / 1.2217 | 8,146,632 | 98,590 |
| Lossless photograph fallback | 86.651 / 0.9916 | 87.873 / 0.9983 | 6,516,744 | 160,032 |

These short points establish no useful gain and no allocation benefit. Retain the
adverse observations and restore without independent confirmation or favorable
resampling. They do not establish a causal regression or permanent family ban.
Preparation remains inside timing; all original inputs, settings, single-CPU,
native brackets, quiet guards, source/output and ownership checks pass.

A preceding count-only audit establishes capacity feasibility: the 1,048,576 B
head table ends before reverse queries, allowing 265,734 B of exact two-channel
samples in the same charged seed. Green alone is invalid because prediction
produces varying alpha residuals. This adds no allocation, owner or reservation;
capacity calculations are not measured physical peaks or CPU savings. Retained
scratch stays zero; no new peak measurement is claimed.

Windows/Linux each pass 820 internal and 194 public WebP checks in both normal
and scalar modes. New comparisons cover full-index equivalence, varying/nonzero
R/B fallback, arbitrary alpha bits, pruning/tails/overlap, storage transfer and
cancellation. Existing refusal/recovery and frozen packet checks pass. One core
and one internal-test build per OS; zero worker/native/selector/package builds.

All production sources, original tests, actual DLL/PDB/XML and the Linux source
manifest are restored byte-exact; candidate-only tests are removed. Unchanged
HSP200's 5,541 full/live checks per OS and the same actual Release package/three
examples remain valid, without broad repeats or package regeneration. Current
qualified benchmark costs stay unchanged. Next, the current opaque encoder's
attribution guides an independent scoring audit.

[Evidence](webp-compact-matching-inconclusive-evidence.json) pins producers,
complete observations, checks and restoration. Raw
`webp-compact-matching-inconclusive.tar.xz`: 2,321,104 B / 266 members, SHA256
`a54e9791512040c6fcb05b055f0be1ce64661596d46477a40ea3d821317aec19`, mirrored/member-verified. It depends on
the retained histogram-separation qualification and alpha capacity audit, whose
source/tool identities and hashes are pinned in the evidence.
