# Completed JPEG marker acquisition — rejected

One candidate avoided the async method path when marker reads completed
immediately, preserving the grammar, pending I/O, counters and ownership.
The existing complete async JPEG dimension probe showed only **1.83% filtered /
1.79% all elapsed** reduction and **1.37% normalized point** improvement, below
the predeclared 5% screen gate. Allocation remained 744 B/op. The candidate is
reverted; no variants or independent confirmation follow.

| Quiet screen, three actuals | Lokad µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: |
| Original | 0.426148 | 2.094 / 3.270 | 744 |
| Candidate, rejected | 0.418348 | 2.066 / 2.970 | 744 |

All original packet, descriptor, source acquisition, native AVIO adapter,
independent output/release and idle-v2 gates pass. Native allocations are
unmeasured. Wide short-run bounds establish no CPU parity, gain confidence or
regression. The [existing independent probe evidence](WARM-PROBE-COMPLETED-READS.md)
remains applicable to the restored production path.

Production sources and DLL/PDB/XML are restored exactly on both OSes. Three useful
public controls remain for repeated fills and truncated markers across sync,
pending and mixed one-byte reads, including seekable/nonseekable streams, all-fact
probing, complete headers and admission cleanup. They pass on the actual restored
implementation: 101 focused checks each in normal/scalar modes on each OS.

Each OS built one candidate core and one public test assembly. Harnesses, workers,
selectors and fixtures were reused. Restoration needed no rebuild, full-suite
repeat or new package: the qualified CRC baseline, 5,193 checks per OS and same
actual Release package/governed consumer/three examples remain valid. This round
adds no production speedup or broader parity claim.

[Evidence](jpeg-probe-completed-markers-rejected-evidence.json) retains the complete
screen and mirrored, member-verified archive with exact candidate/restored sources
and binaries, guards, receipts and reproducibility dependencies.
