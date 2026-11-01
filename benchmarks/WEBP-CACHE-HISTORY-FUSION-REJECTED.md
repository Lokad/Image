# Fused WebP cache histograms: rejected

The existing RGBA8 VP8 control is **2.34% slower** in one short screen
(2.24% native-normalized), with unchanged allocation and an exact packet.
Reject without confirmation or variants. These point means do not establish an
independently confirmed regression or change accepted production costs.

| Complete original 513×259 encode | Lokad µs | Ratio / upper | Managed B/op | Packet B |
|---|---:|---:|---:|---:|
| Reused original, independent12 | 69,770.340 | 1.298 / 1.307 | 21,353,208 | 98,590 |
| Candidate, short3 | 71,400.079 | 1.327 / 1.883 | 21,353,208 | 98,590 |

A six-second trace on the current DLL has 5,937 samples: match-index construction
798 (13.4%), cache-history construction 234 (3.9%) and histogram replay 285 (4.8%).
Samples are attribution, not phase timings or predicted gains. Mapping the same
trace to the exact 2,792-byte match-builder JIT body finds insufficient removable
headroom for a comparison-only SIMD trial; none was built. The trace and untimed
JIT replay reuse cached hosts, harness, fixtures and workers, with zero builds.

The single prototype instead combines cache-history construction and both
read-only seed histograms in one pixel pass. It reuses the existing byte workspace
as the second integer histogram when it fits. The original small-input path,
full owned capacities, admission/reservation, terminal refusal, cancellation,
cleanup, population costs and strict cache-width ties remain. Native drift is
+0.09%. Exact complete packets, reconstructed pixels/alpha, frozen settings,
source/output ownership and both idle-v2 guards pass.

Candidate **114 internal/392 public** checks pass normal/scalar Linux, including
2,048 independent cache-choice pairs, 24,558 frozen histograms and three new
workspace-cutoff/shared-refusal/recovery cases. One core/internal build; zero
public/harness/native/selector or Windows builds. No new performance inputs.

Source, core/PDB/XML and every saved linked test binary are restored
**byte-exactly**, without rebuilding. The prototype-only cutoff cases and fused
oracle extension leave with it; restored **111 internal/392 public** pass in both
modes. Windows and the actual Release package are unchanged. Applicable
**5,369 checks/OS**, 948 live comparisons and the same actual package/three examples
remain valid, without full/package repeats. All 268 paths remain required;
applicable passing cases stay stopped. No full parity claim follows.

The console collector label says 12; the retained actual log, statistics and both
native brackets contain 3, as the measurement gate requires. A profile collection
filename error was recovered by fetching the completed data, without resampling.
Neither correction changed a score or weakened a gate.

Close this fused-history/coupled-histogram family. [Evidence](webp-cache-history-fusion-rejected-evidence.json)
retains scope, decision, qualification reuse and exact restoration. Mirrored/member-verified
webp-cache-history-fusion-rejected.tar.xz, 2,738,816 B/85 members,
SHA256 f5eeb1d4ac6010d8eceed8e1c6ded2ac0d028573bcc7a396077c756a5c93b129.
The pinned current-profile dependency is current-vp8-alpha-attribution.tar.xz,
SHA256 bc83587d79839a5b2784ea24540cc4a231d162e97d99abce3e781533c8d25b9c.
Raw retains actual binaries, source, tests, samples/maps, guards, receipts, cached
workers/selectors and the qualified/original-confirmation dependencies.
