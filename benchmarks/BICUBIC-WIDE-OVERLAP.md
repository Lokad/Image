# Shared wide bicubic source windows

The ten reference costs remain source-valid. The large managed-only default
cost below is superseded by [shared cached-wide windows](CACHED-WIDE-SHARED-WINDOWS.md),
which owns the current default/compact screen and whole-build qualification.

WOP718 shares ordered overlap arithmetic and byte-offset preparation across
cached, narrow-column and wide-column execution. Complete bounded wide-byte
resize saves **30.65 µs / 6.72% raw / 6.68% reference-normalized**,
455.93→425.28 µs. Filtered/all-actual raw and normalized confidence separate in
the reversed independent comparison; native movement is −0.045%.
Current upper1.233 remains a gap. Exact pixels and allocations are unchanged.

| Existing complete 513×259 resize | Lokad µs | zimg µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Byte →128×64, 4 KiB working | 425.28 | 346.96 | 1.226 / 1.233 | 113 | gap |
| Word →128×64, 4 KiB working | 405.87 | 416.73 | 0.974 / 0.978 | 113 | pass |
| Byte →256×129, 4 KiB working | 504.20 | 419.02 | 1.203 / 1.208 | 113 | gap |
| Word →256×129, 4 KiB working | 545.86 | 573.09 | 0.952 / 0.956 | 113 | pass |
| Byte →256×129, cached | 460.84 | 418.20 | 1.102 / 1.106 | 199,981 | gap |
| Word →256×129, cached | 463.81 | 569.75 | 0.814 / 0.821 | 199,955 | pass |
| Byte →256×259, cached | 451.06 | 566.32 | 0.796 / 0.804 | 199,977 | pass |
| Word →256×259, cached | 559.38 | 578.58 | 0.967 / 0.977 | 199,933 | pass |
| Byte →256×519, cached | 782.07 | 840.35 | 0.931 / 0.936 | 199,943 | pass |
| Word →256×519, cached | 768.81 | 824.92 | 0.932 / 0.936 | 199,898 | pass |

All ten affected comparisons refresh on the actual-package DLL: **seven pass /
three gaps / zero uncertain**. Wide-word now passes and no earlier pass becomes
a gap. Other point differences are fresh costs, not causal companion gains.
Passing cases stop. No new benchmark image, shape or settings grid is added.

## Preparation and execution

One overlap kernel handles fixed eight-tap/+2 and sixteen-tap/+4 callers. The
wider mode keeps three source windows in registers, supplying twenty pixels
through ten bounded window loads while preserving each output's tap0..15 order.
All products and additions remain separate binary64 operations. Its actual
5,612-byte byte driver inlines the kernel, folds the fixed mode and retains the
rolling windows in registers without weight-loop ZMM stack transfers. Code
structure alone establishes no CPU saving.

The shared preparation helper resets dead padding each strip and proves counts,
contiguous offsets, stride and complete source bounds before marking it. Only
four-column/equal-sixteen-tap pairs qualify; live seventeen-tap offsets, clamps,
unequal counts and odd tails retain the indexed fallback. Three-column/twenty-tap
execution remains unchanged. The column-specific preparation entry is removed.
The geometry model's60/64 eligible primary pairs are source/index evidence,
not measured branch counts or a removable CPU percentage.

No array, reservation, owner, pool, session or retained scratch is added. Existing
4,096-byte admission covers wide/three-column explicit payload4,068/3,776 B;
narrow payload remains4,080/3,232 B. Wide byte/word results retain32,768/65,536 B
until disposal; smaller-support results retain132,096/264,192 B. Cached byte
workspace stays199,840 B. Default output-pool retention remains separate and
governed calls bypass it. Physical/native/total-stack peaks are unmeasured.

Twenty shared-kernel/preparation controls cover bitwise byte/word/low-alpha sums,
stale/full/odd markers, missing padding, incomplete source windows and cancellation.
Existing actual wide tests cover4.125 scaling, partial strips, three columns,
half ties, poisoned output, direct/frozen pixels, refusal, tiny-alpha retirement,
recovery and independent survivors. The byte horizontal identity boundary retains
its declared binary64 sample-half decisions rather than a rational-equivalence claim.

Windows/Linux each pass **6,120 full/live checks**,285internal/729public per
normal/scalar/AVX2-disabled/AVX512-disabled mode, and the SAME actual Release
package/all three examples. One production/embedded build per OS is reused
throughout. Harnesses, selectors, native workers and fixtures require zero builds;
there are no targeted rechecks or repeated candidate measurements for favorable
results. Encoder packets, metadata exceptions, cancellation and admission remain.

Managed preparation, working storage, computation, independent output and disposal
stay timed, sequential Release/one CPU on the exclusive quiet VM. Zimg retains
its labeled graph/scratch and native float bounds; managed precision is unchanged.

The existing managed-only4097×2051→1024×512 control also refreshes on this package:
default27.33 ms / compact31.32 ms, three actuals each. Intervals overlap; no useful
generalized CPU gain, regression, equivalence or default promotion is established.
No confirmation or repeat follows. Normal/scalar/prior complete-frame hashes,
source and surviving owners agree. Workspace remains1,585,440/4,096 B, governed
peaks3,682,592/2,101,248 B and result2,097,152 B until disposal. Total allocations
are1,585,375/153 B/op. Earlier large costs are historical after their kernels
changed; the reused diagnostic's stale stage banner has no measurement role.

[Evidence](bicubic-wide-overlap-evidence.json) owns costs, comparison, source/JIT/
lifetime proof and qualification. Mirrored/member-verified raw
`bicubic-wide-overlap-qualified.tar.xz`:6,504,628 B /262members, SHA256
`5ccaa3e758c9ff5c25ae8b15344686196b01897d91d323c0e72bcd43556614fe`.
Private package439,930 B, SHA256
`427da1da232f4d4d2c928711b04ed01d9544cbf09e98c303eeaab680d81f4206`.
[Earlier overlap](BICUBIC-SHARED-OVERLAP.md) and
[shared repair](BICUBIC-SHARED-BYTE-REPAIR.md) retain gains/adverse history.
Published packages remain immutable; publication stays postponed and all283paths
remain required. Larger unchanged mixed-byte and cached-wide gaps take priority next.
