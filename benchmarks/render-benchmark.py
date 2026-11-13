"""Render current benchmarks from retained evidence; never measure."""
import json
import math
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def apply_current_resize_weights(rows, evidence, case_owners, bilinear_metadata):
    """Replace every displayed affected resize cost from one current owner."""
    mapping = {
        'Bicubic resize RGBA8 † → 256×129': ('cases', 'Down8'),
        'Bicubic upscale RGBA16 † → 1027×519': ('cases', 'Up16'),
        'Bicubic upscale RGBA8, default workspace † → 1027×519': ('cases', 'TightCross8'),
        'Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519': ('cases', 'TightCross16'),
        'Bicubic RGBA8, 32×16 / input+output+4 KiB † → 32×16': ('cases', 'Tight32x16_16'),
        'Bicubic RGBA8, 32×16 / default workspace † → 32×16': ('cases', 'Tight2x9_8'),
        'Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519': ('cases', 'TightUp8'),
        'Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259': ('cases', 'TightHorizontalUp8'),
        'Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519': ('cases', 'TightVerticalUp8'),
        'Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129': ('cases', 'TightDown8'),
        'Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129': ('cases', 'TightDown16'),
        'Bicubic RGBA8, fresh managed / prepared zimg † → 128×64': ('cases', 'Wide8'),
        'Bicubic RGBA8, width up / height down, input+output+4 KiB † → 1027×129': ('cases', 'TightMixed8'),
        'Bicubic RGBA16, width up / height down, input+output+4 KiB † → 1027×129': ('cases', 'TightMixed16'),
        'Bicubic RGBA8 reduction, input+output+4 KiB † → 64×32': ('cases', 'Tight2x9_16'),
        'Bicubic RGBA16 reduction, input+output+4 KiB † → 64×32': ('cases', 'Tight64x32_16'),
        'Bicubic RGBA8 reduction, input+output+4 KiB † → 128×64': ('cases', 'TightWide8'),
        'Bicubic RGBA16 reduction, input+output+4 KiB † → 128×64': ('cases', 'TightWide16'),
        'Bicubic RGBA8, width down / height up, default workspace † → 256×519': ('cases', 'Cross8'),
        'Bicubic RGBA16, width down / height up, default workspace † → 256×519': ('cases', 'Cross16'),
        'Bilinear upscale RGBA8 † → 1027×519': ('bilinear', 'Up8'),
        'Bilinear resize RGBA8 † → 256×129': ('bilinear', 'Down8'),
        'Bilinear resize RGBA16 † → 256×129': ('bilinear', 'Down16'),
        'Bicubic RGBA16 height reduction, input+output+4 KiB † → 513×129': ('cases', 'TightHorizontal8'),
    }
    for family, adjective, size in [('wide', '', '128×64'), ('narrow', ' narrow', '256×129')]:
        for depth in (8, 16):
            mapping[f'Bicubic one-shot{adjective} RGBA{depth} / zimg one-shot † → {size}'] = ('prepared', f'{family}/OneShot{depth}')
            mapping[f'Bicubic prepared{adjective} RGBA{depth} / zimg prepared † → {size}'] = ('prepared', f'{family}/Repeated{depth}')
    assert len(mapping) == 32
    seen = set()
    result = []
    for kind, row in rows:
        label = row.split(' | ')[0].removeprefix('| ')
        label = re.sub(r' \((?:parity confirmed|confidence open|pass|gap|uncertain)\)', '', label)
        if kind != 'editing' or label not in mapping:
            result.append((kind, row)); continue
        field, case = mapping[label]
        owner, view = case_owners.get(case, (evidence, 'base')) if field == 'cases' else (evidence, 'base')
        scoped = view == 'cross'
        generated = view == 'generated'
        deep = view == 'deep'
        metadata = field == 'bilinear' and case in bilinear_metadata['cases']
        if metadata: owner = bilinear_metadata
        current = owner['cases'][case] if metadata else owner[field][case]
        if scoped and case in {'TightCross8', 'TightHorizontal8'}:
            size = '256×519' if case == 'TightCross8' else '256×259'
            label = f'Bicubic RGBA8, width reduction / ' + ('height enlargement' if case == 'TightCross8' else 'same height') + f', input+output+4 KiB † → {size}'
        if generated:
            label = f"Bicubic RGBA{current['depth']} extreme reduction, input+output+4 KiB † → {current['outputWidth']}×{current['outputHeight']}"
        if deep:
            label = 'Bicubic RGBA16 reduction, input+output+4 KiB † → 32×16'
        assert (field, case) not in seen; seen.add((field, case))
        assert current['actualIterations'] == 12 and current['allActualsRetained']
        assert current['librarySha256'] == owner['qualification']['actualPackage']['librarySha256']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
        assert math.isclose(current['ratio'], current['managedUs'] / current['nativeUs'], rel_tol=1e-12)
        if current['status'] != 'pass': label = label.replace(' † →', f" ({current['status']}) † →")
        result.append((kind, f"| {label} | {current['managedUs']:,.2f} | {current['nativeUs']:,.2f} | {current['ratio']:.3f} / {current['upper']:.3f} | {current['allocatedBytes']:,} |"))
    assert len(seen) == 32
    return result


def apply_current_jpeg_caller_confidence(codecs):
    current = json.loads((ROOT / 'benchmarks/jpeg-caller-current-confidence-evidence.json').read_text(encoding='utf-8'))
    c, q, m = current['case'], current['qualification'], current['measurement']
    assert current['measurementAccepted'] and not current['productionChanged'] and not current['productionSpeedupClaim']
    assert current['requiredMaterialPaths'] == 289 and current['newMaterialPaths'] == 0
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified']
    assert q['productionAndActualPackageByteIdentical'] and q['fullChecksEachOs'] == 6600
    assert q['fullScalarIsaActualConsumerAndThreeExamplesReused'] and q['broadOrPackageRepeats'] == 0
    assert c['librarySha256'] == q['actualPackageLibrarySha256'] == current['producers']['timedArtifactSha256']
    assert m['pairs'] == m['nativeSamplesEachBracket'] == 12 and m['allActualsRetained'] and m['operationsEachSubject'] == 64
    assert c['allOriginalGatesPassed'] and c['quietGuardsPassed'] and c['sourceUnchanged'] and c['independentOwnedOutput']
    assert c['completePacketExact'] and c['metadataExceptionBytes'] == 2 and c['status'] == 'uncertain' and c['lower'] <= 1 < c['upper']
    assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    prefix = '| JPEG 4:2:0 from caller RGB samples'
    found = [line for line in codecs.splitlines() if line.startswith(prefix)]
    assert len(found) == 1
    return codecs.replace(found[0], f'| JPEG 4:2:0 from caller RGB samples (confidence open) † | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | {c["managedPacketBytes"]:,} / {c["nativePacketBytes"]:,} |')


def apply_current_cmyk_confidence(codecs, read_rows):
    """Display accepted immutable-build costs, independent of rejected experiments."""
    current = json.loads((ROOT / 'benchmarks/cmyk-current-owned-confidence-evidence.json').read_text(encoding='utf-8'))
    q = current['qualification']
    assert current['measurementAccepted'] and not current['productionChanged']
    assert not current['productionSpeedupClaim'] and current['requiredMaterialPaths'] == 289
    assert current['newMaterialPaths'] == current['measurementBuilds'] == current['broadOrPackageRepeatsForRestoration'] == 0
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified']
    assert q['productionAndActualPackageByteIdentical'] and q['fullChecksEachOs'] == 6600
    assert q['fullScalarIsaActualConsumerAndThreeExamplesReused']
    assert q['actualPackageSha256'] == current['producers']['actualPackageSha256']
    for c in current['cases'].values():
        assert c['librarySha256'] == q['actualPackageLibrarySha256'] == current['producers']['timedArtifactSha256']
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['allActualsRetained']
        assert c['managedExact'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['metadata']['sourceUnchanged'] and c['metadata']['independentOwnedOutput']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    c = current['cases']['CmykAdobeAc']
    prefix = '| JPEG horizontal-only CMYK, resident decode'
    found = [line for line in codecs.splitlines() if line.startswith(prefix)]
    assert len(found) == 1 and c['status'] == 'gap'
    codecs = codecs.replace(found[0], f'| JPEG horizontal-only CMYK, resident decode (gap) † | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | — |')
    c = current['cases']['CmykAdobeAcFresh']
    indices = [i for i, line in enumerate(read_rows) if line.startswith('| JPEG Adobe CMYK horizontal AC / FFmpeg fresh contexts')]
    assert len(indices) == 1 and c['status'] == 'pass'
    read_rows[indices[0]] = f'| JPEG Adobe CMYK horizontal AC / FFmpeg fresh contexts † | {c["managedUs"]:,.3f} | {c["nativeUs"]:,.3f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | Confirmed pass |'
    c = current['cases']['CmykAdobeSparse2D']
    assert c['raw']['mirrored'] and c['raw']['allMembersVerified'] and c['status'] == 'uncertain'
    prefix = '| JPEG CMYK sparse-2D, resident decode'
    found = [line for line in codecs.splitlines() if line.startswith(prefix)]
    assert len(found) == 1
    codecs = codecs.replace(found[0], f'| JPEG CMYK sparse-2D, resident decode (uncertain) † | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | — |')
    return codecs, read_rows


def apply_released_image_storage(codecs, editing, read_rows):
    """Refresh affected displayed costs from the qualified current owner."""
    current = json.loads((ROOT / 'benchmarks/released-image-storage-evidence.json').read_text(encoding='utf-8'))
    q = current['qualification']
    assert current['productionAccepted'] and current['allAffectedPerformanceReconciled']
    assert current['requiredMaterialPaths'] == 289 and len(current['cases']) == 191
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified']
    assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total'] == 6538
    assert q['actualPackage']['sha256'] == q['samePackageLinux']['sameActualPackageSha256']
    assert q['actualPackage']['librarySha256'] == current['producers']['timedArtifactSha256']
    assert q['actualPackage']['actualConsumerPassed'] and q['samePackageLinux']['actualConsumerPassed']
    assert q['actualPackage']['usageExamples'] == q['samePackageLinux']['usageExamples'] == 3
    mapping = {'Bicubic RGBA16 extreme reduction, input+output+4 KiB † → 2×9': 'bicubic/Tight2x9_16',
     'Bicubic RGBA16 reduction, input+output+4 KiB † → 128×64': 'bicubic/TightWide16',
     'Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129': 'bicubic/TightDown16',
     'Bicubic RGBA16 reduction, input+output+4 KiB † → 32×16': 'bicubic/Tight32x16_16',
     'Bicubic RGBA16 reduction, input+output+4 KiB † → 64×32': 'bicubic/Tight64x32_16',
     'Bicubic RGBA16, width down / height up / input+output+4 KiB † → 256×519': 'bicubic/TightCross16',
     'Bicubic RGBA16, width down / height up, default workspace † → 256×519': 'bicubic/Cross16',
     'Bicubic RGBA16, width up / height down, input+output+4 KiB (uncertain) † → 1027×129': 'bicubic/TightMixed16',
     'Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519': 'bicubic/TightUp8',
     'Bicubic RGBA8 extreme reduction, input+output+4 KiB † → 2×9': 'bicubic/Tight2x9_8',
     'Bicubic RGBA8 height enlargement, input+output+4 KiB † → 513×519': 'bicubic/TightVerticalUp8',
     'Bicubic RGBA8 reduction, input+output+4 KiB † → 128×64': 'bicubic/TightWide8',
     'Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129': 'bicubic/TightDown8',
     'Bicubic RGBA8 width enlargement, input+output+4 KiB † → 1027×259': 'bicubic/TightHorizontalUp8',
     'Bicubic RGBA8, fresh managed / prepared zimg (uncertain) † → 128×64': 'bicubic/Wide8',
     'Bicubic RGBA8, width down / height up, default workspace † → 256×519': 'bicubic/Cross8',
     'Bicubic RGBA8, width reduction / height enlargement, input+output+4 KiB † → 256×519': 'bicubic/TightCross8',
     'Bicubic RGBA8, width reduction / same height, input+output+4 KiB † → 256×259': 'bicubic/TightHorizontal8',
     'Bicubic RGBA8, width up / height down, input+output+4 KiB (uncertain) † → 1027×129': 'bicubic/TightMixed8',
     'Bicubic one-shot RGBA16 / zimg one-shot † → 128×64': 'preparedWide/OneShot16',
     'Bicubic one-shot RGBA8 / zimg one-shot † → 128×64': 'preparedWide/OneShot8',
     'Bicubic one-shot narrow RGBA16 / zimg one-shot † → 256×129': 'preparedNarrow/OneShot16',
     'Bicubic one-shot narrow RGBA8 / zimg one-shot † → 256×129': 'preparedNarrow/OneShot8',
     'Bicubic prepared RGBA16 / zimg prepared † → 128×64': 'preparedWide/Repeated16',
     'Bicubic prepared RGBA8 / zimg prepared † → 128×64': 'preparedWide/Repeated8',
     'Bicubic prepared narrow RGBA16 / zimg prepared † → 256×129': 'preparedNarrow/Repeated16',
     'Bicubic prepared narrow RGBA8 / zimg prepared † → 256×129': 'preparedNarrow/Repeated8',
     'Bicubic resize RGBA8 (uncertain) † → 256×129': 'bicubic/Down8',
     'Bicubic upscale RGBA16 † → 1027×519': 'bicubic/Up16',
     'Bilinear resize RGBA16 † → 256×129': 'bilinear/Down16',
     'Bilinear resize RGBA8 † → 256×129': 'bilinear/Down8',
     'Bilinear upscale RGBA8 † → 1027×519': 'bilinear/Up8',
     'JPEG 4:1:0, resident decode (confirmed pass) †': 'jpeg/Decode410',
     'JPEG 4:1:1, resident decode (confirmed pass) †': 'jpeg/Decode411',
     'JPEG Adobe CMYK horizontal AC / FFmpeg fresh contexts †': 'jpeg/CmykAdobeAcFresh',
     'JPEG CMYK sparse-2D, resident decode (confirmed pass) †': 'jpeg/CmykAdobeSparse2D',
     'JPEG YCCK420, resident decode (confirmed pass) †': 'jpeg/Ycck420',
     'JPEG baseline420, resident decode (confirmed pass) †': 'jpeg/NativeDecode420',
     'JPEG dense CMYK, resident decode (confirmed pass) †': 'jpeg/CmykAdobeGeneral',
     'JPEG horizontal-only CMYK, resident decode (confidence open) †': 'jpeg/CmykAdobeAc',
     'JPEG ordinary progressive444, resident decode (confirmed pass) †': 'jpeg/Progressive444',
     'JPEG progressive grayscale, resident decode (confirmed pass) †': 'jpeg/ProgressiveGray',
     'JPEG progressive420, resident decode (confirmed pass) †': 'jpeg/Progressive420',
     'JPEG progressive422, resident decode (confirmed pass) †': 'jpeg/Progressive422',
     'JPEG progressive444, live-token resident decode (confirmed pass) †': 'jpeg/ProgressiveCancelable444',
     'Lanczos3 one-shot RGBA16 / zimg one-shot † → 1027×519': 'lanczos/OneShotEnlarge16',
     'Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129': 'lanczos/OneShot16',
     'Lanczos3 one-shot RGBA8 / zimg one-shot † → 1027×519': 'lanczos/OneShotEnlarge8',
     'Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129': 'lanczos/OneShot8',
     'Lanczos3 prepared RGBA16 / zimg prepared † → 1027×519': 'lanczos/RepeatedEnlarge16',
     'Lanczos3 prepared RGBA16 / zimg prepared † → 256×129': 'lanczos/Repeated16',
     'Lanczos3 prepared RGBA8 / zimg prepared † → 1027×519': 'lanczos/RepeatedEnlarge8',
     'Lanczos3 prepared RGBA8 / zimg prepared † → 256×129': 'lanczos/Repeated8',
     'PNG RGBA16 decode, tight working limit †': 'png/DecodeTight16',
     'PNG RGBA8 decode, live token †': 'png/DecodeCancelable8',
     'PNG RGBA8 decode, tight working limit †': 'png/DecodeTight8',
     'VP8L RGBA8, fresh seekable Stream decode †': 'webp/WebpStream',
     'VP8L RGBA8, resident decode †': 'webp/WebpResident',
     'VP8L palette16 RGBA8, resident decode (confirmed pass) †': 'webp/IndexedNative16Colors8',
     'Validate VP8+alpha, span †': 'webp/ValidateVp8Alpha',
     'Validate VP8L+alpha, span †': 'webp/ValidateVp8lAlpha',
     'WebP VP8 + alpha, fresh seekable Stream (parity confirmed) †': 'webp/Vp8Stream'}
    seen = set()
    def update(lines, read=False):
        result = []
        for line in lines:
            cells = [c.strip() for c in line.split('|')[1:-1]]
            if not cells or cells[0] not in mapping:
                result.append(line); continue
            label = cells[0]; key = mapping[label]; c = current['cases'][key]
            assert label not in seen; seen.add(label)
            assert c['allOriginalGatesPassed'] and c['quietGuardsPassed'] and c['allActualsRetained']
            assert c['actualIterations'] == 12 and c['librarySha256'] == current['producers']['timedArtifactSha256']
            assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
            label = re.sub(r' \((?:uncertain|confidence open|parity confirmed|confirmed pass|confirmed gap|gap|pass)\)', '', label)
            if not read and c['status'] != 'pass':
                label = label.replace(' †', f" ({c['status']}) †") if ' †' in label else label + f" ({c['status']})"
            digits = 3 if read else 2
            cells[:5] = [label, f"{c['managedUs']:,.{digits}f}", f"{c['nativeUs']:,.{digits}f}",
                         f"{c['ratio']:.3f} / {c['upper']:.3f}", f"{c['allocatedBytes']:,}"]
            if read: cells[-1] = {'pass': 'Confirmed pass', 'gap': 'Confirmed gap', 'uncertain': 'Confidence open'}[c['status']]
            result.append('| ' + ' | '.join(cells) + ' |')
        return result
    codecs = '\n'.join(update(codecs.splitlines()))
    editing = '\n'.join(update(editing.splitlines()))
    read_rows = update(read_rows, read=True)
    assert seen == set(mapping), sorted(set(mapping) - seen)
    return codecs, editing, read_rows, current


def apply_mixed_word_cache(editing):
    current = json.loads((ROOT / 'benchmarks/mixed-word-packed-cache-evidence.json').read_text(encoding='utf-8'))
    q = current['qualification']
    assert current['productionAccepted'] and current['allAffectedPerformanceReconciled']
    assert current['requiredMaterialPaths'] == 289 and current['newMaterialPaths'] == 0
    assert current['currentImageOwnerViewStatuses'] == {'pass': 163, 'gap': 12, 'uncertain': 16}
    assert current['currentSeventyThreeResizeStatuses'] == {'pass': 67, 'gap': 1, 'uncertain': 5}
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified']
    assert q['windows']['total'] == q['linux']['total'] == q['fullChecksEachOs'] == 6551
    assert q['actualPackage']['sha256'] == q['samePackageWindows']['sameActualPackageSha256']
    assert q['actualPackage']['actualConsumerPassed'] and q['samePackageWindows']['actualConsumerPassed']
    assert q['actualPackage']['usageExamples'] == q['samePackageWindows']['usageExamples'] == 3
    c = current['cases']['TightMixed16']
    assert c['librarySha256'] == q['actualPackage']['librarySha256'] == current['producers']['timedArtifactSha256']
    assert c['status'] == 'pass' and c['upper'] <= 1
    assert c['managedExact'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
    assert c['actualIterations'] == 12 and c['allActualsRetained']
    assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    label = 'Bicubic RGBA16, width up / height down, input+output+4 KiB'
    rows, seen = [], 0
    for line in editing.splitlines():
        if line.startswith('| ' + label):
            seen += 1
            line = f"| {label} † → 1027×129 | {c['managedUs']:,.2f} | {c['nativeUs']:,.2f} | {c['ratio']:.3f} / {c['upper']:.3f} | {c['allocatedBytes']:,} |"
        rows.append(line)
    assert seen == 1
    return '\n'.join(rows)


def apply_lanczos_product_reuse(editing):
    current = json.loads((ROOT / 'benchmarks/lanczos-word-product-reuse-evidence.json').read_text(encoding='utf-8'))
    q = current['qualification']
    assert current['productionAccepted'] and current['allAffectedPerformanceReconciled']
    assert current['requiredMaterialPaths'] == 289 and current['newMaterialPaths'] == 0
    assert current['currentImageOwnerViewStatuses'] == {'pass': 164, 'gap': 11, 'uncertain': 16}
    assert current['currentEightLanczosStatus'] == {'pass': 7, 'gap': 0, 'uncertain': 1}
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified']
    assert q['windows']['total'] == q['linux']['total'] == q['fullChecksEachOs'] == 6573
    assert q['actualPackage']['sha256'] == q['samePackageWindows']['sameActualPackageSha256']
    assert q['actualPackage']['actualConsumerPassed'] and q['samePackageWindows']['actualConsumerPassed']
    assert q['actualPackage']['usageExamples'] == q['samePackageWindows']['usageExamples'] == 3
    mapping = {
        'Lanczos3 one-shot RGBA8 / zimg one-shot † → 256×129': 'OneShot8',
        'Lanczos3 prepared RGBA8 / zimg prepared † → 256×129': 'Repeated8',
        'Lanczos3 one-shot RGBA16 / zimg one-shot † → 256×129': 'OneShot16',
        'Lanczos3 prepared RGBA16 / zimg prepared † → 256×129': 'Repeated16',
        'Lanczos3 prepared RGBA16 / zimg prepared † → 1027×519': 'RepeatedEnlarge16',
    }
    seen, rows = set(), []
    for line in editing.splitlines():
        cells = [c.strip() for c in line.strip('|').split('|')]
        label = re.sub(r' \((?:gap|uncertain)\)', '', cells[0])
        if label in mapping:
            case = mapping[label]; c = current['cases'][case]
            assert case not in seen; seen.add(case)
            assert c['actualIterations'] == 12 and c['allActualsRetained']
            assert c['allOriginalGatesPassed'] and c['quietGuardsPassed'] and c['managedExact']
            assert c['librarySha256'] == q['actualPackage']['librarySha256'] == current['producers']['timedArtifactSha256']
            assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
            if c['status'] != 'pass': label = label.replace(' †', f" ({c['status']}) †")
            assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
            cells[:5] = [label, f"{c['managedUs']:,.2f}", f"{c['nativeUs']:,.2f}",
                         f"{c['ratio']:.3f} / {c['upper']:.3f}", f"{c['allocatedBytes']:,}"]
            line = '| ' + ' | '.join(cells) + ' |'
        rows.append(line)
    assert seen == set(mapping.values())
    return '\n'.join(rows), current


def apply_bounded_wide_quartet(editing):
    current = json.loads((ROOT / 'benchmarks/bounded-wide-horizontal-quartet-evidence.json').read_text(encoding='utf-8'))
    q = current['qualification']
    assert current['productionAccepted'] and current['allAffectedPerformanceReconciled']
    assert current['requiredMaterialPaths'] == 289 and current['newMaterialPaths'] == 0
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified']
    assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total'] == 6592
    assert q['actualPackage']['sha256'] == q['samePackageWindows']['sameActualPackageSha256']
    assert q['actualPackage']['actualConsumerPassed'] and q['samePackageWindows']['actualConsumerPassed']
    assert q['actualPackage']['usageExamples'] == q['samePackageWindows']['usageExamples'] == 3
    c = current['cases']['TightWide8']
    assert c['librarySha256'] == q['actualPackage']['librarySha256'] == current['producers']['timedArtifactSha256']
    assert c['managedExact'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
    assert c['actualIterations'] == 12 and c['allActualsRetained']
    assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
    assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    label = 'Bicubic RGBA8 reduction, input+output+4 KiB'
    rows, seen = [], 0
    for line in editing.splitlines():
        if line.startswith('| ' + label) and line.split('|')[1].strip().endswith('128×64'):
            seen += 1
            qualifier = '' if c['status'] == 'pass' else f" ({c['status']})"
            line = f"| {label}{qualifier} † → 128×64 | {c['managedUs']:,.2f} | {c['nativeUs']:,.2f} | {c['ratio']:.3f} / {c['upper']:.3f} | {c['allocatedBytes']:,} |"
        rows.append(line)
    assert seen == 1
    return '\n'.join(rows)


def render():
    # Check current measurement owners; rejected experiments retain their own
    # linked evidence without becoming inputs to every summary update.
    evidence = json.loads((ROOT / 'benchmarks/release-0.2.0-benchmark-evidence.json').read_text(encoding='utf-8'))
    assert evidence['version'] == '0.2.0' and len(evidence['cases']) == 31
    assert evidence['allDisplayedCasesRefreshed'] and not evidence['productionChanged']
    stored = json.loads((ROOT / 'benchmarks/warm-png-bulk-stored-evidence.json').read_text(encoding='utf-8'))
    current = json.loads((ROOT / 'benchmarks/warm-png-distance-symbol-evidence.json').read_text(encoding='utf-8'))
    vp8_current = json.loads((ROOT / 'benchmarks/vp8-rounded-border-prediction-evidence.json').read_text(encoding='utf-8'))
    assert vp8_current['productionAccepted'] and vp8_current['requiredMaterialPaths'] == 283
    assert vp8_current['raw']['mirrored'] and vp8_current['raw']['allMembersVerified']
    assert vp8_current['statusCounts'] == {state: sum(c['status'] == state for c in vp8_current['cases'].values())
                                         for state in ('pass', 'gap', 'uncertain')}
    assert set(vp8_current['cases']) == {'opaque8', 'opaque16', 'photograph', 'diagram', 'selected', 'trellis', 'distortion', 'alpha', 'alpha16'}
    for name, c in vp8_current['cases'].items():
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12
        assert c['librarySha256'] == (vp8_current['producers']['linuxSha256'] if name == 'distortion'
                                      else vp8_current['reuse']['originalLinuxSha256'])
        assert c['completePacketExact'] and c['packetSha256'] == c['nativePacketSha256']
        assert c['managedPacketBytes'] == c['nativePacketBytes']
        assert c['allOriginalGatesPassed'] and c['quietGuardsPassed'] and c['sourceUnchanged'] and c['independentOwnedOutput']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    proof = vp8_current['sourceProof']
    assert proof['sharedPredictionAndEncoderFrameAnalysisHistogramSourcesByteExact']
    assert proof['selectReconstructionAndUvBodiesExact'] and proof['originalPortablePredictionBodyPreserved']
    assert proof['perModeCancellationOrderAndStrictHeaderPrunePreserved']
    assert proof['actualCSharpPredictedSamples'] == 696320
    assert vp8_current['reuse']['unchangedCases'] == sorted(set(vp8_current['cases']) - {'distortion'})
    assert vp8_current['qualification']['fullChecksEachOs'] == 5939
    assert vp8_current['qualification']['sameActualPackageBothOs']
    assert vp8_current['qualification']['actualPackage']['sha256'] == vp8_current['qualification']['samePackageLinux']['sameActualPackageSha256']
    prepared_model = json.loads((ROOT / 'benchmarks/vp8-position-model-preparation-evidence.json').read_text(encoding='utf-8'))
    assert prepared_model['productionAccepted'] and prepared_model['requiredMaterialPaths'] == 289
    assert prepared_model['raw']['mirrored'] and prepared_model['raw']['allMembersVerified']
    assert prepared_model['allAffectedPerformanceReconciled'] and not prepared_model['materialPassToGapContradictions']
    assert set(prepared_model['cases']) == {'Native8', 'StreamNative8', 'Opaque8', 'SimpleOpaque8', 'OpaqueCancelable8', 'ValidateVp8Alpha'}
    vp8_costs = {('Vp8Stream' if name == 'StreamNative8' else name): cost for name, cost in prepared_model['cases'].items()}
    for cost in vp8_costs.values():
        assert cost['actualIterations'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['librarySha256'] == prepared_model['producers']['timedArtifactSha256']
        assert cost['allOriginalGatesPassed'] and cost['quietGuardsPassed']
        assert cost['sourceUnchanged'] and cost['independentOwnedOutput']
        if cost['caseId'] == 'ValidateVp8Alpha':
            assert cost['descriptorExact'] and cost['prefixAndCodingScopesVerified']
        else:
            assert cost['managedOutputSha256'] == cost['nativeOutputSha256'] and cost['alphaExact']
            assert cost['decoderMaximum'] == 0 and cost['managedOutputBytes'] == 531468
        assert cost['status'] == ('pass' if cost['upper'] <= 1 else 'gap' if cost['lower'] > 1 else 'uncertain')
        assert math.isclose(cost['ratio'], cost['managedUs'] / cost['nativeUs'], rel_tol=1e-12)
    assert all(prepared_model['sourceProof']['originalProductionSourcesReconstructedExactlyAfterRevertingOnlyModelPreparationAndAddressing'].values())
    assert prepared_model['storage']['modelPayloadBytes'] == 3072
    assert prepared_model['storage']['additionalModelArrayBytes'] == 1536
    assert prepared_model['storage']['conservativelySummedFixedBoundBytes'] <= prepared_model['storage']['coarseHeaderWorkspaceBytes'] == 8192
    assert prepared_model['storage']['newRetainedStateBytes'] == 0
    assert prepared_model['comparison']['Opaque8']['normalizedGainSeparated']
    assert all(v['rawGainSeparated'] for v in prepared_model['comparison']['Opaque8']['views'].values())
    q = prepared_model['qualification']
    assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total'] == 6247
    assert q['sameActualPackageBothOS'] and q['actualPackage']['sha256'] == q['samePackageLinux']['sameActualPackageSha256']
    live = vp8_costs['OpaqueCancelable8']
    assert all(v['rawGainSeparated'] for v in vp8_current['comparison']['views'].values())
    assert all(v['normalizedGainSeparated'] for v in vp8_current['comparison']['views'].values())
    assert vp8_current['storage']['newOwnedHeapCapacityBytes'] == vp8_current['storage']['newRetainedStateBytes'] == 0
    jpeg = json.loads((ROOT / 'benchmarks/warm-jpeg-column-plans-evidence.json').read_text(encoding='utf-8'))
    assert jpeg['qualification']['fullChecksEachOs'] == 5163 and jpeg['decision']['passed']
    assert jpeg['qualification']['package']['sha256'] == jpeg['qualification']['linuxPackage']['sameActualPackageSha256']
    assert jpeg['qualification']['governedStreamingConsumerPassed'] and jpeg['postcheckRecovery']['measurementReruns'] == 0
    assert jpeg['status'] == {'Rgb420': 'confirmed-gap', 'Rgba16': 'screen-gap'}
    vp8l = json.loads((ROOT / 'benchmarks/warm-vp8l-prepared-prediction-cost-evidence.json').read_text(encoding='utf-8'))
    reads = json.loads((ROOT / 'benchmarks/warm-image-read-evidence.json').read_text(encoding='utf-8'))
    vp8_validation = json.loads((ROOT / 'benchmarks/warm-vp8-direct-token-decisions-evidence.json').read_text(encoding='utf-8'))
    assert vp8_validation['productionChanged'] and vp8_validation['independentCounterbalancedBlock']
    assert vp8_validation['decision']['allGatesPassed'] and vp8_validation['qualification']['sameActualPackageBothOs']
    png_crc_scan = json.loads((ROOT / 'benchmarks/warm-png-scan-crc-reuse-evidence.json').read_text(encoding='utf-8'))
    assert png_crc_scan['productionChanged'] and png_crc_scan['decision']['passed']
    assert png_crc_scan['qualification']['sameActualPackageBothOs'] and len(png_crc_scan['cases']) == 5
    png_retained = json.loads((ROOT / 'benchmarks/current-png-retained-root-cost-evidence.json').read_text(encoding='utf-8'))
    assert not png_retained['productionChanged'] and png_retained['allBuilds'] == 0
    assert png_retained['producers']['linuxSha256'] == png_crc_scan['producers']['linuxSha256']
    png_root_rows = json.loads((ROOT / 'benchmarks/warm-png-caller-root-rows-evidence.json').read_text(encoding='utf-8'))
    assert png_root_rows['productionChanged'] and png_root_rows['decision']['passed']
    assert png_root_rows['qualification']['sameActualPackageBothOs']
    assert set(png_root_rows['cases']) == {'Png8Retained2', 'DecodeNoCompression16'}
    for case, observations in png_root_rows['cases'].items():
        root_observation = observations[-1]
        assert root_observation['receipt']['librarySha256'] == png_root_rows['producers']['linuxSha256']
        assert len(root_observation['managed']['actual']) == 12 and all(q['passed'] for q in root_observation['quietness'].values())
        assert (root_observation['conservativeRatioUpper'] <= 1) == (png_root_rows['status'][case] == 'confirmed-pass')
    png_rgba_costs = json.loads((ROOT / 'benchmarks/current-png-rgba-decode-costs-evidence.json').read_text(encoding='utf-8'))
    assert not png_rgba_costs['productionChanged'] and png_rgba_costs['productionHarnessNativeBuilds'] == 0
    assert png_rgba_costs['producers']['linuxSha256'] == png_root_rows['producers']['linuxSha256']
    assert set(png_rgba_costs['cases']) == {'Decode8', 'Decode16', 'DecodePaeth8', 'DecodePaeth16', 'DecodeAverage8', 'DecodeAverage16'}
    for case, observations in png_rgba_costs['cases'].items():
        rgba_observation = observations[-1]
        assert len(rgba_observation['managed']['actual']) == 12 and all(q['passed'] for q in rgba_observation['quietness'].values())
        assert rgba_observation['receipt']['librarySha256'] == png_rgba_costs['producers']['linuxSha256']
        rgba_status = png_rgba_costs['status'][case]
        if rgba_status == 'confirmed-pass':
            assert rgba_observation['conservativeRatioUpper'] <= 1
        elif rgba_status == 'confirmed-confidence-open':
            assert png_rgba_costs['comparisonLowerBounds'][case] <= 1 < rgba_observation['conservativeRatioUpper']
        else:
            assert rgba_status == 'confirmed-gap' and png_rgba_costs['comparisonLowerBounds'][case] > 1
    png_average = json.loads((ROOT / 'benchmarks/warm-png-average-simd-evidence.json').read_text(encoding='utf-8'))
    assert png_average['productionChanged'] and png_average['decision']['passed'] and png_average['secondaryDecision']['passed']
    assert png_average['qualification']['sameActualPackageBothOs'] and png_average['qualification']['fullChecksEachOs'] == 5088
    assert set(png_average['cases']) == {'DecodeAverage8', 'DecodeAverage16', 'ValidateIndexedAdam7'}
    for case, observations in png_average['cases'].items():
        average_observation = observations[-1]
        assert len(average_observation['managed']['actual']) == 12 and all(q['passed'] for q in average_observation['quietness'].values())
        assert average_observation['receipt']['librarySha256'] == png_average['producers']['linuxSha256']
        assert (average_observation['conservativeRatioUpper'] <= 1) == (case != 'ValidateIndexedAdam7')
    png_sub_up = json.loads((ROOT / 'benchmarks/current-png-sub-up-costs-evidence.json').read_text(encoding='utf-8'))
    assert not png_sub_up['productionChanged'] and png_sub_up['allBuilds'] == 0
    assert png_sub_up['producers']['linuxSha256'] == png_average['producers']['linuxSha256']
    assert set(png_sub_up['cases']) == {'DecodeSub8', 'DecodeSub16', 'DecodeUp8', 'DecodeUp16'}
    for case, observations in png_sub_up['cases'].items():
        sub_up_observation = observations[-1]
        assert png_sub_up['status'][case] == 'confirmed-pass' and sub_up_observation['conservativeRatioUpper'] <= 1
        assert len(sub_up_observation['managed']['actual']) == 12 and all(q['passed'] for q in sub_up_observation['quietness'].values())
        assert sub_up_observation['receipt']['librarySha256'] == png_sub_up['producers']['linuxSha256']
    png_word_costs = json.loads((ROOT / 'benchmarks/current-png-word-decode-costs-evidence.json').read_text(encoding='utf-8'))
    assert not png_word_costs['productionChanged'] and png_word_costs['allBuilds'] == 0
    assert png_word_costs['producers']['linuxSha256'] == png_average['producers']['linuxSha256']
    assert set(png_word_costs['cases']) == {'DecodeAdam7_16', 'DecodeRgb16', 'DecodeGrayAlpha16', 'DecodeGray16'}
    for case, observations in png_word_costs['cases'].items():
        word_observation = observations[-1]
        assert len(word_observation['managed']['actual']) == 12 and all(q['passed'] for q in word_observation['quietness'].values())
        assert word_observation['receipt']['librarySha256'] == png_word_costs['producers']['linuxSha256']
        assert (word_observation['conservativeRatioUpper'] <= 1) == (case != 'DecodeAdam7_16')
    png_adam7 = json.loads((ROOT / 'benchmarks/warm-png-adam7-framed-output-evidence.json').read_text(encoding='utf-8'))
    assert png_adam7['productionChanged'] and png_adam7['decision']['passed']
    assert png_adam7['qualification']['sameActualPackageBothOs'] and png_adam7['qualification']['fullChecksEachOs'] == 5094
    adam7_observation = png_adam7['cases']['DecodeAdam7_16'][-1]
    assert len(adam7_observation['managed']['actual']) == 12 and all(q['passed'] for q in adam7_observation['quietness'].values())
    assert adam7_observation['receipt']['librarySha256'] == png_adam7['producers']['linuxSha256']
    assert adam7_observation['conservativeRatioUpper'] <= 1
    png_byte_costs = json.loads((ROOT / 'benchmarks/current-png-byte-decode-costs-evidence.json').read_text(encoding='utf-8'))
    assert not png_byte_costs['productionChanged'] and png_byte_costs['allBuilds'] == 0
    assert png_byte_costs['producers']['linuxSha256'] == png_adam7['producers']['linuxSha256']
    assert set(png_byte_costs['cases']) == {'DecodeRgb8', 'DecodeGrayAlpha8', 'DecodeGray8', 'DecodeNoCompression8'}
    for case, observations in png_byte_costs['cases'].items():
        byte_observation = observations[-1]
        assert byte_observation['conservativeRatioUpper'] <= 1 and png_byte_costs['status'][case] == 'confirmed-pass'
        assert len(byte_observation['managed']['actual']) == 12 and all(q['passed'] for q in byte_observation['quietness'].values())
        assert byte_observation['receipt']['librarySha256'] == png_byte_costs['producers']['linuxSha256']
    png_packed = json.loads((ROOT / 'benchmarks/current-png-indexed-packed-costs-evidence.json').read_text(encoding='utf-8'))
    assert not png_packed['productionChanged'] and png_packed['allBuilds'] == 0
    assert png_packed['producers']['linuxSha256'] == png_adam7['producers']['linuxSha256']
    assert len(png_packed['cases']) == 8 and list(png_packed['status'].values()).count('confirmed-pass') == 7
    for case, observations in png_packed['cases'].items():
        packed_observation = observations[-1]
        assert len(packed_observation['managed']['actual']) == 12 and all(q['passed'] for q in packed_observation['quietness'].values())
        assert packed_observation['receipt']['librarySha256'] == png_packed['producers']['linuxSha256']
        assert (packed_observation['conservativeRatioUpper'] <= 1) == (png_packed['status'][case] == 'confirmed-pass')
    png_paeth_sources = json.loads((ROOT / 'benchmarks/current-png-paeth-source-costs-evidence.json').read_text(encoding='utf-8'))
    assert not png_paeth_sources['productionChanged'] and png_paeth_sources['allBuilds'] == 0
    assert png_paeth_sources['producers']['linuxSha256'] == png_adam7['producers']['linuxSha256']
    assert len(png_paeth_sources['cases']) == 4 and list(png_paeth_sources['status'].values()).count('confirmed-pass') == 3
    for case, observations in png_paeth_sources['cases'].items():
        o = observations[-1]
        assert len(o['managed']['actual']) == 12 and all(q['passed'] for q in o['quietness'].values())
        assert o['receipt']['librarySha256'] == png_paeth_sources['producers']['linuxSha256']
        assert (o['conservativeRatioUpper'] <= 1) == (png_paeth_sources['status'][case] == 'confirmed-pass')
    png_rgb_paeth = json.loads((ROOT / 'benchmarks/warm-png-rgb8-paeth-simd-evidence.json').read_text(encoding='utf-8'))
    assert png_rgb_paeth['productionChanged'] and png_rgb_paeth['decision']['passed']
    o = png_rgb_paeth['cases']['DecodeRgbPaeth8'][-1]
    assert len(o['managed']['actual']) == 12 and all(q['passed'] for q in o['quietness'].values())
    assert o['conservativeRatioUpper'] <= 1 and o['managedMemory']['BytesAllocatedPerOperation'] == 3448
    assert o['receipt']['librarySha256'] == png_rgb_paeth['producers']['linuxSha256']
    assert png_rgb_paeth['qualification']['fullChecksEachOs'] == 5107
    assert png_rgb_paeth['qualification']['package']['sha256'] == png_rgb_paeth['qualification']['linuxPackage']['sameActualPackageSha256']
    png_keys = json.loads((ROOT / 'benchmarks/current-png-transparency-costs-evidence.json').read_text(encoding='utf-8'))
    assert not png_keys['productionChanged'] and png_keys['allBuilds'] == 0
    assert png_keys['producers']['linuxSha256'] == png_rgb_paeth['producers']['linuxSha256']
    assert len(png_keys['cases']) == 4
    for case, observations in png_keys['cases'].items():
        o = observations[-1]
        assert len(o['managed']['actual']) == 12 and all(q['passed'] for q in o['quietness'].values())
        assert o['receipt']['librarySha256'] == png_keys['producers']['linuxSha256']
        assert (o['conservativeRatioUpper'] <= 1) == (png_keys['status'][case] == 'confirmed-pass')
    png_gray_key = json.loads((ROOT / 'benchmarks/warm-png-gray16-key-row-evidence.json').read_text(encoding='utf-8'))
    assert png_gray_key['productionChanged'] and png_gray_key['decision']['passed']
    o = png_gray_key['cases']['DecodeGray16Transparency'][-1]
    assert len(o['managed']['actual']) == 12 and all(q['passed'] for q in o['quietness'].values())
    assert o['conservativeRatioUpper'] <= 1 and o['managedMemory']['BytesAllocatedPerOperation'] == 2456
    assert o['receipt']['librarySha256'] == png_gray_key['producers']['linuxSha256']
    assert png_gray_key['qualification']['fullChecksEachOs'] == 5129
    assert png_gray_key['qualification']['package']['sha256'] == png_gray_key['qualification']['linuxPackage']['sameActualPackageSha256']
    png_palette_boundaries = json.loads((ROOT / 'benchmarks/current-png-palette-boundary-costs-evidence.json').read_text(encoding='utf-8'))
    assert not png_palette_boundaries['productionChanged'] and png_palette_boundaries['allBuilds'] == 0
    assert png_palette_boundaries['producers']['linuxSha256'] == png_gray_key['producers']['linuxSha256']
    assert len(png_palette_boundaries['cases']) == 4
    for case, observations in png_palette_boundaries['cases'].items():
        o = observations[-1]
        assert len(o['managed']['actual']) == 12 and all(q['passed'] for q in o['quietness'].values())
        assert o['receipt']['librarySha256'] == png_palette_boundaries['producers']['linuxSha256']
        assert (o['conservativeRatioUpper'] <= 1) == (png_palette_boundaries['status'][case] == 'confirmed-pass')
    png_policies = json.loads((ROOT / 'benchmarks/current-png-policy-packed-key-costs-evidence.json').read_text(encoding='utf-8'))
    assert not png_policies['productionChanged'] and png_policies['allBuilds'] == 0
    assert png_policies['producers']['linuxSha256'] == png_gray_key['producers']['linuxSha256']
    assert len(png_policies['cases']) == 8
    for case, observations in png_policies['cases'].items():
        o = observations[-1]
        assert len(o['managed']['actual']) == 12 and all(q['passed'] for q in o['quietness'].values())
        assert o['receipt']['librarySha256'] == png_policies['producers']['linuxSha256']
        assert (o['conservativeRatioUpper'] <= 1) == (png_policies['status'][case] == 'confirmed-pass')
    png_stream_costs = json.loads((ROOT / 'benchmarks/current-png-stream-composition-costs-evidence.json').read_text(encoding='utf-8'))
    assert not png_stream_costs['productionChanged'] and png_stream_costs['coreBuilds'] == png_stream_costs['selectorNativeBuilds'] == 0
    assert png_stream_costs['harnessBuilds'] == 1 and png_stream_costs['remainingAffectedCosts'] == 0
    assert png_stream_costs['producers']['linuxSha256'] == png_gray_key['producers']['linuxSha256']
    assert len(png_stream_costs['cases']) == 5 and png_stream_costs['defaultProofRestored']['restoredByteExact']
    for case, observations in png_stream_costs['cases'].items():
        o = observations[-1]
        assert len(o['managed']['actual']) == 12 and all(q['passed'] for q in o['quietness'].values())
        assert o['receipt']['librarySha256'] == png_stream_costs['producers']['linuxSha256']
        assert (o['conservativeRatioUpper'] <= 1) == (png_stream_costs['status'][case] == 'confirmed-pass')
    png_cancel_cursor = json.loads((ROOT / 'benchmarks/warm-png-cancelable-row-cursor-evidence.json').read_text(encoding='utf-8'))
    assert png_cancel_cursor['productionChanged'] and png_cancel_cursor['decision']['passed']
    assert png_cancel_cursor['qualification']['fullChecksEachOs'] == 5132
    assert png_cancel_cursor['qualification']['package']['sha256'] == png_cancel_cursor['qualification']['linuxPackage']['sameActualPackageSha256']
    assert png_cancel_cursor['status'] == {'DecodeTightCancelable8': 'confirmed-gap', 'DecodeTightCancelable16': 'confirmed-pass'}
    for case, observations in png_cancel_cursor['cases'].items():
        o = observations[-1]
        assert len(o['managed']['actual']) == 12 and all(q['passed'] for q in o['quietness'].values())
        assert o['receipt']['librarySha256'] == png_cancel_cursor['producers']['linuxSha256']
        assert (o['conservativeRatioUpper'] <= 1) == (png_cancel_cursor['status'][case] == 'confirmed-pass')
    png_sparse_palette = json.loads((ROOT / 'benchmarks/warm-png-sparse-palette-expansion-evidence.json').read_text(encoding='utf-8'))
    assert png_sparse_palette['productionChanged'] and png_sparse_palette['decision']['passed'] and png_sparse_palette['secondaryDecision']['passed']
    assert png_sparse_palette['qualification']['fullChecksEachOs'] == 5153
    assert png_sparse_palette['qualification']['package']['sha256'] == png_sparse_palette['qualification']['linuxPackage']['sameActualPackageSha256']
    assert png_sparse_palette['status'] == {'DecodePalette8Adam7': 'confirmed-pass', 'DecodePalette4Adam7': 'confirmed-pass', 'DecodeGray1Adam7': 'confirmed-gap'}
    for case, observations in png_sparse_palette['cases'].items():
        o = observations[-1]
        assert len(o['managed']['actual']) == 12 and all(q['passed'] for q in o['quietness'].values())
        assert o['receipt']['librarySha256'] == png_sparse_palette['producers']['linuxSha256']
        assert (o['conservativeRatioUpper'] <= 1) == (png_sparse_palette['status'][case] == 'confirmed-pass')
    png_sparse_gray = json.loads((ROOT / 'benchmarks/warm-png-sparse-gray-expansion-evidence.json').read_text(encoding='utf-8'))
    assert png_sparse_gray['productionChanged'] and png_sparse_gray['decision']['passed']
    assert png_sparse_gray['qualification']['fullChecksEachOs'] == 5157
    assert png_sparse_gray['qualification']['package']['sha256'] == png_sparse_gray['qualification']['linuxPackage']['sameActualPackageSha256']
    refresh = png_sparse_gray['packageDocumentationConsumerRefresh']
    assert refresh['productionUnchanged'] and refresh['sameActualPackagePassedWindowsLinux']
    assert refresh['librarySha256'] == png_sparse_gray['qualification']['package']['librarySha256']
    assert refresh['governedStreamingConsumerPassed'] and refresh['usageExamplesEachOs'] == 3
    assert png_sparse_gray['status'] == {'DecodeGray1Adam7': 'confirmed-pass'}
    o = png_sparse_gray['cases']['DecodeGray1Adam7'][-1]
    assert len(o['managed']['actual']) == 12 and all(q['passed'] for q in o['quietness'].values())
    assert o['receipt']['librarySha256'] == png_sparse_gray['producers']['linuxSha256']
    assert o['conservativeRatioUpper'] <= 1 and o['managedMemory']['BytesAllocatedPerOperation'] == 1440
    png_small_lookup = json.loads((ROOT / 'benchmarks/warm-png-small-palette-lookup-evidence.json').read_text(encoding='utf-8'))
    assert png_small_lookup['productionChanged'] and png_small_lookup['decision']['passed']
    q = png_small_lookup['qualification']
    assert q['fullChecksEachOs'] == 5158 and q['focusedInternalEachModeEachOs'] == 72
    assert q['package']['sha256'] == q['linuxPackage']['sameActualPackageSha256']
    assert q['governedStreamingConsumerPassed'] and q['usageExamplesEachOs'] == 3
    assert png_small_lookup['status'] == {'DecodePalette8Short': 'confirmed-pass'}
    o = png_small_lookup['cases']['DecodePalette8Short'][-1]
    assert len(o['managed']['actual']) == 12 and all(g['passed'] for g in o['quietness'].values())
    assert o['receipt']['librarySha256'] == png_small_lookup['producers']['linuxSha256']
    assert o['conservativeRatioUpper'] <= 1 and o['managedMemory']['BytesAllocatedPerOperation'] == 1512
    cmyk_contract = json.loads((ROOT / 'benchmarks/cmyk-fixed-device-benchmark-contract-evidence.json').read_text(encoding='utf-8'))
    assert not cmyk_contract['productionChanged'] and cmyk_contract['scoredActuals'] == 0
    assert not cmyk_contract['performanceClaim'] and cmyk_contract['status'] == 'resolved-by-fixed-oracle-cost'
    assert all(v['equalsDeclaredFixedDeviceOracle'] and v['equalsFrozenNative'] for v in cmyk_contract['results'].values())
    cmyk_current = json.loads((ROOT / 'benchmarks/warm-jpeg-direct-baseline-planes-evidence.json').read_text(encoding='utf-8'))
    assert cmyk_current['productionChanged'] and cmyk_current['independentQuietConfirmation']
    assert all(cmyk_current['status'][case] == 'confirmed-gap' for case in ['CmykCaller', 'CmykAdobeSparse2D', 'NativeDecode420'])
    assert cmyk_current['status']['ValidateJpegBaseline420'] == 'confirmed-pass'
    assert cmyk_current['decision']['passed'] and cmyk_current['qualification']['fullChecksEachOs'] == 5178
    assert cmyk_current['qualification']['package']['sha256'] == cmyk_current['qualification']['linuxPackage']['sameActualPackageSha256']
    assert cmyk_current['fixtureCorrection']['encodedPacketRasterProjectionBlocksReferenceSettingsUnchanged']
    assert cmyk_current['fixtureCorrection']['proofOverlayRestoredByteExact'] and cmyk_current['fixtureCorrection']['acceptedDerivedOraclesInstalled']
    extended_jpeg = json.loads((ROOT / 'benchmarks/warm-extended-jpeg-current-costs-evidence.json').read_text(encoding='utf-8'))
    assert not extended_jpeg['productionChanged'] and extended_jpeg['independentQuietConfirmation']
    assert extended_jpeg['producers'] == cmyk_current['producers']
    assert extended_jpeg['qualification']['package']['sha256'] == cmyk_current['qualification']['package']['sha256']
    assert all(status == 'confirmed-gap' for status in extended_jpeg['status'].values())
    ycck_rows = json.loads((ROOT / 'benchmarks/warm-ycck-row-tiles-evidence.json').read_text(encoding='utf-8'))
    assert ycck_rows['productionChanged'] and ycck_rows['independentQuietConfirmation']
    assert ycck_rows['decision']['passed'] and ycck_rows['status']['Ycck420'] == 'confirmed-pass'
    assert ycck_rows['qualification']['fullChecksEachOs'] == 5180
    assert ycck_rows['qualification']['package']['sha256'] == ycck_rows['qualification']['linuxPackage']['sameActualPackageSha256']
    quarter_strips = json.loads((ROOT / 'benchmarks/warm-jpeg-quarter-strips-evidence.json').read_text(encoding='utf-8'))
    assert quarter_strips['productionChanged'] and quarter_strips['independentQuietConfirmation']
    assert quarter_strips['decision']['passed'] and quarter_strips['status'] == {'Decode410': 'confirmed-pass', 'Decode411': 'confirmed-gap'}
    assert quarter_strips['qualification']['fullChecksEachOs'] == 5185
    assert quarter_strips['qualification']['package']['sha256'] == quarter_strips['qualification']['linuxPackage']['sameActualPackageSha256']
    quarter_single = json.loads((ROOT / 'benchmarks/warm-jpeg-quarter-single-strips-evidence.json').read_text(encoding='utf-8'))
    assert quarter_single['productionChanged'] and quarter_single['independentQuietConfirmation'] and quarter_single['decision']['passed']
    assert quarter_single['qualification']['fullChecksEachOs'] == 5188
    assert quarter_single['qualification']['package']['sha256'] == quarter_single['qualification']['linuxPackage']['sameActualPackageSha256']
    baseline_blocks = json.loads((ROOT / 'benchmarks/warm-jpeg-baseline-dc-ac-fusion-evidence.json').read_text(encoding='utf-8'))
    assert baseline_blocks['productionChanged'] and baseline_blocks['independentQuietConfirmation'] and baseline_blocks['decision']['passed']
    assert baseline_blocks['qualification']['fullChecksEachOs'] == 5193
    assert baseline_blocks['qualification']['package']['sha256'] == baseline_blocks['qualification']['linuxPackage']['sameActualPackageSha256']
    assert baseline_blocks['postcheckRecovery']['measurementReruns'] == 0
    assert set(baseline_blocks['cases']) == {'CmykAdobeSparse2D', 'CmykCaller', 'NativeDecode420', 'Ycck420', 'Decode411', 'Decode410', 'ValidateJpegBaseline420'}
    png_gfni = json.loads((ROOT / 'benchmarks/warm-png-gfni-crc-evidence.json').read_text(encoding='utf-8'))
    assert png_gfni['productionChanged'] and png_gfni['independentQuietConfirmation'] and png_gfni['decision']['passed']
    assert png_gfni['qualification']['fullChecksEachOs'] == 5193 and len(png_gfni['cases']) == 12
    assert png_gfni['qualification']['package']['sha256'] == png_gfni['qualification']['linuxPackage']['sameActualPackageSha256']
    assert png_gfni['hardwareProof']['exact'] and png_gfni['hardwareProof']['impulseChecks'] == 65536
    for case, observations in png_gfni['cases'].items():
        observation = observations[-1]
        assert observation['receipt']['librarySha256'] == png_gfni['producers']['linuxSha256']
        assert len(observation['managed']['actual']) == len(observation['receipt']['nativeBefore']) == len(observation['receipt']['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        assert (observation['conservativeRatioUpper'] <= 1) == (png_gfni['status'][case] == 'confirmed-pass')
    probes = json.loads((ROOT / 'benchmarks/warm-probe-completed-reads-evidence.json').read_text(encoding='utf-8'))
    webp_current = json.loads((ROOT / 'benchmarks/webp-scoring-preparation-evidence.json').read_text(encoding='utf-8'))
    assert webp_current['productionAccepted'] and webp_current['decision']['exactPacketsPreserved']
    assert webp_current['independentQuietConfirmation']
    paired_webp = webp_current['interleavedConfidence']
    assert not paired_webp['productionChanged'] and paired_webp['productionBuilds'] == 0
    for case in paired_webp['cases']:
        assert case['librarySha256'] == webp_current['producers']['linuxSha256']
        assert case['pairs'] == case['statistics']['N'] == 12
        assert case['statistics']['ConfidenceInterval']['Lower'] > 1
        assert case['allOriginalGatesPassed'] and case['quietGuardsPassed']
    lossless_boundaries = json.loads((ROOT / 'benchmarks/current-lossless-webp-boundaries-evidence.json').read_text(encoding='utf-8'))
    assert not lossless_boundaries['productionChanged'] and lossless_boundaries['allBuilds'] == 0
    assert lossless_boundaries['independentQuietConfirmation']
    assert lossless_boundaries['producers']['linuxSha256'] == webp_current['producers']['linuxSha256']
    webp_probe = json.loads((ROOT / 'benchmarks/current-webp-dimension-probe-cost-evidence.json').read_text(encoding='utf-8'))
    assert not webp_probe['productionChanged'] and webp_probe['allBuilds'] == 0
    assert webp_probe['independentQuietConfirmation'] and webp_probe['status']['ProbeDimensionsWebp'] == 'confirmed-gap'
    png_probe = json.loads((ROOT / 'benchmarks/current-png-dimension-probe-cost-evidence.json').read_text(encoding='utf-8'))
    assert not png_probe['productionChanged'] and png_probe['allBuilds'] == 0
    assert png_probe['independentQuietConfirmation'] and png_probe['comparisonLowerBounds']['ProbeDimensionsPng'] > 1
    jpeg_probe = json.loads((ROOT / 'benchmarks/warm-jpeg-probe-marker-pairs-evidence.json').read_text(encoding='utf-8'))
    assert jpeg_probe['decision']['passed'] and jpeg_probe['qualification']['fullChecksEachOs'] == 5240
    assert jpeg_probe['qualification']['sameActualPackageBothOs'] and jpeg_probe['qualification']['jpegProbePrefixesAndInvalidLimitPassed']
    assert jpeg_probe['memory']['allocatedBytesPerOperation'] == 744 and jpeg_probe['memory']['dimensionAdmittedBytes'] == 512
    assert set(jpeg_probe['cases']) == {'ProbeDimensionsJpeg', 'ProbeDimensionsJpegAsync'}
    png_validation = json.loads((ROOT / 'benchmarks/png-validation-current-evidence.json').read_text(encoding='utf-8'))
    png_cursor = json.loads((ROOT / 'benchmarks/warm-png-streaming-cursor-evidence.json').read_text(encoding='utf-8'))
    current_reconciliation = json.loads((ROOT / 'benchmarks/current-png-vp8-reconciliation-evidence.json').read_text(encoding='utf-8'))
    png_prediction = json.loads((ROOT / 'benchmarks/png-predictable-compression-evidence.json').read_text(encoding='utf-8'))
    predictive_cost = json.loads((ROOT / 'benchmarks/warm-png-word-match-evidence.json').read_text(encoding='utf-8'))
    down8_cost = json.loads((ROOT / 'benchmarks/current-bicubic-down8-cost-evidence.json').read_text(encoding='utf-8'))
    horizontal_cost = json.loads((ROOT / 'benchmarks/warm-bicubic-horizontal-taps-evidence.json').read_text(encoding='utf-8'))
    word_publication = json.loads((ROOT / 'benchmarks/warm-bicubic-shared-word-publication-evidence.json').read_text(encoding='utf-8'))
    byte_zero_taps = json.loads((ROOT / 'benchmarks/warm-bicubic-zero-taps-evidence.json').read_text(encoding='utf-8'))
    cross16_cost = json.loads((ROOT / 'benchmarks/warm-bicubic-output-row-cache-evidence.json').read_text(encoding='utf-8'))
    assert cross16_cost['productionChanged'] and cross16_cost['workflow']['productionBuilds'] == 0
    assert cross16_cost['independentCounterbalancedBlock'] and cross16_cost['decision']['allGatesPassed']
    assert cross16_cost['qualification']['sameActualPackageBothOs']
    horizontal_word_current = json.loads((ROOT / 'benchmarks/current-tight-horizontal-word-bicubic-evidence.json').read_text(encoding='utf-8'))
    assert not horizontal_word_current['productionChanged'] and horizontal_word_current['allBuilds'] == 0
    assert horizontal_word_current['comparisonLowerBound'] > 1
    horizontal_word_observation = horizontal_word_current['cases']['TightHorizontal16'][-1]
    assert len(horizontal_word_observation['managed']['actual']) == 12
    assert all(q['passed'] for q in horizontal_word_observation['quietness'].values())
    assert horizontal_word_current['qualificationReused']['sameActualPackageBothOs']
    assert horizontal_word_current['recovery']['measurementReruns'] == 0
    assert not down8_cost['productionChanged'] and down8_cost['independentQuietConfirmation']
    assert down8_cost['preparation']['productionSha256'] == predictive_cost['producers']['linuxSha256']
    assert horizontal_cost['productionChanged'] and horizontal_cost['independentQuietConfirmationThisPair']
    assert horizontal_cost['qualification']['sameActualPackageBothOs']
    assert word_publication['productionChanged'] and word_publication['independentQuietConfirmationThisPair']
    assert word_publication['qualification']['sameActualPackageBothOs']
    assert word_publication['status']['VerticalUp16'] == 'confirmed-pass'
    assert word_publication['cases']['VerticalUp16'][-1]['conservativeRatioUpper'] <= 1
    assert horizontal_cost['baselineReference']['librarySha256'] == down8_cost['preparation']['productionSha256']
    assert byte_zero_taps['qualification']['sameActualPackageBothOs']
    assert byte_zero_taps['sourceScope']['otherCachedSourceUnchanged']
    bicubic_byte_cost = json.loads((ROOT / 'benchmarks/current-bicubic-byte-reduction-cost-evidence.json').read_text(encoding='utf-8'))
    assert not bicubic_byte_cost['productionChanged'] and bicubic_byte_cost['allBuilds'] == 0
    assert bicubic_byte_cost['independentQuietConfirmation'] and bicubic_byte_cost['status']['Down8'] == 'confirmed-gap'
    assert len(bicubic_byte_cost['cases']['Down8'][-1]['managed']['actual']) == 12
    editing_costs = {'case22': bicubic_byte_cost['cases']['Down8'][-1],
                     'case23': word_publication['cases']['Up16'][-1],
                     'case24': cross16_cost['cases']['TightCross16'][-1]}
    editing_producers = {'case22': bicubic_byte_cost, 'case23': word_publication,
                         'case24': cross16_cost}
    streamed = json.loads((ROOT / 'benchmarks/warm-png-streaming-validation-evidence.json').read_text(encoding='utf-8'))
    reconciled = json.loads((ROOT / 'benchmarks/warm-read-reconciliation-evidence.json').read_text(encoding='utf-8'))
    header_skips = json.loads((ROOT / 'benchmarks/warm-header-skip-fast-path-evidence.json').read_text(encoding='utf-8'))
    assert header_skips['productionChanged'] and header_skips['decision']['passed']
    assert header_skips['qualification']['fullChecksEachOs'] == 5332
    assert header_skips['qualification']['sameActualPackageBothOs']
    assert header_skips['qualification']['package']['sha256'] == header_skips['qualification']['linuxPackage']['sameActualPackageSha256']
    assert set(header_skips['cases']) == {'ProbeDimensionsJpeg', 'ProbeDimensionsJpegAsync', 'PngHeaders'}
    byte_color_png = json.loads((ROOT / 'benchmarks/current-byte-color-png-costs-evidence.json').read_text(encoding='utf-8'))
    assert not byte_color_png['productionChanged'] and byte_color_png['allBuilds'] == 0
    assert byte_color_png['independentQuietConfirmations'] == 3
    assert byte_color_png['producers'] == header_skips['producers']
    assert byte_color_png['qualificationReused']['packageSha256'] == header_skips['qualification']['package']['sha256']
    png_encode_coverage = json.loads((ROOT / 'benchmarks/current-png-encode-coverage-evidence.json').read_text(encoding='utf-8'))
    assert not png_encode_coverage['productionChanged'] and png_encode_coverage['allBuilds'] == 0
    assert png_encode_coverage['independentQuietConfirmations'] == 6
    assert png_encode_coverage['producers'] == header_skips['producers']
    assert png_encode_coverage['qualificationReused']['packageSha256'] == header_skips['qualification']['package']['sha256']
    word_color_png = json.loads((ROOT / 'benchmarks/warm-png-word-color-packing-evidence.json').read_text(encoding='utf-8'))
    assert word_color_png['productionChanged'] and word_color_png['decision']['passed']
    assert word_color_png['qualification']['fullChecksEachOs'] == 5342
    assert word_color_png['qualification']['sameActualPackageBothOs']
    assert word_color_png['qualification']['package']['sha256'] == word_color_png['qualification']['linuxPackage']['sameActualPackageSha256']
    assert set(word_color_png['cases']) == {'EncodeGray16', 'EncodeRgb16', 'EncodeGrayAlpha16'}
    png_framing = json.loads((ROOT / 'benchmarks/png-normal-cursor-publication-evidence.json').read_text(encoding='utf-8'))
    assert png_framing['productionAccepted'] and png_framing['requiredMaterialPaths'] == 283
    assert len(png_framing['cases']) == 51 and len(png_framing['refreshedCases']) == 43
    assert len(png_framing['retainedCases']) == 8
    assert png_framing['statusCounts'] == {status: sum(c['status'] == status for c in png_framing['cases'].values())
        for status in ('pass', 'gap', 'uncertain')}
    assert png_framing['raw']['mirrored'] and png_framing['raw']['allMembersVerified']
    assert png_framing['sourceProof']['sameNormalTraversalModuloEobBreakAndExceptionPublication']
    assert png_framing['sourceProof']['refillReadHelpersByteExact']
    assert png_framing['sourceProof']['allExplicitExceptionMessagesAndOrderExact']
    for name, framing_case in png_framing['cases'].items():
        assert framing_case['actualIterations'] == len(framing_case['managedActualNanoseconds']) == 12
        expected = (png_framing['retainedCases'][name]['librarySha256'] if name in png_framing['retainedCases']
                    else png_framing['producers']['linuxSha256'])
        assert framing_case['librarySha256'] == expected
        assert framing_case['allOriginalGatesPassed'] and framing_case['quietGuardsPassed']
        assert framing_case['sourceUnchanged'] and framing_case['independentOwnedOutput']
        assert framing_case.get('descriptorExact', framing_case.get('managedExact', False))
        assert framing_case['status'] == ('pass' if framing_case['upper'] <= 1 else 'gap' if framing_case['lower'] > 1 else 'uncertain')
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated']
               for comparison in png_framing['comparison'].values() for v in comparison['views'].values())
    png_q = png_framing['qualification']
    assert png_q['fullChecksEachOs'] == png_q['windows']['total'] == png_q['linux']['total'] == 5907
    assert png_q['actualPackage']['sha256'] == png_q['samePackageLinux']['sameActualPackageSha256']
    assert all(q['sharedOrdinaryPngCursorAndRowsOwnershipPassed']
               for q in (png_q['actualPackage'], png_q['samePackageLinux']))
    png_live = json.loads((ROOT / 'benchmarks/png-live-run-preparation-evidence.json').read_text(encoding='utf-8'))
    assert png_live['productionAccepted'] and png_live['allAffectedPerformanceReconciled']
    assert png_live['requiredMaterialPaths'] == 289 and png_live['familyStatusCounts'] == {'pass': 49, 'gap': 0, 'uncertain': 2}
    assert png_live['raw']['mirrored'] and png_live['raw']['allMembersVerified']
    assert png_live['sourceProof']['wholeOriginalSourceReconstructsFromOnlyRunGuardAndBorrowedViews']
    assert png_live['sourceProof']['oldSingleSymbolOrderAndTwoCancellationPollsPreserved']
    assert png_live['sourceProof']['ordinaryRollingCountingCursorEncoderAndOwnersVerbatim']
    assert png_live['comparison']['word']['normalizedGainSeparated']
    assert all(v['rawGainSeparated'] for v in png_live['comparison']['word']['views'].values())
    assert not png_live['comparison']['byte']['normalizedAdverseSeparated']
    assert png_live['storage']['newOwnedCapacityBytes'] == png_live['storage']['newRetainedStateBytes'] == 0
    for live_case in png_live['cases'].values():
        assert live_case['actualIterations'] == live_case['nativeSamplesEachBracket'] == len(live_case['managedActualNanoseconds']) == 12
        assert live_case['quietGuardsPassed'] and live_case['allOriginalGatesPassed']
        assert live_case['managedExact'] and live_case['alphaExact'] and live_case['hiddenRgbExact']
        assert live_case['sourceUnchanged'] and live_case['independentOwnedOutput']
        assert live_case['librarySha256'] == png_live['producers']['windowsSha256']
        assert live_case['status'] == 'uncertain' and live_case['lower'] <= 1 < live_case['upper']
    resident = json.loads((ROOT / 'benchmarks/current-png-resident-identify-evidence.json').read_text(encoding='utf-8'))
    resident_other = json.loads((ROOT / 'benchmarks/current-jpeg-webp-resident-identify-evidence.json').read_text(encoding='utf-8'))
    stream_identify = json.loads((ROOT / 'benchmarks/current-png-stream-identify-evidence.json').read_text(encoding='utf-8'))
    decoders = json.loads((ROOT / 'benchmarks/current-default-decoders-evidence.json').read_text(encoding='utf-8'))
    resident_decode = json.loads((ROOT / 'benchmarks/webp-palette-output-evidence.json').read_text(encoding='utf-8'))
    assert resident_decode['productionAccepted'] and resident_decode['requiredMaterialPaths'] == 283
    assert resident_decode['sourceProof']['originalRestoredByRemovingFinalPaletteBranch']
    assert resident_decode['raw']['mirrored'] and resident_decode['raw']['allMembersVerified']
    assert set(resident_decode['costs']) == {'ValidateVp8lAlpha', 'ValidateVp8Alpha', 'WebpResident', 'Lossless8',
        'IndexedNative16Colors8', 'NativeLosslessCancelable8', 'WebpStream', 'Native8', 'Vp8Stream'}
    q = resident_decode['qualification']
    assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total'] == 6058
    assert q['sameActualPackageBothOS'] and q['finalPaletteConsumerBothOs']
    assert q['actualPackage']['sha256'] == q['samePackageLinux']['sameActualPackageSha256']
    for name, cost in resident_decode['costs'].items():
        assert cost['actualIterations'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['librarySha256'] == q['actualPackage']['librarySha256']
        assert cost['quietGuardsPassed'] and cost['allOriginalGatesPassed']
        assert cost['sourceUnchanged'] and cost['independentOwnedOutput']
        if name.startswith('Validate'):
            assert cost['descriptorExact'] and cost['prefixAndCodingScopesVerified']
        else:
            assert cost['managedOutputSha256'] == cost['nativeOutputSha256'] and cost['alphaExact']
        assert cost['status'] == ('pass' if cost['upper'] <= 1 else 'gap' if cost['lower'] > 1 else 'uncertain')
        assert math.isclose(cost['ratio'], cost['managedUs'] / cost['nativeUs'], rel_tol=1e-12)
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated']
               for v in resident_decode['comparison']['views'].values())
    assert resident_decode['storage']['newRetainedScratchBytes'] == 0
    lpf = {**resident_decode['costs'], **vp8_costs}
    huffman_front = json.loads((ROOT / 'benchmarks/webp-huffman-read-frontend-evidence.json').read_text(encoding='utf-8'))
    assert huffman_front['productionAccepted'] and huffman_front['allAffectedPerformanceReconciled']
    assert huffman_front['requiredMaterialPaths'] == 289 and not huffman_front['materialPassToGapContradictions']
    assert huffman_front['raw']['mirrored'] and huffman_front['raw']['allMembersVerified']
    assert huffman_front['sourceProof']['entireOriginalProductionFileReconstructs']
    assert huffman_front['storage']['newOwnedCapacityBytes'] == huffman_front['storage']['newRetainedCapacityBytes'] == 0
    assert set(huffman_front['costs']) == set(resident_decode['costs'])
    for name, cost in huffman_front['costs'].items():
        assert cost['actualIterations'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['allActualsRetained'] and cost['allOriginalGatesPassed'] and cost['quietGuardsPassed']
        assert cost['sourceUnchanged'] and cost['independentOwnedOutput']
        assert cost['librarySha256'] == huffman_front['producers']['windowsSha256']
        assert cost['status'] == ('pass' if cost['upper'] <= 1 else 'gap' if cost['lower'] > 1 else 'uncertain')
        assert math.isclose(cost['ratio'], cost['managedUs'] / cost['nativeUs'], rel_tol=1e-12)
        if name.startswith('Validate'):
            assert cost['descriptorExact'] and cost['prefixAndCodingScopesVerified']
        else:
            assert cost['alphaExact'] and cost['managedOutputSha256'] == resident_decode['costs'][name]['managedOutputSha256']
    assert huffman_front['comparison']['normalizedGainSeparated']
    assert all(view['rawGainSeparated'] for view in huffman_front['comparison']['views'].values())
    live_cost = huffman_front['costs']['NativeLosslessCancelable8']
    assert live_cost['allocatedBytes'] == 152728 and live_cost['operationsPerSubject'] == 4
    assert live_cost['cancellationCanBeCanceled'] and not live_cost['cancellationRequested'] and live_cost['alreadyCanceledRejected']
    assert live_cost['allRgbaSamplesExact'] and live_cost['hiddenRgbExact']
    lpf.update(huffman_front['costs'])
    color_tiles = json.loads((ROOT / 'benchmarks/webp-inverse-color-tiles-evidence.json').read_text(encoding='utf-8'))
    assert color_tiles['productionAccepted'] and color_tiles['allAffectedPerformanceReconciled']
    assert color_tiles['requiredMaterialPaths'] == 289 and not color_tiles['materialPassToGapContradictions']
    assert color_tiles['sourceProof']['entireOriginalFileReconstructsAfterReplacingOnlyInverseColorLoop']
    assert color_tiles['raw']['mirrored'] and color_tiles['raw']['allMembersVerified']
    assert color_tiles['storage']['newOwnedCapacityBytes'] == color_tiles['storage']['newRetainedCapacityBytes'] == 0
    assert set(color_tiles['costs']) == set(huffman_front['costs']) - {'ValidateVp8lAlpha', 'ValidateVp8Alpha'}
    assert color_tiles['validationReuse']['sourceValid'] and color_tiles['validationReuse']['additionalTimingJobs'] == 0
    assert color_tiles['validationReuse']['costs'] == {name: huffman_front['costs'][name] for name in ('ValidateVp8lAlpha', 'ValidateVp8Alpha')}
    for name, cost in color_tiles['costs'].items():
        assert cost['actualIterations'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['allActualsRetained'] and cost['allOriginalGatesPassed'] and cost['quietGuardsPassed']
        assert cost['sourceUnchanged'] and cost['independentOwnedOutput'] and cost['alphaExact']
        assert cost['managedOutputSha256'] == huffman_front['costs'][name]['managedOutputSha256']
        assert cost['librarySha256'] == color_tiles['producers']['windowsSha256']
        assert cost['status'] == ('pass' if cost['upper'] <= 1 else 'gap' if cost['lower'] > 1 else 'uncertain')
        assert math.isclose(cost['ratio'], cost['managedUs'] / cost['nativeUs'], rel_tol=1e-12)
    assert color_tiles['comparison']['normalizedGainSeparated']
    assert all(view['rawGainSeparated'] for view in color_tiles['comparison']['views'].values())
    lpf.update(color_tiles['costs'])
    lossless_read_counts = {state: sum(lpf[name]['status'] == state for name in ('WebpResident', 'Lossless8', 'IndexedNative16Colors8', 'NativeLosslessCancelable8', 'WebpStream', 'ValidateVp8lAlpha')) for state in ('pass', 'gap', 'uncertain')}
    assert lossless_read_counts == {'pass': 6, 'gap': 0, 'uncertain': 0}
    vp8_decode_counts = {state: sum(lpf[name]['status'] == state for name in vp8_costs)
                         for state in ('pass', 'gap', 'uncertain')}
    assert vp8_decode_counts == {'pass': 5, 'gap': 0, 'uncertain': 1}
    jpeg_decode = baseline_blocks
    assert not decoders['productionChanged'] and len(decoders['cases']) == 3
    assert not reconciled['productionChanged'] and len(reconciled['cases']) == 6
    assert not resident['productionChanged'] and resident['status']['PngResident'] == 'screen-gap'
    assert not resident_other['productionChanged'] and resident_other['independentQuietConfirmation']
    assert not stream_identify['productionChanged'] and len(stream_identify['cases']) == 2
    assert not reads['productionChanged'] and len(reads['cases']) == 12
    assert probes['productionChanged'] and len(probes['cases']) == 4
    assert not png_validation['productionChanged'] and png_validation['sourceAndQualifiedProducersRestored']
    assert png_cursor['productionChanged'] and png_cursor['independentQuietConfirmationThisPair']
    assert set(png_cursor['cases']) == {'ValidatePng8', 'ValidatePng16', 'ValidatePng16Async'}
    assert not current_reconciliation['productionChanged'] and current_reconciliation['allBuilds'] == 0
    assert png_prediction['productionChanged'] and png_prediction['qualification']['actualPackageBothOs']
    assert set(png_prediction['cases']) == {'EncodeSmallest8', 'EncodeSmallest16', 'EncodeFast8', 'EncodeNoCompression16'}
    assert predictive_cost['productionChanged'] and predictive_cost['independentQuietConfirmationThisPair']
    assert streamed['productionChanged'] and len(streamed['cases']) == 4
    assert jpeg['productionChanged'] and jpeg['independentQuietConfirmationThisPair']
    assert vp8l['version'] == '0.2.1' and vp8l['independentQuietConfirmationThisPair']
    assert current['version'] == '0.2.1' and current['independentQuietConfirmationThisPair']
    assert set(current['cases']) == {'EncodeFast8', 'EncodeSmallest8', 'EncodeSmallest16'}
    rows = []
    for case in evidence['cases']:
        if case['id'] in {'case21', 'case00', 'case06', 'case08', 'case09', 'case10', 'case11', 'case07', 'case13', 'case14', 'case15', 'case16', 'case27', 'case28', 'case29', 'case30'}:
            continue  # Replaced by current self-contained family evidence below.
        observation = case['observation']
        vp8l_case = case['id'] in {'case12', 'case13', 'case14'}
        updated = vp8l_case or case['id'] in editing_costs or case['id'] in {'case00', 'case02', 'case03', 'case16', 'case17', 'case18', 'case19', 'case20'}
        producer = jpeg if case['id'] in {'case02', 'case03'} else vp8l if vp8l_case else stored if case['id'] == 'case20' else current
        if case['id'] in {'case17', 'case18'}:
            producer = predictive_cost
        elif case['id'] in {'case19', 'case20'}:
            producer = png_prediction
        if case['id'] in editing_costs:
            observation = editing_costs[case['id']]
        elif case['id'] in {'case02', 'case03'}:
            observation = jpeg['cases']['Rgb420' if case['id'] == 'case02' else 'Rgba16'][-1]
        elif updated:
            observation = producer['cases'][observation['receipt']['caseId']][-1]
        if case['id'] in {'case17', 'case18', 'case19', 'case20'}:
            producer = png_gfni
            observation = png_gfni['cases'][observation['receipt']['caseId']][-1]
        if case['id'] == 'case12':
            producer = webp_current
            observation = webp_current['cases']['Encode8']
            assert (observation['conservativeRatioUpper'] <= 1) == (observation['status'] == 'Confirmed pass')
            assert observation['completePacketExact'] and observation['quietGuardsPassed']
        receipt = observation['receipt']
        stats = observation['managed']['statistics']
        native = min(b['mean'] for b in observation['nativeBrackets'])
        ratio = observation['worstBracketRatio']
        upper = observation['conservativeRatioUpper']
        if case['id'] in editing_costs:
            expected_library = editing_producers[case['id']]['producers']['linuxSha256']
        else:
            expected_library = producer['producers']['linuxSha256'] if updated else evidence['librarySha256']
        assert receipt['librarySha256'] == expected_library
        assert all(g['passed'] for g in observation['quietness'].values())
        samples = 12 if case['id'] in {'case00', 'case02', 'case06', 'case07', 'case08', 'case09', 'case10', 'case11', 'case12', 'case13', 'case14', 'case15', 'case16', 'case21', 'case22', 'case23', 'case24', 'case27', 'case28', 'case29', 'case30'} or updated and receipt['caseId'] in {'EncodeSmallest8', 'EncodeSmallest16', 'EncodeNoCompression16', 'EncodeFast8'} else 3
        assert (8 if samples == 12 else 2) <= stats['N'] <= samples
        assert len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == samples
        assert math.isclose(stats['Mean'] / native, ratio, rel_tol=1e-12)
        allocated = observation['managedMemory']['BytesAllocatedPerOperation']
        label = case['label'] + (' †' if updated else '')
        if case['kind'] == 'editing':
            label += f' → {receipt["outputWidth"]}×{receipt["outputHeight"]}'
        cells = [label, f'{stats["Mean"] / 1000:,.2f}', f'{native / 1000:,.2f}',
                 f'{ratio:.3f} / {upper:.3f}', f'{allocated:,}']
        if case['kind'] == 'codec':
            packet = f'{receipt["managedOutputBytes"]:,} / {receipt["nativeOutputBytes"]:,}' if case['operation'] == 'encode' else '—'
            cells.append(packet)
        rows.append((case['kind'], '| ' + ' | '.join(cells) + ' |'))
    for case, label in [('EncodeGray8', 'PNG grayscale8, Optimal6 / native6'),
                        ('EncodeRgb8', 'PNG RGB8, Optimal6 / native6'),
                        ('EncodeGrayAlpha8', 'PNG gray-alpha8, Optimal6 / native6')]:
        observation = byte_color_png['cases'][case][-1]
        receipt = observation['receipt']
        assert byte_color_png['status'][case] == 'confirmed-gap'
        assert receipt['librarySha256'] == byte_color_png['producers']['linuxSha256']
        assert len(observation['managed']['actual']) == 12 and all(q['passed'] for q in observation['quietness'].values())
        assert receipt['managedEncodedSha256'] == receipt['nativeOutputSha256']
        assert receipt['managedOutputBytes'] == receipt['nativeOutputBytes']
        stats = observation['managed']['statistics']
        native = min(b['mean'] for b in observation['nativeBrackets'])
        packet_bytes = receipt['managedOutputBytes']
        rows.append(('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | {packet_bytes:,} / {packet_bytes:,} |'))
    for case, label in [('Encode8', 'PNG RGBA8, Optimal6 / native6'),
                        ('Encode16', 'PNG RGBA16, Optimal6 / native6'),
                        ('EncodeRgb16', 'PNG RGB16, Optimal6 / native6'),
                        ('EncodeGray16', 'PNG grayscale16, Optimal6 / native6, red-plane reference'),
                        ('EncodeGrayAlpha16', 'PNG gray-alpha16, Optimal6 / native6'),
                        ('EncodeNoCompression8', 'PNG RGBA8, stored0 / native0')]:
        producer = word_color_png if case in word_color_png['cases'] else png_encode_coverage
        observation = producer['cases'][case][-1]
        receipt = observation['receipt']
        assert producer['status'][case] == 'confirmed-gap'
        assert receipt['librarySha256'] == producer['producers']['linuxSha256']
        assert len(observation['managed']['actual']) == 12 and all(q['passed'] for q in observation['quietness'].values())
        assert receipt['managedEncodedSha256'] == receipt['nativeOutputSha256']
        assert receipt['managedOutputBytes'] == receipt['nativeOutputBytes']
        stats = observation['managed']['statistics']
        native = min(b['mean'] for b in observation['nativeBrackets'])
        packet_bytes = receipt['managedOutputBytes']
        rows.append(('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | {packet_bytes:,} / {packet_bytes:,} |'))
    png_adler = json.loads((ROOT / 'benchmarks/png-block-adler-evidence.json').read_text(encoding='utf-8'))
    assert png_adler['productionAccepted'] and png_adler['allAffectedPerformanceReconciled']
    assert len(png_adler['readCases']) == 51 and png_adler['readStatusCounts'] == {'pass': 49, 'gap': 0, 'uncertain': 2}
    assert not png_adler['generalCpuGainClaimed'] and not png_adler['normalizedGainConfirmed']
    assert all(v['rawGainSeparated'] for v in png_adler['primaryEffect']['views'].values())
    for case, limit in [('DecodeCancelable8', None), ('DecodeTight8', 1235173), ('DecodeTight16', 3254193)]:
        framing_case = png_adler['readCases'][case]
        assert framing_case['tightWorkingBytes'] == limit
        assert framing_case['cancellationCanBeCanceled'] == (case == 'DecodeCancelable8')
        assert not framing_case['cancellationRequested']
        assert framing_case['normalScalarCliExact'] and framing_case['alphaExact'] and framing_case['hiddenRgbExact']
        assert framing_case['nativeDecodeEndianConversionTimed'] == (case == 'DecodeTight16')
        rows.append(('codec', f'| {framing_case["label"]} † | {framing_case["managedUs"]:,.2f} | {framing_case["nativeUs"]:,.2f} | '
                     f'{framing_case["ratio"]:.3f} / {framing_case["upper"]:.3f} | {framing_case["allocatedBytes"]:,} | — |'))
    predicted = png_gfni['cases']['EncodePredictable16'][-1]
    receipt, stats = predicted['receipt'], predicted['managed']['statistics']
    assert receipt['librarySha256'] == png_gfni['producers']['linuxSha256']
    assert len(predicted['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert all(q['passed'] for q in predicted['quietness'].values()) and receipt['allRgbaSamplesExact']
    native = min(b['mean'] for b in predicted['nativeBrackets'])
    assert math.isclose(stats['Mean'] / native, predicted['worstBracketRatio'], rel_tol=1e-12)
    rows.append(('codec', f'| PNG predictable RGBA16 3000², Optimal6 / mixed6 † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                 f'{predicted["worstBracketRatio"]:.3f} / {predicted["conservativeRatioUpper"]:.3f} | '
                 f'{predicted["managedMemory"]["BytesAllocatedPerOperation"]:,} | '
                 f'{receipt["managedOutputBytes"]:,} / {receipt["nativeOutputBytes"]:,} |'))
    for case, label in [('NativeDecode420', 'JPEG baseline420, resident decode'),
                        ('CmykCaller', 'JPEG DC-only, caller CMYK decode'),
                        ('WebpResident', 'VP8L RGBA8, resident decode'),
                        ('WebpStream', 'VP8L RGBA8, fresh seekable Stream decode'),
                        ('IndexedNative16Colors8', 'VP8L palette16 RGBA8, resident decode (confirmed pass)')]:
        if case in lpf:
            c = lpf[case]
            qualifier = ' (confidence open)' if c['status'] == 'uncertain' else ''
            rows.append(('codec', f'| {label}{qualifier} † | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | '
                         f'{c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | — |'))
            continue
        producer = jpeg_decode
        observation = producer['cases'][case][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        samples = 12
        assert producer['status'][case].startswith('confirmed-')
        assert len(observation['managed']['actual']) == samples
        assert receipt['librarySha256'] == producer.get('reusedCaseProducers', {}).get(case, producer['producers']['linuxSha256'])
        assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256']
        assert all(q['passed'] for q in observation['quietness'].values())
        assert len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == samples
        native = min(b['mean'] for b in observation['nativeBrackets'])
        assert math.isclose(stats['Mean'] / native, observation['worstBracketRatio'], rel_tol=1e-12)
        rows.append(('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | — |'))
    cmyk = baseline_blocks['cases']['CmykAdobeSparse2D'][-1]
    receipt, stats = cmyk['receipt'], cmyk['managed']['statistics']
    assert len(cmyk['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert receipt['librarySha256'] == baseline_blocks['producers']['linuxSha256']
    assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256'] and receipt['decoderMaximum'] == receipt['scalarDecoderMaximum'] == 0
    assert all(g['passed'] for g in cmyk['quietness'].values())
    rows.append(('codec', f'| JPEG CMYK sparse-2D, resident decode † | {stats["Mean"] / 1000:,.2f} | '
                 f'{min(b["mean"] for b in cmyk["nativeBrackets"]) / 1000:,.2f} | '
                 f'{cmyk["worstBracketRatio"]:.3f} / {cmyk["conservativeRatioUpper"]:.3f} | '
                 f'{cmyk["managedMemory"]["BytesAllocatedPerOperation"]:,} | — |'))
    for case, label in [('Ycck420', 'JPEG YCCK420, resident decode'), ('Decode411', 'JPEG 4:1:1, resident decode'), ('Decode410', 'JPEG 4:1:0, resident decode')]:
        producer = baseline_blocks
        observation = producer['cases'][case][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert receipt['librarySha256'] == producer['producers']['linuxSha256']
        assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256']
        assert receipt['nativePixelsExact'] and not receipt['nativeExceptionUsed']
        assert receipt['decoderMaximum'] == receipt['scalarDecoderMaximum'] == 0
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        rows.append(('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | '
                     f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | — |'))
    observation = webp_current['cases']['EncodeOpaque8']
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == webp_current['producers']['linuxSha256'] and receipt['completePacketExact']
    assert receipt['managedOutputBytes'] == receipt['nativeOutputBytes'] == 121896
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert all(q['passed'] for q in observation['quietness'].values())
    native = min(b['mean'] for b in observation['nativeBrackets'])
    opaque_row = f'| VP8L opaque RGBA8 / libwebp exact=1 † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | {observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | {observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | 121,896 / 121,896 |'
    position = next(i for i, (kind, row) in enumerate(rows) if row.startswith('| VP8L RGBA8 /'))
    rows.insert(position + 1, ('codec', opaque_row))
    for case, label in [('Encode16', 'VP8L RGBA16, including normalization / libwebp exact=1'),
                        ('WebpLossless8AsyncNonSeekable', 'VP8L async, fresh nonseekable output / libwebp exact=1')]:
        observation = lossless_boundaries['cases'][case]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert receipt['librarySha256'] == lossless_boundaries['producers']['linuxSha256']
        assert receipt['completePacketExact'] and observation['allOriginalGatesPassed']
        assert receipt['managedOutputBytes'] == receipt['nativeOutputBytes'] == 183812
        assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256']
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        assert (observation['conservativeRatioUpper'] <= 1) == (lossless_boundaries['status'][case] == 'confirmed-pass')
        native = min(b['mean'] for b in observation['nativeBrackets'])
        assert math.isclose(stats['Mean'] / native, observation['worstBracketRatio'], rel_tol=1e-12)
        rows.append(('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | 183,812 / 183,812 |'))
    for filename, case, label, managed_bytes, native_bytes, build_key in [
            ('current-async-jpeg-rgb420-cost-evidence.json', 'JpegRgb420AsyncSamples',
             'JPEG caller RGB24 async nonseekable, 420', 52091, 52093, 'allMeasurementBuilds'),
            ('current-async-jpeg-rgba8-cost-evidence.json', 'Jpeg8AsyncNonSeekable',
             'JPEG RGBA8 async nonseekable, 444', 133039, 133041, 'productionHarnessNativeBuilds')]:
        async_jpeg = json.loads((ROOT / 'benchmarks' / filename).read_text(encoding='utf-8'))
        assert not async_jpeg['productionChanged'] and async_jpeg[build_key] == 0
        assert async_jpeg['independentQuietConfirmation'] and async_jpeg['qualificationReuse']['sameActualPackageBothOs']
        assert async_jpeg['currentQuality']['reconstructedNativeAndManagedPixelsExact']
        assert async_jpeg['currentQuality']['probe']['qualityMatch']['PsnrGapDb'] == 0
        assert async_jpeg['comparisonLowerBounds'][case] > 1
        observation = async_jpeg['cases'][case][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert receipt['librarySha256'] == async_jpeg['producers']['linuxSha256'] and receipt['encodedPacketsExact']
        assert receipt['managedOutputBytes'] == managed_bytes and receipt['nativeOutputBytes'] == native_bytes
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        rows.append(('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | '
                     f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | {managed_bytes:,} / {native_bytes:,} |'))
    tight_byte_up = json.loads((ROOT / 'benchmarks/warm-bicubic-byte-fixed-certificate-evidence.json').read_text(encoding='utf-8'))
    assert tight_byte_up['productionChanged'] and tight_byte_up['independentQuietConfirmation']
    assert tight_byte_up['qualification']['fullChecksEachOs'] == 5245
    assert tight_byte_up['qualification']['sameActualPackageBothOs']
    assert tight_byte_up['decisions']['confirm']['passed'] and tight_byte_up['decisions']['confirm']['normalizedSeparated']
    # Current enlargement and shared byte-quartet caller costs have one owner.
    quartet = json.loads((ROOT / 'benchmarks/bicubic-shared-quartet-execution-evidence.json').read_text(encoding='utf-8'))
    assert quartet['productionAccepted'] and quartet['requiredMaterialPaths'] == 283
    assert quartet['raw']['mirrored'] and quartet['raw']['allMembersVerified']
    qq = quartet['qualification']
    assert qq['fullChecksEachOs'] == qq['windows']['total'] == qq['linux']['total'] == 6183
    assert qq['sameActualPackageBothOS'] and qq['actualPackage']['sha256'] == qq['samePackageLinux']['sameActualPackageSha256']
    assert qq['actualPackage']['actualConsumerPassed'] and qq['samePackageLinux']['actualConsumerPassed']
    assert quartet['storage']['newOwnedHeapCapacityBytes'] == quartet['storage']['newRetainedStateBytes'] == 0
    assert quartet['sourceProof']['allMathOutsideByteQuartetPreparationVerbatim']
    assert quartet['sourceProof']['byteOriginalOrderRepairAndTinyReplayVerbatim']
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated'] for v in quartet['comparison']['views'].values())
    assert len(quartet['cases']) == 15
    assert quartet['statusCounts'] == {state: sum(c['status'] == state for c in quartet['cases'].values())
                                     for state in ('pass', 'gap', 'uncertain')}
    for current in quartet['cases'].values():
        if current['caseId'] in {'Up8', 'TightUp8', 'HorizontalUp8', 'TightHorizontalUp8', 'VerticalUp8', 'TightVerticalUp8',
                                  'Up16', 'TightUp16', 'HorizontalUp16', 'TightHorizontalUp16', 'VerticalUp16', 'TightVerticalUp16'}:
            continue  # Current horizontal preparation/execution below owns these costs.
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['librarySha256'] == qq['actualPackage']['librarySha256']
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['sourceUnchanged'] and current['independentOwnedOutput'] and current['managedQualifiedOriginalPixelsExact']
        assert current['managedRationalReferenceExact'] or current['certifiedExactRationalAlphaHalves'] + current['certifiedExactRationalRgbHalves'] > 0
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    enlargement_modes = json.loads((ROOT / 'benchmarks/bicubic-enlargement-horizontal-modes-evidence.json').read_text(encoding='utf-8'))
    assert enlargement_modes['productionAccepted'] and enlargement_modes['allAffectedPerformanceReconciled']
    assert enlargement_modes['requiredMaterialPaths'] == 289 and not enlargement_modes['materialPassToGapContradictions']
    assert enlargement_modes['raw']['mirrored'] and enlargement_modes['raw']['allMembersVerified']
    assert len(enlargement_modes['cases']) == 12 and enlargement_modes['workflow']['newPerformanceControls'] == 0
    assert enlargement_modes['statusCounts'] == {'pass': 11, 'gap': 0, 'uncertain': 1}
    es = enlargement_modes['sourceProof']
    assert es['wholeSharedMathReconstructsByRemovingOnlyNewFourTapHelpers']
    assert es['wholeScheduleReconstructsByRemovingOnlyHorizontalModesAndNewPairedFilter']
    assert es['originalNoPairFilterBodyAndSignatureVerbatim'] and es['preparedDeadSlotOnly'] and es['fullFiveTapLiveLastSlotPreserved']
    assert es['identityRemovesEntireHorizontalFilterAndIntermediateCopy'] and es['identityWidthGuard'] == 2**26
    assert es['simdExactLanes'] == 2048 and es['additionalIdentityRationalFrames'] == 4
    assert enlargement_modes['storage']['newOwnedCapacityBytes'] == enlargement_modes['storage']['newRetainedStateBytes'] == 0
    assert enlargement_modes['storage']['admittedScratchBytes'] == 4096
    for current in enlargement_modes['cases'].values():
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['managedExact'] and current['managedRationalReferenceExact']
        assert current['sourceUnchanged'] and current['independentOwnedOutput']
        assert current['librarySha256'] == enlargement_modes['producers']['windowsSha256']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    for case in ('TightVerticalUp8', 'TightVerticalUp16', 'TightUp16'):
        effect = enlargement_modes['comparison'][case]
        assert effect['normalizedGainSeparated'] and not effect['normalizedAdverseSeparated']
        assert all(v['rawGainSeparated'] for v in effect['views'].values())
    he = enlargement_modes['cases']['TightHorizontalUp8']
    assert he['status'] == 'uncertain'
    labels = {'TightUp8': 'Bicubic RGBA8 enlargement,',
              'TightHorizontalUp8': 'Bicubic RGBA8 width enlargement,',
              'TightVerticalUp8': 'Bicubic RGBA8 height enlargement,'}
    for case, label in labels.items():
        current = enlargement_modes['cases'][case]
        rows.append(('editing', f'| {label} input+output+4 KiB † → {current["outputWidth"]}×{current["outputHeight"]} | '
                     f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | '
                     f'{current["allocatedBytes"]:,} |'))
    tight_byte_down = json.loads((ROOT / 'benchmarks/current-tight-byte-bicubic-reduction-cost-evidence.json').read_text(encoding='utf-8'))
    assert not tight_byte_down['productionChanged'] and tight_byte_down['allBuilds'] == 0
    assert tight_byte_down['producers'] == tight_byte_up['producers']
    assert tight_byte_down['qualificationReuse']['sameActualPackageBothOs']
    observation = tight_byte_down['cases']['TightDown8'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == tight_byte_down['producers']['linuxSha256']
    assert receipt['managedRationalReferenceExact'] and receipt['managedMaxWorkingBytes'] == 667660
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert all(q['passed'] for q in observation['quietness'].values())
    rows.append(('editing', f'| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | {stats["Mean"] / 1000:,.2f} | '
                 f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                 f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                 f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
    wide_byte = json.loads((ROOT / 'benchmarks/cached-wide-certified-publication-evidence.json').read_text(encoding='utf-8'))
    current = wide_byte['cases']['Wide8']
    cached_publication_qualification = wide_byte['qualification']
    wq = cached_publication_qualification
    assert wide_byte['productionAccepted'] and wide_byte['requiredMaterialPaths'] == 283
    assert wq['fullChecksEachOs'] == wq['windows']['total'] == wq['linux']['total'] == 6158
    assert wq['actualPackage']['sha256'] == wq['samePackageLinux']['sameActualPackageSha256']
    assert wq['sameActualPackageBothOS']
    assert wq['resizeInternalEachModeEachOs'] == 332 and wq['resizePublicEachModeEachOs'] == 729
    assert all(p['originalReconstructedExactly'] for p in wide_byte['sourceCodeProof']['sources'].values())
    assert wide_byte['storage']['newOwnedCapacityBytes'] == wide_byte['storage']['newRetainedOperationStorageBytes'] == 0
    assert wide_byte['raw']['mirrored'] and wide_byte['raw']['allMembersVerified']
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated'] for v in wide_byte['comparison']['views'].values())
    assert wide_byte['largeGeneralization']['normalScalarAndPriorPixelsExact'] and wide_byte['largeGeneralization']['quietGuardsPassed']
    for case, cost in wide_byte['cases'].items():
        assert cost['librarySha256'] == wq['actualPackage']['librarySha256']
        assert cost['actualIterations'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['allOriginalGatesPassed'] and cost['quietGuardsPassed']
        assert cost['managedRationalReferenceExact'] and cost['independentOwnedOutput'] and cost['sourceUnchanged']
        assert cost['status'] == ('pass' if cost['upper'] <= 1 else 'gap' if cost['lower'] > 1 else 'uncertain')
    rows.append(('editing', f'| Bicubic RGBA8 wider-support reduction † → 128×64 | {current["managedUs"]:,.2f} | '
                 f'{current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | '
                 f'{current["allocatedBytes"]:,} |'))
    exact_alpha = json.loads((ROOT / 'benchmarks/warm-exact-alpha-int128-evidence.json').read_text(encoding='utf-8'))
    assert exact_alpha['qualification']['fullChecksEachOs'] == 5267
    assert exact_alpha['qualification']['sameActualPackageBothOs']
    assert exact_alpha['decisions']['confirm']['passed'] and exact_alpha['decisions']['confirm']['normalizedSeparated']
    alpha_callers = json.loads((ROOT / 'benchmarks/current-shared-alpha-caller-costs-evidence.json').read_text(encoding='utf-8'))
    assert alpha_callers['producers'] == exact_alpha['producers']
    assert not alpha_callers['productionChanged'] and alpha_callers['allBuilds'] == 0
    refreshed_prefixes = {
        'Bicubic RGBA8 enlargement,': 'bicubic-TightUp8',
        'Bicubic RGBA8 width enlargement,': 'bicubic-TightHorizontalUp8',
        'Bicubic RGBA8 height enlargement,': 'bicubic-TightVerticalUp8',
        'Bicubic RGBA8 reduction,': 'bicubic-TightDown8',
        'Bicubic RGBA8 wider-support reduction': 'bicubic-Wide8',
        'Bicubic upscale RGBA16': 'bicubic-Up16',
    }
    reused_prefixes = {
        'Bicubic resize RGBA8': 'bicubic-Down8',
        'Bicubic RGBA16, width down / height up': 'bicubic-TightCross16',
        'Bicubic RGBA8, 32×16 / input+output': 'bicubic-Tight32x16_8',
        'Bicubic RGBA8, 32×16 / default': 'bicubic-Down32x16_8',
    }
    refreshed, reused = set(), set()
    for index, (kind, row) in enumerate(rows):
        label = row.split('|')[1].strip()
        if kind != 'editing' or not any(name in label.lower() for name in ('bilinear', 'bicubic')):
            continue
        key = next((key for prefix, key in refreshed_prefixes.items() if label.startswith(prefix)), None)
        if key is None:
            key = next(key for prefix, key in reused_prefixes.items() if label.startswith(prefix))
            assert alpha_callers['noCallProof']['cases'][key]['changedCalleeUnreachable']
            assert alpha_callers['noCallProof']['cases'][key]['reusePriorPerformance']
            reused.add(key)
            continue
        if key in {'bicubic-TightUp8', 'bicubic-TightHorizontalUp8', 'bicubic-TightVerticalUp8', 'bicubic-Wide8'}:
            refreshed.add(key)
            continue  # Current owner above supersedes the earlier alpha cost.
        observation = (alpha_callers if key == 'bicubic-Up16' else exact_alpha)['cases'][key][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert receipt['librarySha256'] == exact_alpha['producers']['linuxSha256']
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        rows[index] = (kind, f'| {label} | {stats["Mean"] / 1000:,.2f} | {min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                       f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | {observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |')
        refreshed.add(key)
    assert refreshed == (set(exact_alpha['cases']) - {'bilinear-Up8'}) | {'bicubic-Up16'}
    assert reused == set(alpha_callers['noCallProof']['cases']) - {'bilinear-Down8', 'bilinear-Down16'}
    progressive = json.loads((ROOT / 'benchmarks/current-progressive-jpeg-costs-evidence.json').read_text(encoding='utf-8'))
    assert not progressive['productionChanged'] and progressive['allBuilds'] == 0
    assert progressive['producers'] == exact_alpha['producers']
    assert progressive['preparation']['originalHeadersRestored']['originalHeadersByteExact']
    for key, observations in progressive['cases'].items():
        observation = observations[-1]
        receipt = observation['receipt']
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        assert receipt['librarySha256'] == exact_alpha['producers']['linuxSha256']
        assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256']
        assert receipt['decoderMaximum'] == receipt['scalarDecoderMaximum'] == 0
        assert progressive['comparisonLowerBounds'][key] > 1
    retained_png = json.loads((ROOT / 'benchmarks/current-retained-png-decode-cost-evidence.json').read_text(encoding='utf-8'))
    assert retained_png['producers'] == exact_alpha['producers'] and retained_png['allBuilds'] == 0
    observation = retained_png['cases']['Png8Retained2'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == exact_alpha['producers']['linuxSha256']
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert all(a['Operations'] == 4 for a in observation['managed']['actual'])
    assert all(q['passed'] for q in observation['quietness'].values())
    assert all(retained_png['correctness'].values())
    assert retained_png['comparisonLowerBounds']['Png8Retained2'] > 1
    rows.append(('codec', f'| PNG8 decode, two retained images / shared caller root † | {stats["Mean"] / 1000:,.2f} | '
                 f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                 f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                 f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | — |'))
    local_repair = json.loads((ROOT / 'benchmarks/warm-bicubic-local-repair-evidence.json').read_text(encoding='utf-8'))
    assert local_repair['qualification']['fullChecksEachOs'] == 5285
    assert local_repair['qualification']['sameActualPackageBothOs']
    assert local_repair['decisions']['confirm']['defaultFivePercentPassed']
    assert local_repair['decisions']['confirm']['normalizedSeparated']
    observation = local_repair['cases']['TightDown8'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == local_repair['producers']['linuxSha256']
    assert all(q['passed'] for q in observation['quietness'].values())
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    updated = 0
    for index, (kind, row) in enumerate(rows):
        label = row.split('|')[1].strip()
        if kind == 'editing' and label.startswith('Bicubic RGBA8 reduction,'):
            rows[index] = (kind, f'| {label} | {stats["Mean"] / 1000:,.2f} | '
                           f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                           f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                           f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |')
            updated += 1
    assert updated == 1
    compact_geometry = json.loads((ROOT / 'benchmarks/compact-mixed-bicubic-geometry-evidence.json').read_text(encoding='utf-8'))
    assert compact_geometry['productionAccepted'] and compact_geometry['requiredMaterialPaths'] == 289
    assert compact_geometry['raw']['mirrored'] and compact_geometry['raw']['allMembersVerified']
    assert compact_geometry['statusCounts'] == {'pass': 2, 'gap': 0, 'uncertain': 2}
    assert compact_geometry['sourceProof']['bothExecutionSourceBodiesRestoredByRevertingOnlyLoanExtent']
    assert compact_geometry['byteBaselineProjectionExact'] and compact_geometry['sourceValidProfileReused']
    for current in compact_geometry['cases'].values():
        assert current['librarySha256'] == compact_geometry['producers']['windowsSha256']
        assert current['managedRationalReferenceExact'] and current['independentOwnedOutput']
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    current_word = compact_geometry['cases']['TightMixed16']
    compact_geometry_prefixes = tuple(f'| Bicubic RGBA{depth}, width up / height down, input+output+4 KiB'
                                      for depth in (8, 16))
    result_bands = json.loads((ROOT / 'benchmarks/bicubic-result-backed-bands-evidence.json').read_text(encoding='utf-8'))
    for depth, current in [(8, compact_geometry['cases']['TightMixed8']), (16, current_word)]:
        qualifier = ' (confidence open)' if current['status'] == 'uncertain' else ''
        rows.append(('editing', f'| Bicubic RGBA{depth}, width up / height down, input+output+4 KiB{qualifier} † → 1027×129 | '
                     f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                     f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |'))
    packed_current = json.loads((ROOT / 'benchmarks/packed-horizontal-window-reuse-evidence.json').read_text(encoding='utf-8'))
    packed_qualification = packed_current['qualification']
    assert packed_current['productionAccepted'] and packed_current['requiredMaterialPaths'] == 289
    assert packed_current['allAffectedPerformanceReconciled'] and not packed_current['materialPassToGapContradictions']
    assert packed_current['raw']['mirrored'] and packed_current['raw']['allMembersVerified']
    assert packed_qualification['fullChecksEachOs'] == packed_qualification['windows']['total'] == packed_qualification['linux']['total'] == 6417
    assert packed_qualification['sameActualPackageBothOS']
    assert packed_qualification['actualPackage']['sha256'] == packed_qualification['samePackageLinux']['sameActualPackageSha256'] == packed_current['producers']['actualPackageSha256']
    assert packed_qualification['resizeInternalEachModeEachOs'] == 508 and packed_qualification['resizePublicEachModeEachOs'] == 789
    assert packed_qualification['actualPackage']['actualConsumerPassed'] and packed_qualification['samePackageLinux']['actualConsumerPassed']
    assert packed_current['sourceProof']['originalFileReconstructedExactly']
    assert packed_current['sourceProof']['allOldPairIndexedOddSuffixDecodeVerticalPublishCertificateAndRepairBodiesVerbatim']
    assert packed_current['storage']['workingAdmissionBytes'] == 4096
    assert packed_current['storage']['newOwnedCapacityBytes'] == packed_current['storage']['retainedOperationScratchBytes'] == 0
    effect = packed_current['comparison']['Tight64x32_8']
    assert effect['normalizedGainSeparated'] and all(v['rawGainSeparated'] for v in effect['views'].values())
    assert packed_current['statusCounts'] == {'pass': 1, 'gap': 0, 'uncertain': 1}
    adjacent_word = packed_current['adjacentWord']
    assert adjacent_word['status'] == 'pass' and adjacent_word['operationsPerSubject'] == 64
    assert adjacent_word['allPairsRetained'] and adjacent_word['threeNativeBracketsRetained']
    assert not adjacent_word['currentGeneralPassAccepted']
    for depth in (8, 16):
        current = packed_current['cases'][f'Tight64x32_{depth}']
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12 and current['allActualsRetained']
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['managedRationalReferenceExact'] and current['sourceUnchanged'] and current['independentOwnedOutput']
        assert current['librarySha256'] == packed_qualification['actualPackage']['librarySha256'] == packed_current['producers']['timedArtifactSha256']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
        assert current['status'] == ('pass' if depth == 8 else 'uncertain') and current['allocatedBytes'] == 113
        assert current['managedMaxWorkingBytes'] == (513 * 259 + 64 * 32) * (depth // 2) + 4096
        assert current['nativeRgbMaximum'] <= 1 and current['nativeAlphaMaximum'] <= 1
        assert current['nativeRgbRmse'] <= .25 and current['nativeAlphaRmse'] <= .25
        qualifier = ' (confidence open)' if depth == 16 else ''
        rows.append(('editing', f'| Bicubic RGBA{depth} reduction, input+output+4 KiB{qualifier} † → 64×32 | '
                     f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |'))
    bicubic_overlap = json.loads((ROOT / 'benchmarks/bicubic-wide-overlap-evidence.json').read_text(encoding='utf-8'))
    assert bicubic_overlap['productionAccepted'] and bicubic_overlap['requiredMaterialPaths'] == 283
    cq = bicubic_overlap['qualification']
    assert cq['fullChecksEachOs'] == cq['windows']['total'] == cq['linux']['total'] == 6120
    assert cq['sameActualPackageBothOS']
    assert cq['actualPackage']['sha256'] == cq['samePackageLinux']['sameActualPackageSha256']
    assert bicubic_overlap['raw']['mirrored'] and bicubic_overlap['raw']['allMembersVerified']
    assert bicubic_overlap['storage']['newOwnedHeapCapacityBytes'] == bicubic_overlap['storage']['newRetainedOperationStorageBytes'] == 0
    assert len(bicubic_overlap['cases']) == 10 and bicubic_overlap['statusCounts'] == {'pass': 7, 'gap': 3, 'uncertain': 0}
    for case, c in bicubic_overlap['cases'].items():
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12
        assert c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        # Byte horizontal identity keeps its declared binary64 half-tie decisions.
        assert c['managedRationalReferenceExact'] == (case != 'Horizontal8')
        assert c['managedQualifiedOriginalPixelsExact']
        assert c['sourceUnchanged'] and c['independentOwnedOutput']
        assert c['librarySha256'] == cq['actualPackage']['librarySha256']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated'] for v in bicubic_overlap['comparison']['views'].values())
    byte_publication = json.loads((ROOT / 'benchmarks/bicubic-shared-byte-publication-evidence.json').read_text(encoding='utf-8'))
    assert byte_publication['productionAccepted'] and byte_publication['requiredMaterialPaths'] == 283
    byte_publication_qualification = byte_publication['qualification']
    assert byte_publication_qualification['fullChecksEachOs'] == byte_publication_qualification['windows']['total'] == byte_publication_qualification['linux']['total'] == 6157
    assert byte_publication_qualification['actualPackage']['sha256'] == byte_publication_qualification['samePackageLinux']['sameActualPackageSha256']
    assert byte_publication_qualification['resizeInternalEachModeEachOs'] == 331 and byte_publication_qualification['resizePublicEachModeEachOs'] == 729
    assert byte_publication['raw']['mirrored'] and byte_publication['raw']['allMembersVerified']
    assert byte_publication['statusCounts'] == {'pass': 2, 'gap': 2, 'uncertain': 0}
    assert all(body['instructionsIdentical'] for bodies in byte_publication['byteCodeReuse']['byteBodies'].values() for body in bodies)
    assert set(byte_publication['cases']) == {'TightDown8', 'TightWide8', 'TightDown16', 'TightWide16'}
    for current in byte_publication['cases'].values():
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed'] and current['managedRationalReferenceExact']
        expected = byte_publication['byteTimedWindowsSha256'] if current['reusedUnchangedPath'] else byte_publication_qualification['actualPackage']['librarySha256']
        assert current['librarySha256'] == expected
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    bicubic_overlap['cases'].update(byte_publication['cases'])
    for case in ('TightWide8', 'TightWide16'):
        cost = bicubic_overlap['cases'][case]
        depth = 8 if case == 'TightWide8' else 16
        rows.append(('editing', f'| Bicubic RGBA{depth} reduction, input+output+4 KiB † → 128×64 | '
                     f'{cost["managedUs"]:,.2f} | {cost["nativeUs"]:,.2f} | '
                     f'{cost["ratio"]:.3f} / {cost["upper"]:.3f} | {cost["allocatedBytes"]:,} |'))
    jpeg_bank = json.loads((ROOT / 'benchmarks/jpeg-prepared-huffman-bank-evidence.json').read_text(encoding='utf-8'))
    assert jpeg_bank['productionAccepted'] and jpeg_bank['requiredMaterialPaths'] == 283
    assert jpeg_bank['raw']['mirrored'] and jpeg_bank['raw']['allMembersVerified']
    assert len(jpeg_bank['cases']) == 17
    assert jpeg_bank['statusCounts'] == {state: sum(c['status'] == state for c in jpeg_bank['cases'].values())
                                        for state in ('pass', 'gap', 'uncertain')}
    jpeg_tiles = json.loads((ROOT / 'benchmarks/jpeg-gray-tile-preparation-evidence.json').read_text(encoding='utf-8'))
    assert jpeg_tiles['productionAccepted'] and jpeg_tiles['requiredMaterialPaths'] == 283
    assert jpeg_tiles['raw']['mirrored'] and jpeg_tiles['raw']['allMembersVerified']
    assert set(jpeg_tiles['cases']) == {'RgbaGray', 'CallerGray'}
    assert jpeg_tiles['sourceProof']['onlyImageGrayPreparationScheduleChanged']
    assert jpeg_tiles['sourceProof']['normalizationTransformQuantizationEntropyReplayHeaderAndOwnershipSourceUnchanged']
    assert jpeg_tiles['qualification']['fullChecksEachOs'] == 6073
    assert jpeg_tiles['qualification']['actualPackage']['sha256'] == jpeg_tiles['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert jpeg_tiles['timedArtifactSha256'] == jpeg_tiles['qualification']['actualPackage']['librarySha256']
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated'] for v in jpeg_tiles['comparison']['views'].values())
    jpeg_encoding_cases = {**jpeg_bank['cases'], **jpeg_tiles['cases']}
    assert jpeg_tiles['statusCounts'] == {state: sum(c['status'] == state for c in jpeg_encoding_cases.values())
                                         for state in ('pass', 'gap', 'uncertain')}
    for name, c in jpeg_encoding_cases.items():
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12
        assert c['librarySha256'] == (jpeg_tiles['timedArtifactSha256'] if name in jpeg_tiles['cases']
                                      else jpeg_bank['producers']['linuxSha256'])
        assert c['allOriginalGatesPassed'] and c['quietGuardsPassed'] and c['independentOwnedOutput']
        assert c['completePacketExact'] and c['metadataExceptionBytes'] == (0 if name in ('RgbaGray', 'CallerGray') else 2)
        assert c['nativePacketBytes'] - c['managedPacketBytes'] == c['metadataExceptionBytes']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    assert jpeg_bank['sourceProof']['recordGenerationOwnershipGrowCommitDisposeExact']
    assert jpeg_bank['sourceProof']['writerValidationPollAppendStuffingFlushDisposeByteExact']
    jpeg_labels = {
        'Encode444Stream8': 'JPEG 4:4:4, forward-only / exact reference',
        'CallerRGB420': 'JPEG 4:2:0 from caller RGB samples',
        'GovernedRGBA16': 'JPEG RGBA16 4:2:0 / caller IMemoryBudget',
        'RgbaGray': 'JPEG grayscale, standalone SIMD libjpeg-turbo',
        'CallerGray': 'JPEG from caller GRAY8, standalone SIMD libjpeg-turbo',
        'JpegRgb420AsyncSamples': 'JPEG caller RGB24 async nonseekable, 420',
        'Jpeg8AsyncNonSeekable': 'JPEG RGBA8 async nonseekable, 444',
    }
    for name, label in jpeg_labels.items():
        c = jpeg_encoding_cases[name]
        positions = [i for i, (_, row) in enumerate(rows) if row.startswith('| ' + label)]
        assert len(positions) == 1, label
        qualifier = {'pass': 'parity confirmed', 'gap': 'confirmed gap', 'uncertain': 'confidence open'}[c['status']]
        rows[positions[0]] = ('codec', f'| {label} ({qualifier}) † | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | '
                              f'{c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | '
                              f'{c["managedPacketBytes"]:,} / {c["nativePacketBytes"]:,} |')
    mixed_costs = json.loads((ROOT / 'benchmarks/current-default-mixed-bicubic-costs-evidence.json').read_text(encoding='utf-8'))
    assert not mixed_costs['productionChanged'] and mixed_costs['allBuilds'] == mixed_costs['newCases'] == 0
    assert mixed_costs['qualificationReuse']['sameActualPackageBothOs']
    for case, depth in [('Cross8', 8), ('Cross16', 16)]:
        observation = mixed_costs['cases'][case][-1]
        receipt = observation['receipt']
        assert len(observation['managed']['actual']) == 12 and all(q['passed'] for q in observation['quietness'].values())
        assert receipt['librarySha256'] == mixed_costs['producers']['linuxSha256']
        assert receipt['managedQualifiedOriginalPixelsExact'] and receipt['managedMaxWorkingBytes'] is None
        assert mixed_costs['comparisonLowerBounds'][case] > 1
        rows.append(('editing', f'| Bicubic RGBA{depth}, width down / height up, default workspace † → 256×519 | '
                     f'{observation["managed"]["statistics"]["Mean"] / 1000:,.2f} | '
                     f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
    # Current axis-swapping costs have one owner; superseded tile stages remain history.
    turn_current = json.loads((ROOT / 'benchmarks/shared-axis-swap-evidence.json').read_text(encoding='utf-8'))
    turn_q = turn_current['qualification']
    assert turn_current['productionAccepted'] and turn_current['allAffectedPerformanceReconciled']
    assert turn_current['requiredMaterialPaths'] == 289 and turn_current['newMaterialPaths'] == 0
    assert turn_current['raw']['mirrored'] and turn_current['raw']['allMembersVerified']
    assert turn_q['windows']['total'] == turn_q['linux']['total'] == turn_q['fullChecksEachOs'] == 6600
    assert turn_q['actualPackage']['sha256'] == turn_q['samePackageWindows']['sameActualPackageSha256']
    assert turn_q['actualPackage']['actualConsumerPassed'] and turn_q['samePackageWindows']['actualConsumerPassed']
    assert turn_q['actualPackage']['usageExamples'] == turn_q['samePackageWindows']['usageExamples'] == 3
    for case, label in [('WarmTranspose16', 'Plain transpose RGBA16'),
                        ('WarmTranspose8', 'Plain transpose RGBA8'),
                        ('WarmCounterclockwise8', 'Counterclockwise quarter turn RGBA8'),
                        ('WarmCounterclockwise16', 'Counterclockwise quarter turn RGBA16'),
                        ('WarmTransverse8', 'Mirrored transpose RGBA8')]:
        c = turn_current['cases'][case]
        assert c['librarySha256'] == turn_q['actualPackage']['librarySha256'] == turn_current['producers']['timedArtifactSha256']
        assert c['actualIterations'] == 12 and c['allActualsRetained']
        assert c['managedExact'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
        if c['status'] != 'pass': label += f" ({c['status']})"
        rows.append(('editing', f"| {label} † → 259×513 | {c['managedUs']:,.2f} | {c['nativeUs']:,.2f} | {c['ratio']:.3f} / {c['upper']:.3f} | {c['allocatedBytes']:,} |"))
    # Current JPEG costs have one owner; superseded algorithms/experiments remain
    # linked history rather than prerequisites of every summary rendering.
    jpeg_read = json.loads((ROOT / 'benchmarks/jpeg-shared-block-support-evidence.json').read_text(encoding='utf-8'))
    jpeg_qualification = jpeg_read['qualification']
    assert jpeg_read['productionAccepted'] and jpeg_read['requiredMaterialPaths'] == 289
    assert jpeg_read['allAffectedPerformanceReconciled'] and not jpeg_read['materialPassToGapContradictions']
    assert jpeg_qualification['fullChecksEachOs'] == jpeg_qualification['windows']['total'] == jpeg_qualification['linux']['total'] == 6248
    assert jpeg_qualification['sameActualPackageBothOS']
    assert jpeg_qualification['actualPackage']['sha256'] == jpeg_qualification['samePackageLinux']['sameActualPackageSha256']
    assert jpeg_read['raw']['mirrored'] and jpeg_read['raw']['allMembersVerified']
    assert all(jpeg_read['sourceProof'][key] for key in ('entireInverseSourceReconstructedAfterOnlyDequantizationSupportProducersAndCallPreludeRestored',
        'entropyGrammarInt32CoefficientsAndDecoderCallSitesUnchanged', 'scalarMatrixAndAllInverseNumericalOperationsVerbatim',
        'encoderAndFormatScanUnchanged', 'supportReflectsActualProductsAndCorrectedSaturatedDc'))
    assert jpeg_read['storage']['newPerImageOwnedCapacityBytes'] == jpeg_read['storage']['retainedOperationScratchBytes'] == 0
    assert len(jpeg_read['refreshedCases']) == 27 and len(jpeg_read['reusedCases']) == 2 and len(jpeg_read['cases']) == 29
    for case, cost in jpeg_read['cases'].items():
        assert cost['managedActuals'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['quietGuardsPassed'] and cost['allOriginalGatesPassed']
        assert cost['reusedUnchangedPath'] == (case in jpeg_read['reusedCases'])
        assert cost['reusedUnchangedPath'] or cost['librarySha256'] == jpeg_qualification['actualPackage']['librarySha256']
        assert cost['status'] == ('confirmed-pass-stop' if cost['upper'] <= 1 else 'confirmed-gap' if cost['lower'] > 1 else 'confirmed-confidence-open')
        assert math.isclose(cost['managedMicroseconds'] / cost['nativeMicroseconds'], cost['ratio'], rel_tol=1e-12)
    assert all(c['normalizedIntervalsSeparate'] and all(v['rawIntervalsSeparate'] for v in c['views'].values())
               for c in jpeg_read['comparison'].values())
    jpeg_current = jpeg_read['cases']
    jpeg_counts = {state: sum(c['status'] == state for c in jpeg_current.values())
                   for state in ('confirmed-pass-stop', 'confirmed-gap', 'confirmed-confidence-open')}
    assert jpeg_counts == jpeg_read['statusCounts']
    replacements = [('NativeDecode420', 'JPEG baseline420, resident decode'), ('CmykCaller', 'JPEG DC-only, caller CMYK decode'), ('CmykAdobeSparse2D', 'JPEG CMYK sparse-2D, resident decode'), ('Ycck420', 'JPEG YCCK420, resident decode'), ('Decode411', 'JPEG 4:1:1, resident decode'), ('Decode410', 'JPEG 4:1:0, resident decode'), ('CmykAdobeGeneral', 'JPEG dense CMYK, resident decode'), ('CmykAdobeAc', 'JPEG horizontal-only CMYK, resident decode'), ('Ycck420CallerRgb', 'JPEG YCCK420, caller RGB24 decode')]
    replacements += [('ProgressiveGray', 'JPEG progressive grayscale, resident decode'),
                     ('Progressive420', 'JPEG progressive420, resident decode'),
                     ('Progressive422', 'JPEG progressive422, resident decode'),
                     ('Progressive444', 'JPEG ordinary progressive444, resident decode'),
                     ('ProgressiveCancelable444', 'JPEG progressive444, live-token resident decode')]
    for case, label in replacements:
        c = jpeg_current[case]
        qualifier = ' (confirmed pass)' if c['status'] == 'confirmed-pass-stop' else ' (confidence open)' if c['status'] == 'confirmed-confidence-open' else ''
        row = (f'| {label}{qualifier} † | {c["managedMicroseconds"]:,.2f} | '
               f'{c["nativeMicroseconds"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | '
               f'{c["allocatedBytes"]:,} | — |')
        if case in {'Ycck420CallerRgb', 'CmykAdobeGeneral', 'CmykAdobeAc', 'ProgressiveGray', 'Progressive420', 'Progressive422', 'Progressive444', 'ProgressiveCancelable444'}:
            rows.append(('codec', row))
        else:
            position = next(i for i, (kind, previous) in enumerate(rows) if previous.startswith('| ' + label))
            rows[position] = ('codec', row)
    rectangle = json.loads((ROOT / 'benchmarks/rectangle-copy-spans-evidence.json').read_text(encoding='utf-8'))
    assert rectangle['requiredMaterialPaths'] == 282 and rectangle['newRequiredCases'] == 0
    assert rectangle['qualification']['sameActualPackageBothOS'] and rectangle['raw']['allMembersVerified']
    for case, label in [('DrawingCopy8', 'Exact rectangle copy into borrowed RGBA8 canvas'),
                        ('DrawingCopy16', 'Exact rectangle copy into borrowed RGBA16 canvas (confidence open)')]:
        c = rectangle['cases'][case]['cost']
        assert c['actuals'] == c['nativeSamplesEachBracket'] == 12
        assert rectangle['cases'][case]['confirmation']['receipt']['allRgbaSamplesExact']
        rows.append(('editing', f'| {label} | {c["managedMicroseconds"]:,.2f} | {c["nativeMicroseconds"]:,.2f} | '
                     f'{c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} |'))
    fill = json.loads((ROOT / 'benchmarks/rectangle-fill-packed-evidence.json').read_text(encoding='utf-8'))
    assert fill['requiredMaterialPaths'] == 282 and fill['newRequiredCases'] == 0
    assert fill['qualification']['sameActualPackageBothOS'] and fill['raw']['allMembersVerified']
    for case, label in [('DrawingFill8', 'Exact clipped fill into borrowed RGBA8 canvas (parity confirmed)'),
                        ('DrawingFill16', 'Exact clipped fill into borrowed RGBA16 canvas (parity confirmed)')]:
        c = fill['cases'][case]['cost']
        assert c['actuals'] == c['nativeSamplesEachBracket'] == 12 and c['status'] == 'confirmed-pass-stop'
        assert c['upper'] <= 1 and fill['cases'][case]['confirmation']['receipt']['allRgbaSamplesExact']
        rows.append(('editing', f'| {label} | {c["managedMicroseconds"]:,.2f} | {c["nativeMicroseconds"]:,.2f} | '
                     f'{c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} |'))
    overlap = json.loads((ROOT / 'benchmarks/current-overlapping-copy-cost-evidence.json').read_text(encoding='utf-8'))
    c = overlap['cost']
    assert not overlap['productionChanged'] and c['actuals'] == c['nativeSamplesEachBracket'] == 12
    assert c['publicMovesPerPair'] == 2 and c['nativeCallsPerPair'] == 6 and c['lower'] > 1
    assert overlap['rejectedTrial']['restoration']['productionRestoredByteExactly']
    rows.append(('editing', f'| Overlapping RGBA8 copy + reverse restoration, per pair (gap) | {c["managedMicroseconds"]:,.2f} | '
                 f'{c["nativeMicroseconds"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} |'))
    text_current = json.loads((ROOT / 'benchmarks/text-drawing-current-evidence.json').read_text(encoding='utf-8'))
    assert text_current['allNinePreviouslyMissingMeasured'] and text_current['requiredMaterialPaths'] == 282
    font_load = json.loads((ROOT / 'benchmarks/font-owned-storage-evidence.json').read_text(encoding='utf-8'))
    assert font_load['productionAccepted'] and font_load['requiredMaterialPaths'] == 283
    assert font_load['sourceProof']['parserAndLookupAndAdmissionSourceUnchanged']
    assert font_load['sourceProof']['poolOriginalFieldsRentReturnSourceUnchanged']
    assert font_load['raw']['mirrored'] and font_load['raw']['allMembersVerified']
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated']
               for v in font_load['comparison']['views'].values())
    assert font_load['current']['librarySha256'] == font_load['producers']['linuxSha256']
    assert font_load['current']['allOriginalGatesPassed'] and font_load['current']['conservativeRatioLower'] > 1
    assert font_load['storage']['addedOwnedPayloadBytes'] == 0
    assert font_load['storage']['globalPoolCeilingBytes'] == 4194304
    assert font_load['storage']['globalPoolIdleSlots'] == 2
    assert font_load['current']['managedMemory']['BytesAllocatedPerOperation'] == 176
    composites = json.loads((ROOT / 'benchmarks/compositing-shared-rows-evidence.json').read_text(encoding='utf-8'))
    assert composites['productionAccepted'] and composites['independentQuietConfirmation']
    assert composites['qualification']['fullChecksEachOs'] == 5423
    assert composites['qualification']['actualPackage']['sha256'] == composites['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert composites['cases']['DrawingComposite8']['status'] == 'Confirmed gap'
    assert composites['cases']['DrawingComposite16']['status'] == 'Confirmed pass'
    composite8_current = json.loads((ROOT / 'benchmarks/byte-composite-exact-vector-evidence.json').read_text(encoding='utf-8'))
    assert composite8_current['productionAccepted']
    assert composite8_current['current']['allOriginalGatesPassed'] and composite8_current['current']['quietGuardsPassed']
    assert composite8_current['current']['actualIterations'] == 12
    assert composite8_current['current']['librarySha256'] == composite8_current['actualPackage']['librarySha256']
    assert composite8_current['allocation']['refreshedOnActualTimedDll']
    assert composite8_current['current']['upper'] < 1
    assert composite8_current['qualification']['fullChecksEachOs'] == 5975
    assert composite8_current['actualPackage']['sha256'] == composite8_current['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert composite8_current['raw']['mirrored'] and composite8_current['raw']['allMembersVerified']
    raster = json.loads((ROOT / 'benchmarks/text-render-preparation-evidence.json').read_text(encoding='utf-8'))
    assert raster['productionAccepted'] and raster['independentQuietConfirmation']
    assert raster['qualification']['fullChecksEachOs'] == 6244
    assert raster['qualification']['actualPackage']['sha256'] == raster['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert raster['storage']['allEightFrozenFramesExact']
    assert raster['equivalentPreparedReference']['allEightReferenceFramesExact']
    assert raster['sourceProof']['drawAndBandExactAfterOnlyCopiedGlyphStorageAndExplicitExecutionTokenSubstitution']
    assert raster['raw']['mirrored'] and raster['raw']['allMembersVerified']
    for case in raster['cases'].values():
        assert case['librarySha256'] == raster['producers']['timedArtifactSha256']
        assert case['managedExact'] and case['allOriginalGatesPassed'] and case['quietGuardsPassed']
        assert case['actualIterations'] == case['nativeSamplesEachBracket'] == 12
        assert case['status'] == ('pass' if case['upper'] <= 1 else 'gap' if case['lower'] > 1 else 'uncertain')
        assert math.isclose(case['ratio'], case['managedUs'] / case['nativeUs'], rel_tol=1e-12)
        prepared = case['caseId'].startswith('Prepared')
        assert case['allocatedBytes'] == (56 if prepared else 59304)
        assert case['retainedBytes'] == (40408 if prepared else 0)
        depth = case['depth']
        status = 'confidence open' if case['status'] == 'uncertain' else case['status']
        label = f'Text {"prepared raster" if prepared else "ordinary draw"} RGBA{depth} / ImageSharp.Drawing ({status})'
        rows.append(('editing', f'| {label} † | {case["managedUs"]:,.2f} | {case["nativeUs"]:,.2f} | '
                     f'{case["ratio"]:.3f} / {case["upper"]:.3f} | {case["allocatedBytes"]:,} |'))
    for case_id, previous in text_current['observations'].items():
        if case_id in ('DrawText8', 'DrawText16'):
            continue  # Current ordinary and explicitly prepared boundaries above.
        if case_id == 'DrawingComposite8':
            current = composite8_current['current']
            rows.append(('editing', f'| {composites["cases"][case_id]["label"]} (pass) † | {current["managedUs"]:,.2f} | '
                         f'{current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | '
                         f'{current["allocatedBytes"]:,} |'))
            continue
        case = (font_load['current'] if case_id == 'FontLoadRegular'
                else composites['cases'].get(case_id, previous))
        if case_id in composites['cases']:
            assert case['librarySha256'] == composites['producers']['linuxSha256']
            assert case['validation']['exactManagedPixels'] and case['managedMemory']['BytesAllocatedPerOperation'] == 56
        assert len(case['managed']['actual']) == 12 and case['quietGuardsPassed']
        assert case['approvedScopeDate'] == '2026-11-02'
        native_mean = min(bracket['mean'] for bracket in case['nativeBrackets'])
        status = 'pass' if case['status'] == 'Confirmed pass' else 'gap' if case['status'] == 'Confirmed gap' else 'confidence open'
        mark = ' †' if case_id == 'FontLoadRegular' else ''
        rows.append(('editing', f'| {case["label"]} ({status}){mark} | {case["managed"]["statistics"]["Mean"] / 1000:,.2f} | '
                     f'{native_mean / 1000:,.2f} | {case["worstBracketRatio"]:.3f} / {case["conservativeRatioUpper"]:.3f} | '
                     f'{case["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
    lanczos_current = json.loads((ROOT / 'benchmarks/lanczos-paired-word-rows-evidence.json').read_text(encoding='utf-8'))
    lanczos_qualification = lanczos_current['qualification']
    assert lanczos_current['productionAccepted'] and lanczos_current['storage']['primaryExtraOwnedCapacityBytes'] == 8208
    assert lanczos_current['sourceProof']['existingByteSingleAndPairedWordArithmeticSourceVerbatim']
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated']
               for case in ['Repeated16', 'OneShot16']
               for v in lanczos_current['mechanismComparisons'][case]['views'].values())
    assert all(v['normalizedGainSeparated'] for v in lanczos_current['currentComparisons']['Repeated16']['views'].values())
    assert not any(v['normalizedLossSeparated'] for v in lanczos_current['currentComparisons']['Repeated8']['views'].values())
    assert lanczos_qualification['fullChecksEachOs'] == 6197
    assert lanczos_qualification['actualPackage']['sha256'] == lanczos_qualification['samePackageLinux']['sameActualPackageSha256']
    assert lanczos_current['raw']['mirrored'] and lanczos_current['raw']['allMembersVerified']
    assert len(lanczos_current['cases']) == 8
    assert lanczos_current['statusCounts'] == {'pass': 6, 'gap': 1, 'uncertain': 1}
    assert lanczos_current['cases']['RepeatedEnlarge16']['upper'] <= 1
    # Only the current displayed confidence depends on this read-only owner;
    # its unselected implementations remain linked history, not renderer gates.
    lanczos_confidence = json.loads((ROOT / 'benchmarks/lanczos-byte-product-preparation-evidence.json').read_text(encoding='utf-8'))
    assert lanczos_confidence['measurementAccepted'] and not lanczos_confidence['productionChanged']
    assert lanczos_confidence['requiredMaterialPaths'] == 289
    assert lanczos_confidence['raw']['mirrored'] and lanczos_confidence['raw']['allMembersVerified']
    byte_confidence = lanczos_confidence['case']
    assert byte_confidence['librarySha256'] == lanczos_confidence['producers']['windowsSha256']
    assert byte_confidence['actualIterations'] == byte_confidence['nativeSamplesEachBracket'] == 12
    assert byte_confidence['managedExact'] and byte_confidence['sourceUnchanged'] and byte_confidence['independentOwnedOutput']
    assert byte_confidence['allOriginalGatesPassed'] and byte_confidence['quietGuardsPassed']
    assert byte_confidence['status'] == 'uncertain' and byte_confidence['lower'] <= 1 < byte_confidence['upper']
    assert byte_confidence['retainedBytes'] == 139800 and byte_confidence['allocatedBytes'] == 138
    assert math.isclose(byte_confidence['ratio'], byte_confidence['managedUs'] / byte_confidence['nativeUs'], rel_tol=1e-12)
    assert all(lanczos_current['sources'][name] == digest
               for name, digest in lanczos_confidence['sourceProof']['restoredExactly'].items())
    lanczos_costs = {**lanczos_current['cases'], 'Repeated8': byte_confidence}
    position = next(i for i, (kind, _) in enumerate(rows) if kind == 'editing')
    current_resize_rows = []
    for case, label, size in [
            ('OneShot8', 'Lanczos3 one-shot RGBA8 / zimg one-shot', '256×129'),
            ('Repeated8', 'Lanczos3 prepared RGBA8 / zimg prepared', '256×129'),
            ('OneShot16', 'Lanczos3 one-shot RGBA16 / zimg one-shot', '256×129'),
            ('Repeated16', 'Lanczos3 prepared RGBA16 / zimg prepared', '256×129'),
            ('OneShotEnlarge8', 'Lanczos3 one-shot RGBA8 / zimg one-shot', '1027×519'),
            ('RepeatedEnlarge8', 'Lanczos3 prepared RGBA8 / zimg prepared', '1027×519'),
            ('OneShotEnlarge16', 'Lanczos3 one-shot RGBA16 / zimg one-shot', '1027×519'),
            ('RepeatedEnlarge16', 'Lanczos3 prepared RGBA16 / zimg prepared', '1027×519')]:
        observation = lanczos_costs[case]
        assert observation['managedExact'] and observation['allOriginalGatesPassed'] and observation['quietGuardsPassed']
        assert observation['reusedUnchangedPath'] or observation['librarySha256'] == lanczos_current['producers']['timedArtifactSha256']
        assert observation['actualIterations'] == 12
        current_resize_rows.append(('editing', f'| {label} † → {size} | {observation["managedUs"]:,.2f} | '
                                    f'{observation["nativeUs"]:,.2f} | {observation["ratio"]:.3f} / '
                                    f'{observation["upper"]:.3f} | {observation["allocatedBytes"]:,} |'))
    rows[position:position] = current_resize_rows
    current_vp8l_prefixes = ('| VP8L RGBA8 /', '| VP8L opaque RGBA8 /',
                             '| VP8L RGBA16, including normalization /', '| VP8L async, fresh nonseekable output /')
    historical_workspace = lambda row: (row.startswith('| VP8L ') and 'decode' not in row
                                      and not row.startswith(current_vp8l_prefixes)
                                      or row.startswith('| VP8 RGBA8 †')
                                      or row.startswith('| VP8 RGBA16 + alpha,'))
    assert all(sum(row.startswith(prefix) for _, row in rows) == 1 for prefix in current_vp8l_prefixes)
    rows = [(kind, row.replace(' † |', ' † ‡ |', 1) if historical_workspace(row) else row)
            for kind, row in rows]
    assert webp_current['productionAccepted'] and webp_current['decision']['accepted']
    assert webp_current['decision']['storageTradeoffMeasured']
    assert webp_current['qualification']['fullChecksEachOs'] == 5507
    assert webp_current['qualification']['actualPackage']['sha256'] == webp_current['qualification']['samePackageLinux']['sameActualPackageSha256']
    position = next(i for i, (_, row) in enumerate(rows) if row.startswith('| VP8L RGBA8 /'))
    current_webp_rows = []
    for case, label in [('photograph', 'VP8L photograph / libwebp exact=1'),
                        ('diagram', 'VP8L text/diagram / libwebp exact=1')]:
        observation = webp_current['cases'][case]
        assert observation['completePacketExact'] and observation['quietGuardsPassed']
        assert observation['librarySha256'] == webp_current['producers']['linuxSha256']
        assert len(observation['managed']['actual']) == 12
        stats = observation['managed']['statistics']; receipt = observation['receipt']
        native = min(b['mean'] for b in observation['nativeBrackets'])
        current_webp_rows.append(('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | '
                                  f'{native / 1000:,.2f} | {observation["worstBracketRatio"]:.3f} / '
                                  f'{observation["conservativeRatioUpper"]:.3f} | '
                                  f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | '
                                  f'{receipt["managedOutputBytes"]:,} / {receipt["nativeOutputBytes"]:,} |'))
    rows[position:position] = current_webp_rows
    cached_resize = json.loads((ROOT / 'benchmarks/bicubic-shared-cached-driver-evidence.json').read_text(encoding='utf-8'))
    assert cached_resize['productionAccepted'] and cached_resize['decision']['wideControlParityPreserved']
    assert cached_resize['qualification']['fullChecksEachOs'] == 5416
    assert cached_resize['qualification']['actualPackage']['sha256'] == cached_resize['qualification']['samePackageLinux']['sameActualPackageSha256']
    current_cached = {'Bicubic resize RGBA8': 'Down8', 'Bicubic upscale RGBA16': 'Up16',
                      'Bicubic RGBA8, 32×16 / default workspace': 'Down32x16_8'}
    refreshed_cached = set()
    for i, (kind, row) in enumerate(rows):
        if kind != 'editing' or not row.startswith('| Bicubic') or row.startswith(compact_geometry_prefixes):
            continue
        label = row.split('|')[1].strip()
        case = next((case for prefix, case in current_cached.items() if label.startswith(prefix)), None)
        if case is None:
            rows[i] = (kind, row.replace(' →', ' ¶ →', 1))
            continue
        observation = cached_resize['cases'][case]
        assert observation['librarySha256'] == cached_resize['producers']['linuxSha256']
        assert observation['receipt']['managedRationalReferenceExact'] and observation['quietGuardsPassed']
        assert len(observation['managed']['actual']) == 12
        if '†' not in label:
            label = label.replace(' →', ' † →', 1)
        stats = observation['managed']['statistics']; native = min(b['mean'] for b in observation['nativeBrackets'])
        rows[i] = (kind, f'| {label} | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                         f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                         f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |')
        refreshed_cached.add(case)
    assert refreshed_cached == set(current_cached.values())
    # One historical-cost marker covers all affected families. Their reports
    # retain the distinct source changes and older observations.
    rows = [(kind, row.replace(' ‡', '').replace(' §', '').replace(' ¶', '')
             if kind == 'editing' and row.startswith(('| Bicubic RGBA8 enlargement,', '| Bicubic RGBA8 width enlargement,', '| Bicubic RGBA8 height enlargement,'))
             else row.replace(' ‡', ' H').replace(' §', ' H').replace(' ¶', ' H'))
            for kind, row in rows]
    # The output allocator changes both filters; overlay only refreshed controls.
    overwrite = json.loads((ROOT / 'benchmarks/resize-overwrite-output-storage-evidence.json').read_text(encoding='utf-8'))
    assert overwrite['productionAccepted'] and overwrite['independentQuietConfirmation']
    assert overwrite['qualification']['fullChecksEachOs'] == 5497
    assert overwrite['qualification']['actualPackage']['sha256'] == overwrite['qualification']['samePackageLinux']['sameActualPackageSha256']
    rows = [(kind, row.replace(' →', ' H →', 1)
             if kind == 'editing' and row.startswith(('| Bicubic', '| Bilinear')) and ' H ' not in row
             and not row.startswith(('| Bicubic RGBA8 enlargement,', '| Bicubic RGBA8 width enlargement,', '| Bicubic RGBA8 height enlargement,'))
             and not row.startswith(compact_geometry_prefixes)
             else row) for kind, row in rows]
    # Fresh package owners supersede the blanket historical marker.
    for label, current in [
        ('Bicubic RGBA8 wider-support reduction † → 128×64', wide_byte['cases']['Wide8']),
        ('Bicubic RGBA8 reduction, input+output+4 KiB † → 64×32', packed_current['cases']['Tight64x32_8']),
        ('Bicubic RGBA16 reduction, input+output+4 KiB (confidence open) † → 64×32', packed_current['cases']['Tight64x32_16']),
        ('Bicubic RGBA8 reduction, input+output+4 KiB † → 128×64', bicubic_overlap['cases']['TightWide8']),
        ('Bicubic RGBA16 reduction, input+output+4 KiB † → 128×64', bicubic_overlap['cases']['TightWide16']),
    ]:
        positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing'
                     and row.split('|')[1].strip().replace(' H ', ' ') == label]
        assert len(positions) == 1
        rows[positions[0]] = ('editing', f'| {label} | {current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |')
    labels = {'Down8': 'Bicubic resize RGBA8', 'Up16': 'Bicubic upscale RGBA16',
              'Down32x16_8': 'Bicubic RGBA8, 32×16 / default workspace',
              'Up8': 'Bicubic upscale RGBA8, default workspace'}
    assert set(overwrite['cases']) == set(labels)
    for case, label in labels.items():
        if case == 'Down32x16_8':
            continue  # The current reduction owner below supplies this row.
        if case in {'Up8', 'Up16'}:
            current = enlargement_modes['cases'][case]
            row = ('editing', f'| {label} † → {current["outputWidth"]}×{current["outputHeight"]} | '
                   f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                   f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |')
        else:
            observation = overwrite['cases'][case]; receipt = observation['receipt']
            assert receipt['librarySha256'] == overwrite['producers']['linuxSha256']
            assert receipt['managedRationalReferenceExact'] and observation['quietGuardsPassed']
            assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
            stats = observation['managed']['statistics']; native = min(b['mean'] for b in observation['nativeBrackets'])
            row = ('editing', f'| {label} † → {receipt["outputWidth"]}×{receipt["outputHeight"]} | '
                   f'{stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                   f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                   f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |')
        positions = [i for i, (_, old_row) in enumerate(rows) if old_row.startswith('| ' + label + ' †')]
        if case == 'Up8':
            assert not positions
            rows.insert(next(i for i, (_, old_row) in enumerate(rows) if old_row.startswith('| Bicubic upscale RGBA16 †')) + 1, row)
        else:
            assert len(positions) == 1, label
            rows[positions[0]] = row
    wide_family = wide_byte
    current = wide_family['cases']['Down32x16_8']
    assert wide_family['raw']['mirrored'] and wide_family['raw']['allMembersVerified']
    assert current['actualIterations'] == 12 and current['allOriginalGatesPassed'] and current['quietGuardsPassed']
    assert current['managedRationalReferenceExact'] and current['independentOwnedOutput']
    assert current['librarySha256'] == wq['actualPackage']['librarySha256']
    label = 'Bicubic RGBA8, 32×16 / default workspace † → 32×16'
    positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing'
                 and row.split('|')[1].strip().replace(' H ', ' ') == label]
    assert len(positions) == 1
    rows[positions[0]] = ('editing', f'| {label} | {current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                         f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |')
    # Current WebP costs use scoped unchanged-path reuse. Older reports
    # above retain their source-specific proofs; do not copy their experiment
    # histories or substitute old costs for this accepted stage's current rows.
    webp_owned = json.loads((ROOT / 'benchmarks/webp-huffman-workspace-evidence.json').read_text(encoding='utf-8'))
    assert webp_owned['productionAccepted']
    qualification = webp_owned['qualification']
    assert qualification['fullChecksEachOs'] == qualification['windows']['total'] == qualification['linux']['total']
    assert qualification['windows']['fullLivePassed'] and qualification['linux']['fullLivePassed']
    assert qualification['actualPackage']['sha256'] == qualification['samePackageLinux']['sameActualPackageSha256']
    assert qualification['actualPackage']['actualConsumerPassed'] and qualification['samePackageLinux']['actualConsumerPassed']
    assert webp_owned['raw']['mirrored'] and webp_owned['raw']['allMembersVerified']
    assert set(webp_owned['cases']) == {'ordinary', 'opaque', 'photograph', 'diagram', 'rgba16', 'async', 'alpha', 'alpha16'}
    for name in ['ordinary', 'opaque', 'diagram', 'rgba16', 'async']:
        current = webp_owned['cases'][name]
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['sourceUnchanged'] and current['independentOwnedOutput'] and current['completePacketExact']
        assert current['librarySha256'] == webp_owned['producers']['timedArtifactSha256']
        assert current['packetSha256'] == current['nativePacketSha256']
        assert current['managedPacketBytes'] == current['nativePacketBytes']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    adjacent_confidence = json.loads((ROOT / 'benchmarks/warm-adjacent-confidence-evidence.json').read_text(encoding='utf-8'))
    assert not adjacent_confidence['productionChanged'] and not adjacent_confidence['productionSpeedupClaim']
    assert adjacent_confidence['raw']['mirrored'] and adjacent_confidence['raw']['allMembersVerified']
    assert set(adjacent_confidence['cases']) == {'alpha', 'alpha16', 'Up16'}
    for case in adjacent_confidence['cases'].values():
        assert case['actualPairs'] == case['nativeBatchesEachBracket'] == 12 and case['allActualsRetained']
        assert case['allOriginalGatesPassed'] and case['quietGuardsPassed'] and case['independentOwnedOutputs']
        assert case['librarySha256'] == adjacent_confidence['producers']['windowsSha256']
        assert case['retainedOperationScratchBytes'] == 0
    alpha_current = adjacent_confidence['cases']['alpha']
    assert alpha_current['upper'] <= 1 and alpha_current['status'] == 'pass'
    assert alpha_current['completePacketExact'] and alpha_current['alphaExact']
    assert alpha_current['operationsPerSubject'] == 4
    assert alpha_current['managedPacketBytes'] == alpha_current['nativePacketBytes'] == 98590
    assert alpha_current['packetSha256'] == alpha_current['nativeOutputSha256']
    webp_owned['cases']['alpha'] = alpha_current
    alpha16_current = adjacent_confidence['cases']['alpha16']
    assert alpha16_current['lower'] <= 1 < alpha16_current['upper'] and alpha16_current['status'] == 'uncertain'
    assert alpha16_current['operationsPerSubject'] == 4 and alpha16_current['fullSourceNormalizationTimed']
    assert alpha16_current['completePacketExact'] and alpha16_current['alphaExact']
    assert alpha16_current['managedPacketBytes'] == alpha16_current['nativePacketBytes'] == 98590
    assert alpha16_current['packetSha256'] == alpha16_current['nativeOutputSha256']
    assert alpha16_current['raw']['mirrored'] and alpha16_current['raw']['allMembersVerified']
    webp_owned['cases']['alpha16'] = alpha16_current
    streaming = json.loads((ROOT / 'benchmarks/packed-streaming-paired-byte-rows-evidence.json').read_text(encoding='utf-8'))
    assert streaming['productionAccepted'] and streaming['requiredMaterialPaths'] == 283
    streaming_qualification = streaming['qualification']
    assert streaming_qualification['fullChecksEachOs'] == 6155
    assert streaming_qualification['actualPackage']['sha256'] == streaming_qualification['samePackageLinux']['sameActualPackageSha256']
    assert streaming_qualification['resizeInternalEachModeEachOs'] == 329
    assert streaming['raw']['mirrored'] and streaming['raw']['allMembersVerified']
    assert len(streaming['cases']) == 8 and streaming['statusCounts'] == {'pass': 7, 'gap': 1, 'uncertain': 0}
    for current in streaming['cases'].values():
        assert current['actualIterations'] == 12 and current['managedExact'] and current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        if not current['reusedUnchangedPath']:
            assert current['librarySha256'] == streaming['timedWindowsSha256'] == streaming_qualification['actualPackage']['librarySha256']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    current_labels = {
        'ordinary': 'VP8L RGBA8 / libwebp exact=1',
        'photograph': 'VP8L photograph / libwebp exact=1',
        'diagram': 'VP8L text/diagram / libwebp exact=1',
        'opaque': 'VP8L opaque RGBA8 / libwebp exact=1',
        'rgba16': 'VP8L RGBA16, including normalization / libwebp exact=1',
        'async': 'VP8L async, fresh nonseekable output / libwebp exact=1',
    }
    assert set(current_labels) <= set(webp_owned['cases'])
    photo_evidence = json.loads((ROOT / 'benchmarks/webp-lossless-photograph-confidence-evidence.json').read_text(encoding='utf-8'))
    photo = photo_evidence['current']
    assert not photo_evidence['productionChanged'] and photo_evidence['allBuilds'] == 0
    assert photo_evidence['raw']['mirrored'] and photo_evidence['raw']['allMembersVerified']
    assert photo['status'] == 'pass' and photo['upper'] <= 1
    assert photo['actualIterations'] == photo['nativeSamplesEachBracket'] == 12
    assert photo['pairedOperationsPerSubject'] == 4
    assert photo['managedExact'] and photo['completePacketExact'] and photo['independentOwnedOutput']
    assert photo['allOriginalGatesPassed'] and photo['quietGuardsPassed']
    assert photo['managedPacketBytes'] == photo['nativePacketBytes'] == 160032
    assert photo['packetSha256'] == photo['nativePacketSha256']
    assert photo['librarySha256'] == photo_evidence['producers']['windows']
    assert photo_evidence['producers']['actualPackageContainsTimedDll']
    opaque_evidence = json.loads((ROOT / 'benchmarks/webp-opaque-current-confidence-evidence.json').read_text(encoding='utf-8'))
    opaque = opaque_evidence['current']
    assert not opaque_evidence['productionChanged'] and not opaque_evidence['productionSpeedupClaim']
    assert opaque_evidence['requiredMaterialPaths'] == 289 and opaque_evidence['workflow']['driverBuilds'] == 1
    assert opaque_evidence['workflow']['onlyDriverAllowlistExtended'] and opaque_evidence['workflow']['productionEmbeddedPublicHarnessNativeFixtureOrPackageBuilds'] == 0
    assert opaque_evidence['raw']['mirrored'] and opaque_evidence['raw']['allMembersVerified']
    assert opaque['status'] == 'uncertain' and opaque['lower'] <= 1 < opaque['upper']
    assert opaque['actualIterations'] == opaque['nativeSamplesEachBracket'] == 12 and opaque['pairedOperationsPerSubject'] == 4
    assert opaque['allActualsRetained'] and opaque['allOriginalGatesPassed'] and opaque['quietGuardsPassed']
    assert opaque['managedExact'] and opaque['completePacketExact'] and opaque['independentOwnedOutput']
    assert opaque['managedPacketBytes'] == opaque['nativePacketBytes'] == 121896
    assert opaque['packetSha256'] == opaque['nativePacketSha256']
    assert opaque['librarySha256'] == opaque_evidence['producers']['windows'] == opaque_evidence['qualificationReused']['actualPackage']['librarySha256']
    assert opaque['retainedOperationScratchBytes'] == 0
    webp_owned['cases']['opaque'] = opaque
    for case, label in current_labels.items():
        owner = webp_owned
        current = photo if case == 'photograph' else owner['cases'][case]
        positions = [i for i, (_, row) in enumerate(rows) if row.split('|')[1].strip().removesuffix(' H').removesuffix(' †') == label]
        assert len(positions) == 1, label
        rows[positions[0]] = ('codec', f'| {label} † | {current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} | '
                             f'{current["managedPacketBytes"]:,} / {current["nativePacketBytes"]:,} |')
    lossless_status_counts = {state: sum((photo if name == 'photograph' else case)['status'] == state
                                         for name, case in webp_owned['cases'].items())
                              for state in ('pass', 'gap', 'uncertain')}
    assert lossless_status_counts == {'pass': 6, 'gap': 0, 'uncertain': 2}
    vp8_labels = {
        'opaque8': 'VP8 opaque RGBA8',
        'opaque16': 'VP8 opaque RGBA16, including normalization',
        'photograph': 'VP8 opaque photograph',
        'diagram': 'VP8 opaque text/diagram',
        'distortion': 'VP8 diagram, effort2 / distortion-only search',
        'trellis': 'VP8 diagram, effort6 / trellis',
        'alpha': 'VP8 RGBA8 + lossless alpha',
        'alpha16': 'VP8 RGBA16 + alpha, including normalization',
    }
    position = next(i for i, (_, row) in enumerate(rows) if row.startswith('| VP8L RGBA8 /'))
    vp8_confidence = json.loads((ROOT / 'benchmarks/vp8-encode-current-confidence-evidence.json').read_text(encoding='utf-8'))
    assert vp8_confidence['measurementAccepted'] and not vp8_confidence['productionChanged']
    assert not vp8_confidence['productionSpeedupClaim'] and vp8_confidence['allBuilds'] == vp8_confidence['broadOrPackageRepeats'] == 0
    assert vp8_confidence['raw']['mirrored'] and vp8_confidence['raw']['allMembersVerified']
    assert vp8_confidence['qualification']['sameActualPackageBothOS'] and vp8_confidence['qualification']['productionInputsUnchanged']
    assert set(vp8_confidence['cases']) == {'photograph', 'distortion'}
    for case, operations, packet in [('photograph', 8, 17688), ('distortion', 32, 7330)]:
        c = vp8_confidence['cases'][case]
        assert c['librarySha256'] == vp8_confidence['qualification']['actualPackage']['librarySha256']
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == len(c['managedActualNanoseconds']) == 12
        assert c['allActualsRetained'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['operationsPerSubject'] == operations and c['status'] == 'uncertain' and c['lower'] <= 1 < c['upper']
        assert c['completePacketExact'] and c['packetSha256'] == c['nativePacketSha256']
        assert c['managedPacketBytes'] == c['nativePacketBytes'] == packet
        assert c['sourceUnchanged'] and c['independentOwnedOutput']
    vp8_encode_cases = {**vp8_current['cases'], **vp8_confidence['cases']}
    vp8_rows = []
    for case, label in vp8_labels.items():
        c = webp_owned['cases'][case] if case in ('alpha', 'alpha16') else vp8_encode_cases[case]
        assert not any(row.split('|')[1].strip().removesuffix(' H').removesuffix(' †') == label for _, row in rows)
        vp8_rows.append(('codec', f'| {label} † | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | '
            f'{c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | '
            f'{c["managedPacketBytes"]:,} / {c["nativePacketBytes"]:,} |'))
    rows[position:position] = vp8_rows
    vp8_status_counts = {state: sum((webp_owned['cases'][case] if case in ('alpha', 'alpha16')
                                   else vp8_encode_cases[case])['status'] == state
                                  for case in vp8_encode_cases)
                         for state in ('pass', 'gap', 'uncertain')}
    assert vp8_status_counts == vp8_confidence['currentLossyStatusCounts'] == {'pass': 6, 'gap': 0, 'uncertain': 3}
    selected_cost = vp8_current['cases']['selected']
    distortion_cost = vp8_current['cases']['distortion']
    assert selected_cost['managedPacketBytes'] == 7026
    stream = lpf['Vp8Stream']
    rows.insert(0, ('codec', f'| WebP VP8 + alpha, fresh seekable Stream (parity confirmed) † | {stream["managedUs"]:,.2f} | '
        f'{stream["nativeUs"]:,.2f} | {stream["ratio"]:.3f} / {stream["upper"]:.3f} | '
        f'{stream["allocatedBytes"]:,} | — |'))
    bilinear = json.loads((ROOT / 'benchmarks/bilinear-shared-preparation-evidence.json').read_text(encoding='utf-8'))
    assert bilinear['productionAccepted'] and bilinear['allAffectedPerformanceReconciled']
    assert not bilinear['materialPassToGapContradictions']
    assert {'Down8', 'Down16', 'Up8'} <= set(bilinear['cases'])
    assert bilinear['raw']['mirrored'] and bilinear['raw']['allMembersVerified']
    bilinear_qualification = bilinear['qualification']
    assert bilinear_qualification['fullChecksEachOs'] == bilinear_qualification['windows']['total'] == bilinear_qualification['linux']['total'] == 6251
    assert bilinear_qualification['sameActualPackageBothOS'] and bilinear_qualification['threeNamedStorageSuitesIncluded']
    assert bilinear_qualification['actualPackage']['sha256'] == bilinear_qualification['samePackageLinux']['sameActualPackageSha256']
    for name in ['Up8']:
        cost = bilinear['cases'][name]
        assert cost['actualIterations'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['managedExact'] and cost['managedRationalReferenceExact'] and cost['independentOwnedOutput']
        assert cost['quietGuardsPassed'] and cost['allOriginalGatesPassed']
        assert cost['librarySha256'] == bilinear['producers']['windowsSha256'] == bilinear_qualification['actualPackage']['librarySha256']
    current = bilinear['cases']['Up8']
    assert current['upper'] <= 1 and current['status'] == 'pass'
    rows.append(('editing', f'| Bilinear upscale RGBA8 (parity confirmed) † → 1027×519 | {current["managedUs"]:,.2f} | '
                 f'{current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |'))
    bilinear_windows = json.loads((ROOT / 'benchmarks/bilinear-ordered-horizontal-windows-evidence.json').read_text(encoding='utf-8'))
    assert bilinear_windows['productionAccepted'] and bilinear_windows['allAffectedPerformanceReconciled']
    assert bilinear_windows['requiredMaterialPaths'] == 289 and not bilinear_windows['materialPassToGapContradictions']
    assert bilinear_windows['raw']['mirrored'] and bilinear_windows['raw']['allMembersVerified']
    assert bilinear_windows['statusCounts'] == {'pass': 1, 'gap': 0, 'uncertain': 1}
    assert bilinear_windows['sourceProof']['bothWholeDownsampleSourcesRestoredByRemovingOnlyPreparationAndEligibleWindowDispatch']
    assert bilinear_windows['sourceProof']['sharedWindowMathBodyVerbatim']
    for name, cost in bilinear_windows['cases'].items():
        assert cost['actualIterations'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['managedExact'] and cost['managedRationalReferenceExact'] and cost['independentOwnedOutput']
        assert cost['allOriginalGatesPassed'] and cost['quietGuardsPassed']
        assert cost['librarySha256'] == bilinear_windows['producers']['windowsSha256']
        assert cost['status'] == ('pass' if cost['upper'] <= 1 else 'gap' if cost['lower'] > 1 else 'uncertain')
        gain = bilinear_windows['comparison'][name]
        assert gain['normalizedGainSeparated'] and all(v['rawGainSeparated'] for v in gain['views'].values())
        depth = 8 if name == 'Down8' else 16
        qualifier = 'parity confirmed' if cost['status'] == 'pass' else 'confidence open'
        rows.append(('editing', f'| Bilinear resize RGBA{depth} ({qualifier}) † → 256×129 | {cost["managedUs"]:,.2f} | '
                     f'{cost["nativeUs"]:,.2f} | {cost["ratio"]:.3f} / {cost["upper"]:.3f} | {cost["allocatedBytes"]:,} |'))
    current = bicubic_overlap['cases']['TightDown8']
    positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing'
                 and row.startswith('| Bicubic RGBA8 reduction, input+output+4 KiB ')
                 and '→ 256×129 |' in row]
    assert len(positions) == 1
    rows[positions[0]] = ('editing', f'| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | '
                         f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                         f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |')
    current = bicubic_overlap['cases']['TightDown16']
    assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
    assert current['managedRationalReferenceExact'] and current['managedQualifiedOriginalPixelsExact']
    assert current['quietGuardsPassed'] and current['allOriginalGatesPassed']
    assert current['librarySha256'] == byte_publication_qualification['actualPackage']['librarySha256']
    rows.insert(positions[0] + 1, ('editing', f'| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | '
                                f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                                f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |'))
    current = png_adler['sharedRoot']
    assert current['actualIterations'] == len(current['managedActualNanoseconds']) == 12
    assert current['librarySha256'] == png_adler['producers']['windowsSha256']
    assert current['managedExact'] and current['independentOwnedOutputs'] and current['sourceUnchanged']
    assert current['allOriginalGatesPassed'] and current['quietGuardsPassed']
    assert current['status'] == 'pass' and current['upper'] <= 1
    # One current owner for small-support word publication. The earlier owners
    # above still supply unchanged byte/wide-word rows and linked history.
    word_prep = json.loads((ROOT / 'benchmarks/bicubic-shared-word-preparation-evidence.json').read_text(encoding='utf-8'))
    assert word_prep['productionAccepted'] and word_prep['raw']['mirrored'] and word_prep['raw']['allMembersVerified']
    assert len(word_prep['cases']) == 15
    assert word_prep['sourceProof']['outsideWordPreparationAndCachedWordWriterUnchanged']
    assert word_prep['sourceProof']['all2048ExponentConstructionsBitIdentical']
    assert word_prep['storage']['newOwnedCapacityBytes'] == word_prep['storage']['newCrossOperationRetainedBytes'] == 0
    for current in word_prep['cases'].values():
        if current['caseId'] in {'TightVertical16', 'TightHorizontal16', 'TightCross16'} or current['caseId'] in quartet['cases']:
            continue  # Refreshed by identity geometry below.
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['librarySha256'] == word_prep['producers']['linuxSha256']
        assert current['managedRationalReferenceExact'] and current['sourceUnchanged'] and current['independentOwnedOutput']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    bicubic_windows = json.loads((ROOT / 'benchmarks/bicubic-ordered-horizontal-windows-evidence.json').read_text(encoding='utf-8'))
    word_labels = {'TightCross16': 'Bicubic RGBA16, width down / height up / input+output+4 KiB',
                   'Cross16': 'Bicubic RGBA16, width down / height up, default workspace'}
    for case, label in word_labels.items():
        current = bicubic_windows['cases'].get(case, quartet['cases'].get(case, word_prep['cases'][case]))
        positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing' and row.startswith('| ' + label + ' ')]
        assert len(positions) == 1, label
        rows[positions[0]] = ('editing', f'| {label} † → {current["outputWidth"]}×{current["outputHeight"]} | '
            f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | '
            f'{current["allocatedBytes"]:,} |')
    assert bicubic_windows['productionAccepted'] and bicubic_windows['allAffectedPerformanceReconciled']
    assert bicubic_windows['requiredMaterialPaths'] == 289 and not bicubic_windows['materialPassToGapContradictions']
    assert bicubic_windows['raw']['mirrored'] and bicubic_windows['raw']['allMembersVerified']
    assert bicubic_windows['statusCounts'] == {'pass': 2, 'gap': 0, 'uncertain': 0}
    assert bicubic_windows['sourceProof']['wholeCrossWordSourceReconstructsByRemovingOnlyEligibilityAndPairedHorizontalDispatch']
    assert bicubic_windows['sourceProof']['wholeSharedMathReconstructsByRemovingOnlyIntEligibilityOverload']
    assert bicubic_windows['sourceProof']['existingOrderedFourEightSixteenTapFilterAndAllOtherMathBodiesVerbatim']
    for name, cost in bicubic_windows['cases'].items():
        assert cost['actualIterations'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['managedExact'] and cost['managedRationalReferenceExact'] and cost['independentOwnedOutput']
        assert cost['allOriginalGatesPassed'] and cost['quietGuardsPassed']
        assert cost['librarySha256'] == bicubic_windows['producers']['windowsSha256']
        assert cost['status'] == 'pass' and cost['upper'] <= 1
        gain = bicubic_windows['comparison'][name]
        assert gain['normalizedGainSeparated'] and all(v['rawGainSeparated'] for v in gain['views'].values())
    vq = word_prep['qualification']
    assert vq['fullChecksEachOs'] == vq['windows']['total'] == vq['linux']['total'] == 5866
    assert vq['actualPackage']['sha256'] == vq['samePackageLinux']['sameActualPackageSha256']
    assert vq['actualPackage']['actualConsumerPassed'] and vq['samePackageLinux']['actualConsumerPassed']
    assert result_bands['productionAccepted'] and result_bands['requiredMaterialPaths'] == 283
    assert result_bands['raw']['mirrored'] and result_bands['raw']['allMembersVerified']
    assert set(result_bands['cases']) == {'Mixed16', 'TightMixed16', 'TightVertical16'}
    assert result_bands['sourceProof']['sharedLoopTokensExactAfterRangeAndCheckpointSubstitution']
    assert result_bands['sourceProof']['originalWordTokensExact']
    for current in [result_bands['cases']['TightVertical16']]:
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['librarySha256'] == result_bands['producers']['linuxSha256']
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['managedRationalReferenceExact'] and current['sourceUnchanged'] and current['independentOwnedOutput']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    current = result_bands['cases']['TightVertical16']
    assert current['status'] == 'pass'
    rows.append(('editing', f'| Bicubic RGBA16 height reduction, input+output+4 KiB (pass) † → {current["outputWidth"]}×{current["outputHeight"]} | '
                 f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | '
                 f'{current["allocatedBytes"]:,} |'))
    bq = result_bands['qualification']
    assert bq['fullChecksEachOs'] == bq['windows']['total'] == bq['linux']['total'] == 5948
    assert bq['actualPackage']['sha256'] == bq['samePackageLinux']['sameActualPackageSha256']
    assert bq['actualPackage']['largeBandsExactAndAdmittedPassed'] and bq['samePackageLinux']['largeBandsExactAndAdmittedPassed']
    # One current owner for all affected complete PNG encodes. Superseded cost
    # providers and adjacent sampling studies remain linked from its report.
    png_current = json.loads((ROOT / 'benchmarks/png-huffman-heap-entries-evidence.json').read_text(encoding='utf-8'))
    assert png_current['productionAccepted'] and png_current['allAffectedPerformanceReconciled']
    assert png_current['requiredMaterialPaths'] == 289 and png_current['cpuGainClaimed'] and not png_current['generalCpuGainClaimed']
    assert png_current['raw']['mirrored'] and png_current['raw']['allMembersVerified']
    assert not png_current['compressionChanged'] and not png_current['managedPacketsChanged']
    assert png_current['storage']['newOwnedCapacityBytes'] == png_current['storage']['newRetainedCapacityBytes'] == 0
    assert not png_current['storage']['workspaceAdmissionChanged']
    assert png_current['storage']['logicalCompressedWorkspaceAdmissionBytes'] == 393216
    assert png_current['storage']['elementCapacitySavedBytes'] == 572
    assert png_current['storage']['measuredObjectAllocationSavedBytes'] == 600
    assert png_current['reusedPerformance']['pngReadCases'] == 51 and not png_current['issues']
    assert png_current['sourceProof']['storedCostsReusable'] and png_current['sourceProof']['originalCodesAndRunsSourceExact']
    assert png_current['sourceProof']['actualCommonBodies']['Lokad.Image.Internal.PngDeflater:.ctor'] == {'exact': True, 'codeBytes': 582}
    png_stored = json.loads((ROOT / png_current['reusedPerformance']['storedEvidence']).read_text(encoding='utf-8'))
    assert png_stored['productionAccepted'] and png_stored['raw']['mirrored'] and png_stored['raw']['allMembersVerified']
    assert png_stored['producers']['windowsSha256'] == png_current['reusedPerformance']['storedWindowsSha256']
    assert png_stored['producers']['actualPackageSha256'] == png_current['reusedPerformance']['storedActualPackageSha256']
    reused_stored_cases = set(png_current['reusedPerformance']['storedCases'])
    assert reused_stored_cases == {'EncodeNoCompression8', 'EncodeNoCompression16', 'EncodeNoCompression8Retained', 'EncodeNoCompression16Retained'}
    assert png_current['comparison']['views']['statistics']['rawGainSeparated']
    assert not png_current['comparison']['views']['unfilteredActualStatistics']['rawGainSeparated']
    assert not png_current['comparison']['normalizedGainSeparated'] and not png_current['comparison']['normalizedAdverseSeparated']
    assert not png_current['predictableAttribution']['normalizedAdverseSeparated']
    assert not any(v['rawAdverseSeparated'] for v in png_current['predictableAttribution']['views'].values())
    png_gray_current = json.loads((ROOT / 'benchmarks/png-gray-current-path-evidence.json').read_text(encoding='utf-8'))
    assert png_gray_current['measurementAccepted'] and not png_gray_current['productionChanged']
    assert not png_gray_current['productionSpeedupClaim'] and png_gray_current['allBuilds'] == png_gray_current['broadOrPackageRepeats'] == 0
    assert png_gray_current['raw']['mirrored'] and png_gray_current['raw']['allMembersVerified']
    assert png_gray_current['qualification']['sameActualPackageBothOS'] and png_gray_current['qualification']['productionInputsUnchanged']
    assert png_gray_current['qualification']['fullChecksEachOs'] == 6600
    assert set(png_gray_current['cases']) == {'EncodeGray8'}
    assert png_gray_current['cases']['EncodeGray8']['operationsPerSubject'] == 64
    png_cases = {**png_current['cases'], **png_gray_current['cases']}
    assert len(png_cases) == 20
    png_rgb16 = png_cases['EncodeRgb16']; png_gray8 = png_cases['EncodeGray8']
    assert png_rgb16['status'] == 'pass' and png_gray8['status'] == 'uncertain'
    png_oneshot_status = {status: sum(c['status'] == status and c['boundaryKind'] == 'equivalent-one-shot' for c in png_cases.values()) for status in ('pass', 'gap', 'uncertain')}
    assert png_oneshot_status == png_current['oneShotStatusCounts'] == {'pass': 16, 'gap': 0, 'uncertain': 1}
    def png_label(label):
        return (label.replace(' † H', '').replace(' †', '').replace(', one-shot ', ', ')
                .replace(', red-plane reference', '').replace('NoCompression0', 'stored0'))
    for case, current in png_cases.items():
        reused = case in reused_stored_cases
        assert current['costReused'] == reused
        producer = png_gray_current if case == 'EncodeGray8' else png_stored if reused else png_current
        assert current['librarySha256'] == producer['qualification']['actualPackage']['librarySha256']
        if reused:
            assert all(current[k] == png_stored['cases'][case][k] for k in ('managedUs', 'nativeUs', 'allocatedBytes', 'receipt', 'packetSha256'))
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == len(current['managedActualNanoseconds']) == 12
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed'] and current['completePacketExact']
        assert current['packetBytes'] == current['nativePacketBytes'] and current['packetSha256'] == current['nativePacketSha256']
        assert current['sourceUnchanged'] and current['independentOwnedOutput'] and current['retainedOperationScratchBytes'] == 0
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
        if current['boundaryKind'] != 'equivalent-one-shot':
            continue
        assert current['codecPreparationTimed'] and not current['referenceCodecRetained']
        label = current['label']; target = png_label(label)
        positions = [i for i, (kind, row) in enumerate(rows)
                     if kind == 'codec' and png_label(row.split('|')[1].strip()) == target]
        assert len(positions) <= 1, label
        row = ('codec', f'| {label} † | {current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
               f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} | '
               f'{current["packetBytes"]:,} / {current["nativePacketBytes"]:,} |')
        if positions:
            rows[positions[0]] = row
        else:
            position = max(i for i, (_, old) in enumerate(rows) if old.startswith('| PNG ')) + 1
            rows.insert(position, row)
    for index, (kind, row) in enumerate(rows):
        label = row.split('|')[1].strip().removesuffix(' †')
        if label == 'PNG8 decode, two retained images / shared caller root':
            current = png_adler['sharedRoot']
            rows[index] = (kind, f'| {label} † | {current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                           f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} | — |')
    overlap = json.loads((ROOT / 'benchmarks/bicubic-identity-cache-rows-evidence.json').read_text(encoding='utf-8'))
    assert overlap['productionAccepted'] and overlap['requiredMaterialPaths'] == 283
    assert overlap['raw']['mirrored'] and overlap['raw']['allMembersVerified']
    oq = overlap['qualification']
    assert oq['fullChecksEachOs'] == oq['windows']['total'] == oq['linux']['total'] == 5938
    assert oq['actualPackage']['sha256'] == oq['samePackageLinux']['sameActualPackageSha256']
    assert oq['actualPackage']['identityCacheRowsPassed'] and oq['samePackageLinux']['identityCacheRowsPassed']
    assert overlap['statusCounts'] == {state: sum(c['status'] == state for c in overlap['cases'].values())
                                      for state in ('pass', 'gap', 'uncertain')}
    for case in ('Vertical8', 'Vertical16'):
        assert all(v['rawGainSeparated'] for v in overlap['comparisons'][case]['views'].values())
        assert overlap['comparisons'][case]['normalizedGainSeparated']
        assert overlap['cases'][case]['status'] == 'pass'
    assert overlap['implementation']['originalConversionArithmeticVerbatim']
    assert overlap['storage']['identity513WorkspaceAfterBytes'] == 367436
    assert overlap['storage']['ownedDecodedCapacityRemovedBytes'] == 16416
    # The shared-wide provider above owns Down32x16_8; this older identity
    # overlay supplies only the unchanged narrow/identity paths it still owns.
    overlap_labels = {'Down8': 'Bicubic resize RGBA8',
                      'Cross8': 'Bicubic RGBA8, width down / height up, default workspace',
                      'Cross16': 'Bicubic RGBA16, width down / height up, default workspace'}
    for case, current in overlap['cases'].items():
        assert current['librarySha256'] == overlap['producers']['linuxSha256']
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['sourceUnchanged'] and current['independentOwnedOutput']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
        if case not in overlap_labels:
            continue
        current = bicubic_overlap['cases'].get(case, current)
        label = overlap_labels[case]
        positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing' and row.startswith('| ' + label + ' ')]
        assert len(positions) == 1, label
        rows[positions[0]] = ('editing', f'| {label} † → {current["outputWidth"]}×{current["outputHeight"]} | '
            f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | '
            f'{current["allocatedBytes"]:,} |')
    bounded_quartet = json.loads((ROOT / 'benchmarks/bounded-bicubic-shared-quartets-evidence.json').read_text(encoding='utf-8'))
    bounded_qualification = bounded_quartet['qualification']
    assert bounded_quartet['productionAccepted'] and bounded_quartet['requiredMaterialPaths'] == 289
    assert bounded_qualification['fullChecksEachOs'] == bounded_qualification['windows']['total'] == bounded_qualification['linux']['total'] == 6248
    assert bounded_qualification['sameActualPackageBothOS']
    assert bounded_qualification['actualPackage']['sha256'] == bounded_qualification['samePackageLinux']['sameActualPackageSha256']
    assert bounded_quartet['sourceProof']['allFiveProductionFilesReconstructExactly']
    assert bounded_quartet['sourceProof']['actualOrderedLaneComparisons'] == 40960
    assert bounded_quartet['allAffectedPerformanceReconciled'] and not bounded_quartet['materialPassToGapContradictions']
    assert all(c['normalizedGainSeparated'] and all(v['rawGainSeparated'] for v in c['views'].values())
               for c in bounded_quartet['comparison'].values())
    assert bounded_quartet['storage']['newOwnedCapacityBytes'] == bounded_quartet['storage']['retainedOperationScratchBytes'] == 0
    assert bounded_quartet['raw']['mirrored'] and bounded_quartet['raw']['allMembersVerified']
    for case, current in bounded_quartet['cases'].items():
        assert current['librarySha256'] == bounded_qualification['actualPackage']['librarySha256']
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['managedRationalReferenceExact'] and current['sourceUnchanged'] and current['independentOwnedOutput']
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed'] and current['allocatedBytes'] == 113
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
        label = 'Bicubic RGBA' + ('8' if case == 'TightDown8' else '16') + ' reduction, input+output+4 KiB'
        positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing'
                     and row.startswith('| ' + label + ' ') and '→ 256×129 |' in row]
        assert len(positions) == 1
        rows[positions[0]] = ('editing', f'| {label} † → 256×129 | '
                             f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |')
    wide_quartet = json.loads((ROOT / 'benchmarks/contiguous-wide-source-views-evidence.json').read_text(encoding='utf-8'))
    wide_quartet_qualification = wide_quartet['qualification']
    assert wide_quartet['productionAccepted'] and wide_quartet['requiredMaterialPaths'] == 289
    assert wide_quartet_qualification['fullChecksEachOs'] == wide_quartet_qualification['windows']['total'] == wide_quartet_qualification['linux']['total'] == 6439
    assert wide_quartet_qualification['sameActualPackageBothOS']
    assert wide_quartet_qualification['actualPackage']['sha256'] == wide_quartet_qualification['samePackageLinux']['sameActualPackageSha256']
    assert wide_quartet['comparison']['TightWide8']['normalizedGainSeparated']
    assert all(v['rawGainSeparated'] for v in wide_quartet['comparison']['TightWide8']['views'].values())
    assert wide_quartet['sourceProof']['entireOriginalFilesReconstructed']
    assert wide_quartet['sourceProof']['tests']['orderedBitsAndPoisonAndNoAllocation']
    assert wide_quartet['allAffectedPerformanceReconciled'] and not wide_quartet['materialPassToGapContradictions']
    assert wide_quartet['storage']['newOwnedCapacityBytes'] == wide_quartet['storage']['retainedOperationScratchBytes'] == 0
    assert wide_quartet['raw']['mirrored'] and wide_quartet['raw']['allMembersVerified']
    for case, current in wide_quartet['cases'].items():
        assert current['librarySha256'] == wide_quartet_qualification['actualPackage']['librarySha256']
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['managedRationalReferenceExact'] and current['sourceUnchanged'] and current['independentOwnedOutput']
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed'] and current['allocatedBytes'] == 113
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
        label = 'Bicubic RGBA' + ('8' if case == 'TightWide8' else '16') + ' reduction, input+output+4 KiB'
        positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing'
                     and row.startswith('| ' + label + ' ') and '→ 128×64 |' in row]
        assert len(positions) == 1
        status = 'gap' if current['status'] == 'gap' else 'confidence open' if current['status'] == 'uncertain' else 'pass'
        rows[positions[0]] = ('editing', f'| {label} ({status}) † → 128×64 | '
                             f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |')
    header = '| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |'
    separator = '| --- | ---: | ---: | ---: | ---: |'
    predictable = png_current['cases']['EncodePredictable16']
    positions = [i for i, (kind, row) in enumerate(rows) if kind == 'codec' and row.startswith('| PNG predictable RGBA16 3000²')]
    assert len(positions) == 1 and predictable['status'] == 'pass'
    rows[positions[0]] = ('codec', f'| {predictable["label"]} † | {predictable["managedUs"]:,.2f} | {predictable["nativeUs"]:,.2f} | '
                         f'{predictable["ratio"]:.3f} / {predictable["upper"]:.3f} | {predictable["allocatedBytes"]:,} | '
                         f'{predictable["packetBytes"]:,} / {predictable["nativePacketBytes"]:,} |')
    deep_current = json.loads((ROOT / 'benchmarks/prepared-deep-pair-conversions-evidence.json').read_text(encoding='utf-8'))
    assert deep_current['productionAccepted'] and deep_current['productionChanged'] and deep_current['requiredMaterialPaths'] == 289
    assert deep_current['raw']['mirrored'] and deep_current['raw']['allMembersVerified']
    assert deep_current['qualification']['fullChecksEachOs'] == 6435 and deep_current['qualification']['sameActualPackageBothOS']
    assert deep_current['comparison']['normalizedGainSeparated'] and all(v['rawGainSeparated'] for v in deep_current['comparison']['views'].values())
    deep = deep_current['case']
    assert deep['librarySha256'] == deep_current['producers']['timedArtifactSha256'] == deep_current['qualification']['actualPackage']['librarySha256']
    assert deep['actualIterations'] == deep['nativeSamplesEachBracket'] == 12 and deep['allActualsRetained']
    assert deep['allOriginalGatesPassed'] and deep['quietGuardsPassed'] and deep['managedExact']
    assert deep['sourceUnchanged'] and deep['independentOwnedOutput'] and deep['retainedOperationScratchBytes'] == 0
    assert deep['status'] == 'pass' and deep['upper'] <= 1 and deep['allocatedBytes'] == 113
    label = 'Bicubic RGBA8, 32×16 / input+output+4 KiB'
    positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing' and row.startswith('| ' + label + ' H ')]
    assert len(positions) == 1
    rows[positions[0]] = ('editing', f'| {label} (parity confirmed) † → 32×16 | {deep["managedUs"]:,.2f} | {deep["nativeUs"]:,.2f} | '
                         f'{deep["ratio"]:.3f} / {deep["upper"]:.3f} | {deep["allocatedBytes"]:,} |')
    assert not any(re.search(r'\sH(?:\s|\|)', row) for _, row in rows), 'Historical marked rows remain.'
    codecs = '\n'.join([header + ' Packet B, Lokad / reference |', separator + ' ---: |',
                         *(row for kind, row in rows if kind == 'codec')])
    read_labels = ['Probe PNG dimensions, seekable', 'Probe JPEG dimensions, seekable',
                   'Probe WebP dimensions, seekable', 'Probe JPEG dimensions, async nonseekable',
                   'Validate PNG8, span', 'Validate PNG16, span', 'Validate indexed Adam7 19×17, span',
                   'Validate JPEG baseline420, span', 'Validate JPEG progressive420, span',
                   'Validate VP8L+alpha, span', 'Validate VP8+alpha, span',
                   'Validate PNG16, async nonseekable']
    # One current owner for the shared cached family. Superseded preparation,
    # publication and boundary studies retain history outside renderer inputs.
    cached_horizontal = json.loads((ROOT / 'benchmarks/cached-bicubic-shared-horizontal-schedule-evidence.json').read_text(encoding='utf-8'))
    assert cached_horizontal['productionAccepted'] and cached_horizontal['requiredMaterialPaths'] == 289
    assert cached_horizontal['allAffectedPerformanceReconciled'] and not cached_horizontal['materialPassToGapContradictions']
    assert cached_horizontal['raw']['mirrored'] and cached_horizontal['raw']['allMembersVerified']
    assert len(cached_horizontal['cases']) == 24
    assert cached_horizontal['statusCounts'] == {state: sum(c['status'] == state for c in cached_horizontal['cases'].values())
                                               for state in ('pass', 'gap', 'uncertain')}
    assert cached_horizontal['statusCounts'] == {'pass': 22, 'gap': 0, 'uncertain': 2}
    assert cached_horizontal['storage']['newOwnedCapacityBytes'] == cached_horizontal['storage']['newRetainedCapacityBytes'] == 0
    assert cached_horizontal['sourceProof']['wholeFactoryUnchangedExceptSharedPreparedQuartetCalls']
    assert cached_horizontal['sourceProof']['allOriginalPairAdjacentIndexedOddAndWidePublicationBodiesVerbatim']
    effect = cached_horizontal['comparison']['Down8']
    assert effect['normalizedGainSeparated'] and all(v['rawGainSeparated'] for v in effect['views'].values())
    for cost in cached_horizontal['cases'].values():
        assert cost['librarySha256'] == cached_horizontal['producers']['timedArtifactSha256']
        assert cost['actualIterations'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['managedExact'] and cost['allOriginalGatesPassed'] and cost['quietGuardsPassed']
        assert cost['status'] == ('pass' if cost['upper'] <= 1 else 'gap' if cost['lower'] > 1 else 'uncertain')
        assert math.isclose(cost['ratio'], cost['managedUs'] / cost['nativeUs'], rel_tol=1e-12)
    labels = {
        'Down8': 'Bicubic resize RGBA8',
        'Down32x16_8': 'Bicubic RGBA8, 32×16 / default workspace',
        'Wide8': 'Bicubic RGBA8 wider-support reduction',
        'Cross8': 'Bicubic RGBA8, width down / height up, default workspace',
        'Cross16': 'Bicubic RGBA16, width down / height up, default workspace',
    }
    for case, label in labels.items():
        current = cached_horizontal['cases'][case]
        positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing' and row.startswith('| ' + label + ' ')]
        assert len(positions) == 1, label
        if case == 'Wide8':
            label = 'Bicubic RGBA8, fresh managed / prepared zimg'
        rows[positions[0]] = ('editing', f'| {label} † → {current["outputWidth"]}×{current["outputHeight"]} | '
            f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / '
            f'{current["upper"]:.3f} | {current["allocatedBytes"]:,} |')
    for family, size, retained in [('wide', '128×64', 198432), ('narrow', '256×129', 199840)]:
        for case in ['OneShot8', 'Repeated8', 'OneShot16', 'Repeated16']:
            current = cached_horizontal['cases'][family + '/' + case]
            assert current['status'] == 'pass'
            prepared = case.startswith('Repeated')
            assert current['retainedBytes'] == (retained if prepared else 0)
            qualifier = ' narrow' if family == 'narrow' else ''
            label = f'Bicubic {"prepared" if prepared else "one-shot"}{qualifier} RGBA{current["depth"]} / zimg {"prepared" if prepared else "one-shot"}'
            rows.append(('editing', f'| {label} † → {size} | {current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                         f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |'))
    compact_rows = [row for kind, row in rows if kind == 'editing' and row.startswith(compact_geometry_prefixes)]
    assert len(compact_rows) == 2 and all(' H ' not in row for row in compact_rows)
    resize_weights = json.loads((ROOT / 'benchmarks/shared-resize-filter-weights-evidence.json').read_text(encoding='utf-8'))
    assert resize_weights['productionAccepted'] and resize_weights['allAffectedPerformanceReconciled']
    assert resize_weights['requiredMaterialPaths'] == 289 and resize_weights['newMaterialPaths'] == 0
    assert [len(resize_weights[k]) for k in ('cases', 'prepared', 'bilinear')] == [61, 8, 4]
    assert resize_weights['raw']['mirrored'] and resize_weights['raw']['allMembersVerified']
    assert not resize_weights['materialPassToGapContradictions']
    assert resize_weights['storage']['newOwnedCapacityBytes'] == resize_weights['storage']['newRetainedCapacityBytes'] == 0
    cross_prefix = json.loads((ROOT / 'benchmarks/cross-bicubic-output-prefix-evidence.json').read_text(encoding='utf-8'))
    assert cross_prefix['productionAccepted'] and cross_prefix['allAffectedPerformanceReconciled']
    assert set(cross_prefix['cases']) == {'TightCross8', 'TightHorizontal8', 'Up8', 'TightUp8', 'HorizontalUp8', 'TightHorizontalUp8', 'VerticalUp8', 'TightVerticalUp8', 'Up16', 'TightUp16', 'HorizontalUp16', 'TightHorizontalUp16', 'VerticalUp16', 'TightVerticalUp16'}
    assert cross_prefix['raw']['mirrored'] and cross_prefix['raw']['allMembersVerified']
    assert cross_prefix['sourceProof']['originalCrossStripBodyExactAfterOnlyPrepareTailAndStartRowRemap']
    assert cross_prefix['sourceProof']['actualTimedRootJit']['normalizedRootInstructionsAndBlockOffsetsExact']
    assert cross_prefix['reuse']['allOtherProductionSourcesUnchanged']
    assert cross_prefix['reuse']['newCodeAfterUnchangedIsaRejectionGuard']
    assert cross_prefix['storage']['newOwnedCapacityBytes'] == cross_prefix['storage']['newRetainedCapacityBytes'] == 0
    assert all(c['scopedIncrementOnly'] and not c['newNativeCostOrParityProof'] and c['pairs'] == 12
               for c in cross_prefix['paired'].values())
    assert all(cross_prefix['cases'][n]['status'] == 'pass' and cross_prefix['comparison'][n]['normalizedGainSeparated']
               for n in ('TightCross8', 'TightHorizontal8'))
    assert all(not c['normalizedAdverseSeparated'] for c in cross_prefix['comparison'].values())
    generated_rows = json.loads((ROOT / 'benchmarks/generated-bicubic-shared-execution-evidence.json').read_text(encoding='utf-8'))
    assert generated_rows['productionAccepted'] and generated_rows['allAffectedPerformanceReconciled']
    assert set(generated_rows['cases']) == {'Tight2x9_16', 'Tight2x2_16', 'Tight2x9_8', 'Tight2x2_8', 'Down1x1_8'}
    assert generated_rows['raw']['mirrored'] and generated_rows['raw']['allMembersVerified']
    assert not generated_rows['materialPassToGapContradictions']
    assert generated_rows['storage']['explicitStackArrayBytes'] == 1824
    assert generated_rows['storage']['workspaceAdmissionBytes'] == 4096
    assert generated_rows['storage']['newRetainedCapacityBytes'] == 0
    assert all(c['status'] == 'pass' and c['managedExact'] for c in generated_rows['cases'].values())
    generated_primary = generated_rows['comparison']['Tight2x9_16']
    assert generated_primary['normalizedGainSeparated'] and all(v['rawGainSeparated'] for v in generated_primary['views'].values())
    assert generated_rows['sourceProof']['rootActualTimedModuleJit']['normalizedRootInstructionsAndBlockOffsetsExact']
    assert generated_rows['sourceProof']['duplicatedWordScheduleRemoved']
    streaming_weights = json.loads((ROOT / 'benchmarks/streaming-byte-vector-weights-evidence.json').read_text(encoding='utf-8'))
    assert streaming_weights['productionAccepted'] and streaming_weights['allAffectedPerformanceReconciled']
    assert set(streaming_weights['cases']) == {'Tight16x8_8', 'Tight8x17_8', 'Tight8x4_8'}
    assert streaming_weights['sourceProof']['callerAndWordPathsUnchanged']
    assert streaming_weights['raw']['mirrored'] and streaming_weights['raw']['allMembersVerified']
    for case in streaming_weights['cases'].values():
        assert case['status'] == 'pass' and case['upper'] <= 1 and case['managedExact']
        assert case['actualIterations'] == case['nativeSamplesEachBracket'] == 12 and case['allActualsRetained']
        assert case['allocatedBytes'] == 113
        assert case['librarySha256'] == streaming_weights['qualification']['actualPackage']['librarySha256']
    sw_primary = streaming_weights['cases']['Tight16x8_8']
    sw_seventeen = streaming_weights['cases']['Tight8x17_8']
    sw_four = streaming_weights['cases']['Tight8x4_8']
    bilinear_metadata = json.loads((ROOT / 'benchmarks/bilinear-operation-metadata-evidence.json').read_text(encoding='utf-8'))
    assert bilinear_metadata['productionAccepted'] and bilinear_metadata['allAffectedPerformanceReconciled']
    assert set(bilinear_metadata['cases']) == {'Down8', 'Down16'}
    assert bilinear_metadata['raw']['mirrored'] and bilinear_metadata['raw']['allMembersVerified']
    assert bilinear_metadata['sourceProof']['other71ResizeCostsReused']
    assert bilinear_metadata['comparison']['Down16']['normalizedGainSeparated']
    scaler_confidence = json.loads((ROOT / 'benchmarks/resize-current-adjacent-confidence-evidence.json').read_text(encoding='utf-8'))
    assert scaler_confidence['measurementAccepted'] and not scaler_confidence['productionChanged']
    assert scaler_confidence['allBuilds'] == scaler_confidence['broadOrPackageRepeats'] == 0
    assert set(scaler_confidence['cases']) == {'TightDown8', 'Wide8'}
    assert scaler_confidence['producers'] == bilinear_metadata['producers']
    assert scaler_confidence['raw']['mirrored'] and scaler_confidence['raw']['allMembersVerified']
    assert all(c['allOriginalGatesPassed'] and c['quietGuardsPassed'] and c['allActualsRetained']
               and c['managedRationalReferenceExact'] and c['independentOwnedOutput']
               for c in scaler_confidence['cases'].values())
    mixed_confidence = json.loads((ROOT / 'benchmarks/resize-mixed-word-confidence-evidence.json').read_text(encoding='utf-8'))
    assert mixed_confidence['measurementAccepted'] and not mixed_confidence['productionChanged']
    assert mixed_confidence['allBuilds'] == mixed_confidence['broadOrPackageRepeats'] == 0
    assert mixed_confidence['producers']['windowsSha256'] == mixed_confidence['qualification']['actualPackage']['librarySha256']
    assert mixed_confidence['raw']['mirrored'] and mixed_confidence['raw']['allMembersVerified']
    assert set(mixed_confidence['cases']) == {'TightMixed16'}
    mixed_word_adjacent = mixed_confidence['cases']['TightMixed16']
    assert mixed_word_adjacent['status'] == 'uncertain' and mixed_word_adjacent['lower'] <= 1 < mixed_word_adjacent['upper']
    assert mixed_word_adjacent['allOriginalGatesPassed'] and mixed_word_adjacent['quietGuardsPassed']
    assert mixed_word_adjacent['allActualsRetained'] and mixed_word_adjacent['managedRationalReferenceExact']
    assert mixed_word_adjacent['independentOwnedOutput']
    compact_paired = json.loads((ROOT / 'benchmarks/compact-bicubic-paired-weights-evidence.json').read_text(encoding='utf-8'))
    assert compact_paired['productionAccepted'] and compact_paired['allAffectedPerformanceReconciled']
    assert set(compact_paired['cases']) == {'TightDown8'}
    assert compact_paired['raw']['mirrored'] and compact_paired['raw']['allMembersVerified']
    assert compact_paired['sourceProof']['performanceScope']['other72ResizeCostsReused']
    assert compact_paired['sourceProof']['actualSharedCaller']['exact']
    assert all(v['rawGainSeparated'] for v in compact_paired['comparison']['views'].values())
    assert not compact_paired['comparison']['normalizedAdverseSeparated']
    word_views = json.loads((ROOT / 'benchmarks/deep-word-horizontal-views-evidence.json').read_text(encoding='utf-8'))
    assert word_views['productionAccepted'] and word_views['allAffectedPerformanceReconciled']
    assert word_views['baselineWindowsSha256'] == compact_paired['producers']['windowsSha256']
    assert word_views['raw']['mirrored'] and word_views['raw']['allMembersVerified']
    assert word_views['sourceProof']['wholeOriginalSourceReconstructed']
    assert word_views['sourceProof']['actualSharedCallers']['Lokad.Image.ImageTransforms:Resize']['exact']
    assert word_views['sourceProof']['performanceScope']['other72ResizeCostsReused']
    assert word_views['comparison']['normalizedGainSeparated']
    assert all(v['rawGainSeparated'] for v in word_views['comparison']['views'].values())
    assert set(word_views['cases']) == {'Tight32x16_16'}
    deep_cost = word_views['cases']['Tight32x16_16']
    assert deep_cost['status'] == 'pass' and deep_cost['upper'] <= 1
    assert deep_cost['managedRationalReferenceExact'] and deep_cost['managedQualifiedOriginalPixelsExact']
    assert deep_cost['allOriginalGatesPassed'] and deep_cost['quietGuardsPassed']
    assert deep_cost['allActualsRetained'] and deep_cost['independentOwnedOutput']
    horizontal_confidence = json.loads((ROOT / 'benchmarks/resize-horizontal-confidence-evidence.json').read_text(encoding='utf-8'))
    assert horizontal_confidence['measurementAccepted'] and not horizontal_confidence['productionChanged']
    assert not horizontal_confidence['productionSpeedupClaim']
    assert horizontal_confidence['allBuilds'] == horizontal_confidence['broadOrPackageRepeats'] == 0
    assert horizontal_confidence['producers']['windowsSha256'] == horizontal_confidence['qualification']['actualPackage']['librarySha256']
    assert horizontal_confidence['qualification']['fullChecksEachOs'] == 6517
    assert horizontal_confidence['raw']['mirrored'] and horizontal_confidence['raw']['allMembersVerified']
    assert set(horizontal_confidence['cases']) == {'HorizontalUp8'}
    horizontal_cost = horizontal_confidence['cases']['HorizontalUp8']
    assert horizontal_cost['status'] == 'pass' and horizontal_cost['upper'] <= 1
    assert horizontal_cost['actualIterations'] == 12 and horizontal_cost['operationsPerSubject'] == 64
    assert horizontal_cost['allActualsRetained'] and horizontal_cost['allOriginalGatesPassed'] and horizontal_cost['quietGuardsPassed']
    assert horizontal_cost['managedRationalReferenceExact'] and horizontal_cost['managedQualifiedOriginalPixelsExact']
    assert horizontal_cost['sourceUnchanged'] and horizontal_cost['independentOwnedOutput']
    current_bilinear_cases = {**resize_weights['bilinear'], **bilinear_metadata['cases']}
    current_resize_cases = {**resize_weights['cases'], **cross_prefix['cases'], **generated_rows['cases'], **streaming_weights['cases'], **scaler_confidence['cases'], **compact_paired['cases'], **word_views['cases'], **horizontal_confidence['cases']}
    resize_status_counts = {s: sum(c['status'] == s for c in [*current_resize_cases.values(), *resize_weights['prepared'].values(), *current_bilinear_cases.values()]) for s in ('pass', 'gap', 'uncertain')}
    assert sum(resize_status_counts.values()) == 73
    assert resize_status_counts == horizontal_confidence['currentResizeStatusCounts'] == {'pass': 66, 'gap': 0, 'uncertain': 7}
    resize_case_owners = {case: (owner, view)
                          for owner, view in ((cross_prefix, 'cross'), (generated_rows, 'generated'), (scaler_confidence, 'confidence'), (compact_paired, 'compact'), (word_views, 'deep'))
                          for case in owner['cases']}
    rows = apply_current_resize_weights(rows, resize_weights, resize_case_owners, bilinear_metadata)
    editing = '\n'.join([header, separator, *(row for kind, row in rows if kind == 'editing')])
    read_rows = []
    refreshed_png_reads = set()
    for label, (case, observations) in zip(read_labels, reads['cases'].items(), strict=True):
        if case in png_adler['readCases']:
            current = png_adler['readCases'][case]
            assert current['descriptorExact'] and current['prefixAndCodingScopesVerified'] and current['outputBytes'] == 36
            status = 'Confirmed pass' if current['status'] == 'pass' else 'Confirmed gap' if current['status'] == 'gap' else 'Confidence open'
            read_rows.append(f'| {label} † | {current["managedUs"]:,.3f} | {current["nativeUs"]:,.3f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} | {status} |')
            refreshed_png_reads.add(case)
            continue
        if case in {'ValidateJpegBaseline420', 'ValidateJpegProgressive420'}:
            current = jpeg_read['cases'][case]
            assert current['descriptorSha256'] and current['quietGuardsPassed'] and current['allOriginalGatesPassed']
            assert current['managedActuals'] == current['nativeSamplesEachBracket'] == 12
            status = 'Confirmed pass' if current['status'] == 'confirmed-pass-stop' else 'Confirmed gap' if current['status'] == 'confirmed-gap' else 'Confidence open'
            read_rows.append(f'| {label} † | {current["managedMicroseconds"]:,.3f} | {current["nativeMicroseconds"]:,.3f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} | {status} |')
            continue
        if case in {'ValidateVp8Alpha', 'ValidateVp8lAlpha'}:
            current = lpf[case]
            assert current['descriptorExact'] and current['prefixAndCodingScopesVerified'] and current['managedOutputBytes'] == 36
            status = 'Confirmed pass' if current['status'] == 'pass' else 'Confirmed gap' if current['status'] == 'gap' else 'Confidence open'
            read_rows.append(f'| {label} † | {current["managedUs"]:,.3f} | {current["nativeUs"]:,.3f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} | {status} |')
            continue
        producer = (png_probe if case in png_probe['cases'] else header_skips if case in header_skips['cases'] else png_gfni if case in png_gfni['cases'] else baseline_blocks if case == 'ValidateJpegBaseline420' else png_average if case in png_average['cases'] else png_crc_scan if case in png_crc_scan['cases'] else vp8_validation if case in vp8_validation['cases'] else png_cursor if case in png_cursor['cases']
                    else webp_probe if case in webp_probe['cases'] else jpeg_probe if case in jpeg_probe['cases'] else probes if case in probes['cases'] else streamed if case in streamed['cases'] else reads)
        observations = producer['cases'][case]
        observation = observations[-1]
        receipt = observation['receipt']
        assert receipt['librarySha256'] == producer['producers']['linuxSha256']
        assert receipt['descriptorExact'] and receipt['prefixAndCodingScopesVerified']
        assert all(q['passed'] for q in observation['quietness'].values())
        confirmed = producer['status'][case].startswith('confirmed-')
        samples = 12 if confirmed else 3
        assert len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == samples
        assert len(observation['managed']['actual']) == samples
        native = min(b['mean'] for b in observation['nativeBrackets'])
        stats = observation['managed']['statistics']
        ratio, upper = observation['worstBracketRatio'], observation['conservativeRatioUpper']
        assert math.isclose(stats['Mean'] / native, ratio, rel_tol=1e-12)
        assert (upper <= 1) == (producer['status'][case] == 'confirmed-pass')
        status = 'Confidence open' if producer['status'][case] == 'confirmed-confidence-open' else 'Confirmed pass' if upper <= 1 else 'Confirmed gap' if confirmed else 'Screen gap'
        read_rows.append(f'| {label} | {stats["Mean"] / 1000:,.3f} | {native / 1000:,.3f} | '
                         f'{ratio:.3f} / {upper:.3f} | {observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | {status} |')
    headers = header_skips['cases']['PngHeaders'][-1]
    assert headers['receipt']['descriptorExact'] and headers['receipt']['typedSourceMetadataExact']
    assert headers['receipt']['librarySha256'] == header_skips['producers']['linuxSha256']
    assert len(headers['managed']['actual']) == 12 and all(q['passed'] for q in headers['quietness'].values())
    read_rows.append(f'| Inspect PNG8 complete headers, seekable, adapter reference | '
                     f'{headers["managed"]["statistics"]["Mean"] / 1000:,.3f} | '
                     f'{min(b["mean"] for b in headers["nativeBrackets"]) / 1000:,.3f} | '
                     f'{headers["worstBracketRatio"]:.3f} / {headers["conservativeRatioUpper"]:.3f} | '
                     f'{headers["managedMemory"]["BytesAllocatedPerOperation"]:,} | Confirmed gap |')
    identified = png_gfni['cases']['PngResident'][-1]
    assert identified['receipt']['descriptorExact'] and identified['receipt']['typedSourceMetadataExact']
    assert identified['receipt']['descriptorFields'] == 9 and identified['receipt']['nativeReady']['outputBytes'] == 36
    assert identified['receipt']['librarySha256'] == png_gfni['producers']['linuxSha256']
    assert len(identified['managed']['actual']) == 12 and all(q['passed'] for q in identified['quietness'].values())
    read_rows.append(f'| Identify PNG8 complete container, resident, adapter reference | '
                     f'{identified["managed"]["statistics"]["Mean"] / 1000:,.3f} | '
                     f'{min(b["mean"] for b in identified["nativeBrackets"]) / 1000:,.3f} | '
                     f'{identified["worstBracketRatio"]:.3f} / {identified["conservativeRatioUpper"]:.3f} | '
                     f'{identified["managedMemory"]["BytesAllocatedPerOperation"]:,} | Confirmed gap |')
    for case, label in [('Jpeg8', 'Identify JPEG420, resident, canonical adapter'),
                        ('Webp8', 'Identify VP8L+alpha, resident, parser adapter')]:
        identified = resident_other['cases'][case][-1]
        assert resident_other['status'][case] == 'confirmed-pass' and identified['conservativeRatioUpper'] <= 1
        assert identified['receipt']['typedSourceMetadataExact'] and identified['receipt']['descriptorFields'] == 9
        assert identified['receipt']['librarySha256'] == resident_other['producers']['linuxSha256']
        assert len(identified['managed']['actual']) == 12 and all(q['passed'] for q in identified['quietness'].values())
        read_rows.append(f'| {label} | {identified["managed"]["statistics"]["Mean"] / 1000:,.3f} | '
                         f'{min(b["mean"] for b in identified["nativeBrackets"]) / 1000:,.3f} | '
                         f'{identified["worstBracketRatio"]:.3f} / {identified["conservativeRatioUpper"]:.3f} | '
                         f'{identified["managedMemory"]["BytesAllocatedPerOperation"]:,} | Confirmed pass |')
    for case, label, samples in [('PngSeekable', 'Identify PNG8, fresh seekable Stream, adapter reference', 12),
                                  ('PngAsync', 'Identify PNG8, async nonseekable, adapter reference', 12)]:
        identified = png_gfni['cases'][case][-1]
        assert identified['receipt']['typedSourceMetadataExact'] and identified['receipt']['descriptorFields'] == 9
        assert identified['receipt']['librarySha256'] == png_gfni['producers']['linuxSha256']
        assert len(identified['managed']['actual']) == samples and all(q['passed'] for q in identified['quietness'].values())
        status = 'Confirmed gap' if samples == 12 else 'Screen gap'
        read_rows.append(f'| {label} | {identified["managed"]["statistics"]["Mean"] / 1000:,.3f} | '
                         f'{min(b["mean"] for b in identified["nativeBrackets"]) / 1000:,.3f} | '
                         f'{identified["worstBracketRatio"]:.3f} / {identified["conservativeRatioUpper"]:.3f} | '
                         f'{identified["managedMemory"]["BytesAllocatedPerOperation"]:,} | {status} |')
    assert refreshed_png_reads == {'ValidatePng8', 'ValidatePng16', 'ValidateIndexedAdam7', 'ValidatePng16Async'}
    current = jpeg_read['freshContextCase']
    assert current['nativeReady']['freshDecoderFrameAndPackingContextTimed']
    assert current['nativeReady']['residentPaddedPacketBorrowed']
    assert current['status'] == 'pass' and current['upper'] <= 1
    assert current['librarySha256'] == jpeg_qualification['actualPackage']['librarySha256']
    assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
    assert current['managedExact'] and current['allOriginalGatesPassed'] and current['quietGuardsPassed']
    assert current['managedOutputSha256'] == current['nativeOutputSha256']
    assert current['normalAndScalarNativeMaximum'] == [0, 0] and current['alphaExact']
    assert current['allocatedBytes'] == 20944 and current['outputBytes'] == 531468
    assert math.isclose(current['ratio'], current['managedUs'] / current['nativeUs'], rel_tol=1e-12)
    read_rows.append(f'| JPEG Adobe CMYK horizontal AC / FFmpeg fresh contexts † | {current["managedUs"]:,.3f} | '
                     f'{current["nativeUs"]:,.3f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | '
                     f'{current["allocatedBytes"]:,} | Confirmed pass |')
    text_pipeline = json.loads((ROOT / 'benchmarks/cleared-canvas-storage-evidence.json').read_text(encoding='utf-8'))
    text_pipeline_qualification = text_pipeline['qualification']
    assert text_pipeline['productionChanged'] and text_pipeline['requiredMaterialPaths'] == 289
    assert text_pipeline['raw']['mirrored'] and text_pipeline['raw']['allMembersVerified']
    assert text_pipeline_qualification['fullChecksEachOs'] == 6275
    assert text_pipeline_qualification['actualPackage']['sha256'] == text_pipeline_qualification['samePackageLinux']['sameActualPackageSha256']
    assert text_pipeline_qualification['actualPackage']['librarySha256'] == text_pipeline['producers']['librarySha256']
    assert text_pipeline['sourceProof']['originalFieldsConstructorDisposeInternalAllocateOverwriteCloneRowsSamplesVerbatim']
    assert text_pipeline['sourceProof']['oldPoolRentReturnFontPreservationAndLatestReturnVerbatim']
    assert set(text_pipeline['cases']) == {'OneShot', 'PrepareDraw8', 'Preparation', 'PreparedDraw'}
    for case, cost in text_pipeline['cases'].items():
        assert cost['pairs'] == cost['referenceBracketSamples'] == 12 and cost['operationsPerSample'] == 32
        assert cost['quietGuardsPassed'] and cost['allFrozenChecksPassed'] and cost['allActualsRetained']
        assert math.isclose(cost['ratio'], cost['managedUs'] / cost['referenceUs'], rel_tol=1e-12)
        assert cost['status'] == ('uncertain' if cost['upper'] is None else 'pass' if cost['upper'] <= 1 else 'gap' if cost['lower'] > 1 else 'uncertain')
        assert cost['completeWorkload'] == (case in {'OneShot', 'PrepareDraw8'})
    tp_one = text_pipeline['cases']['OneShot']
    tp_batch = text_pipeline['cases']['PrepareDraw8']
    tp_draw = text_pipeline['cases']['PreparedDraw']
    assert tp_one['status'] == tp_batch['status'] == 'pass' and tp_draw['status'] == 'uncertain'
    assert text_pipeline['effects']['PrepareDraw8']['rawGainSeparated'] and text_pipeline['effects']['PrepareDraw8']['normalizedGainSeparated']
    assert math.isclose(tp_batch['amortizedManagedUs'], tp_batch['managedUs'] / 8, rel_tol=1e-12)
    png_live_qualification = png_live['qualification']
    assert png_live_qualification['fullChecksEachOs'] == png_live_qualification['windows']['total'] == png_live_qualification['linux']['total'] == 6351
    assert png_live_qualification['actualPackage']['sha256'] == png_live_qualification['samePackageLinux']['sameActualPackageSha256'] == png_live['producers']['actualPackageSha256']
    assert png_live_qualification['actualPackage']['librarySha256'] == png_live['producers']['windowsSha256']
    assert png_live_qualification['actualPackage']['actualConsumerPassed'] and png_live_qualification['samePackageLinux']['actualConsumerPassed']
    assert png_live_qualification['actualPackageFourFrozenEnlargementFramesPassedBothOs']
    assert png_live_qualification['threeNamedStorageSuitesIncluded']
    assert png_live_qualification['pngInternalEachModeEachOs'] == 712 and png_live_qualification['pngPublicEachModeEachOs'] == 246
    assert png_live_qualification['unchangedResizeCodecTextIsaEvidenceReused'] and png_live_qualification['additionalResizeIsaRuns'] == 0
    direct_inputs = json.loads((ROOT / 'benchmarks/direct-stream-input-evidence.json').read_text(encoding='utf-8'))
    assert direct_inputs['productionAccepted'] and direct_inputs['allAffectedPerformanceReconciled']
    assert direct_inputs['requiredMaterialPaths'] == 289 and direct_inputs['statusCounts'] == {'pass': 9, 'gap': 2, 'uncertain': 0}
    assert len(direct_inputs['cases']) == 11 and direct_inputs['raw']['mirrored'] and direct_inputs['raw']['allMembersVerified']
    direct_qualification = direct_inputs['qualification']
    assert direct_qualification['windows']['total'] == direct_qualification['linux']['total'] == 6530
    assert direct_qualification['actualPackage']['sha256'] == direct_qualification['samePackageLinux']['sameActualPackageSha256']
    assert direct_qualification['actualPackage']['directStreamInputCapacityAndRefusalRecoveryPassed']
    assert direct_qualification['samePackageLinux']['directStreamInputCapacityAndRefusalRecoveryPassed']
    for case in direct_inputs['cases'].values():
        assert case['actualIterations'] == case['nativeSamplesEachBracket'] == 12
        assert case['librarySha256'] == direct_inputs['producers']['timedWindowsSha256'] == direct_qualification['actualPackage']['librarySha256']
        assert case['allOriginalGatesPassed'] and case['quietGuardsPassed'] and case['sourceUnchanged'] and case['independentOwnedOutput']
        assert case['status'] == ('pass' if case['upper'] <= 1 else 'gap' if case['lower'] > 1 else 'uncertain')
    # Five displayed costs change; the six other affected complete workloads are
    # retained in the linked report, without adding rows or a workload grid.
    for name, prefix in [('WebpStream', '| VP8L RGBA8, fresh seekable Stream decode'),
                         ('Vp8Stream', '| WebP VP8 + alpha, fresh seekable Stream')]:
        c = direct_inputs['cases'][name]
        found = [line for line in codecs.splitlines() if line.startswith(prefix)]
        assert len(found) == 1
        label = found[0].split('|')[1].strip()
        replacement = f'| {label} | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | — |'
        codecs = codecs.replace(found[0], replacement)
    for name, prefix in [('PngSeekable', '| Identify PNG8, fresh seekable Stream'),
                         ('PngAsync', '| Identify PNG8, async nonseekable'),
                         ('ValidatePng16Async', '| Validate PNG16, async nonseekable')]:
        c = direct_inputs['cases'][name]
        indices = [i for i, line in enumerate(read_rows) if line.startswith(prefix)]
        assert len(indices) == 1
        i = indices[0]; label = read_rows[i].split('|')[1].strip()
        status = 'Confirmed pass' if c['status'] == 'pass' else 'Confirmed gap' if c['status'] == 'gap' else 'Confidence open'
        read_rows[i] = f'| {label} | {c["managedUs"]:,.3f} | {c["nativeUs"]:,.3f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | {status} |'

    codecs, editing, read_rows, storage_current = apply_released_image_storage(codecs, editing, read_rows)
    codecs, read_rows = apply_current_cmyk_confidence(codecs, read_rows)
    codecs = apply_current_jpeg_caller_confidence(codecs)
    editing, lanczos_current = apply_lanczos_product_reuse(editing)
    editing = apply_mixed_word_cache(editing)
    editing = apply_bounded_wide_quartet(editing)
    raster_copies = json.loads((ROOT / 'benchmarks/shared-raster-copy-evidence.json').read_text(encoding='utf-8'))
    copy_q = raster_copies['qualification']
    assert raster_copies['productionAccepted'] and raster_copies['allAffectedPerformanceReconciled']
    assert raster_copies['requiredMaterialPaths'] == 289 and raster_copies['newMaterialPaths'] == 0
    assert raster_copies['raw']['mirrored'] and raster_copies['raw']['allMembersVerified']
    assert copy_q['fullChecksEachOs'] == copy_q['windows']['total'] == copy_q['linux']['total'] == 6592
    assert copy_q['actualPackage']['sha256'] == copy_q['samePackageWindows']['sameActualPackageSha256']
    assert copy_q['actualPackage']['actualConsumerPassed'] and copy_q['samePackageWindows']['actualConsumerPassed']
    assert copy_q['actualPackage']['usageExamples'] == copy_q['samePackageWindows']['usageExamples'] == 3
    assert raster_copies['currentImageOwnerViewStatuses'] == {'pass': 168, 'gap': 6, 'uncertain': 17}
    for c in raster_copies['cases'].values():
        assert c['librarySha256'] == copy_q['actualPackage']['librarySha256'] == raster_copies['producers']['timedArtifactSha256']
        assert c['actualIterations'] == 12 and c['allActualsRetained']
        assert c['managedExact'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    read_table = '\n'.join([header + ' Status |', separator + ' --- |', *read_rows])
    displayed_count = sum(len(table.splitlines()) - 2 for table in (codecs, editing, read_table))
    return f"""# Lokad.Image benchmarks

Current development version **0.2.1**. Warm single-CPU parity remains open across
**289 named material paths**; these **{displayed_count} comparisons** summarize that scope.
This is an incremental update: affected costs use the qualified immutable package
DLL; unchanged paths reuse their identified evidence. It is not a full release refresh.

[Shared axis-swapping tiles](../benchmarks/SHARED-AXIS-SWAP.md) pass all eight original
byte/word transpose and quarter-turn controls. Mirrored transpose independently
saves **10.00 µs / 23.76% raw**, **21.24% reference-normalized**; current cost is
**32.07 / FFmpeg 39.50 µs, upper .826**, **112 B/op**. One shared ownership/tile
engine replaces three duplicate classes, with unchanged owned/retained capacity.

The [current family reports](../benchmarks/WARM-COVERAGE.md) reconcile **191 existing
image views: 171 pass, 5 gaps, 15 uncertain**. The tables retain source-valid
unchanged costs; current copy/composition costs are in their linked family report.
Bounded byte-wide bicubic remains uncertain after its scoped paired gain.
Current-code adjacency leaves [grayscale PNG](../benchmarks/PNG-GRAY-CURRENT-PATH.md)
and [two opaque VP8 controls](../benchmarks/VP8-ENCODE-CURRENT-CONFIDENCE.md) uncertain;
no optimization gap is inferred.
[Caller-RGB JPEG encoding](../benchmarks/JPEG-CALLER-CURRENT-CONFIDENCE.md) also
remains uncertain under the current adjacent comparison, with exact packets.
The four Lanczos reduction and eight equivalent bicubic one-shot/prepared controls
pass and stop. [Shared full-raster copying](../benchmarks/SHARED-RASTER-COPY.md)
retains its 7.41% normalized word-clone gain and two passing byte-copy controls;
word copies retain small gaps, and complete word PNG composition stays uncertain.
No pipeline speedup is inferred from historical stage sums.
[Current horizontal-CMYK JPEG](../benchmarks/CMYK-CURRENT-OWNED-CONFIDENCE.md)
remains a retained-context gap and an equivalent fresh-context pass. A word-product
prototype was rejected after conflicting complete-operation results.
The unchanged sparse-2D build is now uncertain; compact-output candidates were
rejected for demonstrated companion regressions. Their costs are not displayed.
Adverse and uncertain captures stay linked; small copy/turn gaps remain open.

Every operation runs sequentially on the dedicated quiet VM, pinned to one CPU,
with .NET 10, optimized JIT and nonconcurrent workstation GC. Startup, transport,
fixture preparation and validation are outside the clocks. Timed calls retain
their declared normalization, input delivery, output ownership and disposal.
Ratios are Lokad/reference; **upper** is the conservative confidence bound, with
all actuals retained. Upper ≤1 passes, lower >1 is a gap; otherwise uncertainty stays open.
**GC B/op** reports managed allocation, including result ownership where applicable,
not peak live capacity or native allocation.

The default store still has **two slots / 4 MiB**. Source-derived word-composition
idle payload is **2,125,872 B**; four simultaneous image payloads total **2,455,088 B**.
Caller-governed storage bypasses that cache. Logical admitted
capacity, allocation and retained storage are separate; physical/native/JIT peaks
are unmeasured. Prepared word Lanczos reduction retains **156,248 B** of plan
storage, separately from its **138 B/op**. Each OS passes **6,600 full/live checks**, focused normal/scalar
ownership checks and the **same actual Release-package consumer / three examples**.
Unchanged arithmetic/ISA qualification is reused; publication is postponed.

† Reference boundaries require care. Codec contexts or filter graphs may remain
prepared; packed conversion, independent payload acquisition and release are timed
as declared. **zimg** is FFmpeg's labeled scaler backend, using float intermediates;
managed Q30/rational pixel guarantees remain exact. One-shot and prepared results
are distinct. Grayscale JPEG uses approved **SIMD libjpeg-turbo**; lossless WebP uses
**libwebp exact=1** where labeled. These are benchmark tools, never production dependencies.

Text comparisons use **ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12**.
The complete one-shot and preparation-plus-eight-draw public integrations pass,
including every layout/fitting call and fresh canvas ownership. Preparation is
reported separately and the complete repeated batch amortized across eight draws.
Loading and prepared-draw rows remain scoped diagnostics: eager/lazy validation
and retained raster state differ; historical stage times are never added to infer
overall text cost. Font loading uses **FreeType 2.13.2 copy/open/retain/dispose**.
RGBA16 compositing uses **ImageSharp Rgba64 SrcOver**, not FFmpeg. Keep the approved
[visual/native bounds](../benchmarks/TEXT-DRAWING-CURRENT.md) distinct from exact
managed pixels. Complete encoder packets and narrow metadata exceptions retain
their [declared scope](codec-parity.md); equal quality numbers are not quality matching.

**Codecs**

{codecs}

**Editing**

{editing}

**Inspection and pixel-coding validation**

{read_table}

[Coverage and current family reports](../benchmarks/WARM-COVERAGE.md) identify
remaining paths, boundaries, allocations, references and raw evidence. Superseded
experiments remain linked separately. Update with `python benchmarks/render-benchmark.py`;
verify with `--check`.
"""

if __name__ == '__main__':
    text = render()
    assert len(re.findall(r'^\| ---', text, re.M)) == 3
    assert len(re.findall(r'^\|', text, re.M)) - 6 == 141, 'Expected141comparison rows.'
    output = ROOT / 'docs/benchmark.md'
    if '--check' in sys.argv:
        assert output.read_text(encoding='utf-8') == text, 'Benchmark summary differs from retained evidence.'
        print('Benchmark summary matches current and retained release evidence (three tables).')
    else:
        output.write_text(text, encoding='utf-8', newline='\n')
