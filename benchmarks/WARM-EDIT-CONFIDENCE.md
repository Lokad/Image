# Single-owner edit confidence checkpoint

**Transpose RGBA8 independently passes warm parity**, with conservative upper
0.995x FFmpeg. Stop this case. Half turn RGBA8 has a small mean gap; clockwise
RGBA16 still straddles. Each case received one independent confirmation.

| Existing operation | Lokad / FFmpeg µs | Ratio / upper | Managed B/op | Actual / selected | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Half turn RGBA8 | 11.466 / 11.422 | 1.004 / 1.024 | 128 | 12 / 12 | Gap |
| Clockwise quarter turn RGBA16 | 43.455 / 44.080 | 0.986 / 1.010 | 128 | 12 / 12 | Confidence |
| Transpose RGBA8 | 38.151 / 38.423 | 0.993 / 0.995 | 128 | 12 / 11 | Pass |

All 12 actuals per case, including transpose's one excluded outlier, and both
`exclusive-vm-idle-v2` guards are retained. The reported upper is the larger of
selected/all-actual managed bounds against the faster native bracket's lower
bound. All three retain exact pixels, alpha and hidden RGB, normal/scalar native
agreement, source preservation and independent owners. B/op includes harness;
native allocation is unmeasured. No repeat-until-pass or tuning followed.

The same two original 513x259 RGBA8/16 rasters, frozen workers and complete public
operation/admission/allocation/materialization/disposal boundaries remain. Native
graph setup/wrappers are reused; actual owned output, normalization, padding and
endian handling retain their approved timed boundaries. Startup, IPC, I/O,
generation/hashing and validation are excluded. One CPU, normal SIMD, no concurrency.

The unchanged qualified producer, archived selectors and current frozen harness
were reused with zero builds. Existing Windows/Linux correctness, live FFmpeg,
scalar and same-actual-package proofs remain applicable. No package proof repeat.
Only three measurements change; the other 204 comparisons and 60 prior passing
proofs remain. Current totals are 61 passes, 145 gaps and one confidence case,
with 127 independent confirmations and 80 qualified shorts.
[Numeric evidence](warm-edit-confidence-evidence.json) pins sources, selectors,
references, all actuals, bounds and reused qualification. The207-case parity goal
remains open; larger demonstrated gaps take priority over these small residuals.
