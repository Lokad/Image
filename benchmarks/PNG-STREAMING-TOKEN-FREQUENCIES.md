# Streaming PNG token frequencies

PFC554, 2026-11-08. Accepted. The greedy parser now produces symbol frequencies
with accepted tokens, removing the block's second scan of its token buffer.
The existing 335-int census stays live through parsing, tree construction, pricing
and emission, then clears after its final reader. One expanded-byte scalar persists
across fragmented writes; counters publish before callbacks and on exceptional
exits. No array, cache, pool, session or storage owner is added. Lazy parsing keeps
its existing census. Candidate order, ties, tokens, tree decisions, cancellation,
partial failures and complete packets remain exact.

[Machine evidence](png-streaming-token-frequencies-evidence.json) owns all fifteen
current encoding costs, source projections, comparisons and qualification.

| Complete 513×259 one-shot Fastest1 control | Lokad / FFmpeg µs | Ratio upper | GC B/op | Status |
| --- | ---: | ---: | ---: | --- |
| RGBA8 | 4,427.44 / 4,340.50 | 1.040 | 820,916 | Gap |
| RGBA16 | 17,956.64 / 18,627.96 | 0.970 | 2,437,660 | Pass; stop |
| RGBA16, live token | 18,279.79 / 18,615.61 | 0.987 | 2,437,660 | Pass; stop |
| RGBA16, forward output | 18,019.73 / 18,642.40 | 0.982 | 2,437,692 | Pass; stop |

Live word independently saves **515.94–550.27 µs** against both unchanged original
launches: 2.74–2.92% raw / 2.70–3.06% reference-normalized. Ordinary word saves
**556.90 µs**, 3.01% raw / 2.95% normalized. Filtered/all-actual raw and normalized
confidence intervals separate for both word controls. Byte's filtered 113.00 µs
gain has overlapping all-actual intervals; it is not a robust incremental gain
claim. The new forward cost establishes passing current performance, without a
separate causal historical gain claim.

All fifteen existing encoding comparisons refresh because the shared Block guards
and private field layout changed: **12 pass, 2 gap, 1 uncertain**. Byte Fastest
(~87 µs) and grayscale8 (~60 µs) remain gaps; word Smallest remains uncertain.
Other codec/decoder/editing/text sources retain valid evidence. No new shape,
quality, filter or fixture grid is introduced. These fifteen displayed rows
refresh while 110 reuse unchanged paths; this is not a whole-library CPU pass or
an actual-package 125-row release refresh.

Each confirmation retains twelve actuals, both native brackets, estimator
exclusions and idle-v2 guards. Both sides include fresh codec/zlib/workspace,
row/sample packing, encoding, independently owned complete output and disposal;
native word-frame endian preparation stays timed. Frozen sources/settings,
236,828/968,216-byte Fastest packets, all 16-bit samples, alpha/hidden RGB and
output ownership remain exact. The reused PLW552 profile locates the old parser
and census work; its instrumented elapsed is discarded, without projected gains.

Admission stays **384 KiB**, with zero retained operation scratch. A four-byte
scalar fits the existing coarse allowance; substantial array capacities and owners
are unchanged. Primary measured allocations are unchanged; grayscale8 measures
408 B/op less, without a capacity-saving claim. Total GC is distinct from live
capacity; complete physical/admitted peaks and native allocations are not measured.
The borrowed census survives slides/callbacks because its array identity is fixed;
tree construction copies counts and outputs retain independent ownership.

Windows/Linux each pass **5,879 full/live checks** plus 43 internal / 246 public
focused checks in each normal/scalar mode. The SAME actual Release package and
all three examples pass both OSes, including frozen Fastest8/16 sync/async packets,
shared admission, cancellation/refusal cleanup and surviving independent owners.
One immutable main/embedded build per OS serves the screens, confirmations and
qualification. No source correction or fixture/harness/native/selector/public-test
rebuild, favorable repeat or publication follows. Prior packages are retained.

Private 0.2.1 package SHA-256:
`6eb84ba6091c1b27b9dcfa8ed0b4346f00c55b70014f712ac34402adce07cd5b`
(428,417 B). The ignored raw `png-streaming-token-frequencies-qualified.tar.xz`
has its identity, verified mirror/member count and dependencies in the evidence.
It retains original/current sources, modules, actuals, commands, guards, package
and qualification. Earlier unselected prefix/hash trials remain separate, with
their adverse results preserved and no renderer dependency.

Next: stop passing word controls and choose the larger remaining ordinary RGB444
JPEG/live opaque VP8/mixed-word resize gaps. Preserve the full 283-path objective;
compression correctness remains first and publication is postponed.
