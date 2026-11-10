# Live VP8 macroblock-window experiment

VMC770 does **not** change production. Three coherent implementations preserve
pixels and cancellation, but establish no useful complete-operation gain. The
qualified implementation, assemblies and existing package evidence are restored.
Current displayed costs remain unchanged. This closes these implementations,
without banning a materially different window design.

The [current attribution](VP8-LIVE-CURRENT-ATTRIBUTION.md) motivated sharing one
arithmetic window across 24/25 live coefficient blocks, as ordinary decoding
already does. All versions retain the original polling grammar, every block and
decision checkpoint, conservative 272 B/block partition guard, exact coefficients,
contexts, successful cursor/following reads and short-input fallbacks. A failed
private decoder is abandoned; no public failure or ownership contract changes.

| Existing complete opaque live decode | Actuals | Managed µs | Native µs | Point ratio |
| --- | ---: | ---: | ---: | ---: |
| Original short | 3 | 4,920.91 | 4,786.94 | 1.028 |
| A: polling core in borrowed window | 3 | 5,364.86 | 4,798.47 | 1.118 |
| B: block-local input span | 3 | 5,348.69 | 5,162.37 | 1.036 |
| C: original explicit input arguments | 3 | 5,337.79 | 5,151.81 | 1.036 |
| Original independent confirmation | 12 | 5,229.57 | 5,141.72 | 1.017 |
| B independent confirmation | 12 | 5,040.38 | 4,796.65 | 1.051 |

Short confidence intervals overlap. B's independent raw point saves 189.19 µs /
3.62%, alongside a 6.71% faster native reference; its normalized point is 3.32%
worse. Both filtered/all-actual raw and normalized intervals overlap: neither a
gain nor regression is established. Every actual, including the original's
filtered-out actual, is retained. No unchanged candidate is sampled again for a
favorable bound. C preserves the original data/model/sample argument placement
and shared state, but gives no useful short-screen point; it receives no further
timing or broad/package qualification.

Generated code motivates the revisions, without proving their speed: A's moved
core is 2,738 B/200 B stack adjustment; B's local span gives 2,758 B/248 B. The
original core is 3,090 B/248 B. Three separate code-inspection runs discard all
instrumented elapsed time. Saved initialization/publication work alone does not
establish a complete workload improvement.

All runs use the frozen 72,380 B packet and exact independently owned 531,468 B
RGBA output, active managed cancellation, CPU0 and idle-v2 guards. The explicitly
labeled native codec/conversion contexts remain retained. Complete managed public
decode, normalization, output and disposal stay timed. No inputs/settings or
reference gates change. Allocation stays **229,344 B/op**, added heap capacity and
retained state stay zero, and the existing 8 KiB coarse stack admission remains.
Physical/native peaks are unmeasured.

Each version passes 1,030 internal/239 public focused checks in normal/scalar modes
on Windows/Linux. Five prototype checks cover actual scalar/per-block/window
coefficients and cursors, validation, following reads, guards, partition ends,
cancellation and borrowed admission. They are removed with the discarded entry
points. Three main/embedded versions per OS; no compile failures, harness/native/
selector/fixture builds or new workload grid. Restored production reuses the
6,182 full/live checks per OS, affected ISA and same actual package/all three
examples from [JBC768C](JPEG-BATCHED-TWO-ROW-COLUMNS.md).

[Evidence](vp8-live-macroblock-window-experiment-evidence.json) pins all producers,
sources, guards, actuals, code and byte-exact restoration. Its mirrored,
member-verified raw archive retains original and all three candidate sources,
assemblies, tests and reproduction scripts. It is not a renderer dependency.
Next examine current byte bicubic enlargement attribution; all 283 required paths
remain in scope and publication is postponed.
