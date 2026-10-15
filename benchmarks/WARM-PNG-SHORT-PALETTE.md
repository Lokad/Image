# Short-palette PNG expansion

Byte-indexed PNG decoding with sixteen palette entries improves from **2.340×
FFmpeg** to independently confirmed **1.317× / upper 1.327×**, with **43.9% less
managed time**. Exact pixels are preserved at unchanged **1,496 managed B/op**.

| Quiet observation | Lokad.Image µs | FFmpeg µs | Ratio / upper | Managed B/op | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| Original scalar expansion | 257.12 | 109.90 | 2.340 / 2.918 | 1,496 | Above target |
| SIMD expansion, three actuals | 144.10 | 109.67 | 1.314 / 3.463 | 1,496 | Screen |
| Independent twelve actuals | 144.22 | 109.47 | 1.317 / 1.327 | 1,496 | Confirmed |
| Full 256-entry palette regression | 361.40 | 393.12 | 0.919 / 2.354 | 2,456 | Three-actual mean passes; prior confirmed |
| 256 entries / two-byte alpha regression | 534.90 | 364.33 | 1.468 / 1.646 | 2,208 | Three-actual mean passes; prior confirmed |

The original 513×259 byte raster's existing palette4 projection is reused:
j=original red>>4, i=17*j, RGB=(i,255-i,(73*i+19)%256), alpha=i.
Sixteen PLTE/tRNS entries remain unchanged; each index j is stored in one byte.
Original authoring follows [PNG](https://www.w3.org/TR/png-3/) with filter None
and BCL Optimal6. Every index, projected sample, chunk CRC and header is checked.
There is one new workload, no new original image or setting/shape grid.

One six-second profile records 5,956 samples, with **68.6%** in palette expansion.
The accepted primitive builds four channel vectors directly from existing
palettes of one to sixteen entries. SSSE3 shuffles sixteen valid byte indices
and interleaves RGBA. A group containing an invalid index resumes the original
scalar loop before any group store, preserving the exact valid prefix and
cancellation/error order. Missing/partial alpha, hidden RGB and scalar tails
remain exact. No padded palette, heap workspace or admission change is added.

Both subjects time complete identical-packet decoding, framing/inflation,
reconstruction, palette expansion and independent output acquisition/release.
Native PNG codec/input packet/frame wrappers are reused; actual PAL8 decoding
and full PAL8-to-owned-RGBA conversion repeat every operation on one conversion
thread. No output is cached. Startup, I/O, IPC, authoring and validation are
excluded. Native allocations are **unmeasured**; managed figures include harness
overhead.

Analytical and normal/scalar native RGBA, alpha/hidden RGB, geometry, source
preservation and two simultaneous independent owners pass. Sequential Release
runs use CPU0, one codec/conversion thread, normal SIMD, disabled tiering and
workstation nonconcurrent GC. Every versioned quiet guard passes. Candidate,
regressions and confirmation reuse one immutable core/harness/native worker.
All twelve confirmation actuals are retained; BenchmarkDotNet selects eleven.
The upper includes selected/all-actual intervals against the faster native
bracket's lower bound. Regression means pass before full qualification; short
intervals do not replace their earlier independent confirmations.

Eighteen row/guard controls pass in normal/scalar modes on both OSes, including
small/partial palettes, alpha variants, vector boundaries, invalid-prefix writes
and canceled-token precedence. Full **3,457 Release tests per OS**, including
**898 live comparisons**, pass. Consumer and all three examples pass through
the **same actual Release package** on Windows/Linux. An initial duplicate
receipt field is corrected before measurement; its compile failure is retained.
One successful harness build and one production candidate build suffice;
the native binary is reused. No whole-library method audit or repeated full/
package qualification is performed.

[Numeric evidence](warm-png-short-palette-evidence.json) retains every actual,
allocation, guard, fixture/source/assembly/job/native identity, profile and actual
package proof. Stop this confirmed pair; full required warm coverage remains open.
