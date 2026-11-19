# Six-tap Lanczos3 word windows

Two adjacent RGBA16 enlargement outputs with identical six-tap source support
now share the conversion of all six source pixels. The fixed-size kernel keeps
four independent products and the original reduction tree, then applies the
last two taps in order to both output accumulators. The managed Q30 coefficients,
rounding certificate and exact fallback are unchanged. Edge windows, longer
support, reductions and the portable path keep their existing execution.

| Original 513×259 to 1027×519 operation | Lokad.Image µs | zimg µs | Conservative ratio interval | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Prepared RGBA16 | 3,886.106 | 4,227.937 | 0.888686–0.947668 | 4,264,760 |
| One-shot RGBA16 | 3,928.213 | 5,092.245 | 0.755905–0.791442 | 4,592,818 |

The reference remains the original zimg 3.0.6 FFmpeg scaler adapter with the
declared prepared and one-shot boundaries. Both include independent output
acquisition and disposal. The prepared plan retains 333,396 B. The 4,264,104 B
owned frame exceeds the default idle pool ceiling. GC allocation, retained
capacity and unmeasured physical peaks are separate quantities.

An independent managed comparison saves **231.31 µs (5.69%)** in prepared resize,
from 4,062.322 to 3,831.014 µs, twelve alternating pairs with a candidate/prior
interval of 0.928082–0.958142. The earlier tail-only trial yielded a similar
scoped gain and two uncertain standalone comparisons. Its interleaved capture
retains a shared shift in both subjects after the third unit and an inconclusive
outer bound. Those measurements remain in the archived trial.

The scalar FMA oracle verifies the producer's exact bits, alpha and unsigned
products. Both original comparisons retain all twelve actuals, exact managed
pixels, bounded native pixels, source preservation, independent outputs and the
versioned quiet guards. Windows and Linux each pass 6,865 Release checks,
including 948 live FFmpeg comparisons, and 163 internal and 33 public checks
per fallback mode. Both verify the same actual Release package and all three
examples. The packed DLL matches the timed DLL.

The [evidence](lanczos-six-tap-word-windows-evidence.json) pins both experiment
archives and qualification. The other eight Lanczos comparison paths retain the
unchanged execution and source-valid [earlier evidence](LANCZOS-CURRENT-PACKAGE.md).
