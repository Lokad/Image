"""Render original same-input VP8 decode observations; no local timing claims."""
import json,statistics
from pathlib import Path
root=Path(__file__).resolve().parent;v=json.loads((root/'warm-vp8-read-inline-evidence.json').read_text())
assert v['allActualTimedProductionIlAndFlagsMatchPackage'] and v['unchangedAllProductionMethodBodies']['onlyVp8ReadInliningFlagChanges']
table=[]
for name,r in [('Retained decode screen',v['baseline']),('Grouped shift, rejected',v['rejectedGroupedShift']),('Read inline screen',v['candidate'][0]),('Read inline 12-sample repeat',v['candidate'][1])]:
 means=[statistics.mean(s['NanosecondsPerOperation'] for s in r[k])/1000 for k in ['nativeBefore','nativeAfter']]
 table.append(f"| {name} | {r['managedStatistics']['Mean']/1000:.2f} | {means[0]:.2f} / {means[1]:.2f} | {r['worstBracketRatio']:.4f}x | {r['conservativeRatioUpper']:.4f}x |")
text=f"""# Warm VP8 arithmetic reader inlining

Inlining the existing arithmetic reader reduces the retained complete VP8
decode mean about {v['screenedMeanGainPercent']:.1f}%, from 2.64x to {v['candidate'][1]['worstBracketRatio']:.2f}x FFmpeg.
This case remains above 2x; uniform warm parity and material coverage are open.

The [numeric evidence](warm-vp8-read-inline-evidence.json) preserves raw samples,
uncertainty, identities, allocation counts, quietness and immutable source/raw
archive hashes. Regenerate with `python benchmarks/render-warm-vp8-read-inline.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
"""+'\n'.join(table)+f"""

The only production change is `AggressiveInlining` on `Vp8BooleanReader.Read`.
All 530 production method bodies are identical before/after; exactly this one
implementation flag changes. Work remains charged before probability/interval
validation, refill and extraction. Bit consumption, partially advanced failure
state, cancellation, work/memory/output admission, precision and ownership remain.
No buffer, table, cache, unsafe code or dependency is added.

A first independent candidate grouped renormalization bits within each byte,
using leading-zero count with a shorter-span fallback. All scalar state controls
passed, but the single screen gave no useful complete-operation gain. It was
discarded without a repeat. Its source, raw samples and actual timed DLL remain
archived. The original bit-by-bit recurrence is retained in production.

One retained CPU profile identified the reader as a large cost before this
change. An optimized-JIT inspection then found two reader calls in Coefficients.
Inspecting the saved actual statistical DLL after inlining finds none; that
method grows from 600 to 1443 bytes. Both diagnostic decodes retain frozen
pixels and both pre/post quietness checks pass. These inspections establish code
shape, not cost attribution. The old CPU profile is not current after inlining.

The same existing original 513x259 RGBA8 raster and frozen native quality-75
VP8/ALPH input are used throughout. No new image, encode-quality search,
regeneration or broad matrix occurs. Identical-input decode does not require
a matched encoder pair; the unmatched VP8 encode catalog is not timed.
Native RGBA matches the pinned CLI exactly. Managed RGBA retains its prior hash,
alpha is exact and RGB max1/RMSE0.132879, including hidden RGB, satisfies the
specified max4/RMSE1.5 bounds. Geometry/orientation, source integrity, independent
live owners and mutation isolation pass.

Both subjects perform complete resident-input decode and independent owned
output acquisition/release. Native timing includes decoder reset, centered
bilinear chroma expansion, limited BT.601 conversion through RGBA16, no-dither
RGBA8 reduction, exact alpha merge and scratch/output allocation/release.
Contexts and packet/frame wrappers are reused; padded rows are exposed directly.
Startup, I/O, IPC, encoding, hashing and validation are excluded. Sequential
Release jobs use one CPU/thread, normal SIMD, CPU 0 on the dedicated EPYC 9V45 VM,
.NET 10.0.8 / SDK 10.0.204, disabled tiering and non-concurrent workstation GC.

Twenty-one new controls pass against the original reader before editing on both
OSes. They compare every normalization width, byte cursor/crossing, probability
and interval boundaries, successful/failing state, work/cancellation precedence,
literal/tree reads and shorter-span failures against the original scalar
recurrence. Two private wrong-state mutations are rejected. All 53 VP8 controls
pass with normal and disabled intrinsics on both OSes. All 2,084 Release tests
pass on Windows and Linux: 504 internal, 686 public and 894 live FFmpeg comparisons.

The same actual dependency-free Release package passes all three usage examples
on both OSes. Both saved statistical DLLs and the fresh Linux assembly match it
in all production method bodies/flags; fresh Windows library bytes match the pack.
Fixed-count complete public decode/disposal allocation counts are exactly equal
between the old/new actual packages across three repeats of sixteen operations.

The candidate has one short screen and one twelve-sample repeat. The baseline
is a short screen, so the improvement is relative to its retained point estimate.
Statistical prequiet passes and postquiet fails under unchanged thresholds;
original cooldown failures remain archived. Independent quiet confirmation and
required/material coverage remain open. No universal <=2x result is claimed.
"""
(root/'WARM-VP8-READ-INLINE.md').write_text(text,encoding='utf-8')
