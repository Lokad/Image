# Enlargement vertical geometry reuse: rejected

The final primary gain is **1.17%raw/all**, **1.73%native-normalized**, below the
4.1%retention gate despite separated confidence. Corrected TightUp8 independently
measures4,420.825µs,1.448×/upper1.462,unchanged2,132,668B/op. It is rejected;
accepted production and its4,472.999µs/1.473×/upper1.481cost remain unchanged.

The initial producer passes the short and independent primary:7.19%filtered/
6.19%all/7.38%normalized reduction at4,151.425µs. Its horizontal-only companion
regresses from1,644.062to1,758.905µs; it cannot use the plan but calls an extracted
helper every row. One correction restores the existing inline cardinal shortcut.
Horizontal returns to1,637.495µs,1.076×/upper1.088,1,064,526B/op, but primary
no longer clears retention. Initial vertical2,057.478µs remains historical.
No further layout,threshold,unroll or precision variants follow.

The trial stores exact vertical geometry in56private bytes at each row's end.
Only fully disjoint column strips read them; original geometry handles the tail
and overwrites every cell before tiny-alpha replay/publication. No new buffer,
owner,quota,capacity or numerical tolerance. A bounded proof gives74→3geometry
passes and153,698→6,231kernel evaluations for the primary. This structural saving
does not establish acceptable complete-operation performance across the controls.
All frozen pixels/native bounds/source/owners/quiet checks pass.

Each producer passes34internal/549public checks normal/scalar on bothOSes plus
LinuxAVX512-off. An initial Windows5,333full/live run and actual package/three
examples pass before the companion regression is resolved; their distinct
producer/package are retained as historical proof, never current qualification.
No Linux initial full/package or corrected full/package run follows the failed
gate. Two core/internal builds perOS; two public builds include a test-only
reference correction, and the corrected producer reuses that public assembly.
No harness/native/selector build,profile or new benchmark input.

Original source,core/PDB/XML,internal assemblies and development package are
restored byte-exactly on bothOSes without rebuilding. Five useful public tests
remain: exact complete frames at the boundary/odd tails,tiny-alpha publication,
shared refusal/cancellation/reuse and independent survivors. All549focused public
checks pass on restored production in each required mode. Three prototype-only
geometry helper tests are removed with their implementation. Prior5,314checks/OS
and the same actual package proof carry forward with12public+4internal tests
separately qualified:5,330applicable,no fresh full5,330claim. Published releases
are unchanged; development package remains
f339e08d6f4541ee278349fe8f9dbfe65150b00d2a3c545716819f7801d34640.

[Numeric evidence](enlargement-vertical-geometry-rejected-evidence.json) preserves
all seven original/initial/corrected jobs, decisions, native brackets and guards.
Mirrored/member-verified raw enlargement-vertical-geometry-rejected.tar.xz SHA256
880c5effd997d743e65bced76c6be6cafc3091331c443002779f8f7b0dd3902d (9,150,116bytes/349members) retains source,
original/initial/corrected binaries,actual package,fixtures,workers,selectors,
tests and qualification/restoration logs. All268required paths remain in scope.
