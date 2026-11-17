# Batched Unicode mapping validation

**Accepted.** Complete owned-font loading saves **3.63% / 0.505 µs** in controlled final-DLL pairs and now passes the approved FreeType comparison. Stop this scoped path.

| Complete owned load | Baseline / candidate µs | Ratio interval | Decision |
| --- | --- | --- | --- |
| Private three-pair screen | 13.983597 /13.406576 | .910485–1.009550 | Promising point |
| Independent private twelve-pair confirmation | 14.168291 /13.721144 | .942284–.995230 | Scoped gain |
| Repository-built DLL, twelve pairs | 13.906689 /13.401896 | 0.961503–0.965910 | Gain transfers |
| Final DLL /FreeType 2.13.2, twelve actuals | 13.393890 /13.668022 | 0.945556–0.986107 | Pass/stop |

The original frozen DejaVu Sans Regular selects format12 with281 groups. Eight
12-byte records share three bounded32-byte loads, byte normalization, field
transposition and Unicode/order/glyph-range predicates. Signed ordering from a
valid predecessor establishes nonnegative endpoints; explicit glyph bounds make
accepted sums fit Int32. Failed batches re-enter the unchanged ordered scalar
validator. Only noncancelable constructor format12 validation changes; cancellable
per-group checks, format4, public lookup/shaping/rendering and ownership remain.
No decoded table, buffer, cache, field, dependency or public API is added.

The actual helper matches an independent UInt64 oracle on663,554 batches, with
tagged field transposition, unaligned reads, high bits, overflow and equality
controls. Eleven real-constructor batch/tail cases plus two malformed/limit/
cancellation/refusal/recovery controls add15 tests. Original16,384 ordered
glyph/ownership oracle cases reuse the exact prefix-corrected binary from
[location carry](FONT-LOCATION-CARRY-TRIAL.md); no oracle rebuild or relaxed gate.
Generated code realizes the intended AVX2 permutations; instrumented clocks are
discarded, with no sample commands. Portable scalar validation stays qualified.

Quiet-VM CPU0, optimized JIT, natural nonconcurrent GC, idle-v2 guards, original
inputs, complete owned-copy/eager-validation/glyph-count/disposal and all actuals
are retained. Paired DLLs establish only the incremental effect. The final
standalone reference separately times FreeType2.13.2 copy/open/retain/dispose;
eager/lazy parser validation differs and no general parser equivalence is claimed.
The three private-screen pairs and twelve confirmation pairs are not pooled.

Affected complete public-API integrations also pass: one-shot
**1109.504 /6197.295 µs**,
upper**0.201025**; preparation plus eight draws
**1234.137 /6778.665 µs**,
upper**0.196101**. Reference is stock
ImageSharp3.1.12/Drawing2.1.7/Fonts2.1.3. All30 fitting trials, every public layout
call, fresh cleared independently owned canvases and cleanup remain timed;
exact glyph/cluster/line/fitting/position/bounds and full16-bit managed raster
checks, approved visual gates and owner checks pass unchanged. Preparation-only
is a separate diagnostic; historical stages are never summed. One minimal driver
condition omits timing of unchanged passing PreparedDraw. Its complete method
bodies, checks, adapters, sample counts and statistics are unchanged; exact patch
and line-ending normalization are recorded. Historical harness metadata flags
are retained; final source/DLL/package identities establish this production change.

Owned font load allocates **176 GC B/op**, unchanged. Borrowed input757,076B,
owned payload678,016B, live admission678,272B and separate default idle4MiB pool
remain. Governed operations bypass that idle pool. Complete fit admitted peak
748,984B and retained preparation4,160B remain exact; no new owned heap capacity
or retention. Pipeline allocations and approximate retained-heap observations
are recorded separately. Physical/native/JIT-stack peaks are unmeasured.

Each OS passes **6,839 full/live checks**, focused322internal/222public per
normal/AVX2-off/scalar mode, and the SAME actual Release package/all three usage
examples. Affected performance reconciles before broad/package checks; timed
DLLs are never rebuilt afterward. One private Windows core/embedded build,
one final core/embedded build per OS, one subset pipeline tool build and one
NoBuild Release pack; no public/oracle/reference/fixture rebuild. Published
packages stay immutable. Unaffected passing font/text/codec/resize/copy paths
reuse their identified evidence. Three copy gaps and inspection gaps/uncertainty,
all289 material paths and the final165-comparison package refresh remain open.

[Evidence](font-mapping-group-validation-evidence.json) pins every actual,
source proof, memory boundary and qualification. Mirrored/member-verified
`font-mapping-group-validation.tar.xz`: **4,744,804 B /196 members**,
SHA256 `d12c4da114f1d8bf33fb64f746a518f7f343404aec1269e72e1dcf81fd090a35`. Archived mgc1243 recipes, source/DLLs, original guards,
checks and identified dependencies reproduce the round. Thirteen completed
generated consumers are pruned from the VM, retaining their success logs,
release packages, reference sources and cited raw evidence.
