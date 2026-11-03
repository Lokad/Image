# Compact green predictor storage: inconclusive

Two coherent stages preserve exact output but show no useful complete-encode CPU
gain. Restore the qualified implementation; neither result permanently excludes
a revised layout or preparation design.

Stage one compacts each green-only predictor's four-plane histograms into one
green plane and two scalar history counts. It retains the alpha zero/one entropy,
red/blue singleton bias, hierarchy costs and all mode ties. Stage two also prepares
the two possible non-green channel prices once per tile, sharing one price across
the thirteen nonzero modes. Both use one operation-owned analysis buffer, without
additional arrays, pooling, new admission mechanisms or lower-memory retries.

| Existing complete direct control | Before → stage one µs | Before → stage two µs | Allocation change B/op, both stages |
| --- | ---: | ---: | ---: |
| Photograph | 90,656.77 → 91,736.84 | 92,540.80 → 92,555.22 | +16 |
| Text/diagram | 9,388.09 → 8,981.40 | 9,275.43 → 9,265.36 | 0 |
| Transparent VP8 | 63,138.76 → 63,168.08 | 63,429.00 → 63,457.52 | −92,112 |
| Small authored control | 784.82 → 826.88 | 833.96 → 836.78 | 0 |

Each stage uses a fresh qualified before/candidate pair, three direct batches and
the unchanged four-control public-operation harness. The transparent points are
0.046%/0.045% slower. These are diagnostic observations without native brackets
or statistical confidence; no causal gain or regression claim. The other mixed
points and all actuals are retained. No twelve-actual confirmation, new workload,
scored rerun or further variant follows.

Each full-size analysis reduces admitted capacity from 62,161 to 16,089 B.
Two calls remove 92,144 B of array payload and 13,160,448 bytes of histogram
clearing; larger analysis objects offset 32 B in total allocated savings. This
does not lower the complete transparent operation's 3,847,888 B admitted peak.
Other direct peaks stay unchanged. No scratch survives disposal. The modest total
allocation reduction, unchanged peak and flat target points do not justify the
added layout/history machinery. Cache-set locality was a hypothesis, not a
demonstrated speedup.

Both candidates pass 296 affected checks per normal/scalar mode on Windows/Linux,
including 16,384 frozen numeric scores, 8,613 semantic tile histograms, maps,
residuals, clipped parents, refusal/cancellation and shared-root recovery. Packets,
sources, settings, ownership and quiet guards pass in both complete screens.

Production source/DLL/PDB/XML are restored byte-exact on both OSes with zero
restoration production builds. Candidate-only APIs and the compact-admission test
are removed. Four useful border/axis/parent regression cases remain against the
actual qualified implementation: 295 affected checks pass in both modes per OS,
with one test-only build per OS. Reuse the unchanged [5,483 full/live checks and
same actual package/three examples](WEBP-INVARIANT-ALPHA-SCORING.md); the four new
cases are focused evidence, not a claim that a new full suite ran. Package and
current benchmark costs are unchanged.

[Evidence](webp-compact-green-inconclusive-evidence.json) pins both designs,
immutable producers, original controls, complete results and restoration. Raw
webp-compact-green-inconclusive.tar.xz: 2,005,752 B / 201 members,
SHA256 c604040410e9c73fda0eea72b353c61e23da4d216fcc1ec65d0a88923ce87499,
mirrored/member-verified. The qualified source/package dependency remains pinned.
Next, reuse applicable VP8 opaque-photograph attribution to investigate the
larger full encode gap; do not retune this flat green-layout implementation.
