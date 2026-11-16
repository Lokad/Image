"""Bounded same-packet grammar controls for complete JPEG/WebP header references; no timings."""
from pathlib import Path
import argparse, hashlib, json, os, struct, subprocess

p = argparse.ArgumentParser()
p.add_argument('--worker', required=True)
p.add_argument('--native', required=True)
p.add_argument('--core-sha256', required=True)
p.add_argument('--output', required=True)
a = p.parse_args()
output = Path(a.output)
output.mkdir()
sha = lambda b: hashlib.sha256(b).hexdigest()
names = ['Format', 'Width', 'Height', 'BitDepth', 'HasAlpha', 'Orientation', 'SourceColor', 'SourceAlpha', 'SourceComponentCount']
cases = []


def add(kind, name, data, values=None):
    expected = None if values is None else dict(zip(names, values))
    cases.append(dict(kind=kind, name=name, data=data, expected=expected))


def segment(marker, payload):
    return bytes([255, marker]) + struct.pack('>H', len(payload)+2) + payload


def chunk(kind, data, pad=0):
    return kind + struct.pack('<I', len(data)) + data + (bytes([pad]) if len(data) & 1 else b'')


def riff(*chunks):
    data = b'WEBP' + b''.join(chunks)
    return b'RIFF' + struct.pack('<I', len(data)) + data


exif = b'II' + struct.pack('<HIH', 42, 8, 1) + struct.pack('<HHIHHI', 0x112, 3, 1, 6, 0, 0)
jpeg = Path('artifacts/warm-reference/read-boundaries/jpeg420.bin').read_bytes()
assert sha(jpeg) == '5d3a88c6287037f759aed58b86ea06bf963f4350fb5af33873de5be2ed8be87c'
base = [1, 513, 259, 8, False, 1, 0, 1, 3]
position, segments = 2, []
while position < len(jpeg):
    start = position
    assert jpeg[position] == 255
    position += 1
    while jpeg[position] == 255:
        position += 1
    marker = jpeg[position]
    position += 1
    if marker == 0xd9:
        break
    length = struct.unpack_from('>H', jpeg, position)[0]
    end = position + length
    segments.append(dict(marker=marker, start=start, end=end))
    position = end
    if marker == 0xda:
        break
sof = next(s for s in segments if s['marker'] in [0xc0, 0xc2])
sos = next(s for s in segments if s['marker'] == 0xda)
add('jpeg', 'original', jpeg, base)
add('jpeg', 'opaque-entropy', jpeg[:sos['end']] + b'\x7f\xff\xd9', base)
add('jpeg', 'late-exif', jpeg[:-2] + segment(0xe1, b'Exif\0\0'+exif) + jpeg[-2:], [*base[:5], 6, *base[6:]])
add('jpeg', 'late-adobe', jpeg[:-2] + segment(0xee, b'Adobe'+bytes(6)+b'\x01') + jpeg[-2:], [*base[:6], 4, *base[7:]])
add('jpeg', 'missing-eoi', jpeg[:-2])
add('jpeg', 'trailing-byte', jpeg+b'!')
add('jpeg', 'invalid-segment-length', jpeg[:4]+b'\x00\x01'+jpeg[6:])
add('jpeg', 'restart-before-scan', jpeg[:2]+b'\xff\xd0'+jpeg[2:])
add('jpeg', 'duplicate-frame', jpeg[:sos['start']]+jpeg[sof['start']:sof['end']]+jpeg[sos['start']:])

webp = Path('artifacts/warm-reference/webp-decode/input.webp').read_bytes()
assert sha(webp) == 'a4ed2454404ca6c5798450fff0cfc502fd44a592c284b77e7861128b0c53927f'
position = 12
while position < len(webp):
    size = struct.unpack_from('<I', webp, position+4)[0]
    if webp[position:position+4] == b'VP8L':
        image = webp[position:position+8+size+(size & 1)]
        header = webp[position+8:position+13]
        break
    position += 8+size+(size & 1)
else:
    raise AssertionError('Original lossless chunk missing.')
canvas = chunk(b'VP8X', bytes([24, 0, 0, 0]) + (512).to_bytes(3, 'little') + (258).to_bytes(3, 'little'))
base = [2, 513, 259, 8, True, 1, 2, 2, 4]
add('webp', 'original', webp, base)
add('webp', 'opaque-image-payload', riff(chunk(b'VP8L', header)), base)
add('webp', 'late-exif', riff(canvas, image, chunk(b'EXIF', exif)), [*base[:5], 6, *base[6:]])
add('webp', 'truncated', webp[:-1])
add('webp', 'trailing-byte', webp+b'!')
add('webp', 'missing-pixels', riff(chunk(b'JUNK', b'1234')))
changed = bytearray(header)
changed[4] |= 32
add('webp', 'unsupported-version', riff(chunk(b'VP8L', changed)))
add('webp', 'nonzero-padding', riff(chunk(b'VP8L', header, 1)))
add('webp', 'missing-flagged-exif', riff(canvas, image))
add('webp', 'unknown-padded-chunk', riff(chunk(b'JUNK', b'1'), image), base)
add('webp', 'duplicate-image', riff(image, image))
assert len(cases) == 20
records = []
for case in cases:
    label = case['kind']+'-'+case['name']
    path = output/(label+'.bin')
    path.write_bytes(case['data'])
    target = output/(label+'.info')
    native = subprocess.run(['taskset', '-c', '0', a.native, case['kind'], str(path), str(target)],
                            input='quit\n', capture_output=True, text=True)
    expected = case['expected']
    if expected is None:
        assert native.returncode != 0, label
    else:
        assert native.returncode == 0, (label, native.stderr)
        assert list(struct.unpack('<9I', target.read_bytes())) == [expected[n] for n in names], label
        ready = json.loads(native.stdout)
        assert ready['eofChecked'] and ready['freshSeekableStream'] and ready['independentLiveOutputs']
    expected_argument = expected or dict(zip(names, [1, 513, 259, 8, False, 1, 0, 1, 3]))
    managed = subprocess.run(['dotnet', a.worker, str(path), str(output/(label+'-managed.info')),
                              a.core_sha256, json.dumps(expected_argument), 'control'], capture_output=True, text=True)
    assert managed.returncode == 0, (label, managed.stdout, managed.stderr)
    value = json.loads(managed.stdout)
    assert value['callerOpen'] and value['accepted'] == (expected is not None), (label, value)
    if expected is not None:
        assert value['eof'] and all(value['info'][n] == expected[n] for n in names), (label, value)
    records.append(dict(name=label, inputSha256=sha(case['data']), bytes=len(case['data']), expected=expected,
                        nativeAccepted=native.returncode == 0, managed=value))
record = dict(project='Lokad.Image', correctnessOnly=True, timingClaim=False, controls=20,
              originalImages=2, newImages=0, workerSha256=sha(Path(a.worker).read_bytes()),
              nativeSha256=sha(Path(a.native).read_bytes()), coreSha256=a.core_sha256, cases=records)
(output/'controls.json').write_text(json.dumps(record, indent=2)+'\n')
print(json.dumps(dict(controls=20, originalImages=2, newImages=0, accepted=sum(c['expected'] is not None for c in records), timingClaim=False)))
