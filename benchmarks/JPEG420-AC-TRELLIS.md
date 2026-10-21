# Default JPEG420 AC path selection

C146 adds one bounded AC trellis after the existing zero-only pass, then clears
AC in wholly invisible interleaved luma padding blocks. DC, quantization tables,
partial edge blocks and chroma sampling remain intact. A last-nonzero DAG uses
the existing geometric prices and sampling-cell-weighted source-DCT squared
error. Only strictly lower estimated AC bits and an improved objective replace
the current block. No reference image or measured quality enters the algorithm.

| Existing 513×259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original 4:2:0, 8-bit input | 55,797 | 51,379 / 52,093 | 0.085506 dB |
| Original 4:2:0, 16-bit input | 55,834 | 51,471 / 52,117 | 0.085851 dB |

Both complete packets pass the unchanged 0.25 dB matched-quality gate, exact
alpha and decoder agreement (maximum 3/4, RMSE 0.562393/0.566679). Source PSNR
improves by 0.027386/0.028233 dB from the previous producer. Inputs, sampling,
managed quality69, native qscale5 and0x204060 flattening stay frozen; equal quality
numbers do not establish equivalence. These controls make no photographic,
diagram or general JPEG compression claim. Optional CompactColor stays separate.

All nine affected default boundaries pass: RGBA8/16, forward-only output at both
depths, live cancellation, shared budget, caller RGB, async caller RGB and the
existing row-restart control (51,428/58,012 B). Production packets exactly match
the independent C145 prototypes on Windows/Linux and normal/scalar paths.
An independent reader/writer exactly replays the fixed-table scan and verifies
all coefficients against the prototype; fixed and adaptive output decode alike.
Unchanged 4:4:4, 4:2:2, gray, CompactColor, PNG and WebP evidence is retained.

Scratch is 1,484 safe stack bytes once per block call within the admitted 8 KiB;
there is no heap matrix, frame copy or new cache. Each stage charges work before
execution, at most21,937 units plus the existing4,096 per block. Adaptive/async
estimates include this bound. Invisible-luma AC clearing charges64 instead.
Shared admission remains borrowed and refusal terminal. Cancellation, ownership,
replacement peaks and asynchronous storage lifetimes retain their contracts.

Each platform passes 4,965 Release tests (2,052 internal, 1,977 public, 936 live
FFmpeg), plus141 internal and57 public scalar checks. Focused tests cover weighted
decisions, the exact maximum work bound, refusal without unselected commits,
cancellation and subsequent success. Public goldens cover both depths and input/
output routes, the fixed8KiB shared root, terminal refusal and independent output
lifetimes. The same actual private0.2.0 package and all three examples pass on
Windows/Linux. Frozen production binaries are reused throughout qualification;
no benchmark/native/selector/differential build or quality search is repeated.
Existing source-quality evidence is reused by exact packet identity; restart's
existing quality gate is revalidated once. Initial Windows tests caught a work
bound four units short and stale test expectations; these were corrected before
the accepted qualification, with no production rebuild after freezing.

One quiet-VM three-actual caller-RGB screen records **68,162.147 µs / reference
1,579.238 µs**, ratio43.161/upper50.354, **170,648 B/op**. Native allocations are
unmeasured. Both idle-v2 guards pass. Normalization, complete encoding, fresh
packet allocation/growth and ownership/release are timed on one CPU. CPU cost is
accepted for the verified4,418/4,363 B savings; no CPU-parity, speed-improvement or
independent confirmation is claimed. The unaffected C142 forward444 observation
retains its original producer. Eight older420 timing rows remain superseded;
current caller timing replaces its prior cost screen.

[Numeric evidence](jpeg420-ac-trellis-evidence.json) and the
[19-row ledger](JPEG-COMPLETE-PACKET-AUDIT.md) retain tested scope and producers.
JPEG now has13 default packet passes and5 open444 rows; PNG retains15/17 passes.
Full compression, warm CPU parity and replacement readiness remain open.

Mirrored raw archives: `c146-qualified-linux.tar.gz` (2,600,884 B; SHA256
`f910cb1803eb41d97b42d1c86aa35171a6be39d1e29446242df0a3dec2d577a2`),
`c146-qualified-windows.tar.gz` (2,589,159 B; SHA256
`a58736080ee3a821b6abe8fc8c558f8840ceb00753ce22f36985097980b20f7b`).
