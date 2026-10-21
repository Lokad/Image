# JPEG444 source-derived precision

C159 reduces the existing Original4:4:4 packets through coefficient precision
chosen from the source quality scale and admitted progressive scans. DQT values
retain the Original formula. DC and selected low AC coefficients may change;
the earlier AC-stage modeled cap is not the subsequent precision rule's quality
gate. No reference image or numerical-quality search enters the codec.

| Existing 513×259 procedural control | Previous B | Current / reference packet B | Source-PSNR gap |
| --- | ---: | ---: | ---: |
| Original4:4:4, RGBA8 | 141,528 | 138,074 / 133,041 | 0.113419 dB |
| Original4:4:4, RGBA16 | 141,529 | 138,002 / 133,043 | 0.116827 dB |

Savings are3,454/3,527 B. Both pass the unchanged0.25 dB matched-quality gate,
exact alpha and decoder bounds (maximum4, RGB RMSE1.5). Source, sampling,
managed quality69/native qscale5 and0x204060 flattening stay frozen. Managed packets use
16 progressive scans; the frozen FFmpeg reference remains baseline MJPEG. Equal numeric
quality does not establish equivalence. The largest complete-packet gap remains
about3.8%; these controls make no photographic, diagram or general JPEG claim.
Optional CompactColor remains separate. Passing420/422/grayscale/WebP controls
retain their evidence and stay stopped; PNG's two open packet gates remain open.

Let `p=(16*scale+50)/100`. Select the largest power-of-two factor `f` such that
`q*f<=p`, or one if none exists. Where `f>1`, use nearest-away source DCT divided
by `q*f`, clamp to legal DC/AC multiples, then multiply by `f`. Else retain the
preceding coefficient. First progressive scans code these exact multiples:
one DC scan/component, then maximal contiguous AC bands sharing a point transform.
Each scan fits its own canonical table and bounded EOB runs. No nonzero refinement
bits are discarded. `BaselineOnly=true` requires baseline syntax with identical
quantizers and reconstructed pixels; fixed-workspace coding also uses baseline.
Other sampling/policy configurations and restart output retain baseline coding.

The plan reuses the admitted event capacity for coefficients, without a second
frame array or encoded-input copy. The precision rule charges2,880 work/MCU
before mutation; conservative scan/table and header bounds enter adaptive/async
admission. Existing8KiB fixed scratch, shared root, terminal refusal, output/work
limits, cancellation and asynchronous ownership remain. A refusal never retries
baseline coding. One scan's table is live within the existing64KiB allowance.

Both platforms pass4,999 Release tests (2,081 internal,1,982 public,936 live
FFmpeg), plus170 internal and62 public scalar checks. Focused cases cover
rounding just below ties, legal amplitudes, pre-mutation refusal/cancellation,
the32,767-block EOB-run boundary, baseline syntax/pixels, exact output quotas,
aggregate admission/refusal cleanup and subsequent successful operations.
Public goldens cover RGBA8/16, caller samples, forward/cancellable output,
fixed/adaptive storage, async output, borrowed inputs and independent lifetimes.
The same actual private0.2.0 package and all three examples pass on both OS.

One core build/OS stays frozen throughout qualification. A test-only API-access
fix and updated admission-bound assertion rebuild only affected tests. The
independent prototype initially rounded by adding0.5; correcting its half-fraction
comparison makes every actual packet byte match at both depths. Actual quality
is rechecked for those corrected pixels. All411,840 chosen coefficients/depth
are independently reconstructed;38,610 affected source coefficients/depth replay
their original nearest values. No benchmark/native/differential rebuild,
reference encode or quality/settings grid is repeated.

One quiet-VM three-actual complete forward444 screen records
**1,300,808.740 µs / reference3,775.236 µs**, ratio344.564/upper380.168,
**613,208 B/op**. Both versioned idle guards pass; native allocations are
unmeasured. Prior C150 was1,282,590.681 µs and564,192 B/op. Costs are accepted for
the verified packet savings. This is neither a CPU-parity pass nor an independent
confirmation; speed-only work remains deferred.

Only five444 ledger packets/current forward cost invalidate C150. Other relevant
paths and confirmations remain valid. The finite256-case warm objective,
replacement readiness and release qualification remain open. Rejected companion
C158 added eight bytes and stopped before quality/depth/timing expansion;
earlier exact-coefficient entropy candidates remain retained, not CPU-rejected.

[Numeric evidence and raw archive hashes](jpeg444-point-precision-evidence.json)
identify the frozen builds, actual package, original receipts and retained raw
archives. The [complete-packet ledger](JPEG-COMPLETE-PACKET-AUDIT.md) states current
size gates; [the summary](../docs/benchmark.md) remains limited to three tables.
