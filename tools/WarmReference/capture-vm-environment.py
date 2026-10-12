import hashlib
import json
import os
import pathlib
import platform
import shutil
import subprocess
import sys
import time

root = pathlib.Path(sys.argv[1]).resolve()
manifest = json.loads((root/'tmp/benchmark-source.json').read_text())
canonical = b''
for item in manifest['files']:
    name = pathlib.PurePosixPath(item['path'])
    assert not name.is_absolute() and '..' not in name.parts
    assert not any(part in ('external', 'tmp', 'artifacts', 'bin', 'obj', 'PLAN.md') for part in name.parts)
    path = root.joinpath(*name.parts)
    assert path.is_file() and not path.is_symlink() and path.stat().st_size == item['bytes']
    assert hashlib.sha256(path.read_bytes()).hexdigest() == item['sha256'], item['path']
    canonical += (item['path'] + '\0' + str(item['bytes']) + '\0' + item['sha256'] + '\n').encode()
assert hashlib.sha256(canonical).hexdigest() == manifest['sourceSha256']

def command(arguments):
    return subprocess.run(arguments, capture_output=True, check=True, text=True, timeout=30).stdout.strip()

def binary(name, arguments):
    path = pathlib.Path(shutil.which(name)).resolve()
    return dict(sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
                identity=command([str(path), *arguments]).replace(str(root), '{repository}'))

def cpu():
    return {parts[0]: list(map(int, parts[1:9]))
            for line in pathlib.Path('/proc/stat').read_text().splitlines()
            if (parts := line.split()) and parts[0] in ('cpu', 'cpu0')}

def jobs():
    blockers = []
    names = {'dotnet', 'ffmpeg', 'ffprobe', 'ffmpeg-png-warm', 'perf',
             'pwsh', 'MSBuild', 'VBCSCompiler', 'cc', 'gcc', 'clang', 'make', 'ninja'}
    for process in pathlib.Path('/proc').iterdir():
        if not process.name.isdecimal():
            continue
        try:
            stat = (process/'stat').read_text()
            state = stat[stat.rfind(')')+2:].split()[0]
            name = (process/'comm').read_text().strip()
            if state != 'Z' and (name in names or name.startswith(('ffmpeg', 'MSBuild', 'VBCSCompiler'))):
                blockers.append(dict(pid=int(process.name), name=name, state=state))
        except (FileNotFoundError, ProcessLookupError, PermissionError):
            continue
    return sorted(blockers, key=lambda job: job['pid'])

jobs_before = jobs()
before = cpu()
start = time.monotonic()
time.sleep(5)
after = cpu()
duration = time.monotonic() - start
jobs_after = jobs()
samples = {}
for name in before:
    delta = [b-a for a,b in zip(before[name], after[name])]
    total = sum(delta)
    assert total > 0 and all(value >= 0 for value in delta)
    samples[name] = dict(cpuBusyFraction=(total-delta[3]-delta[4])/total,
                         ioWaitFraction=delta[4]/total, stealFraction=delta[7]/total)
busy, io_wait, steal = (samples['cpu'][key] for key in ('cpuBusyFraction','ioWaitFraction','stealFraction'))
load = list(os.getloadavg())
quiet = not jobs_before and not jobs_after and all(
    sample['cpuBusyFraction'] <= .05 and sample['ioWaitFraction'] <= .01
    and sample['stealFraction'] <= .01 for sample in samples.values())
memory = dict(line.split(':', 1) for line in pathlib.Path('/proc/meminfo').read_text().splitlines())
cpu_info = dict(line.split(':', 1) for line in pathlib.Path('/proc/cpuinfo').read_text().split('\n\n')[0].splitlines() if ':' in line)
cpu_info = {key.strip(): value.strip() for key,value in cpu_info.items()}
disk = shutil.disk_usage(root)
configuration = root/'external/linux-x64/oracle-identity.json'
record = dict(schemaVersion=1, project='Lokad.Image', purpose='Preparatory environment/quietness evidence; no statistical timing result',
    capturedUtc=time.strftime('%Y-%m-%dT%H:%M:%SZ', time.gmtime()),
    source=dict(sourceSha256=manifest['sourceSha256'], files=len(manifest['files']),
                sourceRevision=manifest['sourceRevision'], verifiedAllFileBytes=True),
    machine=dict(system=platform.system(), architecture=platform.machine(), kernel=platform.release(),
                 distribution=platform.freedesktop_os_release(), logicalProcessors=os.cpu_count(),
                 cpuModel=cpu_info['model name'], cpuFlags=cpu_info['flags'].split(),
                 memoryTotal=memory['MemTotal'].strip(), swapTotal=memory['SwapTotal'].strip(),
                 diskTotalBytes=disk.total, diskFreeBytes=disk.free),
    toolchain=dict(dotnet=binary('dotnet', ['--version']), runtimes=command(['dotnet', '--list-runtimes']).replace(str(root), '{repository}'),
                   powershell=binary('pwsh', ['-NoLogo', '-NoProfile', '-Command', '$PSVersionTable.PSVersion.ToString()']),
                   ffmpeg=binary('ffmpeg', ['-version']), ffprobe=binary('ffprobe', ['-version']),
                   childMemoryCounter=binary('/usr/bin/time', ['--version']),
                   oracleIdentitySha256=hashlib.sha256(configuration.read_bytes()).hexdigest()),
    quietness=dict(protocol='exclusive-vm-idle-v2', sampleSeconds=duration, cpuSamples=samples,
                   overlappingJobsBefore=jobs_before, overlappingJobsAfter=jobs_after, loadAverageDiagnosticOnly=True, cpuBusyFraction=busy, ioWaitFraction=io_wait, stealFraction=steal,
                   loadAverage=load, passed=quiet, thresholds=dict(maxCpuBusyFraction=0.05, maxIoWaitFraction=0.01,
                        maxStealFraction=0.01),
                   scope='Five-second aggregate and CPU0 idle samples plus live build/codec/profiler process checks. Load averages are diagnostic only: dedicated VM use is exclusive and historical own work requires no cooldown. Recheck immediately before and after every statistical run; does not prove future hypervisor isolation.'),
    runtimeSettings={key: os.environ.get(key) for key in ('DOTNET_TieredCompilation','DOTNET_TieredPGO','DOTNET_ReadyToRun',
                    'DOTNET_GCServer','COMPlus_TieredCompilation','COMPlus_TieredPGO','COMPlus_ReadyToRun','COMPlus_gcServer')})
print(json.dumps(record, indent=2))
