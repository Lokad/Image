# Grouped color-source preparation: inconclusive

Prepare weighted R/G or RGB tuples once per color-search plane, then transform
each distinct tuple instead of every source sample for every coefficient trial.
The at-most64-sample path borrows768 unused bytes from the opposite plane's
products, beside the existing trial cache/active prefix. A96-slot open-addressed
table compacts in place; large tiles retain the original histogram. There is no
new array, pool, reservation or retained scratch.

| Existing complete direct encode | Before → candidate µs | Allocated B/op, unchanged | Admitted peak B, unchanged |
| --- | ---: | ---: | ---: |
| photograph | 90,773.01 → 92,322.42 | 13,346,256 | 10,166,464 |
| diagram | 8,889.48 → 9,219.45 | 6,255,920 | 4,802,292 |
| alpha | 62,368.36 → 63,045.06 | 10,747,848 | 3,847,888 |
| authored-bars | 789.18 → 826.66 | 933,440 | 312,716 |

The separate original lossless control is **128.459 → 131.290 ms**
in the existing three-actual short job (-2.20% raw gain point).
Native libwebp exact=1 brackets average
120.181 → 120.996 ms;
the normalized point is -1.52%. Allocation remains24,899,424 B/op,
and the complete183,812-byte packet is exact. No useful complete-operation gain
or storage saving earns this added preparation/weighted-lookup machinery.
These are continuation screens, not independent confirmations or causal
regression claims. Retain all adverse points; no scored rerun or further tuple
variant follows. The transparent path has no color-search phases.

The [preceding count audit](WEBP-COLOR-PRODUCT-CACHE-INCONCLUSIVE.md) verifies
194,146 independent weighted histograms. It predicts1,728,049 fewer transformed
visits for the photograph and3,046,870 for the original control; these counts
did not establish a speedup. Hash preparation, compaction, multiplicity loads,
active-bin updates and actual memory access all remain complete timed work.

Candidate normal/scalar checks pass299 per mode on Windows/Linux. Four new
checks exercise an independent dense signed-byte/floor/modulo oracle, every
coefficient, clipped/axis tiles, alpha variation,64 colliding keys including zero,
termination/compaction guards, unchanged source/tuple storage, cancellation and
zero warm helper allocation. Existing frozen maps, numeric scores, packets,
refusal/cancellation and ownership pass. Screens preserve frozen source/packets,
independent outputs, root cleanup and quiet guards.

Restore source and DLL/PDB/XML/test assets **byte-exact on both OSes**: six
Windows assets and seven Linux assets including the source manifest. Remove
the candidate helper and its dedicated tests. Zero restoration builds; the actual
package is unchanged. Reuse [5,497 full/live checks per OS and the same actual
Release-package/three-example proof](RESIZE-OVERWRITE-OUTPUT-STORAGE.md).
Current costs and qualification remain valid; no broad/package rerun is needed.
This rejects an implementation, not future materially revised preparation.

Next, audit actual clustering-population scans and omitted-bank support costs.
Do not replay either color lookup merely to seek a favorable observation.

[Evidence](webp-source-tuple-preparation-inconclusive-evidence.json) retains
producers, screens, all original short actuals/native brackets and restoration.
Raw webp-source-tuple-preparation-inconclusive.tar.xz: 7,796,260 B /285 members,
SHA256 114c66e2e8ac6fcdc8710dcabd6ff116da650c389f98da00093175f1342bf706, mirrored/member-verified.
The original count/source and qualified-production archives remain pinned.

A subsequent unchanged-production clustering audit observes four candidate
families across the same six warm/observed complete encodes. Photograph green
sums visit6,286,840 bins with1,848,266 nonzero; original-control green sums
visit32,438,304 with15,500,456 nonzero. The two transparent families visit
2,295,040/98,280 green bins with423,545/7,459 nonzero. Every prospective sparse
cost equals the original exact run model. These are work counts, not savings.
Distance alphabets are short and mostly dense; existing color support is already
effective. Next prototype green support in the existing live-population owner,
including preparation, replay, merge and capacity costs. Additional green payload
would be85,800/360,360/25,704/6,120 B for the four observed families; this is a
capacity calculation, not measured allocation or whole-operation peak.
The audit has one private projection build and zero production/harness/native
builds. Exact packets, sources, root cleanup and quiet guards pass. Raw
webp-clustering-support-audit.tar.xz:456,564 B /28 members,
SHA256d4c96779e9a39890b651bfd8be23ce17e203425ee14b330afc2253bcde6b4af9,
mirrored/member-verified; full counts/replay are pinned in the evidence.
