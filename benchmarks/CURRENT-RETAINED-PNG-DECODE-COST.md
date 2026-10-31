# Current retained PNG decoding cost

The existing two-retained-output shared-root control confirms **1.157× FFmpeg /
upper 1.168**, lower 1.149: **3,292.448 µs** and
**1,637,520 allocated B/op**. One operation contains two complete
decodes. This is a confirmed CPU gap on unchanged qualified production.

Both subjects decode the same resident 218,775-byte 513×259 PNG8 packet twice.
The first independently owned RGBA8 image remains alive throughout the second;
both releases are timed. All samples, including hidden RGB, are exact. Managed
admission, uncached output/scratch allocation, retained reservations and refunds
are timed. The finite borrowed root and caller input reservation are prepared
outside timing. Native has no equivalent shared admission API; it includes both
fresh complete FFmpeg decodes and independent frames, overlap and release.
Startup, I/O, IPC, fixture generation and validation are excluded.

The actual governed peak is **1,766,641 bytes**, including the caller's
218,775-byte input and two 531,468-byte retained images, under one 32 MiB root.
Mutating/releasing one output preserves the other and its charge. Borrowed-root,
temporary-charge cleanup, terminal refusal, cancellation and subsequent-success
checks pass. Native allocation is unmeasured. Older peaks and costs predate
decoder/ownership cleanup and do not establish a causal improvement.

One independent twelve retains every actual, both native brackets and the
filtered/unfiltered confidence views. The cached selector invokes four complete
two-decode operations per actual iteration; BDN reports four operations and the
existing parser normalizes time and GC totals by that count once. Native samples
already count complete two-decode operations. Quiet-v2 guards, input/native
identities, exact samples, source and independent ownership all pass.

No production, test, harness, selector or native build, profile, new input or
timing repeat. The immutable A128 library reuses its full 5,267-check/OS Windows/
Linux/live and same actual Release-package/three-example qualification. This
already required ownership boundary becomes displayed comparison 69; all 268
material paths remain required. No full parity or release-wide refresh claim.

[Numeric evidence](current-retained-png-decode-cost-evidence.json) records every
actual, allocation, governed peak, scope and producer. Raw
current-retained-png-decode-cost.tar.xz, SHA256
4cad0b918916e1d1a0bb661ca2a6e4fb81ea9db5c529e593a2ca7224f1dd836f,
is mirrored with all 36 members verified, including actual library/harness,
selector, worker, input, source, scores and guards. Dependencies pin unchanged
qualification. Reproduce the named control using the
[reference tooling](../tools/WarmReference/README.md).
