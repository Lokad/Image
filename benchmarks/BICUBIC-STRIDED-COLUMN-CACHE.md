# Strided bicubic column workspace

The complete tight-memory RGBA8 reduction saves **19.18 µs /
3.36% raw / 3.30% normalized**:
**571.36 → 552.17 µs**. Filtered and all-actual raw
and normalized confidence intervals separate. Allocation remains **113 B/op**.
The current zimg reference is **417.53 µs**, ratio1.322,
lower1.308/upper1.337: **parity remains open**.

This is the existing 513×259 RGBA8 → 256×129 control, local ceiling667,660B
(borrowed source531,468 + independently owned result132,096 + scratch4,096).
The exact Windows-produced DLL **8baf7021159a78bf0f2c2524c9ccb3ec33537b41776c3f977c8c8605697aae1a**
is both measured and subsequently packed/verified on Windows and Linux.
The [unchanged contemporary original](TIGHT-BYTE-BICUBIC-CURRENT-PACKAGE.md)
provides the independent baseline; native points move−0.061%. Each confirmation
has12actuals and12native samples per bracket; high actuals remain in the all-
actual view. The earlier3actual screen is separate, without pooling or repeats.

## Shared preparation and execution

One original numeric range implementation serves a16-column prefix and the
original8/6-column suffix. Each cache slot in the prefix borrows512B from one
unpublished output-row tail; its vector pitch follows the output stride.
For this control,9slots borrow4,608B in rows0–8 at byte[416,928), before the
existing96B geometry footer. Prefix columns0–95 finish before those bytes are
overwritten. The small-stack suffix completes every remaining pixel.

Prefix3,232B and suffix4,080B stack payloads occupy separate NoInlining calls,
so they retire between phases. The outer non-inlined core holds the existing
4KiB reservation once and unwinds before exact tiny-alpha replay. No extra
owned capacity, owner, cache, pool, public API or retained scratch. The independently
owned result stays charged132,096B until disposal; two retained results plus
workspace peak at268,288B in the governed consumer. Physical/native peaks are
unmeasured. Small/unaligned outputs retain the real original-capacity fallback.

The model changes32→26strips and4128→3354driver rows(−18.75%), source conversions
181300→171976(−5.14%). Horizontal filtering66304pixels,16512paired publications
and3unprepared-geometry strips remain. These counts describe removed work;
the elapsed gain is established by complete measurements. Original taps, sum
order, reciprocal/Newton certificate, normalization, OriginalByte, clipping and
fallback remain source-exact. Constant value tags select capacities; there is
no virtual dispatch in pixel loops.

## Boundaries and qualification

The managed operation includes public Resize, admission, geometry preparation,
working storage, independent result allocation/materialization and disposal.
The explicitly labeled FFmpeg zscale **zimg backend** retains graph/input/scratch
outside timing; input/halo conversion, filtering, independent output, repacking
and release are timed. This is not FFmpeg CLI or equivalent one-shot setup.
Frozen inputs/settings and exact managed rational pixels are preserved; approved
native-only max1/RMSE≤.25 remains distinct. Quiet idle-v2 guards, CPU0, tiering off
and nonconcurrent workstation GC are unchanged.

Windows and Linux each pass **5,973 full/live checks**
(2708internal+2317public+948differential),90internal/556public per normal/scalar,
and206internal/703public per disabled-AVX512/AVX2 mode. Nine new checks include
six bounded shapes: loan/suffix poisoning, ten-tap maximum, narrow/short/unaligned
fallbacks, exact frames, source lifetime, terminal refusal, cancellation/recovery,
real tiny-alpha decline/replay and aggregate independently retained outputs.
These are functional generalization, not broader performance claims.

One successful production and embedded build per OS is reused throughout.
An initial internal compile rejected the unscoped loan parameter; it was marked
scoped before any module/timing was produced. The failed log and initial edit
are retained. No new fixture, profile, benchmark harness, selector or native build.

The SAME actual private Release0.2.1 package **1b010016f3d17117ecf476660041322d28411bce03fea43cdc210ac731d4a9eb**
(435284B), symbols **57a32a62cae339b4d66352bcb49d044ae2278dcc1639bc8a7ecce69d270c01f7**,
passes both OSes, allthree examples and the added packaged column ownership
control. Published0.2.0 remains unchanged. Publication is postponed.

Only eligible byte both-axis≤10tap column execution changes. Default cached,
word, enlargement, one-axis, wider-support paths, codecs, text and drawing
retain source-valid evidence. Unmeasured affected layouts/pipelines have no new
CPU claim. All283required-path parity and a full release refresh remain open.

[Compact evidence](bicubic-strided-column-cache-evidence.json) owns this cost.
Raw **bicubic-strided-column-cache-qualified.tar.xz**, SHA256 **d293d2a0d8618b0144f601f0d22a53fe79600ffa8dc00235ce2e13c0ec764ebc**,
13953032B/244members, is mirrored and member-verified.
It retains final/original sources/modules, scripts, logs, guards, receipts,
manifests, actual packages, consumer source and logs. Reproduce with TCS643 prepare/edit/
build/focused, measure/observe/compare, qualify/ISA/package scripts; corrected
edit includes the scoped loan. TCR642 baseline rawee384e5d and MBQ635 qualified
rawc376dfbd remain dependencies. Superseded experiments stay linked history;
their checks do not become current renderer dependencies.
