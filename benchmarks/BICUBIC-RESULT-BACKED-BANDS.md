# Result-backed bicubic row bands

Complete RGBA16 width-enlarging/height-reducing resizing now borrows a larger
filtered-column workspace from its unpublished output. The original 513×259
control becomes 1027×129. Default workspace saves **86.35 µs /
9.31% raw / 8.98% normalized**;
input+output+4 KiB saves **182.46 µs /
17.47% raw / 16.73% normalized**.
Filtered and all-actual raw/reference-normalized confidence separates for both.
Native points move -0.35%/-0.88%.

The shared bounded vertical case has an explicit adverse effect:
**424.04 → 486.02 µs, +61.98 µs /
14.62%**, with separated raw and normalized intervals. It keeps
the identity-geometry specialization and passes parity. Accept the joint stage
for substantial mixed-direction savings and a newly passing default case;
this is not a uniform speedup. Do not tune that passing case independently.

| Existing case | Original µs | Current µs | zimg µs | Upper / status | Allocated B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| Mixed16 | 927.88 | 841.53 | 847.03 | 0.997 / pass | 114 |
| TightMixed16 | 1,044.67 | 862.21 | 844.22 | 1.025 / gap | 114 |
| TightVertical16 | 424.04 | 486.02 | 593.16 | 0.824 / pass | 113 |

Each current cost and contemporary original has one independent twelve-actual
confirmation, with before/after native brackets. The default case passes/stops;
the tight mixed case retains about18 µs of demonstrated gap. Twelve other word
paths reuse source-valid evidence, including four cached costs from
[identity cache rows](BICUBIC-IDENTITY-CACHE-ROWS.md): fifteen word controls now
10pass/4gap/1uncertain.
Historical917.38/987.47/458.51 µs points remain in
[bounded-band](BICUBIC-BOUNDED-BAND-EXECUTION.md) and
[identity-geometry](BICUBIC-IDENTITY-WORD-GEOMETRY.md) history. Causal claims use
the contemporary unchanged original, not those older clocks.

## Boundaries and storage

Complete public Resize, admission, preparation, filtering, exact-result repair,
independently owned output and disposal stay timed. The explicitly labeled
reference is zimg, FFmpeg zscale's backend, with graph/input/scratch prepared
outside timing. Native packed-input deinterleaving, four-pixel clamped halo,
filtering, owned output/repacking/release remain timed. This does not establish
one-shot native setup parity or FFmpeg CLI performance.

The 64-row specialization borrows28,136B from already-admitted output before
existing horizontal geometry. Frozen output uses payload[945408,973544),
geometry begins973596, and only rows0..114 may be written while the bank is
live. Retire that view, then overwrite its suffix using original8/16-row ranges.
One source body retains exact tap order, certificates, OriginalWord repair and
rounding; capacities are compile-time constants. No decoded-history cache.
Modelled source conversions fall182115→142614/157491→139536 and driver columns
17459→4108/9243→3081; these counts are not CPU phase measurements.

No extra heap capacity, pool, session or retained scratch is introduced. Reserve
the existing4/8KiB local scratch once before allocation; refusal is terminal.
Small stack arenas occupy4/8KiB; large spans borrow only existing logical result
bytes, never inflated capacity. Mixed/vertical output owns1059864/529416B until
disposal. Existing default pixel-pool retention is separate; governed outputs
bypass it. Physical/native peaks are unmeasured. The actual-package SIMD shared
two-result peak remains2127920B (2×1059864+8192); surviving results stay charged.

Managed rational/certified pixels, transparent samples, source ownership and
independent output lifetimes remain exact. Existing scoped native float bounds
remain RGB/alpha maximum1/RMSE0.25, with zero-alpha RGBzero. Cancellation checks
bound large-band decode to32source rows and filtering/output to8rows. Codec
packets, precision, admission and asynchronous contracts are unchanged.

## Verification and reproduction

Both OSes pass**5,948 full/live checks**,84internal/556public normal and scalar
focused checks,188internal/703public with AVX2 or AVX512 disabled, and the SAME
actual private Release0.2.1 package plus all three examples. Package
`2c56b42a85771f7fb861f83e273678ffd476e3a81e8f1ef623fed3850b8c5c9a`; Windows main`3e0969368accbc6f09d1d153e61d312d62470077bdef068ea4038a1f413afd59`;
Linux main`a0c9dd32b432480cde0ed6e8f3c3acb74959a0f10772a61a6c6c3d77493fbf24`. Published0.2.0 remains unchanged.

RBA624 records source/layout counts before implementation. InitialRWB625 checks
caught an undersized generalization fixture (61safe rows,64required), corrected
before timing. RWB626 adds bounded internal cancellation and supplies the sole
short screen864.81 µs. RWB627 finalizes source formatting before confirmation.
Three immutable build versions perOS; only the final one is qualified/packaged.
No favorable repeats, profiles, new benchmark fixtures or settings grid.

Raw:`bicubic-result-backed-bands-qualified.tar.xz`, SHA-256`c7ab60b06a13c23d25cec7cb4e70111fbcbdada8820eb7e56c1bd3e97e9ff6cb`,
18,131,756B/390members, mirrored and member-verified. It retains
all prototype sources/modules, failed setup, checks, selectors/workers and
receipts; dependencies identify retained source-valid evidence. Reproduce using
the archivedrwb627build/focused/measure/observe/qualify/isa/package scripts and
frozen worker identities. [Structured evidence](bicubic-result-backed-bands-evidence.json).
Publication and full actual-package126comparison refresh remain postponed.
