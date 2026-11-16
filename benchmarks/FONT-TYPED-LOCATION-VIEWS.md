# Typed font-location views

Current font-loading and complete public-integration costs are in [packed signed bounds](FONT-PACKED-BOUNDS.md). This report retains the qualified typed-location milestone and earlier measurements.

Eager glyph validation now selects the 2/4-byte loca representation once and scans
a borrowed ushort/uint view. Big-endian values, short-entry scaling, check/error
order, every glyph cancellation poll, fixed headers, mapping and ownership remain.
There is no new storage, cache, owner, API or parser policy. Both specialized scans
appear in generated code without per-entry generic conversion calls or boxing;
instrumented times are discarded.

Independent complete managed font loading saves **1.129 µs /5.41%**
across all twelve actuals, with separated raw intervals. Filtered results independently
save 1.127 µs /5.40%. Reference-normalized gain is **inconclusive** and not claimed.
Every original actual and native bracket remains; no favorable timing replay.

| Complete public operation / labeled reference | Managed / reference µs | Upper / status | Managed GC B/op |
| --- | ---: | ---: | ---: |
| Owned font load/dispose / FreeType 2.13.2 | 19.745 / 13.320 | 1.495 / scoped gap | 176 |
| One-shot load/fit/layout/draw/dispose / ImageSharp | 1184.030 / 6163.078 | 0.220 / pass | 299,920 |
| Prepare once plus eight owned draws / ImageSharp | 1660.420 / 6868.650 | 0.271 / pass | 301,266 |

The same frozen DejaVu Sans 2.37 Regular face is 757,076 B /6,253 glyphs.
FreeType copy/open/retain/dispose validates lazily; managed validation remains eager.
Loading is a scoped diagnostic, not general parser equivalence or overall text cost.
Text uses the complete ImageSharp.Drawing 2.1.7 /Fonts 2.1.3 /ImageSharp 3.1.12
public pipeline, with all fitting/layout calls, fresh canvases, explicit clears,
independent outputs and disposal. Exact managed layout/fitting/full16-bit samples
and approved reference visual gates pass. No new faces, scripts or settings.

Creation-only preparation is **1076.748 /6073.688 µs**;
only that diagnostic excludes prepared-owner disposal. The directly measured
eight-draw batch amortizes to **207.553 /858.581 µs/draw**,
including preparation. Stage times are never added. Complete observations establish
passes, not a causal pipeline gain. Prepared drawing remains a scoped uncertainty
with different retained working state; no retuning follows.

Font-owned bytes/descriptor stay **757,076 +256 B**, prepared capacity **48,480 B**,
governed complete peak **818,788 B**; no font/layout survives preparation. GC,
retained storage and peaks remain distinct. Untimed approximate prepared heap
deltas **42,832 /22,504 B** exclude idle pools; physical/native/JIT peaks are
unmeasured. Caller roots bypass unchanged default two-slot/4 MiB idle storage.

Windows/Linux each pass **6,721 full/live checks**, **265 internal/161 public**
focused normal/scalar checks and the **SAME actual package/all three examples**.
One core/embedded build per OS, one pack, no timed-Linux rebuild. Affected costs
reconcile before this one qualification milestone. Unchanged codec, resizing and
drawing execution reuse valid evidence. Published packages remain immutable;
publication and full 289-path completion are open.

[Evidence](font-typed-location-views-evidence.json) retains all identities, checks
and costs. Private package `958275977db3db3b682dfa152b8e81df207cae2b3ae14978869b7db0161628a0` (465,930 B)
contains timed DLL `dc2674db381bfdea18ab0aa6b8949e91a228e36ec2697b31ee317aced4b46c86`. Raw `font-typed-location-views-qualified.tar.xz`
(4,581,368 B/131 members), SHA256
`2b40efee0326c930a7ddc05ba379bf2a84ac007461e5e32a296468f2207c23f5`, is mirrored/member-verified with original tooling/package
dependencies. [Fixed-header views](FONT-FIXED-HEADER-VIEWS.md) retain prior history.
