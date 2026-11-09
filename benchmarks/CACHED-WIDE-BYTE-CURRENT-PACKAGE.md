# Current cached wider-support byte resize

Retained pre-change baseline. [Shared pixel preparation](CACHED-WIDE-PIXEL-PREPARATION.md)
owns current production costs and qualification.

WCR653 confirms the existing default 513×259 → 128×64 RGBA8 bicubic control on
the actual qualified package: **498.26 / zimg 347.09 µs**, ratio **1.436**,
conservative interval **1.356–1.444**, a **151.17 µs** gap. Allocation is
**198449 B/op**. Twelve actuals and both native brackets pass idle-v2 guards.

The displayed older 493.82/347.10 µs and 231241 B observation dates from
2026-11-01. Commit d7fcbd41 (2026-11-03) changes filtered output to complete-overwrite
storage; 9776115e (2026-11-04) changes cached geometry/workspace/row scheduling.
Those observations cannot qualify current complete costs. This refresh establishes
no causal CPU gain or regression against that history.

Timing includes the complete public resize: normalization, axis preparation,
working storage, independently owned result and disposal. The reference remains
explicitly zimg, FFmpeg's scaler backend, retaining graph/scratch before timing.
Original inputs/settings, exact rational/frozen managed frames, native max1/
RMSE≤0.25, transparent RGB and source/output ownership gates remain unchanged.

The result retains 32768 B until disposal; the cached path estimates a 198432 B
workspace reservation. Allocation is distinct from admission or physical peak.
No new storage or retention policy is introduced; existing idle pool capacity
and physical/native peaks are not newly measured.

The timed DLL596a5be7 is the same actual private Release0.2.1 package2db7ad49
already [qualified on Windows/Linux](PACKED-BICUBIC-DECODED-PREFIX.md), including
5985 full/live checks, scalar/ISA checks, package consumers and three examples.
There are zero builds, new inputs/profiles/settings or repeated suites/packages.
Publication is postponed and full 283-path parity remains open.

[Structured evidence](cached-wide-byte-current-package-evidence.json) identifies
current costs, source invalidation and reused qualification. Mirrored,
member-verified raw cached-wide-byte-current-package.tar.xz contains 6144932 B /
116 members, SHA256
5b4b87ef967acc01b6cc22d50c1a0a61724aa399790bb6eba48b1cfa345b1eee.
It retains runtime, scripts, observations and guards, with the actual-package
archive pinned as a dependency. The next bounded attribution distinguishes
preparation/storage from repeated horizontal interpretation, filtering and exact
repair; no new design is accepted by this measurement.
