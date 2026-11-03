# WebP trace output storage

Forward parsing borrows the idle output token capacity for its path and bounded
cost windows. Backtracking writes tokens into the tail, then moves them forward
after all path reads finish. Selected candidates remain independently owned.
Small images retain separate arrays when the physical capacity cannot fit the
state; admission refusal never selects another path or retries.

| Complete warm encode | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original RGBA8 / libwebp exact=1 | 139,765.51 | 124,164.44 | 1.126 / 1.173 | 24,899,424 | 183,812 |
| Opaque RGBA8 / libwebp exact=1 | 79,957.44 | 71,971.99 | 1.111 / 1.202 | 12,957,560 | 121,896 |
| Photograph / libwebp exact=1 | 92,272.76 | 82,578.65 | 1.117 / 1.123 | 13,346,868 | 160,032 |
| Text/diagram / libwebp exact=1 | 9,140.17 | 9,733.81 | 0.939 / 0.959 | 6,256,838 | 7,234 |
| Transparent VP8 / FFmpeg | 64,644.85 | 53,505.59 | 1.208 / 1.217 | 10,749,072 | 98,590 |

Retained for verified storage savings, with CPU parity still open. Independent
confirmations do not establish an incremental normalized CPU gain. The initial
paired direct screen suggested 1.89%/3.25% photograph/transparent gains; independent
confirmation gives only 0.59%/0.70% raw points against the unchanged-path prior
evidence, with overlapping normalized intervals. Neither screen establishes a
qualified speedup.

Adverse ordinary/opaque points are retained. Fresh unchanged baseline
confirmations give 130,066.04/77,582.97 µs: the candidate is 7.46%/3.06% slower in
raw points. Ordinary raw loss separates; opaque raw loss does not. Native timing
brackets drift by 7.46%/2.62% during those candidate jobs, and both normalized loss
intervals overlap. These observations leave causal CPU attribution uncertain.
No candidate job was repeated, retuned or sampled until favorable. Diagram remains
a confirmed pass; the other four controls retain gaps.

Complete direct controls allocate 429,984/429,776/859,552 fewer bytes for
photograph/diagram/transparency; their admitted peaks are respectively
10,166,464/4,802,292/3,847,888 B. Only transparency's peak changes, falling
237,995 B. The smaller authored control exercises the separate-array path and
allocates 208 fewer bytes. For 132,867 pixels, 429,576 already charged bytes can
replace 429,574 bytes of array payload. Existing output capacity stays charged
through every user; no scratch survives disposal. Total GC allocation, admitted
peak capacity and retained storage are distinct. Physical heap peaks, native
allocation and ordinary/opaque admitted peaks are unmeasured.

The overlap proof uses the dense all-literal case: after k+1 tokens consume
S≥k+1 pixels, unread path bytes end at 2(N−S), while the next token starts at
8(N−k−1). Copies only increase separation. Full output capacity is checked
against seed, source and match-index aliases before mutation. Tail moves are
overlap-safe and cancellable; state reservation cleanup precedes token publication.

Windows/Linux each pass 5,476 full/live checks, 284 affected internal and 194
public checks in both normal/scalar modes. Ten new controls cover the legal
27,306/27,307-pixel storage boundary, dense backtracking, idle-capacity aliases,
tail overlap/chunking/empty output, owner guards, cancellation and root reuse.
Existing frozen traces, refusal cleanup, exact packets, hidden RGB and alpha
contracts remain unchanged. Initial added tests used invalid row dimensions;
their corrected runs and original failures are retained without production changes.
The same actual 0.2.1 Release package and three examples pass both OSes.

Complete public encodes retain normalization, preparation, every candidate,
independently owned output and disposal. References retain the disclosed prepared
wrappers; libwebp exact=1 is FFmpeg's encoder backend. Five candidate confirmations
and two fresh baseline checks each retain twelve actuals, native brackets and quiet
guards. One immutable production build per OS serves all checks; no harness,
selector or native rebuild. Other affected encoding costs remain historical;
PNG/JPEG, decoders, opaque VP8 without alpha, editing and text are unchanged.

[Evidence](webp-trace-output-storage-evidence.json) retains source identities,
actuals, ownership checks, adverse results and package proof. Raw
webp-trace-output-storage.tar.xz: 46,019,000 B / 4,710 members,
SHA256 7c75987b28285ad5dc4077f17ce4253232dfaff40d00bf72210af0c654580496,
mirrored/member-verified. The prior qualified package/source and WebP reference
dependencies remain pinned. Publication is postponed.
