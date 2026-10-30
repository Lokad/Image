# Current extended-JPEG decode costs

YCCK420 is superseded by [row conversion](WARM-YCCK-ROW-TILES.md), independently
confirmed at 0.653× / upper 0.660. The quarter costs below are superseded by [strip reconstruction](WARM-JPEG-QUARTER-STRIPS.md).

Three existing required controls now have valid current costs on the unchanged
qualified production build. At this checkpoint, YCCK420 4.835× and 4:1:0 4.703× were the largest
demonstrated gaps. All three have exact independent managed/native pixels.

| Complete owned RGBA8 decode,513×259 | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Encoded input B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Ycck420 | 3,420.79 | 707.48 | 4.835 / 4.864 | 895,458 | 6,704 |
| Decode411 | 1,256.78 | 447.72 | 2.807 / 2.823 | 751,280 | 3,904 |
| Decode410 | 2,388.42 | 507.81 | 4.703 / 4.718 | 720,792 | 3,399 |

Inputs remain the original authored DC-only packets, projections and block means;
no image/settings grid is added. Each complete operation includes component
decode, normalization, independent output acquisition and release. One pinned
CPU, warm Release execution, all12actuals and both idle-v2/native brackets pass.
Native allocations are unmeasured. These are current costs, without a paired
causal comparison against older production/export policies or broader JPEG claims.

Earlier helpers retained obsolete expected pixels. YCCK now uses an independent
block-mean oracle with integral half-grid byte rounding, Q16 black products and
rational BT.601 Q13color. Its pinned native/scalar CLI bytes are unchanged.
For4:1:1/4:1:0, the declared public reference converts actual coded dimensions;
the old helper instead remapped a padded516-column centered grid. The native
adapters now convert513×259directly, without recovered chroma rasters or row
compaction. Independent reference rasters derive from original block means,
normalized Q14horizontal/Q12vertical weights and the declared color precision.
4:1:0 preserves the decoder's byte horizontal pre-expansion boundary.

Both previous and current production DLLs already produced identical normal/scalar
pixels. The corrected independent reference and pinned SIMD/scalar CLI match
every output byte on **Windows and Linux**. No production pixel changed, source
sample was retuned or quality gate relaxed. Historical YCCK native-only bounds
are recorded but unused: all three controls now require exact native RGBA.

The unchanged [JDS1build](WARM-JPEG-DIRECT-BASELINE-PLANES.md) retains5,178checks/OS,
334internal/339public focused normal/scalar checks and the same actual Release
package/governed consumer/three examples. Zero production builds or repeated
full/package runs. Before freeze, two Windowsharness compiles, one Linuxharness
compile and two native compiler builds correct only tooling; no selector build.
One shell launch failed before gcc and was repaired without rebuilding the verified
harness. The earlier failed YCCK setup had zero actuals. No scored job is repeated.

Next, address the per-pixel YCCK sampling/black/color chain as one substantial
row primitive; existing inverse-color SIMD currently receives single pixels.
Preserve all exactness, scalar, memory, ownership and cancellation requirements.
The full **268-path ≤1×** objective remains open.

[Evidence](warm-extended-jpeg-current-costs-evidence.json),
[reference helper](Lokad.Image.Benchmarks/WarmJpegFixedReference.cs).
Mirrored/member-verified extended-jpeg-current-costs-qualified.tar.xz, 211members,
SHA256 eeb790798c5fb9d130b35bafe1709977e61cf12c4ae0bcbdced5329b82cf27e8; original checkpoint/build dependencies are pinned.
