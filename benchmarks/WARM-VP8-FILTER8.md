# Eight-pixel VP8 loop filtering

Opaque VP8 independently confirms **1.803x FFmpeg / conservative upper 1.808x**,
closing its previous upper-2.004x gap. The complete managed decode mean falls
**8.8%** against the retained baseline, with unchanged **760,548 B/op** and exact
frozen pixels. The alpha-bearing case also confirms **1.803x / upper 1.819x** at
unchanged **1,966,616 B/op**. Uniform current-source coverage remains open.

| Workload / stage | Lokad.Image us | FFmpeg us | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Opaque, original three-actual baseline | 9,480.69 | 4,804.53 | 1.973 / 2.230 | 760,548 |
| Opaque, SIMD three-actual screen | 8,675.39 | 4,776.41 | 1.816 / 1.905 | 760,548 |
| Opaque, independent twelve-actual confirmation | 8,649.43 | 4,797.64 | 1.803 / 1.808 | 760,548 |
| Alpha, three-actual control | 10,822.68 | 5,959.82 | 1.816 / 1.876 | 1,966,616 |
| Alpha, independent twelve-actual confirmation | 10,777.02 | 5,977.48 | 1.803 / 1.819 | 1,966,616 |

One production primitive processes eight independent horizontal loop-filter
segments in `Vector128<short>` lanes. Bounded eight-byte loads widen losslessly;
integer threshold and variance masks select the original signed-clamp,
strong/weak and positive/negative rounding formulas from RFC 6386 section15.
Results clamp to0..255 before narrowing. Fully validated horizontal windows of
size8/16 use the kernel when hardware acceleration is available. Other lines
retain the original scalar path. Work is charged before either path, and no
heap workspace, output cache, dependency, unsafe code or new tolerance is added.

Four new test cases establish the scalar reference before production changes on
both OSes. The candidate passes **28 focused checks per OS/mode**, with normal
and disabled intrinsics. Each mode includes **1,280 whole-plane comparisons**
covering both block sizes, edge presence, smooth/mixed samples, thresholds,
sharpness, simple/normal and strong/weak behavior. Original work totals,
work rejection and cancellation are checked. The original scalar Segment remains
the reference, backed by existing hand-computed sample tests.

Both full-image profiles retain every original managed RGBA sample, exact alpha,
source bytes, pinned native CLI output and independent live owners. The original
513x259 byte raster and its existing opaque projection are reused; frozen VP8
packets, native worker and quality policies are unchanged. No new original image
or quality/profile search is introduced. A previous diagnostic profile attributes
19% of samples to loop filtering; its failed post-profile guard makes it
attribution only, with no timing claim.

Measurements time complete public decode, owned output acquisition/materialization
and disposal. Native decode/state reset, centered bilinear chroma expansion,
limited BT.601 conversion through RGBA16, no-dither byte reduction, alpha handling
and scratch/output release remain included. Context/packet/frame wrappers may be
reused; all pixels are decoded every operation. One pinned CPU, one codec/
conversion thread, normal SIMD, tiering disabled and workstation nonconcurrent GC
remain fixed. Startup, file I/O, IPC, fixture creation and validation are excluded.
Native allocations are unmeasured; managed figures include harness overhead.

All versioned pre/post quiet guards pass. Both confirmations retain and select
all twelve actuals; conservative bounds include the larger filtered/all-actual
interval against the faster native bracket's lower bound. The same immutable
candidate core and existing harness serve screens, controls and confirmations.
Only changed-case/job drivers are prepared. The real alpha-control mean passes
before full qualification; no production/harness rebuild occurs within the
frozen candidate experiment.

Windows and Linux each pass **3,293 Release tests**, including **896 live FFmpeg
comparisons**. The consumer and all three examples pass through the **same actual
dependency-free Release package** on both systems. The fully qualified Linux DLL
is byte-identical to the timed DLL. SPEC.md records the exact scalar/vector
contract. No repeated whole-library method audit is performed.

The [numeric evidence](warm-vp8-filter8-evidence.json) retains every sample,
guard, allocation, actual child/source identity and package qualification. Earlier
arithmetic-reader trials remain rejected. Stop both confirmed VP8 decode pairs;
remaining material branches and final current-source uniform coverage stay open.
