# Complete-overwrite resize storage

Bilinear/bicubic now use the existing output allocator shared with Lanczos,
nearest and copies. Every selected filter writes every sample; failed private
results are disposed. Remove redundant zero initialization and allow ordinary
same-size output reuse under the existing capped policy. No kernel, arithmetic,
API, pool, cache framework or admission model is added.

| Complete bicubic resize,513×259 source | Before → after µs | Current zimg µs | Ratio / upper | Allocated B/op, before → after |
| --- | ---: | ---: | ---: | ---: |
| RGBA8 →256×129 | 558.92 → 549.84 | 418.61 | 1.313 / 1.321 | 332,186 → 199,981 |
| RGBA8 →1027×519 | 3,819.16 → 3,434.17 | 3,198.46 | 1.074 / 1.082 | 2,884,554 → 751,975 |
| RGBA16 →1027×519 | 3,384.68 → 3,336.56 | 2,872.72 | 1.161 / 1.167 | 5,016,602 → 5,016,602 |
| RGBA8 →32×16 | 350.47 → 384.00 | 403.93 | 0.951 / 0.962 | 199,372 → 197,298 |

Byte enlargement saves **384.99 µs /10.08% raw /8.94% normalized**, with separated
filtered/all-actual raw and normalized confidence. Word enlargement saves48.12 µs
/1.42%, also separated in both comparisons. Reduction saves9.08 µs /1.62%raw;
normalized1.76% intervals overlap. Claims apply to these controls and boundaries.
The independent short byte-enlargement point was8.76%; confirmation is authoritative.

Wide reduction's **33.53 µs /9.57% adverse raw point** separates, alongside a7.39%
slower native clock. Its normalized2.02% adverse point overlaps confidence;
no causal regression claim. It still passes at0.951x /upper0.962 and stops.
All raw/filtered samples, adverse points and native brackets are retained;
no scored reruns or tuning of this passing case.

The first three controls test reduction, large byte output within the existing
4 MiB ceiling, and word output above it. The additional byte enlargement is an
already-required frozen case, selected for that storage boundary; no shape or
quality grid is expanded. Complete geometry, working storage, current input
processing, output acquisition/ownership and disposal remain timed. Native zimg
retains its graph/scratch under the existing labeled boundary; these are not
equivalent one-shot setup comparisons. Managed exact results and native float
max-error/RMSE gates remain unchanged.

| Storage diagnostic, same controls | Owned output B | Governed admitted peak B, unchanged | Idle output capacity B, before → after |
| --- | ---: | ---: | ---: |
| RGBA8 →256×129 | 132,096 | 331,936 | 0 → 264,192 |
| RGBA8 →1027×519 | 2,132,052 | 2,888,024 | 0 → 2,132,052 |
| RGBA16 →1027×519 | 4,264,104 | 5,015,980 | 0 → 0 |
| RGBA8 →32×16 | 2,048 | 200,096 | 0 → 4,096 |

Idle capacity is measured in isolated processes after disposing two independent
outputs. It is separate from total allocated bytes and live admitted capacity.
The existing process pool retains at most two arrays within4 MiB total, shared
with other eligible operations. Governed calls never use or populate it; their
results stay charged for their actual lifetime, scratch is released, and each
root returns to zero after disposal. Admitted peaks exclude borrowed input.
Physical heap peaks and native allocation remain unmeasured. Large unbudgeted
output reuse trades allocations for idle retention; it is not free storage.

A private dirty-output projection passes796 existing public controls in normal
and scalar modes per OS. Linux exercises **all21 bicubic dispatch routes** plus
prepared/direct fallbacks; Windows exercises portable paths. Eight new tests
prove actual poisoned-array reuse, independent live results, terminal refusal,
cancellation cleanup, subsequent reuse and awaited PNG encoding after source
disposal. The selected production build passes **5,497 full/live checks per OS**,
193 internal and796 public affected checks per normal/scalar mode. The **same
actual Release package** and all three examples pass Windows/Linux.

One immutable production/internal build per OS serves all scoring and checks;
existing harnesses, short/confirm selectors and native workers are reused.
Private poison/retention diagnostics do not publish timing claims. A diagnostic
compile error and reflection-access error were corrected without changing
production, rebuilding it or replaying scored measurements.

[Evidence](resize-overwrite-output-storage-evidence.json) pins source, producers,
all observations, memory measurements, qualification and package proof. Raw
resize-overwrite-output-storage.tar.xz: **20,348,212 B /1,220 members**,
SHA256ba460618fcec926a216ae5f03701ab7752dda0019b7945b97f41d1072446c21e,
mirrored/member-verified, with prior source/producer dependencies retained.
Other affected bilinear/bicubic/pipeline costs remain historical; unchanged
Lanczos/nearest, codec, reading, drawing and text paths reuse valid evidence.
Full282-path CPU parity is open; publication remains postponed.
