# Current opaque lossless WebP confidence

The frozen 513×259 opaque RGBA8 control remains **uncertain** on the actual
current package DLL: 73.41ms / libwebp73.50ms, ratio .999, conservative range
.915–1.070. This is measurement evidence, not a production speedup or a demonstrated
optimization gap. Do not repeat the unchanged job for a favorable bound.

The reference is explicitly **libwebp exact=1, FFmpeg's encoder backend**.
Complete packets remain exactly121,896B, SHA256
`465b70013a8a598ec5e95e2e4d8fff3042317fa21dc31f2350fe9edb46ce465f`.
All source pixels/settings, complete packets, hidden RGB/alpha, syntax, normal/
scalar CLI, source immutability and independent surviving-output checks pass.

This is the first adjacent-pair capture for this existing opaque control. The
established driver measures12 alternating adjacent pairs, four complete operations
per subject, and12 native batches before/after. It retains every actual and uses
the wider of paired log-ratio and all-actual/diagnostic confidence against the
fastest native bracket. Native before/paired/after means are73.496/76.703/75.010ms;
their variation remains visible. The prior global-bracket result stays historical.

Managed public EncodeWebp/admission, full source processing, fresh MemoryStream
growth, independent packet and disposal remain timed. Native pixel import,
encoding and independently owned output remain timed; declared wrapper/input/
scratch state is retained. This does not establish equivalent fresh-context costs.
Input opacity projection, I/O, IPC, setup, JIT and validation remain excluded equally.

Direct managed allocation is4,822,760B/op. Its408B difference from the earlier BDN
counter is a measurement-boundary observation, not an allocation saving. Retained
operation scratch is zero; no new capacity or state is introduced. Governed peak
for this opaque control, physical/native/JIT peaks and default-pool retention are
not remeasured. Packet length is distinct from allocation and retained capacity.

One offline development-driver build adds only the existing harness name to its
allowlist; reverting that string reconstructs the previous source. Statistics,
ordering, batching, validation and operation boundaries are unchanged. No production,
embedded/public-test, harness, native, fixture or package build, new workload grid,
repeated broad qualification or timing repeat. CPU0/idle-v2 quiet guards pass.
The [6,439-check Windows/Linux and same-package qualification](CONTIGUOUS-WIDE-SOURCE-VIEWS.md)
is reused; timed DLL60dd553a is the actual private0.2.1 packagebdbeb557 DLL.

[Evidence](webp-opaque-current-confidence-evidence.json) and mirrored/member-verified
raw `webp-opaque-current-confidence.tar.xz` —900,612B/33members,
SHA256 `d7ab510f3ef974ace51fd76a09593f37504dc8cb4caf2bb06bfd1bc2224ba99d` —
retain driver source/project/binary, frozen harness, all pairs/brackets, output checks,
identities and reproduction. Dependencies retain the actual package and reference.
Lossless/alpha remains6pass/2uncertain; photograph and byte alpha stay stopped.
Continue independent demonstrated work. All289material paths remain; publication
is postponed.
