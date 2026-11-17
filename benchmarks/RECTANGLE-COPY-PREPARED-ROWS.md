# Prepared rectangle-copy execution

Accepted-stage/measurement history. [Shared self-row state](SELF-ROW-STORAGE.md)
owns current copy costs; the original evidence below remains.


Accepted complete original word-copy pairs improve **5.799 → 5.100 µs**, saving
**0.700 µs / 12.07%**, with ratio interval **.878361–.880288**, separated raw mean
intervals and **0 B/op**. All twelve alternating 16,384-operation pairs and quiet
guards pass on the final timed DLL. This is a scoped incremental effect, not
standalone FFmpeg parity or a cumulative historical gain.

The caller selects the existing bulk strategy once for independent rows that fit
one 4,096-pixel cancellation chunk. A small row helper prepares remainder geometry
once and covers the end with one to four bounded 64-byte vectors. At most 63 bytes
overlap the preceding bulk; those writes repeat the same source bytes within the
same independent row. Snapshot Y order and every row checkpoint remain. The
four-load/four-store bulk, shared single-row primitive, small/wide/scalar paths,
same-row memmove, separate-word fallback, validation and admission remain.
No buffer, owner, pin, pool, cache, public API or numerical policy is added.

| Complete operation / existing FFmpeg reference | Managed / reference µs | Conservative upper / status | GC B/op |
| --- | ---: | --- | ---: |
| Original RGBA16 overlap + reverse, 257×129 | 5.004 / 4.457 | 1.133517 / gap | 0 |
| Separate RGBA8 copy, 256×129 | 1.964 / 1.881 | 1.050090 / gap | 0 |
| Separate RGBA16 copy, 256×129 | 4.029 / 3.801 | 1.101418 / uncertain | 0 |
| RGBA8 overlap + reverse, 256×129 | 3.497 / 3.766 | .931853 / pass | 0 |
| Original clipped RGBA8 copy, 254×129 effective | 1.928 / 1.850 | 1.048348 / gap | 0 |

All five frozen inputs, settings, original workers and complete public boundaries
remain. Native arguments are prevalidated; managed public checks stay timed.
Source/canvas preparation, reset where excluded originally, IPC, startup and
verification stay outside both sides. One CPU0 operation runs at a time with
natural nonconcurrent GC. Every exact sample, alpha, hidden RGB, untouched pixel,
snapshot and owner check passes. Current native word costs are faster than older
captures; historical clocks are not subtracted to infer implementation gains.

Separate-word uses its frozen **262,144-operation** cadence. Native phase means
**3.801 / 4.007 / 4.076 µs** and managed actuals drift together; adjacent ratio
confidence **.983412–1.028262** and the conservative gate remain uncertain. All
samples stay, including the adverse pair. No causal regression claim, filtering
or favorable repeat. The initial 65,536-operation diagnostic is retained separately.

The preparation-only stage confirms **1.30% / .075 µs**, but still calls Memmove
per row. Completing vector ends removes that call; prototype screen/independent
effects are **12.79% / 11.22%**. Only the actual final-DLL effect above is accepted.
Actual prototype code is 1,466 B caller +438 B helper versus 1,364 B prior caller;
code size does not establish a speedup or physical/JIT retention.

Windows/Linux each pass **6,794 full/live checks** (3,334 internal, 2,512 public,
948 differential), focused **4 boundary theories /190 public** checks per normal,
AVX-512-disabled and scalar mode, and original full-word worker checks. The boundary
theories cover both depths/directions and bulk, vector-end and cancellation-chunk
edges. The **same actual Release package** and all three usage examples pass both
OSes. The final Linux DLL is never rebuilt after timing. Two isolated core/test
stages, one final core/embedded build per OS, zero existing public/harness/native/
fixture builds and one NoBuild pack. Broad/package checks follow reconciliation.

New admitted and retained capacity remain zero; the caller's word canvas is
1,062,936 B outside the operation. Physical/native/JIT peaks remain unmeasured.
An older harness's assembly-relative root lookup initially selects the wrong
byte reference: that completed capture stays unqualified. Physical runtime
placement restores frozen identities without rebuilding. Pre-guard failures have
zero samples; RAM diagnostics avoid incidental file writes. A persistence error
recovers completed results without repeating operations. First attempts remain.

[Evidence](rectangle-copy-prepared-rows-evidence.json) pins sources, all stages,
actuals, package and qualification. Mirrored/member-verified
`rectangle-copy-prepared-rows-qualified.tar.xz`: **6,060,160 B /286 members**, SHA256
`11477dfaad5fb561773ed7dd057ab15f92f3938da790386d669ab3f8adc7df02`.
Archived `cfr1190-*`, `rbp1191-*`, `rvt1192-*` and declared dependencies reproduce
the round. Unaffected fill/composite, vertical-flip, codec, resizing and font/text
paths reuse identified evidence. The 289-path objective and final full-package
refresh remain open; publication stays postponed.
