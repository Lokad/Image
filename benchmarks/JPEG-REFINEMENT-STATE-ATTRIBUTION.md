# Current JPEG refinement state attribution

The current immutable decoder still spends most identifiable live-token
progressive444 workload samples in refinement. Old fuel/accounting profiles are
superseded for attribution; this capture changes no production code or package.

| Current complete decode, 746 samples | Exclusive samples | Share |
| --- | ---: | ---: |
| Buffered live-token refinement | 430 | 57.64% |
| Scan traversal | 77 | 10.32% |
| Block dispatch/decoding | 60 | 8.04% |
| Inverse transform | 63 | 8.45% |

One one-second capture uses CPU0 and the existing selector, harness, native
worker, frozen 132,961-byte packet and complete public output/disposal boundary.
Every pixel equals the frozen native frame; source, independent output, normal/
scalar CLI and quiet guards pass. No builds. The qualified expected-frame metadata
projection is reused and restored byte for byte in finally. Instrumented elapsed
values are discarded: sample shares are neither component times nor savings.

One untimed projected-source audit on the same packet observes 8,580 live-token
blocks, 27,597 ordinary four-byte refills, 110,388 acquired bytes and 27,214 bytes
acquired again. Block completion returns 23,808 whole lookahead bytes; all 11,043
publications return 29,045. These counts are distinct, not additive savings.
Both normal/live complete decodes retain exact pixels and release the borrowed
root's charges; only the live reader is instrumented. One diagnostic-tool build,
zero production/harness/native/package builds. No allocation or CPU-gain claim.

The [completed scan-state trial](JPEG-SCAN-REFINEMENT-INCONCLUSIVE.md) retains
lookahead across the scan but establishes no useful elapsed gain. It is restored;
stronger field/cancellation tests remain. Instruction-level attribution is next.
The inspected [FFmpeg refinement decoder](https://github.com/FFmpeg/FFmpeg/blob/9047fa1b084f76b1b4d065af2d743df1b40dfb56/libavcodec/mjpegdec.c#L1015)
retains shared reader state and last-nonzero metadata; this is algorithmic insight,
not attribution of its timing. No upstream implementation is copied.

[Evidence](jpeg-refinement-state-attribution-evidence.json) pins producers,
scope, counts, gates and restoration. Current benchmark costs remain unchanged.
Mirrored/member-verified raw archives retain sources, tools and reproduction:

- `jpeg-current-refinement-attribution.tar.xz`: 1,547,684 B / 82 members,
  SHA256 `6ee23eede90b214ecc156d37242a0552453cf68c311190d7058ae17c7df793de`.
- `jpeg-refinement-lookahead-lifetime-audit.tar.xz`: 611,448 B / 29 members,
  SHA256 `46db3d06c4753872822d8f91306cc78a503b2d9843c7df32215fc8942d8ad389`.
