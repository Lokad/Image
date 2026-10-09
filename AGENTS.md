# Lokad.Image

Lokad.Image is an independently implemented, safe C#/.NET 10 image library.
The production package has no NuGet dependencies, unsafe code, native bindings,
subprocess codecs or vendored sources. External reference checkouts and tools
belong in the ignored `external/` directory and are never build inputs.

`SPEC.md` defines the target contract and current support. Keep it and README
accurate as features land. Required targets are still PNG/JPEG/WebP decode and
encode, 8/16-bit editing, orientation, crop, resize, flips and quarter turns.
GIF, animation and drawing/font rendering are deferred.

Use `./test.ps1` for offline Release tests, and `./test.ps1 -Differential` for live
FFmpeg comparisons. Missing FFmpeg must fail the differential suite explicitly.
Use `--tl:off` for dotnet output. Pack only with `-c Release` and verify the actual
package with `./tools/PackageSmokeTest/verify-package.ps1`.

Use original, deterministic fixture inputs with provenance. Do not copy upstream
sources, implementation tables, binaries, docs or test corpora into tracked files.
Format constants and algorithms should reference public specifications.

BenchmarkDotNet smoke runs verify execution and output correctness. Performance
measurement and tuning require the dedicated quiet VM supplied by the project
owner. Run benchmarks sequentially in Release, use FFmpeg baselines with
equivalent workload boundaries, and do not publish local timing numbers.

The local PLAN.md is ignored. Durable requirements belong in tracked documents
and tests. Keep all public files exclusively about Lokad.Image.
