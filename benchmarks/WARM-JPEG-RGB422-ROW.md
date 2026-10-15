# Horizontal subsampled RGB JPEG rows

Direct-RGB JPEG422 decoding improves from **4.081× FFmpeg** to independently
confirmed **1.511× / upper1.520×**, reducing managed time **62.9%**.
Every frozen managed pixel remains exact. Final allocation is **1,978,655 B/op**.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Original generic interpolation | 4,460.92 | 1,093.08 | 4.081 / 6.246 | 1,978,612 | Above target |
| Quarter-weight rows, three actuals | 1,651.87 | 1,094.28 | 1.510 / 1.540 | 1,978,655 | Screen |
| Independent twelve actuals | 1,653.61 | 1,094.64 | 1.511 / 1.520 | 1,978,655 | Confirmed |
| Full-resolution RGB regression | 2,964.52 | 1,566.33 | 1.893 / 2.998 | 2,664,373 | Three-actual mean passes; prior confirmed |
| YCbCr422 regression | 1,902.81 | 1,346.77 | 1.413 / 1.497 | 1,978,651 | Three-actual mean passes; prior confirmed |

The original 513×259 byte raster's native JPEG422 packet is reused. Original
marker-only authoring adds Adobe transform0, assigns R/G/B component identifiers
in SOF/SOS and removes conflicting JFIF metadata. Quantization/Huffman tables
and every entropy byte are unchanged. Sampling factors22/12/12 retain their
shared vertical factor and horizontal2:1 ratio. The quantized planes are
interpreted as RGB; there is no new original image or settings/shape grid.

One six-second profile records5,909 samples: **37.2% interpolation** and
**21.0% rounding**. The accepted primitive specializes horizontal2:1 RGB rows
with full vertical resolution. Centered coordinates are exact quarters; integer
quarter weights preserve every original binary64 interpolation/away-from-zero
rounding result. AVX2 processes eight output pixels per batch, with scalar edges
and tails. Equivalent sampling factors work unchanged. No extra workspace or
pixel cache is introduced. Admission, original row work/cancellation and other
RGB/YCbCr layouts retain their contracts.

Both subjects decode the identical resident JPEG to independent owned RGBA,
including complete decode, conversion/packing, output allocation and release.
Native software mjpeg state resets on every operation; input/context/frame
wrappers are reused. Its decoded GBRP packing is followed by a **timed centered
RGB422 phase correction**: even positions preserve original subsampled G/B
samples, from which clamped quarter weights reconstruct the required grid.
The correction runs after owned RGBA packing with original samples held in
locals before overwriting them. It uses no additional workspace and leaves red
and opaque alpha unchanged. All of this work is included in every native sample.

Uncorrected native normal/scalar output differs by max29/RMSE4.619 and remains
an untimed negative control. An attempted CLI geq graph also fails and is retained
as a diagnostic. Independent CLI output with an immutable rational correction
matches the native worker exactly. The **unchanged** decoder gate remains RGB
max4/RMSE1.5 and exact opaque alpha; this pair actually differs by max1/
RMSE0.126. No managed tolerance is added. Header/packet identities, every frozen
managed sample, source preservation and two simultaneously live independent
owners pass.

Sequential Release runs use CPU0, one codec/conversion thread, normal SIMD,
disabled tiering and workstation nonconcurrent GC. Both versioned quiet guards
pass on every measurement. Confirmation retains all twelve actuals; eleven
are selected. The upper includes selected/all-actual intervals against the
faster native bracket's lower bound. Full-resolution RGB and YCbCr422 regression
means pass **before** full qualification; their short intervals do not replace
their earlier independent confirmations.

Allocation figures include harness overhead and batch amortization; the new
primitive adds no allocation. Native allocations are **unmeasured**. Startup,
I/O, IPC, fixture authoring, hashing and validation are excluded. No decoded
output is cached.

Twelve row/edge/tail/guard controls pass with normal/scalar execution on each OS.
Full **3,469 Release tests per OS** pass:1,185 internal,1,386 public API and
**898 live FFmpeg comparisons**. The same actual Release NuGet package's consumer
and all three examples pass on Windows/Linux. One production candidate build,
one native-worker build and two successful harness builds suffice; an initial
compile refusal and ready-sampling contract refusal are retained and never
measured. Cached generated jobs and immutable binaries are reused within each
experiment; no repeated full qualification, method audit or profile is needed.

[Numeric evidence](warm-jpeg-rgb422-row-evidence.json) retains all actuals,
allocations, guards, source/build/native/fixture identities, refusals, profile,
regressions and qualification. Stop this confirmed pair; full required warm
coverage remains open.
