# Shared conversions for bounded deep bicubic pairs

The existing complete RGBA8 513×259→32×16 Resize saves **97.90 µs /
23.73% raw / 23.80% reference-normalized**.
Filtered/all-actual raw and normalized confidence separate. Native time moves
+0.09%. This control passes the warm target and stops.

| Complete public Resize / same 4 KiB admission | Lokad / zimg µs | Ratio / conservative upper | GC B/op |
| --- | ---: | ---: | ---: |
| Immutable source-driven predecessor | 412.57 / 376.47 | 1.096 / 1.100 | 113 |
| Shared prepared conversions | 314.66 / 376.79 | 0.835 / 0.846 | 113 |

One short screen precedes independent 12-actual confirmation; the source-valid
[predecessor](SOURCE-DRIVEN-DEEP-BICUBIC.md) confirmation is reused. All actuals,
native brackets, source/settings identities, exact rational/managed pixels,
native bounds, ownership and CPU0/idle-v2 guards are retained. Geometry, source
preparation, filtering, certification/repair and independent output/disposal
remain timed. The labeled zimg 3.0.6 reference retains graph/scratch and includes
the existing clamped halo, normalization/packing and independent output. This is
one depth/geometry/budget control, not a whole-family or one-shot-reference claim.

Equal even contiguous windows with a positive even overlapping shift now share
loads and exact UInt16-to-double conversion. Three source bands update two
independent even/odd accumulation streams in their original order, followed by
the original fold. Geometry eligibility is checked once per output pair;
clamped/odd/single/unequal windows retain the indexed producer. No fixture
identity or fixed tap count selects the path. The unchanged row schedule,
vertical arithmetic, normalizers, 72-term certificate and original repair retain
exact pixels. Removing the pair dispatch/helpers reconstructs the original file.

The same 3,536 B explicit row payload and 4 KiB admission remain; the original
4,032 B fallback frame is separate. No new heap buffer, reservation, owner, pool,
retained state or output alias. GC allocation remains 113 B/op; the 2,048 B
independent result survives source disposal, and retained operation scratch is
zero. Physical/native/JIT peaks and default-pool retention are unmeasured.
Actual row code grows 5,910→6,825 B and its prologue reservation 728→808 B;
those costs are included in elapsed time, not hidden as storage savings.
Both pair helpers inline, eligibility is 117 B, no FMA or column fallback appears
in the primary. The source model's 149,184 avoided logical pixel conversions
is an explanation, not a phase-cost forecast.

Windows/Linux each pass **6,435 full/live checks**: 3,106 internal / 2,381 public /
948 live. Affected 524 internal / 791 public checks pass normal/scalar/AVX2-off/
AVX512-off, including all three named storage suites. Five new internal cases
cover exact ordered double bits over four even tap counts and eight patterns,
poison guards, no allocation and eligibility refusal. Existing actual row,
odd generalization, height-17 fallback, tiny-alpha cleanup and cancellation
coverage is reused. One core/embedded build per OS; no public-test, benchmark,
native, selector or fixture rebuild. An archive-generator quoting preflight
failed before any mutation; no production compile failure or repeat milestone.

The **same actual Release package** and all three examples pass both OSes,
including frozen 32×16 pixels/shared admission/surviving output ownership.
Private 0.2.1 package SHA256 e302c04aa439bec0a8de734504884bc5c69873c915977ced5d9436251ee40099 (454,148 B),
timed/packaged DLL f5be649c6ff5a280f17116affe03e220746e7314054716192b569be22959bdfc; independently qualified Linux
DLL cd7d44c22496bb2c539a156c0a988486d1be02f909efc4bf5d95868120a5d470. Published releases remain immutable.

[Evidence](prepared-deep-pair-conversions-evidence.json) and mirrored/member-verified
raw prepared-deep-pair-conversions-qualified.tar.xz — SHA256 bf854cc7a05705884732cc237320cb28714da21a5cdb76f301efa8bccf3d850b
(2,533,136 B / 96 members) retain sources, modules, actual
package/symbols, checks, receipts, statistics, guards, JIT and reproduction.
Dependencies preserve baseline/reference provenance. All 289 material paths
remain in scope; publication is postponed. Passing deep-byte work stops while
independent demonstrated gaps or uncertain evidence receive the next action.
