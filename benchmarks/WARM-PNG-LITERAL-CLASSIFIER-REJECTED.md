# Rejected literal classifier experiment

The framing walk counts literal bytes without using their values. This experiment
collapsed literal lookup entries to their consumed lengths and classified them
with one unsigned range check. EOB, matches, long/unassigned prefixes and the
existing fallback retained their dispatch. No workspace or work admission changed.

One quiet three-actual screen reused the existing 513x259 gray8 PNG and its
original-raster projection, with the same resident-input/owned-RGBA boundary.

| Screen | Lokad.Image µs | FFmpeg µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Accepted dense expansion | 836.63 | 372.62 | 2.245 / 3.003 | 620,246 |
| Experimental classifier | 811.07 | 371.84 | 2.181 / 2.522 | 620,241 |

The mean is only 3.1% lower and remains above 2x. The experiment was **reverted**;
no independent twelve-actual repeat was run. Both unchanged quiet guards pass,
all actuals are retained, and the upper uses the larger filtered/all-actual bound.
This screen does not establish a durable small speedup. The summary retains the
accepted implementation's result. Allocation is raw GC bytes/op rounded up,
including harness overhead; native allocation totals are not measured.

Candidate and restored builds each pass the 98 framing tests on Windows and
Linux, including every tight budget, output overflow, truncation and reserved-code
failure. After restoring original accepted bytes, forced rebuilds on both OSes
match all 575 production methods and flags to the accepted actual package.
The accepted implementation's full suite/package validation remains unchanged.

The [numeric evidence](warm-pngliteral-classifier-rejected-evidence.json) preserves
the measured source, raw archive, four actual child identities, native brackets,
checks and restoration evidence. No additional original performance image was used.
Next, screen one gray-alpha expansion profile derived from an existing raster;
material unknown paths take precedence over repeating this small classifier tweak.
