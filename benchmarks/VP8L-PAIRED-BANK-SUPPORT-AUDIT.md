# Paired WebP histogram support

One isolated current-source projection adds a single read-only observation at
paired population-cost entry. One existing complete RGBA8 encode preserves the
exact183,812-byte packet, source identity and root cleanup; both quiet guards
pass. Production DLL/source/package remain unchanged. One private tool build,
zero production builds; no new input/settings or timing/allocation claim.

| Logical bank | Paired calls | Scanned bins | Zero bins | Mean combined support | Maximum support |
| --- | ---: | ---: | ---: | ---: | ---: |
| Distance40 | 16,051 | 642,040 | 45.4% | 21.84 | 33 |
| Literal R/B/A256 | 69,683 | 17,838,848 | 87.2% | 32.87 | 145 |
| Green/cache1304 | 24,876 | 32,438,304 | 52.2% | 623.11 | 1,180 |

Literal color pairs scan17.84million bins;15.55million (87.2%) are zero.
47,618/69,683calls (68.3%) have fewer than16nonzero combined symbols. These
zero color bins account for30.5% of all paired scanned bins. This supports a
prepared individual-support design; earlier whole-empty64-bin/pair-cache
diagnostics did not measure it. Green/cache is denser and retains its existing
path initially. These are address/support counts, not method times, guaranteed
speedups or merge approval. The current profile’s47.0% attribution is not a
CPU headroom bound for this narrower design.

The proposed end state keeps four UInt64support words per full256-symbol color
bank, within the operation’s working owner. Prepare with existing initial
population work, OR during merges and rebuild/update on replay. Exact sparse
pair scoring enumerates combined support, accounts for zero gaps and adjacent
equal nonzero runs, and retains a portable dense fallback. Green/cache/distance
stay unchanged. Logical bank costs, integer refinement, evaluation thresholds,
merge order, ties and full published populations/packets must remain exact.

Metadata is at most96B per stored histogram (249,600B at the existing2,600tile
bound), plus admitted array/object overhead; compact banks need none. Reserve
before allocation and retain through actual users. This is a storage model,
not measured allocation or peak. Preparation, update and dense-dispatch overhead
may offset saved scanning. No cache, pool, new governor, fixture special case,
budget fallback or changed numerical policy.

Implement one coherent stage with exact sparse/dense cost, run/word boundaries,
high-count, cancellation and admission tests. Use existing ordinaryRGBA8 and
photo/diagram/alpha complete screens; independently confirm relevant promising
cases. Preserve adverse/inconclusive outcomes. Earlier window/run-boundary SIMD
implementations are not replayed; the current evidence justifies a different
preparation design. Windows/Linux/scalar/full/live/actual-package qualification
is required for an accepted production stage.

[Evidence](vp8l-paired-bank-support-audit-evidence.json) retains all counts,
projection/source/runtime identities and limits. Raw
vp8l-paired-bank-support-audit.tar.xz: 607,784B /31members, SHA256
74f8aaaaeb35d4ef2afec492e99d1e0445362d852f9bb2b247a39c613d2c42ef, mirrored/member-verified. Production qualification depends on
webp-constant-bank-workspace.tar.xz (SHA256
cb7feb8c8ded7a5693021ca0d20e7e3299e1df3f8db6235853cf761de0a255d6).
Full source snapshot: current-rgba-vp8l-workspace-attribution.tar.xz, SHA256
3a249ddcf33e8e81fc727f824080b63d43bda1095b3ed2629088b40eb2fde580;
every projected production source hash is verified against it. No new broad or
package qualification is needed for the observation. Publication is postponed.
