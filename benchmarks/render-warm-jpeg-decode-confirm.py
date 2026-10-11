"""Render the current single-input JPEG decode confirmation."""
import json,statistics
from pathlib import Path
root=Path(__file__).resolve().parent
v=json.loads((root/'warm-jpeg-decode-confirm-evidence.json').read_text(encoding='utf-8'))
r=v['observation'];native=[statistics.mean(s['NanosecondsPerOperation'] for s in r[k])/1000 for k in ('nativeBefore','nativeAfter')]
text=f"""# Warm JPEG native-input decode confirmation

The current twelve-sample check reaches {r['worstBracketRatio']:.4f}x FFmpeg,
conservative upper {r['conservativeRatioUpper']:.4f}x. No production code changes
or performance gain are attributed to this confirmation. Independent quiet
confirmation and other required/material cases remain open.

The [numeric evidence](warm-jpeg-decode-confirm-evidence.json) retains actual
samples, uncertainty and source/output/tool identities. Regenerate with
`python benchmarks/render-warm-jpeg-decode-confirm.py`.

| Managed mean, µs | Native before / after, µs | Worst bracket ratio | Conservative upper |
| ---: | ---: | ---: | ---: |
| {r['managedStatistics']['Mean']/1000:.2f} | {native[0]:.2f} / {native[1]:.2f} | {r['worstBracketRatio']:.4f}x | {r['conservativeRatioUpper']:.4f}x |

This reuses the frozen 133,041-byte native JPEG, the existing original 513x259
RGBA8 raster and the unchanged native worker. The JPEG has logical 4:4:4 with
common component factors `{r['sampling']}`. Both subjects decode identical
resident bytes and acquire/release independently owned RGBA8. Native timing
includes state reset, all coefficient/pixel decoding and full-range BT.601
conversion; contexts and packet/frame wrappers are reused. Padded output rows
need no extra packing copy. Startup, IPC, I/O, generation, hashing and validation
are excluded. No extra image or encoder-quality search occurs.

Prior managed/native output hashes remain exact. Alpha is exact; maximum RGB
decoder difference {r['decoderMaximum']} / RMSE {r['decoderRmse']:.6f} remains within
the maximum 4 / RMSE 1.5 contract. Native output matches the pinned CLI. The actual
saved statistical DLL matches all production methods/flags in the previously
verified actual VP8L-inlining package. The same production already passed all
2,113 tests, and that package passed three usage examples on both OSes;
unchanged production needs no repeated
full suite or package examples here.

Sequential Release measurement uses CPU0, one codec/conversion thread, normal
SIMD, disabled tiering and non-concurrent workstation GC on the dedicated VM.
Prequiet passes and postquiet fails under unchanged thresholds; the original
observations and later cooldowns are preserved. This check closes the earlier
short-sample uncertainty numerically, while independent quiet confirmation and
broader material coverage remain open.
"""
(root/'WARM-JPEG-DECODE-CONFIRM.md').write_text(text,encoding='utf-8')
