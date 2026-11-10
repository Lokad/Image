# Paired packed streaming rows

PRR752 shares clamped source indices and packed coefficient reconstruction
between two immediately required RGBA8 rows. Each row keeps its original ordered,
separately rounded multiply/add sequence. One shared scheduler source selects
byte/word modes at entry; the word specialization contains no pair state or calls.
Single-row and word kernels, vertical contribution order, certificates and exact
repair remain. No new array, cache, session, owner or numerical policy.

| Complete 513×259 public bicubic resize,4KiB scratch / zimg | Managed µs | Native µs | Ratio / upper | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Tight4x9_8 | 407.83 | 676.28 | 0.603 / 0.607 | 113 |
| Tight4x9_16 | 644.23 | 778.52 | 0.828 / 0.831 | 113 |
| Tight4x2_8 | 394.10 | 1124.84 | 0.350 / 0.355 | 113 |
| Tight4x2_16 | 628.33 | 1231.63 | 0.510 / 0.516 | 113 |

A contemporary original4×9 byte confirmation measures795.71/native676.28µs.
The independent candidate407.83/native676.28µs saves **387.88µs/48.75% raw and
normalized**, with separated filtered/all-actual confidence. Native point timing
changes less than0.001%. The promising409.60µs short is retained. Each confirmation
has12actuals; all actuals remain available. The other three rows establish current
passing costs, without causal companion-gain claims against older timings.

Managed geometry/coefficient preparation, scratch, independent output and disposal
stay timed. The explicitly labeled zimg3.0.6 FFmpeg zscale backend retains its
graph/input/scratch; its existing halo/conversion/filter/output/release boundary
is unchanged. This comparison does not include fresh native graph construction.
All frozen managed pixels and original native max/RMSE gates, input/output
ownership and quiet single-CPU idle-v2 checks pass. Four unaffected8×17/8×4 costs
reuse unchanged source evidence: **seven passes/one8×17 byte gap across eight**.
Passing controls stop; the full283-path objective remains open.

Total allocations stay113B/op. Fixed workspace admission stays4096B, scratch
retention0, and independent output payloads32/64/144/288B remain charged through
disposal. Existing output-pool retention is separate. Physical/native/JIT-stack
peaks are unmeasured; fewer coefficient decodes do not imply lower allocation.
Borrowed input-row views end with the synchronous helper. Repeated clamped rows,
block reset, cancellation checkpoints and terminal admission cleanup remain.

[Attribution](PACKED-STREAMING-BYTE-CURRENT-ATTRIBUTION.md) locates1899/1998samples
in the655B single-row kernel. Its64-schedule model preserves source/vertical
traces and predicts1036→520calls on4×9; the model alone proves no CPU benefit.
Actual816B pair code shares reconstruction, keeps both accumulators in registers
and uses separate multiply/add. Original655B single instructions remain identical;
the2677B word scheduler erases pair calls/state. The new production-helper test
checks six zero/single/even/odd/long/clipped cases against independent scalar bits,
alpha0/1/255, borrowed guard preservation and zero allocation. Existing exact
multiblock, tiny-alpha repair, aggregate admission, refusal, cancellation,
independent survivors and recovery tests remain.

**6155full/live checks per OS**,329internal/729public resizing checks in
normal/scalar/AVX2-off/AVX512-off modes, and the SAME actual Release package/all
three examples pass Windows/Linux. One immutable main/embedded build perOS is
reused; no benchmark image/settings/harness/native/selector build or failed
production compile. Unchanged codecs, full streaming and other resizing evidence
remain valid. This is scoped qualification, not a full release benchmark refresh.
Publication remains postponed; published packages are unchanged.

[Evidence](packed-streaming-paired-byte-rows-evidence.json) pins producers,
confidence, current costs and qualification. Private0.2.1 package SHA256
`7fafa5bae86f2366f79dca79eea8adbc255856e86392d372c1cfe9af98b61b0a` contains timed DLL `a65e3c83561090f2b3081f0afc118352db534460838ee7fd87042221bcad91db`.
Mirrored/member-verified raw `tmp/packed-streaming-paired-byte-rows.tar.xz`, SHA256
`5dc038f1aee1c70f5b05da55f6939a8a1881d68ce2cd804143ead9c83a3971bb`,6,668,788bytes/165members, retains original/
final modules, source, scripts, logs and historical runtime providers.
[Shared streaming](BICUBIC-SHARED-STREAMING.md) retains earlier evidence.
