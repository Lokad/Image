"""Complete fresh-stream JPEG/WebP header comparisons on the quiet VM; no pixel work."""
from pathlib import Path
import argparse, hashlib, json, math, os, select, signal, statistics, subprocess

p = argparse.ArgumentParser()
p.add_argument('--worker', required=True)
p.add_argument('--native', required=True)
p.add_argument('--core-sha256', required=True)
p.add_argument('--input', required=True)
p.add_argument('--expected', required=True)
p.add_argument('--format', choices=['jpeg', 'webp'], required=True)
p.add_argument('--prefix', required=True)
p.add_argument('--mode', choices=['short', 'confirm'], required=True)
a = p.parse_args()
prefix, expected = Path(a.prefix), json.loads(a.expected)
n = 3 if a.mode == 'short' else 12
critical = {3: 31.59905444857171, 12: 4.436979337146836}[n]
operations = 16384 if a.format == 'jpeg' else 131072
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
    temporary.write_text(json.dumps(raw, indent=2)+'\n')
    temporary.replace(path)


def guard(side):
    os.sync()
    r = subprocess.run(['python3', 'tools/WarmReference/capture-vm-environment.py', '.'], capture_output=True)
    prefix.with_name(prefix.name+'-'+side+'.json').write_bytes(r.stdout)
    assert r.returncode == 0 and json.loads(r.stdout)['quietness']['passed'], r.stderr.decode()


def read(subject):
    worker = workers[subject]
    assert select.select([worker.stdout], [], [], 60)[0], (subject, 'observation timeout', worker.poll())
    line = worker.stdout.readline()
    assert line, (subject, worker.poll())
    value = json.loads(line)
    wire.write(json.dumps(dict(subject=subject, value=value))+'\n')
    return value


def request(subject, command):
    workers[subject].stdin.write(command+'\n')
    workers[subject].stdin.flush()
    return read(subject)


def sample(subject):
    value = request(subject, 'sample '+str(operations))
    assert value['state'] == 'sample' and value['operations'] == operations
    assert value['elapsedNanoseconds'] > 0 and value['cpu'] == 0
    return value


guard('before')
try:
    commands = dict(managed=['taskset', '-c', '0', 'dotnet', a.worker, a.input,
                            str(prefix)+'-managed.info', a.core_sha256, a.expected],
                    native=['taskset', '-c', '0', a.native, a.format, a.input, str(prefix)+'-native.info'])
    for subject in commands:
        errors[subject] = prefix.with_name(prefix.name+'-'+subject+'-stderr.log').open('xb')
        workers[subject] = subprocess.Popen(commands[subject], env=env, stdin=subprocess.PIPE,
            stdout=subprocess.PIPE, stderr=errors[subject], text=True, bufsize=1, start_new_session=True)
        ready = read(subject)
        assert ready['state'] == 'ready' and ready['cpu'] == 0 and ready['freshSeekableStream']
        assert not ready['inputView'] and not ready['wholeInputCopy']
        raw['ready'][subject] = ready
    assert raw['ready']['managed']['coreHash'] == a.core_sha256
    assert raw['ready']['managed']['info'] == {**expected, 'OrientedWidth': expected['Height'] if expected['Orientation'] >= 5 else expected['Width'],
                                               'OrientedHeight': expected['Width'] if expected['Orientation'] >= 5 else expected['Height']}
    assert raw['ready']['native']['eofChecked'] and raw['ready']['native']['independentLiveOutputs']
    assert sha(str(prefix)+'-managed.info') == sha(str(prefix)+'-native.info')
    raw['commands'] = commands
    for subject in workers:
        raw['warmups'][subject] = sample(subject)
    save()
    for _ in range(n):
        raw['before'].append(sample('native'))
    save()
    for index in range(n):
        pair = dict(index=index)
        for subject in (['managed', 'native'] if index % 2 == 0 else ['native', 'managed']):
            pair[subject] = sample(subject)
        raw['pairs'].append(pair)
        save()
    for _ in range(n):
        raw['after'].append(sample('native'))
    for subject in workers:
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
    error = statistics.stdev(values)/math.sqrt(len(values))
    return dict(mean=mean, lower=mean-critical*error, upper=mean+critical*error, values=values)


def ns(value):
    return value['elapsedNanoseconds']/value['operations']


managed = stats([ns(v['managed']) for v in raw['pairs']])
native = dict(before=stats([ns(v) for v in raw['before']]),
              paired=stats([ns(v['native']) for v in raw['pairs']]), after=stats([ns(v) for v in raw['after']]))
paired = stats([math.log(ns(v['managed'])/ns(v['native'])) for v in raw['pairs']])
lower = min(managed['lower']/max(v['upper'] for v in native.values()), math.exp(paired['lower']))
native_lower = min(v['lower'] for v in native.values())
upper = max(managed['upper']/native_lower if native_lower > 0 else math.inf, math.exp(paired['upper']))
reference = min(v['mean'] for v in native.values())
allocations = [v['managed']['allocatedBytes']/operations for v in raw['pairs']]
assert len(set(allocations)) == 1, allocations
result = dict(project='Lokad.Image', caseId='JpegHeaders' if a.format == 'jpeg' else 'WebpHeaders',
    originalRequiredControl='InspectHeaders/Jpeg420FreshStream' if a.format == 'jpeg' else 'InspectHeaders/WebpLosslessRgba8FreshSeekable',
    productionChanged=False, newRequiredPaths=0, librarySha256=a.core_sha256, mode=a.mode,
    actualIterations=n, nativeSamplesEachBracket=n, operationsPerActual=operations, allActualsRetained=True,
    managedUs=managed['mean']/1000, nativeUs=reference/1000, ratio=managed['mean']/reference, lower=lower, upper=upper,
    status='pass' if upper <= 1 else 'gap' if lower > 1 else 'uncertain', allocatedBytes=allocations[0],
    managed=managed, nativeBrackets=native, pairedLogStatistics=paired, quietGuardsPassed=True,
    descriptorExact=True, independentSurvivingOutputs=True, eofChecked=True, inputUnchanged=True,
    inputSha256=sha(a.input), inputBytes=Path(a.input).stat().st_size, expected=expected,
    outputSha256=sha(str(prefix)+'-managed.info'), nativeReady=raw['ready']['native'], clocksPerBatch=2,
    reference='FFmpeg AVIO/owned descriptor APIs plus original bounded structural/EXIF adapter;not public packet-parser performance',
    boundary='Complete public InspectHeaders on fresh non-exposed seekable source through EOF,including source/workspace/descriptor acquisition and source/workspace release. Independent output survives source/sibling release. Exact9typed source fields;no pixels,whole-input copy,cached descriptor or input view. Native AVIO8192B+adapter8192B+header32B is bounded separately from managed reader. Both borrow identical resident packet;preparation,JIT,IPC,hashing and controls excluded equally. OneCPU,natural nonconcurrent workstationGC.',
    confidence='All actuals;two-sided99.9percentStudentt,pairedlogratio and managed mean against fastest of three native bracket lower bounds;no filtering or forcedGC.')
prefix.with_suffix('.json').write_text(json.dumps(result, indent=2)+'\n')
print(json.dumps({k: result[k] for k in ['caseId', 'mode', 'managedUs', 'nativeUs', 'ratio', 'lower', 'upper', 'status', 'allocatedBytes']}))
