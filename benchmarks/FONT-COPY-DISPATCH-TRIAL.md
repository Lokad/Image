# Owned-font copy dispatch trial

**Rejected:** reusing the existing disjoint-row SIMD copy at the owned-font
boundary makes complete loading **9.4% slower**, 16.634 → 18.199 µs
(+1.565 µs). The three alternating, reversed pairs give a 99.9% paired ratio
interval **1.088386–1.099818**; raw mean intervals overlap. Allocation remains
**176 B/operation**. Every actual is retained; no independent confirmation or
standalone-reference refresh follows this adverse screen.

Current managed and approved FreeType 2.13.2 copy/open/retain/dispose profiles
both resolve their hot copy instruction to libc `rep movsb`. Each genuinely
copies all 757,076 frozen DejaVu Sans bytes. The native two-second profile has
1,284/1,997 leaves at that instruction; this locates work, not a phase cost.
There is no demonstrated reference defect or permission to omit the copy.
Managed eager validation and native lazy validation remain different; neither
stage timings nor these profiles establish an overall text-processing slowdown.

The isolated prototype calls the unchanged safe, bounded Vector512 row-copy
kernel only for disjoint spans, preserving BCL memmove for overlapping views
and existing small/scalar fallbacks. All bytes still precede eager validation
and publication. No source parser, cancellation, admission, owner, capacity,
cache or API changes. Actual generated code realizes the intended vector moves.
The canonical production source, qualified modules and package never change.

Both OSes pass **166 existing public checks per normal, scalar and AVX512-off
mode**, covering font/text, shaping, admission and owner cleanup. The complete
public load/dispose actor preserves original metrics, source independence,
survivors and cleanup checks. CPU0, natural nonconcurrent GC, 16,384 warmups and
operations per actual, two batch clocks and versioned quiet guards are fixed.
This paired-DLL result is not FreeType performance or a parser-equivalence claim.

One isolated Windows core build; zero VM, canonical, embedded-test, public-test,
existing harness, reference, fixture or package builds. Reuse current commit
`10bd64a5` qualification: **6,776 full/live checks each OS**, the same actual
Release-package consumers and three examples per OS, and green CI. No broad
qualification repeats for an unaccepted implementation. Added live/retained
capacity is zero; physical/native/JIT peaks are unmeasured.

[Evidence](font-copy-dispatch-trial-evidence.json) pins the exact implementations,
modules, all actuals, guards, native attribution and reused qualification.
Mirrored, member-verified raw `font-copy-dispatch-trial.tar.xz` retains 72 members,
1,582,004 bytes, SHA256
`a10630a2ecdd1ff3757f9fddf55029d5fe45eef7eaedd24ba3b92713dfedb54c`.
Its `fcp1167-*` scripts and declared archive dependencies reproduce the round.
No current displayed costs change; this rejected trial is not a renderer input.

Keep both original copies. Further font-copy work needs distinct attributable
work, rather than adjacent kernel variants. Next is the independent ordinary
RGBA8 drawing gap; unchanged passing integrations remain stopped. The finite
289-path objective stays open, with publication postponed.
