# Certified integer horizontal JPEG decoding

The existing CMYK one-AC complete decode improves **6.7%**, from
668.26 to 623.44 µs. Exact pixels
and ownership are preserved. Warm parity remains open.

| Resident JPEG → independently owned RGBA8 | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op | JPEG / RGBA B |
| --- | ---: | ---: | ---: | ---: | ---: |
| CMYK one-AC | 623.44 | 389.30 | 1.601 / 1.638 | 1,085,689 | 17,346 / 531,468 |

With no vertical AC term, all frozen basis rows are identical. One integer vector
evaluates two copies of the row using the existing original Q16 basis and strict
rounding certificate. Only a fully certified group is duplicated to eight rows;
uncertain bins retain the original ordered double evaluation before any write.
The [proof](../docs/jpeg-inverse-rounding.md), total-magnitude bound and generated
8KiB module data are reused unchanged. There is no new table, allocation, cache,
owner or reservation. The original4096 work admission and cancellation remain.

One quiet short retains three actuals, native pre/post brackets and passing idle-v2
guards. Both sides include complete decoding, CMYK inversion/device-color
normalization, fresh RGBA ownership and disposal. The original packet and every
analytical managed sample are exact. Native normal/scalar maximum1/RMSE0.7093 and
exact alpha pass the unchanged maximum4/RMSE1.5 gate. Source and two-live-result
independence pass. No fixture, quality setting, profile or long repeat was added.

Qualification: **4,467 Release tests per OS** (1,649 internal,1,886 public,932 live
FFmpeg),10sparse/horizontal and30public CMYK/YCCK checks normally/scalar per OS.
Linux exercises the actual SIMD path. The same actual private0.2.0 package passes
both consumers and all three examples, including analytical one-AC/two-AC pixels,
borrowed roots and independent survivor charges. Refusal at every growth and
cancellation at output growth clean up and permit subsequent work.

Only the one-AC timing refreshes. The accepted2D kernel, certificate and basis
are unchanged; its evidence is reused. DC bypasses the sparse entry; dense/scalar
paths are unchanged. Other236numeric rows retain their identified scopes, including
225historical codec/edit rows still awaiting current-admission performance proof.
Four Lanczos outputs/timings and12text comparisons remain valid. Published0.1.0
is immutable; full254-case parity and release readiness stay open.

[Numeric evidence](warm-jpeg-horizontal-integer-evidence.json) retains statistics,
allocations, identities and bounds. Qualified source `b9aeb92ec7c442ce244477e7a0cee02d1dc5d2c732bafcb415d6e60710b79c61`;
actual package `437f9268ce3f589139ba177fa576df876ab2e3bef12876dfb41f3169dcf63d25`. Mirrored raw archives:
`cm9-qualified-windows.tar.gz` (`f06b74d94651105a615b2d5b5bd09e1f3c71b88c474c927d24e378d8f2a42f25`),
`cm9-qualified-linux.tar.gz` (`e580241441010432f5c66ac1fcf7200381f8364b902c19c58341b005b74c9422`). Unchanged runtime dependencies and
native fixtures retain their original archives.
