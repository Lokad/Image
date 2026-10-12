"""Render the rejected correction batch and the separate CPU diagnosis."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent
e=json.loads((folder/'warm-jpegcorrection-rejection-evidence.json').read_text())
assert not e['productionChanged'] and e['rejectedCandidate'] and not e['uniformWarmParityComplete']
a,b=e['observations'];p=e['cpuDiagnosis'];assert p['profilingOnly']
rows='\n'.join(f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.2f} | {o['worstBracketRatio']:.5f}x | {o['conservativeRatioUpper']:.5f}x | {'Pass' if all(g['passed'] for g in o['quietness'].values()) else 'FAIL'} |" for o in (a,b))
def fraction(name):return next(m['fraction'] for m in p['methods'] if '::'+name+'(' in m['method'])
text=f'''# Warm progressive JPEG: correction batch rejected

The EOB-tail correction batch supplies no useful improvement: screened managed
time increases {e['candidateManagedIncrease']*100:.2f}% and remains
{b['worstBracketRatio']:.2f}x FFmpeg. Both quiet guards pass. The candidate is
discarded without a longer confirmation; production is restored exactly.
Progressive decode remains the largest measured gap. Uniform 2x parity is open.

| Implementation/run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

Both subjects decode the same resident six-scan, 75,112-byte grayscale JPEG,
derived losslessly from the existing original 513x259 raster. Quantization and
all coefficients remain exact to the baseline. Owned pixel hashes, normal/scalar
CLI bounds, exact alpha, source preservation and independent ownership pass.
Native context reuse, per-operation decode/reset/conversion/allocation/release,
single CPU/thread, normal SIMD and runtime settings remain unchanged. No image,
quality, geometry or scan-strategy search is added. Startup and preparation are
outside the measured operation.

The rejected routine groups correction bits only in terminal EOB tails. It
transactionally admits complete input and exact work, retaining scalar marker,
truncation, budget and cancellation behavior on refusal. Only existing ReadBlock
changes (707 to 677 IL bytes); two methods are added. All other existing bodies
and implementation flags remain equivalent. The candidate passes 29 reader and
96 public JPEG controls per intrinsic mode on both OSes, plus 2,339 Windows
Release tests and the actual package's three examples. The Linux full candidate
suite is unnecessary after the flat screen and is not claimed. After rollback,
both OSes pass all 2,306 Release tests; all production methods/flags and fixture
pixels match the retained accepted actual package.

A separate CPU sampling diagnosis uses the restored implementation and the same
input, 30 warmup decodes, CPU0 and 20 seconds at 999 Hz. It records
{p['samples']:,} samples with zero reported lost samples. Self-sample shares are
{fraction('ReadBlock')*100:.2f}% in ReadBlock,
{fraction('ReadBits')*100:.2f}% in ReadBits,
{fraction('TryReadHuffman')*100:.2f}% in short Huffman extraction, and
{fraction('TryWork')*100:.2f}% in work admission. Inverse reconstruction accounts
for only {fraction('TryInverseSeparable')*100:.2f}%; rendering accounts for
{fraction('Render')*100:.2f}%. These shares guide work selection and establish no
FFmpeg ratio or independent parity certification.

The first diagnostic recording cannot resolve its incomplete JIT dump and is
retained without drawing conclusions. The second uses complete perf maps;
original parsing maps actual sampled instruction addresses to JIT code ranges.
See Microsoft's [runtime profiling settings](https://learn.microsoft.com/en-us/dotnet/core/runtime-config/debugging-profiling).
The original driver, raw recordings/maps/samples, compiled inputs and restored
test/package proofs are frozen in an ignored immutable archive. No upstream
implementation or reference corpus enters tracked files.

The next substantial hypothesis addresses progressive scan data flow: assess
contiguous spectral coefficient traversal, with natural-order mapping paid once
at reconstruction. This targets repeated zigzag indexing and scattered accesses
across scans. It is an untested hypothesis, not a performance claim. Select one
primitive after checking exact work, memory admission, cancellation, malformed
stream precedence and pixels; screen the same pair. Defer transform/render
micro-tuning and further EOB-only variations.

The [numeric evidence](warm-jpegcorrection-rejection-evidence.json) retains all
managed/native samples, uncertainty, guards, source/package/actual-child hashes
and archives. The [earlier accounting rejection](WARM-JPEGPROGRESSIVE-ACCOUNTING-REJECTION.md)
remains unchanged, including its failed post-run guard. Regenerate this report
with `python benchmarks/render-warm-jpegcorrection-rejection.py`; use `--check`
to verify. See the [reference recipe](../tools/WarmReference/README.md).
'''
path=folder/'WARM-JPEG-CORRECTION-REJECTION.md'
if '--check' in sys.argv:
 assert path.read_text(encoding='utf-8')==text,'Correction rejection report differs.'
 print('Correction rejection report matches numeric evidence.')
else:path.write_text(text,encoding='utf-8',newline='\n')
