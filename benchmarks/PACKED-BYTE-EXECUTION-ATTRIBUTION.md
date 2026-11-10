# Current packed-byte execution attribution

Historical PRV733 state. The [current preparation profile](PACKED-BYTE-PREPARATION-ATTRIBUTION.md)
supersedes its instruction/sample attribution after the contiguous helper changed.

BPA734 uses the immutable package from [shared byte/word execution](PACKED-BYTE-WORD-CONSOLIDATION.md).
The existing complete RGBA8 513×259 → 64×32 control remains 485.70 / native
367.42 µs, a demonstrated 118.28 µs gap. One two-second CPU0 capture with CPU1
sampling changes no production source, fixture or setting. All instrumented
elapsed values are discarded. Exact managed rational/frozen pixels, original
native bounds, input ownership, independent output and idle-v2 guards pass.

The 1,993 sample locations include 1,017 in packed suffix execution, 827 in decoded
prefix execution and 135 in Kernel. The perf-map display names omit generic mode
arguments: correlating actual addresses and emitted code sizes distinguishes the
5,420-byte suffix from the 6,001-byte prefix. Combining their instruction offsets
would be misleading. Locations, inlining and sampling skid are not phase costs
or removable percentages.

Actual suffix code still reads two offset/weight streams, widens packed UInt16
products and checks decoded ranges each tap. Prefix code removes that widening,
while retaining source/product preparation and range checks. No span-view
conversion call samples remain. These are code-generation leads, not new speed
claims or evidence that exact arithmetic can be weakened.

A bounded source-only geometry model finds 30 of 32 adjacent pairs eligible for
contiguous sample views on both existing depth controls, including 32- and 33-tap
cases. The two clipped-edge pairs retain the existing path. Indexed and contiguous
source positions agree for every eligible modeled tap. The next coherent candidate
can prepare those views once per decoded row under a once-per-strip proof and
share variable-length ordered pair execution. The existing fixed 8/+2 and 16/+4
overlap helper cannot serve this geometry unchanged; preserve its measured uses.
No new fixed-support class, changed coefficients, reordered sums, additional
capacity, cache or numerical policy is proposed. Applicability is not a CPU gain.

Build only after a credible emitted-code/storage case; use focused exact-pixel,
ownership and scalar checks, then the existing short complete byte control.
Confirm a promising completed stage, refresh the affected word control and batch
broad/package qualification at an accepted milestone. Independent codecs and text
remain available. No favorable retiming of unchanged candidates is justified.

[Evidence](packed-byte-execution-attribution-evidence.json) retains the hypothesis,
instance attribution, model and identities. Reuse the unchanged 6,147 full/live
checks per OS, scalar/disabled-ISA and SAME actual package/three-example evidence:
no production/test/harness/native build or repeated package proof. This report
adds no renderer dependency or displayed result. The compact current packed-cost
provider links full transition/profile history through its raw archive instead
of repeating the instruction-offset map.

Mirrored/member-verified raw `packed-byte-execution-attribution.tar.xz`:
SHA256 `e71b5a8195248b970386f5fb69f34307acc096a0363abdf9b3b97b4ee0e8fcd5`,
96,848 bytes / 23 members. Original scripts, samples, perf map, actual JIT,
guards, receipt and model are retained; the evidence pins the qualified dependency.
