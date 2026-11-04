# Shared word column preparation

RGBA16 small-support resizing now uses the existing byte/word geometry helper.
Vertical geometry occupies unpublished output-row tails until every aligned prefix
strip finishes; final strips rebuild geometry after those bytes retire. Relative
byte offsets remove the absolute-offset/subtraction pass. Where both scales<2.25,
nine cached rows support eight-column strips; the2.25 boundary retains ten rows/
six columns. Typed filtering, tap order, exact rounding/repair and portable paths
remain. There is no new owner, pool, session or public API.

Independent confirmations on the existing **513×259 RGBA16→256×129, input+output+
4KiB** control save **126.92µs /14.46% raw elapsed**. Filtered/all-actual raw and
conservative bracket-normalized confidence separate. Normalized gain is14.58%,
with native clock0.15% slower. No gain is assigned between shared geometry and
wider strips. The remaining upper1.223 is still a gap.

| Complete resize | Managed µs | zimg µs | Ratio / upper | Allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Current unchanged baseline,12actual/11filtered | 877.94 | 618.17 | 1.420 /1.428 | 114 |
| Selected candidate, independent12/12 | 751.02 | 619.10 | 1.213 /1.223 | 113 |

The three-actual screen750.98µs also remains in the evidence; no timing repeats
or additional variants follow. The114→113 ceiling is batching, not an allocation
saving: raw27,128B/240ops and30,712B/272ops both fit112B/op+248B fixed overhead.

Fixed-array payload rises3,640→4,080B within the unchanged4,096B admission.
Metadata96B×129=12,384B borrows already owned264,192B result capacity, adding no
owned capacity or retained operation scratch. Physical/complete admitted peaks
are not remeasured. Existing ungoverned pixel pooling remains separately bounded;
governed owners, terminal refusal and independent output lifetimes are unchanged.

At this geometry, horizontal strips fall43→32 and geometry construction43→one
preparation+two final strip passes. These are logical work counts, not timings.
For eight columns, source support span(8−1+4)×scaleX<24.75 contains at most25integer
positions; every relative offset is checked before byte storage. At most nine
active consecutive rows have distinct modulo9slots, including repeated clamped
edge contributions. This preserves the original per-pixel operation order.

The complete managed public Resize includes preparation, admission, independent
output allocation/materialization/disposal. The explicitly labeled **zimg3.0.6
FFmpeg zscale backend** retains graph/input workspace/scratch before timing;
halo/deinterleaving, filtering, independent planar/packed output, repacking and
release stay timed. This is the established retained-state boundary, not one-shot
setup parity. Managed frozen/rational pixels remain exact; native RGB/alpha
maximum1 and RMSE≤0.25 remain. The original word worker is unchanged.

Both OSes pass **5,628 full/live checks** (2,393internal,2,287public,948live).
Initial Linux focused33internal/521public checks pass normally/scalar; Windows
final34/521 do too. A Linux test-only build adds the exact2.25 boundary and three
final normal/scalar word cases, reusing the measured production DLL. Frozen pixels,
opaque odd151×83→75×41 and boundary225×99→100×44 reductions agree with the independent
public direct path, source and4KiB ownership checks. These are functional controls,
not an expanded performance grid. One production version per OS, Windows one/
Linux two internal-test versions; no public/harness/native/selector builds.

The **same actual Release package/all three usage examples** pass both OSes.
The packaged frozen word256×129 case checks shared admission/refusal, cancellation,
unchanged source, independently surviving outputs and subsequent recovery. One
coherent broad/package milestone follows the bounded experiment. A reused selector
pathname fails identity before actuals; the archived original own selector is
recovered. A later postcheck compares word and byte worker identities incorrectly;
all12completed baseline actuals are recovered against the original word identity,
without retiming. All failure records remain. Published packages are unchanged.

[Evidence](bicubic-word-column-preparation-evidence.json) pins current costs,
sources, immutable producers, every actual/confidence decision and actual package.
Mirrored/member-verified `bicubic-word-column-preparation-qualified.tar.xz`:
3,620,812B /182members, SHA256
`eec4f826efca86a9ec4ab8ab06dbb0183f4985a05987250c07be42e2bef21384`.
Measured modules, original/final sources, selectors/reference worker, guards,
failure/recovery, qualification and reproduction remain with dependencies pinned.
The summary surfaces this already-required path; the282-path matrix is unchanged.
Byte, other resize kernels, codecs and text/drawing reuse valid unchanged-path
evidence. Full parity remains open; publication stays postponed.
