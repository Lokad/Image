# Owned text render preparation

[Complete public text pipelines](TEXT-COMPLETE-PUBLIC-PIPELINES.md) separately
measure preparation and full one-shot/repeated operations. These retained-state
draw timings remain scoped diagnostics; they do not establish end-to-end costs.

TRP784 adds `TextRenderPlan` for repeated drawing at a fixed canvas shape, depth
and origin. It owns copied glyph positions, prepared edges/events, crossings and
one coverage row. Font and layout may be disposed after creation. Ordinary
`DrawText` shares preparation and execution; coverage, blending, glyph order and
cancellation checkpoints preserve the exact managed contract. There is no raster
cache, global pool or production dependency.

| Complete draw | Lokad.Image µs | ImageSharp.Drawing µs | Ratio / upper | Allocated B/draw | Retained B |
| --- | ---: | ---: | ---: | ---: | ---: |
| Ordinary RGBA8 | 87.12 | 45.40 | 1.919 / 2.945 | 59,304 | 0 |
| Ordinary RGBA16 | 92.57 | 46.48 | 1.992 / 2.975 | 59,304 | 0 |
| Prepared raster RGBA8 | 53.81 | 37.80 | 1.424 / 1.622 | 56 | 40,408 |
| Prepared raster RGBA16 | 56.80 | 37.61 | 1.510 / 2.015 | 56 | 40,408 |

Prepared drawing saves33.30/35.77µs against current ordinary drawing, with
separated filtered and all-actual intervals. These are repeated-use savings from
explicit preparation, not a kernel speedup at the ordinary boundary. Contemporary
original ordinary costs are85.83/94.86µs; ordinary CPU changes are inconclusive.
Its allocation increases304B from59,000B. The stage is retained for verified work
reuse and allocation savings with explicit ownership. Ordinary reference gaps
remain; prepared-byte parity is uncertain and prepared-word remains a gap.
No unchanged job was repeated for favorable numbers.

The reference is **ImageSharp.Drawing2.1.7 / Fonts2.1.3 / ImageSharp3.1.12**, not
FFmpeg. Ordinary drawing retains the public layout paths at the origin and
translates/reconstructs geometry inside each Fill. The new prepared boundary
translates and materializes public paths once, disposes the layout helper, then
uses identity Fill. Every scan/coverage/blend and per-call reference allocation
remains timed. Managed preparation also retains working storage; the reference's
public Fill prepares working storage each time. Canvas reset is excluded equally;
canvases remain borrowed and independently owned. Reference preparation time,
allocations and retained capacity are unmeasured. This is a prepared drawing
comparison; the separate complete public integration still includes every layout
call and the full fitting search, and is not a bare shaping-engine comparison.
The candidate adapter also times its mode dispatch/environment lookup (one per
managed draw, two per reference draw); its isolated cost is unmeasured. Allocation
counters cover complete public drawing calls, excluding adapter dispatch and reset.

The frozen DejaVu Sans18px “AV To”,384×160 canvas and origin8/8 are unchanged.
Six short jobs precede six independent twelve-actual confirmations, using the
intended Windows Release DLL on the quiet single-CPU Linux VM. Both reference
brackets and every actual observation are retained. All eight original managed
and reference frames remain exact, including narrow wrapping, Arabic and fitting
controls at both depths. Those additional frames establish correctness, not
broader performance claims. Approved visual reference gates retain full16-bit
samples; managed sample exactness is unchanged.

Admitted preparation/ordinary peaks are42,488B for Latin,239,312B for Arabic,
41,984B for fitting and7,696B for narrow wrapping. Retained capacities are40,408,
238,384,39,904 and6,640B respectively. Drawing needs no additional admitted storage;
its56B GC allocation is a short-lived operation context. These are coarse logical
capacities excluding borrowed font/layout/canvas, not physical heap peaks. Charges
end on explicit disposal, without waiting for GC. Creation tokens do not govern
later draws; failed/pre-cancelled draws leave the plan reusable. Fifteen additional
public checks cover the new owner and existing rational/frozen pixel oracles.

Windows/Linux each pass **6,244 full/live checks**, plus170internal/126public
affected checks in normal/scalar modes. Unchanged resize/JPEG ISA evidence is
reused. The **same actual private Release package** and all three usage examples
pass both OSes, including frozen pixels after font/layout disposal and retained
admission release. One production/embedded build per OS serves the round; only
the public test assembly receives a second Windows build for the added frozen
oracle variants. The existing generated benchmark driver is reused.

[Evidence](text-render-preparation-evidence.json) records source/module identities,
observations, boundaries and qualification. Mirrored/member-verified raw
`text-render-preparation-qualified.tar.xz`:11,297,488B,273members, SHA256
`6622835194e308c3763fff695670a9cd55b94ec15d63032878badae499a3b05b`.
Actual private package:448,704B, SHA256
`96a290c2b6bea3b0a0707b286f32a6265e09d11b6fb5356e38a9bcce725ab0e6`.
Source-valid font/layout/fitting without raster, image composition, codecs and
resizing evidence is reused. The two new prepared public boundaries extend the
finite inventory287→289; full parity remains open. Publication is postponed.

[Preparation attribution](TEXT-PREPARATION-ATTRIBUTION.md) retains the diagnostic
lead; [endpoint storage](TEXT-ENDPOINT-STORAGE.md) retains superseded ordinary
costs and adverse/inconclusive evidence. The guard initially rejected a stale
source manifest before timing; only authorized changed inputs were reconciled.
Allocation postprocessing was adapted to the existing draw-only counter evidence;
completed captures were reused throughout recovery.
