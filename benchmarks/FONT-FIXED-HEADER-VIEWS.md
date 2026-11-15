# Fixed font-header views

HHV1052 reads the two fixed header fields directly after the original ten-byte
minimum check. It removes an intermediate span and redundant subslice setup;
zero-contour handling, the same two/eight-byte addresses, signed-bound comparisons,
error order, eager validation, every glyph cancellation poll and owned copying remain.
No storage, admission, cache, owner, API, ISA or parser-policy change is introduced.

Contemporary complete font loading improves **22.183→21.155 µs** across all twelve
actuals, saving **1.027 µs / 4.631% raw**. Filtered summaries independently
save **1.035 µs / 4.667%**; both raw intervals separate.
Conservative reference-normalized gain is **4.717%**, also separated;
reference clock moves +0.053%. All actuals/native brackets remain. The
table uses the established filtered font summary with the worse filtered/all-actual
confidence bound. No historical-clock or overall text gain is claimed.

| Complete public boundary / reference | Lokad.Image µs | Reference µs | Ratio / upper | Managed GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| Owned font load/dispose / FreeType 2.13.2 | 21.136 | 13.364 | 1.582 / 1.595 | 176 | Scoped gap |
| Complete one-shot / ImageSharp | 1,147.320 | 6,145.058 | 0.187 / 0.207 | 299,914 | Pass/stop |
| Preparation plus eight owned draws / ImageSharp | 1,662.299 | 6,892.612 | 0.241 / 0.270 | 301,266 | Pass/stop |

Font input is frozen DejaVu Sans 2.37 Regular, 757,076 B / 6,253 glyphs. **FreeType
2.13.2 copy/open/retain/dispose** is an approved tooling reference. It validates
lazily while managed loading eagerly checks mappings/glyph headers: no general
parser equivalence. The old native metadata's approval flag predates that approval.
Production remains safe managed .NET 10 without dependencies or native bindings.

The two complete ConstrainedFit RGBA16 integrations use **ImageSharp.Drawing 2.1.7 /
Fonts 2.1.3 / ImageSharp 3.1.12**. Every fitting/layout call, fresh owned canvas,
explicit clear, independent output and disposal remains inside its declared boundary.
Eight-draw amortization is **207.787 / 861.576 µs/draw**, including preparation.
Creation-only preparation is **1,089.995 / 6,026.749 µs**; disposal is excluded only
from that diagnostic. PreparedDraw remains uncertain. Never add historical stages.

Managed font allocation remains **176 B/op**; owned data/descriptor capacity stays
**757,076 + 256 B** through face disposal. Default idle storage remains two slots /
4 MiB; caller roots bypass it. Prepared raster retains **48,480 B**, no font/layout;
governed complete peak stays **818,788 B**. Allocation, admitted/active capacity,
idle retention and approximate heap deltas stay separate. Physical/native/JIT process
peaks are unmeasured. All exact full-word/layout/visual/ownership gates remain.

FPA1051's 1,999 sample locations and current-module JIT motivated this change;
counts/skid/inlining are leads, not phase costs or gain forecasts. Constructor code
shrinks **3,169→3,094 B**, frame **168→152 B**; only measured complete costs prove
the gain. Existing malformed-header/short-long-loca/signed-bound/metrics/ownership
checks pass. Each OS passes **6,721 full/live checks**, **265 internal /161 public**
focused normal/scalar checks and the **same actual timed Release package / three
examples**. One core/embedded build per OS, no public/harness/reference/fixture build,
one pack, no final Linux rebuild after timing. Broad/package checks run once after
affected performance reconciliation.

Only font construction changes. [Drawing costs](TEXT-DRAW-CURRENT-CONFIDENCE.md)
reuse unchanged timed paths on their previous producer: font construction is outside
those clocks. Codecs, resizing, mapping/outlines/kerning and other owners reuse valid
evidence. No whole-release refresh or expanded workload matrix; publication postponed.

[Evidence](font-fixed-header-views-evidence.json) records scope, source equivalence,
current costs and qualification. Mirrored/member-verified raw
`tmp/font-fixed-header-views-qualified.tar.xz`: 5,199,384 B / 186 members,
SHA-256 `affae0b21a43cce688d489f70052ab2e90140364501fa4dd21d08d9627a95867`. Actual package: 465,559 B,
SHA-256 `14ac5ffd20e8c8cddeea54ab534cd8cfdd41e4b5cd6c5db0236b4fe02b988af5`.
Prior [location preparation](FONT-HEADER-VALIDATION.md) retains its original costs
and raw evidence. Loading remains a scoped gap; the full 289-path objective is open.
