# WebP storage lifetimes

Two complete existing encodes reuse the qualified immutable DLL, original inputs,
settings and exact packets. A private caller-side decorator records ordered shared-
admission deltas and codec stacks; no production instrumentation or rebuild.
Both quiet guards pass. These are coarse capacity lifetimes, not CPU or actual
allocation measurements; callback diagnostics perturb execution.

| Complete encode | Dense heads, bytes × calls | Next greedy buffer, bytes each | Admitted peak owned B | Retained root B |
| --- | ---: | ---: | ---: | ---: |
| Lossless photograph | 1,048,576 × 1 | 1,062,936 | 12,217,052 | 0 |
| VP8 with lossless alpha | 1,048,576 × 2 | 1,062,936 | 4,085,883 | 0 |

Every dense table ends immediately before greedy parsing allocates its ordinary
token buffer. The independent match index stays live; selected token candidates
keep their own ownership. Merely retaining a separate heads array across parsing
would add live capacity during later phases.

A coherent alternative moves the existing token owner earlier, borrows an int
view of its unused capacity for matching, then overwrites it with tokens only
after matching finishes. That could eliminate 1 MiB / 2 MiB of separate array
payload capacity on these controls. Actual allocation and CPU gains are unproven.
An event-by-event admission model preserves both full-operation peaks while raising
the local matching-stage capacity by 14,360 B. This model is not an implemented
peak guarantee or a measured saving.

The next candidate must select reuse solely from actual capacity, preserve the
existing path when a token buffer cannot cover the dense table, and never fall
back after refusal. It must keep exact packets, count/order/ties, independent
selected candidates, reserve-before-allocation, prompt exception cleanup and
bounded cancellation. Use the existing token owner; no pool, additional governor,
general cache or cross-operation retained state. Measure real complete costs,
allocation and admitted peak before acceptance.

The two packets remain exactly160,032 / 98,590 bytes. Source identity, borrowed
root and final cleanup pass; 1,101 / 779 ordered events are retained. One tool
build, zero production builds and exactly two encode calls. The first launch
found a missing expected-evidence file before any encode; the file was supplied
and the same verified tool reused. Failed logs remain. Current performance
tables and existing full/package qualification are unchanged.

[Evidence](webp-storage-lifetimes-evidence.json) pins the model, producer and
reproduction. Raw webp-storage-lifetimes.tar.xz:654,388B /35members, SHA256
26ee17264fa4a5bd3d6b524b38ba77953a80fb56da7542d4fd4a25a20bc57124,
mirrored/member-verified. It includes full traces, private audit source/runtime,
both launches and guards; broader source/qualification depends on the retained
shared-compositing archive. Borrowed input and caller-owned stream capacity are
excluded from admitted encoder peaks; native capacity is not measured here.
