# Sparse VP8 residual-scoring screen

Prepared zero-run prefixes and signed coefficient-price deltas replace dense
accumulation across all four residual types. Nonzero coefficients, their immediate
followers and a nonzero initial context retain explicit corrections. Initial/final
EOB and large-level pricing stay unchanged. Scalar and trellis consumers reconstruct
the same prices from the one operation-owned representation; no pool or new owner.

The independent algebra checks109,072 sequences. Four complete frozen diagnostic
encodes (warmup/observation each) agree exactly: photograph769,122→598,749 logical
visits (22.15% fewer), diagram295,842→238,156 (19.50%). These are applicability
counts, not time savings. Complete operations include all preparation, refresh,
mask traversal, trellis reconstruction, fresh output ownership and disposal.

| Frozen513×259 complete Q75 encode | Original µs | Candidate µs | Ratio | Interval |
| --- | ---: | ---: | ---: | ---: |
| photograph | 11,564.16 | 11,923.63 | 1.031088 | 0.987899–1.076160 |
| diagram | 6,552.18 | 6,678.57 | 1.019290 | 0.984458–1.055353 |
| trellis | 11,421.52 | 11,605.63 | 1.016121 | 0.979369–1.054250 |

All three short screens are **inconclusive, with no useful lead**. Each retains
three alternating pairs×eight complete operations on quiet pinned CPU0. There is
no independent confirmation or favorable repeat; no causal regression or equivalence
claim. This implementation is not retained. Materially revised sparse designs remain
eligible; skipped visits alone do not justify another implementation.

The same rate-plan owner grows20,192→20,512B;32 neutral words and64 Int32 prefixes
add320B. Int32 preserves the original broad14-bit synthetic oracle without narrowing
its sums. Creation plan+scratch peaks21,536B; this is not a whole-operation physical
or admitted peak. Complete allocations grow by320B: photograph809,072→809,392,
diagram740,984→741,304 and trellis754,344→754,664B/op. Scratch retained after encode
is zero; native allocation/physical peaks are unmeasured.

One main/embedded build perOS passes54internal/137public checks in both normal
and scalar modes. Generated signed bounds, refusal/refresh/full-root/replacement/
cancellation, frozen native cost/residual digests and786,432 unchanged synthetic
comparisons pass. Exact packets remain17,688/7,100/6,818B, sources unchanged and
outputs independently owned. Candidate-only representation/tests are restored
with the implementation; source/binary versions remain raw, no obsolete wrapper.

Qualified RQP349 sources, main/embedded modules, public/differential/probe bindings
and Linux manifest are restored byte-for-byte, without a rebuild. Reuse its5,716
full/live checks eachOS, normal/scalar/AVX512-off and SAME actual package/all3examples.
No production change, current-cost override, renderer dependency, repeated package
proof or publication. The full282-path objective remains open. Next reconcile the
named historical complete RGBA8 bicubic enlargement boundary before tuning it.

[Evidence](vp8-sparse-residual-scoring-inconclusive-evidence.json) pins counts,
proof, source/module identities, all actuals/guards and restoration. Raw
`vp8-sparse-residual-scoring-inconclusive.tar.xz`: 8,155,884B/251members,
SHA256 `a875ccb307f91732dab9bf2a079ee322a93f926216e902d5acb408d826069d19`; mirrored/member-verified, with qualified-base dependency.
