"""Original RGBA16 self-composite: complete managed operation versus ImageSharp snapshot + draw."""
from pathlib import Path
import argparse, hashlib, json, math, os, select, signal, statistics, subprocess

p = argparse.ArgumentParser()
p.add_argument('--worker', required=True)
p.add_argument('--core-sha256', required=True)
p.add_argument('--input', required=True)
p.add_argument('--prefix', required=True)
p.add_argument('--mode', choices=['short', 'confirm'], required=True)
a = p.parse_args()
prefix = Path(a.prefix)
n = 3 if a.mode == 'short' else 12
critical = {3: 31.59905444857171, 12: 4.436979337146836}[n]
operations = 128
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()
env = {**os.environ, 'DOTNET_PROCESSOR_COUNT': '1', 'DOTNET_TieredCompilation': '0',
       'DOTNET_gcServer': '0', 'DOTNET_gcConcurrent': '0'}
raw = dict(mode=a.mode, actuals=n, operationsPerActual=operations, critical=critical,
           warmups={}, before=[], pairs=[], after=[], ready={}, checks={}, stopped={})
workers, errors = {}, {}
wire = prefix.with_name(prefix.name + '-wire.jsonl').open('x')


def save():
    wire.flush()
    os.fsync(wire.fileno())
    path = prefix.with_name(prefix.name + '-raw.json')
    temporary = path.with_suffix('.new')
    temporary.write_text(json.dumps(raw, indent=2) + '\n')
    temporary.replace(path)


def guard(side):
    os.sync()
    r = subprocess.run(['python3', 'tools/WarmReference/capture-vm-environment.py', '.'], capture_output=True)
    prefix.with_name(prefix.name + '-' + side + '.json').write_bytes(r.stdout)
    assert r.returncode == 0 and json.loads(r.stdout)['quietness']['passed'], r.stderr.decode()


def read(subject):
    worker = workers[subject]
    assert select.select([worker.stdout], [], [], 60)[0], (subject, 'observation timeout', worker.poll())
    line = worker.stdout.readline()
    assert line, (subject, worker.poll())
    value = json.loads(line)
    wire.write(json.dumps(dict(subject=subject, value=value)) + '\n')
    return value


def request(subject, command):
    workers[subject].stdin.write(command + '\n')
    workers[subject].stdin.flush()
    return read(subject)


def sample(subject):
    value = request(subject, 'sample ' + str(operations))
    assert value['state'] == 'sample' and value['operations'] == operations
    assert value['elapsedNanoseconds'] > 0 and value['cpu'] == 0
    assert value['resetExcluded'] and value['clocksPerOperation'] == 2
    if subject == 'managed':
        assert value['allocatedBytes'] == 0
    return value


guard('before')
try:
    for subject, mode in [('managed', 'managed'), ('reference', 'snapshot')]:
        errors[subject] = prefix.with_name(prefix.name + '-' + subject + '-stderr.log').open('xb')
        command = ['taskset', '-c', '0', 'dotnet', a.worker, mode, a.input,
                   str(prefix) + '-' + subject + '.rgba', a.core_sha256]
        workers[subject] = subprocess.Popen(command, env=env, stdin=subprocess.PIPE,
            stdout=subprocess.PIPE, stderr=errors[subject], text=True, bufsize=1, start_new_session=True)
        ready = read(subject)
        assert ready['state'] == 'ready' and ready['coreHash'] == a.core_sha256
        assert ready['quality']['Passed'] and ready['sourceAliasesCanvas'] and ready['borrowedCanvas']
        assert ready['cpu'] == 0 and ready['depth'] == 16 and ready['resetExcludedBeforeEveryOperation']
        assert ready['snapshotCreationAndDisposalTimed'] == (subject == 'reference')
        raw['ready'][subject] = ready
    assert raw['ready']['managed']['inputHash'] == raw['ready']['reference']['inputHash'] == sha(a.input)
    assert raw['ready']['managed']['referenceHash'] == raw['ready']['reference']['referenceHash']
    for subject in workers:
        raw['warmups'][subject] = sample(subject)
    save()
    for _ in range(n):
        raw['before'].append(sample('reference'))
    save()
    for index in range(n):
        pair = dict(index=index)
        for subject in (['managed', 'reference'] if index % 2 == 0 else ['reference', 'managed']):
            pair[subject] = sample(subject)
        raw['pairs'].append(pair)
        save()
    for _ in range(n):
        raw['after'].append(sample('reference'))
    for subject in workers:
        raw['checks'][subject] = request(subject, 'check')
        assert raw['checks'][subject]['state'] == 'checked' and raw['checks'][subject]['quality']['Passed']
        workers[subject].stdin.write('quit\n')
        workers[subject].stdin.flush()
        raw['stopped'][subject] = read(subject)
        assert raw['stopped'][subject]['allCanvasOwnersDisposed'] and raw['stopped'][subject]['inputUnchanged']
        assert workers[subject].wait(timeout=30) == 0
    save()
finally:
    for subject, worker in workers.items():
        if worker.poll() is None:
            os.killpg(worker.pid, signal.SIGTERM)
            worker.wait(timeout=10)
        errors[subject].close()
    wire.close()
guard('after')


def stats(values):
    mean = statistics.mean(values)
    error = statistics.stdev(values) / math.sqrt(len(values))
    return dict(mean=mean, lower=mean-critical*error, upper=mean+critical*error, values=values)


def ns(value):
    return value['elapsedNanoseconds'] / value['operations']


managed = stats([ns(v['managed']) for v in raw['pairs']])
reference = dict(before=stats([ns(v) for v in raw['before']]),
                 paired=stats([ns(v['reference']) for v in raw['pairs']]),
                 after=stats([ns(v) for v in raw['after']]))
paired = stats([math.log(ns(v['managed']) / ns(v['reference'])) for v in raw['pairs']])
lower = min(managed['lower'] / max(v['upper'] for v in reference.values()), math.exp(paired['lower']))
reference_lower = min(v['lower'] for v in reference.values())
upper = max(managed['upper'] / reference_lower if reference_lower > 0 else math.inf, math.exp(paired['upper']))
reference_mean = min(v['mean'] for v in reference.values())
result = dict(project='Lokad.Image', originalRequiredControl='Composite/Rgba16Overlapping',
    productionChanged=False, newRequiredPaths=0, librarySha256=a.core_sha256,
    reference='ImageSharp 3.1.12 Rgba64 Clone + DrawImage normal SrcOver opacity1 + Dispose;not FFmpeg',
    referenceSha256=raw['ready']['reference']['referenceHash'], mode=a.mode, actuals=n,
    operationsPerActual=operations, allActualsRetained=True, managedUs=managed['mean']/1000,
    nativeUs=reference_mean/1000, ratio=managed['mean']/reference_mean, lower=lower, upper=upper,
    status='pass' if upper <= 1 else 'gap' if lower > 1 else 'uncertain', allocatedBytes=0,
    referenceAllocatedBytes=statistics.mean([v['reference']['allocatedBytes']/operations for v in raw['pairs']]),
    managed=managed, referenceBrackets=reference, pairedLogStatistics=paired,
    quietGuardsPassed=True, exactManagedRationalPixels=True, fullWordSamples=True,
    inputSha256=sha(a.input), outputSha256=raw['ready']['managed']['outputHash'],
    referenceOutputSha256=raw['ready']['reference']['outputHash'], nativeQuality=raw['ready']['reference']['quality'],
    clocksPerOperation=2, resetExcludedBeforeEveryOperation=True, snapshotCreationAndDisposalTimed=True,
    boundary='Original513x259RGBA16self-composite shifted(1,1),effective512x258. Complete public managed operation;ImageSharp source snapshot+publicSrcOver+snapshot disposal are timed. Both mutate borrowed canvas,restored from the same resident seed before EVERY operation outside both clocks. OneCPU,no concurrent operations. Setup,JIT,IPC,hashes,BigIntegeroracle,verification and reset excluded equally. Outputs are full16-bit samples;no metadata/quality adjustment.',
    confidence='All actuals;two-sided99.9percentStudentt,pairedlogratio and managed mean against fastest of three reference bracket lower bounds;no sample filtering or forcedGC.')
prefix.with_suffix('.json').write_text(json.dumps(result, indent=2) + '\n')
print(json.dumps({k: result[k] for k in ['mode', 'managedUs', 'nativeUs', 'ratio', 'lower', 'upper', 'status', 'allocatedBytes', 'referenceAllocatedBytes']}))
