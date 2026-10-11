import json, sys
from pathlib import Path
r = Path(__file__).resolve().parent
v = json.loads((r/'warm-resize-geometry-evidence.json').read_text(encoding='utf-8'))
b, short, confirm = v['baseline'], *v['candidate']
assert v['project'] == 'Lokad.Image' and not v['uniformWarmParityComplete']
assert v['verification']['actualTimedProductionIlAndFlagsMatchPackage']
rows = '\n'.join(f"| {label} | {item['managedStatistics']['Mean']/1000:.2f} | {item['worstBracketRatio']:.5f}x | {item['conservativeRatioUpper']:.5f}x |"
 for label,item in [('Retained baseline', b), ('Short screen', short), ('Confirmation repeat', confirm)])
content = f'''# Warm resize: prepare horizontal geometry once

Complete RGBA8 bilinear resize improves {v['screenedMeanGainPercent']:.2f}% in the
confirmation repeat on one original 513x259-to-256x129 pair. It remains
{confirm['worstBracketRatio']:.2f}x FFmpeg; the uniform 2x target is open.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: |
{rows}

The change prepares short-support horizontal weights and clamped sample offsets
once per public Resize call, reusing them across output rows. Previously it
computed the same horizontal geometry and coefficients for every output row.
The prepared path retains all taps, including zero-weight and repeated edge
positions, with the same binary64 weight multiplication and accumulation order.
Both sample depths, bilinear/bicubic kernels, tiny-alpha exact rounding and owned
output behavior remain. There is no new SIMD, fused arithmetic or separable pass.

Three temporary arrays use admitted workspace with conservative array overhead.
Selection leaves 4096 bytes for exact tiny-alpha arithmetic; wide horizontal
support or a tight memory limit retains the previous direct path. On this pair,
the geometry admits {v['verification']['admittedGeometryBytes']} bytes and adds
{v['verification']['expectedAddedBytesPerOperation']} allocated bytes per call.
Three fixed batches of 16 complete Resize/Dispose calls verify that exact delta.
Preparation and allocation are included in managed measurements. Geometry is
not retained across calls and neither subject caches output pixels.

The native public-FFmpeg-API graph includes packed/planar conversion, widened
triangle filtering, encoded-space premultiplication/unpremultiplication and
independent output pixel acquisition/release. Graph/source/wrappers/geometry
contexts are reused. Native samples before and after bracket the managed run.
One CPU/filter thread, thread type zero, normal SIMD, tiering disabled and
workstation/nonconcurrent GC apply. Startup, IPC, I/O and validation are excluded.

Managed pixels retain the exact rational-reference digest. Native alpha is exact;
RGB retains maximum error 1 / RMSE 0.25, with observed RMSE 0.003177. The selected
output has no zero-alpha pixels; broader native alpha-edge coverage remains open.
No performance image or pair was added.

171 baseline controls per OS and 175 candidate controls per OS with intrinsics
enabled/disabled pass, including four frozen binary64 digests and new tight-memory
geometry fallback controls. All 2122 Release tests per OS pass. The same actual
dependency-free Release package passes all three package examples on both OSes;
both statistical child DLLs and fresh production IL/flags match it. Only Resize
changes among 535 production methods. Fourteen native negative controls pass.

Pre-run quiet guards pass; post-run failures are retained. The repeat is useful
optimization evidence, with independent quiet confirmation still open. A later
cooldown does not certify the statistical intervals. Other depths, directions,
sizes and bicubic warm coverage remain open; no uniform parity claim follows.

The [numeric evidence](warm-resize-geometry-evidence.json) records retained/raw
statistics, boundaries, source and actual DLL identities. The immutable final
archive SHA-256 is `{v['immutableEvidence']['rawArchiveSha256']}`. The prior
actual-package JIT inspection is diagnostic only; it is not a candidate profile.

Regenerate this report with `python benchmarks/render-warm-resize-geometry.py`;
use `--check` to verify that the report matches the numeric evidence.
'''
p = r/'WARM-RESIZE-GEOMETRY.md'
if '--check' in sys.argv:
    assert p.read_text(encoding='utf-8') == content
else:
    p.write_text(content, encoding='utf-8', newline='\n')
