# Exact constant-time PNG length classification

LSF584 replaces the repeated Deflate length-range search with its exact integer
logarithm formula and permits inlining. Lengths 3–10 keep their direct mapping;
258 keeps its dedicated symbol. For 11–257, the original doubling interval gives
`extra = floor(log2(length - 3)) - 2` (RFC1951 section3.2.5).
Token representation, frequencies, trees, block choices, output bits, cancellation
and ownership are unchanged. No table, array, cache, session or API is added.

The independent actual-token-writer oracle covers every legal length and distance
bucket edge, plus canonical alignment and output-buffer boundaries. Complete
frozen packets, fragmented writes, writer failures, accepted prefixes, shared
admission and subsequent recovery retain their existing checks.

The existing RGBA8 Fastest1 confirmation measures **4,343.15 / 4,342.02 µs**, upper
**1.017355: uncertain**. Its 84.29µs/1.90% point saving has separated filtered
raw/normalized intervals, but all-actual intervals overlap. A robust gain is not
established. The existing RGBA16 generalization measures **17,776.55 / 18,600.15
µs**, upper **0.966245: pass**. Its raw 180.09µs/1.00% saving separates in both
views; reference-normalized intervals overlap. No robust normalized gain is
claimed. Original source-valid confirmations are reused, without favorable repeats.

All fifteen affected complete encodes now have current costs: **13 pass/stop,
zero demonstrated gaps, two uncertain**. The previous Smallest16 uncertainty now
passes. Fastest8 and grayscale8 remain uncertain; resolve their confidence before
assuming further optimization is needed. Current costs are in
[machine evidence](png-length-classification-evidence.json). Other refreshed point
differences do not establish blanket causal gains; no separated raw/normalized
regression appears in the bounded review.

Both sides retain fresh one-shot codec/zlib/packet preparation, sample packing,
encoding, independent complete output and disposal. Native word-endian preparation
stays timed. The original 513×259 inputs, settings, delivery modes and exact
complete packet hashes are unchanged; no grid or new fixture is introduced.

Primary GC remains **820,916 / 2,437,660 B/op**. GC allocation is distinct from
live capacity and retention. Admission stays **384KiB**, with zero retained
operation scratch and independently owned output packets. Existing default output
pool retention is separate and unremeasured. Complete physical/native peaks are
not remeasured; no allocation or capacity reduction is claimed.

Windows/Linux each pass **5,907 full/live checks**, plus 43 internal/246 public per
normal/scalar mode. One immutable main/embedded build per OS serves the complete
LSF584 experiment and qualification. Two independent confirmations are reused in
the thirteen-case refresh; frozen workers/harnesses/selectors/fixtures are reused.
The same actual private Release package and all three usage examples pass both
OSes, including byte/word sync/async packets, admission, cancellation and survivors.
Only new packages/receipt transfer for Linux; unchanged consumer inputs are pinned.

CPU results use Linux DLL `39d95e02…`; Windows DLL `92eae3b8…` is packaged in
`8b6c2a50…` (430,742B), symbols `523c8af0…`. This development round is not a full
actual-package refresh of all126 displayed comparisons. Published releases stay
immutable; publication and the full283-path parity objective remain open.

[Evidence](png-length-classification-evidence.json) identifies the mirrored,
member-verified raw archive, source projection, mappings, samples, checks and
reproducibility. The earlier [streaming census](PNG-STREAMING-TOKEN-FREQUENCIES.md)
retains its stage gains as history. Unselected
[row/certificate preparation](BICUBIC-ROW-CERTIFICATE-EXPERIMENT.md) and
[private parser publication](PNG-PRIVATE-PARSER-PUBLICATION-EXPERIMENT.md) retain
adverse evidence and exact restoration; they do not supply current displayed costs.
