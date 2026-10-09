# Representative invocation results — 2026-10-09

The original JPEG transform storage/loop change reduces mean invocation latency by 10.4–16.7% across the six measured 513x259 JPEG cases. All 87 sampled owned bitstream hashes and all 48 retained files are unchanged. The 129x67 cases have a larger proportion of process/runtime overhead; these measurements do not establish isolated library throughput.

Results come from four sequential Release BenchmarkDotNet 0.15.6 SerialShort jobs on a quiet dedicated Linux VM: two original baselines and two current runs. Each case has one launch, three warmups and three measured iterations per run. The table averages the two current run means. The [original numeric snapshot](results/representative-2026-10-09.json) retains all four runs' samples, workload names, source and binary identities, quality results, exclusions and quietness observations.

The VM presents four AMD EPYC 9V45 vCPUs, about 15.6 GiB RAM and no swap, running Ubuntu 24.04.4 x64/kernel 6.17.0-1022-azure, SDK 10.0.204/runtime 10.0.8 and PowerShell 7.6.6. FFmpeg n8.1 is built with the codec/filter dependencies and flags pinned by `tools/external-sources.json` and `tools/setup-linux.sh`.

Each subject launches a fresh process, reads the same prepared input and writes its complete output. Startup/JIT, format conversion, codec/filter work and file I/O are timed; quality calibration, fixture generation and correctness checks are outside timing. FFmpeg uses normal CPU dispatch and one codec/filter thread. The raw nearest and PNG pipeline baselines include separate RGB/alpha zscale point contexts and packing. PNG uses native predictor none/compression level 6 and managed defaults, without compression-effort matching.

Calibration uses six original deterministic inputs, twelve profiles and at most eight owned qualities per profile: five fixed settings and at most three refinements. Eighty-seven settings were evaluated against 24 native anchors. Eleven pairs meet the fixed 0.25 dB source-error band; thirteen excluded anchors remain in the snapshot. This matches sampled RGB error, not perceptual quality or encoder effort. JPEG flattens RGBA16 over RGB `0x204060` with one rounding; VP8 keeps exact normalized alpha and uses premultiplied source error. Decoder agreement remains maximum RGB error 4/RMSE 1.5, including hidden RGB. All PNG/raw outputs are exact.

The complete routine workflow uses eight source images: those six calibration inputs plus two reused PNG/editing inputs. The accepted timed selection uses five of those sources. There are sixteen workload pairs and 32 benchmark cases per run. The table includes every accepted pair and all five PNG/editing workloads.

| Workload | Lokad.Image ms | FFmpeg ms | Invocation ratio | Output bytes managed/native | Child peak MiB managed/native |
| --- | ---: | ---: | ---: | ---: | ---: |
| Jpeg420-SeededNoise-513x259-d16-BinaryEdgesAlpha-bg204060-native16-owned20 | 118.55 | 30.90 | 3.84x | 56,609/20,895 | 37.8/13.3 |
| Jpeg420-SeededNoise-513x259-d16-BinaryEdgesAlpha-bg204060-native5-owned50 | 121.95 | 31.54 | 3.87x | 92,028/45,567 | 39.0/13.7 |
| Jpeg422-Asymmetric-129x67-d8-native16-owned1 | 36.86 | 2.33 | 15.85x | 1,638/3,127 | 35.0/7.8 |
| Jpeg422-Asymmetric-129x67-d8-native5-owned13 | 37.75 | 2.65 | variable control | 5,189/7,292 | 35.1/7.9 |
| Jpeg422-SeededNoise-513x259-d16-BinaryEdgesAlpha-bg204060-native16-owned20 | 123.70 | 31.28 | 3.95x | 73,660/28,853 | 38.4/13.4 |
| Jpeg422-SeededNoise-513x259-d16-BinaryEdgesAlpha-bg204060-native5-owned39 | 126.35 | 31.90 | 3.96x | 110,675/63,951 | 39.5/13.7 |
| Jpeg444-SeededNoise-513x259-d16-BinaryEdgesAlpha-bg204060-native16-owned24 | 138.83 | 32.23 | 4.31x | 134,465/50,527 | 40.3/13.8 |
| Jpeg444-SeededNoise-513x259-d16-BinaryEdgesAlpha-bg204060-native5-owned68 | 145.74 | 33.11 | 4.40x | 221,105/97,890 | 42.9/14.2 |
| Vp8-SeededNoise-513x259-d16-BinaryEdgesAlpha-native50-owned75 | 117.38 | 22.18 | 5.29x | 257,138/37,964 | 38.3/13.3 |
| Vp8-SeededNoise-513x259-d16-BinaryEdgesAlpha-native85-owned100 | 160.77 | 24.12 | 6.67x | 520,722/54,152 | 38.9/13.1 |
| Vp8-SoftGradient-129x67-d8-native50-owned83 | 40.45 | 2.58 | 15.66x | 544/286 | 35.3/7.8 |
| PNG crop/nearest/encode 129x67 RGBA8 | 50.18 | 2.77 | 18.08x | 4,639/4,059 | 39.8/7.9 |
| PNG crop/nearest/encode 129x67 RGBA16 | 59.99 | 3.37 | 17.81x | 13,285/13,342 | 40.1/8.8 |
| FlipHorizontal 129x67 RGBA16 raw | 27.68 | 2.19 | 12.65x | 69,144/69,144 | 33.9/6.7 |
| Clockwise90 129x67 RGBA16 raw | 28.26 | 2.33 | 12.13x | 69,144/69,144 | 33.9/6.8 |
| Nearest upscale 129x67 → 257x135 RGBA16 raw | 30.01 | 6.54 | 4.59x | 277,560/277,560 | 34.4/9.7 |

Child peak RSS is the larger of four separate untimed kernel measurements of each codec/tool process across both current runs and both subject setups. It includes runtime memory and is distinct from BenchmarkDotNet allocations in the parent process. Raw output bytes are uncompressed; PNG/JPEG/VP8 rows report compressed bytes. JPEG/VP8 settings appear as native/owned suffixes in the full workload name, without implying numeric equivalence.

Initial baseline repeat means differ by at most 6.6%. All 32 current cases stay within the provisional regression guard of 20% above the slower repeated baseline mean, with unchanged bitstreams/quality. Thirty-one current repeat checks stay within 10%. One small native JPEG 4:2:2 control varies from 2.34 to 2.96 ms (26.4%); a targeted same-image Short run returns to 2.48 ms, within 10% of the baseline and first current mean. The original variation and diagnostic samples remain in the snapshot; this row has no comparative ratio claim. Native cohort means otherwise remain within 10% of their baselines. Immediate post-run rolling load can include the completed job; original failed observations are retained, and measurement resumes only after the unchanged full quietness thresholds pass. These short runs provide a bounded initial regression baseline. Larger images, varied-content batches, other operations and hardware require separate measurements.

To reproduce the current cohort on a quiet dedicated Linux VM, provision the pinned tools as described in [README](../README.md), then run sequentially:

```powershell
dotnet restore Lokad.Image.slnx --locked-mode --tl:off
./test.ps1 -Differential -NoRestore -SerialBuild
./tools/PackageSmokeTest/verify-package.ps1 -NoBuild
dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --calibrate-encoder-quality
$env:LOKAD_IMAGE_QUALITY_CALIBRATION = 'artifacts/encoder-quality/<completed-run-id>/calibration.json'
./bench.ps1 -Smoke -NoBuild
./bench.ps1 -DedicatedVm -NoBuild
# Wait for the VM to be quiet again before the second statistical run.
./bench.ps1 -DedicatedVm -NoBuild
```

Run IDs are generated by the launcher. Each ignored `artifacts/benchmark-runs/<id>` retains the receipt, setup evidence references and full BenchmarkDotNet JSON reports. Match full workload names; the shortened display parameter column is insufficient. The numeric snapshot retains six measured values per cohort/case so its means, ratios and regression limits can be recalculated independently. Source hashes cover every owned source/fixture byte at measurement time; report and release-document additions follow that measurement snapshot. The [owned baseline patch](results/jpeg-transform-baseline.patch) restores the original rectangular basis storage and inner-loop vertical loads without changing mathematical constants or arithmetic order. Apply it in a separate checkout to reproduce the earlier implementation. No speed claims come from local or Dry timing output.

Regenerate this table from the checked-in original numeric snapshot:

```bash
python3 benchmarks/results/render-representative.py benchmarks/results/representative-2026-10-09.json benchmarks/RESULTS.md
```
