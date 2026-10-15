# Direct-RGB JPEG row packing

Full-resolution direct-RGB JPEG decoding improves from **2.824× FFmpeg** to an
independently confirmed **1.918× / upper 1.923×**, reducing managed time **35.3%**.
Every frozen managed pixel remains exact. Final allocation is **2,664,373 B/op**.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Original direct-RGB screen | 4,302.31 | 1,523.49 | 2.824 / 3.764 | 2,664,332 | Above target |
| Direct-plane packing, three actuals | 2,781.76 | 1,451.09 | 1.917 / 2.039 | 2,664,373 | Screen |
| Independent twelve actuals | 2,781.74 | 1,450.64 | 1.918 / 1.923 | 2,664,373 | Confirmed |
| Baseline YCbCr444 regression | 3,071.31 | 1,729.68 | 1.776 / 4.566 | 2,664,373 | Three-actual mean passes; prior confirmed |
| Progressive YCbCr444 regression | 6,447.09 | 3,345.92 | 1.927 / 1.993 | 2,682,364 | Three-actual mean passes; prior confirmed |

One original-raster native JPEG444 packet is reused. Original marker-only
authoring adds Adobe transform0, assigns R/G/B component identifiers in SOF/SOS
and removes conflicting JFIF metadata if present. Quantization, Huffman tables
and every entropy byte are unchanged. The same quantized component planes are
interpreted as RGB; this is a documented projection, not another original image
or quality/shape grid. Both decoders receive the same 133,057-byte packet.

A single six-second profile records 5,527 samples: rounding 17.8%, interpolation
9.9% and rendering 8.4%, alongside 37.8% in existing AC entropy traversal. The
accepted primitive removes redundant interpolation/rounding for full-resolution
RGB. IDCT has already rounded and clipped each component byte; equal sampling
factors make interpolation fractions zero. AVX2 widens eight bytes per plane,
shifts/interleaves channels and supplies opaque alpha; the scalar path is exact.
Subsampled RGB retains its original interpolation. Per-row logical work and
cancellation admission remain unchanged, with no new table or workspace.

Both subjects time complete public decoding, admission, entropy/IDCT work,
independent owned RGBA acquisition and release. Native software MJPEG decoding
resets state every operation, checks actual GBRP output and packs all planes to
owned RGBA on one conversion thread. Context/input packet/frame wrappers are
reused; decoded pixels are not cached. No YUV matrix is applied to direct RGB.
Startup, I/O, IPC, marker authoring and validation are excluded. Managed allocations
include harness overhead; native allocations are **unmeasured**.

Frozen managed pixels, geometry, source preservation and simultaneous independent
owners pass. Normal/scalar native pixels agree exactly; adapter output equals
the pinned CLI byte for byte. Native/managed RGB maximum1 and RMSE0.125629 stay
within the unchanged JPEG maximum4/RMSE1.5 gate; alpha is exact. The initial
native admission refused untagged RGB range metadata; the scoped GBRP admission
correction preserves exact output and is retained with the refusal. A supervisor
missing preparation metadata fails before launching any measurement.

Sequential Release runs use CPU0, one codec/conversion thread, normal SIMD,
disabled tiering and workstation nonconcurrent GC. Every versioned quiet guard
passes. The candidate screen, regressions and confirmation reuse one immutable
production DLL, harness and native worker. All twelve confirmation actuals are
retained and selected. The upper includes selected/all-actual intervals against
the faster native bracket's lower bound. Regression means pass before full
qualification; their short intervals do not replace earlier confirmations.

Seven independent row/unaligned-guard cases pass with normal/scalar execution on
both OSes. Full qualification passes **3,439 Release tests per OS**, including
**898 live comparisons**. The consumer and all three examples pass on Windows
and Linux through the **same actual Release package**. One harness build, two
necessary native adapter builds and one production candidate build suffice;
no whole-library method audit or repeated full/package qualification is performed.

[Numeric evidence](warm-jpeg-rgb-row-evidence.json) retains every actual, allocation,
guard, input/output/source/assembly/job/native identity, refusal, profile and actual
package proof. Stop this confirmed RGB pair; full required warm coverage remains open.
