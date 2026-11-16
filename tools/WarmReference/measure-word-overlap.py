"""Measure the previously named 257x129 RGBA16 self-overlap boundary on the quiet VM."""
from pathlib import Path
import argparse, hashlib, json, math, os, select, signal, statistics, subprocess

p = argparse.ArgumentParser()
p.add_argument('--managed', required=True)
p.add_argument('--native', required=True)
p.add_argument('--core-sha256', required=True)
p.add_argument('--input', required=True)
p.add_argument('--prefix', required=True)
p.add_argument('--mode', choices=['short', 'confirm'], required=True)
a = p.parse_args()
prefix = Path(a.prefix)
n = 3 if a.mode == 'short' else 12
critical = {3: 31.59905444857171, 12: 4.436979337146836}[n]  # Two-sided 99.9% Student t.
operations = 16384
sha = lambda path: hashlib.sha256(Path(path).read_bytes()).hexdigest()
env = {**os.environ, 'DOTNET_PROCESSOR_COUNT': '1', 'DOTNET_TieredCompilation': '0',
       'DOTNET_gcServer': '0', 'DOTNET_gcConcurrent': '0'}
workers, errors, ready, commands = {}, {}, {}, {}
raw = dict(mode=a.mode, actuals=n, operationsPerActual=operations, critical=critical,
           warmups={}, before=[], pairs=[], after=[], checks={}, stopped={})
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
    q = subprocess.run(['python3', 'tools/WarmReference/capture-vm-environment.py', '.'], capture_output=True)
    prefix.with_name(prefix.name + '-' + side + '.json').write_bytes(q.stdout)
    assert q.returncode == 0 and json.loads(q.stdout)['quietness']['passed'], q.stderr.decode()

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
    if subject == 'managed':
        assert value['allocatedBytes'] == 0
    return value

guard('before')
try:
    commands['managed'] = ['taskset', '-c', '0', 'dotnet', a.managed, a.input,
                           str(prefix) + '-managed.rgba', a.core_sha256]
    commands['native'] = ['taskset', '-c', '0', a.native, 'copy-region-word-overlap',
                          a.input, '513', '259', '16', str(prefix) + '-native.rgba']
    for subject in ['managed', 'native']:
        errors[subject] = prefix.with_name(prefix.name + '-' + subject + '-stderr.log').open('xb')
        workers[subject] = subprocess.Popen(commands[subject], env=env, stdin=subprocess.PIPE,
            stdout=subprocess.PIPE, stderr=errors[subject], text=True, bufsize=1, start_new_session=True)
        ready[subject] = read(subject)
        assert ready[subject]['state'] == 'ready' and ready[subject]['depth'] == 16 and ready[subject]['cpu'] == 0
        assert ready[subject]['sourceAliasesCanvas'] and ready[subject]['publicMovesPerPair'] == 2
    assert ready['managed']['libraryHash'] == a.core_sha256
    assert ready['native']['nativeCallsPerPair'] == 258 and ready['native']['bandHeight'] == 1
    assert sha(str(prefix) + '-managed.rgba') == sha(str(prefix) + '-native.rgba') == ready['managed']['outputHash']
    assert sha(a.input) == ready['managed']['inputHash']
    raw.update(ready=ready, commands=commands)
    for subject in ['managed', 'native']:
        raw['warmups'][subject] = sample(subject)  # Discard elapsed, retain full untimed warm batch.
    save()
    for _ in range(n):
        raw['before'].append(sample('native'))
    save()
    for pair in range(n):
        values = dict(pair=pair)
        for subject in (['managed', 'native'] if pair % 2 == 0 else ['native', 'managed']):
            values[subject] = sample(subject)
        raw['pairs'].append(values)
        save()
    for _ in range(n):
        raw['after'].append(sample('native'))
    for subject in ['managed', 'native']:
        raw['checks'][subject] = request(subject, 'check')
        assert raw['checks'][subject]['state'] == 'checked' and raw['checks'][subject]['exact']
        workers[subject].stdin.write('quit\n')
        workers[subject].stdin.flush()
        if subject == 'managed':
            raw['stopped'][subject] = read(subject)
            assert raw['stopped'][subject]['exact'] and raw['stopped'][subject]['inputUnchanged']
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
native = dict(before=stats([ns(v) for v in raw['before']]),
              paired=stats([ns(v['native']) for v in raw['pairs']]),
              after=stats([ns(v) for v in raw['after']]))
paired = stats([math.log(ns(v['managed']) / ns(v['native'])) for v in raw['pairs']])
lower = min(managed['lower'] / max(v['upper'] for v in native.values()), math.exp(paired['lower']))
native_lower = min(v['lower'] for v in native.values())
upper = max(managed['upper'] / native_lower if native_lower > 0 else math.inf, math.exp(paired['upper']))
reference = min(v['mean'] for v in native.values())
result = dict(project='Lokad.Image', originalRequiredControl='CopyRegion/Rgba16Overlapping',
    productionChanged=False, newRequiredPaths=0, librarySha256=a.core_sha256,
    mode=a.mode, actuals=n, operationsPerActual=operations, allActualsRetained=True,
    managedUs=managed['mean']/1000, nativeUs=reference/1000, ratio=managed['mean']/reference,
    lower=lower, upper=upper, status='pass' if upper <= 1 else 'gap' if lower > 1 else 'uncertain',
    allocatedBytes=0, managed=managed, nativeBrackets=native, pairedLogStatistics=paired,
    quietGuardsPassed=True, exactFullWordSnapshotAndUntouchedPixels=True,
    inputSha256=sha(a.input), outputSha256=ready['managed']['outputHash'],
    clocksPerBatch=2, outputOwnership='One borrowed aliased canvas;two public moves,no output allocation/reset per operation.',
    confidence='All actuals;two-sided99.9%Student-t;paired log ratio and managed mean versus fastest of three native bracket lower bounds;no sample filtering.',
    boundary='Original513x259RGBA16,257x129rectangle,forward(0,0)to(1,1),thenreverse. Complete validation/clipping/cancellation and both moves stay timed. FFmpeg public av_image_copy_plane uses258disjoint one-row calls per pair with scheduling timed. Both borrow one mutable canvas;no per-op reset/output allocation. Preparation,hashing,verification,IPC and startup excluded equally. Native arguments prevalidated;managed public validation remains included.')
prefix.with_suffix('.json').write_text(json.dumps(result, indent=2) + '\n')
print(json.dumps({k: result[k] for k in ['mode', 'managedUs', 'nativeUs', 'ratio', 'lower', 'upper', 'status', 'allocatedBytes']}))
