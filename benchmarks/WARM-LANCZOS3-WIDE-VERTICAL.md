# Exact eight-channel vertical accumulation

Wider vertical vectors cut complete Lanczos3 time **10.8â€“20.5%** from the
preceding byte-product build. All four existing cases remain measured gaps
against independently confirmed warm single-CPU parity.

| Complete operation | Lokad Âµs | Reference Âµs | Ratio / conservative upper | Lokad allocated B/op | Time saved |
| --- | ---: | ---: | ---: | ---: | ---: |
| Reduce8 | 1,156.89 | 490.11 | 2.360 / 2.564 | 146,653 | 15.4% |
| Reduce16 | 1,322.43 | 508.06 | 2.603 / 2.900 | 146,634 | 10.8% |
| Enlarge8 | 6,018.99 | 4,062.54 | 1.482 / 1.590 | 351,910 | 20.5% |
| Enlarge16 | 7,319.41 | 3,929.17 | 1.863 / 1.947 | 4,617,090 | 11.9% |

AVX512F processes eight independent channels using the accepted signed32 limb
products and Int64 planes. Every lane keeps its tap order and absolute prefix
bound `2^62`. A remaining four-channel group uses AVX2; scalar Int128 remains
the fallback. The existing 4/8/4100-channel BigInteger controls cover group and
poll boundaries. All four protected full frames remain exact on both OS.
No coefficients, horizontal products, normalization, scratch, owner, budget,
work charges or 4096-channel cancellation boundaries change. Small allocation
differences reflect harness amortization; no array or cache was added.

A preceding unsigned word-product candidate passed exact controls but saved
only **3.9%** on Reduce16 (1.482â†’1.425 ms). It is parked without expansion or
full qualification. Selected source was restored from Git and rebuilt; the
parked DLL/test assemblies do not qualify this build.

The selected producer is immutable across four shorts. Original inputs, native
worker/quality proofs, harness and compiled selectors are reused. Reference is
**zimg, FFmpeg's zscale backend** with complete equivalent boundaries from the
[baseline](WARM-LANCZOS3-BASELINE.md). Eight scored quiet guards pass; one
Enlarge16 preguard refused I/O wait and launched nothing, then a fresh retry
passed. Both native brackets and selected/all-actual confidence views remain.
No long confirmation was run above one and no settings grid was expanded.

Qualification: **4,448 Release tests per OS** (1,635 internal, 1,881 public and
932 live FFmpeg), **47 Lanczos checks with all intrinsics disabled** and another
**47 with AVX512F disabled per OS**, the same actual private 0.2.0 package consumer
and all three examples on both systems. Twelve font proofs retain unchanged
paths. Published 0.1.0 is immutable. Other 227 rows retain their historical builds;
parity and replacement readiness stay open.

One bounded Reduce16 diagnostic passes both quiet guards. Its 1,492 CPU samples
place **60.9% in horizontal filtering**, 15.8% in vertical accumulation and 9.5%
in final application/output processing; the math library accounts for 6.2%.
These are diagnostic sample fractions, not scored timings. The target and pixel
checks completed; raw evidence records the collector exit after target termination.
A subsequent temporary premultiplied-row trial passes 49 focused controls and
all four protected full frames, but Reduce16 takes 1,349.94 µs versus 1,322.43 µs
(2.1% slower), adding 8,240 B temporary capacity. It is rejected after one quiet
short, without expansion or full qualification. Its first preguard refused I/O
wait before launch; a fresh prefix passed both guards. The accepted source and
producers are restored, with 47 freshly rebuilt focused checks per OS.

Next is a bounded horizontal four-tap vector feasibility audit, including the
retained paired-vector rejection. Prove exact prefix bounds, work/cancellation
and any changed storage/admission before implementation; no repeated profile.

[Numeric evidence](warm-lanczos3-wide-vertical-evidence.json) contains samples,
allocations, identities, bounds, parked/rejected controls and the sanitized profile.
Qualified source: `e78dfb8f1035d5d43d7e589df51b88a8a9fc45856a113bc268e0f330818e3a4e`.
Raw `l3v-qualified-windows.tar.gz`:
`50ee7ca7274827bf763090842f9a2dbdbae9b08613ec4cbc53b3a7d9e1b83758`;
`l3v-qualified-linux.tar.gz`:
`e95c617dec54d520d67112d5fbc6b9c599a52b0347aeb9f0775b6ab50c4596e9`.
Unchanged dependencies, original fixtures and worker remain in the baseline archive.
