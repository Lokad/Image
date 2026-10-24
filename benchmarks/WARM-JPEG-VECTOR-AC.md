# Parallel adaptive JPEG AC preparation

Historical producer: its grayscale parity claim predates admitted raw-exact
gray encoding in3f36e3f. The [current control](WARM-JPEG-SPARSE-SCAN.md) has a
6.973× gap; the table below is retained as evidence of the earlier encoder.

The eight existing adaptive JPEG encodes improve **20.8–23.5%** with unchanged
packets, quality and production allocation strategy. **Single-component grayscale
independently passes parity:0.962x mean/0.970x conservative upper** against the
approved SIMD libjpeg-turbo reference. Stop grayscale; seven gaps remain.

| Existing encode | Lokad / reference µs | Ratio / upper | Managed B/op | JPEG B managed / native | Verification |
| --- | ---: | ---: | ---: | ---: | --- |
| RGBA8 /420 /restart33 | 2,019.53 / 1,611.25 | 1.253 / 1.455 | 297,781 | 74,086 / 58,012 | Qualified short gap |
| RGBA8 /420 | 2,022.21 / 1,766.91 | 1.144 / 1.247 | 297,781 | 74,040 / 52,093 | Qualified short gap |
| RGBA16 /420 | 2,173.56 / 1,841.00 | 1.181 / 1.300 | 297,779 | 74,056 / 52,117 | Qualified short gap |
| RGBA16 /422 | 2,633.91 / 2,421.91 | 1.088 / 1.220 | 294,116 | 90,253 / 71,210 | Qualified short gap |
| RGBA8 /422 | 2,579.93 / 2,354.66 | 1.096 / 1.227 | 294,116 | 90,214 / 71,168 | Qualified short gap |
| RGBA8 /444 | 3,894.69 / 3,771.76 | 1.033 / 1.115 | 559,418 | 158,812 / 133,041 | Qualified short gap |
| RGBA16 /444 | 3,980.90 / 3,855.74 | 1.032 / 1.135 | 556,284 | 158,827 / 133,043 | Qualified short gap |
| RGBA8 /gray (SIMD libjpeg-turbo) | 1,350.95 / 1,404.64 | 0.962 / 0.970 | 154,892 | 58,823 / 58,910 | Parity confirmed |

Four safe AVX512F/CD vectors prepare exact integer coefficient categories and
negative-complement amplitudes. Original scalar scan/histogram/event traversal
retains byte order, zero-run effects and range-error timing, including int.MinValue.
Only64unused words in the already-admitted private event array are used;128free
slots prove that scratch cannot overlap up to63appended AC events. Short tails and
other hardware retain the entire original body. No new field, table, heap array,
workspace reservation, work charge, sampling, transform or writer change is added.

Both sides time complete operations: source normalization/alpha flattening,
color sampling, transforms, Huffman planning and entropy, fresh output growth,
packet acquisition and disposal. Original513×259RGBA8/16 rasters only, background
0x204060 and frozen settings: managed69 except422quality65; FFmpeg qscale5,
restart33 where selected. Gray uses the explicitly approved **SIMD-enabled
standalone libjpeg-turbo3.1.4.1**, quality69/ISLOW/owned quantizers; this exception
is benchmark tooling only. Production remains safe managed dependency-free.NET10.
All frozen packet hashes/sizes, decoded pixels, alpha/source/owners and≤0.25dB
quality comparisons pass. No compression-efficiency parity is claimed.

OneCPU, normalSIMD, tiering0, nonconcurrent workstationGC; reused native contexts
process every sample. Startup, IPC, I/O, preparation/hashing/validation are excluded.
Managed B/op includes varying harness amortization; native allocation is unmeasured.
Eight3actual screens and one independent12actual gray confirmation retain all36
actuals and both5s idle-v2 guards. Upper is the larger selected/all-actual bound
against the conservative native lower bracket. No long repeats for means>1.

All **3,722 Release cases/OS** pass (1,388internal,1,436public,898liveFFmpeg),
including23internal+63public focused checks/OS. Two existing independent scalar
oracle facts now cover2,332coefficient states at short/prepared buffer boundaries,
including exact failure prefixes and histograms. Original oracle body is unchanged.
Unchanged scalar paths reuse prior20adaptive checks/OS. The **same actual Release
package** passes its consumer and allthree examples on Windows/Linux; measured
producer/DLL/PDB bytes remain qualified. Published0.1.0 artifacts/tag stay unchanged.

Exactly eight timings replace selected evidence; other199comparisons/all61prior
passes retain their executed paths. No workload/image/settings grid is added.
[Numeric evidence](warm-jpeg-vector-ac-evidence.json) retains source reconciliation,
references, all actuals, guards, qualification and raw hashes. Full207case parity
remains open:62passes,144gaps,oneconfidence case;127independent and80qualified
short selected observations. No SIMD lane, layout or inlining tuning grid follows.
