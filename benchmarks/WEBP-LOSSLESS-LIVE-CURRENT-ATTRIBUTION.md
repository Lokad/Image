# Current VP8L live-token attribution

Historical attribution for the accepted
[Huffman front end](WEBP-HUFFMAN-READ-FRONTEND.md). That round owns current
costs and generated code; the original counts and hypothesis below are retained.

One two-second CPU0 capture on the current actual package DLL investigates the
[demonstrated live-token decode gap](WEBP-LOSSLESS-LIVE-CURRENT-CONFIDENCE.md).
CPU1 collects samples; every original packet/pixel/native/source/owner/cancellation
and idle-v2 check passes. Instrumented timings are discarded.

Of 1,995 sample locations, Huffman.Read has 724 and ReadImageCore has 696.
Transform Apply has 212, ApplyPredictor 133 and PredictAndAdd 115. These counts
include inlining and sampling skid; they are not phase times, removable percentages
or predicted gains. Annotated instructions retain the actual hot locations.

Huffman.Read generates 1,278 B combining common short-prefix lookup with canonical
and per-bit fallback. The main image decoder is 5,723 B; transform Apply and
ApplyPredictor are 786/837 B. Common table lookup/refill/publication and repeated
calls are visible. The next bounded hypothesis separates an inline single/prefix
front end from a cold private fallback, preserving the entire original read order,
successful bit-cache publication, truncation/cancellation behavior and ownership.
Inlining can increase caller code and register pressure; complete costs and actual
generated callers must decide whether to retain it.

No new table, buffer, pool, owner, memory reservation or numerical policy is
proposed. Direct reader/EOF/cancellation and exact pixels come first. Use the
current immutable baseline and existing live-token control; reconcile affected
VP8L/ALPH/validation costs before a coherent accepted Windows/Linux/scalar/ISA/
SAME-package milestone. Do not assume the two other read uncertainties are gaps.

No production, test, harness, selector, native, fixture or package build and no
broad qualification repeat. WLR845 remains the timing owner: 3,206.92/FFmpeg
3,025.66 µs, ratio interval 1.048–1.070, 152,728 B/op. Existing package proof and
6,449 full/live checks per OS are reused. No new storage or peak/allocation claim.

[Evidence](webp-lossless-live-current-attribution-evidence.json) pins source,
samples, generated code, annotation, original gates and the proposed stage.
Raw `webp-lossless-live-current-attribution.tar.xz`, SHA256
`51242680a50e223eab9937fb087b5919d92159b203cecbcc26ca04939bc45716`
(129,004 B / 26 members), is mirrored/member-verified. Dependencies retain the
actual package, reference workers, immutable harness and source identities.
All 289 paths remain; publication is postponed.
