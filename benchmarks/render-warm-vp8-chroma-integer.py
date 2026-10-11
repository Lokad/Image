"""Render the retained one-pair integer centered-chroma evidence."""
import json,statistics
from pathlib import Path
root=Path(__file__).resolve().parent
v=json.loads((root/'warm-vp8-chroma-integer-evidence.json').read_text(encoding='utf-8'))
rows=[]
for name,r in [('Fixed token 12-sample baseline',v['baseline']),('Integer expansion screen',v['candidate'][0]),('Integer expansion 12-sample repeat',v['candidate'][1])]:
    a,b=[statistics.mean(s['NanosecondsPerOperation'] for s in r[k])/1000 for k in ('nativeBefore','nativeAfter')]
    rows.append(f"| {name} | {r['managedStatistics']['Mean']/1000:.2f} | {a:.2f} / {b:.2f} | {r['worstBracketRatio']:.4f}x | {r['conservativeRatioUpper']:.4f}x |")
text=f"""# Warm VP8 integer centered-chroma expansion

Exact integer interpolation reduces the retained complete VP8 decode mean
{v['screenedMeanGainPercent']:.1f}%, from {v['baseline']['worstBracketRatio']:.2f}x to
{v['candidate'][1]['worstBracketRatio']:.2f}x FFmpeg. This case remains above 2x;
required warm coverage and independent quiet confirmation remain open.

The [numeric evidence](warm-vp8-chroma-integer-evidence.json) preserves raw samples,
uncertainty, identities, allocations and immutable source/raw archive hashes.
Regenerate with `python benchmarks/render-warm-vp8-chroma-integer.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
"""+'\n'.join(rows)+"""

Only the source body of `Vp8Color.Chroma` changes. For the existing centered
mapping `(x+0.5)/2-0.5`, odd pixels have phase 1/4 and even pixels 3/4. The first
pixel clamps to phase zero; duplicated last samples retain the opposite edge.
The source index is `(x-1)/2`, using C# division toward zero at x=0. The same
construction applies vertically. Two integer interpolations have denominator16,
weighted sum at most255*16=4080, and half-up rounding `(sum+8)>>4`. This equals
the original double interpolation and floor exactly for every byte input: all
intermediate fractions are exact binary quarters/sixteenths. The selected chroma
samples round before the unchanged limited BT.601 double color matrix.

Eight new controls pass before editing on both OSes. They cover every byte
impulse at all centered phases, explicit half ties, edge extension, odd/even/thin
planes, padded stride, deterministic asymmetric combinations and uniform samples.
The retained original floating recurrence supplies the reference. All66 focused
VP8 controls pass with normal and disabled intrinsics on both OSes. All2,097
Release tests pass perOS:517 internal+686 public+894 live FFmpeg comparisons.
The same actual dependency-free package passes all three examples on both OSes.
Both actual statistical DLLs and fresh Linux production methods/flags match it;
fresh Windows DLL bytes match the pack. Three fixed repeats of sixteen complete
public decode/disposal operations allocate exactly the same bytes through the
old/new actual packages.

The earlier [scoped-work trial](WARM-VP8-LOCAL-WORK-REJECTED.md) is flat at0.2%
apparent gain and discarded without a longer repeat. Its original sources,
actual timed DLL, raw receipt and meaningful scalar/work-admission controls are
retained. Candidate-only production and tests are restored before this integer
trial. No CPU profile or JIT diagnostic is taken during either trial.

This round reuses one original513x259 RGBA8 raster, its frozen native quality75
VP8/ALPH input and the unchanged native worker. No image/encoder-quality search
occurs; unmatched encode is not timed. Managed pixels retain their old hash,
including hidden RGB. Alpha is exact and RGBmax1/RMSE0.132879 remains within the
declared max4/RMSE1.5 bounds. Native pixels match the pinned CLI exactly. Source
integrity, dimensions/orientation and independent owned outputs remain checked.
Reader/arithmetic state, per-row atomic work admission/cancellation, allocation,
matrix/sample conversion, output writes and memory release retain their code.

Both subjects perform complete resident-input decode and output acquisition/
release. Native time includes codec reset, centered bilinear chroma expansion,
limited BT.601 conversion through RGBA16, no-dither RGBA8 reduction, exact alpha
and scratch/output allocation/release. Contexts and packet/frame wrappers are
reused; padded rows are exposed directly. Startup, I/O, IPC, hashing, validation
and encoding are excluded. Sequential Release measurement uses CPU0, one codec/
conversion thread, normal SIMD, disabled tiering and non-concurrent workstation
GC on the dedicated EPYC9V45 VM, .NET10.0.8 / SDK10.0.204.

One short integer screen and one twelve-sample repeat are retained. Statistical
prequiet passes and postquiet fails under unchanged thresholds. Original quiet
observations and cooldowns remain archived; independent quiet confirmation and
required/material warm coverage stay open. No universal <=2x claim is made.
"""
(root/'WARM-VP8-CHROMA-INTEGER.md').write_text(text,encoding='utf-8')
