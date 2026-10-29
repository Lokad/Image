# Reuse folded CRC in complete PNG scans

The existing 513×259 stored RGBA16 control independently falls from 333.693 to
237.983 µs: **28.7% less elapsed**, unchanged 8,568 B/op. Its 0.750× FFmpeg /
upper 0.760 passes the warm single-CPU target. Both raw 99.9% confidence views
and conservative normalized envelopes separate. The earlier independent
12-iteration baseline is reused. Fresh three-iteration screens and every raw
sample are retained.

| Existing complete operation | Lokad µs | Ratio / upper | Allocated B/op | Status |
| --- | ---: | ---: | ---: | --- |
| Stored RGBA16 decode | 237.983 | 0.750 / 0.760 | 8,568 | Pass |
| PNG8 span validation | 1,425.169 | 0.986 / 1.020 | 240 | Confidence open |
| PNG16 span validation | 2,943.210 | 0.943 / 0.955 | 240 | Pass |
| Indexed Adam7 validation | 5.562 | 1.366 / 1.423 | 1,168 | Gap |
| PNG16 async nonseekable validation | 2,984.361 | 0.811 / 0.843 | 8,544 | Pass |

One call in PngCodec.Scan now uses the already qualified four-residue CRC kernel
on the identical type+payload span. Polynomial, validation/error order, short/
portable fallback, cancellation, memory/limits and output ownership stay. No new
tables, buffers, owners, caches, unsafe code or algorithm copies. PNG encoding
and metadata-only scans already use this kernel and their timed paths are unchanged.

The original stored/None PNG packet stays byte-identical, with exact words,
alpha, hidden RGB and independent outputs. Both subjects include complete decode,
container/CRC/framing/predictor/endian normalization/output ownership and release.
Validation keeps its frozen typed 36-byte descriptor and natural-frame reference;
async includes acquisition/copies/EOF/release. Inputs/settings/workers and quiet
guards remain fixed. Allocation means managed B/op, not native or peak storage.

All 5,082 checks per OS pass, including 948 live comparisons, 117 focused
normal/scalar and 452 public scalar checks per OS. The same actual private 0.2.1
Release package and all three examples pass Windows/Linux. One core build per OS;
zero harness/native/selector builds or new tests/inputs. Word validators pass and stop.
PNG8's two 12-iteration jobs retain an upper above one after a bounded resolution;
no further retiming. Indexed Adam7 remains a gap. No causal gain is inferred from
unmatched validation costs. The original shared producer header is restored exactly.

The shared call affects 57 existing named controls: 47 resident decodes, five
stream/async/composition boundaries, one caller-root operation retaining two
outputs and four validators. This round refreshes five;
the other 52 CPU costs need reconciliation before current pass claims. Their functional/native
proofs remain usable. Unrelated codecs, editing/text on decoded buffers, PNG
encoding and metadata-only paths retain unchanged evidence. No matrix expansion.

[Evidence](warm-png-scan-crc-reuse-evidence.json) names every affected control,
records identities and retains the unnecessary pre-change Up confirmation as
supplemental history. That case already had a valid pass; no new gain is claimed.
Mirrored/member-verified raw archive: png-scan-crc-reuse-qualified.tar.xz,
226 members, SHA256
f367cad0552befdb5b37ad19f8976e384e93f052433f257ba1a786840e4fdab3.
