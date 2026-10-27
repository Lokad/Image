# Packed grayscale PNG transparency

Direct packed-row expansion removes a 4.813× warm decode gap. Independent
confirmation reaches **0.144× FFmpeg / upper 0.145×**, reducing managed time
97.0% with unchanged **648 B/op**. Pixels remain exact.

| Fixed 513×259 workload | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Gray2, tRNS key0, original short screen | 2,411.50 | 501.04 | 4.813 / 4.993 | 648 |
| Gray2, tRNS key0, independent twelve-actual confirmation | 71.49 | 497.90 | 0.144 / 0.145 | 648 |
| Opaque gray2, accepted-build regression screen | 72.29 | 106.40 | 0.679 / 0.931 | 616 |

The fixture reuses the original byte raster's existing gray2 projection:
`gray=(red>>6)*85`, equal RGB, alpha zero only for gray zero. Original authoring
packs samples MSB-first with zero unused padding, filter None, BCL Optimal
level6 and a two-byte big-endian tRNS key0. No new original image, geometry,
filter or effort grid is introduced. Only two workload types are timed.

One six-second profile records 5,985 samples: 51.3% in general pixel expansion,
29.3% in repeated stride access and 10.7% in repeated pixel-buffer access.
Dense gray1/2/4 tRNS rows now expand directly, with bounded eight-pixel safe AVX2
groups and scalar tails. Transparency compares each **unscaled** sample against
the full 16-bit key; out-of-range keys stay opaque and nonzero hidden RGB survives.
Sparse mapping, row ownership, admission, work and cancellation cadence are unchanged.
No workspace, dependency, unsafe code or native production binding is added.

Both subjects decode the identical resident packet into independent owned RGBA8
and release it. Managed framing, admission, inflation and expansion are timed.
Native codec/input packet/frame wrappers are reused; complete decode, owned
YA8-to-RGBA conversion, output acquisition and release are timed. Conversion and
codec use one thread. Startup, I/O, IPC, fixture creation and validation are excluded.
Native allocation totals are **unmeasured**; managed figures include harness
overhead and released-storage reuse.

Exact native normal/scalar samples, analytical RGBA, alpha, hidden RGB, geometry,
source preservation and two simultaneously live owners pass outside timing.
Sequential Release runs use one pinned CPU, normal SIMD, disabled tiering and
workstation nonconcurrent GC. Every quiet guard passes. The accepted screen and
independent confirmation share one immutable production/harness build; all twelve
actuals are retained and selected. The upper includes selected and all-actual
intervals against the faster native bracket's lower bound.

The unfiltered FFmpeg decoder fails the gray1/key1 and gray2/key1 alpha checks;
those comparisons are retained **unscored**, with no timing claim. Its gray1/key1
color-key diagnostic is exact, including white RGB under alpha zero. The existing
[SPEC reference policy](../docs/spec.md) already defines that exact correction; this
round's verified zero-key timing needs no correction. The native adapter admits
only the verified gray2/key0 packed transparency case, preserving other refusals.

Fifty-four new row controls cover all packed depths/keys, high invalid keys,
MSB order, tails, padding, unaligned guards, hidden RGB and pre-cancellation.
All **127 focused checks** pass with normal/scalar code on each OS. Both OSes
pass **3,428 Release tests**, including **898 live comparisons**. The same actual
Release package passes its consumer and all three examples on Windows and Linux.
The opaque regression mean passes before full qualification. Package, production
and measured-source identities are retained; unchanged inputs are reused.

[Numeric evidence](warm-png-packed-transparency-evidence.json) retains raw timing,
allocations, exact-output refusals, profile, guards, source/assembly/job identities
and full qualification. A post-run assertion initially confused original and
projected raster hashes; both were then verified separately, preserving the failed
postcheck without repeating timing. A generated-source path correction similarly
reused already-built jobs. Stop the confirmed pair; remaining material coverage stays open.
