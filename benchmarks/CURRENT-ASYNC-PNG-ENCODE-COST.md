# Current asynchronous PNG encoding cost

The existing complete async nonseekable output case now independently confirms
a **1.303× gap / upper 1.323** on the qualified core. Its former
[BCL-encoder timing pass](WARM-PNG-ASYNC-ENCODE.md) is historical.

| Existing 513×259 RGBA8 control | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Complete packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Async, fresh nonseekable output | 24,513.62 | 18,812.43 | 1.303 / 1.323 | 1,387,492 | 210,973 |

The same original pixels, default Optimal (actual level 6), native level 6 and
filter None are retained. Complete managed/native packets are byte-identical,
SHA256 ad9ed127da583d68bdb749bb983b73966e8e814527216449c4557fdd89668920.
Normal/scalar probes pass on both OSes, including exact independently inflated
samples, alpha/hidden RGB, WriteAsync-only delivery, cancellation, refusal, sink
failure/recovery and independent output/source lifetimes. The unchanged native
worker's normal/scalar packet, CLI pixels and ownership evidence is reused.

Both subjects include complete encoding, owned staging, copying into fresh
independently owned nonseekable output, awaited completion and storage release.
Resident callbacks complete immediately. No final snapshot copy, forced scheduling
or real I/O is timed; the caller output stays open. Native AVIO flush finishes
its buffered writes without requiring managed caller FlushAsync. Allocation
includes harness costs; native allocation is unmeasured. This is a current cost,
without a paired gain/regression claim against different encoders.

All twelve actual iterations and native pre/post brackets are retained, including
the managed outlier excluded by BenchmarkDotNet's eleven selected results. Both
idle-v2 guards pass. Two prelaunch refusals caught idle build servers; no scoring
ran until those verified preparation processes stopped. Their records are retained.

**Production is unchanged.** One harness-only build per OS corrects stale level-9
labels and checks the prepared complete-packet hash. No core, test, selector or
native build; no profile, repeated score, full suite or package verification.
The same **5,342 checks per OS**, 948 live checks and actual Release package/three
examples remain applicable, including shared admission and pending-I/O lifetime
coverage. Current package SHA256
318791526e4dd7b351856a237fecb54cff8959cd28b9a8c9729e38c4d3587589.

Exact managed encoding and shared checksum changes invalidate the earlier
218,775-byte BCL packet's CPU pass under a 5% IDAT tolerance. Reference inputs,
settings, packet and lossless pixels are unchanged. This resolves one already
named boundary, adding no case/grid. Stop retiming it; all **268 material paths**
remain in the warm conservative-upper ≤1× objective.

[Numeric evidence](current-async-png-encode-cost-evidence.json) retains all actuals,
confidence, identities, boundaries and qualification reuse. Mirrored/member-verified
current-async-png-encode-cost.tar.xz, SHA256
ff9c9e55e987c7bad52baa43279a2d7da18545221d1ea24e86e16e031f8fd70f (2,257,552 B / 65 members),
retains the immutable core/harness, cached selector/worker, inputs, controls,
failed preflights, guards and reproducing scripts. WPC31/H13 raw dependencies remain.
