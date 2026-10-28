"""Replay the existing PNG stream controls through actual ReadAsync-only input."""
from pathlib import Path
import hashlib
import json
import os
import resource
import struct
import subprocess
import sys

root = Path(__file__).resolve().parents[2]
resource.setrlimit(resource.RLIMIT_CORE, (0, 0))
harness = root / (sys.argv[1] if len(sys.argv) > 1 else
                  'benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.Benchmarks.dll')
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
core = digest(harness.parent / 'Lokad.Image.dll')
prior = root / 'tmp/c04-png-stream-identify-controls'
old = json.loads((prior / 'proof.json').read_text())
assert old['descriptorFields'] == 9 and old['typedSourceMetadataExact']
assert len(old['controls']) == 21
assert old['inputSha256'] == '4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f'
output = root / 'tmp/h11-png-async-identify-controls'
output.mkdir(exist_ok=True)
worker = root / 'external/warm-reference/ffmpeg-png-nonseekable-identify-warm'
controls = []
for control in old['controls']:
    name = control['name']
    source = prior / (name + '.png')
    assert digest(source) == control['inputSha256']
    expected = control['descriptor']
    for scalar in [False, True]:
        suffix = 'scalar' if scalar else 'normal'
        env = os.environ.copy()
        env.pop('LOKAD_IMAGE_SCALAR_REFERENCE', None)
        if scalar:
            env['LOKAD_IMAGE_SCALAR_REFERENCE'] = '1'
        destination = output / (name + '-' + suffix + '.info')
        result = subprocess.run(['taskset', '-c', '0', str(worker), 'png-nonseekable-identify',
                                 str(source), str(destination)], input='check\nquit\n',
                                capture_output=True, text=True, env=env)
        (output / (name + '-native-' + suffix + '.stdout')).write_text(result.stdout)
        (output / (name + '-native-' + suffix + '.stderr')).write_text(result.stderr)
        assert (result.returncode == 0) == control['accepted'], (name, suffix, result.stderr[-1000:])
        if expected is not None:
            assert list(struct.unpack('<9I', destination.read_bytes())) == expected
            ready = json.loads(result.stdout.splitlines()[0])
            assert ready['scalarMode'] == scalar and ready['outputBytes'] == 36
            assert ready['freshNonSeekableStream'] and not ready['freshSeekableStream']
            assert not ready['inputLengthQueryTimed'] and not ready['seekCallback'] and ready['lengthQueries'] == 0
            assert not ready['pixelDecode']
            assert all(ready[key] for key in ['privateInputGrowthTimed', 'residentCallbacksImmediate',
                       'inputCopyTimed', 'eofChecked', 'callerSourceLeftOpen', 'streamAcquisitionAndReleaseTimed',
                       'publicParserTimed', 'fullCrcTimed', 'structureMetadataTimed',
                       'descriptorAcquisitionAndReleaseTimed', 'independentLiveOutputs'])
        env = os.environ.copy()
        env['DOTNET_PROCESSOR_COUNT'] = '1'
        env.pop('DOTNET_EnableHWIntrinsic', None)
        if scalar:
            env['DOTNET_EnableHWIntrinsic'] = '0'
        result = subprocess.run(['taskset', '-c', '0', 'dotnet', str(harness),
                                 '--probe-async-png-identify', str(source)],
                                capture_output=True, text=True, env=env)
        (output / (name + '-managed-' + suffix + '.stdout')).write_text(result.stdout)
        (output / (name + '-managed-' + suffix + '.stderr')).write_text(result.stderr)
        assert result.returncode == 0, (name, suffix, result.stderr[-2000:])
        info = json.loads(result.stdout)
        assert info['accepted'] == control['accepted']
        assert info['AsyncReads'] > 0 and info['EofReads'] > 0
        assert info['SyncReads'] == info['MetadataQueries'] == 0
        assert info['BytesRead'] == source.stat().st_size and info['callerOpen']
        if expected is not None:
            descriptor = info['descriptor']
            assert [descriptor[key] for key in ['Format', 'Width', 'Height', 'BitDepth', 'HasAlpha',
                    'Orientation', 'SourceColor', 'SourceAlpha', 'SourceComponentCount']] == expected
    controls.append({**control, 'normalScalarNativeAgree': True, 'normalScalarManagedAgree': True,
                     'actualReadAsyncAndEof': True, 'noSyncSeekLengthAndCallerOpen': True})
    print(json.dumps(dict(control=name, matched=True)), flush=True)
proof = dict(schemaVersion=1, project='Lokad.Image', timing=False, librarySha256=core,
             inputSha256=old['inputSha256'], inputBytes=old['inputBytes'],
             nativeBuild=json.loads((worker.parent / 'png-nonseekable-identify-identity.json').read_text()),
             controls=controls, equivalentMetadataAndValidation=True, typedSourceMetadataExact=True,
             descriptorFields=9, nonSeekableAsyncDeliveryVerified=True, normalScalarNativeAgree=True,
             normalScalarManagedAgree=True,
             scope='Same original PNG and21existing controls; actualReadAsync-only/EOF/caller-open input, '
                   'native immediate nonseekableAVIO/private growth/full parser-CRC adapter. All9facts, '
                   'no pixel decode. No production build,qualification repeat,timing or workload grid.')
(output / 'proof.json').write_text(json.dumps(proof, indent=2) + '\n')
assert digest(harness.parent / 'Lokad.Image.dll') == core
print(json.dumps(dict(verified=len(controls), productionChanged=False, timing=False)))
