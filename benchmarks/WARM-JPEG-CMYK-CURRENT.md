# Earlier CMYK sparse-2D JPEG cost

**Historical cost, superseded.** [Current fixed-policy confirmation](CURRENT-CMYK-FIXED-INVERSE-COST.md)
gives2.850× / upper2.858 with exact managed/native pixels. The packet/settings
are unchanged; corrected expected inverse/device precision replaces the old
oracle. This earlier numeric row remains historical, without a current claim.

The original round demonstrated a **1.829× gap** on its then-qualified
0.2.0 producer. This refresh resolves old-code uncertainty; it is not an
optimization or an independent parity confirmation.

| Complete resident JPEG → owned RGBA8 | Lokad µs | FFmpeg µs | Ratio / conservative upper | Lokad allocated B/op | JPEG / RGBA B |
| --- | ---: | ---: | ---: | ---: | ---: |
| CMYK sparse-2D | 805.26 | 440.28 | 1.829 / 1.902 | 1,085,680 | 22,708 / 531,468 |

Both sides include complete component decode, CMYK/black normalization, fresh
independent RGBA ownership and release. Original resident packet, preparation,
analytical pixels, normal/scalar CLI quality and alpha gates are reused. Managed
pixels are exact; native maximum1/RMSE0.7091 and alpha255 pass the existing
maximum4/RMSE1.5 contract. Source and two-live-output independence pass.
ICC conversion and EXIF reorientation remain off.

One quiet short retains all three actuals, pre/post native brackets and both
versioned guards. Current frozen production/harness and an archived generic JPEG
selector share existing immutable runtime dependencies in an isolated folder;
the prepared Lanczos runtime is preserved. No production, harness, selector or
native rebuild; no fixture/settings grid or long confirmation above one.
The CMYK helper and timed dispatch prefix are unchanged from the earlier harness.

Reuse [current qualification](WARM-LANCZOS3-COEFFICIENT-PERMUTATIONS.md): 4,448 tests
per OS, affected hardware-off controls, same actual private package and all three
examples on Windows/Linux. No production inputs changed. Only this numeric row
refreshes; full parity and replacement qualification remain open.

[Numeric evidence](warm-jpeg-cmyk-current-evidence.json). Raw
`cm2-current-short-linux.tar.gz`: `58f55ec23afd4ecb6ca874dc2f016096171c900c9bef5ce53fba118a68b872a4`. Producer:
`c12fbeebea4edb6b4959837183a4b12379756be3139bd46b5e3c1798ff97671d`. The existing generic selector/dependencies retain
their identified source archives; no full duplicate runtime is added.
