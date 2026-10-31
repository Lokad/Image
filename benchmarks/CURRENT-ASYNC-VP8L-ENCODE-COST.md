# Current asynchronous lossless WebP encoding

The existing 513×259 RGBA8 nonseekable-output control independently confirms a
**1.293× / conservative upper 1.316** CPU gap. Both complete packets are exactly
**183,812 bytes**, SHA256 `08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde`.
This replaces stale cost evidence; it is not a causal speedup claim.

| Complete warm single-CPU operation | Lokad µs | Reference µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| EncodeWebpAsync, fresh nonseekable output | 156,836.693 | 121,322.878 | 1.293 / 1.316 | 29,697,312 |

Managed work includes admission, complete lossless encoding through segmented
staging, actual WriteAsync delivery, output growth, awaited completion and storage
release. The labeled libwebp exact=1 FFmpeg encoder backend includes repacking,
fresh import allocation, compression/staging, fresh append-only output growth,
copies and release. Both use immediate resident callbacks, no real I/O or forced
scheduling, no final snapshot copy, method4/quality75 and one pinned CPU.
Inputs and settings are frozen; native allocation is unmeasured.

All twelve actuals, both confidence views, native brackets and passing idle-v2
guards are retained. Exact packet/pixel/source/independent-output checks pass.
The current untimed public async probe also verifies retained owners, terminal
refusal, cancellation before/during delivery, sink failure cleanup and recovery.
Identical frozen native bytes permit reuse of normal/scalar CLI pixel proof;
the scoped producer header is restored byte-for-byte after measurement.

Zero builds or profiles: qualified immutable core, cached harness, worker, original
fixture and verified selector are reused. Unchanged Windows/Linux full/live and
same actual Release-package/three-example qualification carries forward. A JSON
key-casing postcheck failure is recovered from the original completed actuals and
quiet guards, without retiming. This adds no required case or workload grid.

[Evidence](current-async-vp8l-encode-cost-evidence.json) pins all samples, boundaries,
source/proof reuse and qualification. Mirrored/member-verified raw:
`current-async-vp8l-encode-cost.tar.xz`, SHA256 `1277bed28395225639c0858a026ca653620ed9bda1a9b35b6db493e08e08fe51`.
The 268-path upper≤1× objective remains open.
