# Warm PNG RGBA8 encode coverage

The existing complete public encoder independently confirms at
0.421x FFmpeg, conservative upper
0.430x. Both unchanged quiet guards pass. This fixed
pair clears 2x; full required warm coverage remains open. Production is unchanged.

| Run | Managed mean (microseconds) | Worst native bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| Quiet coverage screen | 7598.46 | 0.44181x | 0.78044x | Pass |
| Independent quiet confirmation | 7241.99 | 0.42057x | 0.42967x | Pass |

One existing original 513x259 RGBA8 raster is reused, including alpha and
zero-alpha hidden RGB. Both subjects encode noninterlaced RGBA PNG, level 6,
filter None. Owned decoded samples are exact; separate normal/scalar FFmpeg CLI
encodes preserve every source sample. IDAT payloads are
218,682 managed versus 210,283 native
bytes, within the frozen 5% effort-matching limit. Source and simultaneous held
outputs remain independent and deterministic. No new raster or profile sweep.

Complete public encoding includes fresh managed MemoryStream creation/growth,
output acquisition and disposal, with no redundant ToArray in timing. Native
codec/frame/packet wrappers are reused, while every operation encodes and
acquires/releases an independently owned packet. RGBA8 needs no endian conversion.
Startup, I/O, IPC, generation, hashing and validation are excluded. CPU0,
one codec/conversion thread, thread type zero, normal SIMD, disabled tiering and
nonconcurrent workstation GC remain fixed on the dedicated reference VM.

The confirmation retains all 12 actual managed measurements and 12 native
samples before/after. BenchmarkDotNet filters to 10
results; the upper uses the larger filtered/all-actual view:
0.42799x / 0.42967x.
Managed allocation is 527,629 B/op from raw GC bytes / operation count,
rounded up and including harness overhead. FFmpeg allocation totals are unmeasured.

Both actual timing children and the fresh Windows build match all 565 production
method bodies/flags in the accepted actual Release package. The preceding
2,385-test results per OS and same-package/all-three-example checks remain
applicable; no production method or option behavior changes. The only harness
change extends the existing normal/scalar CLI encode check to RGBA8.

The [numeric evidence](warm-png8encode-evidence.json) retains every actual/native
measurement, source/input/output/actual-child/package identity, guard and immutable
archive digest. Regenerate with `python benchmarks/render-warm-png8encode.py`;
`--check` verifies this report. See the [current summary](../docs/benchmark.md)
and [measurement recipe](../tools/WarmReference/README.md). Stop tuning this
passing pair; prioritize the largest remaining gap.
