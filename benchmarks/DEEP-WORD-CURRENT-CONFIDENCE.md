# Current bounded word reduction confidence

Current follow-up: [qualified horizontal views](DEEP-WORD-HORIZONTAL-VIEWS.md).
Costs and next actions below retain their original checkpoint.

The existing 513×259→32×16 RGBA16 bicubic control now has a **confirmed gap**:

| Complete operation | Lokad µs | Prepared zimg µs | Ratio / conservative range | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Public Resize, input + output + 4 KiB | 444.73 | 411.71 | 1.080 / 1.065–1.091 | 112 |

This read-only measurement resolves the former uncertainty; it is no production
regression or speedup claim. The earlier accepted 112.44 µs/20.30% raw and 14.21%
normalized gain remains [separate evidence](DEEP-WORD-VECTOR-WEIGHTS.md).

One existing driver measures twelve alternating adjacent managed/native pairs,
64 complete operations per subject and twelve native batches before/after.
Native bracket means are 411.71/412.94/412.15 µs. All actuals remain; conservative
confidence includes paired log ratios and all-actual/diagnostic managed confidence
against the fastest native bracket. This changes adjacency from the old global
brackets, without changing acceptance thresholds or repeating a successful job.

Managed preparation, exact filtering/repair, admission, independent output and
disposal remain timed. Explicitly labeled zimg retains its graph/scratch and times
source normalization/halo/filtering/output acquisition and release. This is the
original asymmetric boundary, not equivalent fresh graph preparation. Frozen
input/settings, exact rational/qualified managed samples, approved native-only
bounds, source immutability and surviving owners all pass. Managed output SHA256
remains `56948d8eacf081102f915276cdbd5daac3b2414f07ac93e3cf9f91fac5e5f053`.

Direct allocation counters give 112 B/op rather than the earlier BDN 113 B/op;
this is an instrumentation difference, not a saving. Explicit arrays remain
4,032 B under the unchanged 4 KiB preadmission, with zero retained operation
scratch. Physical/native/JIT peaks are unmeasured. No production, test, driver,
harness, fixture, native or package build is needed. Reuse source-valid Windows/
Linux 6,512 full/live checks and SAME actual-package consumer qualification.
The timed DLL is exactly the current development package DLL; released packages
are unchanged.

[Evidence](deep-word-current-confidence-evidence.json) pins current package,
sources, workload, driver and all checks. Mirrored/member-verified raw
`deep-word-current-adjacent-confidence.tar.xz`, SHA256
`d9bdb5386a5ca4749a713ba86a19b143651c3b30fdff4936a720f4ae93046fd3`
(386,308 B / 23 members), retains reproduction and historical dependencies.
It also retains an aborted duplicate opaque-WebP attempt: case binding failed
before setup/timing, after which existing OWC838 evidence was reused.

Current resize scope is 73 controls: 64 pass, one gap and eight uncertain.
Attribute the demonstrated 33.01 µs excess on the actual current package before
selecting a redesign. Stop passing controls; preserve exact pixels, memory,
cancellation and ownership. All 289 required paths remain; publication stays
postponed.
