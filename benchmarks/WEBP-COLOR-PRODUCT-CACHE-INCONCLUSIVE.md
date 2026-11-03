# Phase-local color product cache: inconclusive

A64-slot direct cache reuses exact medium entropy products during color-transform
trials. It borrows768 unused bytes from the opposite plane's products for tiles
with at most64 samples, beside the existing trial cache and active-bin prefix.
Larger tiles and small/large arguments retain their original arithmetic. There
is no new array, pool, owner or admission request. History/plane changes reset keys.

| Existing complete direct encode | Before → candidate µs | Allocated B/op | Admitted peak B |
| --- | ---: | ---: | ---: |
| photograph | 92,298.13 → 94,601.18 | 13,346,256 → 13,346,256 | 10,166,464 → 10,166,464 |
| diagram | 9,324.83 → 9,238.99 | 6,255,920 → 6,255,920 | 4,802,292 → 4,802,292 |
| alpha | 62,885.33 → 63,114.82 | 10,747,848 → 10,747,848 | 3,847,888 → 3,847,888 |
| authored-bars | 824.76 → 823.51 | 933,440 → 933,440 | 312,716 → 312,716 |

The target photograph point is **2.50% slower**; other mixed points and the
unchanged alpha path are retained. These are three-batch diagnostic observations,
without native brackets or statistical confidence. Do not infer a causal
regression or a gain from the other points. No twelve-actual confirmation,
scored rerun, new workload or further cache variant follows. No scratch survives.

The preceding count audit suggested320,041 photograph and54,207 original-control
reuses. Actual simulated hit rates are39.65%/49.66% for photograph red/blue and
8.08%/56.03% for the original control; misses, collisions and per-phase tag clears
are real costs. The transparent control has zero color-search phases. These
counts motivate investigation, not a speed claim. Eliminating repeated arithmetic
did not earn this implementation's added lookup/initialization machinery.

Candidate normal/scalar checks pass297 per mode on Windows/Linux, including
all65,536 frozen numeric scores with cached/repeated evaluations. Two added
checks prove all64 active bins, colliding keys, poisoned phase storage, small/
medium/large boundaries, guard preservation and zero warm helper allocation.
Existing maps, residuals, complete packets, cancellation/refusal and ownership pass.
The full screen preserves frozen packets, source pixels, root cleanup and guards.

Restore source/DLL/PDB/XML and original test binaries **byte-exact on both OSes**,
with zero restoration builds. Remove candidate-only APIs/tests. Reuse unchanged
[5,497 full/live checks and the same actual Release-package proof](RESIZE-OVERWRITE-OUTPUT-STORAGE.md);
no broad, package or benchmark refresh is needed for restored production. Current
summary costs remain unchanged. This does not permanently exclude materially
revised color-search preparation.

[Evidence](webp-color-product-cache-inconclusive-evidence.json) pins the design,
both immutable producers, all observations, adverse points and restoration.
Raw webp-color-product-cache-inconclusive.tar.xz:3,183,504 B /115 members,
SHA2569baf689b7204f2f4c3486aadf1fce84e5334105069a8b1a383a9fa2fde12cb01,
mirrored/member-verified. The count/source dependencies remain pinned.
A subsequent unchanged-source audit verifies194,146 weighted histograms exactly.
Grouping source pairs/RGB tuples would remove1,728,049 photograph visits and
3,046,870 original-control visits; median unique counts are42/53 and20/37 for
red/blue phases. This is a larger work-removal lead, not a speedup. Its source,
all six complete packet/source/root checks and replay are retained in
webp-source-tuple-audit.tar.xz,514,036 B /26 members,
SHA2566c9dc6727116bb4bfcbd45037f45dfbe6e5ca11241c5a8e2e2a6c03e0a016124,
mirrored/member-verified. Prototype weighted source preparation separately;
do not replay this product cache for favorable samples.
