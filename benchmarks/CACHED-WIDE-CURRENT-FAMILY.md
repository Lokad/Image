# Current cached wide-family baselines

Retained pre-change baseline. [Shared pixel preparation](CACHED-WIDE-PIXEL-PREPARATION.md)
owns current production costs and qualification; the measurements below remain
the immutable comparison baseline.

WFB655 resolves seven missing current confirmations before changing the shared
horizontal filter. Together with the unchanged-build Wide8 confirmation, the
eight already-required controls have **four passes, four gaps and no uncertain
results**. There are no new images or settings: two original 513×259 byte/word
rasters serve the existing support32/64/128/256 and heap-metadata boundaries.

| Existing default control | Managed µs | zimg µs | Ratio / upper | Managed B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Wide8 | 498.26 | 347.09 | 1.436 / 1.444 | 198,449 | gap |
| Down64x32_8 | 432.00 | 406.22 | 1.063 / 1.074 | 197,679 | gap |
| Wide16 | 493.37 | 455.48 | 1.083 / 1.090 | 198,444 | gap |
| Down64x32_16 | 450.17 | 428.62 | 1.050 / 1.076 | 197,673 | gap |
| Down32x16_8 | 392.69 | 414.33 | 0.948 / 0.952 | 197,298 | pass |
| Down32x16_16 | 387.36 | 451.72 | 0.858 / 0.867 | 197,294 | pass |
| Down16x8_8 | 381.48 | 499.72 | 0.763 / 0.770 | 201,202 | pass |
| Down16x8_16 | 373.55 | 543.82 | 0.687 / 0.693 | 201,199 | pass |

Each row retains twelve actuals, both native brackets and idle-v2 guards.
Managed rational/frozen pixels, original native max1/RMSE≤0.25, source and
independent output ownership pass. Complete normalization/preparation/working
storage/result ownership/disposal remain timed. The explicitly labeled zimg
reference retains its graph/scratch before timing. These are warm prepared
backend comparisons, not fresh graph construction.

The earlier 2026-11-01 family predates the allocation and cached-scheduler
[changes identified in the Wide8 refresh](CACHED-WIDE-BYTE-CURRENT-PACKAGE.md).
Historical clocks establish no causal gain/regression here. Passing 32×16/16×8
controls stop; changes to their shared helper require regression qualification,
rather than individual retuning.

All costs use actual qualified DLL596a5be7 / private Release0.2.1 package2db7ad49.
[Existing qualification](PACKED-BICUBIC-DECODED-PREFIX.md) supplies 5985 full/live
checks per OS, scalar/ISA checks, the same actual-package consumers and three
examples. Zero builds, new fixtures/profiles or repeated suites/packages occur.

[Structured evidence](cached-wide-current-family-evidence.json) pins every case
and the next typed-pixel-view hypothesis. Mirrored/member-verified raw
cached-wide-current-family-baselines.tar.xz contains 6167008 B / 179 members,
SHA256 4d8548983b9e644e6022c8d5f5408de4fa40613fc803432948f82e33e59ebd6c.
The reused Wide8/current-package archive is a pinned dependency. Full 283-path
parity remains open; publication is postponed.
