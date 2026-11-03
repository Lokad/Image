# WebP scoring preparation

Two fixed integer coefficient sequences replace repeated residual-bias weight
generation. Prediction, color and ALPH share one short bias kernel, preserving
each signed pair's coupled rounding. Compact green/cache alphabets retain direct
scanning; larger cache alphabets keep prepared population models. Prepared
references are admitted only when a dense user actually needs them. Selection,
ties, complete packets, metadata and reconstructed pixels remain unchanged.

| Current complete encode, global brackets | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original RGBA8 / libwebp exact=1 | 119,655.48 | 132,957.76 | 0.900 / 0.912 | 7,476,640 | 183,812 |
| Opaque RGBA8 / libwebp exact=1 | 74,136.07 | 73,214.47 | 1.013 / 1.033 | 5,983,424 | 121,896 |
| Photograph / libwebp exact=1 | 90,209.39 | 86,127.74 | 1.047 / 1.078 | 6,517,254 | 160,032 |
| Text/diagram / libwebp exact=1 | 8,439.00 | 11,032.69 | 0.765 / 0.781 | 4,560,910 | 7,234 |
| Transparent VP8 RGBA8 / FFmpeg | 72,338.27 | 58,800.38 | 1.230 / 1.242 | 9,685,968 | 98,590 |
| Transparent VP8 RGBA16 / FFmpeg | 73,201.64 | 58,709.71 | 1.247 / 1.261 | 9,685,968 | 98,590 |

Original and diagram retain passing evidence and stop. Transparent controls retain
gaps. Photograph/opaque global-bracket intervals cross parity; the fresh
interleaved confirmations below independently establish both remaining gaps and
incremental gains. These are distinct estimators: do not subtract historical table
times to infer this stage's gain or substitute paired ratios for aggregate times.

| Fresh interleaved comparison | Before → after paired ratio | Current 99.9% interval | Raw saved / gain | Normalized gain | Allocation saved B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| Photograph VP8L | 1.138 → 1.110 | 1.100–1.119 | 3.276 ms / 3.21% | 2.49% | 44,096 |
| Opaque RGBA8 VP8L | 1.084 → 1.049 | 1.043–1.054 | 2.081 ms / 2.54% | 3.24% | 48,280 |

Filtered/global actuals and all twelve paired samples are retained. Both fresh
comparisons separate raw and normalized confidence; native clock drift is
−0.74%/+0.73%. Paired ratios favor native by using the fastest of six adjacent
native samples around two complete managed calls, with all twelve ratios in the
pinned BDN interval and no filtering. Existing compiled methods/workers and all
original input/settings/packet/ownership gates are reused. Complete preparation,
normalization and independent output acquisition/disposal remain timed.

Separate complete-operation diagnostics save 44,096 B/photo, 8,744 B/transparent
encode and 7,440 B/small authored control; diagram allocation is unchanged.
Governed peaks remain 3,470,059 / 3,386,043 / 3,847,888 / 312,716 B respectively
for photo/diagram/transparency/authored controls. These include overlapping owned
capacity and exclude borrowed inputs. Original/opaque governed peaks, native
allocation and physical heap peaks remain unmeasured. Per-operation scratch
retention is zero. The two input-independent coefficient arrays retain **256 B
payload plus minor headers per process**; their warm initialization is excluded
like existing mathematical constants. This is not a per-input cache.

Compact clustering fits the isolated 16 KiB capacity again. A new refusal test
admits references, refuses their first run map, keeps actual capacity charged
through cleanup and then succeeds using the same borrowed root. Actual reference
and map capacity is reserved before allocation, including partial preparation;
no retry, waiting, lower-memory fallback or new admission abstraction.

The intermediate hybrid stage had modest photo/alpha points and an adverse
original point. Fusing bias into every active entropy bin was exact but materially
adverse on photo; that traversal was removed. All stage sources, binaries and
adverse screens remain retained. The completed revision shares invariant
preparation while preserving useful separate kernels; no candidate was replayed
to obtain favorable numbers.

Windows/Linux each pass **5,507 full/live tests**, 305 internal and 194 public WebP
controls per normal/scalar mode. Existing 65,536 scalar color-cost oracle cases,
prediction/extrema checks, frozen maps and complete packets pass. The **same actual
Release package** and all three examples pass both OSes, including sync/async
packets, hidden RGB, shared admission and independently surviving outputs.

Each stage reuses one immutable production/internal build per OS. Existing public
harnesses, selectors, native workers and paired tool require no rebuild. Six
global current-cost jobs and four fresh paired jobs run sequentially on the quiet
single-CPU VM with guards. Other adaptive VP8L/ALPH costs remain historical;
unrelated codecs, reading, resizing, drawing and text reuse unchanged evidence.

[Evidence](webp-scoring-preparation-evidence.json) pins compact current costs,
paired comparisons, source/package identities and qualification. Raw
`webp-scoring-preparation.tar.xz`: 32,999,028 B / 1,317 members, SHA256
`ea578b0ee58a17382a77b8118d39aa8f263fe23ea5258e238d3459ba959f4965`,
mirrored and member-verified. Dependencies retain prior prepared-cost, paired
method and attribution evidence. Full 282-path parity is open; publication remains
postponed.
