# Ordered bilinear horizontal windows

Existing ordered-window filtering now serves both byte and word triangle
reduction. Complete Down8 saves **28.77µs/6.42%raw/6.35%reference-normalized**;
Down16 saves **32.94µs/7.08%raw/6.88%normalized**. Filtered/all-actual raw and
normalized intervals separate for both. Native points move−.072%/−.208%.
Word passes/stops; byte remains uncertain, not a parity pass.

| Existing513×259→256×129 public Resize | Lokadµs | FFmpegµs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Down8 | 419.27 | 417.88 | 1.003 / 1.013 | 8,473 | uncertain |
| Down16 | 432.57 | 449.05 | 0.963 / 0.974 | 8,473 | pass |

Source rows are decoded/premultiplied and cached as before. Prepared four-tap/+2
pairs use the existing overlap helper: three two-pixel windows feed one512-bit
accumulator in original t0,t1,t2,t3 order, replacing two256-bit loops. The source
model finds124eligible/128output pairs; four clipped/unequal pairs keep their
original path. Counts/offsets prove the six decoded values are live. Eligibility
is prepared once per six-column strip in dead offset slot9, within the existing
maximum six taps. No new filter body, descriptor or array is added. Logical
load/MAC counts are not phase-time or CPU-gain forecasts.

Both whole source bodies reconstruct after removing just preparation and eligible
dispatch. Existing helper code is unchanged; source decoding, row cache, all
weights/normalizers, vertical accumulation, division, certificates/packing,
original/tiny-alpha repair, cancellation, admission and owners remain verbatim.
No zero padding, reassociation, FMA or numerical-policy change. Actual helper
tests verify1,024four-tap lanes bit for bit against independently ordered scalar
products, including low alpha/negative weights and guard/borrowed input preservation.
Prepared metadata preserves every live offset, with edges/unequal layouts declined.
Full rational/qualified frame and existing odd/tiny-alpha/ownership tests remain.

Managed timing includes admission/preparation, source processing, full filtering/
repair and independently owned output acquisition/disposal. The approved FFmpeg
public triangle graph/input/geometry contexts stay prepared; its conversion,
filtering/normalization and output acquisition/release remain timed. This is the
existing repeated-reference boundary, not fresh-graph equivalence. Original inputs,
settings, sample depth, exact managed frames and native bounds are frozen. Up8/16
and other kernels are unchanged; no new image/settings grid or worker is introduced.

**Allocation remains8,473B/op**, separately from admitted scratch **12,608B**:
8,512B geometry plus4KiB stack allowance, with3,560B existing explicit stack arrays.
No heap/stack-array capacity, owner, pool/cache or retained scratch is added.
Byte/word result payload132,096/264,192B remains independently owned until disposal;
default pool retention and governed bypass stay unchanged. Physical/native/JIT
peaks are unmeasured. Shared refusal remains terminal, without retry or fallback.

## Confidence and qualification

The byte screen399.67/native411.14µs did not establish parity. Independent
confirmation419.27/native417.88µs is uncertain, upper1.013. Comparing it with old
420.16/native385.48µs would mix unchanged raw time and an8.4%native movement.
One contemporary unchanged predecessor gives448.04/native418.18µs: the stated
28.77µs gain separates in both views. No candidate is repeated for a favorable bound.
Word screen418.80/native422.41µs stays diagnostic; its independent432.57/native449.05µs
passes upper.974 versus source-valid465.51/native449.98µs. Every actual and both
native brackets remain, CPU0/tieringoff/nonconcurrentGC and idle-v2 before/after.

Only after both affected costs reconcile: **6,291 full/live checks per OS**,
382internal/789public resize checks in normal/scalar/AVX2off/AVX512off modes,
including all three named storage suites. **SAME actual Release-package consumer
and all three examples pass Windows/Linux**, including both reductions versus
the unchanged tight-memory fallback and shared-root cleanup. One core/embedded
build per OS; zero public/benchmark/selector/native/fixture builds or compile
failures. One broad/ISA/package milestone; other source-valid qualification is reused.

Private0.2.1 package`5a14cbba810f2666f3b7dc9fe20ce8d9636dbbcdff9f0dd3a23c5da31e85ae5a` contains timed DLL
`c910c4f4c615f53689761dfc81bd3c93ba111f22a8c5bee4d5e759b5c158dfee`. Published releases stay immutable; publication is postponed.
The combined four bilinear controls now have two passes/two uncertainties.
Stop Down16/Up8; Down8/Up16 uncertainty is not a presumed remaining gap. All289
material paths remain required; independent codec/resize work continues.

[Evidence](bilinear-ordered-horizontal-windows-evidence.json) pins sources,
all affected costs/countercheck, storage, invalidation and qualification. Mirrored/
member-verified raw`bilinear-ordered-horizontal-windows.tar.xz`:3,627,352B/150members,
SHA256`9826b52fcab268ffa0c31af4dbe9fc3e791b96f6254111ece0054fc39700da8e`. Dependencies retain original fixtures, reference/selector
identities and[shared-preparation history](BILINEAR-SHARED-PREPARATION.md).
