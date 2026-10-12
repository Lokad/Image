# Warm RGBA8 crop/clone and RGBA16 horizontal-flip coverage

All three fixed pairs independently confirm below 2x FFmpeg, including
conservative uncertainty and both quiet guards. Production and timed benchmark
implementations are unchanged. Full required warm coverage remains open;
no averaging establishes uniform parity.

| Fixed pair | Lokad.Image microseconds | FFmpeg microseconds | Ratio / upper | Managed allocated B/op | Actual / filtered | Quiet guards |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Crop RGBA8 | 2.34 | 2.27 | 1.03283 / 1.03882 | 96 | 12 / 12 | Pass |
| Clone RGBA8 | 9.82 | 7.19 | 1.36555 / 1.39047 | 96 | 12 / 11 | Pass |
| Horizontal flip RGBA16 | 21.72 | 21.94 | 0.98960 / 1.00696 | 128 | 12 / 11 | Pass |

All final measurements use owned source SHA-256
`040085681beeeed99f35d86c771655aa0505ee49f21eff9ca088640bc05f0304`,
revision `1c1fd74596bad4cc1db9f66babf9d05dd1ae7f22`. The same two original
513x259 RGBA8/16 rasters are reused, without new images, profiles or quality
searches. Crop takes x=127,y=65 into 256x129 RGBA8; clone and horizontal flip
retain 513x259 at their respective depths. Complete channel samples, alpha,
hidden RGB, source integrity and independent owned outputs remain exact.

Crop and clone use one three-actual screen and one independent twelve-actual
confirmation, with three/twelve native samples per pre/post bracket. Crop's
screen failed its post-run guard; its initial confirmation precheck launched
nothing and was archived before one fresh confirmation. Clone's first precheck
also launched nothing and was frozen before one fresh screen. Final guards pass.
Horizontal RGBA16 uses one fresh twelve-actual confirmation after the intervening
three-pair round passed all twelve idle intervals. Its previous failed post-run
and bounded pre-run attempt remain failed and retained; no existing interval is
recategorized. Already qualified horizontal RGBA8 was not scored again.

Native brackets target 50 ms and managed iterations 200 ms. Every actual is
retained; upper is the larger filtered/all-actual conservative bound. All final
confirmations pass `exclusive-vm-idle-v2`, without threshold changes, historical
cooldown or other operator waits. Actual four-child identities and original
receipts are frozen before subsequent preparation or owned-source mutation.
The generated flip dispatch is checked: ID 1 selects RGBA16; preparation validates
both depths once, while only RGBA16 enters the fresh scored run.

Separate crop normal/scalar CLI controls passed before timing. The crop harness
previously constructed scalar arguments but removed them before execution;
after all evidence was frozen, it was fixed to execute that check and emit its
normal/scalar receipt flag on future runs. Its Release build and validation-only
smoke pass; production methods still match the accepted package. This changes
untimed validation only.
Clone's setup and separate horizontal-flip controls also verify both CLI modes.

Timing includes complete public Crop/Clone/FlipHorizontal, admission, independent
output acquisition, full materialization and disposal. Released managed pixel
storage may be reused; simultaneously live owners remain independent. Native
crop/null graphs initially return shared views, then allocate and copy independent
pixels inside every operation before release. Native horizontal flip materializes
its graph result. Native contexts/source/frame wrappers are reused; no output
is cached. Padded rows remain exposed. Startup, I/O, IPC, preparation, hashing
and validation are excluded. CPU0, one filter thread, normal SIMD, tiering off
and nonconcurrent workstation GC remain fixed.

Managed allocation is **96 B/op** for crop/clone and **128 B/op** for horizontal
flip, raw GC allocated bytes / timed operations rounded up including harness
overhead. Native allocations are
unmeasured. All 569 production method bodies and flags in every actual child
match the same accepted Release package. Its preceding 2,390 tests and 39 scalar
Huffman controls per OS, plus actual-package/all-three-example checks on both
OSes, remain applicable; these were not repeated without production/package changes.

[Numeric evidence](warm-cropclone8-flip16-evidence.json) preserves screens,
all actuals, brackets, source/input/output/child/package identities, scalar CLI
controls, dispatch, guards and immutable archive digests, including failures.
Compiler prefix paths are normalized only in public copies; original receipts
stay bound by their digests. The [previous horizontal-flip evidence](WARM-FLIP-ORIENTATION-QUIET.md)
retains its failures at its own source, without a newly certified before/after
speedup claim. See the [current three-table summary](../docs/benchmark.md) and
[measurement recipe](../tools/WarmReference/README.md).
Regenerate with `python benchmarks/render-warm-cropclone8-flip16.py`; `--check`
verifies this report. Stop tuning passing points and cover a remaining case.
