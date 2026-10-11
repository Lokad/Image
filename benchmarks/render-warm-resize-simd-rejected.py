"""Render the rejected four-channel double accumulation screen."""
import json, statistics
from pathlib import Path
root = Path(__file__).resolve().parent
v = json.loads((root / 'warm-resize-simd-rejected-evidence.json').read_text(encoding='utf-8'))
rows = []
for label, r in [('Retained twelve-sample baseline', v['baseline']), ('SIMD short screen', v['candidate'])]:
    native = [statistics.mean(s['NanosecondsPerOperation'] for s in r[k]) / 1000 for k in ('nativeBefore', 'nativeAfter')]
    rows.append(f"| {label} | {r['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {r['worstBracketRatio']:.2f}x | {r['conservativeRatioUpper']:.2f}x |")
text = f'''# Rejected warm resize channel accumulation

The four-channel SIMD accumulation screen gives no useful complete-operation
gain: its mean is {v['screenedMeanChangePercent']:.1f}% higher than the retained
scalar baseline. The production trial is discarded without a longer repeat.
The retained matched bilinear case remains {v['baseline']['worstBracketRatio']:.2f}x
FFmpeg; the 2x target and independent quiet confirmation remain open.

The [numeric evidence](warm-resize-simd-rejected-evidence.json) preserves raw
samples, uncertainty, actual child/source/tool/pixel identities, frozen scalar
correctness hashes and immutable archive hashes. Regenerate with
`python benchmarks/render-warm-resize-simd-rejected.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
''' + '\n'.join(rows) + '''

Only the trial's Resize accumulation changes: AVX converts the four independent
integer lanes to binary64, then performs separate packed multiplication and
addition, with the original scalar fallback. RGB retains weight times alpha,
then sample; alpha uses the fourth lane times one. Tap order, kernels, exact
tiny-alpha fallback, rounding, work/cancellation/memory admission and owned
outputs remain. The short screen does not establish the cause of the unfavorable
point result; no CPU profile, JIT diagnostic or longer run is taken.

One existing original 513x259 RGBA8 raster produces one independently owned
256x129 bilinear output. Managed rational-reference pixels, native alpha,
source integrity, independent owners and native CLI/deterministic pixels pass.
Native RGB retains maximum 1 / RMSE 0.25, with the existing one-level float
rounding-boundary difference and observed RMSE 0.003177. The same native filter
worker, geometry, alpha rules and complete operation boundaries remain. No new
performance image, quality search, baseline substitution or comparison-bound
change occurs. Fourteen native negative controls and benchmark smoke pass.

All 167 existing focused controls pass before edits on both OSes; 171 candidate
controls pass with normal and disabled intrinsics on both OSes. Four meaningful
regressions remain tracked: original asymmetric-v1 9x7 rasters, five existing
target shapes, transparent/opaque inputs and both 8/16-bit filtered kernels.
Their concatenated output digests are captured before edits through the retained
actual package's scalar code. They preserve exact binary64 output at quantization
boundaries, tightening the independent rational reference's one-level bound.
These are correctness inputs, not additional benchmark images or upstream data.

The actual candidate child DLL and raw/source evidence are archived before exact,
guarded production restoration on both OSes. All production sources then match
the retained revision; fresh production IL/flags match its previous actual
dependency-free Release package in all 535 methods. The four new controls pass
normally and with disabled intrinsics after restoration on both OSes. The prior
2,114 Release tests per OS and three examples through that same actual package
on both OSes remain applicable; no full suite or package repetition is needed
for this test/report-only checkpoint. Thirty existing original JPEG package
correctness outputs also remain exact.

Sequential Release measurement uses CPU0, one filter thread, normal SIMD,
disabled tiering and non-concurrent workstation GC on the dedicated EPYC 9V45
VM, .NET 10.0.8 / SDK 10.0.204. Both subjects perform complete resident-input
filtering and independent owned output acquisition/release; native graph/source/
wrappers/geometry contexts are reused, with all packed/planar conversion and
encoded-space alpha processing included. Startup, IPC, I/O, generation, hashes
and validation are excluded. Prequiet passes and immediate postquiet fails under
unchanged thresholds. The short observation is a screen, not a regression
certificate or independent quiet confirmation.

Next is one horizontal kernel-weight reuse primitive inside the complete resize
operation, preserving original arithmetic/tap order and all admission checks.
Broader warm coverage and uniform parity remain open.
'''
(root / 'WARM-RESIZE-SIMD-REJECTED.md').write_text(text, encoding='utf-8')
