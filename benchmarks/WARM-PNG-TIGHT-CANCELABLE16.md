# Cancelable PNG decoding at the working-memory boundary

Complete warm RGBA16 decoding improves **23.1%**, from **2.466×** FFmpeg to
independently confirmed **1.897× / upper 1.914×**, at **5,459.12 µs** and
**8,896 B/op**. All twelve actuals are retained, including two BDN-filtered
outliers; the upper bound includes both filtered and all-actual statistics.
Every measured run passes both versioned quiet guards. Stop this fixed pair.

The original 513×259 word raster and frozen 960,565-byte PNG remain unchanged.
The exact managed working limit is **3,254,193 bytes**: encoded input + IDAT +
output + original 256 KiB inflater allowance + two rows. One byte below fails.
Neither optional whole-filter nor 8 KiB read buffering fits, so the original BCL
inflation path remains. This admission limit is per operation, not process RSS.
Released input/output storage may be reused; no output is cached.

The live cancelable token is never signaled during timing. Token/source and options
setup/disposal are excluded; public decode, admission, framing, inflation, endian
conversion and owned output disposal are included. The unchanged native worker
times complete decode, full RGBA64BE-to-RGBA64LE conversion and both owned-frame
releases. Native exposes no matching cancellation or caller memory-limit setting.
Exact pixels, all words/alpha/hidden RGB, source/owner independence, normal/scalar
native CLI results, exact admission and already-canceled rejection precede timing.

Existing attribution for the same framing primitive guides one local-accounting
change. A first loop with exception handling reaches only 2.046× and receives no
long repeat or full qualification. The accepted loop keeps bit/budget state local,
polls cancellation at both original per-symbol boundaries, and commits successful
charges before returning. Short budgets, cancellation, tails and other symbols
return to the original dispatcher for errors. Completed work survives cancellation;
no heap workspace or relaxed check interval is introduced. Independent per-symbol
materialization controls check work/failure precedence across literal budgets.

The short mean is 1.912×, with a wide 2.600× upper; the independent repeat confirms
the target on the same core/harness. Ordinary tight-memory and default-memory
cancelable regression means pass at 1.812× and 1.909× before full qualification;
these remain screens with prior confirmations linked. Only three workload types
and the two existing original rasters are used. An initial guard catches post-build
disk I/O before any benchmark launches; its failure is retained. Live inspection
finds no active work, writes are flushed and the same build passes fresh guards.

All **3,334 Release tests per OS**, including **896 live FFmpeg comparisons**,
and **147 scalar checks per OS** pass. The same actual dependency-free Release
package passes its consumer and all three examples on Windows/Linux. Runs use
sequential Release, one pinned CPU, one codec thread, normal SIMD, tiering off
and workstation nonconcurrent GC. Startup, I/O, IPC, preparation and validation
are excluded; native allocations are unmeasured.
[Numeric evidence](warm-png-tight-cancelable16-evidence.json) retains actuals,
allocations, guards, four child identities, rejected trial and qualification.
Remaining required material/current-source warm coverage stays open.
