# Current PNG16 stream confidence

The complete PNG16 stream decode **passes and stops** on the current immutable
package DLL. This resolves uncertainty without a production change or speedup claim.

| Existing 513×259 RGBA16 operation | Lokad / FFmpeg µs | Conservative ratio interval | Managed allocated B/op |
| --- | ---: | ---: | ---: |
| Fresh non-exposed seekable stream, complete decode/dispose | 3,106.21 / 3,349.15 | 0.917–0.934 | 8,664 |

Both sides include fresh input acquisition/copies/EOF, full decode/normalization,
independent output ownership and disposal. Managed creates a new non-exposed
MemoryStream; native creates AVIO/scratch/packet payload and converts complete
RGBA64BE to independently owned RGBA64LE. FFmpeg retains its decoder/frame/packet
wrappers, as before. Source input, reference settings/identities, exact full-word
pixels, alpha/hidden RGB, stream ownership and independent-survivor checks remain.

The older bracketed capture has a chronological managed step near 2.81→3.30 ms;
its complete 3.180 ms mean and uncertain bound remain. A separate two-managed-DLL
pair remains inconclusive. The new measurement changes **adjacency**, alternating
12 managed/native pairs of 64 complete operations, plus 12 native batches before
and after. Every actual and the same conservative paired/three-bracket confidence
rule remain. Native clock movement versus history is not a CPU gain. No unchanged
favorable repeat follows. Quiet CPU0, Release optimized JIT, nonconcurrent GC and
both idle-v2 guards pass. All original correctness and ownership gates pass.

A separate two-second read-only current-package profile finds 1,656/1,996 sampled
locations in InflateCursor and one in input acquisition. Inlining/skid/unmapped
work prevents phase-time or removable-percentage claims. It does not establish
the earlier step-change cause; instrumented elapsed is discarded. Earlier cursor
experiments stay retained rather than being replayed.

Allocation is **8,664 B/op**. No capacity or retention changes. Encoded input is
960,565 B borrowed; its owned input bucket is 1,048,576 B and result is 1,062,936 B.
Their 2,111,512 B source-derived sum excludes other workspace, caller storage and
replacement overlap; it is not measured peak admission or physical storage. The
two-slot/4 MiB pixel store and caller-root bypass are unchanged. Actual native/JIT/
physical peaks and idle payload are not remeasured. Retained operation scratch is zero.

One private driver build adds only WarmStreamDecode to the existing harness
allowlist. Production, embedded tests, original harness, fixtures and reference
worker are reused. The timed DLL remains in the same qualified actual package;
reuse 6,624 full/live checks per OS, focused scalar/ISA and SAME-package consumers/
all three examples. No broad/package repeat. Current image views are **174 pass,
one gap, 16 uncertain**; synchronous stream views have six passes/two uncertainties.
Composition and VP8 stream confidence remain open. All 289 material paths remain
required; publication stays postponed.

[Evidence](png-stream-word-current-confidence-evidence.json) preserves identities,
all actuals, validation and reproduction. Raw `png-stream-word-current-adjacent.tar.xz`:
107,852 B / 36 members, SHA256 `896829bd44cfcf48f707ee1984dadae7aec366bc1179737ccda8e592872379bb`,
mirrored/member-verified. Dependencies retain original fixtures/workers, all
adverse/uncertain comparisons, packages and qualification.
