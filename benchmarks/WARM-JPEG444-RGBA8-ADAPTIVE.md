# Warm RGBA8 JPEG444 adaptive Huffman revalidation

The shared adaptive encoder independently confirms **1.772× FFmpeg**, conservative
upper **1.818×**, averaging **6,705.88 µs** with **559,384 managed bytes allocated
per operation**. Both quietness guards pass; all twelve actual iterations are
retained. Stop tuning this fixed pair; uniform warm parity remains open.

| Stage | Lokad µs | Ratio / upper | Managed allocated B/op |
| --- | ---: | ---: | ---: |
| Earlier fixed-table confirmation | 7,390.28 | 1.956 / 1.969 | 1,051,857 |
| Adaptive three-actual screen | 6,719.32 | 1.780 / 2.191 | 559,384 |
| Adaptive independent confirmation | 6,705.88 | 1.772 / 1.818 | 559,384 |

The original 513×259 RGBA8 raster, managed quality **69**, native qscale 5 and
background RGB 0x204060 are unchanged. Benchmark preparation now checks this
previously matched anchor once for the adaptive policy, avoiding the original
seven-quality sweep. The original receipt remains intact, and incorrect native
qscale is rejected before creating a receipt. The RGB source-error gap stays
**0.082440 dB**, within the unchanged 0.25 dB gate. No source image was added.

Blending, color conversion, sampling, transform, histogram/table construction,
entropy coding and independent output acquisition/release are timed. Independent
entropy recoding reproduces the adaptive packet byte for byte. Every coefficient
and amplitude is unchanged; normal and scalar FFmpeg decoding preserve the
previous decoded pixels exactly. Alpha, geometry, decoder agreement, unchanged
source and simultaneously held independent output checks pass. Private event
storage is reused, with contents recomputed for every operation.

Managed packets shrink from **324,002 to 158,812 bytes**; native packets remain
133,041 bytes. This is a source-quality timing comparison; compression efficiency
parity is not established. Allocations decrease **46.8%** from the earlier
confirmation. Only benchmark preparation changed; production matches all 629
bodies and implementation flags in the tested Release package. Its 2,767 tests
and three actual-package usage examples per OS are reused.

An initial confirmation launch failed its pre-run guard: CPU0 I/O wait was 1.8%,
above the 1% limit. No statistical child or actual iteration started. That guard
and all four prepared child files were frozen. After pending writes were flushed,
the same unchanged child completed the independent confirmation above. The
failed guard remains rejected and unscored.

Sequential Release runs use one pinned CPU, one native codec/conversion thread,
normal SIMD, tiering off and workstation nonconcurrent GC. Startup, I/O, IPC,
fixture preparation and checks are excluded; native allocations are unmeasured.
Raw evidence, source, actual package and all four child files were frozen before
subsequent preparation. The [numeric evidence](warm-jpeg4448-adaptive-evidence.json)
retains the screen, failed guard and confirmation. The
[earlier fixed-table report](WARM-JPEG-ENCODE-QUIET.md) remains intact.

Next, cover PNG RGB16 transparency decoding. JPEG420 RGBA8's upper-bound gap and
the remaining material contract remain open.
