"""Render isolated traversal counts and the direct-boundary rejection."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent;e=json.loads((folder/'warm-jpeginsertion-rejection-evidence.json').read_text());a,b=e['observations'];d=e['traversalDiagnosis']
assert not e['productionChanged'] and e['rejectedCandidate'] and not e['uniformWarmParityComplete']
rows='\n'.join(f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'FAIL'} |" for o in (a,b))
text=f'''# Progressive JPEG: direct insertion boundary rejected

The direct-boundary trial supplies no managed-time improvement: screened mean
increases {e['candidateManagedIncrease']*100:.2f}% and remains above 2x FFmpeg.
Its post-run load guard fails and stays failed. The trial is discarded without
confirmation; production is restored exactly. Progressive 4:4:4 remains the
largest measured gap at {a['worstBracketRatio']:.2f}x. Uniform parity is open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

An isolated diagnostic copy of 55 original owned source files instruments only
group attempts, admitted prefix/nonzero lengths and scalar corrections. One
decode of the same fixed input retains the exact owned pixel hash. Every real
production/owned source byte is verified unchanged before and after. The ignored
copy and compiled driver remain outside production and benchmark build inputs;
the instrumented execution supplies no timing or parity claim.

| Traversal | Attempts | Successful empty prefixes | Successful groups with zero corrections |
| --- | ---: | ---: | ---: |
| Insertion | {d['attempts'][0]:,} | {d['prefixes'][0][0]:,} | {d['corrections'][0][0]:,} |
| ZRL | {d['attempts'][1]:,} | {d['prefixes'][1][0]:,} | {d['corrections'][1][0]:,} |
| EOB | {d['attempts'][2]:,} | {d['prefixes'][2][0]:,} | {d['corrections'][2][0]:,} |

These counts expose frequent rediscovery of zero/nonzero positions. The trial
handles an immediate insertion zero directly in ReadBlock, avoiding the vector
helper for 54,930 empty prefixes. It preserves coefficient insertion, entropy
order, logical work, memory admission and cancellation/error behavior. Only that
body changes (738 to 760 IL bytes), with all 560 methods, other bodies and flags
equivalent after resolving metadata. Both OSes pass 96 focused public JPEG checks
in normal/scalar modes. The candidate passes 2,347 Windows Release tests and its
actual package's three examples. A full Linux candidate suite and independent
confirmation are unnecessary after the flat/failed-guard screen and are not
claimed. After rollback, both OSes pass 2,347 Release tests; all methods/flags and
fixture pixels match the retained accepted actual package.

The same single resident 132,961-byte, ten-scan full-resolution input and original
513x259 raster remain. Coefficients, quantization, sampling factors, pixels,
normal/scalar CLI bounds, alpha, source and independent owners retain their prior
proofs. No image, geometry, quality or scan search is added. Full public decode/
allocation/disposal compares with software native decode/reset/full-range
conversion/independent output release. CPU0, one codec/conversion thread, thread
type zero, normal SIMD, tiering off and nonconcurrent workstation GC remain fixed.
Native 50ms brackets and three managed 200ms iterations exclude startup, I/O,
IPC, preparation and validation. All raw/source/actual-child inputs are frozen
before later edits. Bounds include BDN results and every actual measurement.

The next substantial hypothesis computes one nonzero-position bitmask per
refinement block, reuses it across symbols and updates it only on insertion.
This may remove repeated vector discovery and zero-run traversal rather than
adding another boundary/width tweak. It is untested; choose one primitive only
after checking entropy order, work, memory, cancellation and malformed-input
precedence. The same pair remains the target.

The [numeric evidence](warm-jpeginsertion-rejection-evidence.json) retains the
complete histograms, raw measurements, uncertainty, failed guard, source/package/
actual-child identities and immutable archives. No upstream implementation or
reference corpus is tracked. Regenerate with
`python benchmarks/render-warm-jpeginsertion-rejection.py`; use `--check` to verify.
See the [preceding wider-group rejection](WARM-JPEG-REFINE16-REJECTION.md) and
[reference recipe](../tools/WarmReference/README.md).
'''
p=folder/'WARM-JPEG-INSERTION-REJECTION.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==text,'Insertion rejection report differs.'
 print('Insertion rejection report matches numeric evidence.')
else:p.write_text(text,encoding='utf-8',newline='\n')
