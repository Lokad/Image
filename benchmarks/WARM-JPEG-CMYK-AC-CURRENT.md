# Current one-AC CMYK JPEG decoding

The existing horizontal-only one-AC case is a demonstrated **1.714× gap**
on the current qualified producer. This resolves old-code uncertainty; it is
neither an optimization nor an independent parity confirmation.

| Complete resident JPEG → owned RGBA8 | Lokad µs | FFmpeg µs | Ratio / upper | Lokad allocated B/op | JPEG / RGBA B |
| --- | ---: | ---: | ---: | ---: | ---: |
| CMYK one-AC | 668.26 | 390.00 | 1.714 / 1.764 | 1,085,686 | 17,346 / 531,468 |

The horizontal-only return precedes the new certified 2D integer branch. Both
sides time complete component decoding, inversion/device-color normalization,
fresh independent RGBA ownership and disposal. Original packet and analytical
managed samples remain exact. Native normal/scalar maximum 1/RMSE 0.7093 and
alpha 255 pass the unchanged maximum 4/RMSE 1.5 gate. Source and two-live-output
independence pass; ICC conversion and EXIF reorientation remain disabled.

One quiet short retains three actuals, both native brackets and both idle-v2
guards. The current frozen producer, prepared worker, generic selector, harness,
fixture and quality controls are reused without builds, profile, new settings or
long repeat above one. The [4,464-test/OS qualification](WARM-JPEG-SPARSE-INTEGER.md),
affected scalar checks and same actual private-package consumers/examples remain
valid and are reused. Only this existing row refreshes; 225 historical codec/edit
rows retain their old producer scope.

Next: qualify a row-specific integer primitive using the existing original basis,
with exact fallback before writes. Do not retune the accepted 2D kernel. Full
warm parity and release readiness remain open.

[Evidence](warm-jpeg-cmyk-ac-current-evidence.json). Raw `cm8-current-ac-linux.tar.gz`:
`6037d211a24ef335b49bc0f15ff07087bc6139f532f1d29590cfba4c122d80c3`. The archive is mirrored; unchanged runtime dependencies,
producer and native fixtures retain their preceding archives.
