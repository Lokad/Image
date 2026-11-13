# JPEG inverse workspace study

The standalone workspace rewrite is **deferred**, with the exact accepted code,
test assemblies and package restored. It is a verified small CPU lead, not a
failed correctness implementation or a native-parity result.

The actual optimized wrapper cleared two 128-byte stack buffers per block.
One 256-byte borrowed stack workspace per existing block walk removes those four
64-byte stores and reduces wrapper code from 495 to 410 bytes. This authored
513×259 four-component control has 8,580 blocks: 2,196,480 bytes of repeated
clearing removed. Code size and traffic do not predict whole-operation savings.

One controlled complete public Decode comparison retains all 12 alternating pairs
of 128 operations: **398.55→395.75 µs**, a **2.80 µs / 0.70%** point gain;
ratio interval **0.9895–0.9964**. Input, normalization, independent outputs,
checksum and disposal are included identically. Exact pixels and surviving owners
pass. Native costs/parity are not established by this managed pair. The preceding
three-actual screen is inconclusive and retained; no favorable repeat follows.

Allocation, admitted heap capacity and retained storage do not improve. The added
workspace type and plumbing reach every baseline/progressive/caller inverse walk.
That scope is not justified as a standalone stage by this modest absolute saving.
Keep the lead for a broader reconstruction redesign; no fixed percentage threshold
or permanent algorithm-family restriction follows.

Windows/Linux each pass 464 internal and 342 public JPEG-focused checks in normal,
scalar, AVX512-off and AVX2-off modes. Poisoned reusable storage preserves DC,
low-support, dense and scan-order matrix pixels, guards and pre-mutation cancellation.
One core and embedded build per OS; zero public/native/harness/fixture builds.
All instrumented clocks are discarded. Every capture passes both quiet-v2 guards.
No broad or package qualification follows this unaccepted stage. Restored modules,
embedded tests and actual package are byte-identical to their qualified checkpoint.

The next hypothesis combines baseline coefficient output and dequantization.
Current direct reconstruction clears/stores 64 Int32 coefficients, then reads and
dequantizes all 64 into signed words before evaluating the inverse. Investigate
one shared entropy traversal with a typed destination for direct reconstruction;
keep actual Int32 coefficient observation and progressive paths. Assess plain,
cancelable, prepared-pair and canonical continuation writes, exact modular AC /
full-width saturated DC products, error cursors and cancellation before coding.
No implementation, saving or acceptance is presumed.

[Evidence](jpeg-inverse-workspace-study-evidence.json) pins both immutable subjects,
code inspection, samples, checks and restoration. Raw `jpeg-inverse-workspace-study.tar.xz`,
SHA256 `dd308939d116fb86159db2d5944b20bcb5af13faa4a33b3d6a2393aa9489490b` (1,413,640 B / 96 members), is mirrored
and member-verified. Its dependency preserves the unchanged pair tool, original
reference workers, frozen packets and qualified package. Current production costs
remain in [the CMYK owner](CMYK-CURRENT-OWNED-CONFIDENCE.md); all 289 paths and
compression/pixel/ownership requirements remain in scope. Publication is postponed.
