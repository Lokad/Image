# JPEG quarter interpolation

Accepted 2026-11-10. Complete resident 513×259 JPEG 4:1:1 decode falls from
515.15 to **390.09µs**, saving **125.06µs / 24.28% raw / 22.51% reference-normalized**.
Filtered and all-actual raw/normalized confidence intervals separate. FFmpeg
changes −2.277%; current447.08µs reference gives ratio.873 / conservative upper.885:
this control passes and stops. The shared4:1:0 control remains passing at
372.66/FFmpeg519.59µs, upper.752. Its historical point difference is not a causal gain.

## Shared preparation and execution

The4:1:1 schedule previously represented every horizontal endpoint twice, used
the4:1:0 averaging fetch, then blended identical vertical rows at weight zero.
A type-selected direct fetch now shares the original Q14 horizontal filter with
the expanded4:1:0 fetch. One color loop selects an exact zero-weight producer;
the general vertical interpolation and scalar fallback remain. No duplicate
color loop, public API, cache or session is introduced.

For byte samples `(a+a)>>1 == a`. For valid Q7 samples0..32640,
`(sample*4096)>>10 == sample<<2`, including the original centering. Intermediates
fit Int32. All Q14 rounding, color coefficients/biases, clipping and byte packing
remain unchanged. Argument checks still validate both planes at weight zero.
The new test covers the full valid Q7 domain, independent bottom planes, an odd
tail, unaligned guards and unchanged inputs. Existing weights/extremes, quarter
progressive decoding, zero-allocation, refusal, cancellation and owner tests remain.
Actual-package JIT confirms direct loads after type selection; instrumented
elapsed times are discarded.

## Boundaries, storage and scope

Both sides time component decoding, normalization, fresh independent RGBA output
and disposal; FFmpeg retains its documented codec/conversion contexts. Inputs,
actual coded dimensions, native scalar/SIMD checks and analytical managed pixels
are unchanged and exact. The4:1:1 packet remains3,904B, SHA256 `eeb88c75…a56e77`;
the decoded frame remains531,468B, SHA256 `c10fa7db…ef99841`. These original
DC-coded sampling controls establish this scope, not general photographic speed.
No encoder packet or metadata policy changes.

Total amortized GC is219,549→219,551B/op for4:1:1 and189,088B/op for4:1:0;
this is not an allocation gain. Fixed render workspace remains1,536/1,792B with
the same reservation before allocation, root forwarding, windows and cleanup.
The independently owned531,468B result remains charged until disposal; retained
operation scratch is zero. No new owned capacity. Full admitted/physical/native
peaks are not remeasured; default idle output-pool retention remains separate and
governed calls bypass it.

## Verification and reproduction

One immutable main/embedded build per OS serves focused checks, the short
screen, reversed independent twelve-actual confirmation, shared4:1:0 refresh and
full qualification. No new fixture/settings, harness/native/selector rebuild,
favorable extra repeat or expectation projection. All original/adverse samples
remain. The two changed costs supersede their earlier results;27 unchanged
JPEG costs reuse valid evidence. The29-control status is **20pass / 6gaps / 3uncertain**.

Windows/Linux each pass6,133 full/live checks and453internal/375public JPEG
checks per normal/scalar/disabled-AVX2/disabled-AVX512 mode. The same actual
440,589B private Release0.2.1 package and allthree examples pass on both OS.
Unchanged resizing297/729 per-mode qualification is reused. Published packages
remain immutable; publication and the full127-row release refresh are separate.

[Machine evidence](jpeg-quarter-interpolation-evidence.json) records full module,
source, package, frame, confidence and qualification identities. Retained raw
`tmp/jpeg-quarter-interpolation-qualified.tar.xz` is6,324,172B /157 members,
SHA256 `d0a9f382a265469536ffbf9f6763253e323e1a4442b3f56a851f3a18c2092208`.
The mirror and every member are verified. It includes snapshots, build/test logs,
measurement/guard receipts, samples, scripts and runtime-provider mapping; prior
archives retain unchanged harness/reference inputs. Run the retained
`jqi726-measure.py` with the recorded subject/case/mode, then its observation and
comparison scripts. Qualification scripts reuse that exact build and package.

Next: attribute the existing sparse-CMYK/direct-RGB gaps before another redesign.
Passing quarter controls stop; all283 required paths remain the objective.
