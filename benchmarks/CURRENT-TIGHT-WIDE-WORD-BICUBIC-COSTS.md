# Current tight wider-support word bicubic costs

Both existing tight RGBA16 controls are independently confirmed CPU gaps.

| Complete operation | Lokad µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| TightWide16 | 611.340 | 1.464 / 1.524 | 65,674 |
| Tight64x32_16 | 518.012 | 1.329 / 1.355 | 16,521 |

The original513×259 RGBA16 raster reduces to128×64 or64×32 with input+output+4KiB
allowance. The three-column Double and packed UInt32 two-column paths, including
unsigned conversion/word guards, are distinct from byte and default-workspace
controls. Full normalization, independent output and disposal are included.
Frozen rational/frame pixels, original native RGB/alpha max1/RMSE≤.25,
zero-alpha RGB, source/output ownership and quiet-v2 pass. The reference is
zimg, FFmpeg's scaler backend, with frozen halos/settings/packing; native
allocations are unmeasured. Twelve managed actuals and both native brackets remain.

Existing workers, fixtures and selectors are reused: zero builds, profiles,
new inputs or timing repeats. Historical source paths changed, so these resolve
uncertain current costs without claiming a gain. Full5,314checks/OS and the
same actual package/three-example proof carry forward with3new public controls
qualified separately (5,317applicable; no fresh full5,317run). Unrelated VP8L
counting work does not invalidate these resizer measurements.

[Numeric evidence](current-tight-wide-word-bicubic-costs-evidence.json) identifies
scope and reuse. Mirrored/member-verified raw current-tight-wide-word-bicubic-costs.tar.xz SHA256
bd805c5d2a55cae9c862d9f39e8fe0b33f52c8e643e215c3651def548aa7dc52 (1,404,092bytes/78members) retains source,
cores, fixtures, workers, selectors, receipts, actuals and guards. Both gaps and
the full268-path objective remain open; no rescore or grid expansion.
