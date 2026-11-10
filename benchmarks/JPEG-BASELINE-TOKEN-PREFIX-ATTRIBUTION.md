# JPEG baseline token-prefix attribution

JRA762 is read-only attribution on the current qualified package. Complete direct
RGB444 decode remains **1,616.02/native1,559.46 µs**, upper1.055 confirmed gap,
42,936 B/op. Relevant codec sources are unchanged. No new build, timing claim,
benchmark image/settings, full suite or package proof; current 6,167 full/live
checks and same-package Windows/Linux qualification are reused.

One two-second CPU0 capture has 1,998 sampled locations: plain baseline block
reader830, inverse445, baseline scheduler78, ordinary AC54, table preparation46,
dequantization41. CPU1 samples; idle-v2 and frozen input/exact native/managed
531,468 B output gates pass. Instrumented elapsed is discarded. Locations include
inlining/skid; these are not phase costs or removable percentages. Actual code
shows pair descriptor dispatch/stores and repeated canonical fallback lookups.

## Bounded design lead

Keep the existing scan-owned 4,096×UInt32 table and pair priority. Fill unused
entries with tagged single-amplitude/run/width or EOB descriptors. This can remove
a second canonical lookup and repeated sign interpretation at those misses.
No added capacity, pool, root, session, API or general cache. Keep current table
eligibility, reservation-before-allocation and explicit scan release.

An independent T.81 canonical model checks descriptor values and consumed bits
against both existing complete streams, including final padding/EOI:

| Existing packet | Old logical lookup iterations | Pair hits | New single entries used | New EOB entries used |
| --- | ---: | ---: | ---: | ---: |
| Direct RGB444,133,057 B | 124,521 | 102,219 | 16,943 | 2,788 |
| Sparse CMYK,22,708 B | 17,160 | 8,580 | 0 | 8,579 |

RGB has22,301 modeled misses;19,731 can use a preinterpreted entry. Remaining
misses include1,830 canonical cases and740 pair-boundary cases; one final logical
short tail stays unchanged. These counts ignore actual DC/refill/stuffing/window
handoffs. They are logical coverage, not measured execution counts, CPU gains or
general content distributions. The final two-byte prefix alignment was corrected
after a model assertion; the earlier script and both complete corrected checks
remain. No production/benchmark failure occurred.

## Acceptance and transition

Both actual plain/cancelable table users must recognize tags. Preserve pair order,
DC predictors, every coefficient/cursor/following read/partial store, malformed
input behavior, long/ZRL/truncated fields and existing cancellation checkpoints.
Near-end pair misses keep the original grammar. Admission refusal remains terminal;
all scan-owned storage retires after actual users, with no changed ownership.

Extra tag discrimination on existing pair hits and more preparation work are
potential regressions. Start with actual compiled-descriptor/canonical checks and
existing sparse/RGB complete screens, then independent confirmation if useful.
Changed preparation/reader bodies can affect pair-null validation and live/
cancellable callers too: reconcile the current baseline inventory before acceptance.
Preserve progressive costs only when relevant code is unchanged. Broad Windows/
Linux/scalar/ISA/same-package checks belong at a coherent accepted milestone.

The [earlier coefficient model](JPEG-COEFFICIENT-PIPELINE-ATTRIBUTION.md) and
[unselected fusion implementations](JPEG-COEFFICIENT-FUSION-EXPERIMENT.md) retain
context. This lead removes different repeated work; it is not a replay or merge
authorization. There is no fixed continuation threshold or permanent family ban.

[Evidence](jpeg-baseline-token-prefix-attribution-evidence.json) records identities,
source reuse, model, capture and acceptance boundaries. Mirrored/member-verified
raw `jpeg-baseline-token-prefix-attribution.tar.xz`: 302,592 B/33 members, SHA256
`a4a6fc51e9f2f02498aa48e9cc9e35761d2b61743522f187cb5c47b35ed88c56`. It depends on retained current-package and earlier JPEG archives.
No renderer dependency is added because this stage supplies no displayed cost.
All283 required paths remain; publication stays postponed.
