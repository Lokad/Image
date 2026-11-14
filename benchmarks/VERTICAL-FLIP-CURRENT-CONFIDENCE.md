# Current owned byte vertical-flip confidence

The unchanged accepted package's complete RGBA8 vertical flip measures
**9.232 / FFmpeg 9.217 µs**, ratio **1.002**,
confidence interval **0.983–1.016**: uncertain. It is neither
a demonstrated current gap nor a passing control. No production speedup is claimed.

Use the existing supported equal batch parameter **256**, up from 64 before
observing the new screen: ~9 µs calls previously gave only ~0.6 ms blocks.
Both sides still time one complete public owned operation per call. The original
screen passes at 8.539/8.681 µs, upper .998; independent confirmation stays
uncertain. All 12 pairs/actuals/native brackets remain, with no favorable retry.
The older 64-operation gap remains historical evidence.

The same original 513×259 source, integer row placement, full RGBA/hidden RGB,
normal/scalar native checks, unchanged borrowed source, independent output owners
and idle-v2 CPU0 guards pass. FFmpeg retains its graph/source/frame wrappers,
then allocates, materializes and releases independent output pixels inside each
operation. Native padded rows remain exposed directly; no packing stage is added.
Managed admission, output acquisition and disposal remain timed.

GC allocation is **112 B/op**. No capacity, cache, owner or preparation changes;
operation scratch retention is zero. Source and result each occupy 531,468 bytes.
The default two-slot/4 MiB idle store stays distinct from logical live capacity
and unmeasured physical/native/JIT peaks.

The timed DLL is byte-identical to the actual qualified package. Windows/Linux
6,640 full/live checks, relevant scalar/ISA checks, the same actual consumer and
all three examples are reused without build/full/package repeats. The rejected
[bound/grouping trials](VECTOR-COPY-BOUND-SCREEN.md) are fully restored and are
not a renderer dependency. Failed preparation gates ran no benchmarks.

Current 191 image views: **179 pass / zero gaps / 12 uncertain**; current 20
independent-result edits: **14 pass / six uncertain**. In-place drawing and probe
gaps remain outside those views; the full 289-path goal stays open. Resolve
uncertainty only with a justified measurement change, rather than assuming
optimization is needed or repeating unchanged samples.

[Evidence](vertical-flip-current-confidence-evidence.json) records exact
boundaries/identities, raw archive and unchanged qualification. Publication is
postponed.
