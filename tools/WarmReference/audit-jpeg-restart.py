"""Read-only first-divergence audit of the two frozen JPEG420 native packets.

Framing and coefficient geometry follow T.81 B.2 and A.3. The coefficient visitor
uses public libjpeg APIs; no packet, source or implementation table is generated.
"""
import hashlib
import json
import struct
import sys
from pathlib import Path


def digest(data):
    return hashlib.sha256(data).hexdigest()


def packet(path, expected):
    data = Path(path).read_bytes()
    assert digest(data) == expected, "Frozen native packet differs"
    assert data[:2] == b"\xff\xd8"
    segments, restarts = [], []
    at = 2
    while at < len(data):
        assert data[at] == 255
        marker = data[at + 1]
        length = int.from_bytes(data[at + 2:at + 4], "big")
        assert length >= 2 and at + 2 + length <= len(data)
        payload = data[at + 4:at + 2 + length]
        segments.append((marker, payload))
        at += 2 + length
        if marker != 218:
            continue
        while at < len(data):
            if data[at] != 255:
                at += 1
                continue
            following = data[at + 1]
            at += 2
            if following == 0:
                continue
            if 208 <= following <= 215:
                restarts.append(following - 208)
                continue
            assert following == 217 and at == len(data), "Unexpected scan marker"
            break
        else:
            raise AssertionError("Missing EOI")
        break
    return data, segments, restarts


def coefficients(path):
    data = Path(path).read_bytes()
    assert data[:8] == b"LJCF0001"
    assert struct.unpack_from("<III", data, 8) == (513, 259, 3)
    components, at = [], 20
    for index in range(3):
        identity, horizontal, vertical, columns, rows = struct.unpack_from("<IIIII", data, at)
        assert (horizontal, vertical, columns, rows) == ((2, 2, 65, 33) if index == 0 else (1, 1, 33, 17))
        count = columns * rows * 64
        quantizers = data[at + 20:at + 148]
        at += 148
        values = struct.unpack_from(f"<{count}h", data, at)
        at += count * 2
        components.append((identity, quantizers, values))
    assert at == len(data)
    return data, components


def audit(no_restart_path, restart_path, no_restart_coefficients, restart_coefficients):
    plain, plain_segments, plain_rst = packet(no_restart_path,
        "ccca91bcdaddc90c7aecf62d07cdc629c09c27e607c955cfcc97a7bf91caaf08")
    restart, restart_segments, restart_rst = packet(restart_path,
        "bd0060bf5bbf48a9f55120c5e71b22ac2763751bb370e9c5c2d93ebba38ed622")
    assert not plain_rst and restart_rst == list(range(8)) * 2
    assert [payload for marker, payload in restart_segments if marker == 221] == [b"\x00\x21"]
    unchanged = {}
    for marker, name in [(254, "attribution"), (219, "quantization"), (192, "frame"), (218, "scanHeader")]:
        left = [p for m, p in plain_segments if m == marker]
        right = [p for m, p in restart_segments if m == marker]
        unchanged[name] = left == right
        assert unchanged[name], f"Unexpected {name} difference"
    first_dump, first = coefficients(no_restart_coefficients)
    second_dump, second = coefficients(restart_coefficients)
    differences, maximum, total = [], 0, 0
    for index, (left, right) in enumerate(zip(first, second)):
        assert left[:2] == right[:2]
        for position, (a, b) in enumerate(zip(left[2], right[2])):
            total += 1
            if a != b:
                maximum = max(maximum, abs(a - b))
                differences.append({"component": index, "value": position, "noRestart": a, "restart": b})
    tables = []
    for segments in [plain_segments, restart_segments]:
        table_set = []
        for marker, payload in segments:
            if marker != 196:
                continue
            at = 0
            while at < len(payload):
                info = payload[at]
                count = sum(payload[at + 1:at + 17])
                end = at + 17 + count
                assert end <= len(payload)
                table_set.append({"id": info, "symbols": count, "sha256": digest(payload[at:end])})
                at = end
        tables.append(table_set)
    return {
        "schemaVersion": 1, "project": "Lokad.Image", "round": "E9",
        "scope": "Frozen 513x259 RGBA8/YCbCr420 qscale5: no restart versus serial-slice row restart33",
        "nativePacketBytes": [len(plain), len(restart)],
        "nativePacketSha256": [digest(plain), digest(restart)],
        "firstDifferentPacketOffset": next(i for i, (a, b) in enumerate(zip(plain, restart)) if a != b),
        "markers": [[f"{marker:02x}" for marker, _ in s] for s in [plain_segments, restart_segments]],
        "unchangedHeaders": unchanged, "huffmanTables": tables,
        "restartInterval": 33, "restartMarkers": restart_rst,
        "coefficientDumpSha256": [digest(first_dump), digest(second_dump)],
        "coefficientValues": total, "differentCoefficients": len(differences),
        "maximumCoefficientError": maximum, "firstDifferences": differences[:12],
        "productionChanged": False, "productionBuilds": 0, "nativeEncodes": 0, "timingRuns": 0,
        "paddingScope": "Visitor exposes actual component blocks; cropped MCU padding is not claimed by coefficient equality",
    }


if __name__ == "__main__":
    if len(sys.argv) != 5:
        raise SystemExit("Usage: audit-jpeg-restart.py NO_RESTART.jpg RESTART.jpg NO_RESTART.coefficients RESTART.coefficients")
    print(json.dumps(audit(*sys.argv[1:]), indent=2))
