# Opaque predictor histograms: two inconclusive stages

Existing global opacity analysis can replace per-sample alpha histogram updates
with exact final tile counts and omit known alpha clamp/error contributions.
Eligibility is forwarded only through nonpalette prediction; subtract-green
preserves alpha. Palette-normalized and normalized-ALPH inputs remain general.
Full logical histograms, histories, parent sums, costs, candidates and packets stay
exact. No new input scan, array, pool, owner or admission request.

OPA133 checks opacity within the shared accumulator. OPA134 dispatches once per
tile into two value-type specializations of the same formulas, making policy
constant in each kernel. Both are real staged implementations. All allocation,
admitted-peak and retained-scratch figures in the four-control screens are flat.

| Complete continuation screen | Photograph direct ms | Original three-actual ms |
| --- | ---: | ---: |
| OPA133 | 90.622 → 89.383 | 128.737 → 133.426 |
| OPA134 | 90.622 → 89.950 | 128.737 → 131.786 |

These short points preserve adverse observations; they do not establish causal
regressions or incremental CPU gains. Diagram, transparent and authored-control
points are also retained. The opaque companion's independent12-actual run gives
**75.294 ms /70.132 ms** against explicitly labeled libwebp exact=1, ratio1.074,
conservative upper1.084/lower1.063: still a gap. Allocation is12,956,744 B/op;
the complete121,896-byte packet is exact. There is no fresh before/after opaque
gain claim from subtracting historical points. Existing warm inputs, sampling,
settings, complete operation boundaries, native gates and quiet guards stay put.

Both implementations pass299 focused checks per normal/scalar mode on each OS.
Four added checks compare every logical mode against the full ARGB builder,
covering axis/clipped/border/right-wrap tiles, existing populations, randomized
clamp/error/tie arithmetic, borrowed storage and zero warm helper allocation.
Original frozen maps/packets, cancellation/refusal and ownership remain covered.

A bounded generated-method inspection confirms constant folding: no alpha-policy
getter or runtime dispatch calls remain. Baseline accumulator size is2,331 B,
opaque specialization1,965 B and full-alpha specialization2,280 B. This is code
inspection, not speed evidence or an explanation of all complete-operation costs.
The existing public worker first covers the opaque kernel; the original selector
then covers the nonopaque kernel. All diagnostic elapsed numbers are discarded.
Two missing cached-tool checks fail before any diagnostic encode; the available
verified selector is then used. No scored observation repeats for favorable data.

Restore **eight Windows/nine Linux assets byte-exact**, remove the candidate-only
helper tests/dispatch entry and reuse [5,497 full/live checks per OS and the same
actual package/three-example proof](RESIZE-OVERWRITE-OUTPUT-STORAGE.md).
Zero restoration builds, no broad/package rerun for identical production. Current
costs/package remain unchanged. The staged results do not earn added specialization
machinery; future materially different designs are not permanently excluded.

A subsequent unchanged-source audit checks exact plane-ordered predictor scores
across eight complete encodes. RGB photograph scoring has924,567 expensive
combined-count evaluations but420,513 distinct contributions; opaque/original
have332,542/331,234 evaluations and227,272/226,112 distinct. These are work counts,
not available elapsed savings. Eager count-one pricing would perform96,032
expensive preparations versus241,796 current photograph calls, but117,409/116,493
preparations versus57,288/56,738 calls on opaque/original: do not implement it
generally. Alpha counters are excluded because shared-alpha scoring already exists.
Next, simulate a direct per-bin last-count cache across the unchanged mode order,
including resets, access costs and medium/large terms. A plane-ordered design may
reuse alpha products only after their actual scoring users finish; no new buffers.

[Evidence](webp-opaque-histogram-stages-inconclusive-evidence.json) pins both
producers, every point, method inspection, restoration and price audit. Raw
webp-opaque-histogram-stages-inconclusive.tar.xz: 14,152,368 B /486 members,
SHA256 ddc08cbbe22136cc621ddf0f9e7634647fdad15450791949a26e1113e512bb22, mirrored/member-verified.
Price audit webp-prediction-price-audit.tar.xz: 445,864 B /25 members,
SHA256 ca9920f350127f28dbc94d543685016fd2a85588fd57cfad315ae1cb2a60bec2, mirrored/member-verified.
Earlier [green-support](WEBP-GREEN-SUPPORT-STAGES-INCONCLUSIVE.md),
[source-tuple](WEBP-SOURCE-TUPLE-PREPARATION-INCONCLUSIVE.md) and
[product-cache](WEBP-COLOR-PRODUCT-CACHE-INCONCLUSIVE.md) designs/results remain
separate. Their cited raw evidence and qualified-production dependency are retained.
