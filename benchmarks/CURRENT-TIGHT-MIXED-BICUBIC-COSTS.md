# Current tight mixed bicubic costs

Both existing width-enlargement/height-reduction controls are confirmed CPU gaps.
These replace uncertain old-producer costs; no causal gain/regression is claimed.

| RGBA depth | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| 8 | 1,217.911 | 1.408 / 1.449 | 530,400 |
| 16 | 1,221.634 | 1.431 / 1.452 | 1,060,367 |

The original 513×259 rasters resize to 1027×129 with input + output + 4 KiB local
workspace. This qualifies the distinct byte/word vertical-first mixed kernels,
including independent output allocation, normalization and release. Reference:
zimg, FFmpeg's scaler backend, with frozen halos, packing and settings. Managed
rational/qualified pixels are exact; native RGB/alpha max1 and RMSE≤.25,
zero-alpha RGB, deterministic pixels, input/output ownership and quiet-v2 pass.
All twelve actuals and both twelve-sample native brackets are retained. Native
allocations are unmeasured.

One frozen PIM1core supplies both costs. Zero builds, new inputs, profiles or
repeated qualification: existing 5,310 checks per OS, scalar/live and same actual
package/three-example proofs remain applicable. Retained selectors and workers
are reused; native timed apply is byte-identical after the existing halo-name
alias. A source-inventory guard failure preceded all timing; six committed
documentation/evidence files were synchronized, with no score repeated.

[Numeric evidence](current-tight-mixed-bicubic-costs-evidence.json) pins producers,
scope and qualification reuse. Mirrored/member-verified raw current-tight-mixed-bicubic-costs.tar.xz
SHA256 db80910b1e6b1ca1acd9ca9581040137cce90d298bf9745c2b461975771695b8 (2,701,596 bytes,
51 members) retains scripts, selectors, source, binaries, fixtures,
workers, actuals and guards. Both paths stay open within the full 268-case goal;
no workload matrix is added.
