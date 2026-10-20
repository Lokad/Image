# Lanczos3 reduction diagnostic before certified FMA

Updated 2026-10-20. One completed **one-second diagnostic** on the then-qualified
RGBA16 reduction producer collected **958 samples**. Both idle-v2 guards
pass; actual producer/harness/selector bytes remain unchanged. These are sampled
CPU locations, **not scored timings or a new performance confirmation**.

- Horizontal16: 416 samples (43.4%).
- Vertical accumulation: 262 (27.3%).
- Apply, including inlined final normalization: 139 (14.5%).

This supplied optimization direction for the subsequent experiments. At capture,
Reduce16 was the largest measured gap at 1.836×. The [prepared-row experiment](WARM-LANCZOS3-PREPARED-ROW-TRIAL.md) tested reuse of
RGBA16 premultiplication across overlapping windows, but only saved7.4%/3.8% and
added storage and was rejected. The subsequent
[certified FMA round](WARM-LANCZOS3-CERTIFIED-FMA.md) retains integer horizontal
filtering and accelerates vertical sums, saving13.6–23.8% across the four cases.
These earlier sampled fractions are historical and do not describe that producer.

An initial longer capture ended with SIGTERM (-15) after retaining 1,495 samples;
its benchmark exited successfully and raw data is retained. It is not presented
as a completed capture. The independent one-second capture completed with exit0.
Neither profile supplies timing/allocation results. No production/native/harness/
selector build, repeated full/package proof, new image or settings grid.

[Evidence](warm-lanczos3-current-profile-evidence.json). Sampled producer:
`c12fbeebea4edb6b4959837183a4b12379756be3139bd46b5e3c1798ff97671d`. Raw `h17-controls-linux.tar.gz`:
`61b0d4fed828fec4f863ecac68ea23ad0b13897950ad98e3e64b8832baa757e4`. Full parity/release readiness remains open.
