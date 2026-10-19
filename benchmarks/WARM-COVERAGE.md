# Material warm coverage

Updated 2026-10-19. The **warm single-CPU parity objective remains open**:
70 independent passes,160 measured mean gaps and one confidence-open case across
231 numeric rows. The [three-table summary](../docs/benchmark.md)
contains126 independent observations and105 qualified shorts.23named required
boundaries remain unmeasured. Reuse unchanged passing paths; no image/settings grid.

The finite scope is now **254 cases:231 numeric+23 unmeasured**. The existing
227 rows retain their identified performance-qualified builds; those earlier
codec/edit rows do not yet requalify 0.2.0 admission, disposal or governed storage.
[Lanczos3 functional qualification](../docs/lanczos3-qualification.md) adds four
required representatives using the same513×259 asymmetric RGBA rasters. Their
[four-tap byte screens](WARM-LANCZOS3-BYTE-FOUR-TAP.md) improve reduction16.7% and
enlargement2.7%; word rows reuse their unchanged
[four-tap build](WARM-LANCZOS3-WORD-FOUR-TAP.md). All remain gaps:

| Named case | Fixed output and reason | State |
| --- | --- | --- |
| LanczosReduce8 | 256×129; widened radius-three antialias support and byte output | Gap1.965x/upper2.193x |
| LanczosReduce16 | 256×129; full word precision and alpha normalization | Gap1.910x/upper2.223x |
| LanczosEnlarge8 | 1027×519; negative-lobe enlargement, byte saturation and tiny alpha | Gap1.437x/upper1.523x |
| LanczosEnlarge16 | 1027×519; full word precision, overshoot and tiny-alpha normalization | Gap1.746x/upper1.852x |

The clamped halo/active-region and premultiplied float-output reference is now
qualified for all four, including normal/scalar checks and twelve rejection
controls each. All normalization, ownership and disposal are timed with one CPU.
No other rows, gates or performance evidence are replaced. Aspect-fit uses the
same kernel after small geometry work; add a separate timing case only if that
boundary is material. Four-tap byte horizontal groups use registers only,
with4448tests/47scalar and47AVX512F-off checks, the same actual private package
consumer/examples passing on both OS. Only two byte rows refresh; word dispatch
does not enter the changed byte helper. Other arithmetic/owners/boundaries remain
unchanged. Earlier3.9% word-product and2.1% slower row-storage trials stay parked/
rejected. A current Reduce8 diagnostic isolates horizontal filtering at46.4% of
CPU samples. Next: packed coefficient loads/register permutations, with exact
lane, memory/work/cancellation proofs before code. No checklist expansion.

A bounded audit of the accepted 0.2.0 additions identifies exactly three required
paths absent from the older checklist; these are not a shape/depth/settings grid:

- `TextLayout/OpenTypeArabicMarksDigits`: reuse the fixed `ArabicMarksDigits`
  text/settings and licensed regular face from the twelve functional comparisons.
  Arabic joining, bidi order, GSUB and mark positioning are distinct from the
  three original legacy outline-layout controls below. Complete owned layout,
  measurement and disposal; immutable font preparation excluded.
- `TextFit/OpenTypeConstrained`: reuse `ConstrainedFit`, 96×30, maximum24,
  minimum8, step.25 and the same face. The descending search performs30 candidate
  layouts and selects16.75; cumulative work/admission and rejected-owner cleanup
  are a new public boundary, absent from single-layout measurements.
- `MemoryBudget/Png8Retained2`: two sequential decodes of the existing resident
  PNG8 packet, one borrowed shared test-adapter root, with the first output alive
  until the second completes. Include both complete admissions, materializations
  and disposals. Governed uncached storage and independent retained reservations
  differ from default pooling and the earlier `ImageBudget` quota.

All three remain **unmeasured**; establish equivalent native font/admission
adapters and quality/ownership gates before timing. They add no new image or
font/settings grid. These additions keep the full objective explicit while the
demonstrated Lanczos3 gaps take priority. Further additions require evidence of
a distinct omitted required algorithm or public boundary.

[Exact SIMD JPEG410](WARM-JPEG-410-SIMD.md) reduces complete time76.7%,
3427.72→799.34us:1.669x/upper1.877x,720,810B/op. Exact1/32 chroma fractions,
binary64 matrix/final rounding and scalar borders/tails/fallback preserve pixels.
Native maximum1/RMSE0.136/alpha and complete timed ownership stay unchanged.
Only410 and the411 H4 selector control refresh;225other rows and70passes reuse
evidence. Full4212tests/99HWoff per OS and the SAME actual private Windows package
consumer/examples pass on both OS. No profile/tool/worker/selector/fixture rebuild,
long above one or image/settings grid. Scope247=227numeric+20unmeasured remains
70passes/156gaps/one confidence-open. Largest current gap:CMYK2D sparse1.812x,
with its prefix trial still rejected. Next named missing boundary:async PNG Identify.

[Exact SIMD JPEG411](WARM-JPEG-411-SIMD.md) cuts complete time82.6%,
3527.46→613.09us, reducing7.771x to1.352x/upper1.587x,751,279B/op. Same original
packet and exact managed/native pixels; unchanged centered reference and complete
owned-output operation. One exact integer-eighth/SIMD/binary64 row primitive,
scalar borders/tails/fallback, no work/admission/pool change. Only411 and two
EOI selector rejection controls refresh;223other rows/70passes reuse evidence.
Full4210tests/97HWoff checks per OS and the SAME actual Windows private package
consumer/examples pass on both OS. No long above one, new profile or settings grid.
Scope247=226numeric+21unmeasured stays70passes/155gaps/one confidence-open.
Largest current gap:CMYK2D sparse1.812x; its prefix trial remains rejected.
Next named missing case:JPEG410 H4V2; establish native equivalence before timing.

[Exact SIMD caller YCCK RGB](WARM-JPEG-YCCK-CALLER-SIMD.md) reduces time80.2%,
4532.59→898.47us:1.488x/upper1.550x,1,337,490B/op. Shared ownedYCCK improves20.3%
to1.163x; callerCMYK control1.592x. These three rows refresh after the caller
selector/shared color body changed. All222others and70passing proofs retain evidence.
Full4209tests/96hardware-off checks per OS and the SAME actual private Windows
package consumer/examples pass on both OS. Same packet/native/CLI/harness/selectors,
exact pixels/approved bounds/owners/work; no new image/settings grid or profile.
Scope247=225numeric+22unmeasured,70passes/154mean gaps/one confidence-open.
Current largest gap:CMYK2D sparse1.812x; its prefix trial remains rejected.

[The first caller YCCK420 screen](WARM-JPEG-YCCK-CALLER-RGB.md) demonstrated the largest
gap:7.504x/upper7.804x,4532.59/604.01us,1,337,431 B/op. The already-named retained
caller matrix/black algorithm uses the existing packet/reference, with exact managed
pixels, approved native bounds and equivalent fresh caller ownership. Direct native
RGB24 matches the old native RGBA colors exactly. No new case/image/settings grid.
Production/all224previous rows/full4208/95HWoff/package proof are reused. One short
and one6sec profile; no long above one. Scope247=225numeric+22unmeasured,70passes/
154mean gaps/one confidence-open. The next primitive targets scalar caller rows.

[Four-component materialization](WARM-JPEG-FOUR-COMPONENT-PLANES.md) reduces caller
CMYK time 47.7%, 1010.51→528.07us: 1.590x/upper1.736x, 1,635,475 B/op. Owned
YCCK improves 9.0% to 1.463x. Caller RGB420 independently reconfirms at 0.962x/
upper0.966x; gray remains a 1.345x short gap. Exactly these four numeric rows
refresh after the common materializer selection; all 220 others and 69 unaffected
passes retain evidence. Current totals stay 224 numeric/23 unmeasured, 70 passes/
153 mean gaps/one confidence-open. Reuse frozen workers/fixtures/harness/selectors;
one immutable producer per OS passes 4208 tests, 95 hardware-off checks and the same
actual private package consumer/examples on both OS. Full objective remains open.
Largest measured gap: CMYK 2D sparse 1.812x; its prefix trial stays rejected.

[The preceding caller CMYK packing round](WARM-JPEG-CMYK-PACKING.md) cut elapsed time64.1%,
2817.13→1010.51us:3.057x/upper3.169x,1,635,459B/op. Exact four-channel samples,
work/admission/cancellation, source/padding and independent owners remain.
Only that numeric row changes;223others and70passing proofs are reused. Full
4208tests/OS,72hardware-off checks/OS and the same actual Windows private package
consumer/all three examples pass on both OS. Production/harness/native/selector
builds during the bounded experiment:one/zero/zero/zero; no repeated profile.
Current finite scope247=224numeric+23unmeasured. The new named control below
documents one retained required algorithm omitted from the earlier checklist,
with the existing packet/reference, not an image/settings grid. Parity stays open.

[The initial caller CMYK measurement](WARM-JPEG-CALLER-CMYK.md) demonstrated the largest
gap:8.522x/upper8.986x,2817.13/330.56us,1,635,373B/op. All four canonical ink
channels and normal/scalar public CLI are exact, with varying black preserved.
Complete fresh caller ownership/decode/packing/release are timed on one CPU.
Untagged original projection, no RGBA/header patch/new image/settings grid.
All223previous rows and full4202/66HWoff/package proof are reused; production
unchanged. One named pending boundary becomes numeric, scope246=224+22.
One6sec profile selects scalar sample packing as the next substantial primitive.

[Exact SIMD YCCK420 rows](WARM-JPEG-YCCK-SIMD.md) reduce time76.1%,
4,761.24→1,135.97us:1.604x/upper1.636x,1,470,246B/op. Exact original pixels,
approved native-only transition16/interior4/RMSE1.5, source/owners/work remain.
Four shared-dispatch CMYK gaps refresh at1.391x/1.732x/1.113x/1.812x.
These five rows replace their prior measurements; all218other rows and70passing
proofs are reused. One immutable producer/OS; frozen harness/native/selector/
fixtures/CLI unchanged. All4,202tests/OS,66affected hardware-off checks/OS and
the same actual Windows private package consumer/examples pass on both OS.
Scope246=223numeric+23pending, unchanged. CMYK2D sparse1.812x is largest;
its bounded prefix trial remains rejected. No new image/settings grid.

[The bounded sparse-prefix trial](WARM-JPEG-SPARSE-PREFIX-TRIAL.md) was rejected:
3.1% mean gain still left1.75x. The exact accepted source and producers are
restored on both OS; all existing rows/qualification are reused. One current
6sec profile and one3actual screen, no new case or full package rerun. The later YCCK420 measurement and SIMD round supersede that next step.

[Identical horizontal JPEG rows](WARM-JPEG-HORIZONTAL-ROW.md) improve the one-AC
CMYK mean10.0%,746.37→671.44us, at1.726x. DC/general remain1.395x/1.126x,
about1.086MB/op. The new `JpegDecode/CmykAdobeSparse2DResident` selects the retained
2D sparse algorithm after horizontal-row rejection. This is one concrete omitted
algorithm: firsttwo AC=4 at(1,0)/(0,1), SAME projection/geometry/unit quantization.
Analytical pixels and original native gates pass; its first short measures1.819x.
No new shape/depth/filter/quality grid. Scope246=222numeric+24pending. All4,197tests/
OS,61affected hardware-off checks/OS and same actual Windows private package
consumer/all three examples pass on both OS. Three rows refreshed/one added;
all70unaffected passing proofs stay stopped. One immutable producer/OS, one new
control-harness build/OS, original native/selector/three fixtures reused; all12
actuals/guards/brackets retained. No long confirmations above one or new profile.

[Direct caller RGB420 sample writing](WARM-JPEG-CALLER-RGB420.md) removes the
largest3.369x gap:71.3% less elapsed time, independently0.966x/upper0.971x,
624,508B/op, exact398,601-byte caller RGB24. Gray's new rejection dispatch is
refreshed at1.326x/273,422B/op and remains a short gap. Precisely these two rows
change; all69existing passes stay valid and the new RGB pass stops. Exact pixels,
normal/scalar native gates, padding/source, limits/cancellation and independent
survivors pass. All4,176tests/OS,73affected hardware-off checks/OS and same actual
private Windows package consumer/all three examples pass on both OS. Scope stays
That round retained218numeric/25unmeasured; the CMYK row below makes219/24.
Next largest measured gap is RGBA8 128x64 tight-memory bicubic1.687x.

[Caller-RGB MCU preparation](WARM-JPEG-CALLER-MCU.md) improves the largest known
gap33.7%,3,162.12→2,097.34µs, retaining exact74,040/52,093-byte matched-quality
packets. Current RGB short1.328x/upper1.408,297,894B/op; gray control1.162x,
155,007B/op. Precisely the two caller-sample rows are replaced, no new case.
Safe admitted384-byte MCU preparation shares packed RGB loads/channel extraction;
old scalar/tight/callback/cancelable/non420 samplers remain. Source/precision/work/
owners and normal/scalar native gates pass. All4,169tests/OS,109affected hardware-off
checks/OS and same actual private package consumer/all3examples pass on both OS.
RGBA/shared-budget JPEG encoding and all other executed paths retain evidence;
all69passing cases stop until affected. That round retained216numeric/27pending; the caller-gray/RGB rows below make218/25.

[Input reconciliation](WARM-INPUT-RECONCILIATION.md) refreshes the remaining22
prior rows after shared JPEG parsing and final encoded-input copy removal.
Fifteen JPEG pixel decodes remain1.118–1.546x short gaps; JPEG Identify independently
passes0.488x/upper0.494,88B/op. Of six PNG/WebP Stream cases, RGBA16 PNG independently
passes0.988x/upper0.991,8,648B/op; the other five remain1.139–1.562x short gaps.
The preceding [baseline grayscale result](WARM-JPEG-GRAY-CURRENT.md) remains current
and is reused. All90actuals/guards/quality/ownership checks remain. Exactly the same
qualified immutable producer/harness, native workers, fixtures and timing drivers
are reused; missing selectors were restored from exact retained archive members.
Only the21 changed-path metadata controls were replayed; no production/harness/
native/selector build, source synchronization, broad audit or package recheck.
Full4,167tests/OS,183affected intrinsics-disabled checks/OS and same actual package
consumer/all3examples on both OS remain valid. Stop the two newly passing paths.

[Bounded VP8 coefficient adaptation](WARM-VP8-ADAPTIVE-PROBABILITIES.md) changes
exactly six existing default complete encode timings/packet sizes and adds
`WarmWebp/EncodeBudget8`: the existing alpha8 raster with MaxWork32,000,000,
which declines optional adaptation and exercises the original fixed-model encoder.
This distinct required caller-budget algorithm, not a new image/settings grid,
has independent passing evidence and byte-exact old output. Opaque bytes shrink
54.5%, allocations41.6%, time7.9–9.8%; both opaque cases remain1.320–1.323x short
gaps. Alpha bytes shrink39.2%, time8.5–8.8%. Photograph/diagram packets shrink
1.8%/7.5%, with1.6%/1.7% higher times. Diagram independently measures1.000208x/
upper1.009341x, superseding its old pass. It remains open; no repeated unchanged
measurement to force a pass. The other three default passing cases plus budget
fallback independently pass. All81 actuals, guards, quality/ownership gates and
identities are retained. Exactly4,167tests/OS,183affected intrinsics-disabled
checks/OS and the same actual private package consumer/all3examples pass on
both OS. Five lossless proofs and other unaffected paths remain current. Stop
passing cases. Compression efficiency is improved; full parity remains open.

[Previous-row references](WARM-VP8L-PREVIOUS-ROW.md) reduce the diagram's VP8L
size31,072→15,120B (51.3%) and allocation67,720→34,928B/op (48.4%). The photograph
shrinks385,854→383,154B (0.7%). The changed lossless encoder invalidates precisely
the three existing WarmWebpLossless encodes and two new WarmCompression lossless
cases. All five have fresh independent passing evidence, conservative uppers
0.0816/0.0995/0.1316/0.1132/0.2838. Stop them until affected. Additional matching
work raises their prior-round mean times about40–55%; the tradeoff is explicit,
with all five still faster than their reference. No extra workspace/frame copy.

All4,161 tests/OS,30 affected hardware-off checks/OS and the same actual Windows
private package consumer/all three examples on Windows/Linux pass. Other codecs,
decoding/Identify, editing, drawing and text production paths are unchanged.
Existing literal/run decoder input packets also stay byte-identical. Lossy WebP
packets reproduce frozen anchors exactly; no quality retuning or timing repeat.
Shared/async lossless paths remain already named/unmeasured. No case/settings
grid, native/harness/timing-driver rebuild or broad qualification repeat for
unaffected production. Historical [opaque alpha coding](WARM-VP8L-OPAQUE-ALPHA.md)
and its qualification remain archived. Counts are unchanged by this replacement.

The [four content comparisons](WARM-WEBP-COMPRESSION.md) add exactly two fixed
513×259 opaque RGBA8 inputs, with provenance in [their manifest](Fixtures/Compression/manifest.json):
`WarmCompression/PhotographLossless8` covers photographic texture and lossless
size, `DiagramLossless8` covers repeated flat regions and antialiased text,
`PhotographLossy8` covers photographic rate/distortion, and `DiagramLossy8` covers
text edges and flat-region rate/distortion. These distinct required content
families were absent from the procedural controls. No shape/depth/effort grid.
All four measure complete output ownership and allocations with equivalent
delivery. The two lossless cases and photograph VP8 independently pass; the later
bounded-model diagram result is the open comparison above. Lossless sizes are now 2.394x/2.090x the exact native packets; VP8
sizes are 1.212x/1.167x at matched source quality. The preceding lossless change replaces only the five affected timing proofs;
other production paths and qualification evidence are reused. Known scope is now247 named
cases:225 numeric plus22 unmeasured. Stop these three time-passing cases unless
their relevant production path changes; compression-size work remains required.

The two [shared-budget codec boundaries](WARM-SHARED-BUDGET.md) are now measured:
`SharedBudgetDecode/PngRgba8Resident` reuses `WarmPng/Decode8`; complete public
budgeted decode and retained-owner disposal measure1.439x,4,561B/op.
`SharedBudgetEncode/Jpeg420Rgba8` reuses `WarmJpeg/Encode420_8`; complete output/
work settlement measures1.145x,297,781B/op, unchanged74,040/52,093-byte packets.
Both are qualified short gaps, not passing confirmations. One finite long-lived
caller budget and source/input reservation are prepared outside timing; original
per-operation limits remain. Two live owners/packets, exact retained charges,
surviving outputs, source stability and temporary-memory/work settlement pass.
No new case/grid, native/production/selector build or repeated full package proof.
Only benchmark harness inputs changed; existing executed production evidence stays
valid.23new boundaries remain unmeasured. The original unbudgeted paths are reused.

Direct JPEG sample decoding has two named representatives, both currently
measured in [the RGB420 writing round](WARM-JPEG-CALLER-RGB420.md).
`JpegSamplesDecode/GrayscaleResident` reuses `WarmJpegDecode/OwnedDecodeGray`:
new rejection-dispatch control1.326x/upper1.445x,273,422B/op,132,867-byte GRAY8.
`JpegSamplesDecode/Rgb420Resident` reuses `WarmJpegDecode/NativeDecode420`:
independent0.966x/upper0.971x,624,508B/op,398,601-byte RGB24. Stop this new pass.
Both include fresh caller destination acquisition, complete decode/normalization
and independent output lifetime with equivalent native delivery. RGB additionally
includes native centered2x2 chroma expansion/crop and BT.601 packing. There is no
RGBA frame. Original input/settings/native workers and JPEG quality gates stay
fixed. Exact qualified managed pixels, native CLI delivery, normal/scalar gates,
padding/source and independent surviving owners pass. The new SIMD packing
primitive changes precisely RGB and gray dispatch timings; older other evidence
is retained. Full cross-platform/differential/package qualification is recorded.
JPEG sample decoding also supports CMYK, as described below.

Direct JPEG sample encoding adds two now **screened** representatives:
`JpegSamplesEncode/GrayscaleResident` uses the fixed grayscale projection and the
approved SIMD-enabled libjpeg-turbo reference; `JpegSamplesEncode/Rgb420Resident`
uses the fixed opaque RGB projection and FFmpeg's 4:2:0 reference. Retain existing
quality/size matching, with equivalent raw sample delivery, complete row
normalization, streaming and independent encoded output ownership. The cases
cover single-component samples and packed RGB conversion, without a settings grid.
Both direct-sample decode and shared-budget boundaries above have qualified shorts. The [current sample screens](WARM-JPEG-CALLER-MCU.md) measure gray1.162x/upper1.422x,
155,007B/op and RGB4201.328x/upper1.408x,297,894B/op, after the33.7% RGB MCU win. Both are mean gaps, so no long
confirmation follows. Native raw-input adapters use the same pinned libraries
and exact frozen packets; each operation includes necessary normalization/coding
and fresh independent output ownership. Production and H4 qualification are reused.

The [current direct-sample quality check](warm-jpeg-sample-quality-evidence.json)
reproduces both frozen managed packets exactly, using the same original gray/RGB
projections. Gray:58,823/58,910 bytes and PSNR gap0.000033dB; RGB420:74,040/52,093
bytes and gap0.106110dB. Both pass the existing0.25dB gate, decoder agreement,
source preservation and independent-output checks. This proves reusable quality
and size anchors. The new raw-input screens supply the distinct time/allocation
evidence; existing RGBA operation timings were not substituted for these paths.

JPEG compatibility changes shared frame/header parsing (one/three/four components),
scan setup (four component slots), sampling validation (horizontal ratios up to
four), Adobe marker handling and final decode dispatch. These changes originally invalidated **all17 existing
JPEG decode/inspection rows**, including the
fresh-Stream restart case and resident Identify. Their old numbers are historical; the current input reconciliation above
replaces all17, without treating an unchanged IDCT kernel as complete-operation proof. Entropy/IDCT kernels are unchanged, but that alone does not prove the
complete operation unchanged. JPEG encode, PNG/WebP and editing evidence is reused.
All17 affected rows now have current evidence in the reconciliation above.

Synchronous Stream decode now leases its private input instead of making a final
encoded-input copy, admitting actual retained capacity through decoding. This
originally invalidated exactly seven existing complete operations: `Png8`, `Png16`,
`PngCropFit8`, `PngCropFit16` (WarmStreamDecode), `StreamNative8` (WarmVp8Decode),
`StreamNativeLossless8` (WarmWebpDecode), and `StreamDecode420Restart` (WarmJpegDecode).
The last was already among the seventeen prior JPEG rows; six additional rows
became prior production, for23 total. All23 are now current, including the
preceding grayscale result; historical allocation counts are superseded.
No new workload is needed. Resident codec paths, synchronous Identify, async input,
encoding and editing remain unchanged. No unaffected operation was retimed.

Five representatives cover the required JPEG compatibility algorithms; all five have measured current gaps; no JPEG compatibility representative remains unmeasured:

- `JpegDecode/CmykAdobeResident` (**measured**1.391x DC gap): four component planes, Adobe inversion and
  complete owned RGBA conversion from an original CMYK projection. The
  separately named horizontal, 2D sparse and all63-AC controls above cover each
  retained transform algorithm.

- `JpegDecode/Ycck420Resident` (**measured**1.163x gap): subsampled color
  reconstruction, explicit matrix and full-resolution black, original varying input.
- `JpegSamplesDecode/CmykResident` (**measured**1.592x gap): untagged full-resolution
  canonical ink, all4raw native planes preserved and packed to fresh caller storage.
  Adobe inversion has correctness coverage; no RGBA intermediate.
- `JpegDecode/411Resident`: [measured1.327x control gap](WARM-JPEG-410-SIMD.md); quarter-width horizontal chroma reconstruction on the
  existing RGB projection; native sample-grid normalization and complete ownership verified.
- `JpegDecode/410Resident`: [measured1.669x gap](WARM-JPEG-410-SIMD.md); quarter-width, half-height chroma
  (JPEG factors4×2, not video4×4). Original native samples and centered normalization
  verified; complete owned-output boundary measured.

The accepted full-resolution CMYK caller selector exposes one concrete omitted
required path: `JpegSamplesDecode/Ycck420RgbResident` (**measured**1.488x gap). SourceYcck
and outputRgb reject raw byte packing; the retained WriteCmykSamples path performs
centered chroma expansion, binary64 matrix/inversion/black and directRGB24 stores.
Neither owned-RGBA YCCK SIMD nor caller YCbCr420 RGB measures that complete public
boundary. Reuse the existing YCCK420 packet and analytical RGB; establish equivalent
fresh native RGB24 ownership/normalization and preserve the approved fixed-fixture
quality bound before timing. This single additional case changes246to247; no
new image, shape, depth, filter, quality or sampling grid. Other sampling/orientation
combinations retain correctness coverage, without a Cartesian timing expansion.

Baseline/progressive analytical and live FFmpeg compatibility checks cover these
layouts; they are not performance confirmations. Reuse the existing source
projections and fixed settings. The current finite total is stated at the top;
the named async boundaries below are already included. Do not add a
shape, depth or quality grid. Add cases only for a distinct required path omitted
from this list, with its concrete reason.

Async input adds exactly two **unmeasured** common front-end boundaries:
`AsyncStreamIdentify/PngRgba8FreshNonSeekable` and
`AsyncStreamDecode/PngRgba8FreshNonSeekable`. Both reuse the existing PNG8 input
and full structural/pixel gates, adding actual async reads through EOF, fresh
private input acquisition/growth/release and independent descriptor/pixel owners.
They cover the shared async acquisition path and its two different complete
public operations. Shared-budget awaits have correctness coverage; no new
codec/depth/stream scheduling grid is added. Resolve equivalent native input
delivery before timing. These cases are already included in the finite total.
Original synchronous methods and codec kernels are unchanged by async acquisition;
the later synchronous input-copy change's23affected rows now have current evidence.

Async output adds four **unmeasured** representatives, all on existing inputs:

- `AsyncStreamEncode/PngRgba8FreshNonSeekable`: batched chunk staging, complete
  encode, asynchronous delivery and independent caller-owned output.
- `AsyncStreamEncode/JpegRgba8FreshNonSeekable`: the distinct stock-memory adaptive
  admission probe and bounded contiguous staging for the existing RGBA8 anchor.
- `AsyncStreamEncode/JpegRgb420Samples`: direct borrowed RGB memory and the separate
  sample/adaptive admission boundary, reusing the fixed RGB420 projection.
- `AsyncStreamEncode/WebpLosslessRgba8FreshNonSeekable`: byte-at-a-time bit-writer
  emission into staging, distinct from PNG's batched writes, with exact hidden RGB.

These cover distinct new staging/admission/delivery paths, without a codec,
sampling, depth or scheduling grid. Resolve equivalent native destination
acquisition/copies and output lifetime before timing. Shared-await correctness
is separately qualified. These four cases are already included in the finite total.
Original encoder bodies/state/codec kernels are unchanged. Later synchronous input
acquisition changes affect only the separately reconciled operations above.

Fixed-workspace header inspection adds three **unmeasured** representatives using
the existing encoded inputs, each for a distinct required container grammar:

- `InspectHeaders/PngRgba8FreshSeekable`: chunk framing, dimensions/EXIF and skipped
  opaque IDAT, including source acquisition and EOF; excludes PNG CRC/pixel checks.
- `InspectHeaders/Jpeg420FreshStream`: full marker/stuffing/restart scan through
  entropy and late metadata to EOF, without entropy-symbol decoding.
- `InspectHeaders/WebpLosslessRgba8FreshSeekable`: RIFF framing, VP8L dimensions,
  feature flags and metadata following skipped image data, through EOF.

Use equivalent native header validation, input delivery and output ownership before
timing; full `Identify` timings cannot substitute for these cheaper boundaries.
Async scheduling/shared reservation variants have correctness coverage, without
another timing grid. The new inspector uses independent methods and types; existing
successful method bodies, state layouts and numeric evidence are unchanged by the
inspector. The separate synchronous decode acquisition change is accounted above.

Drawing adds six **unmeasured** cases on the existing RGBA8/16 rasters:

- `FillRectangle/Rgba8Clipped` and `FillRectangle/Rgba16Clipped`: distinct byte/word
  store loops and color precision; one fixed clipped rectangle per existing raster.
- `CopyRegion/Rgba8Independent`: two caller-owned buffers and exact row/chunk copy.
- `CopyRegion/Rgba16Overlapping`: one owner, reverse traversal and overlap-safe
  word-sample copying without a snapshot raster.
- `Composite/Rgba8StraightAlpha`: complete clipped source-over on two byte owners.
- `Composite/Rgba16Overlapping`: full-width rational arithmetic, word samples and
  overlap-safe self-compositing with one owner.

Reuse original 513×259 rasters and fixed 257×129 fill/copy regions; placement (-3,5)
clips fill/independent copy/composite, with overlap shifted (1,1). No image/shape/
alpha grid. Resolve equivalent native in-place canvas delivery, state restoration,
rounding and ownership before timing. Exact BigInteger and bounded native profile
correctness checks are not performance confirmations. Existing paths/statuses are
unchanged: 190 current rows, 23 prior rows. Including text below, there are 29
named unmeasured boundaries.

Text layout/measurement adds three **unmeasured** complete operations on original
generated fonts, each exercising a distinct required outline/mapping path:

- `TextLayout/OriginalUnicode`: cmap12, long loca, ordinary coordinate words,
  quadratic extrema and a basic compound accent.
- `TextLayout/PackedBmp`: cmap4, short loca, packed/repeated flags, implied
  quadratic points and scaled component offsets.
- `TextLayout/CompositeTransforms`: two-by-two matrices, point attachment and
  inherited component metrics.

Reuse the three tiny `--font-layout` controls and one fixed string
`AV g Á\u0301 \uE000\nAV\tg AV`, size16, wrap64, line spacing1.25. Prepare the
same immutable face outside timing on both sides; include complete layout,
needed outline decoding, measurement, positioned values and independent output
ownership. Resolve equivalent FFmpeg/font-backend scope before timing. The
15-glyph native geometry check is correctness evidence only. No text timing or
allocation claim, font/style/size grid, rasterization case or changed numeric
status follows from this addition.

Rendering adds `TextRender/Rgba8Clipped` and `TextRender/Rgba16Clipped`, exercising
distinct byte/word canvas sample writes and precision. Reuse the original font,
fixed text/settings above, existing 513×259 canvases and position(-.25,1.5).
Only immutable font preparation is outside timing. Include layout/needed geometry,
coverage and complete in-place drawing with equivalent native canvas delivery,
state restoration and output ownership; do not substitute a cached rendered mask.
Both time and allocations are **unmeasured**. Analytical quadratic coverage checks
and the separately recorded default FreeType raster differences are development
correctness evidence; equivalent native quality/scope still needs resolution before
timing. No existing native pixel allowance or numeric evidence changes.

Latest [wider RGBA8 horizontal reversal](WARM-HORIZONTAL-FLIP512.md) improves
single-output flip **22.0%**, passing parity at **0.950× / upper 0.965×**. Two live
outputs improve **27.9%** but remain a qualified **1.053×** gap. Both retain
**128 B/image**, exact pixels, hidden RGB and independent owners. The RGBA16
passing kernel and original fallbacks remain unchanged; no new allocation or
resource/work charge. At that round, other 205 comparisons/all 63 prior passing
proofs remained valid; the JPEG rows above now require reconfirmation.
All 3,739 Release tests/OS, 67 affected public hardware-off checks/OS and the same
actual private package consumer/all three examples pass on Windows/Linux.
The newly passing single-owner path stops; no width/unroll/inlining grid follows.

Earlier [partial PNG transparency](WARM-PNG-PARTIAL-ALPHA.md) retains its 33.3%
gain and passing proof; fixed JPEG fields, packed bicubic reduction and PNG/JPEG
reconstruction retain their measured gains. Those executed paths remain unchanged.

The finite checklist's **previous <=2x phase was completed for released0.1.0**.
Its original independent evidence and unchanged-path proofs remain historical;
subsequent production changes identify exactly which current rows they supersede.
Only independent upper<=1 paths are stopped for the present objective. The coverage
items below name required algorithms and public boundaries; links marked closed
refer to that previous phase. Current results and allocations are in the summary.

Scope is the existing two original513x259 RGBA8/16 rasters, documented projections
and the named fixed selectors in the linked evidence/renderer. Required still
PNG/JPEG/WebP codecs,8/16-bit edits, orientation/crop/nearest/bilinear/bicubic,
resident/fresh-Stream and existing cancellation/limits/owner boundaries are
represented. Cases were chosen for distinct material algorithms or public
boundaries, not a Cartesian grid of settings. The61 bicubic selectors were already
established before this audit and were reconciled, not expanded.

| Item | Named fixed scope and reason | Current evidence / state |
| --- | --- | --- |
| B01 | Bilinear Down8/16,Up8/16: shared resize dispatch and byte/word alpha-aware filtering | [Previous phase closed](WARM-CURRENT-BILINEAR.md); [Down8](WARM-BILINEAR-BYTE-ROW-CACHE.md), [Down16](WARM-BILINEAR-WORD-CACHE.md) and [Up8](WARM-BILINEAR-BYTE-FACTORIZATION.md) improved, still <=1x gaps |
| B02 | JPEG adaptive Encode8/16,Encode422_8/16: changed AC planner at444/422 and both input depths | [All four closed](WARM-CURRENT-JPEG-ADAPTIVE.md) |
| B03 | JPEG Decode8,NativeDecode8,NativeDecode420/422,OwnedDecodeGray,NativeDecode420Restart,DirectRgb422/420: changed ordinary AC loop plus distinct component renderers | [All eight closed](WARM-JPEG-GRAY-CURRENT.md) |
| B04 | JPEG ProgressiveGray/420/422 plus ordinary/live-token444: masked refinement and its guarded fused readers | [Four ordinary proofs reused](WARM-JPEG-REFINEMENT-FUSED.md); [live-token444 improved again](WARM-JPEG-REFINEMENT-LOOKAHEAD.md), still a <=1x gap |
| B05 | The25 named PNG decode selectors in the linked report: byte/word predictors, expansion and input-pool changes | [All25 closed](WARM-CURRENT-PNG-DECODE.md) |
| B06 | PNG DecodeAdam7_16,DecodeGray1Adam7: interlaced placement on changed shared framing; later palette4/8 cases reuse their own proofs | [Closed](WARM-CURRENT-PNG-ADAM7.md) |
| B07 | Png8/16,PngCropFit8/16: complete fresh input, orientation/crop/fit and intermediate ownership | [Closed](WARM-CURRENT-PNG-STREAM.md), with word fusion proof reused |
| B08 | PNG Encode8/16: changed RGBA packing, exact samples and fixed IDAT-size policy | [Both closed](WARM-CURRENT-PNG-ENCODE.md) |
| B09 | VP8 Encode8/16 with alpha: changed output writer and word normalization | [Both closed](WARM-CURRENT-VP8-ENCODE.md) |
| B10 | VP8L Lossless8 literal/run input: changed entropy/run path | [Closed](WARM-CURRENT-VP8L-LITERAL.md) |
| B11 | Clone,crop,normal orientation,horizontal/vertical flip,half turn,CW/CCW,transpose,transverse at both depths: shared output-pool acquisition | [All20 closed](WARM-CURRENT-EXACT-EDITS.md); retained-two flip unchanged |
| B12 | The61 named bicubic selectors: changed dispatch/helper/guard paths, default/tight scratch, byte/word and established support widths | [Previous phase closed](WARM-CURRENT-BICUBIC.md); [word](WARM-BICUBIC-PAIRED-COLUMNS.md) and [byte](WARM-BICUBIC-BYTE-PAIRED.md) paired columns plus [byte strips](WARM-BICUBIC-BYTE-STRIPS.md) and [byte](WARM-BICUBIC-WEIGHT-BROADCAST.md) and [word](WARM-BICUBIC-WORD-BROADCAST.md) packed weights improved, still <=1x gaps |
| B13 | Existing PNG stored/Paeth/predictor,RGB/gray/gray-alpha8/16,packed gray1/2/4,palette1/2/4/8,tRNS,Adam7 and frozen compression-mode rows; four named exact-limit RGBA8/16 ordinary/live-token boundaries | [Up8/16](WARM-PNG-UP-SIMD.md), [RGBA8 Sub](WARM-PNG-SUB-PREFIX.md), [stored RGBA16 encode](WARM-PNG-STORED-CAPACITY.md) and [RGB16 Paeth](WARM-PAETH-SIX-CHANNEL.md) parity confirmed; [tight RGBA streaming](WARM-PNG-STREAMING-ROWS.md) improves four gaps; other applicable proofs reused; precise selectors/allocations in the [summary](../docs/benchmark.md) |
| B14 | JPEG4208/16,restart,gray and geometric live-token/forward output; VP8 ordinary/simple/alpha/live-token/Stream and opaque8/16 encode; VP8L native/indexed/live-token/Stream and exact backend encodes | [VP8 source views](WARM-VP8-SOURCE-VIEWS.md) improve four encodes; [fixed JPEG encodes](WARM-JPEG-FIXED-AC-FUSION.md) and other unchanged proofs reused |
| B15 | Nearest Down8/16,Up8/16: unchanged early dispatch/integer kernels and allocation path | Existing independent proofs reused |
| C01 | PNG resident Identify: full container/CRC/EXIF without inflation, fresh descriptor; decode/cold timings do not cover it | [Closed](WARM-PNG-IDENTIFY.md),1.234x/upper1.238x,88B/op |
| C02 | JPEG resident Identify: complete marker/scan/stuffing/EXIF/EOI scan without entropy/pixels | [Prior-production confirmation](WARM-JPEG-ENTROPY.md),0.499x/upper0.506x,88B/op; reconfirm after four-component header support |
| C03 | WebP resident Identify: distinct RIFF/feature/header/EXIF algorithm, one existing VP8L packet | [Closed](WARM-WEBP-IDENTIFY.md),0.0040x/upper0.0045x,88B/op,explicit FFmpeg parser reference |
| C04 | Fresh non-exposed seekable Stream Identify on the same C01 PNG: fast metadata boundary exposes input acquisition/read/EOF/ownership costs | [Closed](WARM-PNG-STREAM-IDENTIFY.md),1.445x/upper1.459x,184B/op |

Historical <=2x audit gaps were resolved: stored16 PNG2.219x→1.090x, wide-cache
bicubic2.121x→passing current-path evidence, PNG Identify2.797x→1.234x,
JPEG Identify33.725x→1.840x, and Stream Identify7.863x→1.445x. Each production
round retains correctness/control evidence, short mean before confirmation,
all actuals/quiet guards and full accepted-change qualification. Raw refusals,
failed guards and superseded experiments remain in their original reports.

Historical [stored-capacity reconciliation](warm-png-stored-capacity-evidence.json)
replaces only the two storedRGBA8/16 encodes: word independent0.861x/upper0.870,
1,140,504B/op; byte1.295x/upper1.430,604,319B/op qualified short. Capacity allocation
is inside the complete fresh-output operation; packets, packing, compression,
checksums, work/working admission, writes/callbacks, limits and owners remain.
Other205comparisons/all59previouspasses are reused. Full3701+65scalar/OS and the
same actual package consumer/all3examples bothOS pass. No added case/grid.
60passes/143gaps/4confidence;145selected independent+62short. Passing word stops.

The preceding [Sub-prefix reconciliation](warm-png-sub-prefix-evidence.json) replaces
only DecodeSub8:50.5% faster,independent0.851x/upper0.855,4456B/op. Stride4 Sub
uses exact SIMD byte prefixes with unchanged resources/work,cancellation and
partial-failure behavior. Other206 comparisons/all58 previous passes are reused;
word Sub/byte Paeth control means remain within1.2%. No newcase/raster/settings
grid or profile. Full3692+92scalar/OS and the same actual package consumer/all3
examples bothOS pass.59passes/144gaps/4confidence;144selected independent+63short.

The preceding [packed-weight reconciliation](warm-bicubic-weight-broadcast-evidence.json)
replaces only Tight64x32_8 with a qualified short:12.5% less time,1.527x/upper1.797.
One exact vector-construction expression changes in the >20/≤36tap byte packed
core; all arithmetic/order, guards, workspace, work/cancellation and owners remain.
Generic join first regressed78.7%; retained JIT evidence identifies a full-width
stack dependency. Direct insert removes it, with no new profile or settings grid.
Unchanged controls retain their selected evidence; other206 rows/all58 passes
are reused. Full3680+138scalar/OS and the same actual package consumer/all3examples
bothOS pass.144selected independent+63qualified short; full parity remains open.

The preceding [live refinement reconciliation](warm-jpeg-refinement-lookahead-evidence.json)
replaces only ProgressiveCancelable444:10.1% less time, still1.552x/upper1.731.
Bounded ordinary lookahead retains original logical cursor, pending bits, per-bit
cancellation/work and immediate updates on every exit. Original fused/scalar
readers are untouched; two methods and one dispatch are added. Other206 rows and
all58 passing proofs are reused; unchanged ordinary-progressive/baseline controls
are within0.9%. Full3680+475scalar/OS and the same actual package consumer/all3
examples bothOS pass. All three short jobs retain actuals/both quiet guards on one
immutable producer/harness/selector/reference build. No new performance matrix.

The preceding [general byte-row reconciliation](warm-bicubic-byte-emitter-evidence.json)
replaces those same thirteen cache rows without new coverage. Five byte paths
improve11.6–20.8%; one dispatch/depth predicate and private name change, with the
entire emitter body unchanged. VerticalUp8's1.010x short warrants one independent
uncertainty check:1.020x/upper1.024x, a gap. Other12 remain qualified short gaps;
no unchanged repeat until pass. Other194 comparisons/all58 passing proofs retain
paths. Full3663+209scalar/OS and the same actual package consumer/all3 examples
bothOS pass. Native workers/gates and full boundaries are unchanged; no new
profile, raster, selector build or settings grid.

The preceding [byte-row reconciliation](warm-bicubic-byte-row-evidence.json) replaces
exactly thirteen existing default-cache timings, conservatively including word
and cross controls sharing the changed hot method. HorizontalUp8 improves16.4%;
all remain qualified short gaps. One suspicious vertical control is checked once
against the frozen predecessor:0.94% mean difference rather than4.14% against
older evidence. All actuals/guards retained, no repeat-until-pass. Other194
comparisons/all58 passing proofs retain their paths. Full3663 Release tests/OS,
209 scalar cache/reference checks/OS and the same actual package consumer/all3
examples bothOS pass. Original arithmetic, guards, allocation/work and approved
native policy remain. Two vertical reference identities reuse existing source
proofs without rebuilding. No new performance selector, input or settings grid.

The preceding [cancelable JPEG reconciliation](warm-jpeg-cancelable-ac-cache-evidence.json)
replaces only NativeDecodeCancelable8 with a qualified short gap after 9.3% less
managed time. One local cache method changes; prefix/amplitude work, cancellation
and partial-state semantics remain exact. Other206 comparisons/all58 passing
proofs retain their paths. Full3662 Release tests/OS,96 scalar JPEG checks/OS and
the same actual package consumer/all3 examples bothOS pass. The current approved
ordinary native444 worker is reused with original packet/output hashes and
unchanged complete boundaries; direct-RGB mode additions do not apply here.
No new performance image, selector or settings grid.

The preceding [byte bilinear reconciliation](warm-bilinear-byte-factorization-evidence.json)
replaces exactly Up8 with a qualified short gap after 16.8% less managed time.
Exact integer factorization preserves every final sum, original rounding and
admission. Only byte bilinear enlargement on both axes executes the changed
helper. Other206 comparisons/all58 passing proofs retain their paths. Full3659
Release tests/OS,132 scalar bilinear/reference checks/OS and the same actual
package consumer/all3 examples bothOS pass. Reference, quality and complete
operation boundaries are unchanged; no new performance input or settings grid.

The preceding [enlargement-strip reconciliation](warm-bicubic-enlargement-strips-evidence.json)
replaces exactly seven timings: tight both-axis/horizontal/vertical byte and word,
plus default horizontal word. All are qualified short gaps after8.3–17.8% less
managed time. Only existing strip capacities change; reversing them recovers both
entire predecessor files. Fixed arrays4000B remain within4096B admission; all
arithmetic, work, source/owner and exact-pixel/approved native gates are unchanged.
All200 other comparisons/all58 passing proofs retain their paths. Full3656
Release tests/OS,163 scalar resize controls/OS and the same actual package
consumer/all3 examples bothOS pass. Native horizontal identity reuse follows the
existing source proof; no reference build, new baseline policy or timing repeat.

The preceding [VP8L lookup reconciliation](warm-vp8l-borrowed-green-evidence.json)
replaces exactly seven decode timings with qualified short gaps: native VP8L
ordinary/live-token/Stream, literal/run, indexed16 and compressed-alpha VP8
ordinary/Stream. Native lossless improves13.5–13.7%; literal/run's mean is2.1%
above its preceding result and alpha controls are roughly flat. Original private
pixel tail scratch and stable grouping add no allocation/reservation/work.
All three predecessor files are recovered by removing only this primitive.
All200 other comparisons/all58 passing proofs retain their paths. Full3654
Release tests/OS,43 scalar Huffman/lookup checks/OS and the same actual package
consumer/all3 examples bothOS pass. Original validator refusals were reconciled
without repeating completed timings; no baseline/tolerance/boundary changed.

The preceding [adaptive JPEG reconciliation](warm-jpeg-adaptive-ac-evidence.json)
replaces all eight affected encode timings with qualified short gaps,9.9–14.8%
less managed time. One optional occupancy branch changes AC planning; removing
it restores the entire predecessor JpegEncoder file. All199 other comparisons
and58 passing proofs remain applicable. Packets, compressed sizes, approved
grayscale reference and quality/admission/complete-output boundaries are frozen.
Full3650 Release tests/OS,20 scalar adaptive checks/OS and the same actual package
consumer/all3 examples bothOS pass. No case was added or long gap confirmation run.

The preceding [JPEG scanner reconciliation](warm-jpeg-entropy-evidence.json)
replaces only resident JPEG Identify:0.499x/upper0.506x,72.9% less managed time.
One noncancelable branch/helper batches admitted entropy/stuffing; removing it
and three imports restores the entire predecessor JpegIdentifier file. All206
other comparisons and57 prior passing proofs retain their executed paths.
Full3648 Release tests/OS,20 scalar scanner checks/OS and the same actual package
consumer/all3 examples bothOS pass. The approved reference and all21 equivalent-
boundary controls remain unchanged. No case was added.

The preceding [byte packing reconciliation](warm-png-byte-packing-evidence.json)
replaces exactly gray8/RGB8/gray-alpha8 encode timings; all three independently
pass upper<=1 with exact packets and frozen Optimal9/native6/5% IDAT policy.
The entire predecessor PngCodec file is restored by removing one dispatch and
two packing helpers. All204 other comparisons and54 prior passing proofs remain
applicable. Current3646 Release cases/OS,36 scalar packing/admission controls/OS
and the same actual package consumer/all3 examples bothOS pass. No case was added.

The preceding [PNG Up reconciliation](warm-png-up-simd-evidence.json) changes only
filter2 dispatch and a safe SIMD row helper. DecodeUp8/16 independently pass
at0.984x/upper0.989x and0.995x/upper0.997x. The entire predecessor PngCodec file
is restored by removing that dispatch/helper; all205 other comparisons and52
prior passing proofs remain applicable. No admission/work/allocation or quality
change. Current3642 Release cases/OS, five scalar Up and127 scalar public PNG
controls/OS and the same actual package consumer/all3 examples bothOS pass.

The preceding [PNG chunk checksum reconciliation](warm-png-chunk-crc-evidence.json)
affects exactly16 existing PNG encode selectors. Ten independently reconfirm
upper<=1; six retain qualified short gaps. Stored8/16 means are1.624/1.664x;
the word gain uses a contemporaneous accepted-build control because its old
504.25us result was not reproduced. All packets/settings/quality/ownership gates
remain frozen. The other191 comparisons, including42 passing proofs, retain
unchanged executed paths. Decode/metadata Compute and its old helpers are
byte-identical. Current3,639 Release cases/OS,83 scalar CRC and49 scalar public
PNG controls/OS and the same actual package consumer/all three examples on
Windows/Linux pass. No material case or input was added.

The preceding [six-byte Paeth reconciliation](warm-paeth-six-channel-evidence.json)
adds one SIMD reconstruction kernel: RGB16 independently passes at0.959x/upper0.969x.
RGBA8/RGB8 dispatch controls have qualified short screens; their kernels remain
unchanged. The other204 comparisons, including all51 earlier passing proofs,
are reused. The stopped passing RGBA16 Paeth first branch is unchanged.

The prior [VP8 source reconciliation](warm-vp8-source-views-evidence.json)
changes only source conversion/sample access for four existing encode selectors.
Opaque8/16 are qualified screens at1.661/1.685x; alpha8/16 independently reconfirm
passing upper<=1. The other203 comparisons, including49 passing proofs, retain
their executed paths. No other codec, decode, metadata or edit path changed.

The prior [fixed AC reconciliation](warm-jpeg-fixed-ac-fusion-evidence.json)
changes the selected fixed writer traversal while preserving exact packets,
ordered work/callbacks, cancellation, quotas and partial state. Forward4208/16,
forward4448 and live-token4448/42016 have current qualified short screens at
1.399–1.548x; no independent parity confirmation is claimed. Adaptive coding,
general writer methods/fields, scalar fallback and all other codecs/edits retain
their paths. The other202 comparisons and all51 passing proofs remain applicable.

The preceding [cancelable refinement reconciliation](warm-jpeg-cancelable-refinement-evidence.json)
adds one post-fast-path live-token dispatch and a guarded block reader/helper/
wrapper. Previous methods and fields are unchanged. Only ProgressiveCancelable444
replaces its old timing: the qualified short screen is1.726x/upper1.798x, with
per-bit cancellation, immediate updates and exact work/failure state retained.
Ordinary progressive paths remain unchanged; subsequent encoder updates are above.

The preceding [paired-column reconciliation](warm-bicubic-paired-columns-evidence.json)
changes only the admitted word column-filter body and its checked row-offset
storage. Mixed16, TightMixed16 and TightVertical16 have fresh qualified quiet
screens; no independent parity confirmation is claimed while their means remain
1.307–1.462x. Other resize paths retain applicable proofs. No new case or raster
was added.

The preceding [fused refinement reconciliation](warm-jpeg-refinement-fused-evidence.json)
changes ReadBlock's refinement dispatch and adds a guarded reader plus small
helpers. All five affected progressive selectors have fresh independent
comparisons. Existing fallback methods, baseline JPEG, encoders, metadata,
Stream acquisition and other codecs retain their executed source paths and proofs;
the later three resize changes are recorded above.
The [preceding Stream reconciliation](warm-png-stream-identify-evidence.json)
still applies; its common acquisition boundary is represented once, with three
separate resident parsers. At that checkpoint, 3,619 Release cases per OS and consumer/all
three examples through the same actual Release package on Windows/Linux pass.

The original audit at349fedd reconciled203 numeric rows and fourteen historical
short regressions. Subsequent changes invalidate only the identified executed
paths; source/whole-DLL hash changes alone do not invalidate unrelated methods.
The linked B01-B12 reports record each old/new path and the accepted reuse or
replacement confirmation. Latest short regressions do not substitute for an
independent current-path proof when the timed production path actually changed.

Approved baselines, exact managed pixels, source-quality/size comparisons,
complete operation owners and quiet-VM confidence requirements remain unchanged.
Centered/clamped crop, AutoOrient and ResizeToFit use the named kernels after
small geometry work; correctness/composition controls cover those aliases.
The measured scope excludes GIF/animation, unmeasured drawing/text, arbitrary profiles/
sizes/hardware, startup and concurrent throughput. Add a later case only when
concrete evidence identifies a distinct omitted required algorithm or public
boundary, with a named representative and reason. Do not expand settings grids
or retune a confirmed case without affected inputs or a demonstrated regression.
