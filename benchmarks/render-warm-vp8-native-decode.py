"""Render the retained one-input VP8 decode screen from original numeric evidence."""
import json,statistics
from pathlib import Path
root=Path(__file__).resolve().parent
v=json.loads((root/'warm-vp8-native-decode-evidence.json').read_text())
assert v['project']=='Lokad.Image' and v['caseId']=='Native8' and v['actualTimedProductionIlAndFlagsMatchPackage']
before,after=[statistics.mean(s['NanosecondsPerOperation'] for s in v[k])/1000 for k in ['nativeBefore','nativeAfter']]
reader=v['profile']['exclusiveLeafPercent']['Lokad.Image!Lokad.Image.Internal.Vp8BooleanReader.Read']
text=f"""# Warm native-input VP8 decode

The newly covered native VP8/ALPH decode case screens at {v['worstBracketRatio']:.4f}x
FFmpeg, the largest current measured gap. Production code is unchanged. This
establishes coverage and ranking; it is not a codec speedup or uniform parity claim.

The [numeric evidence](warm-vp8-native-decode-evidence.json) retains raw managed
and native samples, uncertainty, identities, quietness, ownership controls and
immutable source/evidence archive hashes. Regenerate this report with
`python benchmarks/render-warm-vp8-native-decode.py`.

| Case | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
| Native8 | {v['managedStatistics']['Mean']/1000:.2f} | {before:.2f} / {after:.2f} | {v['worstBracketRatio']:.4f}x | {v['conservativeRatioUpper']:.4f}x |

One existing original 513x259 RGBA8 raster and its frozen native quality-75
VP8/ALPH input are reused. There is no new image, regeneration, encode-quality
search or broad matrix. Identical-input decode does not need a matched encoder
pair. The unmatched VP8 encode catalog remains unchanged and is not timed.

Both subjects perform complete resident-input decode and independent owned
RGBA8 acquisition/release. Native timing includes state reset, software webp
decode, centered bilinear chroma expansion, limited BT.601 conversion through
RGBA16, explicit no-dither RGBA8 reduction, exact alpha merge and scratch/output
allocation/release. Native contexts and packet/frame wrappers are reused;
padded output rows are exposed without a packing copy. Every call decodes and
converts every pixel. Startup, I/O, IPC, encoding, hashing and validation are
excluded. One CPU, one codec thread and one thread per conversion context,
thread types zero and normal SIMD remain. Release uses .NET 10.0.8 / SDK
10.0.204 on the dedicated EPYC 9V45 VM, CPU 0, disabled tiering and non-concurrent
workstation GC.

Native RGBA matches the pinned CLI exactly. Managed RGBA retains its frozen
hash; every alpha byte is exact. RGB maximum difference is
{v['correctness']['decoderMaximum']}, RMSE {v['correctness']['decoderRmse']:.6f}, including zero-alpha hidden
RGB, within the specified maximum 4/RMSE 1.5 bounds. Distinct live owners,
mutation isolation, source integrity, geometry and orientation pass. Seventeen
negative native controls cover affinity, arguments, protocol, geometry and RIFF
framing. Linux BenchmarkDotNet smoke passes before the one statistical pair.

All {v['unchangedProductionAndTestFiles']} production/test file bytes remain equal to the previously validated
checkpoint. Its 2,063 tests and same actual package/all three examples already
passed both OSes. The saved actual statistical and diagnostic libraries match
that actual package in every production method body and implementation flag.
Benchmark-only work does not repeat the full test/package cycle.

One separate CPU sampling profile attributes {reader:.2f}% of managed workload
time exclusively to `Vp8BooleanReader.Read`. It supplies the next primitive to
inspect: arithmetic bit-reader renormalization. Method-name matching excludes
parameter types; workload stacks include warmup/calibration. This is attribution,
not parity timing. Raw trace, source and actual diagnostic library are archived.

Statistical and diagnostic prequiet checks pass; postquiet checks fail under
unchanged thresholds. Original cooldown failures remain in the archives.
The short screen has substantial uncertainty and needs independent quiet
confirmation. Other codec strategies, sizes, depths and editing paths remain
open; no universal <=2x result is claimed.
"""
(root/'WARM-VP8-NATIVE-DECODE.md').write_text(text, encoding='utf-8')
