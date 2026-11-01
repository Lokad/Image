# Current default mixed-axis bicubic costs

Two existing default-workspace controls now have independent current twelves.
Both are confirmed gaps against warm single-CPU upper ≤1×. These observations
resolve old short-only evidence; they establish no causal gain or regression.

| Existing 513×259 → 256×519 resize | Lokad µs | Ratio / upper | Managed B/op | Status |
|---|---:|---:|---:|---|
| Cross8, RGBA8 | 912.313 | 1.084 / 1.107 | 731,768 | Confirmed gap |
| Cross16, RGBA16 | 974.304 | 1.178 / 1.196 | 1,263,193 | Confirmed gap |

Original inputs, exact qualified/rational pixels, native frame hashes and
source/independent-output ownership pass. Reference is zimg, FFmpeg's zscale
backend, including unchanged packing, halos and alpha work. Native RGB/alpha
errors remain ≤1 and both RMSE ≤0.25. Complete admission, resize, fresh output
and release are timed on both sides; native allocation is unmeasured.

A scoped review verifies the authentic DHP1 source snapshot against its accepted
Git object. Mixed byte/word writers and original-order replay are unchanged.
The common entry lost two nonmixed locals in a962175; no unchanged-JIT-cost
claim follows. The changed 9dc661c byte writer is not called here. Low-alpha
replay can call ResizeAlpha.Round, changed in b5dd62e; its reachability on these
inputs was not proven by this review. Fresh current costs avoid relying on that
uncertainty. Original frozen expectations and native settings are not refreshed.

One cached selector/worker/input per case and the same immutable current library
and harness serve the two sequential confirmations. Every actual, bracket and
quiet-v2 guard passes. **Zero builds, new profiles or new cases**; no metadata
projection or timing repeat. An initial raw-retention source-path typo is
corrected without rerunning either score; its failure is retained.

Unchanged 5,342 checks per OS, three additional probe contracts, 948 live
comparisons and SAME actual Release-package/three-example proof are reused.
Published packages remain immutable. Stop retiming these controls unchanged;
the full **268 required material paths** remain in scope.

[Numeric evidence](current-default-mixed-bicubic-costs-evidence.json) records all
actuals, brackets, confidence, exact source/worker/output identities and guards.
Raw `current-default-mixed-bicubic-costs.tar.xz`, SHA256
ed71aca40e0808b39b5f1d080d3193e5633b98951bc6504c7b314471204d27c9 (1,239,884 B / 62 members),
is mirrored and member-verified. Linked original cached-cost, selector/reference
and J4N31 archives retain reproducibility and unchanged qualification. This is
development-path evidence, not a full immutable-package release refresh.
