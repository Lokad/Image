# Current bicubic RGBA8 reduction cost

The existing Down8 control (513×259 → 256×129) independently confirms
632.921 µs, 1.471× the reference (conservative upper 1.663), 332,187 B/op.
All twelve actual samples and both native brackets are retained; nine managed
samples remain after filtering. The result establishes a gap, without a gain
or regression claim against historical screens.

The older independent confirmation predates removal of work/ownership accounting;
the published 0.2.0 screen has wide uncertainty. This round measures the qualified
current production DLL using the archived constant-case driver, current cached
harness and unchanged zimg worker (FFmpeg zscale backend). No component is rebuilt.
Input, dimensions, settings and complete independently owned output boundary stay
fixed. Exact managed rational/frozen pixels, native RGB/alpha maximum 1 and
RMSE ≤0.25, source ownership and idle/process guards pass. Current full/package
qualification is reused because production is unchanged.

[Evidence](current-bicubic-down8-cost-evidence.json) records all samples, confidence,
allocations and pins. Mirrored/member-verified raw current-bicubic-down8-cost.tar.xz
has 47 members, SHA256
39aa08cbb97e576c58f7e3521007e7b8f25fd228027173afea4d4eb32f1396f9.
A postcheck initially rejected only an absolute path in native provenance; the
original completed run was recovered after verifying the worker, source, builder,
flags and static archives. No rescore or reference adjustment was needed.

This does not complete the full 268-path warm CPU objective. TightCross16 retains
a larger current central ratio (1.522×); its existing attribution identifies a
distinct row-window/weight-hoisting lead for the next bounded experiment.
