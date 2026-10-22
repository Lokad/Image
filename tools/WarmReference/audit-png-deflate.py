"""Original bounded RFC1950/RFC1951 observer for the frozen PNG level6 packet.

This development tool never encodes: it exposes independent stage expectations.
Format descriptions: https://www.rfc-editor.org/rfc/rfc1950.html and rfc1951.html.
No upstream implementation or implementation tables are incorporated.
"""
import argparse
import hashlib
import json
import struct
import zlib
from pathlib import Path


def sha(data):
    return hashlib.sha256(data).hexdigest()


class Bits:
    def __init__(self, data):
        self.data, self.position = data, 0

    def read(self, count):
        assert self.position + count <= len(self.data) * 8
        value = sum(((self.data[(self.position + i) // 8] >> ((self.position + i) % 8)) & 1) << i
                    for i in range(count))
        self.position += count
        return value


def coding(lengths):
    result, code = {}, 0
    for width in range(1, 16):
        for symbol, size in enumerate(lengths):
            if size == width:
                result[width, code] = symbol
                code += 1
        assert code <= 1 << width
        code <<= 1
    assert result
    return result


def symbol(bits, table):
    code = 0
    for width in range(1, 16):
        code = code * 2 + bits.read(1)
        if (width, code) in table:
            return table[width, code]
    raise ValueError('Invalid Huffman symbol')


def dynamic(bits):
    literals, distances, count = bits.read(5) + 257, bits.read(5) + 1, bits.read(4) + 4
    alphabet = [0] * 19
    # RFC1951 section3.2.7 defines this format permutation.
    for index in [16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15][:count]:
        alphabet[index] = bits.read(3)
    table, lengths, runs = coding(alphabet), [], []
    while len(lengths) < literals + distances:
        value = symbol(bits, table)
        if value < 16:
            lengths.append(value)
            runs.append([value, 0, 0])
        elif value == 16:
            assert lengths
            extra = bits.read(2)
            lengths.extend([lengths[-1]] * (extra + 3))
            runs.append([value, extra, 2])
        elif value == 17:
            extra = bits.read(3)
            lengths.extend([0] * (extra + 3))
            runs.append([value, extra, 3])
        else:
            extra = bits.read(7)
            lengths.extend([0] * (extra + 11))
            runs.append([value, extra, 7])
        assert len(lengths) <= literals + distances
    assert lengths[256]
    return lengths[:literals], lengths[literals:], alphabet, runs, count


def observe(packet_path, token_path):
    packet = Path(packet_path).read_bytes()
    assert sha(packet) == 'ad9ed127da583d68bdb749bb983b73966e8e814527216449c4557fdd89668920'
    assert packet[:8] == b'\x89PNG\r\n\x1a\n'
    at, idat = 8, bytearray()
    while at < len(packet):
        size = struct.unpack_from('>I', packet, at)[0]
        assert size <= 2_000_000 and at + size + 12 <= len(packet)
        kind, payload = packet[at + 4:at + 8], packet[at + 8:at + 8 + size]
        assert zlib.crc32(kind + payload) == struct.unpack_from('>I', packet, at + 8 + size)[0]
        if kind == b'IDAT':
            idat.extend(payload)
        at += size + 12
    assert at == len(packet) and idat[:2] == b'\x78\x9c'
    bits, output, all_tokens, blocks = Bits(idat[2:-4]), bytearray(), [], []
    final = False
    while not final:
        start_bit, start_output, start_token = bits.position, len(output), len(all_tokens)
        final, kind = bits.read(1), bits.read(2)
        if kind == 0:
            bits.position = (bits.position + 7) // 8 * 8
            length, complement = bits.read(16), bits.read(16)
            assert length ^ complement == 65535
            for _ in range(length):
                value = bits.read(8)
                output.append(value)
                all_tokens.append((value, 0, 0))
        else:
            assert kind in (1, 2)
            if kind == 2:
                literal_lengths, distance_lengths, code_lengths, runs, code_length_count = dynamic(bits)
            else:
                literal_lengths = [8 if i < 144 else 9 if i < 256 else 7 if i < 280 else 8 for i in range(288)]
                distance_lengths = [5] * 32
                code_lengths = []
                runs, code_length_count = [], 0
            literal_table, distance_table = coding(literal_lengths), coding(distance_lengths)
            while True:
                value = symbol(bits, literal_table)
                if value < 256:
                    output.append(value)
                    all_tokens.append((value, 0, 0))
                elif value == 256:
                    break
                else:
                    assert 257 <= value <= 285
                    if value < 265:
                        length = value - 254
                    elif value == 285:
                        length = 258
                    else:
                        extra = (value - 261) // 4
                        length = 3 + ((4 + (value - 265) % 4) << extra) + bits.read(extra)
                    distance = symbol(bits, distance_table)
                    assert distance < 30
                    if distance < 4:
                        distance += 1
                    else:
                        extra = distance // 2 - 1
                        distance = 1 + ((2 + distance % 2) << extra) + bits.read(extra)
                    assert 0 < distance <= min(len(output), 32768)
                    all_tokens.append((0, length, distance))
                    for _ in range(length):
                        output.append(output[-distance])
                assert len(output) <= 531727 and len(all_tokens) <= 531727
        encoded = b''.join(struct.pack('<HHH', *token) for token in all_tokens[start_token:])
        blocks.append(dict(type=kind, final=bool(final), firstToken=start_token, tokens=len(all_tokens) - start_token,
                           outputOffset=start_output, outputBytes=len(output) - start_output,
                           startBit=start_bit, endBit=bits.position, tokenSha256=sha(encoded),
                           literalLengths=literal_lengths if kind else [],
                           distanceLengths=distance_lengths if kind else [],
                           codeLengths=code_lengths if kind else [],
                           headerRuns=runs if kind else [], codeLengthCount=code_length_count if kind else 0))
    assert (bits.position + 7) // 8 == len(idat) - 6
    assert zlib.adler32(output) == struct.unpack('>I', idat[-4:])[0]
    assert len(output) == 531727 and output == zlib.decompress(idat)
    assert sha(output) == '8bd1106ca96eb736e92f0e71e6fbe486d0beebe70cf7e3fdf311946ea0a25e10'
    encoded = b''.join(struct.pack('<HHH', *token) for token in all_tokens)
    Path(token_path).write_bytes(encoded)
    return dict(project='Lokad.Image', reference='Frozen PNG native6', packetSha256=sha(packet),
                tokenBytes=len(encoded), tokenCount=len(all_tokens), tokenSha256=sha(encoded),
                independentExpansionExact=True, filteredBytes=len(output), filteredSha256=sha(output), blocks=blocks)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--packet', required=True)
    parser.add_argument('--tokens', required=True)
    parser.add_argument('--output', required=True)
    arguments = parser.parse_args()
    result = observe(arguments.packet, arguments.tokens)
    Path(arguments.output).write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(result))
