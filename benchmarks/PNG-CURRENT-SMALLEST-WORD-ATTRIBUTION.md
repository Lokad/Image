# Current PNG SmallestSize word attribution

One two-second attachment to the existing complete RGBA16 SmallestSize9 operation
records 1,944 target-process samples. The lazy parser accounts for 1,404; bit output
149, token writing 100 and Huffman ordering 79. Its current JIT body is 2,146 B;
word matching is 199 B. Hot locations include dictionary access, tail-pair checks,
search continuation and lazy decisions, with live-state reloads around the chain
loop. Samples, skid and code size are not component times or predicted savings.

The immutable qualified PLI228 core and existing benchmark/native workers run the
same complete one-shot boundary as the [current costs](PNG-SMALLEST-ONE-SHOT-CURRENT.md).
The exact 963,202 B packet, full samples, frozen source/settings, output ownership
and both quiet guards pass. One CPU, sampler on another CPU, zero builds or new
controls. Necessary managed/native preparation and output remain included.
Instrumented elapsed is discarded; no current timing or allocation row changes.
Older pre-hash Optimal6 offsets remain historical and are not reused as current
machine-code evidence.

Next inspect a smaller dictionary-search execution region separated from the
lazy parser’s pending/output state. It may reduce chain-loop register pressure
and repeated spill/reloads. Preserve candidates, search limits, order/ties,
packets, direct cancellation and admitted ownership. This is a new architectural
hypothesis, not a demonstrated gain or permission to replay unchanged prefix/
word-match implementations. Reuse the existing byte/word level-9 controls.

[Evidence](png-current-smallest-word-attribution-evidence.json) pins actual-source
JIT, sample offsets, producers, guards and exact output. Mirrored/member-verified
raw `png-current-smallest-word-attribution.tar.xz`: 1,439,824 B / 42 members,
SHA256 `f0dfb736d1fb46a5b9a11a59bb12b3074ff4b7c0963d9b074e3be3c0051e6061`. Trace, map, analysis, source, immutable runtime,
validation and reproduction scripts are retained. Qualified core/package and
current level-9 reference/runtime archives are pinned dependencies. No production
change or repeated qualification. Publication is postponed.
