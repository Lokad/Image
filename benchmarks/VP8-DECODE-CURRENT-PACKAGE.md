# VP8 decode on the current package

Five original owned-output VP8 comparisons remain uncertain on the current
0.2.1 package. Exact pixels, source ownership and reference checks pass.

| Complete operation | Lokad.Image µs | FFmpeg µs | Conservative interval | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Opaque with live cancellation token | 4,969.17 | 4,821.00 | 0.923035–1.080555 | 230,880 |
| Transparent, fresh seekable stream | 5,845.96 | 6,008.13 | 0.871249–1.019694 | 240,496 |
| Transparent, resident | 5,968.06 | 6,009.20 | 0.901183–1.044184 | 240,400 |
| Opaque with simple filter | 4,984.92 | 4,925.66 | 0.912091–1.066994 | 230,880 |
| Opaque, resident | 4,945.38 | 4,843.16 | 0.919620–1.074378 | 230,880 |

The inputs, outputs, fixtures and reference identities match the earlier
[fixed-position round](VP8-FIXED-CODE.md). The original paired driver runs twelve
alternating pairs of 128 complete operations, with twelve equal reference batches
before and after. Managed parsing, cancellation, normalization, fresh stream
acquisition where applicable, and independently owned output remain timed.
FFmpeg retains its declared reference state and decodes every image. The
original normal/scalar checks and both VM guards pass. The coding-only VP8
validator retains its evidence because it returns a descriptor and skips the
changed output pool.

In the live-token case the first native bracket averages 5,132.9 µs and the
last 4,821.0 µs. Managed and adjacent native batches also move together.
The paired interval is 0.982905–1.007425, while the full conservative rule
keeps the upper at 1.080555. In three other resident cases the native brackets
move in the same direction. The cause remains unverified. The stream and
transparent resident comparisons pass under paired intervals alone, while the
full gate remains open. These measurements establish neither a demonstrated
production slowdown nor passing parity. Further sampling needs a concrete
way to address the timing phases while preserving the full gate.

The immutable package and driver are reused. Allocations match the previous
measurements. They are separate from the owned output capacity and from
unmeasured physical peaks. The mirrored `vp8-decode-current-package-confirmations.tar.xz`
contains 36 verified members and 34,744 B, SHA-256
`b56bde45b1a954159b2e9e65221be8075a5125aa05a4c8ba4395b01392671ab6`. It retains original and current samples,
selection commands, VM guards and quality/ownership receipts.
[Evidence](vp8-decode-current-package-evidence.json)
