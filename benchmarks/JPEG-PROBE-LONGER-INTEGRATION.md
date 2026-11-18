# JPEG probe longer integration

**Uncertain; no production change.** One predeclared fourfold integration of the
same complete asynchronous JPEG probe does not resolve its confidence interval.
The larger, adverse interval remains visible. Stop captures for this unchanged
case; do not infer that optimization is required.

| Same immutable package / FFmpeg AVIO adapter | Managed / reference µs | Conservative ratio interval | GC B/op |
| --- | --- | --- | --- |
| Prior integration | .216719 / .215312 | .983597–1.028464 | 568 |
| Fourfold integration, one capture | .219043 / .209541 | .972123–1.120761 | 568 |

Offline review found original native subunits lasting3.42–3.79ms, with unit
wall/CPU correlation.999742 and standard deviations2.428/2.409ns. Native
confidence spread exceeded the1.407ns point difference. This justified one
integration-sensitivity check; it did not establish an external scheduling cause
or predict a passing result.

The longer capture has wider variation. Its last two units slow on both subjects;
all twelve units remain. Native wall/CPU correlation is.999992, with standard
deviations5.850/5.827ns. Their cause is unproved. The short-integration explanation
is unconfirmed; there is no sample removal, favorable retry or further count sweep.

## Matched-unit diagnostic

In the longer capture, managed time correlates 0.979 and 0.972 with the
before and after reference groups. The per-unit managed mean is higher
than both reference means in all twelve units. Using the twelve log ratios
and the existing critical multiplier gives diagnostic intervals of
**1.037535–1.053301** against the before group and **1.033122–1.050810**
against the after group. These intervals show the direction within this
capture and leave the original conservative result uncertain. In the shorter
capture, the corresponding intervals are **0.988740–1.024735** and
**0.987232–1.023556**. The roughly 10 ns difference in the longer capture is a
target for attribution. Its relationship to cadence and GC remains open.

## Remaining input bound reused during refill

An isolated reader reuses the remaining encoded-input bound across an
immediately completed stream read. A pending read recomputes it after the
actual user returns. Read sizes, sentinel handling, failure precedence,
admission and cancellation stay the same. The three-pair screen measures
**0.209377 → 0.207245 µs**, interval **0.872472–1.122934**, at **568 B/op**.
The original descriptor, independent owner and idle guards pass, and Windows
passes 146 focused public tests in each normal and scalar mode.

The point savings are about 2 ns and the interval includes unchanged
performance. The trial ends at the screen. The complete FFmpeg comparison
remains uncertain. [Screen and diagnostic evidence](probe-refill-input-bound-trial-evidence.json)

## Cadence and collection diagnostic

One instrumented run alternates four units at each original cadence on the
qualified package. The generated workload loop, exact descriptor and reference
checks, 16 subdivisions and original warmups are retained. Collection counters
are read outside managed spans. CPU 1 observes CPU 0's APERF and MPERF at 125 ms
intervals. All exactness, ownership and idle guards pass.

Short and long units average **234.5/235.7 ns** managed and **221.3/220.4 ns**
in the faster native groups. Gen0 counts scale from 29 to 116 per unit, with
zero Gen1 or Gen2. Observed managed pauses average **2.56/2.17 ns/op**.
APERF/MPERF ranges from **1.617 to 1.640** across the matched windows, with
one clock sample per short unit and eight or nine per long unit. The managed
difference appears at both cadences despite modest clock movement and similar
pause time per call. This bounds the observed effects in this run. The eight
instrumented units are diagnostic and supply no parity result.
[Raw capture and analysis](probe-refill-input-bound-trial-evidence.json)

## Completion of an already assembled direct field

An isolated reader returns immediately when a validated fill completes the
direct field already present in `Header`. The field model checks 5,984
offset/read combinations through the 32-byte limit. Partial fills and pending
reads retain the existing path. Windows passes 146 focused normal and scalar
tests per mode. Both quiet-VM captures retain exact descriptors, independent
owners, all actuals and 568 B/op.

The three-pair screen averages **0.213377 → 0.206657 µs**. Independent pairs
give **0.213812 → 0.209762 µs**, interval **0.947178–1.016801**. About 4 ns of
point savings and an interval spanning unchanged performance end this trial.
The scored FFmpeg result remains uncertain.
[Screen and confirmation evidence](probe-refill-input-bound-trial-evidence.json)

## Dimension-only classification and pending operation initialization

One private variant skips unused color classification during dimension-only
JPEG probes. It retains the same reads, validation and published facts,
including grayscale source metadata. Windows passes 152 focused tests per mode
in normal and scalar runs. Its three-pair quiet-VM screen gives **0.240809 →
0.239083 µs**, interval **0.885845–1.112768**.

A separate private variant has each caller initialize the pending operation
before advancing. The resumed caller still waits for the actual read to
finish before clearing its state. The Linux JIT removes eight stores at
`AdvanceJpeg` entry. Windows passes 146 focused tests per mode. The three-pair
screen gives **0.244417 → 0.240359 µs**, interval **0.676552–1.429353**.

Both screens pass the exact descriptor, independent ownership and idle
checks at **568 B/op**. The point savings are about 1.7 and 4.1 ns and both
intervals include unchanged performance. The variants stop at screening.
The scored asynchronous FFmpeg comparison remains uncertain.
[Screens, generated code and verification evidence](probe-refill-input-bound-trial-evidence.json)

## Acquiring the next marker with an opaque payload

The private candidate combines the end of a skipped JPEG segment with
the following marker request. It applies to asynchronous, forward-only
dimension probes without a live cancellation token. The JPEG grammar
still validates the marker, and both caps still limit the read. The
frozen input drops from **14 to 11 read callbacks**, consuming the same
**308-byte prefix** and stopping before pixel data.

The short screen gives **0.219484 → 0.201194 µs**. Independent twelve-pair
confirmation gives **0.232484 → 0.211988 µs**, a **20.5 ns** saving with
an interval of **0.864021–0.961622**. Against the frozen FFmpeg AVIO
adapter, the standalone private build measures **0.202327 / 0.206028 µs**,
interval **0.962987–0.998826**. It keeps the original generated workload,
12 statistical units, 16 subdivisions and complete input and ownership
boundary. All runs allocate **568 B/op**.

The 1,006-case read audit preserves successful descriptors and prefix
lengths, cap failures, and the tested live-token and seekable traces.
One malformed input delivered in three-byte fragments raises the same
data error after consuming 88 bytes instead of 89. Both Windows modes
pass 191 focused tests. The private result supplies a passing comparison
for the candidate DLL. Production acceptance still requires a gated
pending-read ownership check, reconciliation of the affected inspection
paths, and confirmation on the actual Release package.
[Immutable builds, delivery traces, guards and results](probe-refill-input-bound-trial-evidence.json)

## Unchanged operation and gates

Each unit now includes3420160managed and1048576native complete operations,
four times the prior855040/262144 counts. Twelve statistical units per group and
sixteen subdivisions remain. The original three full warmups retain855040/262144
counts. The exact compiled BenchmarkDotNet constructor, consumer, unrolled and
remainder loops remain;16- and7-operation allocation checks pass. One small
orchestration build changes counters and reporting only. Original native calls,
all-actual/upper-outlier conservative fastest-bracket confidence and every
observation remain. Clock/loop overhead stays included, without subtraction.

Both sides include fresh resident-source delivery, bounded prefix acquisition,
independent owned descriptors and source/scratch/result disposal. No cached result,
whole-input buffering or excluded preparation is introduced. The reference stays
explicitly FFmpeg AVIO/owned descriptors with the original bounded header adapter,
not public packet-parser performance. Inputs, settings and source identities match.

Normal/scalar preflights reproduce every original descriptor/source/ownership
field. Full confirmation passes original gates, aggregation/count checks and
idle-v2 guards: sequential Release CPU0, optimized JIT and natural nonconcurrent
workstation GC. Allocation568B and admission512B remain; owned/admitted/retained
capacity is unchanged. Physical/native/JIT peaks remain unmeasured.

Production, harness, native reference, original compiled loop, fixtures and package
are never rebuilt. Reuse [JSP1255](JPEG-PENDING-PUBLICATION.md)'s6844full/live checks
per OS, SAMEactualpackage/all3examples. Only the current asynchronous probe cost
changes presentation. Passing sync/header/PNG/WebP and other families stop. Three
copy gaps, this uncertainty, the289-path objective and final165-comparison package
refresh remain open. Publication remains postponed.

## Evidence

[Compact evidence](jpeg-probe-longer-integration-evidence.json) pins the unchanged
timed Linux DLL `7b59eabed152f4c05331d957dad515b41aee8e278d026943de40e7202e270bd0`,
package `f4c5b5dbaeb6f7d8d1782c9f2d1100dccbcdfa7994750c1a041fed3662b3ccf0`,
driver `55b8975b86bf788682a1119e7cc85cf7971d3ed20aba0a1d5c774969f7af7bb3`
and original compiled-loop/harness/reference identities.

Mirrored/member-verified `tmp/jpeg-probe-longer-integration.tar.xz`:76496B,
31members; SHA-256
`513b3bbc7de35fcdb7fcd9db9369799bcb2f452e7baa4dab77f17d8d73e9926d`.
It retains exact old/new driver source and patch, binary/symbols, preflights,
every raw actual, guards and recipes `jli1258-prepare.py`, `jli1258-deploy.py`,
`jli1258-measure.py` and `jli1258-record.py`. The pinned JSP1255 archive retains
unchanged runtime/fixtures/reference/package prerequisites. Reproduce in a fresh
directory instead of overwriting immutable outputs.
