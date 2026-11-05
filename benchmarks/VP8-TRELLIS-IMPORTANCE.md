# VP8 trellis model preparation

The sixteen fixed Gaussian importance values are now immutable module data,
independently derived from the existing formula. The checked copy and custom
`Apply` weights remain unchanged. This removes repeated exponential evaluation
without adding frame/search span plumbing, an input-dependent cache, owned
capacity or a pool. It adds 32 B of policy data. Candidate order, ties, bounds,
cancellation and complete packets remain exact.

One counterbalanced comparison finds **119.90 µs / 0.86% elapsed improvement** on
the frozen 513×259 opaque diagram, Q75/effort6: 13,826.11→13,706.21 µs. Twelve
alternating pairs × sixteen complete public encodes per subject, all retained.
The ratio interval is **0.98276379–0.99997432**;
**it only narrowly excludes no change**. Both subjects allocate 754,344 B/op.
Stop tuning this helper; this is a narrow gain, not robust broad improvement or
FFmpeg parity. Fresh output construction/growth, encoding, independent ownership
and disposal are timed; equal source setup, JIT and validation are excluded.

| Complete frozen control | Lokad µs | FFmpeg/libwebp µs | Ratio / upper | Managed B/op | Exact packet B, both |
| --- | ---: | ---: | ---: | ---: | ---: |
| Diagram, Q75/effort6 | 13,502.33 | 10,337.05 | 1.306 / 1.329 | 754,344 | 6,818 |

This once-refreshed current cost remains a gap. Both native brackets, all twelve
actuals, exact complete packet, source, loaded producer and quiet guards pass.
Native allocations and physical/admitted peaks were not remeasured. No managed
allocation saving or cross-setting compression/CPU claim follows.

The [count audit](VP8-TRELLIS-PREPARATION-AUDIT.md) found 1,122 identical model
derivations / 17,952 exponentials on this control; these were counts, not phase
times. A first frame-owned/span-borrowed design passes focused checks but has no
useful short lead on diagram or photograph and is restored exactly. The selected
fixed-data short is also adverse versus an older independent capture, with native
clock movement. All adverse observations are retained. The simple fixed model,
unchanged calling convention and concrete repetition justified one paired check;
no further samples were sought after its marginally separated result.

Windows/Linux pass **5,642 full/live checks each**, plus 853 internal / 357 public
affected checks in both normal and scalar modes. The one new actual-production
fact independently derives the model and checks every value, canaries, copy
independence, all short destination failures before mutation and zero warmed
allocation. Unchanged QDP300 AVX512-off kernels reuse their 26 checks/OS. The
**same actual private Release package and all three examples pass both OSes**.
An initial test-only span-overload compile error and a later missing-log transfer
are recorded; corrected successful checks were collected without rerunning them.

Only methods5/6 use the changed model. Other affected method5/6 H costs remain
historical; the seven displayed method2/4 VP8 paths reuse unchanged
[luma4 evidence](VP8-LUMA4-PREPARATION.md). VP8L, PNG/JPEG, decoders, resizing and
text/drawing are unaffected. This is development-source qualification, not a full
release refresh on the actual package DLL. The 282-path objective remains open;
publication is postponed.

[Evidence](vp8-trellis-importance-evidence.json) pins producers, all stages,
confidence, packets, qualification and archive members. Reproduce with retained
`vtp306*-checks.py`, `vtp306-paired-*.py`, `vtp306-refresh*.py`, qualification and
package scripts; unchanged harness/selectors/reference tools are pinned through
the qualified dependencies. No upstream sources or implementation tables are
included in production.

Raw `vp8-trellis-importance-qualified.tar.xz`: 5,920,896 B /
265 members, SHA256 `084e99e66737c97304f39ecc0e3d4e56124852e41dc1594ad4f44bef58d5302d`;
mirrored and member-verified locally and on the VM.
