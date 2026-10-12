# Warm VP8 encode: alpha-weighted chroma

The fixed lossy VP8 encode independently confirms at
0.787x FFmpeg, conservative upper 0.805x,
with both quiet guards passing. Stop tuning this point; full required warm
coverage remains open. Compression efficiency parity is not established.

| Run | Lokad.Image ms | FFmpeg ms | Ratio / upper | Managed allocated B/op | Actual/filtered | Quiet guards |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Fixed-pair screen | 42.807 | 54.151 | 0.79051 / 0.93311 | 2,615,472 | 3/3 | Pass |
| Independent quiet confirmation | 42.880 | 54.485 | 0.78701 / 0.80479 | 2,615,472 | 12/10 | Pass |

One existing original 513x259 RGBA8 raster is encoded, with its hidden RGB and
alpha unchanged. Native quality 75 and managed quality 100, both effort 4, are
fixed before measurement. Their source PSNRs are 16.701663 and 16.698017 dB:
gap 0.003646 dB clears the unchanged 0.25 dB band. Alpha is exact. Registered
normal/scalar native decoding agrees exactly; managed/native decoded RGB agrees
within maximum 1 / RMSE 0.136842. No additional raster, quality enumeration,
profile sweep or relaxed acceptance bound is used. The fixed-anchor metadata
gate recomputes the match and rejects fifteen invalid controls before native work.

Lokad.Image emits **471,436 bytes** versus FFmpeg's **98,590 bytes**. The managed
encoder uses fixed entropy models and raw ALPH; the native encoder uses adaptive
coding. This is a quality-matched complete-operation timing comparison; encoded
size efficiency remains a limitation. Native allocations are unmeasured.

One production primitive changes chroma preparation: each 2x2 cell averages
encoded RGB using normalized alpha weights before the existing limited-range
BT.601 matrix and rounding. Hidden colors therefore do not dilute visible
chroma. Fully transparent cells keep the original average; opaque and uniform
opacity keep their original result. Luma, exact normalized alpha, 16-to-8
rounding, odd/padded edge extension, admission, work charges and cancellation
remain unchanged. No transfer table, new workspace or upstream code is added.
The earlier linear-sRGB candidate remains a rejected historical experiment.

Twelve original analytical controls cover both depths, visible/hidden chroma,
unequal and uniform opacity, zero alpha and odd/padded edges. The existing 42
managed corpus recipes/options are replayed: 15 alpha bitstreams and their
independent native decoded goldens change; all opaque bitstreams and native
fixtures stay identical. The first QA replay failure is retained before that
selective refresh. Regenerate with the registered original oracle through
`tools/Fixtures ... --refresh-managed-vp8`; no new fixture inputs are introduced.

Timing includes the complete public encode, new managed MemoryStream creation/
growth, independent bytes and disposal. Native RGBA-to-BGRA repacking, internal
RGB-to-YUV conversion and complete encode run every operation; codec/scratch/
frame/packet wrappers are reused. No output is cached. Startup, I/O, IPC,
generation, validation and quality matching are excluded. CPU0, one codec thread,
thread type zero, normal SIMD, tiering off and nonconcurrent workstation GC
remain fixed. Managed iterations target 200ms; native brackets target 50ms.
All twelve actuals, including two filtered outliers, are retained; upper takes
the larger filtered/all-actual conservative bound. `exclusive-vm-idle-v2`
checks live overlap plus five-second aggregate/CPU0 busy <=5%, I/O wait <=1%,
steal <=1%. Both screen and confirmation pass both guards without retries.

Measured source is `4b066c81c5cc3d7b94fe2e9479e40d7a52b2a39a2f90d2454c15012e055a8427`
at recorded revision `c1c30988f51f9caeaa2b485bc4d2b4426df0daa4`. Raw receipts, four
actual child files, source and package are archived before mutation. Fixture
refresh uses a separately bound subsequent source with identical production
method bodies and flags. Both OSes pass 2,410 Release tests each (660 internal,
854 public API, 896 live comparisons), 19 scalar chroma/admission controls each
and all three examples through the same actual dependency-free Release package.
All 571 fresh/timed/package production methods and flags match.

[Numeric evidence](warm-vp8-alphaweighted-evidence.json) retains all actuals,
brackets, quality/size figures, guards, input/output/source/child/package
identities, prototype, selective refresh and immutable archive digests. See the
[current three-table summary](../docs/benchmark.md) and
[measurement recipe](../tools/WarmReference/README.md).
Regenerate with `python benchmarks/render-warm-vp8-alphaweighted.py`;
`--check` verifies this report. Unmatched and uncovered scope stays open.
