# Bicubic row-window hoisting: rejected

The fixed TightCross16 screen measures 1,362.661 µs, 1.572× (upper 1.714),
1,063,410 B/op. It fails the 5% gate against the fresh unchanged-production
1,263.401 µs screen. No gain or regression claim follows from unmatched short
screens; there are no variants or independent confirmations.

The trial hoists four/five vertical row windows and broadcast weights outside
the output-pair loop. Height preservation/enlargement fixes scaleY=1, so the
radius-two support has four or five taps. Multiply/add order, rounding guards,
exact replay, cancellation and the 4 KiB reservation remain unchanged. Exact
managed rational/frozen pixels, native RGB/alpha maximum 1 and RMSE ≤0.25, input,
ownership and quiet guards pass. Candidate and restored production each pass
16 affected public cases normal/scalar on Windows and Linux.

The original source, actual DLLs and package are restored exactly. Unchanged
full/package qualification is reused. [Evidence](bicubic-row-hoisting-rejected-evidence.json)
records the decision and retained pins. Mirrored/member-verified raw
bicubic-row-hoisting-rejected.tar.xz contains all observations, sources, producers,
selectors, checks and guards: 114 members, SHA256
0a7119f711b3dbd936748a9fb8af464952e1049da333a71327d27306bb762e3e.
The full 268-path objective remains open. This closes the row-window/weight-hoist
lead; further work moves to the independently confirmed Down8 path.
