# Warm matched JPEG encode: quiet confirmation

The retained complete public JPEG encoder independently confirms at
1.956x FFmpeg, conservative upper
1.969x. Both unchanged quiet guards pass. This fixed
pair clears 2x; full required warm coverage remains open. Production is unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Quiet screen | 7324.07 | 1.93959x | 2.24191x | Pass |
| Guard-failed confirmation | 7324.54 | 1.94054x | 1.96785x | FAIL |
| Independent quiet confirmation | 7390.28 | 1.95559x | 1.96949x | Pass |

One existing original 513x259 RGBA8 raster and its original matched preparation
are reused. Both subjects flatten alpha once onto RGB 0x204060 and encode
opaque 8-bit 4:4:4 JPEG. Managed quality 69 and native
qscale 5 retain source PSNR gap 0.08244 dB,
within the unchanged 0.25 dB band. Bitstreams, decoder bounds, alpha, source and
independent outputs are freshly checked before/after; preparation bytes remain
identical. No quality, effort, shape or source-raster enumeration.

Managed fixed Huffman output is 324,002 bytes; native optimal
Huffman output is 133,041 bytes. This is quality matching, not
equal compression efficiency. Complete operations include single-round alpha
flattening, full-range BT.601 conversion, encoding, independent output acquisition
and release. Native codec/conversion/scratch/frame/packet wrappers are reused;
every pixel is reprocessed. Fresh managed MemoryStream creation/growth and
disposal are timed; no redundant ToArray. Startup, I/O, IPC, preparation,
quality enumeration and validation are excluded.

The first 12-actual confirmation fails post-load
0.305176 above the unchanged 0.25 limit.
It stays failed and is frozen before one bounded fresh attempt. The fresh process
reuses the same four prepared child identities and waits for starting load<=0.01;
code, input, settings, targets and guards remain unchanged. Its 12 actual/12
filtered managed measurements and 12 native samples per bracket pass both guards.
The upper takes the larger filtered/all-actual bound:
1.96948x / 1.96949x.
All original actuals and failed guards remain retained.

Managed allocation is 1,051,857 B/op from raw GC bytes / operation count,
rounded up and including harness overhead. Native allocation totals are unmeasured.
CPU0, one codec/conversion thread, thread type zero, normal SIMD, disabled tiering
and nonconcurrent workstation GC are fixed on the dedicated reference VM.

Both actual timing children and the fresh Windows build match all 565 production
method bodies/flags in the accepted actual package. The preceding 2,385 tests per
OS and same-package/all-three-example checks remain applicable. There is no
production or benchmark implementation change. The
[numeric evidence](warm-jpegencode-quiet-evidence.json) retains all measurements,
quality/source/input/output/child/package identities, guards and immutable archive
digests. Regenerate with `python benchmarks/render-warm-jpegencode-quiet.py`;
`--check` verifies this report. See the [current summary](../docs/benchmark.md)
and [reference recipe](../tools/WarmReference/README.md). Stop tuning this passing
pair and prioritize the largest remaining gap.
