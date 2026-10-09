import json
from pathlib import Path
import sys

snapshot_path = Path(sys.argv[1])
output_path = Path(sys.argv[2])
data = json.loads(snapshot_path.read_text())
cases = {row['fullName']: row for row in data['cases']}
large_jpeg = [row for row in data['cases'] if '.Managed(Workload: Jpeg' in row['fullName'] and '513x259' in row['fullName']]
assert len(large_jpeg) == 6
gains = [(1 - row['currentToBaselineRatio']) * 100 for row in large_jpeg]
lines = [
    '# Representative invocation results — 2026-10-09', '',
    f'The original JPEG transform storage/loop change reduces mean invocation latency by {min(gains):.1f}–{max(gains):.1f}% across the six measured 513x259 JPEG cases. All 87 sampled owned bitstream hashes and all 48 retained files are unchanged. The 129x67 cases have a larger proportion of process/runtime overhead; these measurements do not establish isolated library throughput.', '',
    'Results come from four sequential Release BenchmarkDotNet 0.15.6 SerialShort jobs on a quiet dedicated Linux VM: two original baselines and two current runs. Each case has one launch, three warmups and three measured iterations per run. The table averages the two current run means. The [original numeric snapshot](results/representative-2026-10-09.json) retains all four runs\' samples, workload names, source and binary identities, quality results, exclusions and quietness observations.', '',
    'The VM presents four AMD EPYC 9V45 vCPUs, about 15.6 GiB RAM and no swap, running Ubuntu 24.04.4 x64/kernel 6.17.0-1022-azure, SDK 10.0.204/runtime 10.0.8 and PowerShell 7.6.6. FFmpeg n8.1 is built with the codec/filter dependencies and flags pinned by `tools/external-sources.json` and `tools/setup-linux.sh`.', '',
    'Each subject launches a fresh process, reads the same prepared input and writes its complete output. Startup/JIT, format conversion, codec/filter work and file I/O are timed; quality calibration, fixture generation and correctness checks are outside timing. FFmpeg uses normal CPU dispatch and one codec/filter thread. The raw nearest and PNG pipeline baselines include separate RGB/alpha zscale point contexts and packing. PNG uses native predictor none/compression level 6 and managed defaults, without compression-effort matching.', '',
    'Calibration uses six original deterministic inputs, twelve profiles and at most eight owned qualities per profile: five fixed settings and at most three refinements. Eighty-seven settings were evaluated against 24 native anchors. Eleven pairs meet the fixed 0.25 dB source-error band; thirteen excluded anchors remain in the snapshot. This matches sampled RGB error, not perceptual quality or encoder effort. JPEG flattens RGBA16 over RGB `0x204060` with one rounding; VP8 keeps exact normalized alpha and uses premultiplied source error. Decoder agreement remains maximum RGB error 4/RMSE 1.5, including hidden RGB. All PNG/raw outputs are exact.', '',
    'The complete routine workflow uses eight source images: those six calibration inputs plus two reused PNG/editing inputs. The accepted timed selection uses five of those sources. There are sixteen workload pairs and 32 benchmark cases per run. The table includes every accepted pair and all five PNG/editing workloads.', '',
    '| Workload | Lokad.Image ms | FFmpeg ms | Invocation ratio | Output bytes managed/native | Child peak MiB managed/native |',
    '| --- | ---: | ---: | ---: | ---: | ---: |'
]
for workload in data['workloads']:
    parameters = {p['name']:p['value'] for p in workload['parameters']}
    if workload['type'] == 'MatchedQualityEncodeInvocation':
        label = parameters['Workload']
    elif workload['type'] == 'PngCropResizePipeline':
        label = f"PNG crop/nearest/encode 129x67 RGBA{parameters['Depth']}"
    elif workload['type'] == 'GeometryRawFile':
        label = f"{parameters['Operation']} 129x67 RGBA16 raw"
    else:
        label = 'Nearest upscale 129x67 → 257x135 RGBA16 raw'
    managed, native = cases[workload['managedFullName']], cases[workload['nativeFullName']]
    outputs = [{key.lower(): value for key, value in workload['outputs'][subject].items()} for subject in ('managed','ffmpeg')]
    sizes = [row.get('encodedbytes', row.get('outputbytes')) for row in outputs]
    assert all(isinstance(size, int) and size > 0 for size in sizes), workload
    memory = [max(workload['childPeakResidentBytes'][subject]) / 1048576 for subject in ('managed','native')]
    ratio = 'variable control' if any(issue['name'] == workload['nativeFullName'] for issue in data['repeatabilityIssues']) else f"{managed['currentMeanMilliseconds']/native['currentMeanMilliseconds']:.2f}x"
    lines.append(f"| {label} | {managed['currentMeanMilliseconds']:.2f} | {native['currentMeanMilliseconds']:.2f} | {ratio} | {sizes[0]:,}/{sizes[1]:,} | {memory[0]:.1f}/{memory[1]:.1f} |")
lines += ['',
    'Child peak RSS is the larger of four separate untimed kernel measurements of each codec/tool process across both current runs and both subject setups. It includes runtime memory and is distinct from BenchmarkDotNet allocations in the parent process. Raw output bytes are uncompressed; PNG/JPEG/VP8 rows report compressed bytes. JPEG/VP8 settings appear as native/owned suffixes in the full workload name, without implying numeric equivalence.', '',
    'Initial baseline repeat means differ by at most 6.6%. All 32 current cases stay within the provisional regression guard of 20% above the slower repeated baseline mean, with unchanged bitstreams/quality. Thirty-one current repeat checks stay within 10%. One small native JPEG 4:2:2 control varies from 2.34 to 2.96 ms (26.4%); a targeted same-image Short run returns to 2.48 ms, within 10% of the baseline and first current mean. The original variation and diagnostic samples remain in the snapshot; this row has no comparative ratio claim. Native cohort means otherwise remain within 10% of their baselines. Immediate post-run rolling load can include the completed job; original failed observations are retained, and measurement resumes only after the unchanged full quietness thresholds pass. These short runs provide a bounded initial regression baseline. Larger images, varied-content batches, other operations and hardware require separate measurements.', '',
    'To reproduce the current cohort on a quiet dedicated Linux VM, provision the pinned tools as described in [README](../README.md), then run sequentially:', '',
    '```powershell',
    'dotnet restore Lokad.Image.slnx --locked-mode --tl:off',
    './test.ps1 -Differential -NoRestore -SerialBuild',
    './tools/PackageSmokeTest/verify-package.ps1 -NoBuild',
    'dotnet benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll --calibrate-encoder-quality',
    "$env:LOKAD_IMAGE_QUALITY_CALIBRATION = 'artifacts/encoder-quality/<completed-run-id>/calibration.json'",
    './bench.ps1 -Smoke -NoBuild',
    './bench.ps1 -DedicatedVm -NoBuild',
    '# Wait for the VM to be quiet again before the second statistical run.',
    './bench.ps1 -DedicatedVm -NoBuild',
    '```', '',
    'Run IDs are generated by the launcher. Each ignored `artifacts/benchmark-runs/<id>` retains the receipt, setup evidence references and full BenchmarkDotNet JSON reports. Match full workload names; the shortened display parameter column is insufficient. The numeric snapshot retains six measured values per cohort/case so its means, ratios and regression limits can be recalculated independently. Source hashes cover every owned source/fixture byte at measurement time; report and release-document additions follow that measurement snapshot. The [owned baseline patch](results/jpeg-transform-baseline.patch) restores the original rectangular basis storage and inner-loop vertical loads without changing mathematical constants or arithmetic order. Apply it in a separate checkout to reproduce the earlier implementation. No speed claims come from local or Dry timing output.', '',
    'Regenerate this table from the checked-in original numeric snapshot:', '',
    '```bash',
    'python3 benchmarks/results/render-representative.py benchmarks/results/representative-2026-10-09.json benchmarks/RESULTS.md',
    '```', ''
]
output_path.write_text('\n'.join(lines), encoding='utf-8')
print(json.dumps(dict(complete=True, rows=len(data['workloads']), minimumLargeJpegGainPercent=min(gains), maximumLargeJpegGainPercent=max(gains))))
