# Exact horizontal preparation and execution in enlargement

Vertical byte enlargement saves **286.55 µs / 16.42% raw / 15.98% normalized**;
vertical word saves **232.95 µs / 15.15% raw / 19.18% normalized**. Tight full-word
enlargement saves **78.84 µs / 2.91% raw / 2.97% normalized**. Filtered, all-actual
and reference-normalized intervals separate for these three controls. Native clocks
move −.52%/+4.98%/+.063%; raw and normalized gains remain distinct.

All twelve affected boundaries independently refresh: **11 pass / 1 uncertain**.
Stop confirmed cases. Tight horizontal byte remains uncertain; the earlier
implementation's passing result does not prove the final implementation passes.

| Existing complete public Resize | Lokad µs | zimg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Up8 | 3,113.60 | 3,291.86 | 0.946 / 0.955 | 116 | pass |
| TightUp8 | 3,064.21 | 3,289.31 | 0.932 / 0.939 | 116 | pass |
| HorizontalUp8 | 1,557.88 | 1,646.88 | 0.946 / 0.958 | 114 | pass |
| TightHorizontalUp8 | 1,499.15 | 1,521.64 | 0.985 / 1.048 | 114 | uncertain |
| VerticalUp8 | 1,562.11 | 1,879.56 | 0.831 / 0.837 | 114 | pass |
| TightVerticalUp8 | 1,458.80 | 1,744.79 | 0.836 / 0.845 | 114 | pass |
| Up16 | 2,893.73 | 3,105.31 | 0.932 / 0.940 | 4,264,573 | pass |
| TightUp16 | 2,629.90 | 2,877.16 | 0.914 / 0.919 | 4,264,573 | pass |
| HorizontalUp16 | 1,490.98 | 1,570.73 | 0.949 / 0.958 | 114 | pass |
| TightHorizontalUp16 | 1,453.67 | 1,578.33 | 0.921 / 0.959 | 114 | pass |
| VerticalUp16 | 1,343.50 | 1,798.61 | 0.747 / 0.754 | 114 | pass |
| TightVerticalUp16 | 1,304.89 | 1,751.48 | 0.745 / 0.756 | 114 | pass |

All use the original 513×259 raster, byte/word depth and frozen output shapes:
full 1027×519, horizontal 1027×259, vertical 513×519; default or input+output+4 KiB.
Every necessary managed preparation, processing, repair, independent output and
disposal remains timed. The explicitly labeled zimg worker retains its graph and
scratch. This is the original prepared-reference boundary, not fresh-graph parity.
Inputs/settings, exact managed hashes and native tolerances remain unchanged.
There are no new performance fixtures, selectors, reference workers or grid cells.

One shared enlargement schedule selects horizontal work at source-row boundaries:

- **Identity width:** for equal positive widths ≤2^26, binary64 cardinal coordinates
  are exact and the five weights are +0,+0,1,+0,+0. Decoded unsigned premultiplied
  samples are finite/nonnegative with +0, so direct decoding into the existing
  cached row is bit-identical to filtering. It removes the full horizontal stage
  and intermediate copy/halo decoding. Larger widths keep the original path.
- **Paired interpolation:** prepare common/adjacent four-tap windows once per
  full-row geometry or fourteen-column strip. Existing dead offset slot 4 stores
  proof only; full five-tap records and all live offsets remain unchanged. The
  current horizontal raster has 255 common/254 adjacent eligible pairs of 513.
  The new helper uses original t0,t1,t2,t3 multiply/add order in each half.
- **Other layouts:** retain the original filter body/signature and exact singleton
  fallback. Clipped, unequal, nonconsecutive or full five-tap pairs are not forced
  into the new primitive. No per-pixel generic policy, reassociation, FMA or padding.

Original weights/normalizers, five-row scheduling, output-backed workspace views,
static arrays, decoder bodies, certified publication, original/tiny-alpha repair,
NoInlining retirement and owner/admission policy remain. Removing just horizontal
mode selection/direct-cache decoding and the two new paired helpers reconstructs
both previous source files. Existing four/eight/sixteen-tap math is unchanged.
Twenty-two focused cases include 2,048 exact SIMD lanes, live-offset/edge/odd/full-tap
guards, cancellation, and four independent byte/word rational identity-width frames.
No numerical-policy relaxation or new public API.

**Allocations/capacity:** 114 B/op for ordinary boundaries,116 B/op for full byte,
4,264,573 B/op for full word. No allocation-saving claim, new owned capacity,
stack-array capacity, pool or retained scratch. Existing strip arrays 4,000 B and
4 KiB scratch admission remain. Prefix views stay 232×outputWidth+32×sourceWidth
bytes inside already-owned output; identity width leaves the old decoded view
unused. Returned results retain independent charges until disposal. Default pool
and governed bypass are unchanged; physical/native/JIT peaks remain unmeasured.
Refusal stays terminal; direct cancellation and cleanup remain bounded.

## Evidence and limits

Version A had a passing primary byte result but a material raw vertical-byte
slowdown versus its unchanged predecessor. A was not accepted. Version B removes
that redundant identity-width filtering. All A/B screens and twelve-actual costs,
including adverse/inconclusive results and the two predecessor checks, remain.
Sampling from [capacity trials](ENLARGEMENT-IDENTITY-CAPACITY-TRIALS.md) motivated
the filter work; its 31% exclusive share was not an elapsed-time or gain forecast.

Final tight horizontal byte 1,499.15/zimg1,521.64 µs, upper1.048, is **uncertain**.
Its lower point and the older A pass do not establish current parity. No candidate
is repeated simply for a favorable bound. Full-byte raw times move adversely
while reference clocks move more; current costs pass, but there is no uniform raw
speedup claim. All native brackets and filtered/all-actual views remain explicit.

Only after all twelve costs reconcile: **6,351 full/live checks per OS**,
442 internal/789 public resize checks in normal/scalar/AVX2off/AVX512off modes,
including all three named storage suites. SAME actual Release-package consumer
and all three examples pass Windows/Linux, including four original frozen
horizontal/vertical byte/word frames, refusal/cancellation/recovery and independent
survivors. Two core/embedded versions per OS; no public/harness/native/fixture
builds or compile failures. One broad/package milestone; other source-valid codec,
text, editing, reduction and prepared cached-plan qualification is reused.
An unprepared BicubicResizePlan delegates to this kernel; no separate timing claim.

The private 0.2.1 package `a2f3ea5a1831d63665fa197eb96d73aef47e875b084bdf9bb6025287abe77f39` contains timed DLL
`8a0fff84ae5f4bb66c605614c1bdbdfa3bf5600bc0446b4dd5ed7fe927ca8dda`. Published releases are unchanged; publication postponed.
[Evidence](bicubic-enlargement-horizontal-modes-evidence.json) pins all costs,
source scope, storage and qualification. Mirrored/member-verified raw
`bicubic-enlargement-horizontal-modes.tar.xz`: 4,802,988 B/406 members, SHA256
`856e6d3f68e8a8f5e5faacb5d50c49b08fec2cb9fefe3761a7d3df741a904cfd`. Dependencies retain original controls, actual predecessor
package and sampling history. All 289 required material paths remain in scope.
