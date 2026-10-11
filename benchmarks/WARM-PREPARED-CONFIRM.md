# Prepared warm confirmation: RGBA16 nearest resize

The accepted exact row recurrence receives independent quiet confirmation on
the existing 513x259 RGBA16 to 256x129 nearest pair: 0.61962x FFmpeg,
with conservative upper 0.62936x. Both unchanged quiet guards pass.
This certifies only this pair. Uniform warm parity and other required cases
remain open. Production code and output are unchanged.

| Native batch calibration target | Managed mean (microseconds) | Worst bracket ratio | Conservative upper | Quiet guards |
| --- | ---: | ---: | ---: | --- |
| 200 ms | 106.09 | 0.61557x | 0.63400x | Post-run load failed |
| 50 ms | 105.69 | 0.61962x | 0.62936x | Pass |

Both prepared runs execute the retained BenchmarkDotNet 0.15.6 generated
WarmConfirm child directly, after compilation and fixture preparation have
finished. The generated entry, benchmark assembly, production DLL and runtime
configuration are hashed before and after each run and frozen with raw evidence.
Only compatible benchmark assemblies may replace a cached assembly; smoke
validation and inspection of the actual entry are required before replay.
No benchmark host build occurs inside the guarded interval.

The first replay keeps the historical 200 ms native calibration target and
fails its post-run load guard. It remains uncertified. The next protocol declares
a 50 ms native target, still measuring complete operations with twelve native
samples before and twelve after. Calibration selects 512 operations per batch,
about 87 ms here; the target is a minimum, not an imposed elapsed time.
Managed measurement remains twelve actual iterations at a 200 ms target and
three warmups. A failed preparatory I/O-wait check launches no benchmark and is
retained. A fresh subsequent preguard and its immediate postguard both pass.
No threshold, pixel bound, workload boundary or failed-run status changes.

Both subjects use CPU0, one filter thread, thread type zero and normal SIMD.
Managed processor count is one, tiering is off, and workstation GC is
non-concurrent. Complete public Resize includes admission, output allocation,
materialization and disposal. The native worker runs the same public FFmpeg
filter graph and owned-output boundary as the retained
[nearest recurrence round](WARM-NEAREST-RECURRENCE.md). Source, alpha, hidden RGB,
every full UInt16 sample, independent simultaneous owners and the pinned CLI
remain exact. Startup, IPC, I/O, generation and validation stay outside samples.
The saved timed production DLL matches every method body and implementation flag
in the accepted package; its 2,137-test results per OS remain applicable.

[ParseBenchmarkLog](../tools/WarmReference/ParseBenchmarkLog/Program.cs) uses the
pinned BenchmarkDotNet Measurement and Statistics APIs to parse elapsed/count
values and already adjusted, filtered WorkloadResult rows. It preserves all
twelve raw actual rows, result rows, GC and threading lines. Its count, mean,
standard error/deviation and confidence bounds reproduce the retained official
full JSON report. Rounded printed per-operation times are never statistical
inputs. Native confidence bounds use the managed critical value, and the
conservative ratio divides managed confidence upper by the smaller bracket
confidence lower.

The [numeric evidence](warm-prepared-confirm-evidence.json) preserves both runs,
the pre-run abort, guards, complete native samples, actual binary identities
and immutable archive digests. Regenerate this report with
`python benchmarks/render-warm-prepared-confirm.py`; add `--check` to verify.
See the [replay recipe](../tools/WarmReference/README.md#prepared-confirmation).
No further optimization of this already passing pair is planned.
