# Bounded wide bicubic: current tail attribution

The first profile after [contiguous source addressing](CONTIGUOUS-WIDE-SOURCE-VIEWS.md)
uses the [current actual package](WEBP-INVERSE-COLOR-TILES.md), with unchanged
resizing sources. Complete128×64RGBA8/4KiB Resize retains its source-valid
357.33/zimg350.73µs,upper1.033 gap. No new timing or allocation claim.

One two-second CPU0 capture/CPU1 sampler has1,994locations: row-loan1,071,
tail754, geometry Build69 and Kernel56. Row/tail code is6,498/6,512B. Inlining/
skid locations are not phase times, removable fractions or predicted gains.
Instrumented elapsed is discarded. Original exact rational pixels, approved
native-only bounds, frozen input/reference, source/owner and idle-v2 gates pass.

The row-loan uses two contiguous256-bit ordered multiply/add streams. The tail
retains shuffled512-bit pairs and repeated coefficient broadcasts/inserts;
source conversion is also hot. The next bounded experiment shares the accepted
contiguous helper with proven16tap/+4 byte tails, keeping indexed/clamped/odd/
count17 and word paths, certificates/repair, scheduling and memory lifetime.
It adds no kernel, buffer, admission, owner, pool, retained state or API. Code
layout or reduced arithmetic width may erase the benefit; complete costs decide.
This does not replay the earlier overlap activation or expanded-coefficient trials.

Reuse existing exact-bit/poison/noallocation and actual row/tail/generalization/
cancellation/admission tests, including all three named resize storage suites.
Inspect generated word code rather than assuming static guards imply identical
costs. Short then independent confirmation only if promising; reconcile affected
costs before one accepted-stage full/ISA/SAMEpackage milestone. Do not assume the
word uncertainty is a gap. No new workload grid or numerical policy.

This profile needs zero builds or qualification repeats; current6,456checks perOS
and SAMEactualpackage/all3examples remain valid. [Evidence](wide-bicubic-current-tail-attribution-evidence.json)
retains source/JIT/sample identities and the hypothesis. Raw`wide-bicubic-current-tail-attribution.tar.xz`,SHA256`eee2ad57db7aa48ee87b3eedaf4e2b3e90901510939e6c7cc9a45747db149971`
(126680B/27members), is mirrored/member-verified; dependencies preserve
the actual package, original timing/native contexts and frozen fixture. All289
required paths remain; publication is postponed.
