# VP8 decode on the current package

The first five owned-output VP8 captures were uncertain on the current 0.2.1
package. Partitioned sampling confirms the two transparent cases below. All
exact-pixel, source ownership and reference checks pass.

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
production slowdown nor passing parity. The partitioned captures below test closer timing alignment under the same gate.

The immutable package and driver are reused. Allocations match the previous
measurements. They are separate from the owned output capacity and from
unmeasured physical peaks. The mirrored `vp8-decode-current-package-confirmations.tar.xz`
contains 36 verified members and 34,744 B, SHA-256
`b56bde45b1a954159b2e9e65221be8075a5125aa05a4c8ba4395b01392671ab6`. It retains original and current samples,
selection commands, VM guards and quality/ownership receipts.
[Evidence](vp8-decode-current-package-evidence.json)

## Partitioned confirmation

| Complete operation | Lokad.Image µs | FFmpeg µs | Conservative interval | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Opaque with live cancellation token | 5,198.14 | 5,194.48 | 0.984622–1.016033 | 230,880 | Confidence open |
| Transparent, fresh seekable stream | 6,082.74 | 6,494.10 | 0.924882–0.947445 | 240,496 | Pass |
| Transparent, resident | 6,184.82 | 6,437.62 | 0.947734–0.971896 | 240,400 | Pass |

The original live-token managed/native ratio means for the first and last six
pairs were 0.992 and 0.998, with a larger move between separate reference
brackets. The revised schedule interleaves before, paired and after reference
groups within each actual. Each is split into sixteen eight-operation
subdivisions. Four whole 128-operation warmups per side and twelve full
statistical units per group remain. All subdivisions are retained. The
conservative gate takes the worst paired and all-actual bounds with all three
reference groups. The original diagnostic on upper outliers is retained.

Both transparent cases pass and stop. The live-token interval narrows and still
straddles parity. The complete decode, output and disposal stay inside every
operation. CPU0, natural GC, exact pixels and original stream and owner checks
remain. Differences in measured clocks do not establish a causal production
effect.

An initial stream capture exceeded the post-run guard with 4.2% CPU0 I/O wait
and 1.048% aggregate I/O wait against the 1% ceiling. It remains unscored.
The next run uses memory-backed storage for untimed temporary files and saves
its log after the guard. Both corrected captures pass the original guards.
The timed DLL, harness, native worker and fixtures are identical.

The mirrored `vp8-partitioned-current-package-confirmations.tar.xz` has 35 verified members
and 102,940 B, SHA-256 `2b2174384ebc090ae57bf7a18f8e3428ef8db55691fbb87a0907156ba608dd14`. It retains
the rejected capture, all accepted subdivisions, identities, gates and
reproduction commands.
