import json,statistics
from pathlib import Path
root=Path(__file__).resolve().parent
d=json.loads((root/'warm-png-framing-costs-evidence.json').read_text());profile=d['profile']
lines=['# Warm PNG framing costs','',
 'Before Work inlining, the retained PNG16 profile identified strict framing as the',
 'largest sampled cost. Separate literal output-admission and wider prefix-refill',
 'trials gave no useful complete-operation gain and were discarded. Production',
 'codec behavior was unchanged during those trials. Their historical 2.36/2.33x',
 'baseline is below; the accepted [work-admission round](WARM-WORK-ADMISSION.md)',
 'records the subsequent improvement.','',
 'The [numeric evidence](warm-png-framing-costs-evidence.json) retains original',
 'timing samples, source/input/output identities, the profile, assembly controls',
 'and raw archive hashes. Regenerate with',
 '`python benchmarks/render-warm-png-framing-costs.py`.','',
 '| PNG16 stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
 '| --- | ---: | ---: | ---: | ---: |']
for label,key in [('Retained filter None','retainedBaseline'),('Rejected literal admission','rejectedLiteralAdmission'),('Rejected wider prefix refill','rejectedWidePrefixRefill')]:
 for run in d['observations'][key]:
  for v in run['cases']:
   if v['caseId']!='Decode16':continue
   brackets=[statistics.mean(s['NanosecondsPerOperation'] for s in v[k])/1000 for k in ('nativeBefore','nativeAfter')]
   lines.append(f"| {label} | {v['managedStatistics']['Mean']/1000:.2f} | {brackets[0]:.2f} / {brackets[1]:.2f} | {v['worstBracketRatio']:.2f}x | {v['conservativeRatioUpper']:.2f}x |")
lines += ['',
 'Both trials and the separate CPU profile use the same original 513x259 RGBA16',
 'raster and identical resident PNG bytes, including zero-alpha hidden RGB.',
 'No additional benchmark image or quality enumeration. Complete decode and',
 'independent output acquisition/release are timed, including required native',
 'endian conversion. Native contexts/scratch are reused. Startup, IPC, I/O,',
 'generation, hashes and validation remain outside the equivalent boundary.','',
 'Runs are sequential in Release on CPU 0 of the dedicated AMD EPYC 9V45 VM,',
 'one codec thread, normal SIMD, .NET 10.0.8 / SDK 10.0.204, disabled tiering',
 'and non-concurrent workstation GC. The separate profile includes generated',
 'warmup/calibration workload loops and is not native parity timing.','',
 '| Method before Work inlining | Inclusive sampled percent |',
 '| --- | ---: |']
for method in ('ZlibFraming.Validate','PngCodec.DecodeRaster','Inflater.Inflate','Crc32.Compute','PngCodec.CopyRgbaRow','Adler32.Append'):
 lines.append(f"| `{method}` | {profile['inclusivePrimitivePercent'].get(method,0):.2f}% |")
lines += ['',
 'Match exact method names before parameter lists. Inclusive costs overlap and',
 'cannot be added. The last visible frame may include inlined work or native',
 'transitions without their own symbols. Framing is about 56.5% inclusive;',
 'row copy and Adler are small in this case. No pure managed-only inflation',
 'cost or literal-loop-only percentage is inferred.','',
 'The rejected branch kept Huffman decoding, bit consumption, cancellation/work,',
 'history/window checks and errors. All 78 original framing controls pass on',
 'both platforms and all 2,026 candidate Windows tests pass. No repeat, affected',
 'timing screen or full Linux candidate suite was run after the unhelpful result.','',
 'Two new original fixed-then-stored streams pin prefix lookahead across LEN/NLEN',
 'and payload, checksum cursor with optional unused trailing bytes, exact 9,522',
 'work units, atomic three-unit stored-payload work and scratch release on both',
 'success and failure. They agree with BCL inflation and add no benchmark image.',
 'The wider prefix-only trial safely read eight bytes, admitting eight to an',
 'empty cache or seven masked bytes when one to eight bits remained. Stored',
 '16-bit reads kept four-byte refill and their exact SkipBytes cursor. All 80',
 'framing controls pass on both platforms and all 2,028 candidate Windows tests',
 'pass. The 2.31x screen, conservative upper 2.44x, supplies no useful gain over',
 'the retained results; allocation matches the latest retained screen. No repeat, affected timing',
 'screen or full Linux candidate suite. Two pre-run quiet checks failed before',
 'any samples were collected. Keep every failure and original sample.','',
 'The restored code passes all 2,028 tests on Windows and Linux: 448 internal,',
 '686 public and 894 live FFmpeg comparisons. All production method bodies',
 'match the same existing actual dependency-free Release package, previously',
 'verified with all three usage examples on both platforms. The built profile',
 'project also matches that package. Source inventory and original child build',
 'log accompany the trace; BDN cleans its child directory, so the assembly',
 'comparison covers the surviving profile project. Package SHA256:',
 f"`{d['validation']['existingActualPackageSha256']}`.",'',
 'Both screens and the profile pass pre-run quietness and fail post-run checks.',
 'All original samples, failed quietness and later cooldowns remain in ignored',
 'evidence. One screen supplies no parity certification. PNG remains the largest',
 'retained measured gap; other measured, unknown and unmatched directions stay',
 'open. One retained-code JIT inspection on the same Decode16 input shows',
 'separate OperationContext.Work calls in the consumed-prefix path. Its optimized',
 'Validate listing is 7,441 bytes; this inspection supplies no parity timing.',
 'Both quiet checks pass. Next, try inlining only Work with its original method',
 'body and admission/cancellation semantics. The subsequent work-admission',
 'report records that trial; avoid repeating refill and literal-admission trials.','']
(root/'WARM-PNG-FRAMING-COSTS.md').write_text('\n'.join(lines),encoding='utf-8',newline='\n')
