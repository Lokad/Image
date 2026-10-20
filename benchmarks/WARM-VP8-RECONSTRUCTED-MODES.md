# VP8 reconstructed-mode compression milestone

Reconstruction-based luma and chroma mode selection reduces the fixed photograph
packet **5.2%** and diagram **4.2%** from C54, within the unchanged quality gates.
The diagram remains below its complete-packet ceiling. The photograph remains
**1,084 B (6.13%)** above reference; broader compression parity stays open.

| Frozen opaque513×259 control | C54 → luma screen → current / reference B | Source-PSNR gap dB | Complete managed / reference µs | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: |
| Photograph8 | 19,808 → 18,858 → 18,772 / 17,688 | 0.087712 | 25,716.14 / 9,739.11 | 660,776 |
| Diagram8 | 7,046 → 6,914 → 6,752 / 7,100 | 0.130170 | 11,665.63 / 5,815.49 | 554,595 |

Managed quality78/75, native75 and effort4 stay frozen. Matching is the existing
source-PSNR absolute gap≤0.25dB, jointly with exact alpha and decoder bounds;
equal quality numbers are not a matching criterion. The photograph/diagram
ceilings remain17,688/7,100 B. Lossy pixels may change. Native normal/scalar RGBA
is exact for both current packets; chroma refinement preserves all luma samples
from the luma-only candidate. Inputs and independent outputs remain intact.

The [C38 audit](WARM-VP8-COMPRESSION-AUDIT.md) already reconstructed all six luma
candidates and found useful gains, rejected under the older size/CPU ordering.
This milestone reuses that owned search on the materially different accepted
intra16/Y2/full-frame entropy encoder. Each admitted prediction is quantized and
reconstructed before scoring sample error plus coefficient/mode bit cost. The
adjacent chroma primitive compares the existing four8x8 U/V predictions the same
way. Candidate counts, quantizers, nearest rounding and lambda are unchanged.
No native code, maps or tables are copied; format syntax follows RFC6386.

One block or U/V pair is staged on bounded stack storage within the existing
codec allowance. Performed trials charge work and check cancellation; optional
local admission estimates include the extra search. The same borrowed shared
root governs substantial storage; refusal is terminal, without retry/fallback.
No new full-frame copy, retained cache or mandatory heap buffer is introduced.

**4,603 tests/OS** pass:1,708 internal,1,961 public and934 live FFmpeg, including
two targeted8/16-bit alpha comparisons on129×129 input. That one geometry crosses
the optional-cache admission boundary absent from older129×67 tests. It adds no
benchmark case. Both OSes also pass136 internal,2 public and2 differential scalar
checks, and the same actual private0.2.0 package/all three examples. Two Linux
per-primitive core builds and one final Windows core build; one internal and one
differential test build perOS, no public/reference/harness/native/selector rebuild.
Final producer bytes are frozen throughout qualification and cost measurement.

The luma-only stage has byte/quality evidence without a separate timing or package
round. Qualification is batched on the coherent combined milestone. Two sequential
three-actual short cost screens retain idle-v2 guards and native pre/post brackets
on the dedicated EPYC9V45 VM:one pinned CPU,.NET10.0.8, normal SIMD, tiering off,
nonconcurrent workstation GC. Complete public admission, normalization, encoding,
fresh output growth and independent packet ownership are included equivalently;
preparation, validation, IPC and startup are excluded. Native allocations remain
unmeasured. CPU costs increase and are retained; parity enforcement, independent
timing confirmations and speed-only work are deferred while compression is open.

Only the two matched-quality content VP8 encode rows change. Five Quality100/q0
VP8 encodes retain their previous branch; VP8L, PNG, JPEG, editing, inspection,
fonts and frozen decoder evidence are reused within their original scope. Optional
JPEGCompactColor remains distinct from default encoding. PNG whole-packet size
acceptance remains separate from its historical5% IDAT timing-equivalence gate.

[Numeric evidence](vp8-reconstructed-mode-evidence.json) retains all actuals,
quality checks, identities, allocations and guards. Exact raw archives on both
hosts:

- `c56-qualified-windows.tar.gz`: `e0555df093050a3bb964cb48607261883595e8ff2497cb009245c2797d4b06a4`

- `c56-qualified-linux.tar.gz`: `921168fd279946ca1e4e5e44bc27f31a0c2d7a2876a007f9b4df4d4f75036993`

- `c55-luma-screen.tar.gz`: `4cd97c2b2ba3bee75d4d7c3ee96a7486b7ece30505c0bae0b69db7767d096c43`

Actual private package SHA256: `c93f8f57c7b848f40a1f11ecc6a8d3ede1febc7bdc2c7dd05ccd1405bc593bfe`.
