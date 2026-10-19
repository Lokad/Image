# Sparse JPEG prefix experiment

**Rejected; production and all current timing rows are unchanged.** The exact
leading-horizontal-prefix candidate improved the existing2D sparse CMYK mean
only3.1%, leaving a1.750x FFmpeg gap. Prior accepted qualification
is reused after restoring the exact source and producer bytes on both OS.

| Existing complete operation | Managed / FFmpeg µs | Ratio / upper | Managed allocated B/op |
| --- | ---: | ---: | ---: |
| Accepted2D sparse | 795.30 / 437.20 | 1.819 / 1.928 | 1,085,656 |
| Rejected prefix candidate | 770.77 / 440.55 | 1.750 / 1.810 | 1,085,660 |

One quiet CPU0 six-second profile of the current qualified complete operation
collected5,853samples: sparse inverse32.65%, plain AC entropy13.12%, scan8.83%,
CMYK materialization8.35%, baseline block6.65%. These are diagnostic samples,
not standalone primitive timings. The candidate shared the identical v=0 prefix
of each row, then continued the original ordered binary64 additions. Exact
pixels,4,096work-before-writes, native quality/alpha/source and independent-owner
gates passed. No FMA, reassociation, tables, input copies or resource changes.

One immutable candidate per OS;19affected targeted checks/OS. A test initially
requested9active terms against the existing8-term sparse limit; its corrected
control and the failed log are retained. No production bug was found. The single
three-actual200ms screen, all outliers, both idle-v2 guards and50ms native
brackets are retained. Existing harness, selector, native worker and fixture were
reused. No extra image, settings grid, repeated profile, control sweep, long
confirmation, full correctness run or package proof for this rejected change.

All source/test changes were reverted; restored producer SHA256:
Windows`699404f71933e84ccb88e8d902b9cfdf4304420e241e8b1e1987d1bc4934a19b`,
Linux`bcfb67fd47aaa51180b6e316d880a455798d27f1973443ca6e3c98e04f3d94b8`.
Reuse [accepted qualification](WARM-JPEG-HORIZONTAL-ROW.md):4,197tests/OS,
61affected hardware-off checks/OS and same actual private package consumer/all
examples on both OS. Released0.1.0 remains immutable. Scope stays246cases,
222numeric+24unmeasured,70passes/151gaps/one confidence-open. Passing cases stop.

Next resolve native normalization for the already-listed YCCK420 case. Its
full-resolution black plane and subsampled matrix are distinct required paths;
an unmeasured path is not presumed slow. No new checklist case is needed.

[Numeric/profile evidence](warm-jpeg-sparse-prefix-trial-evidence.json), raw on
both hosts:

- `h10m-profile.tar.gz` SHA`392871e59f3779f6e6fc41f1e09a9b15d64bb9db027b08590ac941c9116e5b8c`.

- `h10m-measurements.tar.gz` SHA`4d98ec7d64ecf4a346690ace2da795feaf649c0554f6daf4e606baa0744ab8b6`.

- `h10m-windows-targeted.tar.gz` SHA`8ecd641fc84c8195cc8ddb137728ac75e94a70145cb43c20ffbc1f10b0f9869f`.
