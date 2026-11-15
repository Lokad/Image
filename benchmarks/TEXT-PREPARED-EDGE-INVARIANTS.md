# Prepared immutable text edges

[Current drawing confidence](TEXT-DRAW-CURRENT-CONFIDENCE.md) supersedes the four drawing costs below; this report retains the accepted production stage.

EIP1047 computes edge deltas, Y bounds and winding once during geometry preparation,
removing repeated immutable arithmetic from event preparation and scan conversion.
Original ordered scalar/SSE2 endpoint expressions, signed-zero/nonfinite/default
semantics, clipping, cancellation and exact managed pixels remain. No reciprocal,
new numerical policy, ISA, owner, cache, pool or public API is introduced.

Contemporary prepared-RGBA16 pairs improve **41.888→36.497 µs**, saving
**5.390 µs / 12.868% raw** and **13.047% paired-normalized**.
Both intervals separate; wider bracket-normalized gain remains inconclusive.
Paired reference clock moves +0.207%. All twelve actuals and three
reference groups remain; no favorable primary repeat or historical-stage sum.

| Public boundary | Lokad.Image µs | ImageSharp µs | Ratio / upper | Managed GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Ordinary RGBA8 | 54.669 | 45.026 | 1.214 / 1.507 | 75,944 | uncertain |
| Ordinary RGBA16 | 58.029 | 46.159 | 1.257 / 1.513 | 75,944 | uncertain |
| Prepared RGBA8 | 33.424 | 37.386 | 0.894 / 1.028 | 56 | uncertain |
| Prepared RGBA16 | 36.497 | 37.661 | 0.969 / 1.123 | 56 | uncertain |
| Complete one-shot | 1,167.238 | 6,182.754 | 0.189 / 0.214 | 299,916 | Pass/stop |
| Preparation plus eight owned draws | 1,653.578 | 6,783.430 | 0.244 / 0.269 | 301,270 | Pass/stop |

These are **ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12** public
integration comparisons, never FFmpeg or bare shaping. All four drawing confidence
intervals remain open; favorable point estimates do not establish parity. The
ordinary/prepared diagnostic retains frozen DejaVu Sans 2.37 Regular “AV To”,
18 px, 384×160, origin 8/8, with 1,024 individually reset draws per actual. Public
GC above excludes the timed adapter's additional 24 B/draw.

The complete ConstrainedFit RGBA16 integrations include all thirty fitting trials,
every public reference layout call, fresh owned explicitly cleared canvases and
disposal. Eight-draw amortization is **206.697 / 847.929 µs/draw**,
including preparation. Creation-only preparation is **1,067.868 / 6,030.194 µs**;
disposal is excluded only from that diagnostic. The separate complete-fit prepared
draw remains uncertain. Never infer overall text cost from summed stage timings.

This CPU gain has a memory cost: each edge grows **40→56 B**. Latin retained
capacity grows **40,408→48,984 B**, preparation peak **42,488→52,920 B**,
and ordinary GC **59,304→75,944 B/draw**. Prepared GC stays **56 B/draw**.
Complete-fit retained capacity is **48,480 B**, governed peak **818,788 B**.
Old/new buffers overlap under admission before allocation; retained capacity stays
charged until disposal. Refusal, cancellation, cleanup and subsequent recovery
remain tested. Allocation, admitted peak, active retention and idle storage are
separate. Approximate heap deltas exclude idle pools; physical/native/JIT process
peaks remain unmeasured. No font/layout survives managed preparation.

One existing frozen ArabicMarksDigits RGBA16 managed-DLL generalization improves
**492.631→441.106 µs**, saving **51.525 µs / 10.459%**,
with separated independent intervals. GC increases **441,696→583,776 B/draw**.
This is not an external-reference elapsed/parity or broader font/script claim.

NTA1046's same-module profile/JIT identifies repeated delta/min/max operations;
its 1,995 sample locations across 27,295 checked draws are attribution leads,
not phase costs or a gain forecast. Instrumented elapsed is discarded. The
independent original endpoint oracle, every retained permutation field, physical
record size, exact peak/one-byte-below refusal and all eight frozen frames pass.
All original full-word/layout/visual/ownership gates remain. Parser validation and
prepared state still differ; unchanged [FreeType loading](FONT-HEADER-VALIDATION.md)
evidence stays scoped.

Each OS passes **6,721 full/live checks**, **265 internal / 161 public** focused
checks per normal/scalar mode, and the **same actual timed Release package / three
examples**. The initial embedded-test compile exposed an endpoint-coupled test;
its revision and later exact-size/peak updates are retained with their logs.
Linux builds the core twice and embedded tests three times (one failed); Windows
builds each once. No public/harness/reference/fixture build, one pack, no final Linux
rebuild after timing. Broad/package qualification runs once after reconciliation.

[Evidence](text-prepared-edge-invariants-evidence.json) records current scope,
source proofs, memory costs and qualification. Mirrored/member-verified raw
`tmp/text-prepared-edge-invariants-qualified.tar.xz`: 5,773,048 B / 242 members,
SHA-256 `b2a8c7de8cb85937784220b88231e7311ae7fa4f4ac65f0b7d93d70b71b8d058`.
Actual package: 465,549 B, SHA-256 `d05ba728d23304db2c9cf2c617a217d20145c80921652d6d589c10f3a58ea53a`.

[Direct quadrilateral area](TEXT-DIRECT-QUADRILATERAL-AREA.md) and prior natural-run
evidence retain superseded costs and adverse captures. Font loading, layout/fitting
without raster, codecs and resizing reuse unchanged-path evidence. No whole-release
refresh, new workload grid or overall CPU gain claim; publication remains postponed
and the complete 289-path objective remains open.
