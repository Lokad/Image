# Current 16-bit PNG decode costs

Three word-expansion controls pass and stop. The Adam7baseline below is
superseded by [framed-output reuse](WARM-PNG-ADAM7-FRAMED-OUTPUT.md), which passes. No production change or causal comparison
against older producers is made.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Adam7 RGBA16 | 5,293.592 | 1.346 / 1.362 | 23,616 | Prior; superseded |
| RGB16 → RGBA16 | 2,193.172 | 0.986 / 0.996 | 6,520 | Pass; stop |
| Gray-alpha16 → RGBA16 | 1,519.872 | 0.846 / 0.851 | 4,472 | Pass; stop |
| Gray16 → RGBA16 | 702.848 | 0.732 / 0.739 | 2,424 | Pass; stop |

All12actuals, native brackets, filtered/unfiltered bounds and both idle-v2 guards
are retained. Complete decode, normalization, independent output acquisition and
release remain timed. Frozen packets/settings, exactRGBA16/alpha/hiddenRGB and
source/output ownership pass. Managed allocations are B/op; native allocations
unmeasured. Reuse the immutable PAV1build, selector, harness/native workers and
full5088tests/OS plus SAMEactualpackage/three examples; zero builds or repeats.

A six-second current-code diagnostic collects5979samples:47.1%BCL inflater,
38.3%framing literal runs,5.5%block counting,1.7%sparse copying. It has no timing
claim. The cached original driver is reused; current PIDmap and all raw samples
remain. Investigate duplicate inflation before copy microoptimizations. A
bounded trial may reuse existing framed output for eligible interlacedRGBA,
preserving bounded admission and the existing shared-root behavior.

37existing costs still need refresh, without case/image expansion. The full
268path target remains active. [Evidence](current-png-word-decode-costs-evidence.json).
Mirrored/member-verified archive current-png-word-costs-profile.tar.xz, 74members,
SHA256 6604404baf2b5b50f5c8db3c6ccbedadbb97a74e3abb76d199b6a0a52affdc53. Qualified-build dependencies are pinned by hash.
