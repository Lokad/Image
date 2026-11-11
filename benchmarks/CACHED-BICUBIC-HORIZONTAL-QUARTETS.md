# Prepared horizontal bicubic quartets

For current cached costs and package proof, see the
[family status](WARM-COVERAGE.md#current-family-status). Measurements below retain
this stage's design, incremental effects and raw evidence as history.

The existing cached Wide8 complete Resize improves **374.39→350.57 µs**:
**23.82 µs / 6.36% raw / 6.25% reference-normalized**. Filtered/all-actual raw
and conservative normalized confidence intervals separate. Native time changes
−0.124%; both views remain explicit. This is an incremental gain: ratio 1.008,
range 1.001–1.014 remains a small demonstrated gap, not a parity pass.

All 24 existing shared factory/executor costs refresh on the same immutable build:
**22 pass / 2 gaps / 0 uncertain**. The second gap is default narrow Down8, upper 1.063.
All eight equivalent wide/narrow one-shot/prepared comparisons pass; narrow
prepared byte's previous uncertainty is resolved. Passing cases stop.

| Complete public operation / explicitly labeled zimg | Lokad / native µs | Ratio / upper | GC B/op | Retained plan B | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Wide8 | 350.57 / 347.95 | 1.008 / 1.014 | 198,449 | 0 | gap |
| Wide16 | 384.90 / 438.63 | 0.877 / 0.890 | 198,446 | 0 | pass |
| Down64x32_8 | 379.24 / 403.54 | 0.940 / 0.946 | 197,682 | 0 | pass |
| Down64x32_16 | 363.96 / 387.95 | 0.938 / 0.956 | 197,680 | 0 | pass |
| Down32x16_8 | 309.20 / 375.55 | 0.823 / 0.830 | 197,300 | 0 | pass |
| Down32x16_16 | 305.36 / 413.33 | 0.739 / 0.748 | 197,296 | 0 | pass |
| Down16x8_8 | 315.59 / 487.14 | 0.648 / 0.656 | 201,203 | 0 | pass |
| Down16x8_16 | 309.03 / 525.34 | 0.588 / 0.619 | 201,200 | 0 | pass |
| Down8 | 435.54 / 418.81 | 1.040 / 1.063 | 199,983 | 0 | gap |
| Down16 | 434.97 / 575.48 | 0.756 / 0.767 | 199,959 | 0 | pass |
| Horizontal8 | 460.67 / 609.23 | 0.756 / 0.777 | 199,977 | 0 | pass |
| Horizontal16 | 575.61 / 630.12 | 0.913 / 0.922 | 199,930 | 0 | pass |
| Vertical8 | 479.15 / 594.10 | 0.807 / 0.865 | 367,677 | 0 | pass |
| Vertical16 | 454.58 / 596.49 | 0.762 / 0.801 | 367,598 | 0 | pass |
| Cross8 | 838.58 / 894.73 | 0.937 / 0.949 | 199,938 | 0 | pass |
| Cross16 | 820.54 / 881.47 | 0.931 / 0.941 | 199,891 | 0 | pass |
| wide/Repeated8 | 276.17 / 371.76 | 0.743 / 0.751 | 137 | 198,432 | pass |
| wide/Repeated16 | 354.89 / 448.31 | 0.792 / 0.801 | 137 | 198,432 | pass |
| wide/OneShot8 | 372.90 / 1074.07 | 0.347 / 0.352 | 198,456 | 0 | pass |
| wide/OneShot16 | 425.63 / 642.79 | 0.662 / 0.668 | 198,452 | 0 | pass |
| narrow/Repeated8 | 409.35 / 416.50 | 0.983 / 0.989 | 137 | 199,840 | pass |
| narrow/Repeated16 | 423.30 / 622.41 | 0.680 / 0.688 | 137 | 199,840 | pass |
| narrow/OneShot8 | 482.51 / 1038.61 | 0.465 / 0.493 | 199,990 | 0 | pass |
| narrow/OneShot16 | 506.63 / 778.01 | 0.651 / 0.656 | 199,983 | 0 | pass |

Ordinary rows include complete managed geometry/storage preparation, source reads,
exact computation/certification/repair, independent output and disposal; their
zimg 3.0.6 reference retains graph/scratch as originally declared. The eight
one-shot/prepared rows compare equivalent public boundaries and never sum
historical stage costs. Frozen inputs/settings/full-depth pixels, original native
float bounds, source/ownership checks and CPU0 idle-v2 guards remain unchanged.
Every actual/native bracket remains, including filtered outliers. No new images,
quality/filter/settings grid or successful favorable repeats.

For four count 16/+4 windows in 32-slot records, each live coefficient block leaves
16 unused doubles. Four tails hold 64 interleaved coefficients: no new capacity,
field, owner, reservation or pool. Original live weights/offsets/counts/normalizers,
pair markers and repair inputs stay unchanged. A distinct dead offset slot marks
the group. Unequal/17tap/clamped/shifted/short windows keep the original pair,
indexed and odd fallbacks. Preparation occurs once per operation or owned plan.

The numeric quartet delays seven two-pixel vectors, reading 28 pixels versus 40 for
two existing overlapping pairs. Two ordered pair accumulators retain every lane's
t0..t15 Add(Multiply) sequence; no FMA, regrouping or numerical policy change.
Coefficient packing is reused from the owned padding on every decoded row.
The canonical source-only model has 30 groups/15,360 B reused padding; counts are
not phase costs or CPU forecasts.

A already has exact pixels and no vector spills, but its independent result
392.73/native 366.50 µs versus predecessor 374.39/native 348.38 supplies no useful
normalized gain. It stays unselected. B bounds private views once and replaces
signed division/modulo with equivalent masks over fixed nonnegative taps.
The actual timed-module kernel shrinks 646→566 B with the same 16 B frame and no
vector spills. Code size alone proves no speedup. Startup-only private-kernel
disassembly is complete before warm actuals; there is no loop instrumentation.

Other current costs supply no uniform causal speedup/nonregression claim.
Horizontal word raw time rises 3.96% while native rises 6.00%; vertical word
rises 2.78% with native+1.37% and overlapping raw intervals. Their normalized
intervals overlap and both pass. All 24 costs reconcile before broad qualification;
no material pass-to-gap or separated normalized adverse contradiction remains.

Total GC and capacity remain separate. Wide8 GC 198,449 B/op versus 198,451 earlier
reflects amortized instrumentation, not an array/capacity reduction. Prepared
GC 137 B/op and declared wide/narrow retention 198,432/199,840 B remain unchanged.
The same actual package consumer on Linux measures coarse governed owned peaks
231,200/263,968 B for byte/word, including independently retained results 32,768/65,536 B
and excluding borrowed source. Windows uses the portable fallback: zero plan
retention and coarse owned peaks 32,768/65,536 B. Operation scratch retention after disposal is 0;
physical/native/JIT peaks remain unmeasured.

Nineteen new focused cases verify live/padding preservation, exact ordered-double
bits across eight existing geometries, poisoned storage, rejected layouts, source
guards, no allocation/admission, cancellation and rational low-alpha byte/word
public-plan survivors. Windows/Linux each pass **6,382 full/live checks** and
473 internal/789 public resize checks in normal/scalar/AVX2-off/AVX512-off modes,
including the three named storage suites. Other codec/text ISA evidence is reused.
The SAME actual Release-package consumer/all three examples pass both OSes,
including both frozen wide byte/word frames, cancellation/admission and survivors.
Two coherent main/embedded versions per OS, no compile failures or public/worker/
fixture builds, one broad/package milestone and one pack. Published releases stay
unchanged; the private 0.2.1 package is not publication.

Package `876f228906eead5f0dd9bac0178ad0e7982a5016022a8258b4c9808dbf5cbc7f` contains timed DLL `dd51e32c984ffd4b76ba5e9fa9964bf4ab8eed70d805d2393d0d62101a618ee6`.
[Evidence](cached-bicubic-horizontal-quartets-evidence.json). Raw `cached-bicubic-horizontal-quartets-qualified-v2.tar.xz`:
3,246,296 B/372 members, SHA256 `1ca4d419934b2626963d3bb69f1748534fbd8613497e35c98531f9b5843f347c`,
mirrored/member-verified with prior reference/profile/qualification dependencies.
Earlier [cached-wide](CACHED-WIDE-SHARED-QUARTETS.md),
[narrow-publication](CACHED-NARROW-SHARED-PUBLICATION.md) and
[preparation](BICUBIC-SHARED-PREPARATION.md) reports retain history; this report
owns their current affected costs. All 289 material paths remain required.
