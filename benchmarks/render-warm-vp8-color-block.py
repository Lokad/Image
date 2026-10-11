"""Render the retained single-input VP8 color-conversion evidence."""
import json,statistics
from pathlib import Path
root=Path(__file__).resolve().parent
v=json.loads((root/'warm-vp8-color-block-evidence.json').read_text(encoding='utf-8'))
rows=[]
for label,r in [('Integer-chroma baseline',v['baseline']),('Eight-pixel screen',v['candidate'][0]),('Eight-pixel 12-sample repeat',v['candidate'][1])]:
    native=[statistics.mean(s['NanosecondsPerOperation'] for s in r[k])/1000 for k in ('nativeBefore','nativeAfter')]
    rows.append(f"| {label} | {r['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {r['worstBracketRatio']:.4f}x | {r['conservativeRatioUpper']:.4f}x |")
text=f"""# Warm VP8 eight-pixel color conversion

Safe SIMD color conversion reduces the retained complete VP8 decode mean by
{v['screenedMeanGainPercent']:.1f}%, from {v['baseline']['worstBracketRatio']:.2f}x to
{v['candidate'][1]['worstBracketRatio']:.2f}x FFmpeg. Independent quiet confirmation
and coverage of other required warm cases remain open.

The [numeric evidence](warm-vp8-color-block-evidence.json) preserves raw samples,
uncertainty, source/output/tool identities, allocations and immutable archive
hashes. Regenerate with `python benchmarks/render-warm-vp8-color-block.py`.

| Stage | Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| --- | ---: | ---: | ---: | ---: |
"""+'\n'.join(rows)+"""

Only `Vp8Color` changes. Eight adjacent interior pixels use safe, bounded span
reads and stores with AVX-512 double matrix operations and AVX2/SSSE3 packing.
Integer centered-chroma interpolation retains exact quarter phases and rounding
before the matrix. Separate double operations preserve scalar expression order;
no fused multiply-add is used. First pixels, edges, tails and unsupported hardware
use the existing scalar conversion. Alpha and RGB beneath transparent pixels
remain exact. Per-row atomic work/cancellation, output allocation/disposal and
plane-memory release retain their admission boundaries. No unsafe code, native
production binding, dependency or copied upstream implementation is introduced.

Before editing, thirteen full-render controls pass on both OSes against the
original floating geometry and scalar matrix. They cover thin, odd and even images,
vector block boundaries/tails, padded strides, extreme samples, alpha/hidden RGB,
atomic row work failure, exact working-memory admission and cancellation before
row access. A correctness-only prototype checks all 16,777,216 byte Y/U/V triples
and 6,144 centered-phase samples; it supplies no primitive timing claim. All 82
focused VP8 controls pass with normal and disabled intrinsics on both OSes.
All 2,113 Release tests pass per OS: 533 internal, 686 public API and 894 live FFmpeg
comparisons. The same actual dependency-free Release package passes all three
usage examples on both OSes. Actual short/long statistical DLLs and fresh Linux
production methods/flags match the package; fresh Windows DLL bytes match it.
Three fixed repeats of sixteen complete public decodes/disposals allocate exactly
the same bytes through the old/new actual packages.

This uses one original 513x259 RGBA8 raster, frozen native quality-75
VP8/ALPH input and unchanged native worker. Managed pixels retain the prior hash;
alpha is exact and maximum RGB error 1 / RMSE 0.132879 remains. Native pixels match the pinned CLI.
Source integrity and independently owned outputs remain checked. No extra images,
encoder search, CPU profile or new JIT inspection occurs in this conversion round.
The preceding [reader-state trial](WARM-VP8-READER-STATE-REJECTED.md) is flat and
discarded without a longer repeat; its offset regression controls remain.

Both subjects perform complete resident-input decode and owned output acquisition/
release. Native timing includes reset, centered chroma expansion, limited BT.601
conversion through RGBA16, no-dither RGBA8 reduction, alpha and scratch/output
allocation/release. Contexts and packet/frame wrappers are reused; padded output
rows are exposed directly. Startup, I/O, IPC, hashing, validation and encoding are
excluded. Sequential Release measurement uses CPU0, one codec/conversion thread,
normal SIMD, disabled tiering and non-concurrent workstation GC on the dedicated
EPYC 9V45 VM, .NET 10.0.8 / SDK 10.0.204.

One short screen and one twelve-sample repeat are retained. Prequiet checks pass;
postquiet observations and later cooldowns remain archived under unchanged
thresholds. These screens support the improvement; independent quiet passing
confirmation is still required. This single profile does not establish uniform
warm parity or a performance guarantee on hardware using the scalar fallback.
"""
(root/'WARM-VP8-COLOR-BLOCK.md').write_text(text,encoding='utf-8')
