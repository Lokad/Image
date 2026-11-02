# Complete one-shot PNG comparison

The two existing RGBA8 controls now compare fresh encoder state on both sides.
Native timing includes codec/options/zlib setup, a fresh packet wrapper, complete
encoding, independently owned output and packet/context disposal. Both sides
borrow their resident source image; neither adds an unnecessary input copy.

| Frozen complete encode | Lokad µs | FFmpeg µs | Ratio / upper | Managed allocated B/op | Complete bytes, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Optimal6 / native6 | 24,970.38 | 18,944.11 | 1.318 / 1.356 | 1,325,732 | 210,973 |
| Fastest1 / native1 | 6,589.28 | 4,649.33 | 1.417 / 1.458 | 1,344,428 | 236,828 |

Each has one three-actual screen followed by an independent twelve-actual
confirmation, twelve native samples before/after, one pinned CPU and passing
quiet guards. Both are confirmed CPU gaps: conservative lower ratios are
1.282 / 1.331. This corrects a comparison boundary; **it is not a managed speedup**.
No production code, settings, input or complete packet changes.

The earlier retained-context comparisons remain in the
[Optimal coverage](current-png-encode-coverage-evidence.json) and
[Fastest evidence](warm-png-distance-symbol-evidence.json), with their original
producers and actuals. Those native workers retain/reset codec/zlib and packet
wrappers. Do not subtract older elapsed numbers to infer setup costs; the runs
were independent and clocks differ. Other displayed PNG encode comparisons
retain those explicitly labeled boundaries.

A separate instrumented native adapter observes public context/open/packet
preparation around 4.3–4.4 µs and disposal around 77–79 µs after validation;
the first diagnostic preparations take 70–75 µs. Its encode/output phase is
about 19.7 / 5.2 ms and includes per-frame working storage and first-use effects,
not just arithmetic. All twelve diagnostic observations are retained; the first
three held-output validation calls are separated from the following nine.
These phase observations are attribution, not complete-operation parity evidence.
Pinned FFmpeg PNG initialization invokes zlib initialization; frame completion
resets it. All necessary first-use work remains inside the one-shot operation.

The unchanged managed Deflate sources reuse the earlier
[workspace attribution](PREPARATION-WORKSPACE-ATTRIBUTION.md): 659,576 B and
12–26 µs of construction against 7–25 ms complete encodes. Reuse could reduce
allocation; neither this attribution nor the new comparison establishes a CPU
benefit from a managed session. Defer that public API while the demonstrated
millisecond gaps and shared resize execution remain higher-value work.

The original adapter is compiled with an explicit one-shot flag into a distinct
worker/identity. Normal/scalar native preflight preserves complete packet hashes,
exact pixels/alpha/hidden RGB, borrowed source and held outputs surviving encoder
disposal. A separate default build passes both controls in normal/scalar modes.
Existing scored workers/identities remain byte-for-byte unchanged. The harness
rejects this option outside the two existing RGBA8 controls and checks the
reported preparation/lifetime boundary before measuring.

Production DLLs, package and published releases are unchanged. Reuse the accepted
5,411 Windows/Linux checks and same actual-package consumer qualification from
the [trace-storage stage](VP8L-TRACE-FORWARD-WINDOW.md), plus unchanged
[PNG packet/scalar evidence](png-byte-parity-evidence.json).
The portable harness was built on Windows and reused on Linux; no production,
Linux harness or selector rebuild was needed. Publication is postponed.

[Evidence](png-one-shot-boundary-evidence.json) retains source/producer identities,
all screens/confirmations, phase observations, original comparisons and raw
reproduction. The main summary replaces these two controls without adding a
shape/depth/filter/settings grid. Full performance parity remains open.
Raw png-one-shot-boundary.tar.xz: 10,799,704 B / 196 members, SHA256
270931e460dca327c115652edf615516ab344925bd2cef5e2fcbabda578241fe,
mirrored/member-verified. The unchanged production/package proof depends on the
retained trace-storage archive identified in the evidence.
