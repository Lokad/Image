# PNG RGBA8 Optimal packet parity

C161 extends the existing deeper `Optimal` search policy to RGBA8. The public
option remains `Optimal`; the actual comparison is **managed backend9 versus
frozen FFmpeg6**, with filter None. This is not an equal-effort comparison.
Reference inputs, settings and packet hash remain unchanged. Explicit compression
choices and16-bit output keep their previous backend selections.

| Existing513×259 RGBA8 boundary | Previous B | Current / reference packet B | Packet gate |
| --- | ---: | ---: | --- |
| Synchronous PNG encode | 218,775 | 209,407 /210,973 | Pass |
| Async encode to fresh nonseekable output | 218,775 | 209,407 /210,973 | Pass |

Each saves9,368 B, including every chunk and framing byte. Exact decoded RGBA,
alpha and hidden RGB agree with the frozen source. The managed packet reproduces
the independently checked existing smallest-size backend output. This closes
the two declared PNG packet gaps; the other15 controls retain valid unchanged
path evidence. These results make no all-image compression claim. A5% IDAT
tolerance or timing pass is not the acceptance gate.

Both platforms pass5,000 Release tests (2,081 internal,1,983 public,936 live
FFmpeg), plus465 internal and157 public PNG scalar checks. Normal/scalar sync
and async packets agree exactly; RGBA16 retains its original packet. Existing
byte-color tests now require complete packets no larger than their frozen
references. Async probes check nonseekable delivery, resource limits, cancellation,
sink failure/recovery, borrowed streams and independent surviving outputs.
Shared admission and row work remain unchanged: one packed row,64KiB IDAT
capacity and the existing1MiB BCL workspace allowance; no new frame/input copy.

The same actual private0.2.0 package and all three examples pass on Windows/Linux.
One core/OS and one portable benchmark build remain frozen through qualification;
the benchmark build is reused on Linux. Decoder fixtures keep their original
bytes. Async encoding uses a distinct expected owned packet. The unchanged
dedicated native nonseekable boundary proof is retained, while current managed
staging/ownership/cancellation is probed on every setup. The existing PNG driver
is reused byte-for-byte, with actual files included in current raw evidence.
No native, differential or generated-driver build is repeated.

One quiet-VM three-actual complete sync operation records **14,417.461 µs /
reference17,231.327 µs**, ratio0.837/upper0.885, **527,580 B/op**. Both versioned
idle guards pass; native allocations are unmeasured. The previous level6 sync mean was5,406.297 µs. More search costs CPU;
this short screen is recorded as a cost, without independent confirmation or
a CPU-parity claim. Source preparation, startup, I/O, IPC and verification are
excluded; complete compression, admission, output growth/ownership and release
are included, on one CPU/thread with normal SIMD.

Only the default RGBA8 sync/async encode costs change in the finite warm ledger.
The sync screen replaces its earlier producer; the async timing is superseded
and awaits later CPU qualification. Earlier compound PNG8 crop/nearest/encode
costs also contain the changed encoder and remain historical. Other PNG inputs,
explicit-mode/16-bit encodes, JPEG and WebP paths retain their evidence.
JPEG444 compression and the full warm single-CPU objective remain open.

[Numeric evidence and raw archive hashes](png-optimal-rgba8-evidence.json) identify
the actual builds, package, packets and retained receipts. The
[complete-packet ledger](PNG-COMPLETE-PACKET-AUDIT.md) now records17/17 declared
PNG passes; [the benchmark summary](../docs/benchmark.md) retains three tables.
