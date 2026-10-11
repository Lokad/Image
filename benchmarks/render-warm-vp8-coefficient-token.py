"""Render the retained single-pair VP8 coefficient-token evidence."""
import json,statistics
from pathlib import Path
root=Path(__file__).resolve().parent
v=json.loads((root/'warm-vp8-coefficient-token-evidence.json').read_text(encoding='utf-8'))
rows=[]
for name,r in [('Branchless 12-sample baseline',v['baseline']),('Fixed token screen',v['candidate'][0]),('Fixed token 12-sample repeat',v['candidate'][1])]:
    a,b=[statistics.mean(s['NanosecondsPerOperation'] for s in r[k])/1000 for k in ('nativeBefore','nativeAfter')]
    rows.append(f"| {name} | {r['managedStatistics']['Mean']/1000:.2f} | {a:.2f} / {b:.2f} | {r['worstBracketRatio']:.4f}x | {r['conservativeRatioUpper']:.4f}x |")
text=f"""# Warm VP8 coefficient-token decisions

Replacing generic coefficient-token tree traversal with fixed decisions reduces
the retained complete VP8 decode mean {v['screenedMeanGainPercent']:.1f}%, from
{v['baseline']['worstBracketRatio']:.2f}x to {v['candidate'][1]['worstBracketRatio']:.2f}x FFmpeg.
This case remains above 2x. Material coverage and quiet confirmation remain open.

The [numeric evidence](warm-vp8-coefficient-token-evidence.json) retains raw samples,
uncertainty, source/input/output identities, allocations and archive hashes.
Regenerate with `python benchmarks/render-warm-vp8-coefficient-token.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
"""+'\n'.join(rows)+f"""

One CPU profile of the accepted branchless baseline attributes 44.71% of workload
time exclusively to Coefficients, including inlined arithmetic reads. Workload
stacks include warmup/calibration; parameter types do not affect method matching.
Both launches retain the frozen pixels, ownership, input and single-thread contract;
the actual diagnostic library matches the validated baseline package in every
production method body/flag. This is attribution, not parity timing.

The new `Vp8BooleanReader.CoefficientToken` independently expresses the fixed tree
in [RFC 6386 section 13.2](https://www.rfc-editor.org/rfc/rfc6386.html#section-13.2).
It returns EOB, zero, one, small magnitudes and category tokens directly, retaining
the original probability accesses and arithmetic read order. After a zero it skips
only the EOB decision. `Vp8Decoder.Coefficients` calls it in place of generic tree
traversal. Arithmetic interval selection/normalization, sign/category bits,
dequantization, coefficient writes and the general tree reader retain their
algorithms. No table, allocation, cache, dependency or unsafe code is introduced.

Five new controls pass against the original scalar recurrence before production
editing on both OSes. They cover every token, skipped EOB, partial work budgets,
interval boundaries, probability/span/partition truncation, cancellation and
exhausted work. They compare token, exception type/message, all five arithmetic
state fields and remaining work. All 58 focused VP8 controls pass with normal and
disabled intrinsics on both OSes. All 2,089 Release tests pass per OS: 509 internal,
686 public and 894 live FFmpeg comparisons. The same actual dependency-free package
passes all three usage examples on both OSes; both actual statistical DLLs and the
fresh Linux assembly match its production methods/flags. Fresh Windows DLL bytes
match the pack. Three fixed repeats of sixteen complete public decode/disposal
operations allocate exactly the same bytes through the old/new actual packages.

This round reuses one original 513x259 RGBA8 raster, its frozen native quality-75
VP8/ALPH input and the unchanged native worker. No image or quality search occurs;
unmatched encode is not timed. Managed pixels retain their previous hash. Alpha is
exact, RGB max1/RMSE0.132879 remains within max4/RMSE1.5, native output equals the
pinned CLI, and hidden RGB, source integrity and independent owned outputs remain
checked before and after timing.

Both subjects acquire and release independent outputs from identical resident
input. Native time includes codec reset, centered bilinear chroma expansion,
limited BT.601 conversion through RGBA16, no-dither RGBA8 reduction, exact alpha
and scratch/output allocation/release. Contexts and packet/frame wrappers are
reused; padded rows are exposed directly. Startup, I/O, IPC, hashing, validation
and encoding are excluded. Sequential Release measurement uses CPU0, one codec/
conversion thread, normal SIMD, disabled tiering and non-concurrent workstation GC
on the dedicated EPYC 9V45 VM, .NET 10.0.8 / SDK 10.0.204.

One short screen and one actual twelve-sample repeat are retained. An initial
long-run launch fails its quiet guard before starting a statistical child; its
observation and cooldowns remain archived. Statistical prequiet passes and postquiet
fails under unchanged thresholds. Independent quiet confirmation and required
warm coverage remain open; this is not a universal <=2x result.
"""
(root/'WARM-VP8-COEFFICIENT-TOKEN.md').write_text(text,encoding='utf-8')
