# Exact allocation-counter qualification

Lokad.Image's internal tests require exact zero managed allocation at selected
primitives. A standalone original diagnostic reproduced positive per-thread byte
deltas in empty, noninlined intervals under concurrent background collection.
Its escaped 40-byte array control remained exactly 64 B. No image code, test
framework, reflection or profiler runs inside the empty interval.

| Platform/runtime | Process GC mode | Nonzero empty intervals | Allocating control B |
|---|---|---:|---:|
| windows, .NET 10.0.13 | default | 16/256 | 64 |
| windows, .NET 10.0.13 | gc-off | 0/256 | 64 |
| linux, .NET 10.0.8 | default | 23/256 | 64 |
| linux, .NET 10.0.8 | gc-off | 0/256 | 64 |

This is consistent with the runtime's [allocation-context accounting issue](https://github.com/dotnet/runtime/pull/134855).
These are allocation diagnostics, without timing claims. They establish a counter
limitation; the old CI failure is retained rather than attributed to an invented
allocation stack. All quiet baseline empty intervals were zero, and all allocating
controls returned 64 B in both modes. The diagnostic requested nonblocking full
collections under bounded retained-memory pressure and verified background GC
occurred only in its default-mode runs.

Only the internal test host sets `ConcurrentGarbageCollection=false` using the
[documented runtime setting](https://learn.microsoft.com/dotnet/core/runtime-config/garbage-collector#background-gc).
A configuration guard and an escaped-allocation positive control enforce the
measurement conditions. Original loops, warmups, parallelism, tiering and zero-byte
assertions remain. Both platforms pass all 2,044 internal checks. Public API,
differential and actual package consumers retain their normal GC settings;
production DLLs and their qualified performance/package evidence are unchanged.
No retry, minimum selection, tolerance or production workaround is introduced.
