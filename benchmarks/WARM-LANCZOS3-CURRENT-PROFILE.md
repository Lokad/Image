# Current Lanczos3 reduction diagnostic

Updated 2026-10-20. One completed **one-second diagnostic** on the current qualified
RGBA16 reduction producer collected **958 samples**. Both idle-v2 guards
pass; actual producer/harness/selector bytes remain unchanged. These are sampled
CPU locations, **not scored timings or a new performance confirmation**.

- Horizontal16: 416 samples (43.4%).
- Vertical accumulation: 262 (27.3%).
- Apply, including inlined final normalization: 139 (14.5%).

This supersedes the older coefficient-loop profile as optimization direction.
The largest current measured gap remains Reduce16 at 1.836×; no numeric row or
qualification changes. The horizontal pass recomputes RGBA16 premultiplication
for overlapping source windows. Next bounded experiment: prepare one admitted
premultiplied source row, reuse it through horizontal weighting, preserve exact
signed sums and all limits/cancellation/ownership, then screen this existing case.
Do not assume the additional memory traffic or allocation will be worthwhile.
Retain the old path when selected before admission for insufficient local
workspace; a cooperative admission refusal must terminate without fallback.

An initial longer capture ended with SIGTERM (-15) after retaining 1,495 samples;
its benchmark exited successfully and raw data is retained. It is not presented
as a completed capture. The independent one-second capture completed with exit0.
Neither profile supplies timing/allocation results. No production/native/harness/
selector build, repeated full/package proof, new image or settings grid.

[Evidence](warm-lanczos3-current-profile-evidence.json). Current producer:
`c12fbeebea4edb6b4959837183a4b12379756be3139bd46b5e3c1798ff97671d`. Raw `h17-controls-linux.tar.gz`:
`61b0d4fed828fec4f863ecac68ea23ad0b13897950ad98e3e64b8832baa757e4`. Full parity/release readiness remains open.
