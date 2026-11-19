# JPEG caller CMYK output and low-frequency inverse transforms

The actual development package `5b67c367`, containing timed Linux DLL
`aaf783c2`, closes the original caller DC-only and resident sparse Adobe CMYK
gaps. Fourteen original decode comparisons and two coding validators pass the
conservative warm single-CPU gate on this DLL. [Case evidence and archive hashes](jpeg-cmyk-caller-output-evidence.json)
record the complete measured set.

| Complete operation | Lokad.Image µs | FFmpeg µs | Conservative ratio interval | Managed GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Caller CMYK, original constant-block control | 301.986 | 354.787 | 0.830517–0.864556 | 536,377 |
| Resident sparse Adobe CMYK | 411.770 | 433.599 | 0.920372–0.963860 | 20,948 |
| Caller CMYK, supplemental nonconstant control | 594.306 | 646.188 | 0.868340–0.973146 | 552,518 |

The caller operations include a fresh destination, full decode, sample delivery
and independent output ownership. The supplemental control retains its original
row padding, exact source and sample checks, and FFmpeg worker. It saves about
549 KB/op of managed allocation versus the paired prior package. The independent
confirmation measures 602.598 µs versus 651.858 µs, upper 0.978072. The first
capture's upper of 1.000819 is retained in the 24-unit pooled result. The
allocation counters measure total allocation, separate from admitted MCU scratch,
borrowed destination capacity and unmeasured physical peaks.

For a terminal baseline scan with equal sampling grids and canonical raw CMYK,
the decoder writes completed MCUs into the caller span. Adobe interpretation,
orientation and other scan layouts keep their separate reconstruction. A failed
call can leave completed pixels in the caller span under the existing public
contract. Tests cover malformed duplicate scans, refusal, cancellation, row
padding and release of the shared budget. The fused entropy path identifies a
first-symbol end-of-block so verified constant blocks use the exact constant
sample calculation. AVX-512 evaluates three frequency rows when the actual
dequantized coefficient support permits it. The byte result preserves the
existing modular arithmetic, rounding, clipping and scalar fallbacks.

Windows and Linux each pass 6,865 offline and live checks, including 948 live
FFmpeg checks, and 472 internal plus 356 public focused JPEG checks in scalar
mode. Both operating systems validate the same actual Release package and run all
three usage examples. The packaged DLL matches the timed DLL. The remaining JPEG
workloads keep their original fixtures and reference boundaries for the final
refresh, including fresh native contexts and fresh stream delivery.

The isolated witness timings and earlier implementation trials are retained in
the verified `tmp/final-refresh/jpeg-three-row` and
`tmp/final-refresh/jpeg-cmyk-combined` archives. The earlier owned JPEG results
retain their original identities. [Previously measured owned boundaries](JPEG-DECODE-CURRENT-PACKAGE.md)
