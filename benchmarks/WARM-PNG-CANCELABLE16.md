# Cancelable RGBA16 PNG decoding

Warm decoding with a live cancelable token improves **22.7%**, from **2.450×**
FFmpeg to independently confirmed **1.896× / upper 1.909×**, at **5,454.37 µs**
and **8,552 B/op**. The independent repeat retains all twelve actuals on the
same frozen core and harness; every versioned quiet guard passes. Stop this pair.

The existing 513×259 RGBA16 raster and 960,565-byte PNG remain unchanged.
The token is never signaled during measurement; source/token creation and disposal
are outside timing. Public Decode, admission, framing, inflation, sample conversion,
owned output acquisition and disposal remain timed. FFmpeg exposes no equivalent
cancellation setting; its unchanged worker includes complete decode, full
RGBA64BE-to-RGBA64LE conversion and both owned-frame releases. Normal/scalar native
CLI pixels, every word/alpha/hidden RGB, independent outputs, unchanged source and
already-canceled rejection are checked before timing. No tolerance changes.

A six-second CPU profile attributes about 60% to framing/literal dispatch and
34% to BCL inflation. A checked literal-loop trial reaches only 2.089× and is
reverted without a longer repeat or full qualification. The accepted change
instead enables existing private framing/inflation materialization for eligible
cancelable inputs, eliminating the second inflate walk. Framing source and its
per-symbol cancellation checks/immediate work charges remain unchanged. The private
checksum checks cancellation every 4096 bytes. Exact admission, complete-tree and
checksum certification, original errors and tight/large/Adam7 fallbacks remain.

The short screen passes at 1.910× / upper 1.972× before independent confirmation.
Shared PNG8 None and PNG16 Paeth regression means pass at 1.411× and 1.028× before
full qualification; these are screens, with their prior confirmations retained.
Only three workload types and the two existing originals are used. Runs are
sequential Release on one pinned CPU, one codec thread, normal SIMD, tiering off
and workstation nonconcurrent GC. Startup, I/O, IPC, preparation, hashing and
validation are excluded; native allocations are unmeasured.

All **3,333 Release tests per OS**, including **896 live FFmpeg comparisons**,
pass on Windows/Linux, along with **146 scalar checks per OS**. The same actual
dependency-free Release package passes its consumer and all three usage examples
on both systems. Production and benchmark inputs remain frozen through qualification;
only contract/resource documentation changes between timing and qualification.
[Numeric evidence](warm-png-cancelable16-evidence.json) retains all actuals, native
brackets, allocation figures, guards, four child identities, rejected trial and
qualification archives. Required material/current-source warm coverage remains open.
