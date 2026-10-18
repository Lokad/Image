"""Original development pixel oracle for the six --font-render controls.

Solves the original quadratic equations at 4096 midpoint ordinates per pixel
row and integrates horizontal nonzero-winding intervals. No flattening,
trapezoid clipping, font parser or production code is used by this oracle.
"""
from pathlib import Path
import hashlib
import json
import math
import struct
import sys


def lines(points):
    return [(points[i - 1], None, points[i]) for i in range(len(points))]


triangle = [(50, 0), (300, 700), (550, 0)]
cases = {
    'triangle': lines(triangle),
    'quadratic': [((0, 0), (0, 800), (600, 0)), ((600, 0), (300, -200), (0, 0))],
    'accent': lines(triangle) + lines([(200, 780), (200, 850), (400, 850), (400, 780)]),
    'implied': [((100, 0), (0, 0), (0, 100)), ((0, 100), (0, 200), (100, 200)),
                ((100, 200), (200, 200), (200, 100)), ((200, 100), (200, 0), (100, 0))],
    # Half-sized A at offset100, with the child's untransformed phantom Left=26.
    'metrics': lines([(99, 0), (224, 350), (349, 0)]),
    'matrix': lines([(50, -25), (650, 550), (550, -275)]),
}


def intersections(segments, y):
    result = []
    for p, c, q in segments:
        if c is None:
            a, b, constant = 0, q[1] - p[1], p[1] - y
        else:
            a, b, constant = p[1] - 2 * c[1] + q[1], 2 * (c[1] - p[1]), p[1] - y
        if a == 0:
            roots = [] if b == 0 else [-constant / b]
        else:
            discriminant = b * b - 4 * a * constant
            roots = [] if discriminant <= 0 else [(-b - math.sqrt(discriminant)) / (2 * a), (-b + math.sqrt(discriminant)) / (2 * a)]
        for t in roots:
            if not 0 <= t < 1:
                continue
            derivative = 2 * a * t + b
            if derivative == 0:
                continue
            x = p[0] + t * (q[0] - p[0]) if c is None else (1 - t)**2 * p[0] + 2 * t * (1 - t) * c[0] + t*t * q[0]
            result.append((x, 1 if derivative > 0 else -1))
    return sorted(result)


folder = Path(sys.argv[1])
managed = json.loads((folder / 'managed-render.json').read_text())
rows = []
steps = 4096
for case in managed['rows']:
    assert (case['width'], case['height'], case['size'], case['x'], case['y'], case['baseline']) == (20, 16, 10, .25, .5, 8)
    # Literal authored design coordinates; transform independently to canvas pixels.
    def position(p):
        return None if p is None else (.25 + p[0] / 100, 8.5 - p[1] / 100)
    segments = [tuple(position(p) for p in segment) for segment in cases[case['name']]]
    expected = [0.0] * (20 * 16)
    for py in range(16):
        for sample in range(steps):
            y = py + (sample + .5) / steps
            crossing = intersections(segments, y)
            winding = 0
            for x, direction in crossing:
                previous = winding
                winding += direction
                if previous == 0 and winding != 0:
                    left = x
                elif previous != 0 and winding == 0:
                    for px in range(max(0, math.floor(left)), min(20, math.ceil(x))):
                        expected[py * 20 + px] += max(0, min(px + 1, x) - max(px, left)) / steps
            assert winding == 0
    actual8 = (folder / (case['name'] + '.alpha8')).read_bytes()
    actual16 = struct.unpack('<320H', (folder / (case['name'] + '.alpha16')).read_bytes())
    assert len(actual8) == 320
    coverage_error = max(abs(a / 65535 - b) for a, b in zip(actual16, expected))
    byte_error = max(abs(a - math.floor(b * 255 + .5)) for a, b in zip(actual8, expected))
    rows.append(dict(name=case['name'],maximumCoverageError=coverage_error,maximumByteAlphaError=byte_error,
                     expectedArea=sum(expected),actualSixteenBitArea=sum(actual16) / 65535))
report = dict(schemaVersion=1,scope='Development correctness: independent original quadratic-equation coverage integration; no performance baseline',
              managedProducerSha256=managed['assemblySha256'],referenceSourceSha256=hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),
              verticalSamplesPerRow=steps,maximumCoverageErrorGate=1/512,maximumByteAlphaErrorGate=1,rows=rows)
(folder / 'analytic-render-comparison.json').write_text(json.dumps(report, indent=2) + '\n')
print(json.dumps(report))
assert all(row['maximumCoverageError'] <= 1/512 and row['maximumByteAlphaError'] <= 1 for row in rows)
