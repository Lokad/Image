# Current bicubic cross-axis and streaming costs

The existing default-workspace RGBA8 32×16 reduction independently passes warm
parity: 349.24 µs, 0.927× reference time, conservative upper 0.934× and
199,372 B/op. All 12 actual/filtered samples and both native brackets remain.
Stop this named case while its relevant production path is unchanged.

| Existing complete operation | Lokad µs | Ratio / upper | Allocated B/op | Verification |
| --- | ---: | ---: | ---: | --- |
| RGBA16 to 256×519, input+output+4 KiB | 1,282.86 | 1.549 / 1.694 | 1,063,410 | Short gap |
| RGBA8 to 32×16, input+output+4 KiB | 463.48 | 1.235 / 1.437 | 2,185 | Short gap |
| RGBA8 to 32×16, default workspace | 348.71 | 0.929 / 1.052 | 199,372 | Short; confidence open |
| RGBA8 to 32×16, default workspace | 349.24 | 0.927 / 0.934 | 199,372 | Independent pass |

Each starts from the existing 513×259 original raster and times complete public
Resize, local admission, fresh output ownership and release. The three controls
exercise cross-axis, constrained streaming and default-cache paths. The reference
is zimg, FFmpeg's zscale backend, with the existing full clamped halo, source
packing, filtering, alpha division, quantization and independently owned output.
Managed rational/frozen pixels remain exact; existing native maximum 1 and
RMSE 0.25 bounds pass. Working limits, source preservation and independent
surviving outputs are checked before and after timing.

Production is unchanged. The same qualified producer, native workers and cached
selectors are reused; selector bytes and case ordinals are recovered from their
retained archives. One short per case, then one confirmation for the promising
default case. No builds, profile, new image/settings grid or repeated full-suite/
package proof. [Evidence](warm-current-bicubic-cross-streaming-evidence.json)
links applicable [Windows/Linux and same-package qualification](WARM-VP8-SIGNED-DOMAIN-GUARD.md),
samples, identities and the mirrored, member-verified raw master.

This pass covers only the named default-workspace RGBA8 geometry. Other bicubic
depths, geometries and memory boundaries need their own evidence. Full 255-path
warm upper ≤1× and replacement readiness remain open.
