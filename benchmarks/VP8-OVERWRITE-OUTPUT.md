# Complete-overwrite VP8 output ownership

OPA465, 2026-11-07. Accepted; publication postponed.
[Evidence](vp8-overwrite-output-evidence.json) retains five historical decoder
costs; [prepared models](VP8-PREPARED-COEFFICIENT-MODEL.md) own their current costs
and qualification. The ownership design below remains unchanged. The 283-path
performance objective remains open.

Unprepared VP8 rendering now uses the existing complete-overwrite image allocator.
Every RGB/alpha sample is written before publication, including scalar/vector edges
and tails. Prepared ALPH output, color arithmetic and coefficient readers are unchanged.
Default calls can reuse the existing exact-size pixel pool after disposal; governed
calls admit the retained owner before fresh uninitialized storage and bypass the pool.
No new pool, cache, owner, numerical tolerance or fallback is introduced.

Independent contemporary original/candidate comparisons show ordinary opaque decode
improves **5,145.37→5,040.32µs**, saving105.05µs/2.04%raw/1.99%native-normalized.
Live-token decode improves **5,293.38→5,162.06µs**, saving131.33µs/2.48%raw/
2.53%normalized. Both filtered/all-actual raw and normalized confidence intervals
separate for both comparisons. Native clocks move−0.049%/+0.046%. Initial short
screens and every actual are retained. No further sampling follows.

| Existing complete operation | Managed µs | FFmpeg µs | Ratio / upper | Managed GC B/op | State |
| --- | ---: | ---: | ---: | ---: | --- |
| Opaque8 | 5,040.32 | 4,793.39 | 1.052 / 1.056 | 228,864 | gap |
| OpaqueCancelable8 | 5,162.06 | 4,790.47 | 1.078 / 1.082 | 228,864 | gap |
| SimpleOpaque8 | 5,143.24 | 4,890.68 | 1.052 / 1.057 | 228,864 | gap |
| Native8, compressed alpha | 6,229.54 | 5,972.66 | 1.043 / 1.047 | 238,384 | gap |
| StreamNative8, compressed alpha | 6,235.75 | 5,992.89 | 1.041 / 1.045 | 238,480 | gap |

Dedicated quiet VM, Release, CPU0;12managed actuals and12samples per native
bracket, idle-v2 guards. Frozen72380B opaque inputs produce exact531468B RGBA;
simple-filter and98590B ALPH controls retain their original identities/settings.
Complete parsing, reconstruction, filtering, color conversion, independent output
and disposal remain timed. The stream case includes fresh seekable input delivery.
FFmpeg retains its documented codec/conversion contexts and private scratch. These
are current development-module costs, not one-shot context-setup parity or a full
actual-package performance refresh. Only ordinary/live opaque have incremental
gain evidence; the other three rows establish current costs, without causal claims.
One displayed cost refreshes;124 numerical rows reuse unchanged paths.

Opaque GC allocation falls760564→228864B/op. Storage observations on the same
complete opaque span input agree on both OSs, with normal and live tokens:

| Storage boundary | Original → current |
| --- | ---: |
| Governed admitted peak | 747,020 → 747,020 B |
| Governed result retained until disposal | 531,468 → 531,468 B |
| Governed charges after disposal | 0 → 0 B |
| Sequential default idle pixel capacity after disposal | 0 → 531,468 B |

The pool still has two exact-size slots within4MiB; simultaneous results own distinct
arrays. Governed calls neither consume nor populate it. Total GC, admitted live
capacity and idle retention are separate; physical/native peaks are unmeasured.
Default timings do not establish a governed CPU gain. The earlier
[alpha handoff](VP8-ALPHA-OUTPUT-HANDOFF.md) retains its distinct storage evidence.

Windows/Linux each pass **5801full/live checks** (2536internal/2317public/948differential),
plus34internal/260public in each normal/scalar mode. Existing numerical kernels
reuse their unchanged disabled-ISA evidence. Added tests poison output storage,
cover widths1/8/9/10/17 and alpha modes, keep simultaneous survivors, and verify
terminal refusal, cancellation, render failure, root borrowing and recovery.
Existing dense color-oracle/edge/tail tests preserve exact managed results.

One immutable core/embedded build per OS is reused. Public/differential/harness/
native/selector/fixture builds are zero. The SAME actual private0.2.1Release package
and all3examples pass both OSs, including frozen opaque live sync/async retained
results. PackageSHA256`3e7fcc04037988e46120b11a2820329a7e212fa6bc90834a4cf167ee5688d196`,
423205B; Windowscore18da78b2…/Linuxcoredddc1c9f…. Published packages are unchanged.

Raw`vp8-overwrite-output-qualified.tar.xz`,1965612B/144members,
SHA256`abe549dbd45a6426d04608631cebe2ae47681c3283d3559b53c6aaca2bffa747`,
is mirrored/member-verified. Sources/modules, originals, every screen/confirmation,
guards, memory/full/package logs and prior-archive input providers are retained.
One diagnostic reflection lookup failed before correction; its log remains.
Encoder packets, validation/lossless/other-codec/editing/text paths are unchanged.
The next decoder hypothesis concerns repeated live coefficient-window preparation;
this allocation gain does not establish that hypothesis.
