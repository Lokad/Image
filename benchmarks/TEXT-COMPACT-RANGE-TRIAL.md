# Compact text range trial

**Not retained.** Outward-rounded float bounds halve the temporary rejection
hulls from 16 to 8 bytes while all coordinates, events and coverage remain double.
The wider hulls conservatively preserve exact events and pixels. Conversion and
less selective rejection can cost CPU; lower allocation alone establishes no gain.

| Twelve adjacent managed DLL pairs | Ordinary RGBA16 µs | Public GC B/op | Preparation peak B |
| --- | ---: | ---: | ---: |
| Qualified producer | 55.040217 | 83,136 | 58,264 |
| Compact temporary ranges | 56.417130 | 79,552 | 54,680 |

The candidate costs **2.50% elapsed**, paired interval **1.009908–1.040335**;
all-actual mean intervals also separate. Allocation saves 3,584 B / 4.31%; retained
Latin plan capacity remains 48,984 B and prepared drawing adds no admitted capacity.
The timed adapter adds 24 B on both sides. Other frozen-case capacity is recorded
separately; full-pipeline allocation/peak and physical/native/JIT peaks are unmeasured.

The unchanged prepared helper runs original public ordinary drawing in two
independent processes, 12 alternating 1,024-operation pairs, 1,024 warmups each,
one CPU 0 operation at a time and natural nonconcurrent GC. Canvas reset, output
validation, IPC and allocation reads remain outside each operation's two clocks.
Every received line is durably captured before statistics. All 12 pairs are retained.
Original ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12 checks run without
reference timing. This is a scoped managed comparison, not native parity or an
overall text slowdown. No historical stage sum. The existing complete one-shot/
eight-draw public integrations remain passing after exact restoration.

Linux passes 266 internal / 161 public checks and all eight frozen full-word frames in
normal, AVX-disabled and scalar modes. Outward enclosure is checked against the
actual production edge, independent original all-pair event oracle, 40,000 seeded
pairs and floating-point boundary cases. Every-growth admission refusal, cancellation,
cleanup and recovery pass. No new owner, cache, pool or retained state.

Qualified source, tests, core/embedded/public modules and source inventory are
exactly restored without rebuilding. Windows modules and the actual package
never change. Reuse 6,728 full/live checks per OS and the same actual-package/three-
example proofs. One Linux core/embedded build; zero other builds. No independent
confirmation, codegen, extra timing or broad/package campaign follows.

[Evidence](text-compact-range-trial-evidence.json) pins every observation and the
restoration. Raw `text-compact-range-trial.tar.xz` (1,812,888B/98members), SHA256
`e8cedfce1bf7e999033050130e634d330dd750c50fb27cba175bacd52f429030`, is mirrored and member-verified with its qualified-baseline
dependency. The current-cost renderer has no dependency on this rejected trial.
Continue independent codec/resize/ownership work; publication/full289-path
completion remain open.
