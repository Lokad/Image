# Shared RGBA predictor histograms: rejected

The fresh photograph screen misses the predeclared 5% continuation gate:
**1.65%** less elapsed and **0.57%** native-normalized point reduction, with
overlapping intervals. The candidate is removed without variants or confirmation.

| Existing complete 513×259 encode | Mean µs | Ratio / upper | Allocated B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: |
| Original, three actuals | 107,092.948 | 1.291 / 1.516 | 20,418,338 | 160,032 |
| Rejected shared histogram | 105,321.801 | 1.283 / 1.436 | 20,418,338 | 160,032 |

One borrowed-storage helper shared neighbour loads and exact packed-byte floor
averages across all fourteen predictor modes. Select/clamp decisions, nested
rounding, all channel histograms, borders/right wrap and per-pixel cancellation
remain exact. Green-only analysis is unchanged; no capacity, buffer, owner or
admission mechanism is added. The retained profile is diagnostic; 29 relevant
source identities allow reuse without another profile or baseline confirmation.

The fixed photograph, effort4/quality75 and explicitly labeled libwebp exact=1
reference remain. Complete encoding includes normalization, independently owned
packet acquisition and disposal. Exact complete packets/reconstructed RGBA,
alpha/hidden RGB, native normal/scalar CLI, source/owners and idle-v2 gates pass.
Short screens establish no accepted gain, parity or historical allocation claim.

Both OS pass 22 internal and 118 public focused checks normal/scalar. Two new
independent scalar-oracle cases cover every-mode histograms, channel carries,
ties/clipping, partial tiles/edges, guards and cancellation/borrowed-root recovery.
They are removed with the candidate. Qualified source, test assemblies and
DLL/PDB/XML are restored; 20 restored internal checks pass normal/scalar per OS.
The unchanged 5,153 checks/OS, same actual package and three examples remain valid.
One core build/OS, zero harness/selector/native builds, no repeated full/package
qualification. Accepted costs and all 268 required paths remain unchanged.

[Evidence](vp8l-shared-predictor-histogram-rejected-evidence.json) retains all raw/
filtered samples, native brackets, exact proofs, identities and restoration.
Mirrored/member-verified archive vp8l-shared-predictor-histogram-rejected.tar.xz, 135 members,
SHA256 163a64e06a202c516d4189e624c4df5f944a55f381ad644dd9fd171b033161ca; reproduction dependencies are pinned by hash.
