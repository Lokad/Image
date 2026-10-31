# Alternating wider bicubic tap sums: rejected

One existing Wide8 short measures **587.187µs**, versus fresh original
**532.115µs**:10.35%higher raw/10.52%normalized point means. It fails continuation;
there is no independent candidate confirmation or unroll-count variant. These
shorts establish no confirmed regression or change to accepted Wide8 costs:
**529.180µs,1.518×/upper1.534,231,239B/op** remains current.

The trial splits each paired output's horizontal taps into two independent
binary64 sums, combining once. This shortens addition dependencies, with no
FMA,precision,coefficient,margin,guard,repair,storage or ownership change. A cold
exact-rational bound verifies no larger forward-error envelope for every
supported tap count through2048. Retained WCP1 samples/JIT motivate the trial;
they are diagnostics, not component elapsed measurements. The source-level
dependency reduction did not produce a complete-operation CPU win.

Frozen rational/frame pixels, original native RGB/alpha max1/RMSE≤.25 and hashes,
source/output ownership and quiet-v2 pass. Allocations231,239→231,236B/op reflect
3B measurement noise; owned capacity is unchanged. Native point drift is−.15%.
Seven original source pins match; the fresh original is+.55%raw/−.38%native versus
the retained original twelve. No new fixture,profile,worker or selector build.

One core/public-test build perOS passes221focused checks in every normal/scalar
mode, plus LinuxAVX512-off. Four new public checks cover exact-reference full
frames for8/16bit,unequal supports,odd tails,zero/tiny/full alpha and surviving
outputs. Qualified source/core/PDB/XML are restored byte-exactly; all221checks
pass against restored production too, with no restoration rebuild. Full5,314/OS
and the same actual package/three-example proof carry forward with7public and
4internal checks separately qualified (5,325applicable; no fresh full/package run).
Published releases and the current development package remain unchanged.

[Evidence](bicubic-alternating-tap-sums-rejected-evidence.json) pins all actuals,
source/error proof, candidate/restored producers and qualification reuse.
Mirrored/member-verified raw bicubic-alternating-tap-sums-rejected.tar.xz SHA256
b0da4a870c8d15ac73cae1203cb4c08314ecb9b951ba5df03fb710860b4a84a5 (1,773,516bytes/115members) retains source,
cores, tests, fixtures, workers, selectors, guards and the reused diagnostic.
The primitive is closed; all268required paths and existing costs remain in scope.
