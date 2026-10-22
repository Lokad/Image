#!/usr/bin/env python3
"""Compare complete JPEG packets and locate structural divergence (ITU-T T.81 B).

Development tooling only. Inputs are original generated controls, not upstream
corpora. No decoding, metadata stripping, byte normalization or quality matching.
Exit 0 means identical complete packets (or the explicit attribution exception);
exit 1 means different packets. No general metadata normalization is permitted.
"""

import argparse
import hashlib
import json
from pathlib import Path


def digest(data):
    return hashlib.sha256(data).hexdigest()


def describe(data):
    if not data.startswith(b"\xff\xd8"):
        raise ValueError("Missing JPEG SOI")
    markers = [{"marker": "FFD8", "offset": 0, "bytes": 2}]
    position = 2
    frame = None
    while position < len(data):
        start = position
        if data[position] != 255:
            raise ValueError(f"Expected marker at {position}")
        while position < len(data) and data[position] == 255:
            position += 1
        if position == len(data):
            raise ValueError("Truncated marker")
        code = data[position]
        position += 1
        if code == 0xD9:
            markers.append({"marker": "FFD9", "offset": start, "bytes": position - start})
            if position != len(data):
                raise ValueError("Trailing bytes after JPEG EOI")
            return {"bytes": len(data), "sha256": digest(data), "frame": frame, "markers": markers}
        if code in (0, 0xD8) or 0xD0 <= code <= 0xD7:
            raise ValueError(f"Unexpected marker {code:02X}")
        if position + 2 > len(data):
            raise ValueError("Truncated segment length")
        length = int.from_bytes(data[position:position + 2], "big")
        end = position + length
        if length < 2 or end > len(data):
            raise ValueError("Invalid segment length")
        payload = data[position + 2:end]
        segment = {"marker": f"FF{code:02X}", "offset": start,
                   "bytes": end - start, "payloadSha256": digest(payload)}
        if code in (0xC0, 0xC1, 0xC2):
            if len(payload) < 6 or len(payload) != 6 + 3 * payload[5]:
                raise ValueError("Invalid SOF")
            components = [{"id": payload[i], "horizontal": payload[i + 1] >> 4,
                           "vertical": payload[i + 1] & 15, "quantization": payload[i + 2]}
                          for i in range(6, len(payload), 3)]
            frame = {"mode": f"FF{code:02X}", "depth": payload[0],
                     "height": int.from_bytes(payload[1:3], "big"),
                     "width": int.from_bytes(payload[3:5], "big"), "components": components,
                     "mcuWidth": 8 * max(c["horizontal"] for c in components),
                     "mcuHeight": 8 * max(c["vertical"] for c in components)}
            segment["frame"] = frame
        elif code == 0xDB:
            tables, cursor = [], 0
            while cursor < len(payload):
                info = payload[cursor]
                precision = info >> 4
                if precision > 1:
                    raise ValueError("Invalid DQT precision")
                width = precision + 1
                table_end = cursor + 1 + 64 * width
                if table_end > len(payload):
                    raise ValueError("Truncated DQT")
                tables.append({"id": info & 15, "precisionBits": width * 8,
                               "zigzagValues": [int.from_bytes(payload[i:i + width], "big")
                                                for i in range(cursor + 1, table_end, width)]})
                cursor = table_end
            segment["quantization"] = tables
        elif code == 0xDA:
            if not payload or len(payload) != 4 + 2 * payload[0]:
                raise ValueError("Invalid SOS")
            segment["scan"] = {"components": [{"id": payload[i], "dcTable": payload[i + 1] >> 4,
                                                "acTable": payload[i + 1] & 15}
                                               for i in range(1, 2 * payload[0] + 1, 2)],
                               "spectralStart": payload[-3], "spectralEnd": payload[-2],
                               "successiveHigh": payload[-1] >> 4, "successiveLow": payload[-1] & 15}
        elif code == 0xFE:
            segment["commentHex"] = payload.hex()
        markers.append(segment)
        position = end
        if code != 0xDA:
            continue
        entropy_start = position
        restarts = 0
        while position < len(data):
            if data[position] != 255:
                position += 1
                continue
            marker_start = position
            while position < len(data) and data[position] == 255:
                position += 1
            if position == len(data):
                raise ValueError("Truncated entropy marker")
            next_code = data[position]
            if next_code == 0 or 0xD0 <= next_code <= 0xD7:
                restarts += int(next_code != 0)
                position += 1
                continue
            position = marker_start
            break
        entropy = data[entropy_start:position]
        markers.append({"marker": "entropy", "offset": entropy_start, "bytes": len(entropy),
                        "sha256": digest(entropy), "restartMarkers": restarts})
    raise ValueError("Missing JPEG EOI")


def attribution_packet(packet, description, permitted):
    """Replace exactly one pinned encoder comment, retaining all other bytes."""
    comments = [m for m in description["markers"] if m["marker"] == "FFFE"]
    if len(comments) != 1 or comments[0]["commentHex"] != permitted.hex():
        return None
    comment = comments[0]
    canonical = b"\xff\xfe\x00\x0eLokad.Image\0"
    return packet[:comment["offset"]] + canonical + packet[comment["offset"] + comment["bytes"]:]


def compare(actual, reference, allow_encoder_attribution=False):
    difference = next((i for i, (a, b) in enumerate(zip(actual, reference)) if a != b), None)
    if difference is None and len(actual) != len(reference):
        difference = min(len(actual), len(reference))
    result = {"exact": difference is None, "firstDifferentOffset": difference,
              "actual": describe(actual), "reference": describe(reference)}
    result["accepted"] = result["exact"]
    if allow_encoder_attribution:
        adjusted_actual = attribution_packet(actual, result["actual"], b"Lokad.Image\0")
        adjusted_reference = attribution_packet(reference, result["reference"], b"Lavc62.28.100\0")
        result["encoderAttributionOnly"] = (adjusted_actual is not None and adjusted_reference is not None
                                           and adjusted_actual == adjusted_reference)
        result["accepted"] |= result["encoderAttributionOnly"]
    if difference is not None:
        for name, packet in (("actual", actual), ("reference", reference)):
            result[name]["differentByte"] = packet[difference] if difference < len(packet) else None
            result[name]["differentSegment"] = next((m["marker"] for m in result[name]["markers"]
                                                     if m["offset"] <= difference < m["offset"] + m["bytes"]), None)
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("actual", type=Path)
    parser.add_argument("reference", type=Path)
    parser.add_argument("--allow-encoder-attribution", action="store_true",
                        help="Allow only the pinned Lavc62.28.100 NUL comment to become Lokad.Image NUL")
    arguments = parser.parse_args()
    packets = []
    for path in (arguments.actual, arguments.reference):
        if path.stat().st_size > 32 * 1024 * 1024:
            parser.error("Diagnostic input exceeds 32 MiB")
        packets.append(path.read_bytes())
    try:
        result = compare(*packets, allow_encoder_attribution=arguments.allow_encoder_attribution)
    except ValueError as error:
        parser.error(str(error))
    print(json.dumps(result, indent=2))
    return 0 if result["accepted"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
