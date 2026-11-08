# Shared raster sorting

Crossings, intersection events and edge activation now share an original bounded
sort with static value comparers. This removes comparator closures, wrappers and
per-band delegate allocations. Smaller-side recursion and a heap fallback bound
the execution stack and worst-case work. Every comparison checks cancellation
directly. Existing arrays, admission, edge identities, tie orders, geometry,
coverage and full-precision blending remain unchanged.

| Complete prepared drawing | Previous µs | Current µs | Reference µs | Ratio / upper | Managed B/draw |
| --- | ---: | ---: | ---: | ---: | ---: |
| RGBA8 | 189.86 | 153.86 | 45.58 | 3.376 / 4.360 | 67,368 |
| RGBA16 | 189.35 | 147.75 | 45.90 | 3.219 / 4.522 | 67,368 |

Allocation falls **25,016 B/draw (27.08%)** from 92,384, verified by three
16-operation samples per depth. Admitted Latin workspace stays **48,544 B**;
all eight existing frame peaks are unchanged and scratch returns to zero.
Physical and reference peaks are unmeasured. No cache, pool, retained state or
public API is added.

Independent twelve-actual confirmations follow three-actual screens. Point costs
fall 36.00/41.61 µs, but **a causal CPU gain remains inconclusive**: normalized
intervals overlap at both depths, and RGBA16 all-sample raw intervals overlap.
RGBA8 raw intervals separate. Native minimum-bracket means move +0.33%/−0.17%.
The short byte reference interval crosses zero and supplies no finite ratio upper.
All samples, including the higher short costs and word outlier, are retained;
no completed timing was repeated to improve confidence. Retention is based on
verified allocation savings and removed execution layers, without a CPU-gain or
parity claim. Both current reference gaps remain demonstrated.

The existing DejaVu Sans 18px “AV To” controls use a 384×160 canvas and origin
8/8. Complete public DrawText includes geometry/events, scan conversion,
composition and disposal. Font/layout are resident; reset is excluded equally.
The explicitly labeled reference remains ImageSharp.Drawing 2.1.7 / Fonts 2.1.3
on ImageSharp 3.1.12. Broader text integration includes every public layout call
and fitting trial; this drawing comparison is not bare shaping. Exact managed
frames and approved normalized visual gates remain, retaining full 16-bit samples.

Before editing, one current two-second capture recorded 1,995 CPU samples across
12,280 exact draws. Comparer callbacks contributed 356 samples, versus 25 in
UInt128 division. Samples identify code locations, not exact phase costs or
predicted gains. Instrumented elapsed time is discarded. This replaces the
superseded pre-active-edge attribution as the lead for this experiment.

Windows/Linux each pass **5,898 full/live checks**, plus 158 internal and 185
public affected checks per normal/scalar mode. These include mixed/duplicate/
reversed sorting, direct cancellation/recovery, eight frozen frames, coincident
contours, admission refusal and cleanup. The SAME actual Release package and all
three examples pass both OSes, including exact clipped mixed-alpha text at both
depths. Main/embedded assemblies build once per OS; all prepared measurement,
reference, resource and fixture tools are reused. Published packages are unchanged.

[Evidence](text-raster-shared-sort-evidence.json) records all samples, identities,
scope, allocation controls, confidence limits and qualification. Earlier
[active-edge scheduling](TEXT-ACTIVE-EDGE-SCHEDULING.md) retains its historical
gains. Raw `text-raster-shared-sort-qualified.tar.xz`: 6,133,272 B / 148 members,
SHA256 `3466972acb78aac80d60f235fed85aa4164284631e76e804f7cc606cb609148d`,
mirrored and member-verified. Retained tda572/tsc573 scripts and cited dependencies
reproduce the round. The full 283-path objective remains open; publication stays
separate and postponed.
