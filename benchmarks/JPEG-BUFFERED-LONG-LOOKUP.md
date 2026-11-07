# Reuse prepared JPEG codes inside entropy windows

The live-token cost is superseded by [normal-exit publication](JPEG-LIVE-NORMAL-PUBLICATION.md).
The other28paths and this stage's paired RGB gain remain valid.

The ordinary fused DC/AC and AC readers now share a lookup that reuses the
existing nine/ten-bit canonical table. These codes stay inside the bounded
entropy window instead of publishing its cursor and entering slower readers.
Table construction remains timed. There is no new table, owner, capacity, pool,
session or retained state. Incomplete fields, stuffing, longer codes and invalid
syntax keep their original fallbacks; live-token and progressive kernels remain.

| Complete caller RGB444 decode, independent twelve | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Original | 1,728.67 | 1,452.51 | 1.190 / 1.198 | 10,088 |
| Accepted | 1,645.93 | 1,451.85 | 1.134 / 1.144 | 10,088 |

This saves **82.75 µs / 4.79% raw / 4.74% reference-normalized**. Filtered and
all-actual raw/normalized intervals separate; native moves −0.045%. The remaining
194 µs gap is open. The preceding three-actual screen is retained, without a
separate gain claim or favorable repeat.

All **23 baseline costs** refresh; **six unchanged progressive confirmations**
are reused. Of 29 required JPEG read/validation controls, **14 pass and stop,
13 remain gaps and two are uncertain**. Ordinary native444 upper 1.018 and caller
CMYK upper 1.004 are uncertain, without demonstrated optimization gaps. Sparse
CMYK measures523.46 / native 439.35 µs, 4,520 B/op; refreshed companion costs do not
establish causal gains across JPEG. Small allocation-counter differences on
other controls do not establish storage changes.

The frozen 133,057 B RGB input and exact independently owned 531,468 B RGBA output
remain. Input delivery, parsing/table preparation, entropy, inverse transform,
color work, output acquisition and disposal are included on one CPU. Native
codec/conversion/input/frame wrappers retain their declared preparation. This is
not a fresh-context comparison, encoder change or numerical-policy change.

New actual-route controls witness completion with nine/ten-bit codes, all cached
phases, signed values, every truncation, stuffing, invalid categories/runs, partial
writes and following reads against the original scalar grammar. Existing cursor,
padding/restart, cancellation, aggregate admission, refusal cleanup and surviving
ownership checks remain. Windows/Linux each pass **5,870 full/live checks**,
plus 401 internal / 375 public per normal/scalar mode. The SAME actual private
Release package and all three examples pass both OSes. One immutable core and
embedded build per OS; zero public/harness/selector/native/fixture builds. Broad
and package qualification run once.

Source-valid governed native444 peak 541,253 B and retained output 531,468 B reuse
[full-grid output ownership](JPEG-FULL-GRID-OUTPUT.md). The default 4 MiB pool and
governed bypass are unchanged. These capacity facts differ from total GC and
unmeasured physical/native peaks. Encoder packets, approved metadata exceptions,
exact managed pixels, shared roots, cancellation and actual async lifetimes stay.

A bounded two-second profile selected this stage: 1,993 leaf samples, 816 fused
reader, 154 plain AC and 395 inverse AVX2. A diagnostic model counts 6,630 blocks,
2,112 fused / 3,335 plain long-prefix misses and 2,427 scalar nine/ten-bit codes.
Its header/sampling/stale-bit authoring corrections and original source are retained.
The final model checks the exact input, syntax, padding and final EOI; it is not
production instrumentation. Those pre-change samples/counts are historical
attribution, not current phase times or predicted savings. Instrumented elapsed
is discarded. Earlier [high-aligned readers](JPEG-HIGH-ALIGNED-READER.md) retain
their paired gains and adverse/inconclusive studies; amplitude-nibble and row-pair
implementations are not replayed or permanently banned.

[Evidence](jpeg-buffered-long-lookup-evidence.json) pins sources, producers,
comparisons, all 29 costs, guards and same-package qualification. Mirrored,
member-verified raw `jpeg-buffered-long-lookup-qualified.tar.xz` is 8,054,136 B /
404 members, SHA256 `7fd6f413347ed69862a1228b3be73aa067e42b997e1649ad19c14e64cdb227c6`.
It preserves original/current modules, sources, scripts, profile/JIT/model,
actuals and regression/package logs, with pinned fixture/tool providers.
Ten displayed rows refresh;115 reuse unchanged paths. This is not a 125-row
actual-package release refresh or completion of all 283 material paths.
Publication remains postponed.
