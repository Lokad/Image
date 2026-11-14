# JPEG marker preparation

Complete sync/async probes and full header inspection improve without additional
storage. Only certified buffered FF/non-FF header pairs bypass async setup. Filler,
partial reads and errors use the original shared marker parser; its entropy loop,
acquisition, cancellation, admission and disposal stay exact. No cache or public API.

| Original complete operation, scoped managed DLL pair | Prior → current µs | Ratio (interval) | GC B/op, unchanged |
| --- | ---: | ---: | ---: |
| ProbeDimensionsJpeg | 0.302 → 0.259 | 0.859 (0.803–0.919) | 632 |
| ProbeDimensionsJpegAsync | 0.384 → 0.343 | 0.895 (0.858–0.934) | 632 |
| InspectHeadersJpeg | 6.463 → 6.012 | 0.930 (0.922–0.938) | 8800 |

Twelve alternating pairs retain every observation with natural GC. Probes use
262,144 calls per subject; the full-header guard uses 16,384 after 4,096 warmup calls.
All use the original 513×259 baseline420 JPEG, fresh resident source, actual public
parsing, independent descriptor and source/workspace disposal. Full headers include
framing, entropy and EOF, with permitted seek skips. Source bytes and complete
descriptors/lifetimes match. These paired effects are not FFmpeg costs.

Independent original-reference confirmations give JPEG probe **0.260 / 0.194 µs,
upper 1.387**, and immediate-async **0.330 / 0.205 µs, upper 1.650**. Both remain
gaps to the explicitly labeled **FFmpeg AVIO + bounded header adapter**, not its
public codec parser. The unchanged PNG/WebP probes and PNG headers reuse
[probe preparation](PROBE-PREPARATION.md); this is not a release-wide timing refresh.

Dimension admission remains 512 B with 256 B scratch; full headers admit at most
8,448 B. Allocations, admitted capacity and zero retained operation storage are
distinct; physical/native/JIT peaks are unmeasured. Async clocks use immediate
callbacks. Four added held JPEG prefix/code tests prove that cancellation/failure
cannot release admission until an uncooperative read finishes, then prove recovery.

Each OS passes **6,696 full/live checks** (3,263 internal/2,485 public/948 live),
plus focused 2 internal/143 public normal/scalar checks. The **same actual Release
package and all three examples pass Windows/Linux**. Two core/embedded builds per
OS cover A/B, one public test build per OS and four tool-only helper builds;
zero harness/native/fixture builds, one pack and no post-timing producer rebuild.
Unchanged arithmetic/ISA and codec/edit/resize/text evidence is reused.

The first factored implementation confirmed small probe gains but its header
guard remained inconclusive with an adverse point. It was revised before broad/
package qualification; all results remain, without a causal regression claim.
B restores the complete parser and increases header integration before measurement.
The attribution capture includes startup/JIT; its samples/code sizes are leads,
not removable phase costs. Failed tool-only validation guards and the corrected
test-log parser are retained; no successful build or measurement was repeated.

[Evidence](jpeg-marker-preparation-evidence.json) pins sources, outputs, references,
all pairs and qualification. Timed DLL `ab6c6f8309f5693ce5bfc346c2bc379a96127038758068607e104b3201850423`;
actual private package `c5036f1c5a298d744bb933841c520d01ce37272e5c3eec77c7a9836cc665de27`, 461,881 B.
Raw `jpeg-marker-preparation-qualified.tar.xz`: 6,687,680 B/391 members,
SHA256 `f89171c3959b9c0f08ac37561691586a49f7589f61bbf2356cc9f98d76e9cae0`, mirrored/member-verified. Dependencies retain original
fixtures/tools, preceding package and attribution. The full 289-path objective
remains open; publication is postponed.
