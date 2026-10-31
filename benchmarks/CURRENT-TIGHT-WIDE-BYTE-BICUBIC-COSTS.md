# Current tight wider-support byte bicubic costs

Both required tight-workspace controls are confirmed CPU gaps on the qualified
MGP1 producer. Older costs do not establish a causal gain or regression.

| Existing complete operation | Lokad µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| TightWide8 | 505.695 | 1.454 / 1.475 | 32,905 |
| Tight64x32_8 | 545.554 | 1.474 / 1.506 | 8,329 |

The same original 513×259 RGBA8 raster reduces to 128×64 or 64×32 with a local
allowance of input + output + 4 KiB. Four-column and packed two-column kernels are distinct from
the default larger-workspace cache. Complete operations include normalization,
independent output allocation and release. Frozen managed rational/frame pixels,
native RGB/alpha max1/RMSE≤.25, zero-alpha RGB, settings, source/output ownership
and quiet-v2 pass. All twelve actuals and both twelve-sample native brackets remain.
Reference is zimg, FFmpeg's scaler backend, with original halos/packing/settings;
native allocation is unmeasured.

Zero builds, profiles, new inputs or successful timing repeats. Original selectors,
workers and raster are reused. Earlier kernels/admission/shared-alpha sources
changed, so their old1.687/1.718costs were uncertain current evidence. These two
confirmations resolve that uncertainty without a new workload grid. Unchanged
5,314-check-per-OS/full-live and same actual package/three-example proofs apply.
The rejected [Double-staging trial](PACKED-COLUMN-DOUBLE-PRODUCTS-REJECTED.md) is
fully restored; its three useful public controls pass on production too.

[Numeric evidence](current-tight-wide-byte-bicubic-costs-evidence.json) records
precise scope and qualification reuse. Mirrored/member-verified raw current-tight-wide-byte-bicubic-costs.tar.xz
SHA256 f9c5d960a6963be585265fc758a76007e8182a143758495e1ebf1fc9c2fc389a (1,355,936 bytes,
78 members) retains scripts, selectors, source, cores, fixtures,
workers, receipts, actuals and guards. Both paths and the full268-case goal remain open.
