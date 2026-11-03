# Current opaque lossless WebP attribution

One two-second quiet-VM capture uses the qualified histogram-separation DLL and
the existing complete 513×259 opaque RGBA8 encode. The complete 121,896 B packet,
source pixels, settings, ownership, native checks and both quiet guards pass.
There are no builds or production changes. Profiled elapsed is discarded; sample
counts and instruction skid are not component times or predicted savings.

| Location | Samples, out of 1,983 |
| --- | ---: |
| Color-histogram construction | 257 |
| Dense summed-population scoring | 182 |
| Full-color predictor histogram | 175 |
| Matching preparation/search | 147 |
| Prepared predictor entropy | 119 |
| Predictor residual bias | 97 |
| Large-count entropy product | 63 |
| Prior color-cost preparation | 60 |

This updates attribution for the opaque control after histogram separation;
the earlier photograph capture remains history. Current elapsed/allocation costs
remain in [benchmark.md](../docs/benchmark.md), with no new timing or peak claim.
The restored [compact matching trial](WEBP-COMPACT-MATCHING-INCONCLUSIVE.md) supplies
no reason to replay the unchanged matching variants.

An existing untimed actual-DLL audit emits the matching 547 B dense-scoring method
without a tool build or another CPU capture. Of its 182 samples, 67 map to summed
count/run discovery, 111 to run-model accumulation and four to setup/call/finish
boundaries. This is substantial arithmetic as well as traversal; it does not
establish that wider comparisons alone would help. Both original packet/source/
shared-root checks pass. No allocation or physical/native peak measurement follows.

Next, a bounded count-only audit will identify dense calls by logical bank,
immutable-leaf versus merged input, repeated stable population versions and
nonzero/edit-run density on the existing opaque and alpha controls. Current
prepared run repair serves large cached-green banks; shorter banks retain direct
scoring. Reconcile earlier support-preparation trials before changing that choice.
Only actual repeated work and preparation/storage costs can justify extending
the existing workspace owner. Keep bank/pair order, early exits, exact integer
costs, ties, packets, cancellation and admission; no new cache framework.

[Evidence](webp-opaque-current-attribution-evidence.json) pins actual producers,
gates and selected attribution. Raw `webp-current-lossless-opaque-attribution.tar.xz`:
1,129,704 B / 54 members, SHA256
`4c29db971cdb993e21c0af4dafe577b87f1dfd85cb5e4ef0c14db6332b25f31e`,
mirrored/member-verified, with the unchanged qualified histogram-separation archive
as source/package dependency. It retains the original capture, exact-source JIT,
relative offsets, audit results and reproduction. Unchanged Windows/Linux/full/
scalar and actual-package proof is reused; publication remains postponed.
