# Current opaque VP8 decode confidence

**Historical after header-mode decoding.** [Current six affected costs](VP8-MODE-WINDOW.md)
supersede these decoder timings. Retain the original confidence/boundaries and raw
evidence; do not present the earlier producer as current proof.


The three existing controls now have **2 passing, 1 gap,
0 uncertain** comparisons on the current immutable actual package.
This is an uncertainty resolution, not a production speedup or historical-clock gain.

| Complete original 513×259 decode / FFmpeg | Managed / reference µs | Conservative ratio interval | Managed allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Opaque8 | 4,732.26 / 4,790.07 | 0.984051–0.990245 | 230,880 | pass |
| OpaqueCancelable8 | 4,841.21 / 4,779.99 | 1.008257–1.015392 | 230,880 | gap |
| SimpleOpaque8 | 4,838.41 / 4,875.94 | 0.987253–0.995912 | 230,880 | pass |

One prespecified longer integration increases16 complete operations (~80ms) to128
(~650ms) per subject. Twelve alternating pairs, twelve native samples before/after,
all actuals and the original conservative paired/three-bracket confidence remain.
Longer integration reduces timer/scheduling sensitivity; no event cause is asserted.
The original16-operation uncertain captures and their producers remain in the
evidence history/raw dependency. No unchanged favorable repeat or new control.

Opaque8 exercises ordinary filtering; SimpleOpaque8 uses the separate frozen nonzero
simple-filter packet. OpaqueCancelable8 uses the ordinary packet with a live
unsignaled token; native has no equivalent cancellation setting. No image, shape,
quality or filter grid. Every managed/native pixel and alpha is exact. Input,
independently surviving outputs, original reference identities and CLI gates pass.

Public managed Decode includes parsing, admission, decoding/filtering, normalization,
independent result acquisition and disposal. FFmpeg retains its declared decoder/
frame/conversion contexts and padded input; full reset, decode, packing and actual
scratch/output allocation/release remain timed. CPU0, sequential Release, optimized
JIT, natural nonconcurrent workstation GC and both idle-v2 guards pass.

Original driver, harness, worker and shared runtime bindings are reused. Only the
qualified package artifacts are copied to a fresh binding directory. No production,
embedded-test, tooling, fixture or package builds; no broad qualification repeats.
Reuse **6,686 full/live checks per OS**, scalar/affectedISA and the **same actual
Release-package consumer/all three examples** from
[variable-price preparation](VP8-VARIABLE-PRICE-PREPARATION.md). Timed DLL
`03c21e15bca16636533b45c081abadb9a99c97367f2236ddcef49ae6a8e5b0b5`, actual package SHA256
`89186b3b58da96d2868c935a149cc00a36e60f7cfac0263c0ca07f9adb9c2478`. Subsequent production changes affected encoding/
copying, not decoder allocation; current complete costs are measured directly.

Allocation remains **230,880 B/op**; each independent result owns **531,468 B**.
Allocation, admitted capacity, active retention and default idle retention are
separate. No pool, capacity or owner change; the two-slot/4MiB default pixel store
and caller-root bypass remain. Physical/native/JIT peaks are unmeasured.

All twelve WebP reads have 11 passes,
1 gaps and 0 uncertainties. Nine other passing views
reuse [stream confidence](VP8-STREAM-CURRENT-CONFIDENCE.md),
[prepared input](PREPARED-MEMORY-INPUT.md) and [released storage](RELEASED-IMAGE-STORAGE.md).
Stop passes; preserve any open cases without presumed optimization. All289material
paths remain required; publication postponed.

[Evidence](vp8-decode-current-confidence-evidence.json) identifies every current
case, source, owner and qualification boundary. Full observations/validations are
in raw `vp8-decode-long-integration.tar.xz`, SHA256
`e7f276497356eaa6ffec978bb83fc51a918dd1107d5ad6f305c65ba824684234` (33,008B/28members),
mirrored/member-verified. Dependencies retain original captures/tools and package.

[Current live-token attribution](VP8-LIVE-DECODE-ATTRIBUTION.md) identifies the
block-by-block arithmetic-state bridge as the next bounded reuse trial. Its
sample locations do not establish preparation costs or a predicted speedup.

[Arithmetic-window trials](VP8-LIVE-WINDOW-TRIALS.md) retain two unaccepted
implementations. Scoped paired gains did not establish a standalone win; qualified
production is restored exactly and current costs remain unchanged.
