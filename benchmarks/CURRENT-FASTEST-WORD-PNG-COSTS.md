# Current RGBA16 Fastest PNG costs

Three existing required boundaries independently confirm warm single-CPU gaps
on the unchanged qualified core. Each has one twelve-actual job; these are
current costs, without a paired gain/regression claim.

| Existing 513×259 control | Lokad µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| EncodeFast16 | 23,284.98 | 1.332 / 1.341 | 3,813,732 |
| EncodeFastCancelable16 | 24,004.55 | 1.372 / 1.383 | 3,813,732 |
| EncodeFastForward16 | 23,265.92 | 1.331 / 1.345 | 3,813,764 |

All three complete **968,216-byte packets** match the unchanged FFmpeg reference
byte for byte, SHA256
`adbcc5115ee9acce294ab70f65b6f24862b7451d90f870a082269a8008c08e25`.
Full 16-bit samples, alpha/hidden RGB, source and independent output lifetimes,
normal/scalar native decoding and both idle-v2 guards pass. Native allocation is
unmeasured; managed allocation includes harness costs. The borrowed original
RGBA16 raster, filter None and managed/native level1 settings remain frozen.
Complete row/endian packing, compression, output growth, ownership and disposal
are timed; preparation/IPC/startup/checking are excluded. The cancelable token is
live and noncanceled; forward-only output remains open and matches stock.

Earlier independent CPU passes predate the exact managed encoder: they compared
BCL level2 with native1 under a 5% IDAT gate. The later GFNI chunk CRC also runs
here, while its displayed refresh covered other named encodes. Those older costs
are historical, without a claim that equivalent production regressed. This
audit adds no shape/depth/filter/settings grid to the finite 268-path inventory.

Forward setup initially stops with **zero actuals** because the harness still
compares against the pre-exact stock packet. One path correction selects the
current stock output, already identical to frozen native output. One benchmark
harness build, separate runtime, same immutable production core/cached selector/
native worker. Stock/cancelable results remain valid because that branch is not
executed. A stale source-snapshot guard also stops before launch; it is preserved
and the manifest is synchronized. No gate waiver or successful score repeat.

No production build, new profile, input or setting; no repeated full/package
checks. SKP1's **5,332 checks per OS**, 948 live comparisons and SAME actual
Release package/three examples remain applicable. Package stays
`dfa0e669b84872990e01a0e65f21cdd87753b802859be904ac592945cfd49425`. Stop uncertainty retiming and resume
one substantial primitive among the largest demonstrated gaps, preserving exact
packets, pixels, admission, cancellation, ownership and the full **268-path ≤1×**
objective. No broad content or parity claim is established.

[Numeric evidence](current-fastest-word-png-costs-evidence.json) retains actuals,
confidence, exact gates, invalidation and qualification reuse. Mirrored/member-
verified `current-fastest-word-png-costs.tar.xz`, SHA256
`f5793eee831e9a089e374c72b1acca7e5052b93347968eed41ef1d8a08e1adf7` (5,346,612 B / 99 members),
retains both harnesses, core, cached driver/worker, original/current stock and
reference packets, failures, guards, logs and reproduction scripts. SKP1 raw
qualification remains a retained dependency.
