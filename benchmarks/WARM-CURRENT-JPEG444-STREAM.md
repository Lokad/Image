# Current forward-only JPEG444 cost

The existing 513×259 RGBA8 control keeps its frozen input, 0x204060 flattening,
native qscale5 and single CPU. Managed reference quality85 reproduces the complete
packet with only the approved attribution COM difference: **133,039/133,041 B**.
Source error and reconstructed pixels agree exactly. Normalization, encoding,
fresh forward-only wrapper/MemoryStream storage and output release are timed.

| Quiet VM run | Lokad µs | FFmpeg µs | Ratio/upper | Allocated B/op |
|---|---:|---:|---:|---:|
| Short | 9,777.77 | 3,770.50 | 2.593/2.792 | 820,080 |

All three actual/selected iterations and both idle-v2 guards pass. This is a
current gap observation, with no independent confirmation or speedup claim.
The [historical source-policy screen](JPEG444-CODED-SOURCE-HISTOGRAMS.md) describes
removed code; its 512× result does not establish a current performance gap.

The immutable library/benchmark/native/fixtures are reused. A verified archived
host binds this exact case; a prior smoke correctly rejected a host fixed to
420. No library or harness build, quality sweep or workload expansion occurred.
Unchanged production reuses E240's 5,130 checks per platform and the same actual
private Release package/all three usage examples. No package was regenerated.

[Evidence](warm-current-jpeg444-stream-evidence.json) records receipts and reuse.
JPEG444 now leads current demonstrated encoder gaps; full 257-case/1× and
replacement readiness remain open.
