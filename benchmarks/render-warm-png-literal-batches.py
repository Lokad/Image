import json, statistics
from pathlib import Path

root = Path(__file__).resolve().parent
d = json.loads((root / 'warm-png-literal-batches-evidence.json').read_text())
lines = ['# Warm PNG literal-batch trials', '',
    'All batching variants are discarded. Partial literal runs improve PNG16,',
    'but regress PNG8; restricting the attempt to an already decoded literal',
    'reduces both effects. Retained production remains unchanged: PNG16 is',
    '2.22x / 2.22x and PNG8 is 2.11x FFmpeg. Uniform warm parity remains open.', '',
    'The [numeric evidence](warm-png-literal-batches-evidence.json) preserves',
    'original samples, confidence bounds, allocations, identities, quietness',
    'failures and immutable raw/source archive hashes. Regenerate this report',
    'with `python benchmarks/render-warm-png-literal-batches.py`.', '',
    '| Variant | Case | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |',
    '| --- | --- | ---: | ---: | ---: | ---: |']
for key, label in [('retainedBaseline', 'Retained Work'),
                   ('rejectedFullEight', 'Rejected eight-only'),
                   ('rejectedUnconditionalPartial', 'Rejected partial'),
                   ('rejectedAfterLiteral', 'Rejected after-literal')]:
    for run in d['observations'][key]:
        for v in run['cases']:
            native = [statistics.mean(s['NanosecondsPerOperation'] for s in v[k]) / 1000
                      for k in ('nativeBefore', 'nativeAfter')]
            upper = v['conservativeRatioUpper']
            bound = f'{upper:.2f}x' if upper is not None else 'Unbounded'
            lines.append(f"| {label} | {v['caseId']} | {v['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {v['worstBracketRatio']:.2f}x | {bound} |")
lines += ['',
    'Eight-only batching has no useful gain and receives no repeat. Partial',
    'batching repeats PNG16 at 2.05x / 2.05x, about 7% lower managed time, but',
    'PNG8 repeats at 2.22x / 2.22x, about 5% higher managed time. Starting the',
    'batch after an ordinary literal screens at 2.17x PNG8 and 2.16x PNG16:',
    'the PNG8 penalty persists, so that variant receives no repeat. Every',
    'candidate keeps the same input and exact owned output identities.', '',
    'The independent RFC 1951 framing walk probes local reader state and commits',
    'only valid literal runs with fully admitted exact bit work, sufficient output',
    'and a noncancelable token. Short inputs, unassigned or long codes, end codes,',
    'matches, tight budgets and cancellable operations keep the original path.',
    'These safeguards pass correctness checks; they do not justify retaining a',
    'performance tradeoff. All trial production code is restored.', '',
    'Two preceding inlining diagnostics receive no statistical timing. Inlining',
    'the full canonical decoder removes its calls, but leaves seven Consume',
    'calls. Also inlining Consume removes those calls but leaves other reader',
    'calls and stack-backed state. Optimized Validate code grows from the retained',
    '9,380 bytes to 10,759 / 11,485 bytes. Both diagnostics pass pre/post quietness',
    'and are discarded. They provide no new CPU-cost attribution.', '',
    'Only the two existing original 513x259 RGBA8/16 rasters are timed. Complete',
    'public decode and independent output acquisition/release remain inside the',
    'boundary, including native conversion and reset. Context/scratch reuse is',
    'declared; startup, IPC, I/O, generation, hashing and validation are excluded.',
    'Sequential Release jobs use CPU 0, one codec/conversion thread, normal SIMD,',
    '.NET 10.0.8 / SDK 10.0.204, disabled tiering and non-concurrent workstation',
    'GC on the dedicated AMD EPYC 9V45 VM. Actual statistical prechecks pass;',
    'postchecks fail and remain recorded. One failed after-literal precheck',
    'collects no samples. No thresholds are relaxed and no <=2x claim is made.', '',
    'Four new reserved-code cases retain exact work and failure precedence after',
    'valid literal runs, atomic failing-prefix admission and scratch release,',
    'with both default and cancellable tokens. They pass against the original',
    'production before the trials. All 2,032 tests pass on restored production',
    'on Windows and Linux: 452 internal, 686 public and 894 live FFmpeg.', '',
    'Restored production bytes match the accepted Work checkpoint. All production',
    'IL and implementation flags match the same previously verified actual',
    'dependency-free Release package, which passed all three usage examples on',
    'both platforms. Its original controls also match the retained statistical',
    'library; rejected candidate libraries remain separate. Package SHA256:',
    f"`{d['validation']['actualPackageSha256']}`.", '']
(root / 'WARM-PNG-LITERAL-BATCHES.md').write_text('\n'.join(lines), encoding='utf-8', newline='\n')
