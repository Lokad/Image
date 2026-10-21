# Smaller subsampled JPEG packets

Default caller-RGB420 encoding produces **55,797 bytes versus 52,093** for FFmpeg, down from
74,040 (24.6%). The remaining 7.1% gap stays open. Source PSNR differs by 0.058120 dB,
inside the unchanged 0.25 dB gate; alpha is exact and decoder error is maximum 3/RMSE
0.5903. These are complete JPEG packets on the frozen 513x259 procedural control at managed
quality 69/native qscale 5. Equal quality numbers are not a matching rule.

Original quantization tables, initial rounding and DC remain unchanged. One bounded reverse
AC pass compares keeping a coefficient with zero, using source-DCT error and the existing
fixed geometric AC prices. Only 4:2:2/4:2:0 use it; fixed/adaptive entropy and
caller/RGBA/async delivery retain the same coefficient decisions. No new frame storage or
heap scratch is added; pricing scans charge work and check cancellation.

The six existing sampling/depth controls below reuse their frozen inputs, native packets and
settings. They qualify quality without expanding the workload matrix. **Quality passes do
not establish size parity.**

| Existing RGBA control | Managed / reference packet B | PSNR gap dB | Size gate |
| --- | ---: | ---: | --- |
| 4:4:4, 8-bit | 158,812 / 133,041 | 0.082440 | Open |
| 4:2:2, 8-bit | 66,050 / 71,168 | 0.130375 | Pass |
| 4:2:0, 8-bit | 55,797 / 52,093 | 0.058120 | Open |
| 4:4:4, 16-bit | 158,827 / 133,043 | 0.090625 | Open |
| 4:2:2, 16-bit | 66,071 / 71,210 | 0.129517 | Pass |
| 4:2:0, 16-bit | 55,834 / 52,117 | 0.057618 | Open |

An earlier global color trial failed 4:4:4 quality by 2.419/2.416 dB and was rejected.
Current 4:4:4 packets exactly restore their frozen producers. Grayscale remains
58,823/58,910 B. Optional **CompactColor** remains 48,701/52,093 B on its separate
procedural RGB420 control; it does not close the default gap or establish broader
compression efficiency.

| Retained complete-operation cost | Managed / reference us | Ratio / upper | Managed allocated B/op |
| --- | ---: | ---: | ---: |
| Caller RGB420, one CPU, three actuals | 17,034.89 / 1,578.57 | 10.791 / 11.190 | 170,539 |

This single short cost observation belongs to the C123 producer before 4:4:4 eligibility was
restricted. The active 4:2:0 transform, decision and pricing code, work, buffers and exact
packet are unchanged; [numeric evidence](jpeg-ac-decision-evidence.json) records that
comparison and both producer identities. It is reused without retiming. Both idle-v2 guards
pass. No independent CPU-parity confirmation is claimed; native allocation is unmeasured.

The operation includes public admission, packed caller input normalization, color
conversion, transform/decisions, Huffman/entropy encoding, fresh MemoryStream growth,
independent complete output and release. Both sides borrow the same resident packed RGB;
source projection, startup, IPC, hashing and validation stay outside timing. FFmpeg uses its
MJPEG backend with one codec/conversion thread.

The accepted restricted producer passes **4,864 Release tests per OS** (including 936 live
FFmpeg comparisons), 46 internal plus 46 public scalar checks per OS, and the **same actual
private 0.2.0 package and all three usage examples** on Windows/Linux. Published 0.1.0 is
unchanged. The historical 320-row JPEG quality report is retained; unchanged
grayscale/4:4:4/quality 100 pixels remain frozen and current entropy modes agree. No
photographic, diagram or general-image compression claim follows.

Raw qualified builds, source, tests and package proofs are mirrored as
`c124-qualified-linux.tar.gz` (SHA256
`79df418179c4e8969764a052bf007dc46e5c1d2fd550262740eac27c5123a666`) and
`c124-qualified-windows.tar.gz`
(`4c89a251785f46a5efbe9cf27ccbcde3c0dce513912da5cf2ece3bd18c9c77d3`). The original cost and
rejected global-scope evidence remain in `c123-qualified-linux.tar.gz`
(`3af23685c92a7a8f08cd0286342afd79b18587c4c99fbd2f02e8c5a3c79c82c7`).
