# Decode packed bicubic source rows once

BDW652 reduces the frozen complete 513×259 → 64×32 RGBA8 resize from
**544.63 to 485.40 µs**, saving **59.23 µs / 10.88% raw / 10.11% normalized**.
Both filtered/all-actual raw and reference-normalized intervals separate.
Allocation remains **113 B/op**. Current zimg measures 370.61 µs: ratio **1.310**,
conservative upper **1.314**, still a 114.79 µs gap. This is one scoped control;
the full 283-path objective remains open.

| Stage / actual iterations | Managed µs | zimg µs | Interpretation |
| --- | ---: | ---: | --- |
| Qualified baseline / 12 | 544.63 | 373.78 | Existing current-package evidence reused |
| Two-column decoded prefix / 3 | 1,107.15 | 366.88 | Adverse construction/spill cost; unselected |
| Explicit half insert / 3 | 542.90 | 371.12 | Flat, inconclusive complete-operation result |
| Four-column prefix / independent 12 | 485.40 | 370.61 | Accepted gain; parity remains open |

The two-column precursor freed stack capacity but its vector construction
generated dependent half inserts and a 64 B temporary spill/reload per tap.
One bounded code/profile capture retained that evidence. An explicit insert
removed the short regression; extending preparation to four columns then removed
repeated strip work. Earlier short stages receive no independent confirmation or
production selection. Twelve actuals/native brackets qualify the final variant;
there are no favorable repeats. Profiling clocks are discarded.

The four-column prefix decodes byte products to doubles once per selected source
row, replacing repeated UInt16 widening/conversion inside horizontal taps.
Products ≤65025 are exactly represented in both forms. Tap add/multiply order,
weights, normalizers, pair/scalar publication certificates and precision replay
remain unchanged. At scales ≤8.125, support has ≤33 samples and four adjacent
centers span ≤58 source samples. Other layouts retain the real packed suffix.
One constant-specialized numeric range serves both forms.

Thirty-two cache slots borrow the last 128 B in the first 32 unpublished result
rows; one slot stays on stack. Prefix pixels end before those tails. Separate
non-inlined calls retire prefix/cache users before suffix publication or precision
replay. Payloads **3880/3992 B** share the existing **4096 B** admission. There is
no new owner, cache framework, pool, public API or retained state. Refusal remains
terminal; source/root lifetimes and cooperative cancellation remain intact.

The count model reduces strips/driver rows **32→24 / 1024→768**, decoded source
pixels **325822→276094** and converted channels **2125872→1515668**. Horizontal
multiply/add work is unchanged. Logical decoded-table writes rise 2.61→4.93 MB
in the same workspace; these are cumulative writes, not allocated capacity or
physical bandwidth. Counts and sample regions are not phase times or predicted gains.

The complete operation includes normalization, preparation, working storage,
independently owned output and disposal. The reference is explicitly **zimg,
FFmpeg's scaler backend, retaining its graph/scratch before timing**. Fresh graph
construction is excluded there. Original input/settings, exact rational/frozen
managed frames, native max1/RMSE≤0.25, transparent RGB and ownership gates pass.
The local ceiling stays 543756 B: source531468 + retained result8192 + workspace4096.
Two governed retained results plus workspace peak at 20480 B. Added owned capacity
and operation-retained scratch are zero; physical/native peaks and existing idle
pool capacity are not newly measured.

Seven poisoned layout controls cover the frozen case, a second 96×48 shape,
small prefix, height/stride/support bounds and packed fallback. Tiny-alpha
decline/replay, terminal refusal, cancellation/recovery and independent surviving
owners pass. Windows/Linux each pass **5985 full/live checks** (2720 internal +
2317 public + 948 differential), focused normal/scalar and affected ISA checks.
Windows uses portable execution; Linux exercises the prepared prefix. Three
successful main/embedded variants per OS are retained; the final immutable build
is reused throughout confirmation and broad qualification.

The timed Windows DLL596a5be7 is embedded in the same actual private Release0.2.1
package **2db7ad498bf72dc794631877daafa98e92916eba0e22b1f92855028c1bc594ef**
(437161 B), verified on both OSes with the new frozen/aggregate-ownership consumer,
all existing checks and three examples. Published packages remain immutable;
publication is postponed.

[Structured evidence](packed-bicubic-decoded-prefix-evidence.json) pins all stages,
sources, allocation, confidence, model and qualification. Mirrored/member-verified
raw packed-bicubic-decoded-prefix-qualified.tar.xz contains 18061436 B / 407 members,
SHA256 4feb150eaa6d52e0ab481e00da9e1134d1465d812c1baa9f86168af83276567a.
The [baseline](PACKED-BYTE-BICUBIC-CURRENT-PACKAGE.md) and preparation-attribution
archive are pinned dependencies. Unchanged cached/wider/word/deep/axis paths and
codecs/text/compositing reuse valid evidence; other eligible layouts receive no
new CPU claim. The [current default wider-support refresh](CACHED-WIDE-BYTE-CURRENT-PACKAGE.md)
confirms a separate151µs gap after its allocation/scheduler changes.
