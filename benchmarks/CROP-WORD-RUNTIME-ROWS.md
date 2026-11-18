# RGBA16 crop confirmation and runtime-row trial

The current development package `6fed564e` passes the original RGBA16 crop
at **4.156 µs / FFmpeg 4.298 µs**, **56 B/op**, with interval
**0.953906–0.977282**. The word copy loop is the existing one. This capture
qualifies word crop after the RGBA8 dispatch change. It establishes current
costs, with the earlier upper bound of 1.005834 retained as historical evidence.
The same 1,024-pixel cancellation chunks, independent output, exact samples,
source and native identities pass. Both idle-v2 guards and all twelve
4,096-call units and reference brackets are retained. The completed
Windows/Linux and actual-package qualification for this DLL is reused.
[Current-package evidence](crop-current-package-evidence.json)

## Private alternative

The candidate keeps the checked runtime copy on RGBA16. If the row fits one
cancellation chunk, it borrows both spans once and copies each complete row
without the inner chunk loop. Admission, output lifetime, bounds, and one poll
before each row copy remain. Wide word crops and byte crop use their existing
paths.

The short screen moved from 4.178 to 4.007 µs with a wide interval. Twelve
independent pairs confirmed **4.117 → 3.962 µs**, a **3.77%** gain, interval
**0.956719–0.967982**, with 56 B/op on both builds. The identical private DLL
passed a separate standalone comparison at **3.917 / FFmpeg 4.258 µs**,
upper **0.924919**. One core build reused the actor, harness and native worker.

| Complete operation on staged Release DLL `4a43e5ea` | Lokad.Image µs | FFmpeg µs | Conservative interval | Managed GC B/op |
| --- | ---: | ---: | ---: | ---: |
| RGBA16 crop | 3.962 | 4.140 | 0.933159–0.978812 | 56 |
| PNG16 decode/orient/crop/fit, prepared reference | 2,846.37 | 2,941.01 | 0.824810–1.034029 | 8,888.16 |
| PNG16 decode/orient/crop/fit, fresh reference | 3,082.13 | 3,578.15 | 0.849206–0.868562 | 8,888 |
| Prepared PNG16, interleaved confirmation | 2,961.65 | 3,114.80 | 0.891768–1.010838 | 8,888.16 |

The first prepared capture contains two slower managed units at 3,049 and
3,218 µs. The other ten range from 2,766 to 2,814 µs. The final native
bracket also shifts. Confirming with before/paired/after samples every 16
operations preserves all twelve 256-operation statistical units. The paired
interval narrows to 0.941509–0.954309. The full gate still crosses parity.
The cause of the variation remains unverified. All inputs, exact pixels,
ownership, stream and quietness checks pass.

Retain the candidate privately. The actual package now passes the direct
crop and keeps its independently qualified PNG16 pipelines. The Release trial
was built once per OS, with one additional partitioned driver for the
measurement. It stopped before broad tests and packaging. The actual-package
confirmation reused the qualified DLL and driver without builds. Both original
crop cases now stop.
[Trial evidence](crop-word-runtime-row-trials-evidence.json)
