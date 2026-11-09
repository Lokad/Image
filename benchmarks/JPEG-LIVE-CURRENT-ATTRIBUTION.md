# Current live progressive JPEG attribution

JPR608 records one two-second capture on qualified Linux `4fa55390`. The frozen
132,961B progressive444 packet and live unsignaled token produce the exact
531,468B independent RGBA frame. Public parsing, preparation, entropy, inverse,
color, ownership and disposal remain included; native retains its declared
codec/input/conversion contexts. Its cancellation setting remains asymmetric.

There are 1,988 sampled locations: buffered cancelable refinement 954, inverse
AVX2 216, Huffman lookup 184, ReadBlock 153, ReadBits 97 and ReadScan 75. The
buffered correction region has 287; its signed update block alone has 92.
Nonzero-mask discovery has 23, offering little justification for an additional
per-block cache. JIT offsets identify locations; they are not phase times or
projected speedups. Per-bit cancellation publication is already conditional.

The subsequent [cached-correction experiment](JPEG-CACHED-CORRECTION-EXPERIMENT.md)
found no useful lead; its adverse screen is retained. These samples do not approve
an implementation or establish savings from an instruction count.

The existing worker/fixtures/selectors are reused with zero builds or statistical
measurements. All frozen native/pixel/source/ownership and idle-v2 gates pass;
CPU0 runs the operation, CPU1 the sampler. Instrumented elapsed is discarded.
The logs establish 64 operations per actual, correcting a copied selector label
without rerunning the capture. Current costs and package qualification remain
unchanged; the full 283-path objective stays open.

[Evidence](jpeg-live-current-attribution-evidence.json) retains source identities,
method/block attribution and output checks. Mirrored/member-verified raw
`jpeg-live-current-attribution-jpr608.tar.xz`, SHA256
`0c7987ade22237920072542d1cb0d2d83ee785b6eefecf1762ede5fa7b0c9a15`,
196,416B/72 members, preserves the trace, maps, JIT, guards, original/corrected
receipt and scripts, with qualified module/worker dependencies linked.

## Current call-boundary audit

JDI676 verifies that the reader, Huffman, decoder and prepared-refinement sources
still match this capture. Decode, caller-sample decode and pixel validation each
construct a fresh private decoder. It never escapes or resumes after failure;
exceptions expose no reader/coefficient state. Scan position publishes after
successful completion. The scan's failure cleanup releases its stack charge;
both top-level cleanup paths return private coefficient storage, and ordinary
decode also disposes unpublished output. Caller-sample writes follow scan completion.

This proves the private-failure boundary, without changing existing internal
partial-state tests. Completed reader state, public failure semantics, live-token
checkpoints and cleanup remain required. The hot buffered reader already uses
locals without enclosing exception handling; failure publication is cold.
Removing that publication alone supplies no credible substantial saving, so this
audit launches no new JPEG build, profile or timing trial. Earlier scan/correction
trials remain leads for materially revised designs, not family prohibitions.
