# Stored PNG row normalization

Stored RGBA8 encoding borrows source rows. RGBA16 converts native words directly
into the existing admitted history, fusing endian permutation, stores and exact
Adler arithmetic on guarded AVX512BW/F. Portable packing/checksum fallbacks remain.
There is no intermediate packed-row allocation, new owner, field, cache or session.
One isolated stored-row executor keeps this routing out of the packed/compressed
scanline loop; header, output, completion and cleanup remain shared.

Independent retained-native word confirmation is **421.10→387.85 µs**, a
**33.25 µs / 7.90% filtered raw gain** with separated confidence. All 12 actuals
give423.21→396.16µs (27.06µs/6.39%); that interval overlaps. Normalized7.93% also
overlaps; native moves+.034%. No all-actual, normalized or general encoder gain
is claimed. The selected module's retained-word parity remains **uncertain**,
range.906707–1.043595; an earlier prototype's passing result is not current proof.

| Complete stored encode | Lokad / FFmpeg µs | Ratio bounds | Managed B/op | Status |
| --- | ---: | ---: | ---: | --- |
| RGBA8, equivalent fresh contexts | 53.27 / 541.40 | 0.089–0.114 | 607,628 | pass |
| RGBA16, equivalent fresh contexts | 389.62 / 1387.71 | 0.276–0.288 | 1,143,324 | pass |
| RGBA8, native context retained, post-GC4 | 53.68 / 188.97 | 0.259–0.327 | 607,628 | pass |
| RGBA16, native context retained, post-GC4 | 387.85 / 406.15 | 0.907–1.044 | 1,143,324 | uncertain |
| RGBA16, native context retained, natural-GC64 | 235.78 / 404.41 | 0.549–0.615 | 1,143,265.875 direct | pass |

Each final capture retains12actuals and12native samples per bracket, original
outliers included. SingleCPU0, optimized warm JIT, workstation nonconcurrent GC
and idle-v2 guards pass. All frozen513×259 samples, compression/filter/color/native
settings, complete packets, full-depth/hiddenRGB/alpha pixels and independent
outputs remain exact. Stored packets stay533,444/1,066,552B; word SHA256
`9b31604447d61d5ab7a30aad3cfde4cd73fef6d85ff506e74baeba807e54574d`.

Managed timing includes fresh MemoryStream creation/growth, complete public
EncodePng and disposal, without ToArray. Equivalent fresh-native views include
codec/zlib/frame/endian workspace/output setup and disposal. Retained-native
contexts and word workspace are explicitly prepared; word conversion is timed.
Natural-GC64 and post-GC4 are separate conditioning scopes, never additive phases.

Storedbyte/word total allocations fall2,080/4,128B on these controls; local
admission falls2,052/4,104B. The array removal is structural: old/new output and
history ownership do not change. Stored deflater capacity remains72KiB, retained
operation scratch0. These logical capacities are distinct from allocations;
physical/native/JIT/pool peaks remain unmeasured. Refusal stays terminal; storage
remains charged through synchronous writers and actual async delivery users.

The row-preserving conversion prototype and row-free stage alone show no
independent CPU gain. Fusion then improves stored words but materially regresses
Predictable16:276.86ms versus qualified254.85ms, raw/all-actual and normalized
adverse intervals separated. That design is rejected. Isolating stored execution
produces254.31ms versus the same254.85ms baseline, with overlapping raw/all-actual
and normalized intervals; no Predictable gain/regression is claimed. Its current
FFmpeg385.75ms comparison passes, upper.679. All adverse/inconclusive sources,
modules, actuals and native movement remain retained; no favorable resampling.

Every20encoder view is refreshed once on the final immutable module before broad
qualification. The17fresh controls now have15passes, one Gray8gap and one Fastest8
uncertainty. Gray8 is2030.26/native1976.23µs, range1.003–1.062; begin attribution
there before another algorithm change. Earlier natural-GC Gray/RGB adjacent costs
retain their original conditioning, not substituted for these post-GC measurements.
The51unchanged PNG reads/shared-root and unrelated families reuse valid evidence.

Four guarded internal cases compare the actual word writer with independently
serialized network samples through the production generic path, including all65535
samples, unaligned input, repeated rows and odd65531-byte hard splits. One public
16383-pixel packing case supplies an independent pixel oracle. Precise admission,
failure, cancellation, cleanup and consumer checks reflect the removed row.
Ten common generated bodies match instructions/labels/code size after relocation
normalization; all three word bodies match the fused prototype. The original
packed/compressed loop reconstructs exactly outside indentation.

Final code passes **6,522 full/live checks per OS** (3192internal/2382public/948live),
713internal/247public PNG/admission checks in normal/scalar/AVX512-off/AVX2-off,
then the **SAME actual Release-package consumer/all three examples** on both OSes.
One core/embedded build per stage/OS, two public-test builds perOS (including one
quota-test correction), no harness/native/fixture builds, one final full/package
milestone. Original failing test logs are retained. The existing19×131 stored
consumer now verifies the smaller working boundary with exact byte/word samples
and sync/async independent owners.

Actual0.2.1 development package457,370B, SHA256
`2325540a2c6a20fe84832d0a5966425f11af87ea994d5548099592b017b9007e`;
timed/Windows DLL `d569b8bdb7d00b80f4de2ffacf56580c650bd480bb4684029311de230a1dd491`,
Linux qualification DLL `80adab8ad32ddc1f07b9fcfd21f51db544f06a83a10ee437022943a58d402d15`.

[Evidence](png-stored-word-normalization-evidence.json) pins all stages, current
costs, source identities, output gates and actual package. Raw
`png-stored-word-normalization-qualified.tar.xz`, SHA256
`bbacaed4cbcbd88d226f04336fd27e531d5617c1be16601519078dc3010d0e1d`,
3,417,240B/833members, is mirrored/member-verified. [Forwarding](PNG-STORED-FORWARDING.md), [CRC](PNG-WIDE-CHUNK-CRC.md)
and [adjacent confidence](PNG-CURRENT-ENCODE-CONFIDENCE.md) retain superseded CPU
costs and reproducibility. Published packages are unchanged; publication remains
postponed. Stop passing scopes; all289required paths remain the overall objective.
