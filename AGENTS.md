# Lokad.Image

Lokad.Image is an independently implemented, safe C#/.NET 10 image library.
The production package has no NuGet dependencies, unsafe code, native bindings,
subprocess codecs or vendored sources. External reference checkouts and tools
belong in the ignored `external/` directory and are never build inputs.

`SPEC.md` defines the target contract and current support. Keep it and README
accurate as features land. Required targets are still PNG/JPEG/WebP decode and
encode, 8/16-bit editing, orientation, crop, resize, flips and quarter turns.
Required extensions include supplied-TrueType text rendering/measurement, drawing
and compositing, CMYK/YCCK and 4:1:1/4:1:0 JPEG, caller-owned JPEG sample buffers,
async/bounded stream inspection, shared admission and structured failures, and
improved WebP compression efficiency. Keep ICC conversion and EXIF reorientation
opt-in. GIF and animation are deferred.

Required qualification also covers caller-owned cooperative memory admission,
supplied-font layout/shaping compatibility (including Arabic) and Lanczos3.
Forward the borrowed ImageLimits.MemoryBudget root through nested work; reserve
owned capacity before allocation and keep retained results charged until disposal.
Admission refusal is terminal, without retry, waiting or a lower-memory fallback.
Preserve input, output, dimension, container and memory bounds and await actual
asynchronous users before releasing their storage. Fuel accounting has been
removed: do not retain MaxWork, cumulative work quotas, work reservations or
partial-charge semantics. Use direct cooperative cancellation checkpoints.
IMemoryBudget is the sole shared admission model; do not recreate ImageBudget,
BudgetedImage, whole-operation ceiling reservations, weak ownership tracking or
finalizer-backed leases. Breaking cleanup of unpublished APIs is authorized;
remove obsolete APIs rather than forwarding them. Published 0.1.0 is immutable;
development targets 0.2.0.
Lokad.Image has no production usage. Unpublished APIs and implementations may be
replaced without compatibility layers; preserve the declared contracts and tests.

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
Single-component grayscale JPEG encoding uses the explicitly labeled SIMD-enabled
libjpeg-turbo reference as an approved benchmark-tooling exception; production
remains safe managed code with no dependencies or native bindings.
Use the versioned idle/process guard in `tools/WarmReference/capture-vm-environment.py`.
VM use is exclusive; do not wait for historical load average to decay after our own work.
Maintain `docs/benchmark.md` after every accepted performance round. Keep at most
three tables, include allocations and verification status, and check the generated
summary with `python benchmarks/render-benchmark.py --check`.

Keep artifacts manageable without an arbitrary checkout-size cap. Routine builds
and package verification must not be blocked by negligible workspace growth;
check actual disk space when substantial growth is expected.
Preserve release packages and cited raw evidence; prune redundant extractions,
stale generated consumers and unreferenced experiment outputs before accumulating
more artifacts. Reinstallable tool caches may be removed; keep reference sources.

The local PLAN.md is ignored. Durable requirements belong in tracked documents
and tests. Keep all public files exclusively about Lokad.Image.

Preserve encoder byte exactness, decoder correctness and meaningful editing
coverage. The fuel, ownership and bicubic cleanup is complete; keep useful
measured kernels and portable fallbacks. Qualify accepted production changes
on Windows/Linux and through the actual Release-package consumer. Historical
benchmark evidence is retained, not current proof.
