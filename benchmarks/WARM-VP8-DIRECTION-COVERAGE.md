# VP8 direction coverage and matched-quality compression

Both primary content controls now pass **complete-packet size and the unchanged
quality gates**: photograph **17,366 /17,688 B**, diagram **6,722 /7,100 B**
(managed /FFmpeg-libwebp). The [seven-control ledger](WARM-VP8-SIZE-COVERAGE.md)
also retains five procedural fixed-q100 gaps; this is not whole-codec parity.

| Frozen opaque513×259 control | Managed / reference packet B | Managed / native quality | Source-PSNR gap dB | Complete managed / reference µs | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| Photograph8 | 17,366 / 17,688 | 77 /75 | 0.140944 | 33,904.97 / 9,712.89 | 589,432 |
| Diagram8 | 6,722 / 7,100 | 75 /75 | 0.117409 | 13,778.71 / 5,837.50 | 570,136 |

The admitted mixed path at effort4..6 and q>0 makes all ten already implemented
luma directions eligible. The retained native header audit selected formerly
excluded directions6..9 for19.43%/10.19% of photograph/diagram small blocks, giving
this one bounded coverage change a concrete reason. Extra prediction/quantization
trials are included in optional work admission. Scratch capacity is unchanged;
q0 and constrained/lower-effort branches retain their prior search caps. Quantizer
mapping, nearest rounding, exact alpha, shared-root refusal and output lifetimes
are preserved. No native source or tables are copied.

At the former managed78/75 settings, the new packets are18,284/6,722 B and PSNR
gaps0.179085/0.117409dB; those results and packets remain recorded. The changed
photograph encoder delivers higher quality at78, so the existing **bounded-v1
closest-quality matcher** is reused on that photograph only: seeds1,25,50,75,100
and one measured log-SSE refinement77. It selects77 by quality distance before
size acceptance. Its0.140944dB gap is closer than78's0.179085dB. The already passing
diagram75 anchor is revalidated without another search. This refreshes a managed
benchmark input, not the library's quality formula or the reference baseline.

Native quality75/effort4, input rasters, native packets, source-PSNR tolerance
≤0.25dB, exact alpha and decoder bounds remain unchanged. Equal numerical qualities
are not the matching rule. Normal/scalar native RGBA agrees exactly for both
selected packets; borrowed-source and independent-output checks pass. There is no
quality/lambda/effort grid, byte-driven quality selection or broader content claim.

Both OSes pass **4,603 tests** (1,708 internal,1,961 public,934 live FFmpeg),
136 internal+2 public+2 differential scalar checks, and the same actual private
0.2.0 package/all three examples. One core and one internal-test build perOS;
public/differential/reference/harness/native/selector project binaries are reused.
The final build stays immutable throughout the screen, matching, qualification
and cost measurements. Tool preparation corrections do not rebuild production
or generate additional quality evaluations.

Two sequential three-actual short cost screens retain idle-v2 guards and native
pre/post brackets on the dedicated EPYC9V45 VM, one pinned CPU,.NET10.0.8, normal
SIMD, tiering off, nonconcurrent workstation GC. Complete admission, normalization,
encoding, fresh output growth and independent ownership are included equivalently;
preparation, validation, IPC and startup are excluded. Native allocation totals
are unmeasured. CPU costs remain recorded, with parity enforcement, independent
timing confirmations and speed-only optimization deferred while compression is
open. Only the same two primary benchmark rows change.

[Numeric evidence](vp8-direction-coverage-evidence.json) retains all actuals,
selected and former-point quality data, the complete six-evaluation matching
transcript, identities, allocations and guards. Exact raw archives on both hosts:

- `c57-qualified-windows.tar.gz`: `d722f8684776fc3c32fec9812281b0c364841943d97b3d3dd4bcabf096adede3`

- `c57-qualified-linux.tar.gz`: `1d3a2259acac2aee97712a391a56871e995113310c6274b1f97960d40c066f9f`

Actual private package SHA256: `722ecaa08c7895bc4b5c7ea3b8fd93dee7ca5fcb123bb0ca9a93bd3e9d5d0e5b`.
