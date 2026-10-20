# Certified integer sparse JPEG decoding

The existing complete CMYK sparse-2D decode improves **11.3%**, with exact
pixels and the same allocation profile. It remains a warm single-CPU parity gap.

| Complete resident JPEG → owned RGBA8 | Lokad µs | FFmpeg µs | Ratio / conservative upper | Lokad allocated B/op | JPEG / RGBA B |
| --- | ---: | ---: | ---: | ---: | ---: |
| CMYK sparse-2D | 714.01 | 441.52 | 1.617 / 1.693 | 1,085,682 | 22,708 / 531,468 |

Four integer vectors evaluate 64 samples from an original Q16 basis. All 64 clipped
rounding bins must certify before any output write. Uncertain or large-magnitude
blocks use the existing double calculation; no approximate managed byte escapes.
The [proof](../docs/jpeg-inverse-rounding.md) and original generator define bounds
and provenance. Its 8 KiB basis is compiler RVA module data: no heap table, caller-data
cache or new workspace. Controls verify zero allocation and every generated entry
against exact rational rounding of the frozen owned basis. No upstream data.

The existing 4096 work charge covers both attempted integer and exact fallback
contributions. Cancellation, shared admission, borrowed input/root, retained owners,
DC/horizontal/dense branches and unsupported hardware retain their behavior.
The CMYK shared-budget test now exercises sparse AC terms. Rejection controls
include half ties, a block with 62 certain/two uncertain lanes, mass/quantizer extremes,
input/destination guards and no partial writes.

One quiet short retains three actuals, native pre/post brackets and both passing
idle-v2 guards. The reference remains FFmpeg's single-threaded software MJPEG decode,
with complete four-component decode, inversion/device-color normalization, fresh
independent RGBA output and release. Original packet, worker, harness, selector
and quality gates are reused. All managed pixels are exact; native normal/scalar
maximum 1/RMSE 0.7091 and alpha 255 pass the unchanged maximum 4/RMSE 1.5 contract.
No new raster/settings/profile or long repeat above one.

Qualification: **4,464 Release tests per OS** (1,647 internal, 1,885 public,
932 live FFmpeg), 8 focused sparse and 29 public CMYK/YCCK checks normally/scalar
per OS. Linux exercises actual SIMD; the RVA and hardware fallbacks are verified.
The same actual private 0.2.0 package passes both consumers and all three examples,
including analytical sparse-AC pixels, borrowed shared roots and independent
survivor charges. The added package check changes neither frozen producer nor
package bytes. Four original Lanczos frames/timings and 12 text comparisons reuse
unchanged paths. Published 0.1.0 remains immutable.

Only the existing sparse-2D row refreshes. Other 236 numeric rows retain their
identified scopes; 226 historical codec/edit rows still need current-admission
performance proof. Next: resolve the existing one-AC case's old-code uncertainty
on this qualified build before assuming it needs optimization. Full 254-case warm
parity and replacement/release readiness remain open.

[Numeric evidence](warm-jpeg-sparse-integer-evidence.json) retains all statistics,
allocations, producer/source/package identities and bounds. Qualified source
`08dea083fb48871968dfeaa5768ff71b2f11b0820a3f494384849224200e9f81`; package-tool source
`180a840158cb53de7c9abad9e94ee765b1e1f685844a85e80b5b7a09289367b5`. Raw `cm7-qualified-windows.tar.gz`:
`0359cba60c2caa1477f5c7492fed35ea17265dced4f9c4db65cb07b3a459a1b6`; `cm7-qualified-linux.tar.gz`:
`0193785a79fbe576970a6abde3c016b1a104ca5b54e6ee08b1695cf10d2b4c74`. Both archives are mirrored. Unchanged dependencies and
reference fixtures retain their original source archives.
