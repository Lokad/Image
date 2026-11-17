# Exact edge-storage trial

**Unpromoted:** compacting each raster edge from 56 to 48 bytes reduces complete
ordinary RGBA8 drawing allocation **83,216 → 74,896 B/op**, but independent
confirmation measures **46.526047 → 47.023989 µs**. The paired ratio interval
**1.002676–1.018812** establishes a slowdown. Keep the qualified representation
and current costs; no standalone-reference refresh or broad/package repeat.

The prototype retains both original Y endpoints and the original rounded delta.
A three-state direction derives Min/Max, with the original Math.Min/Max fallback
for equal or unordered values. Interpolation, separate SIMD arithmetic, ordinal
ties, winding and full double precision remain. This differs from earlier
[capacity hints](TEXT-EDGE-CAPACITY-TRIALS.md) and
[float hulls](TEXT-COMPACT-RANGE-TRIAL.md): no estimate, extra traversal or precision
reduction. Getter work can offset fewer copied bytes; no causal breakdown is
claimed.

The three-pair screen measures 46.860838 → 47.248541 µs, interval
0.937835–1.084003. Its substantial allocation reduction justified one independent
twelve-pair confirmation. All actuals remain. Both rounds reuse the frozen
ordinary public workload, original stock reference correctness checks, default
loader processes and actor. CPU0, sequential execution, natural nonconcurrent GC,
4,096 warmups/operations and versioned quiet guards remain. Each operation's
canvas reset and full-frame validation stay outside its two clocks. These are
incremental DLL comparisons, not ImageSharp or FFmpeg timings.

Both OSes pass **283 internal and 166 public focused checks per normal/scalar
mode**, including event/raster arithmetic, frozen full-byte/full-word frames,
refusal, cancellation and recovery. The private representation probe preserves
full bound/interpolation bits over 256 finite/extreme/zero/infinite/NaN endpoint
pairs per mode. Actual managed size is verified. The authored rectangle's exact
admitted peak falls 6,744 → 6,680 B; one byte less refuses and releases storage.
This is a logical capacity, not a general physical peak. Total allocation,
replacement peaks and retained state are distinct; no new retained storage exists.

Initial test-build/private-field and old-capacity expectation failures remain
archived. Windows reuses its 281 initially passing normal tests and reruns only
the two revised capacity assertions; scalar and Linux run all 283. Unsupported
DLL-runner arguments and a renamed actor path fail before their pending work;
only preparation resumes. No completed timing samples repeat.

One isolated Windows core and two successful isolated embedded-test builds;
zero VM, canonical, public-test, existing actor/harness/reference/fixture/package
builds. Canonical sources/modules/package never change. Reuse **6,790 full/live
checks**, the same actual package and all three examples each OS. No neighboring
edge-layout variant is selected from this result.

[Evidence](text-edge-storage-trial-evidence.json) pins both implementations,
all actuals, gates and qualification reuse. Mirrored/member-verified raw
`text-edge-storage-trial.tar.xz`: **3,375,776 B /119 members**, SHA256
`94e5fd0276bd29fcaedd3f78c7bafb44dc4111a45876fb65e3dffe2fc5ad6001`.
Archived `egs1176-*` recipes and dependencies reproduce the round. This rejected
trial supplies no displayed current costs and is not a renderer dependency.
