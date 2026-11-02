# Partial color-search preparation: inconclusive

A bounded trial prepared four blue-minus-green planes inside the existing color
search owner, reusing them across adaptive red-coefficient trials within a tile.
Misses captured the plane while constructing the original histogram. Order, ties,
integer costs and packets stayed exact; all capacity was admitted before allocation.
No cross-operation cache, pool, new owner or public API.

One instrumented encode of the original photograph counted 26,395 red and 68,497
blue histogram evaluations across 2,145 tiles. Four FIFO slots would reuse the
green contribution for 69.36% of blue-pixel visits. These are arithmetic counts,
not elapsed savings. The instrumented build produced the exact 160,032-byte packet.

The subsequent ordered three-batch direct screen gives:

| Complete operation | Original → candidate µs | Allocated B/op, original → candidate | Admitted peak B |
| --- | ---: | ---: | ---: |
| Photograph | 106,303 → 109,061 | 16,834,504 → 16,834,816 | 12,217,052, unchanged |
| Unchanged diagram control | 9,842 → 10,134 | 7,987,568, unchanged | 5,055,600, unchanged |

Photo elapsed is 2.59% higher; the unchanged diagram is 2.97% higher. This screen
does not establish a useful gain or a confidence-qualified regression. Added
capture/lookup work and storage do not justify confirmation. **The implementation
is restored, with no favorable resampling.** Alpha and authored bars retain their
exact packets and allocations too. No scratch survives the operation.

The same count audit observed 2,393,052 medium entropy-product calls but only
45,640 distinct counts. Repetition alone does not establish useful CPU headroom.
A full table would need 524,288 explicitly owned bytes; no free global cache is
introduced. This remains a separate lead, not approval to replay earlier trials.

Candidate Linux passes 234 affected checks normal/scalar; Windows passes 13
focused exact arithmetic, guarded-storage and ownership checks normal/scalar.
After exact source/core restoration, both platforms pass 234 affected checks in
each mode. The verified Release package is byte-identical; reuse full/package
qualification and current benchmark costs. No broader proof or package rebuild
was repeated. Failed implementations do not permanently close a design family.

[Evidence](webp-partial-color-trial-evidence.json) records the audit, all direct
results, producer pins and restoration. Raw
webp-partial-color-trial-inconclusive.tar.xz: 1,008,488 B / 120 members, SHA256
dba34a677cab18eae0397625030efb1c5ccf83f5d3bb422b17eda540bb1a93ae,
mirrored/member-verified. Instrumented/original/candidate sources, runtimes,
trial sequences, guards and all logs remain; unchanged full/package proof depends
on the retained shared-bicubic archive. Publication remains postponed.
