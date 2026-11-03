# WebP alpha token reuse

Retired main-token capacity now survives one ALPH filter's final emission and
becomes the next compatible filter's matching/parsing seed. Prepared metadata
is disposed normally; selected encoded payloads stay independent. This removes
one full token allocation without a pool or another admission model.

Complete transparent RGBA8/16 operations allocate **8,622,560 B**, down
**1,063,408 B (10.98%)**. The reused token payload is 1,062,936 B. Direct complete-
operation admitted peak stays **3,847,888 B**; retained scratch is zero. Native
allocation and physical heap peaks are unmeasured. Existing fixed bias constants
retain 256 B/process. Allocation, admitted peak and retention are separate metrics.

| Current complete encode | Lokad µs | Reference µs | Ratio / upper | Allocated B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Original RGBA8 / libwebp exact=1 | 118,534.31 | 130,510.77 | 0.908 / 0.916 | 7,476,640 | 183,812 |
| Photograph / libwebp exact=1 | 97,745.17 | 90,242.47 | 1.083 / 1.094 | 6,517,386 | 160,032 |
| Text/diagram / libwebp exact=1 | 7,735.67 | 9,574.91 | 0.808 / 0.835 | 4,560,910 | 7,234 |
| Opaque RGBA8 / libwebp exact=1 | 79,410.78 | 76,132.61 | 1.043 / 1.058 | 5,983,424 | 121,896 |
| RGBA16 normalization / libwebp exact=1 | 120,011.01 | 131,175.44 | 0.915 / 0.923 | 7,476,640 | 183,812 |
| Async nonseekable output / libwebp exact=1 | 118,966.92 | 130,932.63 | 0.909 / 0.919 | 7,477,024 | 183,812 |
| Transparent VP8 RGBA8 / FFmpeg | 68,388.43 | 54,552.56 | 1.254 / 1.353 | 8,622,560 | 98,590 |
| Transparent VP8 RGBA16 / FFmpeg | 70,448.99 | 57,590.91 | 1.223 / 1.235 | 8,622,560 | 98,590 |

Each row has one fresh 12-actual confirmation on the same immutable DLL, frozen
inputs/settings, full preparation/normalization and independent output/disposal.
Native boundaries remain labeled. Original/diagram/depth/async pass and stop;
photograph/opaque/transparency retain CPU gaps. No full-release refresh claim.

There is **no established CPU gain or regression**. The short alpha screen leads
slightly; independent global means are adverse (65.828 → 68.388 ms; normalized
1.231 → 1.254) with overlapping intervals. One justified interleaved check on
the same builds gives normalized 1.226 → 1.222, also overlapping; its apparent
5.12% raw lead accompanies −4.81% native clock drift. All observations remain;
no further favorable sampling or incremental speed claim.

Candidate order, ties, packets, metadata and pixels stay exact. Smaller or
incompatible capacity and packed palettes release spare capacity deterministically;
method6 all-family ownership stays unchanged. Refusal remains terminal. Charges
span the actual users, including replacement overlap, and caller roots stay borrowed.
Four existing direct controls preserve packets and admitted peaks; photo/diagram/
small-authored allocations are unchanged. Their timings are diagnostic only.

Nine new ownership cases cover retained tokens, independently surviving emitted
bytes, next-preparation and second-filter refusal/cancellation, same-root recovery,
palette/all-family/incompatible capacity and validation cleanup. Each OS passes
**5,524 full/live checks**, plus **799 internal / 194 public WebP checks per
normal/scalar mode**. The same actual Release package and three examples pass on
Windows/Linux. Unchanged codec/editing algorithms reuse applicable prior proof.

One production core version per OS, reused throughout. The initial isolated
prototype's source/binary and later source-local ownership cleanup are retained.
Async preflight rejects an older producer proof before timing; existing exact
normal/scalar/native/CLI checks refresh that proof, with five completed measurements
preserved. Retrieval mistakes cause no repeated builds or measurements.

[Evidence](webp-alpha-token-reuse-evidence.json) records compact current costs,
sources, tradeoffs, qualification and raw reproduction. Raw
`webp-alpha-token-reuse-qualified.tar.xz` is mirrored and member-verified:
4,204,040 B / 327 members, SHA256 `4db1eac56708656b7ae8871b910a0cc3b14eb1ebac4d8db99b27510a22a2a3ae`.

Full 282-path warm CPU parity remains open; publication is postponed.
