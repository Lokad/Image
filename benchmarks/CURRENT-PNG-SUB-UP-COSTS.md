# Current PNG Sub and Up decode costs

All four existing controls independently pass the warm one-CPU parity target.
Stop them unchanged. These are current costs, without a causal comparison to
older producers.

| Complete 513×259 decode | Lokad µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: |
| Sub8 | 521.097 | 0.722 / 0.728 | 4,472 |
| Sub16 | 318.278 | 0.404 / 0.413 | 8,568 |
| Up8 | 1,216.958 | 0.759 / 0.767 | 4,472 |
| Up16 | 1,640.467 | 0.814 / 0.818 | 8,568 |

Each has12actuals, all native brackets, filtered/unfiltered bounds and both
idle-v2 guards retained. Both subjects include complete decoding, normalization,
independent output ownership and release. Frozen original packets/raster/settings
and exact RGBA/alpha/hiddenRGB, source and output lifetimes pass. Managed
allocations are B/op; native allocations are unmeasured.

Zero builds: reuse the qualified PAV1core, generic typed-int selector, harness,
fixtures and native worker. Default16call unrolling, one complete operation per
invocation,200ms andthree warmups remain. The earlier scan-CRC change warranted
these refreshes; the new Average guard is not selected. Older native identities
are retained while the already-approved current worker preserves original
operations/settings and staticFFmpegarchives/buildflags. No new cases/images or
retuning. Unchanged full5088tests/OS and SAMEactualpackage/three examples remain
valid.41affected costs need refresh; the full268path objective remains active.

[Evidence](current-png-sub-up-costs-evidence.json). Mirrored/member-verified
archive current-png-sub-up-costs.tar.xz, 56members,
SHA256 ca110f878af147f8bc07e4efa66f3c223b640002cca3900f83a0e4950debaa69. Qualified-build dependency pinned by hash.
