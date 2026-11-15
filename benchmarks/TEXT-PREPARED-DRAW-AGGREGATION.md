# Prepared draw measurement

TEP1039 refreshes one scoped diagnostic on the current qualified DLL `548befe0`,
without changing production. The owning [one-shot/eight-draw integrations](FONT-HEADER-VALIDATION.md)
still pass; their direct timings, allocation and preparation evidence are reused.
This drawing comparison does not establish an overall text slowdown.

The frozen DejaVu Sans 2.37 Regular “AV To”, 18 px, 384×160 RGBA16 canvas and
origin 8/8 are unchanged. The reference remains ImageSharp.Drawing 2.1.7 / Fonts
2.1.3 / ImageSharp 3.1.12. Managed preparation retains copied geometry, crossings
and a coverage row; the reference retains positioned/flattened paths and prepares
working storage per Fill. Neither preparation nor canvas reset is timed here.
The existing complete workloads remain the preparation/ownership comparison.

After a three-actual screen, one independent twelve-actual confirmation groups
1,024 individually reset draws per actual. Both sides use two timestamps per draw;
all complete public drawing work and adapter dispatch stay timed. Exact managed
canvas validation and allocation-counter reads occur outside that timer. The
unchanged worker supplies reference brackets and its original correctness checks.
Existing full-word/visual, layout, ownership and frozen-input checks pass. No new
font, script, shape or settings grid is added.

Managed drawing is **52.003 µs / reference 37.642 µs**, ratio **1.382**, conservative
99.9% interval **1.076–1.613**: a scoped gap. All twelve actuals and all three
reference groups remain; no overhead subtraction, outlier exclusion or favorable
retry. The interval envelopes managed/reference arithmetic intervals and paired
log ratios. The older one-invocation result remains history, not a causal speedup.

Measured allocation is **80 B/draw including the environment-dispatch adapter**.
The earlier **56 B public-call-only** counter is separate, not a new current
measurement. Active raster capacity remains **40,408 B**; each draw adds no admitted
storage. The borrowed 491,520 B canvas, font retained by this harness, idle pools,
reference allocations and physical peaks are not included in that raster figure.
Reference allocation is unmeasured by this unchanged worker, not zero.

One small driver build reuses the original harness and reference worker; production,
embedded tests and native tools are not rebuilt. The first guard caught our compiler
server before timing; shutdown resolved it. A zero-sample startup used the wrong
dependency manifest; the existing immutable driver manifest fixed binding without
rebuilding. Those failures and the local recovery assertion are retained.

The [current profile](TEXT-PREPARED-CURRENT-ATTRIBUTION.md) supports a bounded paired
endpoint investigation, preserving each coordinate's arithmetic order, polygon
order, scalar fallback, cancellation and ownership. No candidate is accepted.
Unchanged Windows/Linux 6,714 full/live checks, actual package and three examples
are reused; no broad qualification campaign is repeated.

[Evidence](text-prepared-draw-aggregation-evidence.json) records all observations
and boundaries. Mirrored/member-verified raw `tmp/text-prepared-draw-aggregation.tar.xz`:
54,244 B / 57 members, SHA-256
`988a1abe76602691d228d9eb95211d3ff317b5f63c473971d0b3c56ea27d22fa`.
