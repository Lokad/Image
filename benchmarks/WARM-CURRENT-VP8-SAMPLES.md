# Current VP8 sample precision and warm cost

E113 reuses the E112 qualified production DLL, prepared BDN host, original packet
and native worker. Only the benchmark project is rebuilt; production, bothOS/live/
scalar and actual-package evidence is reused. The three existing packets behind
all five VP8 worker cases match frozen native normalized RGBA exactly, normal and
scalar. Removed former managed hashes and bounded RGB-error analysis from this
worker; input/reference settings remain unchanged.

One dedicated-VM Release screen follows a passing smoke run. One CPU, complete
fresh non-exposed seekable Stream acquisition/EOF/copies, decode/chroma/color/alpha
and independent output release; identical98,590-byte input. Both versioned quiet
guards pass. Normalized RGB/alpha is exact before and after measurement.

| Existing case | Lokad µs | FFmpeg µs | Ratio / upper | Allocated B/op |
|---|---:|---:|---:|---:|
| VP8/ALPH Stream to owned RGBA8,513×259 | 10,523.95 | 5,981.68 | 1.759 / 1.903 | 1,966,591 |

This resolves one current-producer uncertainty and demonstrates a1× gap. It is
one short screen, with no independent confirmation, optimization or speedup claim.
The other four VP8 timing rows remain prior-production. Resolve current encoder
costs before choosing smaller decoder work; no workload grid is added.

[Evidence](warm-current-vp8-samples-evidence.json) contains samples, allocations,
identities, exact-byte controls and qualification reuse. Full warm parity and
replacement readiness remain open.
