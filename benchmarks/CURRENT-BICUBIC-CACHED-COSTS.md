# Current cached bicubic costs

This is the DHP1 snapshot. [Shared word publication](WARM-BICUBIC-SHARED-WORD-PUBLICATION.md)
supersedes Down16/Up16/Horizontal16 costs and qualifies the word vertical axes.
Its byte and mixed-word paths remain unchanged; their costs below still apply.

All nine existing controls affected by DHP1 now have current costs on the same
qualified producer. Inputs remain the original 513×259 RGBA8/16 raster, default
workspace; output dimensions appear below. These are complete warm one-CPU
operations with independently owned output. Reference: zimg, FFmpeg's zscale
backend, with unchanged approved boundaries/settings. Every case remains a gap.

| Existing control | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Down8 → 256×129 | 560.306 | 1.339 / 1.347 | 332,186 | Confirmed gap |
| Down16 → 256×129 | 655.007 | 1.141 / 1.149 | 464,330 | Confirmed gap |
| Up8 → 1027×519 | 3,916.744 | 1.283 / 1.507 | 3,244,842 | Screen gap |
| Up16 → 1027×519 | 4,182.047 | 1.453 / 1.654 | 5,016,594 | Screen gap |
| Cross8 → 256×519 | 921.734 | 1.109 / 1.200 | 731,768 | Screen gap |
| Cross16 → 256×519 | 992.107 | 1.210 / 1.293 | 1,263,193 | Screen gap |
| Horizontal8 → 256×259 | 691.423 | 1.178 / 1.253 | 465,390 | Confirmed gap |
| Horizontal16 → 256×259 | 843.016 | 1.381 / 1.527 | 730,643 | Confirmed gap |
| HorizontalUp8 → 1027×259 | 2,230.305 | 1.423 / 1.531 | 1,930,356 | Confirmed gap |

Down8/Up16 come from [DHP1](WARM-BICUBIC-HORIZONTAL-TAPS.md). The seven companion
costs require zero builds or repeated full/package proofs; original fixtures,
workers and cached drivers are reused. Down16 and three horizontal cases each
have one independent twelve-actual confirmation resolving their short-screen
uncertainty. Up8/Cross8/Cross16 remain clear-gap three-actual screens. All actuals,
filtered/unfiltered confidence, native brackets, allocations and idle guards remain.
No historical gain/regression inference follows from these cost refreshes.

Exact qualified pixels, approved native bounds, input and output ownership pass.
Horizontal8 retains its existing six independently certified rational alpha
halves. An overbroad postcheck initially rejected its false rational-equality flag;
the completed original timing was recovered after checking the established
contract, without rescoring or adding an exception. Qualification reuses DHP1's
5,074 checks/platform, focused normal/scalar cases and the SAME actual package
on both platforms with three examples. Production and all library inputs remain
unchanged. The full finite 268-path objective is still open.

[Evidence](current-bicubic-cached-costs-evidence.json) retains all seven companions
and links the unchanged producer/qualification. Both raw archives are mirrored
and member-verified:

- current-bicubic-cached-cost-reconciliation.tar.xz, 169 members, SHA256
  9e04ee1f500ec472375901261f634e1d57cb37fff7484d49ffa7e15707a144dc.
- current-bicubic-cached-down16-cost.tar.xz, 50 members, SHA256
  d00739ed2b8c3eba6545bc4db446ca92bc97a09a273b66c7ec7ddda6cb560b8d.

The next lead is two-axis enlargement: assess whether the existing contiguous
four-tap reuse can serve Up16, using one current profile before production edits.
