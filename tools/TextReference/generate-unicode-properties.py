"""Encode normative Unicode property facts, never upstream implementation tables.

The pinned specification inputs must already be in ignored external/. This tool
is not a production build input. No Unicode test corpus or implementation is used.
"""
from pathlib import Path
import argparse, hashlib, re

ROOT = Path(__file__).resolve().parents[2]
INPUT = ROOT / 'external/specifications/unicode-17.0.0'
HASHES = {
    'DerivedBidiClass.txt': '4867b4b7f0731ed1bfcd34cc6251211ff1542541fce0734b6fbda139ee80b3a4',
    'DerivedJoiningType.txt': 'f39ebe974825d6736aee15582250307aa532b2cfab3caf3f86bd23fddc9c5c4d',
    'BidiBrackets.txt': 'dadbaf38a0d0246e5b805bf8725cb81b7c621f93d030595635f5ba2c2f179428',
    'BidiMirroring.txt': 'a2f16fb873ab4fcdf3221cb1a8a85a134ddd6ed03603181823ff5206af3741ce',
    'Scripts.txt': '9f5e50d3abaee7d6ce09480f325c706f485ae3240912527e651954d2d6b035bf',
    'DerivedBinaryProperties.txt': '13dd09d35a9377e33eb388a01e6581d4bfec6b2685316078c341982fa444071a',
}
BIDI = 'L R AL EN AN NSM ES ET CS BN B S WS ON LRE RLE LRO RLO PDF LRI RLI FSI PDI'.split()
JOIN = 'U R L D C T'.split()
ALIASES = dict(zip(('Left_To_Right', 'Right_To_Left', 'Arabic_Letter', 'European_Terminator'), ('L', 'R', 'AL', 'ET')))

def lines(name):
    data = (INPUT/name).read_bytes()
    assert hashlib.sha256(data).hexdigest() == HASHES[name], name
    return data.decode('utf-8').splitlines()

def fields(line):
    return [s.strip() for s in line.split('#')[0].split(';')]

def bounds(text):
    parts = text.split('..')
    return int(parts[0], 16), int(parts[-1], 16)

def property_values(name, names, shift):
    source = lines(name); result = bytearray(0x110000)
    records = [s.split('@missing:', 1)[1].strip() for s in source if '@missing:' in s]
    records += [s for s in source if s.split('#')[0].strip()]
    for record in records:
        region, value, *_ = fields(record)
        value = ALIASES.get(value, value)
        if value == 'Non_Joining': value = 'U'
        lo, hi = bounds(region)
        result[lo:hi+1] = bytes([names.index(value) << shift]) * (hi-lo+1)
    return result

def constants(values, suffix):
    return '\n'.join('        ' + ', '.join(f'0x{x:X}{suffix}' for x in values[i:i+8]) + ',' for i in range(0, len(values), 8))

bidi = property_values('DerivedBidiClass.txt', BIDI, 0)
joining = property_values('DerivedJoiningType.txt', JOIN, 5)
scripts = bytearray([5]) * 0x110000
script_names = ['Common', 'Inherited', 'Latin', 'Hebrew', 'Arabic']
for line in lines('Scripts.txt'):
    if not line.split('#')[0].strip(): continue
    region, name, *_ = fields(line); lo, hi = bounds(region)
    value = script_names.index(name) if name in script_names else 5
    scripts[lo:hi+1] = bytes([value]) * (hi-lo+1)
transitions = []; previous = -1
for scalar in range(0x110000):
    value = bidi[scalar] | joining[scalar] | scripts[scalar] << 8
    if value != previous: transitions.append(scalar | (value << 21)); previous = value
brackets = []
for line in lines('BidiBrackets.txt'):
    if not line.split('#')[0].strip(): continue
    scalar, partner, kind = fields(line)
    brackets.append(int(scalar, 16) | int(partner, 16) << 21 | (kind == 'o') << 42)
mirrors = []
for line in lines('BidiMirroring.txt'):
    if not line.split('#')[0].strip(): continue
    scalar, partner, *_ = fields(line)
    mirrors.append(int(scalar, 16) | int(partner, 16) << 21)
brackets.sort(key=lambda x: x & 0x1FFFFF); mirrors.sort(key=lambda x: x & 0x1FFFFF)
mirrored = []
for line in lines('DerivedBinaryProperties.txt'):
    if not line.split('#')[0].strip(): continue
    region, name, *_ = fields(line)
    if name != 'Bidi_Mirrored': continue
    lo, hi = bounds(region); mirrored.extend(range(lo, hi+1))

code = '''using System.Text;

namespace Lokad.Image.Internal;

internal enum BidiClass : byte { BIDI }
internal enum JoiningType : byte { U, R, L, D, C, T }
internal enum TextScript : byte { Common, Inherited, Latin, Hebrew, Arabic, Other }

// Independent compact encoding of normative Unicode 17 property facts.
// https://www.unicode.org/reports/tr9/tr9-51.html
// https://www.unicode.org/Public/17.0.0/ucd/ (extracted property files)
// Reproduce with tools/TextReference/generate-unicode-properties.py; external
// specification inputs are ignored, pinned by hash and never build inputs.
// ReadOnlySpan constants reside in assembly data, without a managed cache owner.
internal static class UnicodeTextProperties
{
    private const uint ScalarMask = 0x1FFFFF;
    private static ReadOnlySpan<uint> Transitions =>
    [
TRANSITIONS
    ];
    private static ReadOnlySpan<ulong> Brackets =>
    [
BRACKETS
    ];
    private static ReadOnlySpan<ulong> Mirrors =>
    [
MIRRORS
    ];
    private static ReadOnlySpan<uint> MirroredScalars =>
    [
MIRRORED_SCALARS
    ];

    private static uint Properties(int scalar)
    {
        if (!Rune.IsValid(scalar)) throw new ArgumentOutOfRangeException(nameof(scalar));
        var ranges = Transitions; var low = 0; var high = ranges.Length - 1;
        while (low < high)
        {
            var mid = (low + high + 1) / 2;
            if ((ranges[mid] & ScalarMask) <= scalar) low = mid; else high = mid - 1;
        }
        return ranges[low] >> 21;
    }
    internal static BidiClass Bidi(int scalar) => (BidiClass)(Properties(scalar) & 31);
    internal static JoiningType Joining(int scalar) => (JoiningType)((Properties(scalar) >> 5) & 7);
    internal static TextScript Script(int scalar) => (TextScript)(Properties(scalar) >> 8);
    private static ulong Find(ReadOnlySpan<ulong> values, int scalar)
    {
        if (!Rune.IsValid(scalar)) throw new ArgumentOutOfRangeException(nameof(scalar));
        var low = 0; var high = values.Length - 1;
        while (low <= high)
        {
            var mid = (low + high) / 2; var key = (int)(values[mid] & ScalarMask);
            if (key == scalar) return values[mid];
            if (key < scalar) low = mid + 1; else high = mid - 1;
        }
        return 0;
    }
    internal static int Bracket(int scalar, out bool opening)
    {
        var value = Find(Brackets, scalar); opening = (value & (1UL << 42)) != 0;
        return value == 0 ? -1 : (int)((value >> 21) & ScalarMask);
    }
    internal static int Mirror(int scalar)
    {
        var value = Find(Mirrors, scalar);
        return value == 0 ? scalar : (int)((value >> 21) & ScalarMask);
    }
    internal static bool Mirrored(int scalar) => MirroredScalars.BinarySearch((uint)scalar) >= 0;
}
'''.replace('BIDI', ', '.join(BIDI)).replace('TRANSITIONS', constants(transitions, 'U')).replace('BRACKETS', constants(brackets, 'UL')).replace('MIRRORS', constants(mirrors, 'UL')).replace('MIRRORED_SCALARS', constants(mirrored, 'U'))
target = ROOT/'src/Lokad.Image/Internal/UnicodeTextProperties.cs'
parser = argparse.ArgumentParser(); parser.add_argument('--check', action='store_true'); args = parser.parse_args()
if args.check:
    assert target.read_text(encoding='utf-8') == code, 'Unicode property constants differ'
else:
    target.write_text(code, encoding='utf-8', newline='\n')
print(f'Unicode 17 facts verified: {len(transitions)} transitions, {len(brackets)} bracket mappings, {len(mirrors)} mirrors.')
