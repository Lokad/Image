# Predictive PNG current cost

One new required algorithmic control closes a measurement omission: default
Optimal encoding of the original 3000² RGBA16 gradient after horizontal flip.
R=16x, G=16y, B=8(x+y), A=65535 before the flip. The input SHA256 is
`14c4457890991939f2798e6897275eb6c0d5dc53d88e9ede82b3657aaf2bfc95`.
The pinned FFmpeg n8.1 reference uses level6/pred=mixed and one CPU/thread.
This adds one distinct prediction algorithm to the finite inventory, not a grid.

| Quiet CPU0 observation | Lokad µs | Reference µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Short screen | 1,191,151.87 | 419,962.39 | 2.836 / 23.520 | 3,402,944 |
| Independent confirmation | 1,190,543.10 | 436,360.64 | 2.728 / 2.802 | 3,405,048 |

Both subjects produce the same complete 573,041-byte packet, SHA256
`4b1794a8886a95c14a899be3e15418bf198e22fc0e59a3af7d15c33df4c8e205`.
Normal/scalar CLI and warm adapters decode every sample exactly. The practical
compression gate remains exact pixels and <2 MiB; observed packet identity on
this control adds no arbitrary-content byte-identity promise.

The complete managed operation includes public EncodePng, wire packing, content
probe, row prediction, Deflate and fresh MemoryStream growth/disposal. The native
operation includes RGBA64LE-to-BE normalization and fresh independently owned
packet acquisition/encode/release; prepared context/frame/wrappers are reusable.
Preparation, IO, IPC, startup, snapshots, hashing and correctness decoding are
excluded. All actuals, both native brackets and idle-v2 guards pass. The wide
three-sample screen is resolved once by twelve independent actual iterations.

Production and its PG1 qualification/package are unchanged. Both-OS harness
compilation and native normal/scalar/source/ownership checks pass. Original None
worker/source/builder identities stay unchanged; a preflight verifies an existing
None control. A dedicated builder derives only mixed prediction and its ready
identifier from the original adapter. Initial timing tooling and the later
untimed identity/dispatch cleanup are retained separately; timed operation code
and the qualified producer are unchanged. Full/package checks are reused.

A six-second profile has 5,989 samples: ProcessLazy49.01%, Insert9.92%, row
scoring24.75% and Paeth15.08%. This supports one substantial SIMD row-scoring
trial; it is not a component elapsed-time measurement. Preserve filter ties,
residual bytes, portable execution, cancellation and memory ownership. Deflate
search remains a separate primitive. No production optimization is included here.

Reproduce preparation after provisioning the pinned Linux reference:

```sh
bash tools/WarmReference/build-png-predictable-linux.sh
python3 tools/WarmReference/prepare-png-predictable.py
dotnet run --project benchmarks/Lokad.Image.Benchmarks -c Release --no-build -- \
  --warm-short --warm-family Png --warm-cases EncodePredictable16 --warm-cpu 0
```

Set a unique `LOKAD_IMAGE_BENCHMARK_RUN_ID` and use the documented one-CPU/JIT/GC
environment and versioned guards. Preparation is not timing evidence.
[Machine-readable evidence](predictive-png-current-evidence.json) retains the
exact boundaries, producers, samples and profile. Mirrored/member-verified raw
`predictive-png-current-cost.tar.xz` (138 members), SHA256
`0cbed3a57b3f9aa5127f1f85ef8d263ae5f7214eb72cf0ff02c0ad0940fc099b`,
retains source snapshots, initial/final producers and workers, selectors, guards,
packets, profiler samples/maps and reproduction scripts. PG1 retains the reused
full/package proofs. The public preparation command also passed unchanged packet
and pixel checks after the raw timing archive was frozen.
