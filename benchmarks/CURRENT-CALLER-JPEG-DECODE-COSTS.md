# Current caller-owned JPEG decoding costs

Historical costs: affected JPEG boundaries are superseded by the
[current container-scan round](WARM-JPEG-CONTAINER-SCAN.md).
Original observations and raw evidence remain below.

The two existing 513×259 caller-buffer controls now have independent warm
single-CPU evidence. RGB420 passes the conservative ≤1× gate; grayscale remains
a demonstrated gap. These current costs do not establish a paired speedup or
broader JPEG parity.

| Complete sample decode | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op | Encoded input B | Status |
|---|---:|---:|---:|---:|---:|---|
| Grayscale | 754.097 | 646.946 | 1.166 / 1.171 | 273,427 | 147,825 | Gap |
| RGB420 | 1,076.048 | 1,159.188 | 0.928 / 0.938 | 624,516 | 52,093 | Pass; stop |

Original resident packets, settings and input delivery remain frozen. Both
operations acquire fresh zeroed caller storage, decode all samples, pack output,
return typed source metadata and release decoder storage. Neither requires RGBA.
Native output includes its existing tail padding; native allocation is unmeasured.
Managed B/op includes harness costs. Startup, IPC and correctness checks are excluded.

All native samples are exact; normal/scalar CLI gates, source/padding integrity,
independently surviving caller outputs and disabled ICC/EXIF transformations pass.
Both original RGBA metadata hashes already match independently qualified exact
pixels: no metadata projection or expectation change was needed. The initial
stale-hash hypothesis is corrected, with its audit retained.

Earlier costs predate the actual caller-path change enabling direct baseline
sample planes, then ordinary DC/AC fusion. Default RGBA costs cannot replace
caller-buffer evidence. The already current CMYK caller confirmation is reused
without retiming: 1.052 / upper 1.058×, with its later no-call proof intact.

Zero builds, new inputs/settings/cases, profiles or repeated full/package checks.
Two sequential twelve-actual jobs reuse the immutable qualified DLL, harness,
cached selectors, workers and fixtures. All actuals, native brackets and both
idle-v2 guards are retained. Applicable 5,342 checks/OS, three additional public
probe contracts normal/scalar each OS, the same actual Release package and three
examples carry forward; CI passes all three jobs. No new combined full local run
is claimed. Stop RGB420 and do not retime grayscale without a distinct change.
The full 268-path objective remains open.

[Evidence](current-caller-jpeg-decode-costs-evidence.json) records samples,
allocations, hashes, complete boundaries and qualification reuse. Raw archive
`current-caller-jpeg-decode-costs.tar.xz` is mirrored/member-verified:
2,688,656 B / 70 members, SHA256
`04916a1ce5683de130a8e8f904919438142488a68202c58f1ba426a6d08f76e4`.
The retained J4N31 archive supplies unchanged producer/package provenance.
