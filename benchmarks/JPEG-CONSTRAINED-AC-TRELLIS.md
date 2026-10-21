# Source-constrained JPEG444 trellis

C142 refines the existing Original-policy 4:4:4 decisions with a bounded AC
trellis. A last-nonzero-position DAG prices zero gaps, ZRL, categories, amplitudes
and EOB. It searches the fixed source-error budget using the original lambda,
then zero and six bisection probes when needed. Only a cheaper feasible path
improving the original objective is accepted. Source data governs the search;
Original DQT/DC, the geometric trainer and frozen reference settings stay fixed.
Both controls retain managed quality69/native qscale5 and0x204060 flattening;
the unchanged reconstruction gate establishes the match, rather than quality numbers.

| Existing 513×259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original 4:4:4, RGBA8 | 149,037 | 142,580 / 133,041 | 0.188932 dB |
| Original 4:4:4, RGBA16 | 148,972 | 142,544 / 133,043 | 0.195266 dB |

Savings are 6,457/6,428 B. Both pass the unchanged 0.25 dB gate, exact alpha and
decoder agreement (maximum 3, RMSE below 0.229). Actual packets exactly reproduce
the independent C141 candidates on both platforms and normal/scalar paths. The
packet gap remains about 7.2%; these controls do not establish photographic,
diagram or general JPEG compression claims. Optional CompactColor remains
separate. Passing grayscale, 4:2:2 and WebP controls stay stopped.

Trellis scratch uses 1,612 stack bytes once per MCU, rebuilding one frequency's
bank as needed. It fits the existing admitted 8 KiB alongside the model, MCU and
local scratch; no heap bank matrix or frame copy is added. Work is bounded by
521,904 per trellis MCU, or 667,440 including the local pass (222,480/block).
Adaptive and async estimates include this bound; trainer and default work limits
remain unchanged. Work/cancellation checks precede each bounded stage, and failed
planning cannot commit an unselected path.

Each platform passes 4,953 Release tests (2,043 internal, 1,974 public, 936 live
FFmpeg), plus 132 internal and 54 public scalar checks. Nine focused trellis
cases cover runs/EOB, ties, eight-probe work bounds, refusal and cancellation.
Public goldens cover RGBA/caller, fixed/adaptive, forward/live-cancellable/async
boundaries at both depths, including an 8 KiB shared root. A focused refusal test
checks terminal admission failure, cleanup and subsequent success after release.
The same actual private 0.2.0 package and all three examples pass on Windows/Linux.
One immutable production build/platform and one test build/project/platform are
reused. Unchanged reference/quality evidence is reused by exact packet identity.

One quiet-VM three-actual forward-only screen records **796,455.405 µs / reference
3,780.724 µs**, ratio 210.662/upper 221.706, **564,192 B/op**. Both idle-v2 guards
pass; native allocations are unmeasured. The previous producer recorded
106,257.837 µs and 563,924 B/op. This large CPU cost is accepted for the measured
compression gain under the current priority; no CPU-parity, speed improvement or
confirmation is claimed. Normalization, source analysis, complete encode and
fresh output ownership/release are timed on one CPU. The unchanged harness's
`fixed-geometric-v1` text is a historical selector; actual coding uses the trellis,
source-trained prices and adaptive Huffman.

The earlier unconstrained-path experiment saved four bytes and was stopped
before quality/16-bit expansion. Its source-budget failures motivated the bounded
search; its raw evidence remains in the C141 prototype master.

[Numeric evidence](jpeg-constrained-ac-trellis-evidence.json) and the
[19-row ledger](JPEG-COMPLETE-PACKET-AUDIT.md) retain scope and tested producers.
Mirrored raw archives: `c142-qualified-linux.tar.gz` (2,783,338 B; SHA256
`a49bb472555dada3fef3ea4db44189cc538c5c229b401b2b4fcc5620a102b8da`),
`c142-qualified-windows.tar.gz` (2,768,852 B; SHA256
`f3657cca19f0a6b0e43899cb3aade57f542342e9f732eddcd67bd02fb4aa77f8`).
