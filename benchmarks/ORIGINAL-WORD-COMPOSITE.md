# Original word self-composite

The original required `Composite/Rgba16Overlapping` boundary **passes and stops**:
**649.772 / ImageSharp 716.960 µs**, ratio **.906287**, conservative interval
**.803702–.976714**, **0 managed B/op**. No production change or speedup is claimed.
This recovers an existing required path,not a new image/settings grid.

The reference is explicitly **ImageSharp 3.1.12 Rgba64 source Clone + DrawImage
normal SrcOver opacity1 + Dispose**,one CPU;it is **not FFmpeg performance**.
Direct self-drawing fails snapshot semantics on this frozen input. The untimed
probe retains that failure;the necessary source snapshot and its disposal are
inside the complete reference operation. Lokad's public operation handles aliasing
through traversal without an owned snapshot. Both mutate one borrowed caller
canvas. Original513×259RGBA16,shift(1,1),effective512×258;canvas reset from the
same immutable seed is excluded before **every** operation on both sides,matching
the existing compositing boundary. Preparation,JIT,BigInteger oracle,hashing,
verification,IPC and reset are excluded equally.

The short three-actual screen is **620.700 / 708.943 µs**, upper **.914937**.
Independent confirmation retains all twelve actuals,128 complete draws per actual,
alternating order and three reference brackets. Two clocks surround each complete
draw. CPU0,optimized JIT,natural nonconcurrent workstation GC and idle-v2 guard
apply. Confidence uses all actuals,two-sided99.9%Student-t,paired log ratios and
managed upper mean versus the fastest of three reference lower means. No sample
filtering or favorable repeat.

Managed pixels are exact against the independent BigInteger snapshot oracle,
including all16-bit samples,untouched pixels and zero-source-alpha destinations.
The approved reference gates stay maxRGB/alpha≤1 and RMSE≤.25 each;observed
RGB RMSE **.042707**,alpha **.042145**,max1 each,with untouched and zero-alpha
pixels exact. Windows/Linux normal/scalar checks pass,including pre-cancellation,
local refusal without mutation and successful recovery. Reference results are
visual/numerical comparisons within explicit sample bounds,not sample exactness.

Allocations and storage differ: **Lokad0 / reference26,800 GC B/op**. The borrowed
canvas is **1,062,936 B**,outside operation ownership. Lokad adds zero owned or
retained workspace. The reference's temporary snapshot has **at least1,062,936 B
of pixel payload**,but may reuse retained ImageSharp allocator blocks;GC bytes
are not total storage or a measured peak. Reserved/native/physical/JIT peaks and
retained reference pool capacity are unmeasured. Snapshot creation and disposal
remain timed even when its blocks are reused.

The same actual immutable package DLL runs on both platforms. Reuse its
[6,745 full/live checks eachOS and actual package/all three examples](WORD-ALIAS-DISPATCH.md)
for unchanged production;no broad campaign or package rebuild. One successful
Linux tooling build,preceded by a compile failure corrected before any check or
sample;zero production,embedded,public,existing-harness,native,fixture,Windows or
package builds. Published releases remain immutable;publication stays postponed.

[Evidence](original-word-composite-evidence.json) pins complete boundaries,
reference identities,all actuals,pixel/owner checks and qualification reuse. Raw
`tmp/original-word-composite-confirmation.tar.xz`, SHA-256
`ee5cc5e151ad16e1a4663f66c1fb0f5289cfc375478386fa2272c3ca4c2a0ddb`,
1,436,900 B / 65 members,is mirrored/member-verified. Prior core/reference/package
archives are pinned dependencies;original failed source/log remain. Build with
`tools/WarmReference/build-word-composite-linux.sh` against existing immutable
library/reference DLLs;run `measure-word-composite.py --worker DLL
--core-sha256 SHA --input INPUT --prefix OUTPUT --mode short|confirm` on the quiet
VM. The worker's `probe` and `check-only` modes make no timing claim.

The [original finite coverage](ORIGINAL-PLANNED-COVERAGE-RECONCILIATION.md) retains
other missing boundaries and current gaps. A passing independent-source composite
did not substitute for this self-aliasing operation,and this pass does not prove
broader shapes,depths,settings or reference parser equivalence.
