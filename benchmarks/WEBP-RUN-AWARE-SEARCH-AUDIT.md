# WebP run-aware search audit

A bounded private diagnostic checks a run-aware length search against every
actual scalar result on the existing transparent lossy control. The original
scalar results still drive matching. Both complete encodes preserve the exact
98,590-byte packet, source and root cleanup; both quiet guards pass.

| Full-frame call | Scalar comparisons | Run comparisons | Preparation comparisons | Net comparison change |
| --- | ---: | ---: | ---: | ---: |
| first | 1,021,755 | 895,210 | 132,866 | +0.62% |
| second | 1,478,718 | 1,276,887 | 132,866 | −4.66% |

Skipping repeated pixels saves little after preparation and also adds count
loads and minimum arithmetic. These are diagnostic counts, not timing or
allocation measurements. No production candidate, timing screen or confirmation
is justified by this evidence. This stops this implementation without a permanent
ban on revised designs. The earlier [phase attribution](WEBP-MATCHING-PHASE-AUDIT.md)
does not imply that long equal runs dominate search.

One private audit build; zero production builds. Current qualified source, DLL,
package and benchmark costs remain unchanged. Reuse their qualification.
[Evidence](webp-run-aware-search-audit-evidence.json) retains every call aggregate,
output checks and exact runtime identities in the raw archive.

Raw webp-run-aware-search-audit.tar.xz: 613,428B /29members, SHA256
0d8d73381fa784bbf246d959d83a8371b93114fb22897575b7fd77f7edaf1991,
mirrored/member-verified. Qualified source dependency: webp-cache-prefix-aggregation.tar.xz,
SHA256 34e5bbd505797c124e14330bdfaa3e1cb0992c3c6f04a7fc9861d7b10e28cd06.
