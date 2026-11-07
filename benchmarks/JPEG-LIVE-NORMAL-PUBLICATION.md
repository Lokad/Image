# Normal-exit publication in live JPEG decoding

The live AC window now publishes its canonical bit cursor on normal misses and
completion. Exceptions abandon this operation-private reader; its enclosing
decoder still cleans up images/coefficient storage. Removing the unnecessary
exception-state publication lets normal locals stay outside a `finally` handler.

| Complete live-token native444 decode, independent twelve | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Original | 1,972.52 | 1,719.67 | 1.147 / 1.168 | 10,088 |
| Accepted | 1,906.09 | 1,718.85 | 1.109 / 1.125 | 10,088 |

The stage saves **66.42 µs / 3.37% raw / 3.32% reference-normalized**. Filtered
and all-actual raw/normalized intervals separate; native moves −0.048%. Original
twelve actuals/ten filtered and candidate twelve/twelve remain separately retained.
The remaining **187 µs** gap is open. The preceding three-actual screen is not an
independent gain or parity claim; no favorable repeats follow.

The boundary audit establishes no public observer or resumption of the failed
reader. Public decode, caller-sample decode and validation construct a private
decoder. `ReadScan` itself copies the entropy cursor into decoder state only
after success. Caller samples are written after EOI; outer `finally` blocks
release private images/coefficient owners. Those boundaries and public failure
semantics remain. There is no failed-state forwarding wrapper.

Loop arithmetic, input inspection/refills, every separate prefix/amplitude
cancellation checkpoint and normal canonical publication are unchanged. Existing
all-phase/category/sign, raw byte-pair, truncation/marker/restart, following-read,
partial-write, cancellation/refusal/recovery and surviving-owner controls pass.
Windows/Linux each pass **5,870 full/live checks**, plus **401 internal / 375 public**
per normal/scalar mode. The SAME actual private Release package and all three
examples pass both OSes. One immutable core/embedded build per OS; zero public,
harness, selector, native or fixture builds. Full/package qualification runs once.

The frozen 133,041 B JPEG still yields the exact managed/native 531,468 B RGBA
output. Input delivery, parsing/table preparation, entropy, transform/color work,
independent output acquisition and disposal remain timed on one CPU. Native
codec/conversion/input/frame wrappers retain declared preparation. This is not
a fresh-context comparison, encoder change or numerical-policy change.

No array, table, cache, owner, session or retained state is added. GC above is
measured; source-valid native444 governed peak 541,253 B and retained output 531,468 B
reuse [output ownership](JPEG-FULL-GRID-OUTPUT.md). Default 4 MiB pool/governed bypass
remain. These capacity facts differ from unmeasured physical/native peaks.
Shared roots, terminal refusal, async lifetimes, exact packets and managed pixels stay.

Only the live baseline cost refreshes; **28 JPEG costs and all 125 displayed
numeric rows** reuse unchanged paths. JPEG remains 14 pass / 13 gaps / 2 uncertain;
passing and uncertain cases are not optimization targets. Earlier
[lookup-only integration](JPEG-LIVE-LONG-LOOKUP-EXPERIMENT.md) remains unselected
history, excluded from the current renderer's checks. This stage retains the
qualified primary-eight-bit window rather than combining that candidate.

[Evidence](jpeg-live-normal-publication-evidence.json) pins the audited factories,
source/token proof, producer identities, all actuals and same-package qualification.
Mirrored/member-verified raw `jpeg-live-normal-publication-qualified.tar.xz`,
7,494,044 B / 142 members, SHA256
`372c151b086e0e7bd75bd3bacacb98bb3810b4af48411ef5b3b836f8c9ce008e`,
preserves sources/modules, scripts, guards and regression/package logs, with
pinned qualified fixture/tool providers. This is not a 125-row actual-package
release refresh or completion of all 283 material paths. Publication is postponed.
