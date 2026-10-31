# Current RGBA16 JPEG companion costs

Three existing controls now have independent current costs on the accepted
[factored-normalization build](JPEG-FACTORED-WORD-NORMALIZATION.md). All remain
confirmed gaps, including 422's conservative lower bound just above 1.001.
No causal gain claim is made against unmatched older producers or settings.

| Existing case | Managed µs | Ratio / upper | Allocated B/op | Managed / native packet B |
| --- | ---: | ---: | ---: | ---: |
| Encode422_16 | 2,447.318 | 1.008 / 1.016 | 529,871 | 71,208 / 71,210 |
| Encode420Stream16 | 1,910.659 | 1.039 / 1.058 | 388,247 | 52,115 / 52,117 |
| Encode420Cancelable16 | 1,934.327 | 1.047 / 1.065 | 388,215 | 52,115 / 52,117 |

The original 513×259 RGBA16 source, Q85/native qscale 5, sampling and background
0x204060 stay frozen. Every complete operation includes single-round flattening,
normalization, conversion/encoding and fresh output ownership/release. Stream16
also creates/releases its forward-only wrapper; Cancelable16 retains a live,
never-signaled token with pre-canceled behavior checked. Complete coded packets
and reconstruction remain exact, except approved bounded COM attribution.
Input/output ownership and native resource checks pass. Native allocation is
unmeasured. No shared root is installed in these three controls; the governed
companion's admission/lifetime proof remains unchanged.

One current twelve per case, sequential quiet CPU0, optimized warm JIT and
nonconcurrent workstation GC; both idle-v2 guards pass. All actuals, native
brackets, filtered/unfiltered confidence and allocation measurements remain.
Three hash-verified old selectors are recovered from retained raw archives:
zero builds, profiles, new inputs/settings, timing retries or qualification
repeats. The same immutable core reuses 5,331 checks/OS and the actual Release
package/three-example proof on both platforms. Historical catalog owned-quality
calibration does not select this encoder or its actual Q85 packets.

All five measured affected subsampled public boundaries now have current costs.
This supplies no broader content/grayscale claim or new required case; the full
268-path objective remains open. No normalization width/reciprocal variants.

[Evidence](current-jpeg-word-companion-costs-evidence.json) retains observations,
selector/producer identities and qualification reuse. Mirrored/member-verified
raw `current-jpeg-word-companion-costs.tar.xz` SHA256
`0d9d55735e78101aee6c9d9194e3c5a0410879f78b7285178f115c2d5ef61c33` (3,223,892 B / 76 members)
preserves cached selectors, immutable runtime, workers, fixtures, guards and
reproduction scripts, with JFN1 and original selector archives retained.
