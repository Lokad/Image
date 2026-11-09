# Shared wide pixel preparation

CPP657 removes repeated channel-to-pixel interpretation in the shared cached
wide bicubic filter. Checked pixel-vector views are formed once per row; wide
geometry prepares pixel indexes directly. Only original-order repair restores
channel indexes. Narrow geometry, arithmetic order, padding, normalizers,
certificates, cancellation and memory ownership remain unchanged. No array,
public API, pool, session or retained state is added.

The complete default 513×259 →128×64 byte resize improves **498.26→463.92 µs**:
**34.35 µs /6.89% raw /6.84% normalized**. Filtered and all-actual raw/normalized
confidence intervals separate. The native mean changes only −0.053%. Ratio upper
1.344 leaves a 117.01 µs gap; this is an incremental gain, not parity.

| Existing default control | Managed µs | zimg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Wide8 | 463.92 | 346.90 | 1.337 /1.344 | 198,450 | gap |
| Down64x32_8 | 375.38 | 373.50 | 1.005 /1.013 | 197,683 | uncertain |
| Wide16 | 413.28 | 416.74 | 0.992 /0.996 | 198,444 | pass |
| Down64x32_16 | 394.89 | 392.12 | 1.007 /1.019 | 197,679 | uncertain |
| Down32x16_8 | 321.06 | 376.18 | 0.853 /0.857 | 197,299 | pass |
| Down32x16_16 | 316.89 | 412.73 | 0.768 /0.773 | 197,296 | pass |
| Down16x8_8 | 302.43 | 453.34 | 0.667 /0.673 | 201,204 | pass |
| Down16x8_16 | 295.46 | 495.41 | 0.596 /0.600 | 201,202 | pass |

All eight costs retain twelve actuals, both native brackets and idle-v2 guards.
All four previously passing controls still pass. Companion native means move
−8.05% to −9.28% from the fresh baseline; their larger raw time differences are
not attributed entirely to this implementation. Inputs, settings, native build,
readiness/output identities and boundaries match. Current standalone brackets
establish five passes, one gap and two uncertain results; no separated normalized
regression appears. Uncertain controls are not repeated for favorable clocks.

Timing includes complete public Resize, normalization, geometry and working
storage, independent result ownership and disposal. Explicitly labeled zimg,
FFmpeg's scaler backend, retains graph/scratch before timing; processing, halo,
deinterleaving, quantization and independent output remain timed. This is a
prepared-backend comparison, not fresh graph setup parity. Exact rational/frozen
managed frames and approved native max1/RMSE≤0.25 remain unchanged.

Wide8 retains 32768 B until result disposal; estimated workspace admission stays
198432 B. Total allocation differs by 1 ceil-rounded B/op; owned capacity and
retained scratch do not change. Physical/native peaks and existing idle pool
capacity are unmeasured. Tight 4 KiB column paths are separate unchanged kernels.

The first CPV656 short retained channel offsets with per-tap shifts:558.81/
native374.44 µs. Native movement +7.9% and broad uncertainty establish neither
a useful gain nor a causal regression. CPP657 changes prepared representation,
rather than repeating that measurement. Both variants and adverse/inconclusive
observations are retained; no permanent family restriction follows.

Windows31bc1b42/Linux439ae7e8 pass **5985 full/live checks each**, plus affected
206internal/703public checks per normal/scalar/disabled-AVX512/disabled-AVX2 mode.
The SAME actual private Release0.2.1 package406f7d1e (437208 B) passes both OS
consumers and all three examples, including existing frozen wide byte/word
admission, terminal refusal and independent survivor checks. Each variant uses
one main/embedded build per OS; the selected immutable build is reused throughout
confirmation and qualification. No new fixture/harness/reference builds.

[Structured evidence](cached-wide-pixel-preparation-evidence.json) pins current
cases, sources, confidence, actual package and mirrored/member-verified raw
cached-wide-pixel-preparation-qualified.tar.xz. It depends on the retained
[baseline family](CACHED-WIDE-CURRENT-FAMILY.md),
[Wide8 baseline](CACHED-WIDE-BYTE-CURRENT-PACKAGE.md) and CGA654 attribution.
The attribution design JSON's invalid literal suffix is preserved with its
preflight recovery; no build or profile is replayed. Full 283-path parity remains
open; publication is postponed. Passing cases stop.

A later [overlap-load experiment](CACHED-WIDE-OVERLAP-EXPERIMENT.md) supplies no
useful complete lead. Original sources/modules are restored exactly; the accepted
costs above retain valid evidence. Its raw observations remain separate.
