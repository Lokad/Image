# Prepared Huffman storage in live JPEG decoding

JLP713 reuses the baseline decoder's existing long-prefix lookup and scan-owned
pair descriptors with a live cancellation token. Complete baseline444 decoding
saves **201.15 µs / 11.18% raw / 11.14% reference-normalized** in the independent
reversed comparison. Filtered and all-actual raw/normalized intervals separate;
native movement is −0.042%. The current conservative upper **0.935 passes**.

| Existing complete 513×259 live-token decode | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Qualified original | 1,799.58 | 1,718.81 | 1.047 / 1.064 | 10,120 |
| Shared lookup and prepared pairs | 1,598.43 | 1,718.09 | 0.930 / 0.935 | 42,936 |

The frozen input remains 133,041 B, SHA256 `c115cd7d17eb2cd3f34e31c439bdd48421e58ea2c0fce264c1cc8ad3eb1f8274`;
the exact independently owned frame remains 531,468 B,
SHA256 `960a8cdce2bdab9b24fed2adb0f48771317c7c8b55ac208299ad91ece48302f4`.
Parsing, table preparation, entropy, IDCT, normalization, output ownership and
disposal stay timed. FFmpeg retains its labeled contexts and has no equivalent
live cancellation token. This is scoped development evidence, not a release-wide
refresh or a general cancellation-cost comparison.

## Design and memory tradeoff

The live window borrows its existing `JpegHuffman` object instead of a short-only
view. It shares the unchanged 9/10-bit lookup and pair builder used by ordinary
decoding. The original 18-bit refill, complete reader/predictor/coefficient state,
following reads and scalar marker/stuffing/truncation grammar remain. Every logical
prefix and nonempty-amplitude cancellation checkpoint remains before its store.
Failed operation-private state is abandoned at the established call boundary.

Eligible scans prepare pairs using the existing owner: select within the local
ceiling, reserve each 16,408-byte table before allocation, borrow through actual
readers, then release in `finally`. Refusal is terminal. The two-table control's
total allocation and governed live peak both increase **32,816 B**; measured peak
541,253→574,069 B. Its result remains charged 531,468 B until disposal; scratch
after disposal is zero. No new pool, session, cache, quota or retained owner exists.
The default idle output pool is observed separately; governed calls bypass it.
Physical/native peaks are unmeasured. A reused generic storage adapter has a stale
packet banner; its wrapper pins the actual JPEG input, assembly and frame identities.

All **23 affected baseline costs** refresh on the immutable actual-package DLL;
six unchanged progressive costs retain their evidence. The 29-read scope is
**19 pass / 7 gaps / 3 uncertain**. No formerly passing affected control becomes a
gap. RGB422 is now a demonstrated gap; resident/stream restart420 and progressive
live444 remain uncertain. Historical companion point differences establish no
causal gain, nonregression or equivalence. The adverse 4:1:1 point is retained,
without favorable remeasurement. Passing cases stop.

## Qualification and reproducibility

Windows/Linux each pass **6,100 full/live checks**, plus 452 internal / 375 public
per normal/scalar/AVX2-disabled/AVX512-disabled mode. New coverage checks combined
DC+long-prefix following reads, live prepared-pair grammar, and actual two-table
refusal/cancellation/recovery with independently surviving results. The same actual
Release package and all three usage examples pass on both OS. Its frozen grouped
entropy consumer exercises ordinary and live-token sync/async ownership boundaries.
Encoder packets, metadata exceptions and exact managed pixels remain unchanged.

One production build per OS serves the entire bounded experiment. Two fixture
setup repairs rebuilt only Windows embedded tests (three versions versus one on
Linux); the production DLL never changed. No benchmark harness, selector, native
worker or fixture build occurs. Full qualification occurs at the coherent stage;
the final immutable build is reused throughout confirmation and qualification.

[Evidence](jpeg-live-prepared-huffman-evidence.json) owns all 29 current read costs,
the independent comparison, exact source/poll/storage proof and qualification.
The motivating live-window capture is attribution, not current phase timing.
[Paired tile packing](JPEG-PAIRED-TILE-PACKING.md) and
[narrow scan preparation](JPEG-NARROW-SCAN-PREPARATION.md) retain earlier gains and
valid unchanged-path evidence.

Mirrored/member-verified raw `jpeg-live-prepared-huffman-qualified.tar.xz`:
6,054,312 B / 383 members, SHA256
`5e152dfb85b0b0371e5f71fc885d7446f29123f313ec875576511e38d428646b`.
Private package 439,569 B, SHA256
`666068f5884f3b43abd6ae8776f481cc7dbdc95a4fc1c3f1ea115b0d4291b235`.
Published packages remain immutable; publication is postponed. All 283 named
material paths remain required. Independent resizing, codec and text work continues.
