# Progressive JPEG sample-plane scheduling

Full-resolution progressive RGB/YCbCr rendering now keeps one complete byte
sample plane and reconstructs the other two through bounded strips. Complete
ordinary/live444 allocations fall from about **984 KB to 701 KB/op**. Frozen
pixels remain exact. Accept the storage improvement; timing evidence is narrower
than a general CPU improvement or parity claim.

## Preparation, execution and ownership

Previously, all three complete byte planes existed before output allocation.
Now the first component is materialized and its coefficients returned before
creating the same fresh, tightly packed, independently owned RGBA result. The
other components use two eight-row strips in future output bytes, only while
their ranges are disjoint from every destination in the current strip. At a
physical tail collision, two 64 B stack blocks supply clipped tiles instead.
This geometry choice precedes admission and never recovers from refusal.

Keep existing inverse/color arithmetic, scalar/SIMD kernels, every padded block's
numeric validation, cancellation and orientation. Temporary samples cannot escape
or overwrite published pixels. The private result is disposed on failure; remaining
coefficient users finish before capacity is returned. Shared materialization helpers
also serve grayscale, subsampled and caller-sample decoding. No new pool, session,
cache, owner abstraction or numerical policy; caller samples retain their own
boundary without mandatory RGBA storage. Encoders are unchanged.

The frozen 513×259 reference uses equal **1×2 sampling factors**, padding to
520×272. Removing two 141,440 B arrays saves **282,880 B of capacity**. For each
component capacity L and result B≤4L, the original governed core peak is13L;
after first-plane retirement the proposed phase is9L+B≤13L. Its bound remains
1,838,720 B, excluding unchanged parser/table/input charges. This is an algebraic
bound, not a complete physical-peak measurement. Authored1×1 ownership controls
pad to520×264; their distinct bound is recorded. No fixture or sampling changes.

Two strips use8,320 B of output tail for31visible strips, with two visible
block-tail strips and one wholly padded block row reconstructed separately.
Retained result storage is531,468 B; retained operation scratch is zero. The
existing ungoverned exact-size coefficient cache retains1,697,280 B for this
shape, unchanged. Governed calls bypass that cache. Total allocation, live capacity
and retained storage are distinct; minor measured GC/batch differences are not
attributed to sample-array capacity.

## Complete-operation evidence

Quiet pinned CPU0, unchanged actual standalone/default-context worker, native
before/after brackets and original132,961 B input. Normalization, preparation,
admission, full decoding, independent output and disposal remain timed. Independent
original/candidate confirmations each retain12actuals, every excluded point and
original exact-pixel/ownership/source/quiet gates. No expanded workload matrix.

| Complete 513×259 decode | Original µs | Current µs | Original / current B/op | Interpretation |
| --- | ---: | ---: | ---: | --- |
| Ordinary progressive444 | 3,673.81 | 3,657.90 | 984,238 / 701,233 | Timing inconclusive |
| Live-token progressive444 | 4,271.21 | 4,202.35 | 984,212 / 701,182 | Raw gain; normalized inconclusive |

Live raw elapsed time improves68.87µs/1.61%, with separated filtered/all-actual
confidence. Reference-normalized intervals overlap; ordinary confidence also
overlaps. Claim neither normalized CPU gain nor equivalence. Native points move
−0.16%/+0.19% between matched live/ordinary confirmations. Earlier short clocks
differ and remain retained, without another favorable unchanged sample. Allocation
reduces283,030/283,005 B/op at these complete boundaries, about28.8%.

All five affected decoder costs refresh. Progressive420 passes/stops; grayscale,
ordinary444 and live444 remain gaps;422 is uncertain. Coding-validation evidence
is reused because it returns before rendering and calls none of these helpers.
Together the existing six controls remain1pass/3gaps/2uncertain. Other codecs,
encoders, editing and text reuse relevant unchanged paths. These125displayed rows
do not establish the282-path objective or a full actual-package release refresh.

## Qualification and retention

Windows/Linux each pass **5,732 full/live checks** (2485internal/2299public/948live),
373internal/341public per normal/scalar/AVX2-disabled/AVX512-disabled mode, and the
SAME actual private Release package/all three examples. Eight new public checks
cover tiny output, strip collisions, partial blocks, RGB/YCbCr, aggregate admission,
refusal/cancellation cleanup, async input and surviving results. Existing frozen
native pixels and differential coverage remain. The package consumer exercises the
new progressive three-component path, not just the unchanged grayscale renderer.

One immutable core/embedded version perOS. The initial compile needed a scoped
span annotation and produced no executable version. A new async test initially
omitted separately charged stream capacity; correcting its test phases required
only a public-test rebuild. Preserve failures and reuse passed internal checks.
No new harness/native/selector/fixture builds or timing profile. Published releases
remain immutable; publication is postponed.

[Evidence](jpeg-progressive-plane-scheduling-evidence.json) records identities,
all points, geometry/lifetimes, failed preparations, generalization and package proof.
Raw `jpeg-progressive-plane-scheduling-qualified.tar.xz`:2,909,476 B/253members,
`c293e06a01c2871466e642a2f7a7b865809942633ca589640068468729424be1`;
both mirrors and all members verified. [Signed preparation](JPEG-SIGNED-PREPARED-INSTRUCTIONS.md)
retains prior gains and unchanged coding-validation proof; the
[inconclusive correction variants](JPEG-BUFFERED-CORRECTION-PREPARATION.md) retain
adverse points outside renderer dependencies. Continue substantial independent
codec/resize work without retuning this unchanged timing result.
