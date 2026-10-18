# PNG RGBA inflation within the original working limit

Four existing tight-limit PNG comparisons improve **19.2–40.7%**. They remain
warm parity gaps: byte inputs measure **1.45–1.48×**, words **1.07–1.08×**.
These are qualified three-actual screens, not independent parity confirmations.
The other 203 selected comparisons, including all 58 passing proofs, retain
their hot paths and evidence. The full 207-case parity objective remains open.

| Existing selector | Lokad µs | FFmpeg µs | Ratio / upper | Managed B/op | Time reduction |
| --- | ---: | ---: | ---: | ---: | ---: |
| DecodeTight8 | 2,028.73 | 1,395.91 | 1.453 / 1.597 | 68,200 | 22.1% |
| DecodeTightCancelable8 | 2,067.97 | 1,397.05 | 1.480 / 1.826 | 68,200 | 19.2% |
| DecodeTight16 | 3,103.58 | 2,902.28 | 1.069 / 1.242 | 70,248 | 40.5% |
| DecodeTightCancelable16 | 3,120.43 | 2,904.38 | 1.074 / 1.145 | 70,248 | 40.7% |
| Decode8 | 1,895.65 | 1,394.00 | 1.360 / 1.394 | 4,456 | 0.5% |
| Decode16 | 3,010.13 | 2,901.75 | 1.037 / 1.076 | 8,552 | 0.8% |

The final two rows are unchanged default-path controls, within 0.8% of their
retained means. Their existing independent selected evidence is reused. No new
input, selector, shape, filter or quality grid was introduced: the two original
513×259 RGBA8/16 rasters, frozen None packets, ordinary/live tokens and exact
working limits are unchanged. All pixels, alpha, hidden RGB, source bytes,
one-byte-below rejection and independent owners pass exact gates. FFmpeg CLI
normal/scalar pixels agree. Native allocations remain unmeasured.

The tight path previously validated Deflate, then inflated it again through the
BCL. A separate owned streaming walker now writes raw row payloads into the
private result. A 64 KiB sliding buffer keeps the preceding 32 KiB of raw history;
at most 64 KiB stores row filters. Both fit inside the original 256 KiB inflater
allowance, with framing scratch borrowing 4 KiB until validation completes.
The original result/two-row working peak stays exact; taller inputs fall back.
Predictors and byte conversion run only after each original row work charge.
Complete framing, complete trees, valid filters and Adler are required;
uncertain outcomes replay the original BCL raster reader without recharging
framing. Per-symbol bit charges, cancellation, failure order and ownership stay.

The new history/filter/previous-row arrays increase **managed allocations** to
68,200/70,248 B/op. This is a CPU gain within the same logical admission peak;
B/op does not measure native workspace or total process RSS. No decoded pixels,
descriptors or cross-operation outputs are cached.

Removing only the new helpers restores the entire prior `ZlibFraming` source;
removing the new tight-RGBA branch and its comment restores the entire prior
`PngCodec` source. The original validator, full filtered materializer, counting
loop, raster reader, predictors and packing helpers are unchanged. Default,
Stream/composed, Adam7 and non-RGBA fixed cases retain those paths. Encodes,
metadata, other codecs and editing are unaffected.

Six final screens run sequentially, in batches of at most three, on one immutable
build with the prepared worker, harness, selector and fixtures. Each retains all
three 200 ms actuals, 50 ms native pre/post samples and both 5 s quiet guards.
Means were read before qualification. Since means still exceed parity, no routine
12-actual confirmation or profile was added. Two preliminary variants and their
nine screens are retained: one motivated isolation of the original validator;
the other moved row reconstruction behind its original work admission. Neither
received full qualification or supplies a current selected result.

Windows/Linux each pass **3,672 Release cases**: 1,357 internal, 1,417 public and
898 live FFmpeg comparisons, plus **253 scalar controls** (118 Deflate +135 PNG).
New checks cover nonzero overlapping/max-distance history across slides, literal
truncation/work errors, per-row admission before mutation, all filters, word byte
order, exact peaks, owners and the bounded filter-storage/taller fallback.
The same actual Release package passes its isolated consumer and all three
examples on both OSes; measured production bytes survive qualification unchanged.
The published 0.1.0 package/tag are preserved. Unaffected qualification is reused.

[Numeric evidence](warm-png-streaming-rows-evidence.json) retains all six final
screens, allocations, brackets, guards, exact source proof and package identities.
Immutable raw archives retain both preliminary variants and qualification. The
[current summary](../docs/benchmark.md) selects only the four changed tight cases.
Next prioritize the larger opaque VP8 live-token gap; stop unchanged passing paths.
