# Warm RGBA8 normal orientation, vertical flip and half turn

Normal RGBA8 orientation independently confirms below 2x FFmpeg, including
conservative uncertainty and both quiet guards. Vertical flip and half turn
remain **OPEN** because their confirmation guards failed. Numeric bounds alone
do not qualify them. Production and benchmark implementations are unchanged;
full required warm coverage remains open.

| Fixed pair | Lokad.Image microseconds | FFmpeg microseconds | Ratio / upper | Managed allocated B/op | Actual / filtered | Status |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Normal orientation RGBA8 | 9.92 | 7.98 | 1.24275 / 1.29555 | 128 | 12 / 12 | Confirmed |
| Vertical flip RGBA8 | 9.28 | 8.42 | 1.10243 / 1.12892 | 128 | 12 / 11 | Guard failed / OPEN |
| Half turn RGBA8 | 12.22 | 11.27 | 1.08398 / 1.10892 | 128 | 12 / 12 | Guard failed / OPEN |

Failed-guard figures are retained observations, not certified performance claims.
All measurements use owned source SHA-256
`fa5ef14a559861226e18296f7a0dd35fb2a4ccd3f9c584ae0cc0735689e0155d`,
revision `16a4eb11ff48228f9de15ba86d7ca8fb53f68879`. One existing original
513x259 RGBA8 raster is reused, without new images, profiles, shapes or quality
searches. Every channel, alpha, hidden RGB, independent owned output and source
integrity remains exact. Both normal and scalar public FFmpeg CLI checks pass
before timing. Output dimensions and depth remain unchanged.

Each pair retains its three-actual screen and twelve-actual confirmation, with
three/twelve native samples per pre/post bracket. Normal orientation's screen
failed its post-run guard; its independent confirmation passes both guards.
Vertical flip's initial confirmation precheck launched no benchmark and was
archived before one bounded fresh confirmation, which failed its post-run guard.
Half turn's first confirmation failed its post-run guard; the one bounded fresh
attempt failed before launch and was archived. Both cases remain open; no further
repetition or failed-interval recategorization occurs in this round.

Native brackets target 50 ms; managed iterations target 200 ms. All actuals are
retained and upper is the larger filtered/all-actual conservative bound.
`exclusive-vm-idle-v2` keeps live-job checks and five-second aggregate/CPU0
samples: busy <=5%, I/O wait <=1%, steal <=1%. No threshold changes, historical
cooldowns or other operator waits are used. Four actual child identities and
original receipts are frozen before subsequent preparation or source mutation.

Timing includes complete public AutoOrient/FlipVertical/Rotate(Half), admission,
independent output acquisition, full materialization and disposal. Released
managed pixel storage may be reused; simultaneous live results remain independent.
Native null/vertical graphs initially return shared views, then allocate and copy
owned top-to-bottom output inside every operation. Native half turn materializes
its horizontal flip, then reverses that owned raster's row stride; the independent
negative-stride result is exposed without an extra packing copy. Native graph/
source/frame wrappers are reused; no output is cached. Startup, I/O, IPC,
preparation, hashing and validation are excluded. CPU0, one filter thread,
normal SIMD, tiering off and nonconcurrent workstation GC remain fixed.

Managed allocation is **128 B/op**, raw GC bytes / timed operations rounded up
including harness overhead. Native allocations are unmeasured. All 569 production
method bodies and flags in every actual child match the same accepted Release
package. Its preceding 2,390 tests and 39 scalar Huffman controls per OS, plus
actual-package/all-three-example checks on both OSes, remain applicable;
they were not repeated without production/package changes.

[Numeric evidence](warm-copies8-quiet-evidence.json) retains screens, all actuals,
brackets, source/input/output/child/package identities, guards and immutable
archive digests, including failures. Compiler prefix paths are normalized only
in public copies; original receipts stay bound by their digests. See the
[current three-table summary](../docs/benchmark.md) and
[measurement recipe](../tools/WarmReference/README.md).
Regenerate with `python benchmarks/render-warm-copies8-quiet.py`; `--check`
verifies this report. Stop tuning passing points; unmatched and uncovered scope
requires separate work, and failed quiet confirmations remain open.
