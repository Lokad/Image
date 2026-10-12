# Development benchmarks

The [RGBA16 bilinear precision audit](BILINEAR16-PRECISION.md) reproduces the
existing authored raster on both OSes. Managed pixels match the rational
reference; native float alpha differs by one sample at 62 rounding boundaries.
Exact-alpha acceptance refuses timing, leaving this case unmatched. No RGBA16
performance figure or new performance image is introduced.

The [packed alpha/coefficient kernel](WARM-RESIZE-QUAD-ALPHA.md) reduces complete
warm bilinear resize a further 8.9%, from 2.08x to 1.91x FFmpeg (conservative
upper 1.93x). Packed preparation replaces repeated scalar alpha conversions
while retaining exact pixels and allocation totals on the same single pair.
This case clears the numerical target; independent quiet confirmation and
other material warm coverage remain open.

The [grouped four-tap sample kernel](WARM-RESIZE-QUAD-SAMPLES.md) reduces complete
warm bilinear resize a further 20.5%, from 2.64x to 2.08x FFmpeg (conservative
upper 2.12x). Two bounded grouped reads replace eight separate sample reads per
paired-row tap group. Exact pixels and allocation totals remain unchanged.
The same single raster/pair is retained. Independent quiet confirmation,
the remaining resize gap and uniform warm parity remain open.

The [reformulated paired packed-sample kernel](WARM-RESIZE-PAIRED-WEIGHTS.md)
reduces complete warm bilinear resize a further 8.1%, from 2.89x to 2.64x FFmpeg.
It shares horizontal geometry across independent output rows and constructs
wide weighted-alpha coefficients directly in registers. Exact pixels and
allocation totals remain unchanged. The remaining resize gap stays first;
independent quiet confirmation and uniform warm parity remain open.

The [paired output-row trial](WARM-RESIZE-PAIRED-ROWS-REJECTED.md) is slower and
discarded after one short screen. Production is restored exactly. Its retained
JIT reveals per-tap wide coefficient stack materialization; a different packed
sample formulation is the next bulk-filtering hypothesis.

The [packed output trial](WARM-RESIZE-PACKED-OUTPUT-REJECTED.md) shows no useful
gain in one short screen and is discarded without a longer repeat. Production
bytes are restored exactly; six literal half-rounding regression cases remain.
The next resize hypothesis is wider filtering across independent output pixels.

The [packed RGBA8 accumulation kernel](WARM-RESIZE-PACKED-KERNEL.md) reduces
complete warm bilinear resize a further 34.1%, from 4.35x to 2.89x FFmpeg.
One bounded sample read and packed widening replace scalar channel conversions,
while retaining exact binary64 ordering, alpha behavior and allocation totals.
The remaining resize gap takes priority over small tweaks. Independent quiet
confirmation and other material warm cases remain open.

The [compact prepared filtering kernel](WARM-RESIZE-PREPARED.md) reduces complete
warm bilinear resize a further 31.6%, from 6.34x to 4.35x FFmpeg. It removes
repeated row-access calls and runtime depth/fallback dispatch from short-support
filtering, retaining exact pixels, admission and allocations. Resize remains the
largest measured gap; independent quiet confirmation and broader coverage are open.

The [horizontal geometry preparation round](WARM-RESIZE-GEOMETRY.md) reduces
complete warm bilinear resize a further 7.8%, from 6.93x to 6.34x FFmpeg.
Preparation and admitted workspace allocation are included; tight memory limits
retain direct evaluation with identical pixels. The largest measured gap remains
resize. Further work prioritizes substantial filtering-loop improvements;
independent quiet confirmation and broader warm coverage remain open.

The [typed-row accessor inlining round](WARM-TYPED-ROW-INLINE.md) reduces complete
warm bilinear resize a further 2.3%, from 7.07x to 6.93x FFmpeg. All 535 method
bodies remain identical; only two inlining flags change. A fresh baseline profile
selected this primitive. The next round inspected the remaining Resize body.

The [vertical weight-reuse round](WARM-RESIZE-YWEIGHTS.md) reduces complete warm
bilinear resize a further 2.2%, from 7.25x to 7.07x FFmpeg. Exact pixels,
admission and allocations remain. It is still the largest measured gap;
that checkpoint selected a short profile of the current operation.

The [destination-row acquisition round](WARM-RESIZE-DESTINATION.md) reduces
complete warm bilinear resize a further 16.1%, from 8.65x to 7.25x FFmpeg.
Pixels, admission and allocations remain. It is still the largest measured gap;
that checkpoint selected vertical kernel-weight reuse.

The [horizontal weight-reuse round](WARM-RESIZE-XWEIGHTS.md) reduces complete warm
bilinear resize a further 5.7%, from 9.08x to 8.65x FFmpeg. Original pixels,
admission and allocations remain. It is still the largest measured gap;
that checkpoint selected destination-row acquisition.

The [four-channel resize SIMD trial](WARM-RESIZE-SIMD-REJECTED.md) shows no useful
gain in its short screen and is discarded without a longer repeat. Four exact
scalar-pixel regressions remain; production retains source-row span acquisition.

The [filtered source-row acquisition round](WARM-ROW-SPAN.md) reduces complete
warm bilinear resize by about 66.8%, from the [row inlining round's](WARM-ROW-INLINE.md)
27.46x to 9.08x FFmpeg on the same original raster. This remains the largest
measured gap. Managed rational-reference pixels, native alpha and allocations
remain exact; native RGB meets the existing filtered comparison bound. That
checkpoint selected horizontal kernel-weight reuse for the next trial.

The [current JPEG decode confirmation](WARM-JPEG-DECODE-CONFIRM.md) reaches
1.90x FFmpeg with a twelve-sample conservative upper below 2x. The first
[matched warm nearest resize screen](WARM-NEAREST-BASELINE.md) reaches 1.31x
on the same original raster. Those benchmark checkpoints leave production
unchanged. Other filtered resize cases and material paths remain uncovered;
independent quiet confirmation remains open.

The [VP8L lookup inlining round](WARM-VP8L-READ-INLINE.md) reduces complete warm
decode from about 2.00x to 1.74x FFmpeg on the same frozen literal/run input.
All production method bodies, exact RGBA and allocation counts remain.
Independent quiet confirmation and broader material coverage remain open.

The [eight-pixel VP8 color-conversion round](WARM-VP8-COLOR-BLOCK.md) reduces
complete warm decode to about 1.94x FFmpeg on the existing frozen input. Exact
pixels, admission and allocation counts remain. Independent quiet confirmation
and coverage of the other required cases remain open.

The [reader-state caching trial](WARM-VP8-READER-STATE-REJECTED.md) is flat and
discarded without a longer repeat. Three offset regression controls remain;
production retains the accepted integer chroma implementation.

The [integer VP8 chroma-expansion round](WARM-VP8-CHROMA-INTEGER.md) reduces
complete decode a further 5.3%, from 2.29x to 2.17x FFmpeg, with exact original
samples, pixels and allocations. This remains the largest measured gap.
The [scoped-work trial](WARM-VP8-LOCAL-WORK-REJECTED.md) is flat and discarded
without a longer repeat. Both reuse the same single native-input pair.

The [VP8 coefficient-token round](WARM-VP8-COEFFICIENT-TOKEN.md) reduces complete
decode a further 3.6%, from 2.38x to 2.29x FFmpeg, replacing generic traversal
with fixed format decisions. Pixels, arithmetic state, work and allocations
remain exact. This is still the largest measured gap; coverage and independent
quiet confirmation remain open.

The [VP8 branchless interval-selection round](WARM-VP8-BRANCHLESS.md) reduces
complete decode a further 2.2%, from 2.43x to 2.38x FFmpeg. Only the arithmetic
interval-selection body changes; pixels, work, failure state and allocations
remain exact. VP8 decode is still the largest measured gap and above 2x.

The [VP8 arithmetic reader inlining round](WARM-VP8-READ-INLINE.md) reduces
complete decode about 8%, from 2.64x to 2.43x FFmpeg, preserving all production
method bodies and pixels. The grouped-shift trial was flat and discarded.
That checkpoint left VP8 decode as the largest measured gap. Its profile selected
interval selection; required coverage and quiet confirmation remain open.

The [native-input VP8 decode screen](WARM-VP8-NATIVE-DECODE.md) adds one frozen
input pair at 2.64x FFmpeg, the largest measured gap at that checkpoint. Production was unchanged;
native pixels match the CLI exactly, alpha is exact and managed RGB is within
the specified bounds. One CPU profile selects arithmetic bit-reader
renormalization as the next primitive, followed by the inlining round above.

The [PNG bit-reader inlining round](WARM-PNG-READ-INLINE.md) improves complete
PNG16 decode about 3% and PNG8 about 9%, preserving all pixels and method bodies.
The means are now 1.95x / 1.81x FFmpeg; longer conservative uppers are below 2x.
Quiet independent confirmation and required-path coverage remain open. That
checkpoint selected the uncovered VP8 decode direction measured above.

The [certified JPEG forward/quantization round](WARM-JPEG-FORWARD-INTERLEAVED.md)
reduces complete encoding time about 3.7%, from 2.02x to 1.95x FFmpeg, retaining
all encoded bytes. Its 12-sample upper is below 2x; quietness and independent
passing confirmation remain open. That checkpoint left PNG16 decode as the largest
retained measured point gap near 2.0093x, before the reader round above.

The [JPEG paired alpha-blending trial](WARM-JPEG-PAIRED-BLEND.md) gives no useful
complete-operation gain and is discarded. A current encode CPU profile and one
mixed-lane control are retained. That checkpoint left JPEG encode as the largest
measured point gap near 2.0215x, before the certified transform round above.

The [PNG long-literal fallback round](WARM-PNG-LONG-LITERAL.md) skips canonical
depths already excluded by the root lookup. PNG8 decode improves about 1.3%;
PNG16 is effectively unchanged, both near 2.01x FFmpeg. A frozen VP8L ranking
recheck reaches 1.98x. That round left JPEG encode as the largest measured point
gap near 2.0215x; uniform warm parity and required-path coverage remain open.

The [JPEG local entropy-work round](WARM-JPEG-LOCAL-WORK.md) preserves every exact
charge within each baseline coefficient block. Native-input decode screens reach
1.92x / 1.90x FFmpeg; the owned-input affected screen reaches 1.77x. Actual package
allocation counts remain equal. Uniform warm parity remains open; VP8L was
subsequently rechecked in the PNG long-literal round.

The [polynomial CRC folding round](WARM-PNG-CRC-FOLD.md) uses safe carryless
multiplication and independently derived PNG polynomial constants. Complete
PNG16 decode screens reach 1.9955x / 2.0021x, PNG8 2.0370x / 2.0359x; fixed
allocation controls remain identical. Uniform <=2x parity remains open.
Native-input JPEG decode was the largest retained measured gap at that checkpoint.

The [PNG local-accounting round](WARM-PNG-LOCAL-WORK.md) retains every exact bit
charge while avoiding repeated shared tally writes. Complete PNG16 decode
screens reach 2.09x / 2.10x, PNG8 2.07x / 2.07x. Both improve, remain above 2x,
and pass identical allocation counts through the actual packages.

The [PNG literal-batch trials](WARM-PNG-LITERAL-BATCHES.md) discard all three
batching variants: PNG16 gains come with a PNG8 regression. Two inlining
diagnostics are also discarded without timing. Four failure-budget controls
are retained; production stays unchanged and PNG16 remains the largest gap.

The [work-admission round](WARM-WORK-ADMISSION.md) adds one inlining hint with
unchanged admission semantics. PNG16 screens reach 2.22x / 2.22x, PNG8 2.11x
and native-input JPEG decode 2.06x. PNG16 remains the largest measured gap.

The [PNG framing profile before Work inlining](WARM-PNG-FRAMING-COSTS.md)
identified strict framing as the largest PNG16 cost. Literal-admission and
wider-prefix-refill trials were discarded; their original evidence is retained.

The [shared forward-product round](WARM-JPEG-FORWARD-SHARED.md) retains exact
JPEG coefficients and bytes while reducing complete warm encode screens to
2.12x / 2.13x. PNG decode now has the largest measured gap; uniform parity is open.

The [encoder cost profile before sharing](WARM-JPEG-ENCODER-COSTS.md) records two
discarded writer trials and the profile that selected the forward transform.

The [signed-amplitude extension round](WARM-JPEG-SIGNED.md) replaces a conditional
JPEG amplitude offset with an exact sign mask. Native-input warm decode screens
reach 2.13x / 2.15x; the owned affected screen is 2.01x. Uniform parity remains open.

The [fused inverse-certificate magnitude round](WARM-JPEG-MAGNITUDE.md) removes
the separate coefficient reduction, retaining exact bounds and all decoded pixels.
Native-input decode screens reach roughly 2.6x; uniform warm parity remains open.

The initial [dedicated-VM results](RESULTS.md) include the complete representative
table, original numeric snapshot, reproduction commands and measured limitations.
The subsequent [horizontal-flip round](HORIZONTAL-FLIP.md) separates the warm
primitive improvement from the equivalent FFmpeg raw-file batch comparison.
The [warm single-CPU flip round](WARM-HORIZONTAL-FLIP.md) measures in-memory
owned outputs and bounded storage reuse, with independent confirmation samples.
The [warm PNG framing round](WARM-PNG-FRAMING.md) records the initial three-pair
screen and one reader optimization; PNG decode remains above the 2x target.
The [literal prefix lookup round](WARM-PNG-LOOKUP.md) improves the 16-bit path
further; the measured 8-bit decode becomes the largest known warm gap.
The [contiguous RGBA row round](WARM-PNG-ROWS.md) brings both decode gaps to
roughly 4.3-4.4x; it preserves exact samples and resource admission.
The [inlined word-refill round](WARM-PNG-WORD.md) lowers the measured 16-bit
decode gap to 4.06x; 8-bit remains near 4.32x and is the largest measured gap.
The [distance prefix round](WARM-PNG-DISTANCE.md) lowers the 8-bit gap to 3.66x;
the 16-bit affected-path screen is 3.99x and becomes the largest measured gap.
The [safe SIMD Adler round](WARM-PNG-ADLER.md) lowers these retained decode
gaps to 2.92x at 8-bit and 3.25x at 16-bit, with verified scalar fallback.
The [eight-byte CRC round](WARM-PNG-CRC.md) lowers the decode screens to
2.75x / 2.76x; its affected encode screen is 0.80x, with confirmation pending.
The [filter-None dispatch round](WARM-PNG-NONE.md) lowers the retained decode
screens to 2.36x at both depths. They remain above the target.
The [JPEG forward-DCT round](WARM-JPEG-DCT.md) measures one warm matched-quality
4:4:4 encode pair and vectorizes independent coefficients without changing bits.
This largest measured gap remains above the target.
The [JPEG RGB extraction round](WARM-JPEG-RGB.md) reduces repeated row access
and uses the exact normalized 8-bit alpha blend, preserving all encoded bytes.
The [JPEG byte-output round](WARM-JPEG-BYTE.md) removes per-byte heap arrays
while retaining the same span dispatch, cancellation and output admission.
The [JPEG decode screen](WARM-JPEG-DECODE.md) reuses that prepared JPEG and
measures complete decode to independent RGBA8 on both subjects.
The [inverse-DCT round](WARM-JPEG-INVERSE.md) preserves the frozen scalar pixels
and accumulation order while computing independent output pixels in SIMD lanes.
The [Huffman prefix round](WARM-JPEG-PREFIX.md) admits a generated short-code
lookup per table, preserving code-bit work and scalar failure behavior.
The [full-resolution sampling round](WARM-JPEG-SAMPLING.md) reads exact samples
directly and retains both differing timing screens and their uncertainty.
The [shared inverse-DCT products round](WARM-JPEG-INVERSE-ROWS.md) interleaves
independent AVX-512 output rows while preserving each pixel's arithmetic order.
The [block-coordinate round](WARM-JPEG-COORDINATES.md) derives encoder geometry
once per block while preserving every sampled pixel and encoded byte.
The [native-prepared JPEG decode screen](WARM-JPEG-NATIVE-DECODE.md) selects
one existing native JPEG explicitly, using the same original source raster.
The [eight-pixel 4:2:0 render round](WARM-JPEG420-BULK.md) uncovers one fixed
NativeDecode420 case on that raster and replaces repeated scalar geometry/color
work with bulk interpolation and color conversion, preserving every owned byte.
The native comparison includes decoder reset, exact chroma-grid expansion,
conversion, intermediate/output acquisition and disposal. Its default selection
still covers one 4:4:4 input; each explicit decode selection runs one pair.
The [reused-row 4:2:2 round](WARM-JPEG422-ROWS.md) selects one fixed NativeDecode422
input on the same raster. Full vertical chroma resolution reuses one row in the
existing eight-pixel renderer, removing repeated scalar interpolation/color work
with exact owned output bytes. The native boundary includes horizontal chroma
expansion and intermediate/output ownership; no quality enumeration is needed.
The [grayscale byte-expansion round](WARM-JPEGGRAY-PIXELS.md) selects one fixed
OwnedDecodeGray input on the same raster. Repeating already-rounded luma bytes
into RGB removes identity interpolation and floating-point round calls, with safe
AVX2 packing and scalar tails. Both decoders consume identical resident JPEG
bytes; the native boundary includes GRAY8-to-RGBA conversion and output ownership.
The [progressive accounting rejection](WARM-JPEGPROGRESSIVE-ACCOUNTING-REJECTION.md)
adds one lossless six-scan ProgressiveGray input from that same raster. Coefficient,
quantization and pixel hashes remain exact. Its 2.39x baseline identifies the
next measured gap; the small accounting trial is discarded and production restored.
The [EOB-tail correction rejection](WARM-JPEG-CORRECTION-REJECTION.md) is also flat,
with both quiet guards passing. A separate profile places about 93% of sampled
CPU in scan decoding. The [grouped refinement round](WARM-JPEG-REFINEMENT.md)
reduces complete decode about 20%, with an independent quiet confirmation at
1.914x FFmpeg, conservative upper 1.942x. Spectral coefficient traversal and safe
AVX2/BMI2 corrections preserve pixels, logical work, memory admission and scalar
failure/cancellation behavior. This fixed pair clears 2x; broader coverage stays
open. No image, quality or scan-strategy sweep is added.
The [progressive 4:2:0 coverage point](WARM-JPEG-PROGRESSIVE420.md) uses one
lossless rewrite of the existing native baseline input. All valid coefficients,
quantization, geometry and owned pixels remain exact. Independent confirmation
is 1.881x FFmpeg, conservative upper 1.894x including every actual measurement.
This adds one explicit case and no production change or source raster.
The [progressive 4:4:4 baseline and wider-group rejection](WARM-JPEG-REFINE16-REJECTION.md)
use a lossless rewrite of the existing full-resolution native input. The 2.13x
baseline exposes the largest remaining measured gap. A 16-lane correction trial
is slower and is discarded; all production bytes are restored. The next diagnosis
counts correction prefixes and EOB versus zero-run work before another traversal
primitive. No additional performance raster or scan/quality sweep is added.
The [isolated traversal count and direct-boundary rejection](WARM-JPEG-INSERTION-REJECTION.md)
find 54,930 empty insertion prefixes. A direct-boundary check is flat and its
post-load guard fails; it is discarded. The [per-block mask round](WARM-JPEG-MASK.md)
reuses nonzero positions across refinement symbols, reducing confirmed managed
time 12.4% while preserving exact semantics. Its independent quiet confirmation
is 1.860x FFmpeg, conservative upper 1.878x including every actual sample. This
fixed point clears 2x; broader required warm coverage remains open.
The [adaptive long-prefix round](WARM-JPEG-LONG-PREFIX.md) extends input-derived
Huffman lookup for nine/ten-bit codes with explicit memory/work admission.
The [magnitude-category round](WARM-JPEG-CATEGORY.md) replaces the coefficient
width loop with the BCL leading-zero primitive while preserving encoded bytes.
The [sample-conversion round](WARM-JPEG-ROUND-SAMPLES.md) uses safe SIMD for final
inverse-DCT rounding and byte output, preserving half ties and exact decoded pixels.
The [coefficient-quantization round](WARM-JPEG-QUANTIZATION.md) preserves every
quotient and encoded byte, with an explicit safe stack destination and modest gains.
The [component-row round](WARM-JPEG-COMPONENT-ROW.md) vectorizes exact 8-bit alpha
blending and component construction; encode screens improve to roughly 2.5x.
The [decode color-row round](WARM-JPEG-COLOR-ROW.md) preserves every decoded pixel
while constructing full-resolution RGBA rows with safe SIMD; decode remains above 2x.
The [baseline coefficient-block round](WARM-JPEG-BASELINE-BLOCK.md) separates
admitted baseline scans from progressive decoding while preserving failure boundaries.
The [canonical fallback-prefix round](WARM-JPEG-CANONICAL-PREFIX.md) consumes eight
prefix bits together when no short code is assigned, preserving source positions and work limits.
The first [warm VP8 quality preparation](WARM-WEBP-MATCH.md) is unmatched and
retained explicitly; it supplies no warm speed ratio.
The [still VP8L decode screens](WARM-WEBP-DECODE.md) add one lossless pair from
the existing raster, with exact RGBA and independent outputs. This direction
becomes the largest measured warm gap; other WebP decode strategies remain open.
The [admitted VP8L short-prefix round](WARM-WEBP-PREFIX.md) replaces short-code
bit traversal with independently generated canonical entries. It lowers this
gap to roughly 2.05x while retaining exact pixels and failure boundaries.
The [rejected JPEG primitive trials](WARM-JPEG-REJECTED-PRIMITIVES.md) retain
three discarded candidates, 27 additional boundary controls and one current
CPU profile. The retained native JPEG decode gap remains about 2.91x.
The [short-code Huffman wrapper round](WARM-JPEG-HUFFMAN-HOT.md) moves canonical
fallback into a helper and verifies actual wrapper inlining. Native-input repeats
improve to 2.79x / 2.80x; the existing owned-input screen improves to 2.74x.
The [rejected MCU-coordinate hoist](WARM-JPEG-MCU-REJECTION.md) moves division
out of the component block loop but shows no useful gain. The accepted wrapper
implementation and its warm ratios remain the baseline.
The [certified separable inverse round](WARM-JPEG-SEPARABLE.md) preserves the
original rounded bytes through an error bound and direct fallback. Native decode
screens improve to 2.69x / 2.68x; the existing owned-input screen reaches 2.63x.

For warm memory comparisons, the Linux [original FFmpeg reference worker](../tools/WarmReference/README.md)
excludes process launch, IPC and file transport from native elapsed samples.
After building that worker, use `./bench.ps1 -Smoke -Warm -Cpu 0 -NoBuild` to
validate the two horizontal-flip cases, or replace `-Smoke` with `-DedicatedVm`
on the quiet VM for measurement. Both subjects use one pinned CPU; managed
workers disable server/background GC. Native samples bracket the managed
BenchmarkDotNet iterations. `warm-parity.json` joins managed and internally timed
native samples; it never treats protocol latency as the FFmpeg operation time.
The selected CPU must belong to the machine's allowed affinity. Flip and PNG
require no encoder calibration. Other supported operations remain
unmeasured until their own equivalent warm cases are added.
Use `-WarmConfirm` with `-DedicatedVm -Warm` when short-run uncertainty requires
more samples on the same two inputs.

For the separate PNG worker, build/verify it as described in its reference
instructions, then use `-WarmFamily Png`. The initial selection contains only
Encode8, Decode8 and Decode16 on the same two original rasters. Add
`-WarmCases Decode16` or `-WarmCases Decode8,Decode16` to iterate affected
decode paths without running unrelated cases or quality calibration.
Explicit `-WarmCases Encode16` adds required RGBA16 encode coverage on the same
original raster, while retaining the three-case default and three-pair cap.
The [bulk chunk CRC round](WARM-PNG16-ENCODE-CRC.md) removes bitwise encoder
payload CRC work without changing the encoded PNG bytes or compression settings.
Native sample endian conversion and prepared-frame reuse are declared in its
workload boundary and evidence.

`-WarmFamily Jpeg` selects one RGBA8 4:4:4 encode pair on the existing 513x259
raster. Follow the reference-worker instructions to prepare one native quality
anchor against at most eight managed settings outside timing. The fixed match
band is 0.25 dB; unmatched pairs refuse measurement. Both timed operations
include alpha flattening onto RGB 0x204060 and full-range BT.601 conversion.
Setup and cleanup revalidate selected outputs without quality enumeration.

`-WarmFamily Webp` selects one still VP8 encode pair on the same RGBA8 alpha
raster. Its separate native worker accepts one quality anchor, with at most
eight managed settings and the same fixed match band. Effort is 4 on both
subjects. Source quality uses alpha-weighted RGB before composite rounding;
alpha is exact and decoder agreement still includes hidden RGB. Every native
operation includes RGBA-to-BGRA repacking and complete independent WebP bytes.

For rapid horizontal-flip tuning, use the explicit six-case selection:

```powershell
./bench.ps1 -Smoke -HorizontalFlip
./bench.ps1 -DedicatedVm -HorizontalFlip -NoBuild
```

This selection needs no encoder calibration. It reuses just two original
513x259 rasters, RGBA8 and RGBA16, with explicit zero-alpha hidden RGB. Two
`HorizontalFlipPrimitive` cases measure the warm public operation, including
allocation and disposal. These diagnostic rows have no native parity ratio.
Four `HorizontalFlipBatch` cases compare FFmpeg and managed edits of 32 exact
repeats of each raster, reading the same raw input and writing all output in one
process per subject. Startup/JIT, file transport and context reuse remain in
the batch boundary. Repeated frames do not add distinct source images.
Setup and cleanup validate exact independently placed pixels; batch receipts
include every frame, output hashes and separate untimed child-memory counters.
Both families retain full BenchmarkDotNet JSON. Measure baseline and candidate
sequentially on the quiet VM with identical jobs and inputs; Dry runs verify
execution and output only.

After restoring/building and generating the default representative quality
calibration below, `./bench.ps1 -Smoke` runs a small routine selection:

- Every accepted JPEG/VP8 single-image quality pair from the twelve-profile
  calibration, retaining every excluded anchor in the catalog.
- PNG decode/crop/nearest-resize/encode at width 129, depths 8/16 and one frame.
- Horizontal flip, clockwise quarter turn and nearest upscaling on one 129-wide
  RGBA16 input per workload.

Editing/PNG workloads reuse two original raw inputs in addition to the six
calibration sources: at most eight distinct source images overall. Each workload
has FFmpeg and managed methods. The selected scope and full parameter values are
recorded in `representative-run.json` under that run's ignored artifacts.
Full JSON reports retain statistics and individual measurements for analysis.
Builds and out-of-process Dry executions are sequential; smoke verifies execution
and outputs, and supplies no performance conclusions.

An explicit `-Filter` opts into the corresponding extended families. Use
`./bench.ps1 -DedicatedVm` for the same representative selection's Short job on
the quiet VM. Add `-NoBuild` when the current Release binaries are already built.
Initial restore is separate; routine builds use cached dependencies. Larger
matrices are optional targeted work, not routine baseline prerequisites.

The current BenchmarkDotNet cases compare PNG, baseline/progressive JPEG and lossy/lossless WebP
decodeÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Â¦ÃƒÂ¢Ã¢â€šÂ¬Ã…â€œflipÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Â¦ÃƒÂ¢Ã¢â€šÂ¬Ã…â€œPNG encode pipelines. Both subjects launch one process, read the same
encoded input file and write one encoded output file. Source pixels and fixture
generation happen outside the timed methods. FFmpeg runs with its normal CPU
capabilities and one codec thread; it is the baseline for each case.

Global setup validates exact PNG/lossless WebP pixels and the documented JPEG/VP8
error bounds. PNG invocation cases cover both 8-bit and 16-bit samples and check
each subject against the exact flipped source, including alpha and hidden RGB.
Lossless WebP cases cover asymmetric transparent pixels and
repeated-color input. Native fixture encoding may replace zero-alpha RGB;
setup records the number of changed hidden pixels and requires unchanged alpha
and every visible RGB sample. Both timed subjects read the same resulting VP8L
file and must preserve every sample in that encoded input, including hidden RGB.
JPEG chroma expansion and color conversion match the settings in the correctness
oracle. VP8 cases expand chroma and convert with explicit limited-range BT.601
before flipping, including at odd dimensions; alpha must match exactly and RGB
must satisfy the documented conversion bounds. The VP8 baseline's color conversion
passes through RGBA16 and explicit nondithered RGBA8 reduction to preserve
saturation at extreme legal samples; that extra format pass is part of the
recorded FFmpeg workload. These are invocation benchmarks: they include startup, filesystem IO and
PNG encoding. Managed allocation measurements cover the calling process only;
they do not represent subprocess memory usage or isolated codec throughput.
JPEG also has separate decode, encode and resize/transcode invocation cases,
described below. Decode and encode have 32-image batches; exact geometry has
single-image and 32-image raw-file cases. All current workload setups record
Linux child-process memory counters in separate untimed executions. Fresh VM
execution is required for new measurement snapshots. Broader editing/content/size
matrices are optional extensions to the representative baseline.

The ten original invocation profiles now write the same schema-2 validation
records as the newer workloads: original input and output hashes, geometry,
precision, alpha checks, actual child/library/oracle identities and complete
argument vectors. Every horizontal flip is checked against independently placed
pixels, and each managed encoded output must match the complete public API
workflow. Both PNG outputs are independently decoded by FFmpeg and Lokad.Image;
lossy input comparisons retain their existing RGB bounds. These checks happen
outside timed methods and retain the original process/file boundaries and
encoder settings.

`ExifDecodeRawFile` reads one or 32 numbered still files and writes all decoded,
oriented RGBA frames to one raw file. Its 192 profiles cover both sizes, all six
PNG/JPEG/WebP decode profiles, all eight EXIF values and both frame counts.
The batches alternate little/big-endian TIFF directories. Inputs use original
phased pixels with hidden RGB at the first zero-alpha pixel and the owned
encoders; progressive JPEG repeats the original coefficient/scan construction.
Setup validates coded and oriented dimensions, metadata dispatch, cleared
orientation, complete frame order and exact public-API output.

Native color/chroma conversion precedes an explicit RGB orientation filter.
Autorotation is disabled: native transform selection happens before timing,
whereas the managed child reads EXIF and applies it during decode. These rows
compare complete decode/orient file workflows and do not compare EXIF parser
speed. Native geometry must exactly equal independent forward placement of
its decoded pixels. Managed PNG/VP8L pixels are exact, including hidden RGB and
16-bit samples; JPEG/VP8 retain maximum RGB error 4/RMSE 1.5 and exact alpha.
Normal orientation supplies a decode-only control. All encoded and raw reference
hashes, byte orders, argument vectors and file byte counts are recorded.

`IdentifyInvocation` compares one encoded file to a captured JSON header result,
using FFmpeg's `ffprobe` companion as the baseline. Twelve profiles cover both
sizes and all six codec profiles, with no EXIF. Format, coded dimensions, sample
precision and alpha must match the known input. The different JSON schemas and
byte counts are recorded; their common field projection is checked outside
timing. Both methods capture complete stdout into a bounded byte buffer. Evidence
files are written only during setup. The baseline requests `skip_frame=all`;
codec initialization, parsing and codec-specific pixel work may remain part of
ffprobe inspection. These are inspection invocations, not isolated parser
throughput. They make no comparative metadata-discovery claim and have no batch
row. Set `LOKAD_IMAGE_FFPROBE` to select the companion executable; reports record
its actual hash/version separately from FFmpeg. Linux child-memory executions
must preserve the exact validated stdout.

`NearestResizeRawFile` streams raw RGBA8/RGBA16LE through nearest-neighbor
resizing into one raw output file. Thirty-two profiles cover widths 129/513/1025/2049,
both sample depths, one/32 frames and both scale directions. Source heights are
67/259/515/1027; downsize targets are 64x33/256x129/512x257/1024x513 and
upscale targets are 257x135/1025x519/2049x1031/4097x2055.
Each independently phased frame must match integer rational source-cell
selection and the exact public workflow, including alpha and hidden RGB.
Both processes read the same input once and write every output frame; there is
no codec work, source preload, looping or output suppression.

The native baseline extracts RGB and alpha, scales integer RGB with FFmpeg's
[zscale point filter](https://ffmpeg.org/ffmpeg-filters.html#zscale), and scales
alpha separately by replicating it into a second integer RGB input. It extracts
the resulting components, normalizes sample aspect ratios, merges planar GBRA
and packs RGBA. Both contexts retain full range and GBR matrices with dithering
disabled. Separate alpha preserves hidden RGB. Explicit planar 8/16-bit formats
avoid intermediate depth conversion. A live control checks all 65536 values in
every channel, including alpha. Extraction, replication, both point contexts,
merging and packing belong to the measured baseline. FFmpeg uses normal CPU
capabilities and one codec/filter thread; zimg chooses its CPU implementation
automatically. FFmpeg's `-cpuflags 0` does not disable zimg's own dispatch.
Invocation and 32-frame amortized workloads are reported separately, with
untimed Linux kernel child-memory counters. The development Linux bootstrap
pins libzimg and enables this filter; it is not a production package dependency.

The earlier swscale `neighbor` profile selected adjacent source rows at two
boundaries in the 1025x515 to 2049x1031 control. The point profile retains all
larger cases and the exact-output gate. Differential tests also independently
evaluate the source-cell equation using FFmpeg's
[geq pixel access](https://ffmpeg.org/ffmpeg-filters.html#geq) on four scalar
planes. That expression oracle establishes correctness; it is not substituted
for the scaler performance baseline. A complete current preflight and benchmark
worker validation remain prerequisites for dedicated-VM timing comparisons.

An original exact rational reference also checks the public bilinear/bicubic
kernels independently. Transparent filtered native equivalence remains pending:
the inspected float-premultiply/scale/unpremultiply profile introduces precision,
edge and intermediate-clipping differences. These findings do not relax existing
opaque-downsize bounds or establish a matched filtered-resize speed comparison.

`PngCropResizePipeline` compares PNG decode, a contained integer crop, nearest
resize and PNG encode in one process per subject. Its 32 profiles cover widths
129/513/1025/2049, both sample depths, one/32 distinct still files and both scale
directions. Each crop starts at (3,5) with dimensions (width-8,height-10).
Downsize targets halve the crop dimensions with integer division; upscale
targets are (2*cropWidth-1,2*cropHeight+1). The independent reference selects
original source samples directly from integer destination coordinates and the
crop offset. Native and public decoders validate all input and output samples,
including alpha, hidden RGB and 16-bit precision; managed encoded outputs must
also equal the complete direct public workflow.

The native baseline uses the same separate RGB/alpha point scaler described
above. It forces integer RGBA precision before exact cropping and PNG encoding.
FFmpeg uses PNG predictor `none` and compression level 6; Lokad.Image uses its
public PNG defaults. These are independently specified compression settings,
not a matched compression-effort comparison. Reports include compressed sizes,
ordered file hashes, process arguments and child identities. Both subjects read
every prepared PNG and write every output PNG inside their benchmark methods.
Validation, fixture creation and child-memory counters happen separately.
Cleanup checks every input/output file and rejects changed or extra outputs.
Use `--validate-png-crop-resize-full` for all profiles and
`--validate-png-crop-resize-cleanup` for actual corruption controls. This adds PNG
nearest pipeline coverage; broader codec/filter/content pipelines and fresh
dedicated-VM execution remain necessary before complete performance claims.

`JpegDecodeInvocation` reads a baseline/progressive JPEG file and writes a tightly
packed RGBA8 file on both sides, without PNG encoding. Its 16 parameter profiles
cover grayscale and 4:4:4/4:2:2/4:2:0 at 129x67 and 513x259. Progressive inputs
are original coefficient/scan constructions with restart markers, including
larger grayscale frames. Baseline inputs use original opaque asymmetric pixels.

`JpegEncodeInvocation` reads the same original RGBA8 or little-endian RGBA16 raw
file and writes baseline JPEG. Twelve profiles cover both sizes/depths and all
three color sampling modes. Quality 85 and FFmpeg qscale 5 are independent
settings. The native 16-bit input pipeline explicitly implements the public
nearest-integer reduction; a live control checks every possible channel value.
One-component grayscale encoding is excluded from comparative rows because the
registered native MJPEG encoder only emits three-component YUV. Gray decoding
and the owned gray encoder remain covered by correctness/quality tests.

`JpegResizeTranscodeInvocation` reads baseline/progressive 4:2:0 JPEG, downsizes
to 64x33 or 256x129, and writes baseline 4:2:0 JPEG. Twelve profiles cover all
three filters and both sizes/input modes. Resize operates on opaque RGBA8 after
the explicit JPEG color/chroma conversion. Native nearest-neighbor uses `neighbor`;
bilinear uses the triangle kernel; bicubic explicitly sets B=0,C=0.5. Gamma
correction and dithering are disabled. See [FFmpeg scaler options](https://ffmpeg.org/ffmpeg-scaler.html).
Nine original raw-raster controls match nearest exactly and filtered downsizing
within one channel value. Each setup separately verifies native resize against
the public kernel on that native decoded raster (nearest exact, filtered maximum
2/RGB RMSE 0.5) before encoding. Each final JPEG is independently decoded and
must satisfy the unchanged decoder bounds. The tool's managed output must also
equal the requested public-API encode/resize workflow.

Transparent resizing and high-frequency bicubic upscaling are not established
as equivalent native comparisons here. The latter differs by up to six channel
values on the inspected original 129x67-to-257x135 control, despite explicit
matching B/C settings. The broader resize matrix remains pending; this difference
does not change the library's specified kernel or its analytical correctness tests.

JPEG setups save untimed JSON evidence under ignored
`artifacts/benchmark-validation/`: executable/tool hashes and identity, input/output
hashes, exact argument vectors, actual sampling factors, encoded bytes, source
error and decoder/resize agreement. For transcoding, source error is measured
against each subject's validated resized raster and excludes earlier input-JPEG
loss; the cross-subject resize-stage difference is recorded separately. These
records contain no timing measurements. Schema 2 identifies the production DLL
loaded by the ImageProbe codec child and records the setup's in-process library
separately; BenchmarkDotNet can rebuild that validation library into a different
binary. On Linux they also record peak resident memory and page faults from
separate untimed executions, described below. The benchmark methods use normal
native CPU capabilities and one codec/filter thread, include process
startup and file I/O, and return output byte counts. They do not represent isolated
in-process throughput or matched-quality encoder speed.

`DecodeBatch` launches one process per subject to decode 32 numbered still files
into one tightly packed raw output file. Twelve profiles cover 129x67 and 513x259,
PNG RGBA8/RGBA16, baseline/progressive 4:2:0 JPEG, lossless WebP with alpha and
lossy WebP with alpha. Both subjects read each same input file once and write all
frames in order, with autorotation disabled. FFmpeg uses `image2` with an explicit
starting index, frame count and passthrough frame timing; each still bitstream is
independent, with no temporal prediction or looping. Frame count, geometry,
precision and per-frame samples are validated before benchmarking: PNG/VP8L are
exact, JPEG/VP8 retain maximum RGB error 4/RMSE 1.5 and exact alpha. The managed
tool's complete output must also equal the exact public decode workflow.

The original asymmetric pattern varies through 32 recorded phase values;
phase zero preserves the frozen correctness corpus. Progressive JPEG repeats the
original authored coefficient/scan construction with restart markers. Baseline
JPEG and VP8 inputs are independently encoded by the registered native oracle;
PNG and VP8L inputs use the owned encoders. Setup records every input and reference
hash, actual encoded bytes and complete per-frame output validation. Inputs and
references are prepared outside the timed methods. The top-level input hash
identifies the ordered per-file receipt index; input bytes sum the actual
encoded files, with the index size recorded separately.

`EncodeBatch` streams 32 raw frames from one tightly packed RGBA8/RGBA16LE file
and writes 32 numbered independent still bitstreams. Thirty-two profiles cover
129x67, 513x259, 1025x515 and 2049x1027 at both depths, with PNG, baseline 4:2:0
JPEG and lossless/lossy WebP. Both subjects read
the same raw file once; there is no PNG input decode, looping or animation.
Every managed bitstream must equal the requested public API encoding. Each
output is independently decoded by FFmpeg and Lokad.Image, checked for format,
geometry and precision, and recorded with its encoded/decoded hashes and bytes.
PNG is exact at source precision; WebP/JPEG reduce 16-bit samples once to the
public nearest-integer byte value. WebP alpha is exact, and lossy decoder
agreement retains maximum RGB error 4/RMSE 1.5. Source error and encoded size
are recorded separately for each encoder.

Raw-file hashes are streamed, and fixture reference buffers are released after
setup validation. Cleanup checks every final numbered output against the validated
index, including the outputs from timed executions. Process execution limits are
one minute for the two smaller sizes and ten minutes for the two larger sizes;
the same limit applies to separate Linux counter executions. These limits guard
the development harness and do not define performance budgets.

The complete encode-batch preflight and cleanup mutation controls do not require
a quality catalog. With FFmpeg configured, run the built Release harness:

```sh
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --validate-encode-batches
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --validate-encode-batch-cleanup
```

The lossless WebP comparison uses an explicit restriction: shared inputs raise
byte-zero alpha to 1 (257 at source depth 16) while retaining all original RGB.
Every normalized pixel therefore has nonzero alpha. The registered FFmpeg
libwebp wrapper does not expose its hidden-RGB preservation control and can
replace zero-alpha RGB even when the input RGB is zero. These rows compare
partial transparency and cannot establish general hidden-RGB encoding behavior.
The number of adjusted alpha pixels is recorded for every frame. PNG and lossy
WebP inputs retain their original samples; Lokad.Image's WebP hidden-RGB contract
has independent correctness tests.

JPEG quality 85/native qscale 5 and WebP quality 85/effort 4 are subject-specific
settings, not matched quality or compression effort. PNG uses owned default
optimal compression and native level 6/filter none. Both encoder output
directories must contain all 32 ordered files; the output index hash identifies
each file's name, byte length and bitstream hash. The input hash identifies the
actual raw file, with per-frame offsets, lengths, source and reference hashes.

`GeometryRawFile` streams raw RGBA8/RGBA16LE through exact geometry and writes
every result to one raw file. 160 parameter profiles cover all four sizes/both depths,
one or 32 frames, explicit/centered/clamped crops, horizontal/vertical flips,
all three quarter turns and both transposes. Crop profiles produce half-size
rasters: explicit origin (3,5), integer-centered origin, or requested origin
(source width + 7,-7) clamped to the source. FFmpeg receives the corresponding
resolved explicit rectangle with `exact=1`. Quarter turns and transposes use
explicit transpose directions; a half turn uses `hflip,vflip`.

Every public transform and both complete tool outputs must equal independently
constructed source-to-destination coordinate samples. Frame count, order, output
geometry and precision are explicit; all channel values, including alpha and
hidden RGB, are exact. Phases vary across the 32-image inputs. Preparation is
outside the benchmark methods, and both subjects stream the same input once.
These rows include raw transport and process/JIT startup but no image codec
decode or encode. They do not exercise EXIF metadata handling or establish
filtered resize comparisons. Invocation and 32-image results remain distinct.

Both raw edit families write independently computed references to an owned
temporary file during setup and compare actual outputs in 64 KiB chunks. Each
source frame still has exact public API and independent-coordinate evidence;
all complete input/reference/output files and every ordered frame are hashed.
They retain one source/reference raster at a time rather than the entire batch.
Linux counter runs and final cleanup must preserve the validated output hashes
and lengths. Widths 1025/2049 allow ten minutes per process, including counters;
smaller profiles keep one minute. These are execution guards, not speed budgets.

These batch latencies amortize startup and can be divided by 32 for per-image
figures. Codec context reuse, remaining process/JIT startup, filesystem transport
and raw output costs are included. They do not establish isolated in-process
throughput or a fully warmed persistent service. Invocation and batch results
must remain distinct in performance tables. Larger counts and the broader
content/size and editing matrix remain required before a complete baseline.

All current invocation and batch setups run each complete subject under Linux
[GNU `time`](https://www.gnu.org/software/time/manual/html_node/Memory-Resources.html)
in a separate untimed execution. Kernel `wait4`/`rusage` supplies peak resident
memory and major/minor page-fault counts;
the report records the counter executable hash and version. This includes the
codec child process's runtime, JIT, native libraries, buffers, mappings and
threads. It excludes the BenchmarkDotNet caller's allocation counter. Counter
wrapping must preserve the validated output. One peak-RSS sample is execution
evidence, not a statistical memory budget or a count of allocations. Other
platforms explicitly record these counters as unavailable. Install Ubuntu's
`time` package if `/usr/bin/time` is absent. Timing methods do not launch the
counter wrapper.

`Vp8EncodeInvocation` compares PNG-file decode to lossy WebP-file encode with one
process per subject. Quality 85 and effort 4 are explicit subject-specific
settings, not a claim of matched quality or speed. Setup independently decodes
both outputs with FFmpeg, checks exact source alpha and the managed reconstruction
bounds. Each setup records both outputs' bitstream hashes, encoded size,
independently decoded samples, decoder agreement and source RGB error. The
managed bitstream must match the requested public encode workflow, and both
subjects must emit VP8 rather than VP8L. The original corpus report generated by
`tools/Fixtures` with `--quality` supplies broader quality/size evidence; future
timing comparisons must account for these quality and size differences. This
invocation includes PNG input and process/file overhead, rather than isolated
encoder throughput.

`MatchedQualityEncodeInvocation` reads one RGBA8/RGBA16LE raw file and
writes one baseline JPEG or lossy VP8 file per subject. Its routine calibration
has twelve representative profiles from six source images, described below.
An explicitly requested `full` matrix has 576 profiles: 192 opaque profiles at widths 129/513/1025/2049, both depths,
JPEG 4:4:4/4:2:2/4:2:0 and VP8,
and six original patterns (gray ramp, color tiles, soft gradient, asymmetric
high-frequency pixels, seeded noise and procedural texture), plus 96 transparent
VP8 profiles and 288 transparent JPEG profiles at all four sizes/depths, every pattern
and each color sampling profile. Transparent inputs use
horizontal alpha gradients with literal low-alpha first-row boundaries or binary
16-pixel checker edges. RGB samples stay identical when alpha is changed, including
hidden RGB. Gray ramps still
use the requested three-component JPEG profile; these rows do not compare the
one-component gray encoder. Transparent JPEG uses an explicit RGB background
`0x204060`; original boundary controls also cover black and white backgrounds.
The same calibrated profiles also supply the repeated-input 32-image batches
described below. Larger-image and varied-frame matched-quality workloads remain
pending.

Routine calibration uses six original source images in twelve codec profiles:
four 129-pixel-wide opaque RGBA8 inputs cover the three JPEG sampling modes and
VP8; two 513-pixel-wide RGBA16 inputs cover gradient and binary alpha with all
four encoders. The six patterns appear across those sources. This representative
matrix establishes those declared workloads, not every size/content combination.

Each profile samples owned qualities 1, 25, 50, 75 and 100, with at most three
targeted refinements: a strict maximum of eight owned encodes against two native anchors.
Already matched targets stop after the five seeds. Refinements propose a quality
within a measured local error bracket using log-SSE interpolation, with a midpoint
fallback; they do not assume global monotonicity. Native anchors remain
MJPEG qscale 16/5 or libwebp quality 50/85. VP8 effort is fixed at 4 on both sides;
these numeric controls are encoder-specific. Every bitstream has independent
native/public decoding, exact geometry/target alpha and the unchanged unweighted
decoder RGB bounds, including at zero alpha. Source error uses the same explicit
native decoder against the RGBA8 target. Opaque rows use unchanged whole-raster
RGB error. JPEG transparent rows measure RGB after background flattening and
depth reduction with one nearest-integer rounding; target alpha is 255. The
native [geq point equation](https://ffmpeg.org/ffmpeg-filters.html#geq) uses the
full-precision source, and must reproduce that integer target exactly before
encoding. The native baseline includes general expression evaluation, flattening,
depth/color conversion and process/file overhead; it does not establish isolated
JPEG encoder throughput. Input and target alpha hashes are recorded separately.
The Linux oracle bootstrap enables and verifies this required filter; rebuild
older oracle installations before selecting the expanded matrix.
Transparent VP8 rows use premultiplied RGB error before
composite rounding: integer `sum((alpha * RGB difference)^2)` with scale 255 for
reported RMSE/PSNR. Equal source/output alpha makes this the unrounded visible
error over any background; zero-alpha RGB contributes zero. All raster RGB
channels count as samples, including those zero contributions. Source weighting
never relaxes decoder agreement or exact alpha. Native encoding uses the actual
benchmark commands, normal CPU capabilities and one codec/filter thread.

The matching band is fixed at a maximum 0.25 dB PSNR gap before generation. The
closest sampled quality is selected without assuming monotonic error: compare exact integer
SSE ratios, then break equal-distance ties by encoded size and quality setting.
This bounded search does not claim the globally closest setting among 1..100.
Both exact-source reconstructions match separately, without finite PSNR. Every
unmatched native anchor retains its closest candidate, measured gap and reason
in the complete report; these anchors produce no matched-quality timing row.
Matching the recorded source error does not establish equal perceptual quality,
photographic behavior or compression effort. All input recipes and report
generation belong to this repository; no upstream corpus is used.

Generate the untimed calibration outside BenchmarkDotNet, then set the returned
manifest path before selecting this family:

```powershell
dotnet build benchmarks/Lokad.Image.Benchmarks -c Release --no-restore --tl:off
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --calibrate-encoder-quality
$env:LOKAD_IMAGE_QUALITY_CALIBRATION = 'artifacts/encoder-quality/<run-id>/calibration.json'
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --validate-matched-encoder-quality
./bench.ps1 -Smoke -Filter '*MatchedQualityEncodeInvocation*'
```

Calibration files and every native anchor/closest owned bitstream remain ignored
under `artifacts/encoder-quality/`, including unmatched anchors. They allow
independent decoding and source-metric checks. Partial manifests fail explicitly.
Completed manifests must reproduce their complete
profile coverage, the five seed settings, the replayable bounded refinement order,
the eight-setting cap, fixed anchors and match/exclusion selection;
the child library, probe, harness, runtime, platform, oracle and recipe identities
must still match. Benchmark setup regenerates the raw source and reproduces both
selected bitstreams, independently decoded pixels, source errors and matching
band. Managed output must equal the public encoding workflow. Reports identify
the calibration manifest hash and every omitted anchor count. Quality search and
validation never happen in timed methods. The `opaque` and `transparent` scopes
generate their complete 192/384-profile subsets. The default `representative`
scope has 12 profiles from six source images; `full` explicitly requests all 576
and is optional extended validation rather than a routine prerequisite.
The `vp8-transparent` and `jpeg-transparent` scopes contain 96/288 profiles.
The `larger` scope contains the complete 288-profile subset at widths 1025/2049.
A twelve-profile `control` scope includes four opaque codec profiles, two
16-bit VP8 alpha profiles and six 16-bit JPEG alpha/background profiles at width
129. `larger-control` applies those same 12 profiles at width 1025. The
`varied-control` scope contains all six patterns for 8-bit transparent JPEG444
and opaque VP8 at width 129, for 12 profiles. These
scopes are labelled separately and reports describe the selected scope.
Extended matrices are optional targeted validation, not release prerequisites.
Regenerate the representative calibration on the dedicated VM before
its worker, kernel-counter and statistical runs.
Keep a current calibration configured for the BenchmarkDotNet smoke commands:
parameter-source discovery also touches the matched-quality families before
method filtering. The `EncodeBatch.*` filter selects the existing four-encoder
batch family explicitly.

`MatchedQualityEncodeBatch` streams 32 copies of each calibrated raw raster from
one file and writes 32 independent numbered JPEG or VP8 still files in one
process per subject. It selects every accepted pair in the supplied calibration,
including the declared opaque and transparent sources, depths, sizes and each color
sampling profile. JPEG batches declare the same background and single-round
flattening contract; VP8 batches retain the normalized alpha. The managed probe
reads each frame into a fresh public image buffer and encodes every output.
FFmpeg reads the same raw sequence with the same quality, conversion and thread
settings, a 32-frame limit and numbered `image2` still output.

Every output must reproduce the corresponding single-image calibration,
including bitstream bytes, decoded samples, source/decoder error and exact alpha.
A codec whose batch output changes requires a separate batch calibration before
it can supply a matched row. Each distinct bitstream is independently decoded
with FFmpeg and the public library once per subject; every repeated file is
compared byte-for-byte with that validated bitstream. Reports record every frame,
its raw offset/source hash, encoded bytes and metrics, the complete ordered
output index and separate Linux child-memory counters. Counter executions must
preserve all files. Cleanup also checks the outputs after the timed calls.

These batches repeat a single raster; the six original content patterns still
span separate parameter profiles. Repetition is explicit in every report and
does not establish varied-frame batch coverage. Startup/JIT, codec context reuse,
raw transport, depth/color conversion, JPEG background evaluation and all file
writes remain included. Invocation and batch results remain separate. Neither
family establishes isolated codec throughput or local performance claims.

After generating a current calibration, validate this family and execute its
smoke cases:

```powershell
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --validate-matched-encoder-quality-batch
./bench.ps1 -Smoke -Filter '*MatchedQualityEncodeBatch*'
```

`MatchedQualityContentBatch` writes 32 numbered independent still files from a
raw sequence in round-robin order over the content patterns in each calibration
group. Geometry, depth, codec, alpha mode, JPEG background and native quality
anchor remain fixed. Managed quality is calibrated separately for each pattern
and supplied as a fixed per-frame list. Quality selection is never timed. Every
pattern in a group must satisfy the fixed 0.25 dB band; one unmatched pattern
excludes the whole group. Reports retain the complete accepted/excluded group
index and the calibration retains every unmatched target/closest bitstream.

Full scopes contain six distinct authored sources per group, repeated in the
declared 32-frame order. Single-pattern `control`/`larger-control` groups verify
execution without establishing varied-content coverage. Every ordered output
must reproduce its exact single-image calibration and public encoding workflow,
including decoded source error, decoder bounds and alpha. Distinct
pattern/bitstream pairs are decoded once per subject; repeats are checked
byte-for-byte. Input offsets, raw/reference hashes, per-frame qualities, output
indexes, total bytes and separate Linux child counters remain explicit. Cleanup
rechecks final outputs. Both matched batch families allow ten minutes per process
at widths 1025/2049 and one minute at smaller sizes. These limits guard execution;
they do not define performance budgets. Startup/JIT, raw transport, option
selection, codec context reuse and file writes remain inside the workload.

Generate a fresh calibration and validate this family before its smoke run:

```powershell
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --calibrate-encoder-quality varied-control
$env:LOKAD_IMAGE_QUALITY_CALIBRATION = 'artifacts/encoder-quality/<run-id>/calibration.json'
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --validate-matched-content-batches
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --validate-matched-content-batch-cleanup
./bench.ps1 -Smoke -Filter 'MatchedQualityContentBatch.*'
```

The cleanup controls alter bytes or add an unexpected file on each subject;
all four must be rejected and their owned temporary directories removed. Target
coverage does not imply completed calibration, worker validation or performance
measurements. Statistical comparisons require the dedicated quiet VM.

Run an execution check:

```powershell
./bench.ps1 -Smoke -Filter '*JpegFlipInvocation*'
./bench.ps1 -Smoke -Filter '*JpegDecodeInvocation*'
./bench.ps1 -Smoke -Filter '*JpegEncodeInvocation*'
./bench.ps1 -Smoke -Filter '*JpegResizeTranscodeInvocation*'
./bench.ps1 -Smoke -Filter '*DecodeBatch*'
./bench.ps1 -Smoke -Filter 'EncodeBatch.*'
./bench.ps1 -Smoke -Filter '*GeometryRawFile*'
./bench.ps1 -Smoke -Filter '*ExifDecodeRawFile*'
./bench.ps1 -Smoke -Filter '*IdentifyInvocation*'
```

The geometry filter selects 320 benchmark cases (160 profiles, two subjects).
Nearest resize selects 64 cases (32 profiles, two subjects). Complete untimed
preflights and eight actual changed-output cleanup controls are available:

```powershell
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --validate-geometry-full
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --validate-nearest-resize-full
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --validate-raw-edit-cleanup
```

The cleanup controls modify a byte or append a trailing byte on each subject.
Both families must reject all four controls and remove owned directories.
A smaller, untimed preflight validates all ten 16-bit operations with 32 frames:

```powershell
dotnet run --project benchmarks/Lokad.Image.Benchmarks -c Release -- --validate-geometry
dotnet run --project benchmarks/Lokad.Image.Benchmarks -c Release -- --validate-exif
dotnet run --project benchmarks/Lokad.Image.Benchmarks -c Release -- --validate-identify
dotnet run --project benchmarks/Lokad.Image.Benchmarks -c Release -- --validate-invocations
```

The EXIF, identify and original-invocation preflights validate their complete
192/12/10 profile matrices without timing. BenchmarkDotNet selects 384 EXIF,
24 inspection and 20 original-invocation cases when both subjects are included.
Preflight success verifies setup and output; BenchmarkDotNet Dry execution
remains a separate check.

The launcher assigns a unique run ID, recorded in worker validation JSON, and
stores BenchmarkDotNet output under ignored `artifacts/benchmark-runs/{run-id}/`.
Compare complete reports from the same run ID; earlier smoke and preflight
records remain separate evidence. The caller's existing run-ID environment is
restored when the launcher returns.

Smoke timing artifacts are local harness output, not performance evidence.
Wildcard filters are passed literally on Windows and Linux. Empty selections,
critical validation failures and failed benchmark cases return a failing status.
Measurement, optimization and published comparisons require the dedicated quiet
VM supplied by the project owner, using `./bench.ps1 -DedicatedVm` after the
benchmark matrix and reproducible machine/tool manifests are complete.

The `Vp8Decode` warm family adds one identical native-input decode pair,
`Native8`, using the existing frozen 513x259 RGBA8 raster and quality-75 VP8/ALPH
input. It does not search encode qualities or require a matched encode pair.
Its [reference protocol](../tools/WarmReference/README.md#warm-native-input-vp8-decode)
includes decoder reset, chroma expansion, color conversion, alpha merge and
owned output allocation/release, with normal SIMD and one CPU/thread.

The explicit `Nearest/Down16` selection reuses the existing full-precision
RGBA16 raster with exact integer center mapping and every output sample checked.
Its [first screen](WARM-NEAREST16-BASELINE.md) reaches 2.12x with wide short-run
uncertainty. [Exact row recurrence](WARM-NEAREST-RECURRENCE.md) then removes
repeated coordinate division and mapping callbacks, reducing the screened mean
by 70.78% and reaching 0.62x, conservative upper 0.64x. This pair clears the
numerical target. Its [prepared confirmation](WARM-PREPARED-CONFIRM.md) supplies
independent quiet evidence for this fixed pair; broader coverage remains open.

The `VerticalFlip/Whole16` warm selection reuses the same existing RGBA16 raster.
[Reversed row copies](WARM-VFLIP-ROWS.md) replace generic per-pixel mapping and
use existing admitted complete-overwrite storage, reducing the screened mean
98.58%, from 67.28x to 0.99x. Both twelve-sample repeats clear the numerical 2x
bound but fail their post-run quiet guards, so independent quiet confirmation
remains open. No image or geometry sweep is added. The
[prepared-job recipe](../tools/WarmReference/README.md) supports fixed short
screens and confirmation without compilation inside the scored windows.

The `HalfTurn/Whole16` selection reuses that same full-precision raster. Its
[complete-pixel reversal round](WARM-HALFTURN-PIXELS.md) replaces generic mapping
with safe SIMD reversal and existing admitted complete-overwrite storage,
reducing the screened mean 98.26%, from 53.87x to 0.90x. The native public-API
graph uses hflip allocation followed by vflip row placement; no additional full
copy is needed for independent ownership. Its independent twelve-sample repeat
passes both quiet guards at 0.933x, with conservative upper 0.967x. Only this
fixed pair is certified; broader half-turn and orientation coverage remains open.

The `Clockwise/Whole16` selection reuses the same raster, swapping 513x259 to
259x513. [Bounded pixel tiles](WARM-CLOCKWISE-TILES.md) replace per-pixel mapping
with typed cache-local placement and existing admitted complete-overwrite
storage, reducing the screened mean 96.19%, from 26.57x to 1.00x. The native
transpose graph allocates independent pixels directly. Only this fixed pair
is measured. Its independent twelve-sample repeat passes both quiet guards at
0.990x, with conservative upper below 1.014x, certifying this fixed pair.
Other quarter-turn and transpose cases remain open.

The `NormalOrientation/Whole16` selection reuses that raster with Normal metadata.
[Bounded row copies](WARM-NORMAL-ROWS.md) remove generic per-pixel mapping and use
existing admitted complete-overwrite storage, reducing the screened mean 98.47%,
from 71.13x to 1.08x. Native null-filter output is explicitly allocated and copied
inside each operation to preserve independent ownership. The twelve-sample repeat
reaches 1.062x, conservative upper 1.0871x, but fails its post-run I/O-wait guard;
independent quiet confirmation remains open. Failed screen guards are retained.
No additional performance raster or shape sweep is added.

The `Counterclockwise/Whole16` selection reuses the same raster and swaps
513x259 to 259x513. Its [bounded tile round](WARM-COUNTERCLOCKWISE-TILES.md) removes
per-pixel mapping, reducing screened mean 96.06%, from 28.17x to 1.085x. Both
short-screen quiet guards pass, with conservative upper 1.198x. The twelve-actual
repeat reaches 1.043x, upper 1.062x, but fails its post-run load guard; independent
quiet confirmation remains open. The native transpose graph allocates independent
output pixels directly, without an extra packing copy. No image/shape sweep is added.

The `Transpose/Whole16` selection reuses the same raster and swaps 513x259 to
259x513. [Bounded tile placement](WARM-TRANSPOSE-TILES.md) removes generic
per-pixel mapping, reducing screened mean 95.84%, from 26.14x to 1.092x. Both
short-screen quiet guards pass, with conservative upper 1.182x. The twelve-actual
repeat reaches 1.088x, upper 1.093x, but fails its post-run load guard; independent
quiet confirmation remains open. The public FFmpeg cclock_flip graph allocates
independent output pixels directly. No image or geometry sweep is added.

The `Transverse/Whole16` selection reuses the same raster with both output axes
reversed after swapping 513x259 to 259x513. Its [bounded tile round](WARM-TRANSVERSE-TILES.md)
reduces screened mean 96.32%, from 27.38x to 1.051x. Both short-screen quiet guards
pass, while the short conservative bound remains 2.195x. The twelve-actual repeat
reaches 1.021x, upper 1.033x, but fails its post-run load guard; independent quiet
confirmation remains open. Native clock_flip allocates independent pixels directly.
No performance image or geometry sweep is added.

The `Clone/Whole16` selection reuses the existing original full-precision raster.
[Complete-overwrite storage](WARM-CLONE-REUSE.md) removes fresh full-array
allocation and zeroing, retaining Clone's row loop and 16 KiB work/cancellation
intervals. Screened mean falls 95.21%, from 22.55x to 1.046x. The twelve-actual
repeat passes both quiet guards at 1.076x, conservative upper 1.089x, certifying
this fixed pair only. Native null-filter output is allocated and copied inside
each operation; output caching and redundant packing copies are excluded.
Broader Clone and editing cases remain open.

The existing `Transpose/Whole8` kernel also clears the same geometry on the
second retained original raster. Its [RGBA8 coverage round](WARM-TRANSPOSE8-PARITY.md)
requires no production change. The independent twelve-actual repeat passes both
quiet guards at 0.991x, conservative upper 1.003x, certifying this fixed pair.
No new image, shape or profile sweep is added; broader coverage remains open.
