"""Render existing-kernel RGBA8 transpose parity evidence."""
import json,sys
from pathlib import Path
folder=Path(__file__).resolve().parent;e=json.loads((folder/'warm-transpose8-parity-evidence.json').read_text());assert not e['productionChanged'] and not e['uniformWarmParityComplete'];a,b=e['observations'];assert all(o['conservativeRatioUpper']<2 for o in (a,b));assert e['independentQuietConfirmationThisPair'] and all(g['passed'] for o in (a,b) for g in o['quietness'].values())
rows='\n'.join(f"| {o['label']} | {o['managed']['statistics']['Mean']/1000:.3f} | {o['worstBracketRatio']:.6f}x | {o['conservativeRatioUpper']:.6f}x | Pass |" for o in (a,b))
text=f'''# Warm RGBA8 transpose: existing kernel parity

The existing complete-pixel tile kernel clears the fixed RGBA8 transpose case.
No production implementation changes. The independent twelve-actual repeat is
{b['worstBracketRatio']:.6f}x FFmpeg, with conservative upper {b['conservativeRatioUpper']:.6f}x.
Both unchanged quiet guards pass, certifying this fixed pair only. Uniform warm
parity and other material cases remain open. Passing cases receive no small tweaks.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
{rows}

One existing original RGBA8 513x259 raster produces 259x513. Together with the
retained RGBA16 input, exactly two original performance rasters are used. There
is one pair per trial, with no new image, shape, quality or profile sweep.
The managed public Transpose(mirrored:false) includes both-buffer admission,
complete UInt32 tile placement, independent output acquisition and disposal.
Existing 32x32 tiles and 1,024-pixel work/cancellation checks remain unchanged.
Released storage may be reused through the bounded 4 MiB idle slot; every pixel
is processed and outputs are never cached.

The original public-API FFmpeg worker uses transpose with
`dir=cclock_flip:passthrough=none`, allocating and processing independent pixels
inside every operation before release. Graph/source/frame wrappers are reused.
Padded rows require no redundant packing copy. Every RGBA sample, alpha, hidden
RGB, source and two held outputs matches the independent coordinate reference
and pinned normal/scalar CLI. Fourteen negative input/protocol/affinity controls
pass. Timed production methods and flags match the previously accepted actual
Release package. Production has not changed, so prior package/test evidence
remains applicable; this is additional warm coverage.

Pinned FFmpeg's x86 dispatch selects SSE2 routines for one- and two-byte plane
elements; packed four-byte RGBA pixels retain the C routine. Source inspection
provided insight only, with no code, tables or corpus copied into tracked files.

Sequential Release runs use CPU0, one filter thread, thread type zero, normal
SIMD, tiering off and non-concurrent workstation GC. Native before/after batches
use the declared 50 ms target; managed actual iterations retain 200 ms. Generated
children are prepared for validation outside the scored windows with one native
operation before/after and null ratios. Fresh unchanged five-second guards follow
cooldown scheduling. Startup, IPC, I/O, generation, hashing and validation are
excluded. The repeat retains twelve actual and twelve filtered managed results.

The [numeric evidence](warm-transpose8-parity-evidence.json) retains all managed,
GC/threading and native measurements, guards, identities and immutable archives.
Conservative bounds divide managed confidence upper by the smaller native lower,
using the managed critical value. Regenerate with
`python benchmarks/render-warm-transpose8-parity.py`; add `--check` to verify.
See the [reference recipe](../tools/WarmReference/README.md).
'''
p=folder/'WARM-TRANSPOSE8-PARITY.md'
if '--check' in sys.argv:
 assert p.read_text(encoding='utf-8')==text,'RGBA8 transpose report differs.'
 print('RGBA8 transpose report matches numeric evidence.')
else:p.write_text(text,encoding='utf-8',newline='\n')
