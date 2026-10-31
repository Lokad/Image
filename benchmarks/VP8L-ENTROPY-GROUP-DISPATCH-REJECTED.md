# VP8L entropy-group dispatch screen

Historical screen. The identical strategy is now independently qualified on the
current owned-frame decoder: [current group-dispatch evidence](WARM-VP8L-GROUP-DISPATCH.md).

One existing 513×259 resident RGBA8 decode, frozen 183,812-byte libwebp exact=1
packet. Complete public decode, independently owned output and disposal; one warm
CPU. All 531,468 output bytes, alpha and hidden RGB remain exact.

| Quiet short screen | Managed µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Qualified baseline | 4,042.930 | 3,019.029 | 1.339 / 2.218 | 1,216,179 |
| Rejected candidate | 3,852.483 | 3,011.909 | 1.279 / 1.372 | 1,216,179 |

The candidate reduced the matched short mean by 4.71%, below the planned 5%
gate. It is rejected and the qualified implementation restored. These three-sample
screens establish neither a confirmed gain nor parity. One fresh unchanged
baseline resolves native drift from the preceding VL1 confirmation; all actual
samples, both native brackets and idle-v2 guards are retained. No variants,
independent confirmation or full/package qualification were repeated.

The candidate borrowed immutable Huffman trees and the existing tail lookup until
tile/row boundaries, recomputing coordinates after copy jumps. It added no storage
or owner and preserved the unwritten-tail lookup guard. Reused attribution maps
all 1,970 ReadImage samples to matching instruction starts; the division/indexing
block accounts for only 4.1% of whole samples, not the full 34.7% method cost.

Both platforms pass 37 internal and 41 public focused checks in normal and scalar
modes for the candidate and restored implementation. A retained authored 9×6
fixture covers a 36-pixel overlapping copy across multiple tiles and rows, a
vertical tile boundary and a partial right edge. Independent expected pixels and
FFmpeg agree on both platforms. Existing admission/cancellation/ownership coverage
remains. Production source, matched producers and the actual private package are
restored; applicable [VL1 confirmation](current-vp8l-resident-decode-evidence.json)
and VPS1 package qualification remain valid.

[Evidence](vp8l-entropy-group-dispatch-rejected-evidence.json) records the rejected
producer, exact hashes, samples, restoration, regression and mirrored member-verified
raw archive. Next resolve the wide existing photograph VP8 encoding measurement;
the full 267-path parity objective remains open.
