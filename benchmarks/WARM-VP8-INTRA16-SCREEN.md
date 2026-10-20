# VP8 intra16/Y2 candidate screen

This candidate is **rejected**. Mixed 4×4/16×16 luma prediction and grouped DC
coding reduce the fixed photograph/diagram packets by 1.1%/7.8%, but miss both
size ceilings and slow complete encoding. The qualified encoder is restored;
these results do not replace its benchmark rows or qualify a new encoding path.

| Fixed opaque RGBA8 control | Candidate / reference bytes | Source PSNR gap dB | Complete managed / reference µs | Ratio / upper | Managed allocated B/op |
| --- | ---: | ---: | ---: | ---: | ---: |
| Photograph, managed78/native75 | 21,202 / 17,688 | 0.021969 | 12,634.47 / 9,699.23 | 1.303 / 1.391 | 601,368 |
| Text/diagram, managed75/native75 | 7,642 / 7,100 | 0.214030 | 7,241.71 / 5,830.16 | 1.242 / 1.417 | 487,843 |

Both controls remain the frozen 513×259 inputs at effort4. Required complete
packet ceilings are **17,688/7,100 bytes**, jointly with the existing ≤0.25dB
source-quality difference, exact alpha, decoder agreement, resource contracts
and independent warm parity. Equal numerical quality is not quality matching.
Native/public decoder differences are at most one sample; source preservation,
deterministic independent outputs and alpha checks pass. Native allocations
remain unmeasured. Current production packets remain **21,432/8,288 bytes**.

The original prototype stages one macroblock and uses an admitted high-byte
extension to the existing 64-macroblock coefficient cache. It selects intra16
for 90/436 of 561 photograph/diagram macroblocks. Header partitions shrink to
3,140/949 bytes, but coefficient savings are insufficient. A separately authored
80-byte intra16 frame and independent Walsh matrix/range proof establish format
feasibility; they do not qualify the production integration or its resource paths.

Two sequential three-actual screens ran on the dedicated EPYC9V45 VM, one pinned
CPU, normal SIMD, .NET10.0.8, tiering off and nonconcurrent workstation GC. Each
retains idle-v2 guards and native pre/post brackets. The conservative upper uses
the larger selected/all-actual interval. The reused harness includes public
admission, normalization, encoding, fresh output growth and independent output
ownership; input preparation, validation, IPC and startup are outside both timers.
No independent repeat follows these demonstrated gaps. One Linux core build,
zero Windows/harness/native/selector builds, no full suite or new package proof.
Qualified C37 source, incremental outputs, runtimes and actual package are reused.

[Numeric evidence](vp8-intra16-screen-evidence.json) retains all actuals,
allocations, guards, quality results and identities. Raw archives on both hosts:
`c39-first-candidate.tar.gz`
SHA256 `1d8157d8bdc9aa5ec36bfece6fc8d90a33a288bbb808ad094fcda8cbbd1e5086`;
`c39-intra16-screen.tar.gz`
`9083fd276993a06bdb50254199e8b465f2e4e0dcc4d4b4c7eb2a70a80f9571fc`;
`c39-intra16-format-proof.tar.gz`
`d858bd411c83833b9391a35597be4c30a76dc09d028ae2805c3069897e1c6614`.
No broader image, quality, font or replacement-readiness claim follows.
