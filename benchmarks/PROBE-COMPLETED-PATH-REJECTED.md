# Probe immediate completion: rejected

Splitting completed parsing from the outer async state machine misses its short
screen: 2.668% filtered / 2.636% all-actual / 1.740% native-normalized point gain,
below the 4.1% gate. Allocation remains 744 B/op. No confirmation or variants.
Production source/core/PDB/XML are restored exactly on Windows and Linux.

| Existing async JPEG dimension probe | Managed µs | Ratio / upper | Allocated B/op |
|---|---:|---:|---:|
| Original short | 0.398361 | 1.964 / 3.967 | 744 |
| Rejected short | 0.387732 | 1.930 / 2.520 | 744 |

The original nine-field, 36-byte descriptor, encoded input, acquisition/prefix
checks, complete owned-operation boundaries and native worker/settings remain.
All actuals, native brackets and idle-v2 guards pass. These three-actual screens
establish no causal gain or parity. No pixel decoder or codec behavior changes.

The existing independent baseline remains applicable: ten relevant production/
harness source files match SKP1 exactly, and the current original short is within
5% filtered/all/native-normalized of its retained twelve. No new original twelve.
Current benchmark summary rows and the full 268-path objective remain unchanged.

Three useful tests against the actual public API are retained: immediate fault/
cancellation status and token; a pending read that ignores cancellation until it
finishes; and a late stream failure after cancellation. Admission stays charged
until the actual read completes, cleanup releases it, caller stream/root remain
open and a later operation succeeds. Candidate and restored original each pass
2 internal/96 public checks normal/scalar on both OSes. No obsolete helper or API
is retained. The completion-wrapper/inlining family is closed.

One core and one successful public-test build per OS; no other builds or new
controls/settings/profiles. An initial test-only analyzer error was fixed by
awaiting a completed ValueTask; the immutable production builds were reused.
The unchanged 5,342-check/OS, same actual Release-package/three-example proof
carries forward alongside the three new focused contracts. No full/package repeat.

[Evidence](probe-completed-path-rejected-evidence.json) retains source/pin
comparisons, all samples, failures and restoration. Mirrored/member-verified raw
`probe-completed-path-rejected.tar.xz`, 5,276,488 B / 146 members, SHA256
`58165144cef5e2d24e6095d96a9bde9a6542fec4a04901eca690ed5d5bb44337`.
Retained J4N31/SKP1 archives supply unchanged producer/package/worker provenance.
