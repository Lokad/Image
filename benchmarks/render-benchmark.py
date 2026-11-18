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


def apply_current_lanczos(editing):
    current = json.loads((ROOT / 'benchmarks/lanczos-identity-axis-evidence.json').read_text(encoding='utf-8'))
    storage = json.loads((ROOT / 'benchmarks/lanczos-current-package-evidence.json').read_text(encoding='utf-8'))
    package = json.loads((ROOT / 'benchmarks/jpeg-pending-publication-evidence.json').read_text(encoding='utf-8'))
    assert storage['producers'] == storage['oneAxis']['producers'] == package['producers']
    assert all(group['raw']['mirrored'] and group['raw']['allMembersVerified']
               for group in (storage, storage['oneAxis']))
    q = current['qualification']
    assert current['productionAccepted'] and current['allAffectedPerformanceReconciled']
    assert current['requiredMaterialPaths'] == 289 and current['newMaterialPaths'] == 0
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified']
    assert current['statusCounts'] == {'passCount': 8, 'gap': 0, 'uncertain': 0}
    assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total'] == 6742
    assert q['actualPackage']['sha256'] == q['samePackageWindows']['sameActualPackageSha256'] == current['producers']['packageSha256']
    assert q['actualPackage']['librarySha256'] == current['producers']['timedArtifactSha256']
    assert all(q[key]['actualConsumerPassed'] and q[key]['usageExamples'] == 3 for key in ('actualPackage', 'samePackageWindows'))
    assert q['threeNamedResizeStorageSuitesExplicitlyIncluded'] and q['oneAxisRetainedAndAsynchronousSurvivorConsumerBothOs']
    assert q['modes'] == ['normal', 'avx512-off', 'scalar'] and q['acceptedTimedLinuxArtifactRebuildsAfterMeasurement'] == 0
    assert all(current['sourceProof'].values())
    axis = current['identityAxisDiagnostics']
    assert axis['Vertical16']['cpuGainConfirmed'] and axis['Vertical16']['allActualMeanIntervalsSeparated']
    assert not axis['Horizontal16']['cpuGainConfirmed'] and not axis['Horizontal16']['cpuRegressionConfirmed']
    assert all(case['noExternalReferenceElapsedOrParityClaim'] and case['all12PairsRetained'] for case in axis.values())
    mapping = {f'Lanczos3 {"prepared" if prepared else "one-shot"} RGBA{depth} / zimg {"prepared" if prepared else "one-shot"} † → {size}': case
               for case, prepared, depth, size in [
                   ('OneShot8', False, 8, '256×129'), ('Repeated8', True, 8, '256×129'),
                   ('OneShot16', False, 16, '256×129'), ('Repeated16', True, 16, '256×129'),
                   ('OneShotEnlarge8', False, 8, '1027×519'), ('RepeatedEnlarge8', True, 8, '1027×519'),
                   ('OneShotEnlarge16', False, 16, '1027×519'), ('RepeatedEnlarge16', True, 16, '1027×519')]}
    rows, seen = [], set()
    refresh = storage['cases']
    assert set(refresh) == set(current['cases']) - {'OneShotEnlarge16', 'RepeatedEnlarge16'}
    for line in editing.splitlines():
        cells = [part.strip() for part in line.strip('|').split('|')]
        label = re.sub(r' \((?:gap|uncertain)\)', '', cells[0])
        if label in mapping:
            case = mapping[label]; c = current['cases'][case]; assert case not in seen; seen.add(case)
            assert c['status'] == 'pass' and c['upper'] <= 1 and c['actualIterations'] == c['nativeSamplesEachBracket'] == 12
            assert c['allActualsRetained'] and c['managedExact'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
            assert c['reusedUnchangedPath'] or c['librarySha256'] == current['producers']['timedArtifactSha256']
            assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
            if case in refresh:
                c = refresh[case]
                assert c['librarySha256'] == storage['producers']['linuxSha256']
                assert len(c['actualOperationCounts']) == 12 and c['upper'] <= 1 and c['status'] == 'pass'
                assert all(c[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed'))
            cells[:5] = [label, f"{c['managedUs']:,.2f}", f"{c['nativeUs']:,.2f}",
                         f"{c['ratio']:.3f} / {c['upper']:.3f}", f"{c['allocatedBytes']:,}"]
            line = '| ' + ' | '.join(cells) + ' |'
        rows.append(line)
    assert seen == set(current['cases']) and len(seen) == 8
    one_axis = json.loads((ROOT / 'benchmarks/lanczos-identity-axis-reference-evidence.json').read_text(encoding='utf-8'))
    assert one_axis['measurementAccepted'] and not one_axis['productionChanged'] and not one_axis['productionSpeedupClaim']
    assert one_axis['newMaterialPaths'] == 0 and one_axis['newComparisonViews'] == 2
    assert one_axis['producers'] == current['producers'] and one_axis['raw']['mirrored'] and one_axis['raw']['allMembersVerified']
    assert one_axis['qualification']['sameImmutablePackageAndCoreVerifiedBothOs'] and one_axis['qualification']['broadOrPackageRepeats'] == 0
    assert one_axis['measurement']['quietGuardsPassed'] and one_axis['measurement']['independentConfirmationActuals'] == 12
    assert set(one_axis['cases']) == {'Horizontal16', 'Vertical16'}
    axis_storage = storage['oneAxis']
    assert axis_storage['actuals'] == 12 and axis_storage['operationsPerActual'] == 32
    assert axis_storage['allOriginalGatesPassed'] and axis_storage['quietGuardsPassed']
    for name, case in one_axis['cases'].items():
        assert case['status'] == 'pass' and case['upper'] <= 1 and case['allActualsRetained'] and case['managedExactContractPreserved']
        assert case['actuals'] == 12 and case['operationsPerActual'] == 32 and case['sourceUnchanged'] and case['independentOwnedOutputs']
        for quality in (case['quality'], case['scalarQuality']):
            assert quality['matched'] and quality['sourceUnchanged'] and quality['rgbMaximum'] <= 1 and quality['alphaMaximum'] <= 1
            assert quality['rgbMeanSquared'] <= .0625 and quality['alphaMeanSquared'] <= .0625
        measured = axis_storage['cases'][name]
        assert measured['status'] == 'pass' and measured['upper'] <= 1 and measured['allActualsRetained']
        assert measured['outputSha256'] == case['outputSha256'] and measured['quality'] == case['quality']
        allocated = f"{min(measured['allocatedBytes']):,.0f}–{max(measured['allocatedBytes']):,.0f}"
        rows.append(f"| Lanczos3 one-axis one-shot RGBA16 / zimg one-shot † → {case['width']}×{case['height']} | {measured['managedUs']:,.2f} | {measured['nativeUs']:,.2f} | {measured['ratio']:.3f} / {measured['upper']:.3f} | {allocated} |")
    return '\n'.join(rows)


def append_one_shot_png_composition(editing):
    evidence = json.loads((ROOT / 'benchmarks/png-composition-one-shot-evidence.json').read_text(encoding='utf-8'))
    case, qualification = evidence['case'], evidence['qualification']
    assert evidence['measurementAccepted'] and not evidence['productionChanged'] and not evidence['productionSpeedupClaim']
    assert evidence['requiredMaterialPaths'] == 289 and evidence['newMaterialPaths'] == 0 and evidence['newComparisonViews'] == 1
    assert evidence['raw']['mirrored'] and evidence['raw']['allMembersVerified'] and all(evidence['sourceProof'].values())
    assert qualification['sameImmutableActualPackageReused'] and qualification['packageAndCoreIdentityVerifiedBothOs'] and qualification['broadOrPackageRepeats'] == 0
    assert case['librarySha256'] == evidence['producers']['timedArtifactSha256'] and case['actuals'] == case['nativeSamplesEachBracket'] == 12
    assert case['status'] == 'pass' and case['upper'] <= 1 and case['operationsPerSubject'] == 64 and case['allActualsRetained']
    assert all(case[k] for k in ('allOriginalInputPixelOwnershipAndStreamGatesPassed', 'quietGuardsPassed', 'allRgbaSamplesExact', 'sourceUnchanged', 'independentOwnedOutput', 'nativeCliNormalScalarPixelsExact', 'callerStreamLeftOpen', 'inputConsumedToEnd'))
    assert all(case['nativeReady'][k] for k in ('decoderPreparationTimed', 'graphPreparationTimed', 'resultWrapperPreparationTimed', 'preparedStateReleasedBeforeOutputPublication'))
    assert not case['nativeReady']['preparedStateRetainedBetweenOperations'] and evidence['originalAsymmetric']['unchanged']
    assert math.isclose(case['ratio'], case['managedUs'] / case['nativeUs'], rel_tol=1e-12)
    allocated = f"{case['allocatedBytesMinimum']:,.0f}–{case['allocatedBytesMaximum']:,.0f}"
    row = f"| PNG RGBA16 one-shot stream decode/orient/crop/fit / FFmpeg one-shot † → 127×64 | {case['managedUs']:,.2f} | {case['nativeUs']:,.2f} | {case['ratio']:.3f} / {case['upper']:.3f} | {allocated} |"
    return editing + '\n' + row, evidence


def apply_current_jpeg_preparation(codecs, read_rows):
    """Replace affected JPEG costs from their qualified current owner."""
    current = json.loads((ROOT / 'benchmarks/jpeg-horizontal-preparation-evidence.json').read_text(encoding='utf-8'))
    q = current['qualification']
    assert current['productionAccepted'] and current['allAffectedPerformanceReconciled']
    assert current['requiredMaterialPaths'] == 289 and current['newMaterialPaths'] == 0
    assert len(current['cases']) == 28
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified']
    assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total'] == 6602
    assert q['actualPackage']['sha256'] == q['samePackageWindows']['sameActualPackageSha256'] == current['producers']['actualPackageSha256']
    assert q['actualPackage']['actualConsumerPassed'] and q['samePackageWindows']['actualConsumerPassed']
    assert q['actualPackage']['usageExamples'] == q['samePackageWindows']['usageExamples'] == 3
    assert current['companionReconciliation']['controlledPair']['upper'] < 1
    assert current['companionReconciliation']['normalizedLower'] <= 1 <= current['companionReconciliation']['normalizedUpper']
    for c in current['cases'].values():
        assert c['librarySha256'] == q['actualPackage']['librarySha256'] == current['producers']['timedArtifactSha256']
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['allActualsRetained']
        assert c['managedExact'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['metadata']['sourceUnchanged'] and c['metadata']['independentOwnedOutput']
        assert c['status'] == 'pass' and c['upper'] <= 1
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    mapping = {
        'JPEG baseline420, resident decode': 'NativeDecode420',
        'JPEG DC-only, caller CMYK decode': 'CmykCaller',
        'JPEG CMYK sparse-2D, resident decode': 'CmykAdobeSparse2D',
        'JPEG YCCK420, resident decode': 'Ycck420',
        'JPEG 4:1:1, resident decode': 'Decode411',
        'JPEG 4:1:0, resident decode': 'Decode410',
        'JPEG dense CMYK, resident decode': 'CmykAdobeGeneral',
        'JPEG horizontal-only CMYK, resident decode': 'CmykAdobeAc',
        'JPEG YCCK420, caller RGB24 decode': 'Ycck420CallerRgb',
        'JPEG progressive grayscale, resident decode': 'ProgressiveGray',
        'JPEG progressive420, resident decode': 'Progressive420',
        'JPEG progressive422, resident decode': 'Progressive422',
        'JPEG ordinary progressive444, resident decode': 'Progressive444',
        'JPEG progressive444, live-token resident decode': 'ProgressiveCancelable444',
    }
    lines, seen = [], set()
    for line in codecs.splitlines():
        label = line.split(' | ')[0].removeprefix('| ').removesuffix(' †')
        label = re.sub(r' \((?:confirmed pass|confirmed gap|confidence open|gap|uncertain|pass)\)', '', label)
        if label in mapping:
            assert label not in seen; seen.add(label)
            c = current['cases'][mapping[label]]
            line = f'| {label} † | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | — |'
        lines.append(line)
    assert seen == set(mapping)
    codecs = '\n'.join(lines)
    indices = [i for i, line in enumerate(read_rows) if line.startswith('| JPEG Adobe CMYK horizontal AC / FFmpeg fresh contexts')]
    assert len(indices) == 1
    c = current['cases']['CmykAdobeAcFresh']
    read_rows[indices[0]] = f'| JPEG Adobe CMYK horizontal AC / FFmpeg fresh contexts † | {c["managedUs"]:,.3f} | {c["nativeUs"]:,.3f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | Confirmed pass |'
    return codecs, read_rows


def apply_current_prepared_input(codecs, read_rows):
    """Refresh codec stream costs; Identify has its own current owner."""
    current = json.loads((ROOT / 'benchmarks/prepared-memory-input-evidence.json').read_text(encoding='utf-8'))
    q = current['qualification']
    assert current['productionAccepted'] and current['allAffectedPerformanceReconciled']
    assert current['requiredMaterialPaths'] == 289 and current['newMaterialPaths'] == 0
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified']
    assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total'] == 6614
    assert q['actualPackage']['sha256'] == q['samePackageWindows']['sameActualPackageSha256']
    assert q['actualPackage']['preparedMemoryInputSingleCapacityRefusalCancellationRecoveryPassed']
    assert q['samePackageWindows']['preparedMemoryInputSingleCapacityRefusalCancellationRecoveryPassed']
    assert current['statusCounts'] == {'pass': 5, 'gap': 0, 'uncertain': 3}
    assert current['primaryEffect']['normalizedGainSeparated']
    for c in current['cases'].values():
        assert c['librarySha256'] == q['actualPackage']['librarySha256'] == current['producers']['timedArtifactSha256']
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['allActualsRetained']
        assert c['allOriginalGatesPassed'] and c['quietGuardsPassed'] and c['sourceUnchanged'] and c['independentOwnedOutput']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    for name, prefix in [('WebpStream', '| VP8L RGBA8, fresh seekable Stream decode'),
                         ('Vp8Stream', '| WebP VP8 + alpha, fresh seekable Stream')]:
        c = current['cases'][name]
        found = [line for line in codecs.splitlines() if line.startswith(prefix)]
        assert len(found) == 1
        label = found[0].split('|')[1].strip().replace(' (confidence open)', '')
        if c['status'] == 'uncertain': label = label.replace(' †', ' (confidence open) †')
        codecs = codecs.replace(found[0], f'| {label} | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | — |')
    return codecs, read_rows


def borrowed_copy_stage():
    stage = json.loads((ROOT / 'benchmarks/self-row-storage-evidence.json').read_text(encoding='utf-8'))
    q = stage['qualification']
    assert stage['productionAccepted'] and stage['allAffectedPerformanceReconciled']
    assert stage['requiredMaterialPaths'] == 289 and stage['newRequiredPaths'] == 0
    assert stage['raw']['mirrored'] and stage['raw']['allMembersVerified']
    assert q['windows']['total'] == q['linux']['total'] == q['fullChecksEachOs'] == 6805
    assert q['package']['sha256'] == q['windowsPackage']['sameActualPackageSha256'] == stage['producers']['packageSha256']
    assert q['package']['librarySha256'] == stage['producers']['linuxSha256'] == stage['producers']['timedArtifactSha256']
    assert all(q[n]['actualConsumerPassed'] and q[n]['usageExamples'] == 3 for n in ('package', 'windowsPackage'))
    assert q['performanceReconciledBeforeBroadQualification'] and stage['builds']['acceptedTimedLinuxRebuilds'] == 0
    assert q['modes'] == ['normal', 'avx512-off', 'scalar'] and q['focusedInternalEachMode'] == 6 and q['focusedPublicEachMode'] == 190
    assert all(stage['sourceProof'].values()) and stage['pairedEffect']['upper'] < 1 and stage['pairedEffect']['allActualMeanIntervalsSeparated']
    assert stage['memory']['managedAllocatedBytesPerOperation'] == stage['memory']['newAdmittedCapacityBytes'] == stage['memory']['newRetainedCapacityBytes'] == 0
    # Only current displayed providers participate. Superseded native boundaries
    # remain reproducible diagnostics in their own reports and raw archives.
    current = json.loads((ROOT / 'benchmarks/copy-operation-boundary-evidence.json').read_text(encoding='utf-8'))
    assert current['measurementAccepted'] and not current['productionChanged'] and not current['productionSpeedupClaim']
    assert current['referenceToolingCorrected'] and all(current['referenceFindings'].values())
    assert current['requiredMaterialPaths'] == 289 and current['newRequiredPaths'] == 0
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified'] and all(current['sourceProof'].values())
    cq = current['qualification']
    assert cq['reusedForUnchangedProduction'] and cq['fullChecksEachOs'] == 6844
    assert cq['sameActualPackageBothOs'] and cq['allThreeUsageExamplesBothOs']
    assert cq['packageSha256'] == current['producers']['packageSha256']
    assert current['builds']['correctedAuthoredNativeWorker'] == 1
    assert current['builds']['productionEmbeddedPublicHarnessDriverNativeArchiveFixturePackage'] == cq['newBroadOrPackageChecks'] == 0
    assert all(current['preflights'].values()) and all(current['repairs'].values())
    assert set(current['cases']) == {'DrawingCopy8', 'OriginalClippedCopy8', 'DrawingCopy16'}
    stage['caseProducerSha256'] = {name: stage['producers']['linuxSha256'] for name in stage['cases']}
    for name, c in current['cases'].items():
        m = current['measurements'][name]
        assert m['originalPairs'] == 12 and len(m['nativeStatistics']) == 3
        assert m['operationsPerSubject'] == c['operationsPerSubject'] == (262144 if name == 'DrawingCopy16' else 65536)
        assert m['subdivisionsPerUnit'] == c['subdivisionsPerUnit'] == (1 if name == 'DrawingCopy8' else 16)
        assert m['allSubdivisionCountsAndAggregationVerified'] and all(s['N'] == 12 for s in [m['managedStatistics'], *m['nativeStatistics']])
        assert c['referenceGeometryPreparedPerOperation'] and c['clippingTimed']
        assert c['librarySha256'] == current['producers']['linuxSha256']
        assert c['nativeBuild'] == current['referenceBuild']['identity']
        stage['caseProducerSha256'][name] = current['producers']['linuxSha256']
    stage['cases'].update(current['cases'])
    stage['currentBorrowedDrawingCounts'] = current['currentBorrowedDrawingCounts']
    assert set(stage['cases']) == {'DrawingCopy8', 'DrawingCopy16', 'DrawingCopyOverlap8', 'OriginalWordOverlap', 'OriginalClippedCopy8'}
    assert current['currentCopyCounts'] == {'pass': 2, 'gap': 2, 'uncertain': 1}
    for name, c in stage['cases'].items():
        assert c['librarySha256'] == stage['caseProducerSha256'][name]
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['allActualsRetained']
        assert c['managedExact'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed'] and c['allocatedBytes'] == 0
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    assert stage['cases']['DrawingCopy16']['operationsPerSubject'] == 262144
    return stage


def original_word_composite_stage(copy_stage):
    stage = json.loads((ROOT / 'benchmarks/original-word-composite-evidence.json').read_text(encoding='utf-8'))
    c, q = stage['case'], stage['qualification']
    assert stage['measurementAccepted'] and not stage['productionChanged'] and not stage['productionSpeedupClaim']
    assert stage['requiredMaterialPaths'] == 289 and stage['newRequiredPaths'] == 0
    assert copy_stage['reuse']['unaffectedFillCompositeVerified']
    assert q['reusedForUnchangedProduction'] and q['newBroadOrPackageChecks'] == 0
    assert q['fullChecksEachOs'] == 6745 and q['sameActualPackageBothOs'] and q['usageExamples'] == 3
    assert q['packageSha256'] == stage['producers']['packageSha256']
    assert stage['raw']['mirrored'] and stage['raw']['allMembersVerified']
    assert not stage['probe']['direct']['Passed'] and stage['probe']['snapshot']['Passed']
    assert c['librarySha256'] == stage['producers']['linuxSha256']
    assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['operationsPerActual'] == 128
    assert c['status'] == 'pass' and c['upper'] <= 1 and c['allocatedBytes'] == 0
    assert all(c[k] for k in ('allActualsRetained', 'quietGuardsPassed', 'managedExact', 'allOriginalGatesPassed',
                             'snapshotCreationAndDisposalTimed', 'resetExcludedBeforeEveryOperation', 'fullWordSamples'))
    assert c['nativeQuality']['Passed'] and c['nativeQuality']['RgbMax'] <= 1 and c['nativeQuality']['AlphaMax'] <= 1
    assert c['nativeQuality']['RgbRmse'] <= .25 and c['nativeQuality']['AlphaRmse'] <= .25
    assert c['nativeQuality']['UntouchedExact'] and c['nativeQuality']['ZeroSourceAlphaExact']
    assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    return stage


def owned_flip_reference_stage():
    stage = json.loads((ROOT / 'benchmarks/vertical-prepared-rows-evidence.json').read_text(encoding='utf-8'))
    q = stage['qualification']
    assert stage['productionAccepted'] and stage['productionChanged'] and stage['allAffectedPerformanceReconciled']
    assert stage['requiredMaterialPaths'] == 289 and stage['newRequiredPaths'] == 0
    assert stage['raw']['mirrored'] and stage['raw']['allMembersVerified'] and all(stage['sourceProof'].values())
    assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total'] == 6823
    assert q['package']['sha256'] == q['windowsPackage']['sameActualPackageSha256'] == stage['producers']['packageSha256']
    assert q['package']['librarySha256'] == stage['producers']['linuxSha256'] == stage['producers']['timedArtifactSha256']
    assert all(p['actualConsumerPassed'] and p['usageExamples'] == 3 for p in (q['package'], q['windowsPackage']))
    assert q['focusedEachMode'] == {'internal': 10, 'public': 190} and q['modes'] == ['normal', 'avx512-off', 'scalar']
    assert q['allAffectedPerformanceReconciledBeforeBroad'] and q['acceptedArtifactRebuildsAfterTiming'] == 0
    assert stage['builds']['acceptedTimedDllRebuilds'] == 0 and stage['builds']['noBuildReleasePack'] == 1
    assert set(stage['cases']) == {'WarmVerticalFlip8', 'WarmVerticalFlip16'}
    for name, c in stage['cases'].items():
        assert c['librarySha256'] == stage['producers']['linuxSha256']
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['operationsPerSubject'] == 16384
        assert c['referenceGroupsInterleaved'] and c['allocatedBytes'] == 56
        assert all(c[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed', 'managedExact', 'sourceUnchanged', 'independentOwnedOutput'))
        assert c['status'] == 'pass' and c['upper'] < 1 and stage['pairedEffects'][name]['upper'] < 1
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    return stage


def owned_edit_costs(png):
    # Select current records first. Fully superseded flip/experiment providers
    # stay in their reports, without becoming renderer dependencies.
    selected = {}
    for name in ('copy-result-admission-evidence.json', 'raster-copy-scheduling-evidence.json'):
        stage = json.loads((ROOT / ('benchmarks/' + name)).read_text(encoding='utf-8'))
        selected.update((key, (case, stage)) for key, case in stage['cases'].items())
    prior_counts = {**stage['combinedOwnedEditStatusCounts']}
    flips = owned_flip_reference_stage()
    selected.update((key, (case, flips)) for key, case in flips['cases'].items())
    nearest = nearest_row_reuse()
    for name in ('PngCropFit8', 'PngCropFit16'):
        selected[name] = (nearest['cases'][name], nearest)
    assert len(selected) == 20
    for name, (case, owner) in selected.items():
        q = owner['qualification']
        assert owner['requiredMaterialPaths'] == 289 and owner.get('newMaterialPaths', owner.get('newRequiredPaths')) == 0
        assert owner['raw']['mirrored'] and owner['raw']['allMembersVerified']
        assert case['librarySha256'] == owner['producers']['linuxSha256']
        if owner is not flips:  # Current flip qualification is checked above.
            assert owner['productionAccepted'] and owner['allAffectedPerformanceReconciled']
            package = q.get('actualPackage', q.get('package'))
            windows = q.get('samePackageWindows', q.get('windowsPackage'))
            assert package['sha256'] == windows['sameActualPackageSha256']
            assert package['sha256'] == owner['producers']['packageSha256']
            assert package['librarySha256'] == case['librarySha256']
            assert all(platform['actualConsumerPassed'] and platform['usageExamples'] == 3 for platform in (package, windows))
            assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total']
            assert q['acceptedArtifactRebuildsAfterTiming'] == 0
            assert q['allAffectedPerformanceReconciledBeforeBroadQualification']
        assert case['actualIterations'] == case['nativeSamplesEachBracket'] == 12 and case['allActualsRetained']
        assert case['allOriginalGatesPassed'] and case['quietGuardsPassed']
        if name.startswith('PngCropFit'):
            assert all(case['metadata'][key] for key in ('allRgbaSamplesExact', 'sourceUnchanged', 'independentOwnedOutput'))
        else:
            if owner is not flips:
                assert all(case[key] for key in ('managedExact', 'sourceUnchanged', 'independentOwnedOutput'))
            assert case['allocatedBytes'] == 56
        assert case['status'] == ('pass' if case['upper'] <= 1 else 'gap' if case['lower'] > 1 else 'uncertain')
        assert math.isclose(case['ratio'], case['managedUs'] / case['nativeUs'], rel_tol=1e-12)
    cases = {name: case for name, (case, _) in selected.items()}
    counts = {state: sum(case['status'] == state for case in cases.values()) for state in ('pass', 'gap', 'uncertain')}
    assert counts == {'pass': 20, 'gap': 0, 'uncertain': 0} and len(cases) == 20
    return {'cases': cases, 'statusCounts': counts, 'priorStatusCounts': prior_counts,
            'currentImageOwnerViewStatuses': flips['currentImageOwnerViewStatuses']}


def png_scan_stage():
    current = json.loads((ROOT / 'benchmarks/wide-png-scan-crc-evidence.json').read_text(encoding='utf-8'))
    q = current['qualification']
    assert current['productionAccepted'] and current['allAffectedPerformanceReconciled']
    assert current['requiredMaterialPaths'] == 289 and current['newMaterialPaths'] == 0
    assert len(current['cases']) == 60 and current['statusCounts'] == {'pass': 59, 'gap': 0, 'uncertain': 1}
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified']
    assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total'] == 6721
    assert q['sameActualPackageBothOs'] and q['allThreeUsageExamplesBothOs']
    assert q['package']['sha256'] == q['windowsPackage']['sameActualPackageSha256'] == current['producers']['packageSha256']
    assert q['package']['librarySha256'] == current['producers']['timedArtifactSha256']
    assert q['acceptedArtifactRebuildsAfterTiming'] == 0 and q['allAffectedPerformanceReconciledBeforeBroadQualification']
    for c in current['cases'].values():
        assert c['librarySha256'] == q['package']['librarySha256']
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['allActualsRetained']
        assert c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    return current


def nearest_row_reuse():
    stage = json.loads((ROOT / 'benchmarks/nearest-row-reuse-evidence.json').read_text(encoding='utf-8'))
    q = stage['qualification']
    assert stage['productionAccepted'] and stage['allAffectedPerformanceReconciled']
    assert stage['requiredMaterialPaths'] == 289 and stage['newMaterialPaths'] == 0
    assert stage['raw']['mirrored'] and stage['raw']['allMembersVerified']
    assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total'] == 6776
    assert q['actualPackage']['sha256'] == q['windowsPackage']['sameActualPackageSha256'] == stage['producers']['packageSha256']
    assert q['actualPackage']['librarySha256'] == stage['producers']['timedArtifactSha256']
    assert q['actualPackage']['actualConsumerPassed'] and q['windowsPackage']['actualConsumerPassed']
    assert q['actualPackage']['usageExamples'] == q['windowsPackage']['usageExamples'] == 3
    assert stage['pairedEffect']['upper'] < 1 and all(stage['sourceProof'].values())
    assert set(stage['cases']) == {'Down8', 'Down16', 'Up8', 'Up16', 'PngCropFit8', 'PngCropFit16'}
    for case in stage['cases'].values():
        assert case['status'] == 'pass' and case['upper'] <= 1
        assert case['librarySha256'] == stage['producers']['linuxSha256']
        assert case['actualIterations'] == case['nativeSamplesEachBracket'] == 12
        assert case['allActualsRetained'] and case['allOriginalGatesPassed'] and case['quietGuardsPassed']
        assert math.isclose(case['ratio'], case['managedUs'] / case['nativeUs'], rel_tol=1e-12)
    return stage


def current_png_stream_word_note(current, one_shot):
    stage = nearest_row_reuse()
    word, byte, up = (stage['cases'][name] for name in ('PngCropFit16', 'PngCropFit8', 'Up16'))
    gain = stage['pairedEffect']
    return ('[Completed nearest rows](../benchmarks/NEAREST-ROW-REUSE.md) qualify the current complete '
            f'PNG16 pipeline at **{word["managedUs"]:,.2f} / FFmpeg {word["nativeUs"]:,.2f} µs, '
            f'upper {word["upper"]:.6f}**, **{word["allocatedBytes"]:,.2f} B/op**. '
            f'PNG8 passes at **{byte["managedUs"]:,.2f} / {byte["nativeUs"]:,.2f} µs, upper {byte["upper"]:.6f}**, '
            f'**{byte["allocatedBytes"]:,.2f} B/op**. Native contexts remain retained; managed stream delivery, '
            'decode, orientation, crop, nearest fit, every intermediate and independent output/disposal stay timed. '
            f'All four original direct-nearest controls pass and stop. RGBA16 enlargement is **{up["managedUs"]:,.2f} / '
            f'FFmpeg {up["nativeUs"]:,.2f} µs, upper {up["upper"]:.6f}**, **{up["allocatedBytes"]:,.0f} B/op**; '
            'the other three allocate112B/op. Independent final-DLL pairs save '
            f'**{gain["savedUs"]:.3f} µs / {gain["gainPercent"]:.2f}%**, with unchanged allocation and owned/retained capacity. '
            'Copies retain exact complete samples and1024-pixel cancellation checkpoints; no pool-cap or numerical change. '
            'The original I/O-guard failures remain: bounded tmpfs holds only untimed temporary images, with original '
            'inputs/settings, complete-operation boundaries and idle-v2 thresholds unchanged. A dual-context screen stays '
            'diagnostic; accepted gain uses one default-loader process per DLL. Windows/Linux each pass6776full/live '
            'checks,534internal/204public normal/scalar and the SAME actual package/all three examples. '
            'Earlier [shared storage](../benchmarks/COMPOSITION-STORAGE-REUSE.md) independently establishes97.4%lessallocation, '
            'not a summed CPU gain. Earlier fresh-native [one-shot composition](../benchmarks/PNG-COMPOSITION-ONE-SHOT.md) '
            'and other historical storage-dependent costs still await the final complete package refresh.')


def apply_current_png_scan(codecs, reads, current):
    mapping = {
        'PNG RGBA8 decode, live token †': 'png/DecodeCancelable8',
        'PNG RGBA8 decode, tight working limit †': 'png/DecodeTight8',
        'PNG RGBA16 decode, tight working limit †': 'png/DecodeTight16',
        'PNG8 decode, two retained images / shared caller root †': 'root/Png8Retained2',
        'Validate PNG8, span †': 'validator/ValidatePng8',
        'Validate PNG16, span †': 'validator/ValidatePng16',
        'Validate indexed Adam7 19×17, span †': 'validator/ValidateIndexedAdam7',
        'Validate PNG16, async nonseekable †': 'validator/ValidatePng16Async',
    }
    seen = set()
    def update(table, read=False):
        result = []
        for line in table.splitlines():
            cells = [c.strip() for c in line.split('|')[1:-1]]
            if cells and cells[0] in mapping:
                key = mapping[cells[0]]; assert key not in seen; seen.add(key)
                c = current['cases'][key]; digits = 3 if read else 2
                cells[1:5] = [f'{c["managedUs"]:,.{digits}f}', f'{c["nativeUs"]:,.{digits}f}',
                              f'{c["ratio"]:.3f} / {c["upper"]:.3f}', f'{c["allocatedBytes"]:,}']
                if read: cells[-1] = {'pass': 'Confirmed pass', 'gap': 'Confirmed gap', 'uncertain': 'Confidence open'}[c['status']]
                line = '| ' + ' | '.join(cells) + ' |'
            result.append(line)
        return '\n'.join(result)
    codecs, reads = update(codecs), update(reads, True)
    assert seen == set(mapping.values())
    return codecs, reads


def apply_current_vp8_fixed_code(codecs, reads, current):
    q = current['qualification']
    assert current['productionAccepted'] and current['allAffectedPerformanceReconciled']
    assert current['requiredMaterialPaths'] == 289 and current['newMaterialPaths'] == 0
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified']
    assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total'] == 6692
    assert q['sameActualPackageBothOS'] and q['allThreeExamplesBothOS']
    assert q['actualPackage']['sha256'] == q['samePackageWindows']['sameActualPackageSha256']
    assert q['actualPackage']['librarySha256'] == current['producers']['timedArtifactSha256']
    assert current['comparison']['pairedLive']['scopedManagedDllEffectOnly'] and current['comparison']['pairedLive']['upper'] < 1
    assert current['comparison']['standalone']['allSixPass'] and current['comparison']['standalone']['liveGapClosed']
    assert not current['comparison']['standalone']['historicalClockSpeedupClaim']
    assert current['implementation']['sourceProof']['entireOriginalBlockSourceReconstructsByteExact']
    assert current['storage']['newOwnedHeapCapacityBytes'] == current['storage']['newRetainedStateBytes'] == 0
    assert set(current['cases']) == {'Opaque8', 'OpaqueCancelable8', 'SimpleOpaque8', 'Native8', 'StreamNative8', 'ValidateVp8Alpha'}
    for name, c in current['cases'].items():
        assert c['librarySha256'] == current['producers']['timedArtifactSha256']
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['allActualsRetained']
        assert c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert c['status'] == 'pass'
        v = c['metadata']
        assert v['sourceUnchanged'] and v['independentOwnedOutput']
        if name == 'ValidateVp8Alpha':
            assert v['descriptorExact'] and v['prefixAndCodingScopesVerified']
        else:
            assert v['managedOutputSha256'] == v['nativeOutputSha256']
            assert v['decoderMaximum'] == v['decoderRmse'] == 0 and v['alphaExact'] and v['nativeCliPixelsExact']
            if name in {'OpaqueCancelable8', 'SimpleOpaque8', 'StreamNative8'}:
                assert v['scalarCliPixelsExact']
    c = current['cases']['StreamNative8']
    prefix = '| WebP VP8 + alpha, fresh seekable Stream'
    found = [line for line in codecs.splitlines() if line.startswith(prefix)]
    assert len(found) == 1
    codecs = codecs.replace(found[0], f'{prefix} (parity confirmed) † | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | — |')
    c = current['cases']['ValidateVp8Alpha']
    prefix = '| Validate VP8+alpha, span'
    found = [line for line in reads.splitlines() if line.startswith(prefix)]
    assert len(found) == 1
    reads = reads.replace(found[0], f'{prefix} † | {c["managedUs"]:,.3f} | {c["nativeUs"]:,.3f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | Confirmed pass |')
    return codecs, reads


def apply_current_bicubic_adjacent_confidence(editing):
    current = json.loads((ROOT / 'benchmarks/bicubic-current-adjacent-confidence-evidence.json').read_text(encoding='utf-8'))
    q = current['qualification']
    assert current['measurementAccepted'] and not current['productionChanged'] and not current['productionSpeedupClaim']
    assert current['requiredMaterialPaths'] == 289 and current['newMaterialPaths'] == 0
    assert current['raw']['mirrored'] and current['raw']['allMembersVerified']
    assert q['productionAndActualPackageByteIdentical'] and q['fullChecksEachOs'] == 6624
    assert q['sameActualPackageBothOsAndThreeExamplesPassed'] and q['broadOrPackageRepeats'] == 0
    assert current['measurement']['allBuilds'] == 0 and current['measurement']['operationsEachSubject'] == 64
    assert current['currentImageOwnerViewStatuses'] == {'passCount': 179, 'gaps': 1, 'uncertain': 11, 'total': 191}
    for c in current['cases'].values():
        assert c['librarySha256'] == q['actualPackageLibrarySha256'] == current['producers']['timedArtifactSha256']
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['allActualsRetained']
        assert c['managedExact'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['sourceUnchanged'] and c['independentOwnedOutput'] and c['allocatedBytes'] == 112
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert c['nativeRgbMaximum'] <= 1 and c['nativeAlphaMaximum'] <= 1
        assert c['nativeRgbRmse'] <= .25 and c['nativeAlphaRmse'] <= .25
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    for name, depth in [('TightWide8', 8), ('TightWide16', 16)]:
        c = current['cases'][name]
        assert c['status'] == 'pass'
        prefix = f'| Bicubic RGBA{depth} reduction, input+output+4 KiB'
        found = [line for line in editing.splitlines() if line.startswith(prefix) and '128×64' in line]
        assert len(found) == 1
        editing = editing.replace(found[0], f'{prefix} † → 128×64 | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} |')
    c = current['cases']['TightWide8']
    word = current['cases']['TightWide16']
    mixed = current['cases']['Mixed16']
    wide_current = json.loads((ROOT / 'benchmarks/bicubic-wide-current-confidence-evidence.json').read_text(encoding='utf-8'))
    streaming = json.loads((ROOT / 'benchmarks/streaming-column-reuse-evidence.json').read_text(encoding='utf-8'))
    for scoped in (wide_current,):
        c, sq = scoped['case'], scoped['qualification']
        assert scoped['measurementAccepted'] and not scoped['productionChanged'] and not scoped['productionSpeedupClaim']
        assert scoped['measurement']['allBuilds'] == scoped['measurement']['broadOrPackageRepeats'] == 0
        assert scoped['measurement']['previousOperationsPerSubject'] == 64 and c['operationsPerSubject'] == 256
        assert scoped['raw']['mirrored'] and scoped['raw']['allMembersVerified']
        assert c['librarySha256'] == scoped['producers']['timedArtifactSha256'] == sq['actualPackageLibrarySha256']
        assert scoped['producers']['packageSha256'] == sq['actualPackageSha256'] and sq['fullChecksEachOs'] == 6690
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['allActualsRetained']
        assert c['managedExact'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['sourceUnchanged'] and c['independentOwnedOutput']
        assert c['metadata']['nativeRgbMaximum'] <= 1 and c['metadata']['nativeAlphaMaximum'] <= 1
        assert c['metadata']['nativeRgbRmse'] <= .25 and c['metadata']['nativeAlphaRmse'] <= .25
    sq = streaming['qualification']
    assert streaming['productionAccepted'] and streaming['allAffectedPerformanceReconciled']
    assert streaming['raw']['mirrored'] and streaming['raw']['allMembersVerified']
    assert sq['fullChecksEachOs'] == sq['windows']['total'] == sq['linux']['total'] == 6702
    assert sq['package']['sha256'] == sq['windowsPackage']['sameActualPackageSha256']
    assert sq['package']['librarySha256'] == streaming['producers']['linuxSha256']
    assert sq['actualPackageNewPairedPathPassedBothOs'] and sq['acceptedArtifactRebuildsAfterTiming'] == 0
    assert set(streaming['cases']) == {'Tight16x8_16', 'Tight16x8_8', 'Tight8x4_16', 'Tight8x4_8', 'Tight8x17_16', 'Tight8x17_8'}
    for c in streaming['cases'].values():
        assert c['status'] == 'pass' and c['upper'] <= 1 and c['allocatedBytes'] == 112
        assert c['librarySha256'] == streaming['producers']['linuxSha256']
        assert c['allActualsRetained'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['managedExact'] and c['sourceUnchanged'] and c['independentOwnedOutput']
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['operationsPerSubject'] == 256
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    assert streaming['memory']['workspaceAdmissionBytes'] == 4096
    assert streaming['currentResizeStatusCounts'] == {'passCount': 72, 'gaps': 0, 'uncertain': 1, 'total': 73}
    wide, bounded = wide_current['case'], streaming['cases']['Tight16x8_16']
    assert wide['status'] == 'pass' and wide['allocatedBytes'] == 198350
    assert bounded['status'] == 'pass' and bounded['allocatedBytes'] == 112
    prefix = '| Bicubic RGBA8, fresh managed / prepared zimg'
    found = [line for line in editing.splitlines() if line.startswith(prefix) and '128×64' in line]
    assert len(found) == 1
    editing = editing.replace(found[0], f'{prefix} † → 128×64 | {wide["managedUs"]:,.2f} | {wide["nativeUs"]:,.2f} | {wide["ratio"]:.3f} / {wide["upper"]:.3f} | {wide["allocatedBytes"]:,} |')
    c = current['cases']['TightWide8']
    note = (f'[Current bicubic confidence](../benchmarks/BICUBIC-CURRENT-ADJACENT-CONFIDENCE.md) '
            f'resolves default mixed-word resize (**{mixed["managedUs"]:,.2f} / zimg {mixed["nativeUs"]:,.2f} µs, '
            f'upper {mixed["upper"]:.3f}**) and bounded byte-wide resize (**{c["managedUs"]:,.2f} / '
            f'{c["nativeUs"]:,.2f} µs, upper {c["upper"]:.3f}**). Bounded word-wide also passes '
            f'(**{word["managedUs"]:,.2f} / {word["nativeUs"]:,.2f} µs, upper {word["upper"]:.3f}**). '
            'All three stop, **112 B/op**, zero builds. '
            f'[Default byte-wide longer integration](../benchmarks/BICUBIC-WIDE-CURRENT-CONFIDENCE.md) '
            f'also passes/stops: **{wide["managedUs"]:,.2f} / zimg {wide["nativeUs"]:,.2f}µs, upper {wide["upper"]:.6f}**, '
            '**198,350 B/op**. One 64→256 integration retains all actuals and the original uncertain capture; '
            'zero builds/qualification repeats, no production speedup. '
            f'[Streaming column reuse](../benchmarks/STREAMING-COLUMN-REUSE.md) closes the '
            f'bounded 16×8 word uncertainty: **{bounded["managedUs"]:,.2f} / zimg '
            f'{bounded["nativeUs"]:,.2f}µs, upper {bounded["upper"]:.6f}**, **112 B/op**. '
            'Adjacent columns share pixel conversion and vertical preparation, preserving exact ordered sums. '
            'All six affected streaming controls pass/stop; workspace admission remains **4 KiB**, '
            'with no retained operation workspace. Both OSes pass **6,702 full/live** checks, '
            'focused normal/scalar/AVX512-disabled checks and the **SAME actual package/all three examples**. '
            'The adverse first implementation and earlier uncertainty remain history. '
            'Fresh managed/prepared native boundaries remain explicit; equivalent prepared/one-shot passes are reused.')
    return editing, note, streaming


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


def stream_reader_stage():
    owner = json.loads((ROOT / 'benchmarks/probe-owner-completion-evidence.json').read_text(encoding='utf-8'))
    assert owner['productionAccepted'] and owner['allAffectedPerformanceReconciled']
    assert owner['requiredMaterialPaths'] == 289 and owner['newMaterialPaths'] == 0
    assert owner['raw']['mirrored'] and owner['raw']['allMembersVerified']
    q = owner['qualification']
    assert q['windows']['total'] == q['linux']['total'] == q['fullChecksEachOs'] == 6727
    assert q['sameActualPackageBothOs'] and q['allThreeUsageExamplesBothOs']
    assert q['package']['sha256'] == q['windowsPackage']['sameActualPackageSha256']
    assert q['package']['librarySha256'] == owner['producers']['linuxSha256']
    assert q['acceptedArtifactRebuildsAfterTiming'] == 0 and q['allAffectedPerformanceReconciledBeforeBroadQualification']
    assert q['focusedEachMode'] == {'internal': 10, 'public': 154}
    assert owner['primaryPair']['upper'] < 1
    assert owner['primaryPair']['allocatedBaseline'] == owner['primaryPair']['allocatedCandidate'] == 568
    assert owner['primaryAllActualStatistics']['candidate']['ConfidenceInterval']['Upper'] < owner['primaryAllActualStatistics']['baseline']['ConfidenceInterval']['Lower']
    assert all(v['baselineAndCandidatePassed'] for v in q['newUnsignaledCancellationCasesBothImplementationsBothOs'].values())
    assert owner['pendingAllocationDiagnostic']['costs']['ProbeAsync']['addedBytes'] == 8
    assert owner['pendingAllocationDiagnostic']['costs']['InspectHeadersAsync']['addedBytes'] == 0
    assert all(owner['sourceProof'].values())
    assert set(owner['cases']) == {'ProbeDimensionsPng', 'ProbeDimensionsWebp', 'ProbeDimensionsJpeg', 'ProbeDimensionsJpegAsync'}
    for case in owner['cases'].values():
        assert case['managedActuals'] == case['nativeSamplesEachBracket'] == 12
        assert case['allocatedBytes'] == 568 and case['allActualsRetained']
        assert case['allOriginalGatesPassed'] and case['quietGuardsPassed']
        assert case['librarySha256'] == owner['producers']['linuxSha256']
        assert case['allActualStatistics']['N'] == 12 and 8 <= case['filteredResultCount'] <= 12
        assert case['upper'] >= case['allActualUpper']
        assert case['status'] == ('pass' if case['upper'] <= 1 else 'gap' if case['lower'] > 1 else 'uncertain')
    previous = json.loads((ROOT / owner['reusedCaseProvider']).read_text(encoding='utf-8'))
    assert set(owner['reusedCases']) == {'PngHeaders'}
    assert previous['productionAccepted'] and previous['qualification']['sameActualPackageBothOs']
    for name, case in owner['reusedCases'].items():
        assert case == previous['cases'][name] and case['status'] == 'pass'
        assert case['librarySha256'] == previous['producers']['linuxSha256']
        assert case['managedActuals'] == case['nativeSamplesEachBracket'] == 12
        assert case['allOriginalGatesPassed'] and case['quietGuardsPassed'] and case['allActualsRetained']
        assert case['allocatedBytes'] == 568
    owner['cases'] = {**owner['reusedCases'], **owner['cases']}
    return owner


def jpeg_pending_publication_stage():
    stage = json.loads((ROOT / 'benchmarks/jpeg-pending-publication-evidence.json').read_text(encoding='utf-8'))
    assert stage['productionAccepted'] and stage['allAffectedPerformanceReconciled']
    assert stage['requiredMaterialPaths'] == 289 and stage['newMaterialPaths'] == 0
    assert stage['raw']['mirrored'] and stage['raw']['allMembersVerified'] and all(stage['sourceProof'].values())
    q, producer = stage['qualification'], stage['producers']
    assert q['windows']['total'] == q['linux']['total'] == q['fullChecksEachOs'] == 6844
    assert q['package']['sha256'] == q['windowsPackage']['sameActualPackageSha256'] == producer['packageSha256']
    assert q['package']['librarySha256'] == producer['linuxSha256'] == producer['timedArtifactSha256']
    assert q['sameActualPackageBothOs'] and q['allThreeUsageExamplesBothOs']
    assert q['allAffectedPerformanceReconciledBeforeBroad'] and q['productionRebuildsAfterTiming'] == 0
    assert q['focusedEachMode'] == {'internal': 11, 'public': 182} and q['modes'] == ['normal', 'scalar']
    assert stage['pairedEffect']['upper'] < 1
    assert stage['pairedEffect']['baselineAllocatedBytes'] == stage['pairedEffect']['candidateAllocatedBytes'] == 568
    assert set(stage['cases']) == {'ProbeDimensionsJpeg', 'ProbeDimensionsJpegAsync', 'JpegHeaders'}
    for name, c in stage['cases'].items():
        assert c['librarySha256'] == producer['linuxSha256']
        assert c['allActualsRetained'] and c['quietGuardsPassed']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
        if name.startswith('Probe'):
            assert c['managedActuals'] == c['nativeSamplesEachBracket'] == c['allActualStatistics']['n'] == 12
            assert c['allOriginalGatesPassed'] and c['allocatedBytes'] == 568 and c['rawOverheadIncluded']
            assert c['managedOperationsPerUnit'] == (974960 if name == 'ProbeDimensionsJpeg' else 855040)
            assert c['nativeOperationsPerUnitEachBracket'] == 262144 and c['subdivisionsEachStatisticalUnit'] == 16
            assert c['upper'] >= c['allActualUpper']
        else:
            assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['allocatedBytes'] == 8768
            assert c['descriptorExact'] and c['independentSurvivingOutputs'] and c['eofChecked'] and c['inputUnchanged']
    for obs in stage['pendingAllocationDiagnostic']['observations']:
        assert all(obs[k] for k in ('descriptorExact', 'genuineSuspension', 'independentOutputs', 'callerInputLeftOpen'))
        assert all(v['bytesPerOperation'] == (1960 if obs['mode'] == 'ProbeAsync' else 9984) for v in obs['batches'])
    assert stage['codegen']['immediateStackReservationBefore'] == 1080
    assert stage['codegen']['immediateStackReservationAfter'] == 472
    assert stage['headerGuard']['lower'] < 1 < stage['headerGuard']['upper']
    return stage


def jpeg_probe_longer_integration_stage(qualified):
    stage = json.loads((ROOT / 'benchmarks/jpeg-probe-longer-integration-evidence.json').read_text(encoding='utf-8'))
    assert stage['measurementAccepted'] and not stage['productionChanged'] and not stage['productionSpeedupClaim']
    assert stage['requiredMaterialPaths'] == 289 and stage['newRequiredPaths'] == 0
    assert stage['producers'] == qualified['producers'] and all(stage['sourceProof'].values())
    assert stage['raw']['mirrored'] and stage['raw']['allMembersVerified'] and stage['unchangedCaseCapturesStopped']
    c, spec, q = stage['case'], stage['integration'], stage['qualification']
    assert c['librarySha256'] == stage['producers']['timedArtifactSha256']
    assert c['managedActuals'] == c['nativeSamplesEachBracket'] == c['allActualStatistics']['n'] == 12
    assert c['allOriginalGatesPassed'] and c['quietGuardsPassed'] and c['allActualsRetained'] and c['rawOverheadIncluded']
    assert c['allocatedBytes'] == 568 and c['subdivisionsEachStatisticalUnit'] == 16
    assert c['managedOperationsPerUnit'] == spec['managedOperationsPerUnit'] == 3420160
    assert c['nativeOperationsPerUnitEachBracket'] == spec['nativeOperationsEachBracketPerUnit'] == 1048576
    assert spec['multiplier'] == 4 and spec['originalManagedWarmupOperations'] == 855040
    assert spec['originalNativeWarmupOperationsEachBracket'] == 262144 and spec['originalWholeWarmups'] == 3
    assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
    assert c['upper'] >= c['allActualUpper'] and math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    assert all(stage['preflights'].values()) and stage['receipt']['allSubdivisionCountsAndAggregationVerified']
    assert stage['receipt']['originalWarmupCountsRetained'] and stage['receipt']['statisticalUnitsUnchanged']
    assert q['reusedForUnchangedProduction'] and q['fullChecksEachOs'] == 6844 and q['newBroadOrPackageChecks'] == 0
    assert q['sameActualPackageBothOs'] and q['allThreeUsageExamplesBothOs']
    assert q['packageSha256'] == stage['producers']['packageSha256']
    assert stage['builds'] == {'smallOrchestrationDriver': 1, 'productionHarnessNativeFixtureOriginalLoopPackage': 0}
    return stage


def text_scan_band_stage():
    stage = json.loads((ROOT / 'benchmarks/text-scan-band-coordinates-evidence.json').read_text(encoding='utf-8'))
    assert stage['productionAccepted'] and stage['allAffectedPerformanceReconciled']
    assert stage['requiredMaterialPaths'] == 289 and stage['newRequiredPaths'] == 0
    assert stage['raw']['mirrored'] and stage['raw']['allMembersVerified'] and all(stage['sourceProof'].values())
    q = stage['qualification']
    assert q['windows']['total'] == q['linux']['total'] == q['fullChecksEachOs'] == 6824
    assert q['package']['sha256'] == q['windowsPackage']['sameActualPackageSha256'] == stage['producers']['packageSha256']
    assert q['package']['librarySha256'] == stage['producers']['linuxSha256'] == stage['producers']['timedArtifactSha256']
    assert all(q[k]['actualConsumerPassed'] and q[k]['usageExamples'] == 3 for k in ('package', 'windowsPackage'))
    assert q['allAffectedPerformanceReconciledBeforeBroad'] and q['acceptedArtifactRebuildsAfterTiming'] == 0
    assert q['focusedEachMode'] == {'internal': 307, 'public': 177} and q['modes'] == ['normal', 'avx-off', 'scalar']
    assert stage['pairedEffect']['upper'] < 1 and stage['pairedEffect']['operationsEach'] == 16384
    assert set(stage['cases']) == {'Ordinary8', 'Ordinary16'}
    for c in stage['cases'].values():
        assert c['librarySha256'] == stage['producers']['linuxSha256']
        assert c['actuals'] == 12 and c['operationsPerActual'] == 16384 and c['everyReferenceDrawChecked']
        assert c['referenceAllocatedBytes'] == 48576 and len(c['referenceAllocationSamples']) == 36
    assert set(stage['authoredCases']) == {'Render8', 'Render16'}
    for c in stage['authoredCases'].values():
        assert c['bdnGc']['managed']['bytesPerOperation'] == c['allocatedBytes'] + c['bdnManagedScaffoldingBytes']
        assert c['bdnManagedScaffoldingBytes'] == 736
    assert stage['memory']['ordinaryPublicAllocatedBytes'] == 83008
    assert stage['memory']['governedCompleteFitPeakBytes'] == 748984
    assert stage['memory']['retainedPlanCapacityBytes'] == 4160
    return stage


def ordinary_word_interleaving(base):
    stage = json.loads((ROOT / 'benchmarks/ordinary-word-interleaved-reference-evidence.json').read_text(encoding='utf-8'))
    assert stage['measurementAccepted'] and not stage['productionChanged'] and not stage['productionSpeedupClaim']
    assert stage['requiredMaterialPaths'] == 289 and stage['newRequiredPaths'] == 0
    assert stage['producers'] == base['producers'] and all(stage['sourceProof'].values())
    assert stage['raw']['mirrored'] and stage['raw']['allMembersVerified']
    assert stage['allOriginalGatesPassed'] and stage['quietGuardsPassed'] and stage['allActualsRetained']
    assert stage['preflights']['normalScalarAllOriginalFieldsMatch'] and stage['originalHarnessAndReferencesUnchanged']
    q = stage['qualificationReused']
    assert q['fullLiveEachOs'] == 6824 and q['sameActualPackageBothOs'] and q['usageExamplesEachOs'] == 3
    assert q['newBroadPackageChecks'] == q['productionRebuilds'] == 0
    c = stage['case']
    assert c['referenceGroupsInterleaved'] and c['actuals'] == 12 and c['operationsPerActual'] == 16384
    assert c['librarySha256'] == stage['producers']['linuxSha256'] and len(c['referenceAllocationSamples']) == 36
    assert c['referenceAllocatedBytes'] == 48576 and c['allocatedBytes'] == 83008
    assert c['status'] == 'pass' and c['upper'] < 1
    return stage


def render():
    # Check current measurement owners; rejected experiments retain their own
    # linked evidence without becoming inputs to every summary update.
    evidence = json.loads((ROOT / 'benchmarks/release-0.2.0-benchmark-evidence.json').read_text(encoding='utf-8'))
    assert evidence['version'] == '0.2.0' and len(evidence['cases']) == 31
    assert evidence['allDisplayedCasesRefreshed'] and not evidence['productionChanged']
    stored = json.loads((ROOT / 'benchmarks/warm-png-bulk-stored-evidence.json').read_text(encoding='utf-8'))
    current = json.loads((ROOT / 'benchmarks/warm-png-distance-symbol-evidence.json').read_text(encoding='utf-8'))
    reader_current = json.loads((ROOT / 'benchmarks/vp8-fixed-code-evidence.json').read_text(encoding='utf-8'))
    assert reader_current['productionAccepted'] and reader_current['requiredMaterialPaths'] == 289
    assert reader_current['raw']['mirrored'] and reader_current['raw']['allMembersVerified']
    assert reader_current['allAffectedPerformanceReconciled'] and not reader_current['materialPassToGapContradictions']
    assert set(reader_current['cases']) == {'Native8', 'StreamNative8', 'Opaque8', 'SimpleOpaque8', 'OpaqueCancelable8', 'ValidateVp8Alpha'}
    vp8_costs = {('Vp8Stream' if name == 'StreamNative8' else name): {**cost['metadata'], **cost}
                 for name, cost in reader_current['cases'].items()}
    for cost in vp8_costs.values():
        assert cost['actualIterations'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['librarySha256'] == reader_current['producers']['timedArtifactSha256']
        assert cost['allOriginalGatesPassed'] and cost['quietGuardsPassed']
        assert cost['sourceUnchanged'] and cost['independentOwnedOutput']
        if cost['caseId'] == 'ValidateVp8Alpha':
            assert cost['descriptorExact'] and cost['prefixAndCodingScopesVerified']
        else:
            assert cost['managedOutputSha256'] == cost['nativeOutputSha256'] and cost['alphaExact']
            assert cost['decoderMaximum'] == 0 and cost['managedOutputBytes'] == 531468
        assert cost['status'] == ('pass' if cost['upper'] <= 1 else 'gap' if cost['lower'] > 1 else 'uncertain')
        assert math.isclose(cost['ratio'], cost['managedUs'] / cost['nativeUs'], rel_tol=1e-12)
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
    lpf.update(vp8_costs)
    lossless_read_counts = {state: sum(lpf[name]['status'] == state for name in ('WebpResident', 'Lossless8', 'IndexedNative16Colors8', 'NativeLosslessCancelable8', 'WebpStream', 'ValidateVp8lAlpha')) for state in ('pass', 'gap', 'uncertain')}
    assert lossless_read_counts == {'pass': 6, 'gap': 0, 'uncertain': 0}
    vp8_decode_counts = {state: sum(lpf[name]['status'] == state for name in vp8_costs)
                         for state in ('pass', 'gap', 'uncertain')}
    assert vp8_decode_counts == {'pass': 6, 'gap': 0, 'uncertain': 0}
    jpeg_decode = baseline_blocks
    assert not decoders['productionChanged'] and len(decoders['cases']) == 3
    assert not reconciled['productionChanged'] and len(reconciled['cases']) == 6
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
    jpeg_replay = json.loads((ROOT / 'benchmarks/jpeg-local-entropy-replay-evidence.json').read_text(encoding='utf-8'))
    assert jpeg_replay['productionAccepted'] and jpeg_replay['allAffectedPerformanceReconciled']
    assert jpeg_replay['requiredMaterialPaths'] == 289 and jpeg_replay['newMaterialPaths'] == 0
    assert jpeg_replay['raw']['mirrored'] and jpeg_replay['raw']['allMembersVerified']
    jpeg_q = jpeg_replay['qualification']
    assert jpeg_q['fullChecksEachOs'] == jpeg_q['windows']['total'] == jpeg_q['linux']['total'] == 6640
    assert jpeg_q['actualPackage']['sha256'] == jpeg_q['samePackageWindows']['sameActualPackageSha256']
    assert jpeg_q['actualPackage']['actualConsumerPassed'] and jpeg_q['samePackageWindows']['actualConsumerPassed']
    assert jpeg_q['actualPackage']['usageExamples'] == jpeg_q['samePackageWindows']['usageExamples'] == 3
    assert jpeg_q['actualPackage']['librarySha256'] == jpeg_replay['producers']['linuxSha256']
    assert jpeg_q['acceptedArtifactRebuildsAfterTiming'] == 0 and jpeg_q['allAffectedPerformanceReconciledBeforeBroadQualification']
    # Only the17still-displayed/current adaptive cases retain this owner;
    # its superseded restart measurements remain in the linked history.
    jpeg_encoding_cases = {name: c for name, c in jpeg_replay['cases'].items() if name != 'Encode420Restart8'}
    assert len(jpeg_encoding_cases) == 17 and all(c['status'] == 'pass' for c in jpeg_encoding_cases.values())
    for name, c in jpeg_encoding_cases.items():
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['allActualsRetained']
        assert c['librarySha256'] == jpeg_replay['producers']['linuxSha256']
        assert c['allOriginalGatesPassed'] and c['quietGuardsPassed'] and c['independentOwnedOutput'] and c['sourceUnchanged']
        assert c['completePacketExact'] and c['metadataExceptionBytes'] == (0 if name in ('RgbaGray', 'CallerGray') else 2)
        assert c['nativePacketBytes'] - c['managedPacketBytes'] == c['metadataExceptionBytes']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    for c in jpeg_replay['comparison'].values():
        assert c['upper'] < 1 and c['pairs'] == 12 and c['operationsEachSubject'] == 64
        assert c['allActualsRetained'] and c['outputExact'] and c['independentConfirmationAfterShort']
    assert jpeg_replay['memory']['newHeapCapacityBytes'] == jpeg_replay['memory']['newRetainedBytes'] == 0
    jpeg_direct = json.loads((ROOT / 'benchmarks/jpeg-direct-block-emission-evidence.json').read_text(encoding='utf-8'))
    assert jpeg_direct['productionAccepted'] and jpeg_direct['allAffectedPerformanceReconciled']
    assert jpeg_direct['requiredMaterialPaths'] == 289 and jpeg_direct['newMaterialPaths'] == 0
    assert jpeg_direct['raw']['mirrored'] and jpeg_direct['raw']['allMembersVerified']
    direct_q = jpeg_direct['qualification']
    assert direct_q['fullChecksEachOs'] == direct_q['windows']['total'] == direct_q['linux']['total'] == 6648
    assert direct_q['actualPackage']['sha256'] == direct_q['samePackageWindows']['sameActualPackageSha256']
    assert direct_q['actualPackage']['actualConsumerPassed'] and direct_q['samePackageWindows']['actualConsumerPassed']
    assert direct_q['actualPackage']['usageExamples'] == direct_q['samePackageWindows']['usageExamples'] == 3
    assert direct_q['acceptedArtifactRebuildsAfterTiming'] == 0 and direct_q['allAffectedPerformanceReconciledBeforeBroadQualification']
    assert jpeg_direct['sourceProof']['allConfirmed'] and jpeg_direct['sourceProof']['privateCodeValidationAndBlockScheduleUnchangedFromA']
    assert jpeg_direct['sourceProof']['generalInt32CategoryUnchanged'] and jpeg_direct['sourceProof']['completeDcDifferenceDomainChecked'] == 131071
    assert set(jpeg_direct['reusedPassingAdaptiveCases']) == set(jpeg_encoding_cases)
    for name, prior in jpeg_direct['reusedPassingAdaptiveCases'].items():
        assert prior['librarySha256'] == jpeg_encoding_cases[name]['librarySha256'] and prior['status'] == 'pass'
    direct_c = jpeg_direct['cases']['Encode420Restart8']
    assert direct_c['librarySha256'] == jpeg_direct['producers']['linuxSha256'] == direct_q['actualPackage']['librarySha256']
    assert direct_c['actualIterations'] == direct_c['nativeSamplesEachBracket'] == 12 and direct_c['allActualsRetained']
    assert direct_c['allOriginalGatesPassed'] and direct_c['quietGuardsPassed'] and direct_c['sourceUnchanged'] and direct_c['independentOwnedOutput']
    assert direct_c['completePacketExact'] and direct_c['metadataExceptionBytes'] == 2
    assert (direct_c['managedPacketBytes'], direct_c['nativePacketBytes']) == (58010, 58012)
    assert direct_c['status'] == 'pass' and direct_c['upper'] <= 1 and math.isclose(direct_c['ratio'], direct_c['managedUs']/direct_c['nativeUs'], rel_tol=1e-12)
    comparison = jpeg_direct['comparison']
    assert comparison['ratioUpper'] < 1 and comparison['pairs'] == 12 and comparison['operationsEachSubject'] == 64
    assert comparison['allActualsRetained'] and comparison['independentConfirmationAfterShort'] and comparison['outputExact']
    assert jpeg_direct['memory']['newHeapCapacityBytes'] == jpeg_direct['memory']['newRetainedBytes'] == 0
    jpeg_encoding_cases['Encode420Restart8'] = direct_c
    assert len(jpeg_encoding_cases) == 18 and all(c['status'] == 'pass' for c in jpeg_encoding_cases.values())
    jpeg_direct_note = f"""[Direct JPEG block emission](../benchmarks/JPEG-DIRECT-BLOCK-EMISSION.md) closes
the restart gap: **{direct_c['managedUs']:,.2f} / FFmpeg {direct_c['nativeUs']:,.2f} µs, upper {direct_c['upper']:.3f}**,
**{direct_c['allocatedBytes']:,} B/op**, exact **58,010 / 58,012 B** packets with the strict metadata exception.
All 18 declared JPEG encoding comparisons pass and stop; 17 adaptive controls
reuse their unchanged paths and original artifact identities. Separate complete
paired confirmations measure **19.21%** for block emission and **{100*comparison['rawReduction']:.2f}%** for bounded
sign arithmetic. These successive effects are not added into a cumulative claim.
No heap capacity or retention is added; validation and independent owners remain."""
    jpeg_labels = {
        'Encode444Stream8': 'JPEG 4:4:4, forward-only / exact reference',
        'Encode420Restart8': 'JPEG RGBA8 4:2:0, row restart33',
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
        if name == 'Encode420Restart8':
            assert not positions
            positions = [len(rows)]; rows.append(('codec', ''))
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
    borrowed_drawing = json.loads((ROOT / 'benchmarks/borrowed-drawing-validation-evidence.json').read_text(encoding='utf-8'))
    bq = borrowed_drawing['qualification']
    assert borrowed_drawing['productionAccepted'] and borrowed_drawing['allAffectedPerformanceReconciled']
    assert borrowed_drawing['requiredMaterialPaths'] == 289 and borrowed_drawing['newMaterialPaths'] == 0
    assert borrowed_drawing['raw']['mirrored'] and borrowed_drawing['raw']['allMembersVerified']
    assert bq['fullChecksEachOs'] == bq['windows']['total'] == bq['linux']['total'] == 6659
    assert bq['actualPackage']['sha256'] == bq['samePackageWindows']['sameActualPackageSha256']
    assert bq['actualPackage']['actualConsumerPassed'] and bq['samePackageWindows']['actualConsumerPassed']
    assert bq['actualPackage']['usageExamples'] == bq['samePackageWindows']['usageExamples'] == 3
    assert bq['acceptedArtifactRebuildsAfterTiming'] == 0 and bq['allAffectedPerformanceReconciledBeforeBroadQualification']
    assert borrowed_drawing['sourceProof']['allConfirmed'] and borrowed_drawing['sourceProof']['sharedOperationContextAndMemoryBudgetUnchanged']
    drawing_cases = dict(borrowed_drawing['cases'])
    assert len(drawing_cases) == 7
    assert borrowed_drawing['statusCounts'] == {state: sum(c['status'] == state for c in drawing_cases.values()) for state in ('pass', 'gap', 'uncertain')}
    assert borrowed_drawing['memory']['newOwnedHeapCapacityBytes'] == borrowed_drawing['memory']['newAdmittedBytes'] == borrowed_drawing['memory']['retainedOperationBytes'] == 0
    copy_current = borrowed_copy_stage()
    drawing_cases.update((name, case) for name, case in copy_current['cases'].items() if name != 'OriginalClippedCopy8')
    assert len(drawing_cases) == 8
    word_composite = original_word_composite_stage(copy_current)
    drawing_cases['OriginalWordComposite'] = word_composite['case']
    drawing_counts = {field: sum(c['status'] == state for c in drawing_cases.values())
                      for field, state in (('passCount', 'pass'), ('gaps', 'gap'), ('uncertain', 'uncertain'))}
    drawing_counts['total'] = len(drawing_cases)
    assert drawing_counts['total'] == 9
    drawing_labels = {
        'DrawingCopy8': 'Exact rectangle copy into borrowed RGBA8 canvas',
        'DrawingCopy16': 'Exact rectangle copy into borrowed RGBA16 canvas',
        'DrawingFill8': 'Exact clipped fill into borrowed RGBA8 canvas',
        'DrawingFill16': 'Exact clipped fill into borrowed RGBA16 canvas',
        'DrawingCopyOverlap8': 'Overlapping RGBA8 copy + reverse restoration, per pair',
        'OriginalWordOverlap': 'Original overlapping RGBA16 copy, 257×129, shift (1,1), per pair',
        'DrawingComposite8': 'RGBA8 source-over / FFmpeg overlay',
        'DrawingComposite16': 'RGBA16 source-over / ImageSharp 3.1.12',
        'OriginalWordComposite': 'Original self-composite RGBA16 / ImageSharp snapshot + SrcOver',
    }
    for name, label in drawing_labels.items():
        c = drawing_cases[name]
        library = (copy_current['caseProducerSha256'][name] if name in copy_current['cases'] else
                   word_composite['producers']['linuxSha256'] if name == 'OriginalWordComposite' else
                   bq['actualPackage']['librarySha256'])
        assert c['librarySha256'] == library
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12 and c['allActualsRetained']
        assert c['allocatedBytes'] == 0 and c['managedExact'] and c['borrowedCanvas'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs']/c['nativeUs'], rel_tol=1e-12)
        if name in {'DrawingCopyOverlap8', 'OriginalWordOverlap'}:
            assert c['sourceAliasesCanvas'] and c['publicMovesPerPair'] == 2 and c['restorationTimed']
            assert c['nativeCallsPerPair'] == (258 if name == 'OriginalWordOverlap' else 6)
        elif name == 'OriginalWordComposite':
            assert c['sourceAliasesCanvas'] and c['snapshotCreationAndDisposalTimed']
        else:
            assert c['sourceUnchanged']
        status = 'parity confirmed' if c['status'] == 'pass' else 'gap' if c['status'] == 'gap' else 'confidence open'
        mark = ' †' if name.startswith('DrawingComposite') or name == 'OriginalWordComposite' else ''
        rows.append(('editing', f'| {label} ({status}){mark} | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | 0 |'))
    b8, bw, bo = (drawing_cases[n] for n in ('DrawingCopy8', 'DrawingCopy16', 'DrawingCopyOverlap8'))
    word_overlap = drawing_cases['OriginalWordOverlap']
    borrowed_drawing_note = f"""[Shared self-row storage](../benchmarks/SELF-ROW-STORAGE.md) saves
**{copy_current['pairedEffect']['gainPercent']:.2f}% / {copy_current['pairedEffect']['savedUs']:.3f} µs** in controlled complete word pairs.
One shared bounded backing and row step replace duplicate execution state; copying,
snapshot order, cancellation and ownership remain exact. Current original word
cost **{word_overlap['managedUs']:.3f} / FFmpeg {word_overlap['nativeUs']:.3f} µs**, upper
**{word_overlap['upper']:.6f}**, passes/stops. This is a scoped final-DLL gain;
historical clocks and prototype effects are not added.

[Complete copy-operation boundaries](../benchmarks/COPY-OPERATION-BOUNDARY.md)
now include native dynamic clipping and address preparation every operation.
The old fixed adapter prevalidated geometry; compiler versioning hoisted
preparation in the extended clipped adapter. The explicit authored-wrapper
correction preserves FFmpeg, inputs, settings and exact output. Old figures
remain prepared-native diagnostics, without a production speedup claim.

Separate byte is **{b8['managedUs']:.3f} / {b8['nativeUs']:.3f} µs**, upper
**{b8['upper']:.6f}**, a gap; clipped byte also remains a gap. Separate word is
**{bw['managedUs']:.3f} / {bw['nativeUs']:.3f} µs**, interval
**{bw['lower']:.6f}–{bw['upper']:.6f}**, uncertain. All twelve units/group, original
65,536/262,144 operations, case-specific drivers, subdivisions, warmups and
conservative gates stay. No favorable replay or assumed word optimization need.
Byte overlap **{bo['managedUs']:.3f} / {bo['nativeUs']:.3f} µs**, upper
**{bo['upper']:.6f}**, passes/stops. Both self-copy passes retain identified
source-valid shared-self-row evidence; the three corrected comparisons use the
actual pending-publication package. Other primitives keep their own producers.

Copies allocate **0 B/op**, with no new owned/admitted/retained workspace.
Caller storage and unmeasured physical/native/JIT peaks stay separate. All three
corrected normal/scalar preflights and original full-frame/source/owner gates
pass. One authored native-worker build; zero production/harness/driver/archive/
fixture/package builds. Unchanged production reuses **6,844 full/live checks each
OS**, the SAME actual package and all three examples. Superseded/rejected
experiments remain linked history, outside current renderer dependencies.
"""
    wc = word_composite['case']
    borrowed_drawing_note += f"""
[Original RGBA16 self-compositing](../benchmarks/ORIGINAL-WORD-COMPOSITE.md)
passes/stops at **{wc['managedUs']:.3f} / ImageSharp {wc['nativeUs']:.3f} µs**, upper
**{wc['upper']:.6f}**, **0 / {wc['referenceAllocatedBytes']:,.0f} GC B/op**. The reference is explicitly
ImageSharp 3.1.12 Rgba64 **source Clone + SrcOver + Dispose**, with the necessary
snapshot timed; reset is excluded before EVERY operation on both sides. Exact
managed/full-word and reference max1/RMSE .25, untouched/zero-alpha gates pass.
Snapshot payload is at least 1,062,936 B, possibly pooled; retained pool and physical
peaks are unmeasured. No broader content/settings or historical-clock gain claim.
"""
    text_current = json.loads((ROOT / 'benchmarks/text-drawing-current-evidence.json').read_text(encoding='utf-8'))
    assert text_current['allNinePreviouslyMissingMeasured'] and text_current['requiredMaterialPaths'] == 282
    text_band = text_scan_band_stage()
    word_interleaving = ordinary_word_interleaving(text_band)
    ordinary_text = {**text_band, 'cases': {**text_band['cases'], 'Ordinary16': word_interleaving['case']}}
    font_current = json.loads((ROOT / 'benchmarks/font-mapping-group-validation-evidence.json').read_text(encoding='utf-8'))
    assert font_current['productionAccepted'] and font_current['allAffectedPerformanceReconciled']
    assert font_current['requiredMaterialPaths'] == 289 and font_current['newMaterialPaths'] == 0
    assert font_current['raw']['mirrored'] and font_current['raw']['allMembersVerified']
    hq = font_current['qualification']
    assert hq['windows']['total'] == hq['linux']['total'] == hq['fullChecksEachOs'] == 6839
    assert hq['focusedEachMode'] == {'internal': 322, 'public': 222}
    assert hq['allAffectedPerformanceReconciledBeforeBroad'] and hq['productionRebuildsAfterTiming'] == 0
    assert hq['linux']['actualPackage']['sha256'] == hq['windowsPackage']['sameActualPackageSha256'] == font_current['producers']['packageSha256']
    assert hq['linux']['actualPackage']['librarySha256'] == font_current['producers']['timedArtifactSha256']
    assert hq['linux']['actualPackage']['actualConsumerPassed'] and hq['windowsPackage']['actualConsumerPassed']
    assert hq['linux']['actualPackage']['usageExamples'] == hq['windowsPackage']['usageExamples'] == 3
    assert all(font_current['sourceProof'].values())
    font_cost = font_current['font']
    assert font_cost['actualIterations'] == font_cost['nativeSamplesEachBracket'] == 12
    assert font_cost['allOriginalGatesPassed'] and font_cost['quietGuardsPassed'] and font_cost['allActualsRetained']
    assert font_cost['allocatedBytes'] == 176
    assert font_cost['status'] == ('pass' if font_cost['upper'] <= 1 else 'gap' if font_cost['lower'] > 1 else 'uncertain')
    assert math.isclose(font_cost['ratio'], font_cost['managedUs'] / font_cost['nativeUs'], rel_tol=1e-12)
    text_coverage = json.loads((ROOT / 'benchmarks/text-prepared-coverage-evidence.json').read_text(encoding='utf-8'))
    assert text_coverage['productionAccepted'] and text_coverage['allAffectedPerformanceReconciled']
    assert text_coverage['requiredMaterialPaths'] == 289 and text_coverage['newMaterialPaths'] == 0
    assert text_coverage['raw']['mirrored'] and text_coverage['raw']['allMembersVerified']
    tq = text_coverage['qualification']
    assert tq['windows']['total'] == tq['linux']['total'] == tq['fullChecksEachOs'] == 6730
    assert tq['package']['sha256'] == tq['windowsPackage']['sameActualPackageSha256'] == text_coverage['producers']['packageSha256']
    assert tq['package']['librarySha256'] == text_coverage['producers']['timedArtifactSha256']
    assert all(tq[k]['actualConsumerPassed'] and tq[k]['usageExamples'] == 3 for k in ('package', 'windowsPackage'))
    assert tq['allAffectedPerformanceReconciledBeforeBroadQualification'] and tq['acceptedArtifactRebuildsAfterTiming'] == 0
    assert tq['focusedEachMode'] == {'internal': 266, 'public': 163} and tq['modes'] == ['normal', 'avx-off', 'scalar']
    assert tq['allEightFrozenFramesEachMode'] and tq['allEightFrozenFramesBothOs'] and all(text_coverage['sourceProof'].values())
    assert not text_coverage['arabicGeneralization']['gainConfirmed'] and not text_coverage['arabicGeneralization']['regressionConfirmed']
    memory = text_coverage['memory']
    assert memory['temporaryGeometryReleasedBeforePublication'] and memory['newStaticCacheBytes'] == memory['newIdlePoolBytes'] == 0
    assert memory['completeGovernedPeakBytes'] == 822460 and memory['completeFitRetainedCapacityBytes'] == 4160
    assert memory['latinRetainedCapacityBytes'] == 2112
    assert memory['caseCapacities']['NarrowLongWord']['newRetainedCapacity'] > memory['caseCapacities']['NarrowLongWord']['oldRetainedCapacity']
    for case, label in [('OneShot', 'Complete text one-shot / ImageSharp public integration'),
                        ('PrepareDraw8', 'Complete text preparation + eight draws / ImageSharp public integration')]:
        cost = font_current['pipelineCases'][case]
        assert cost['status'] == 'pass' and len(cost['pairs']) == len(cost['referenceBefore']) == len(cost['referenceAfter']) == 12
        assert math.isclose(cost['ratio'], cost['managedUs'] / cost['referenceUs'], rel_tol=1e-12)
        rows.append(('editing', f'| {label} (pass) † | {cost["managedUs"]:,.2f} | {cost["referenceUs"]:,.2f} | '
                     f'{cost["ratio"]:.3f} / {cost["upper"]:.3f} | {cost["managedAllocatedBytes"]:,.0f} |'))
    font_preparation = font_current['pipelineCases']['Preparation']
    font_batch = font_current['pipelineCases']['PrepareDraw8']
    assert math.isclose(font_batch['amortizedManagedUs'], font_batch['managedUs'] / 8, rel_tol=1e-12)
    assert set(text_coverage['cases']) == {'Ordinary8', 'Ordinary16', 'Prepared8', 'Prepared16'}
    for name, case in text_coverage['cases'].items():
        prepared = name.startswith('Prepared')
        if not prepared: case = ordinary_text['cases'][name]
        assert case['librarySha256'] == (text_coverage if prepared else ordinary_text)['producers']['linuxSha256']
        assert case['managedExact'] and case['allOriginalGatesPassed'] and case['quietGuardsPassed'] and case['allActualsRetained']
        assert case['actuals'] == 12 and case['operationsPerActual'] == (1024 if prepared else 16384)
        assert case['status'] == ('pass' if case['upper'] <= 1 else 'gap' if case['lower'] > 1 else 'uncertain')
        assert not prepared or case['status'] == 'pass'
        assert math.isclose(case['ratio'], case['managedUs'] / case['referenceUs'], rel_tol=1e-12)
        assert case['allocatedBytes'] == (56 if prepared else 83008) and case['adapterAllocatedBytes'] == case['allocatedBytes'] + 24
        assert case['retainedBytes'] == (2112 if prepared else 0)
        status = 'confidence open' if case['status'] == 'uncertain' else case['status']
        label = f'Text {"prepared coverage" if prepared else "ordinary draw"} RGBA{case["depth"]} / ImageSharp.Drawing ({status})'
        rows.append(('editing', f'| {label} † | {case["managedUs"]:,.2f} | {case["referenceUs"]:,.2f} | '
                     f'{case["ratio"]:.3f} / {case["upper"]:.3f} | {case["allocatedBytes"]:,} |'))
    rows.append(('editing', f'| Font loading / FreeType 2.13.2 (scoped {font_cost["status"]}) † | {font_cost["managedUs"]:,.2f} | '
                 f'{font_cost["nativeUs"]:,.2f} | {font_cost["ratio"]:.3f} / {font_cost["upper"]:.3f} | {font_cost["allocatedBytes"]:,} |'))
    for case_id, previous in text_current['observations'].items():
        if case_id in ('DrawText8', 'DrawText16', 'DrawingComposite8', 'DrawingComposite16', 'FontLoadRegular'):
            continue  # Current ordinary/prepared text and borrowed composites above.
        case = previous
        assert len(case['managed']['actual']) == 12 and case['quietGuardsPassed']
        assert case['approvedScopeDate'] == '2026-11-02'
        native_mean = min(bracket['mean'] for bracket in case['nativeBrackets'])
        status = 'pass' if case['status'] == 'Confirmed pass' else 'gap' if case['status'] == 'Confirmed gap' else 'confidence open'
        mark = ' †' if case_id == 'FontLoadRegular' else ''
        rows.append(('editing', f'| {case["label"]} ({status}){mark} | {case["managed"]["statistics"]["Mean"] / 1000:,.2f} | '
                     f'{native_mean / 1000:,.2f} | {case["worstBracketRatio"]:.3f} / {case["conservativeRatioUpper"]:.3f} | '
                     f'{case["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
    lanczos_current = json.loads((ROOT / 'benchmarks/lanczos-identity-axis-evidence.json').read_text(encoding='utf-8'))
    lanczos_costs = lanczos_current['cases']
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
    variable_prices = json.loads((ROOT / 'benchmarks/vp8-variable-price-preparation-evidence.json').read_text(encoding='utf-8'))
    assert variable_prices['productionAccepted'] and variable_prices['allAffectedPerformanceReconciled']
    assert variable_prices['requiredMaterialPaths'] == 289 and not variable_prices['materialPassToGapContradictions']
    assert variable_prices['raw']['mirrored'] and variable_prices['raw']['allMembersVerified']
    policy_q = variable_prices['qualification']
    assert policy_q['fullChecksEachOs'] == policy_q['windows']['total'] == policy_q['linux']['total'] == 6686
    assert policy_q['windows']['fullLivePassed'] and policy_q['linux']['fullLivePassed']
    assert policy_q['sameActualPackageBothOS'] and policy_q['allThreeExamplesBothOS']
    assert policy_q['actualPackage']['sha256'] == policy_q['samePackageWindows']['sameActualPackageSha256']
    assert variable_prices['workflow']['postTimingProductionRebuilds'] == variable_prices['workflow']['measurementBuilds'] == 0
    assert variable_prices['storage']['newOwnedHeapCapacityBytes'] == variable_prices['storage']['newRetainedStateBytes'] == 0
    vp8_encode_cases = variable_prices['cases']
    assert set(vp8_encode_cases) == {'opaque8', 'opaque16', 'photograph', 'diagram', 'selected', 'trellis', 'distortion', 'alpha', 'alpha16'}
    for c in vp8_encode_cases.values():
        assert c['librarySha256'] == variable_prices['producers']['linuxSha256'] == policy_q['actualPackage']['librarySha256']
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == len(c['managedActualNanoseconds']) == 12
        assert c['allActualsRetained'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['completePacketExact'] and c['packetSha256'] == c['nativePacketSha256']
        assert c['managedPacketBytes'] == c['nativePacketBytes'] and c['sourceUnchanged'] and c['independentOwnedOutput']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    assert all(c['gainSeparated'] and c['pairs'] == 12 and c['operations'] == 32 for c in variable_prices['paired'].values())
    for name in ['alpha', 'alpha16']:
        webp_owned['cases'][name] = vp8_encode_cases[name]
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
    assert opaque_evidence['requiredMaterialPaths'] == 289 and opaque_evidence['workflow']['driverBuilds'] == 0
    assert opaque_evidence['workflow']['longerFixedBatchOnly'] and opaque_evidence['workflow']['productionEmbeddedPublicHarnessNativeFixtureOrPackageBuilds'] == 0
    assert opaque_evidence['raw']['mirrored'] and opaque_evidence['raw']['allMembersVerified']
    assert opaque['status'] == 'pass' and opaque['upper'] <= 1
    assert opaque['actualIterations'] == opaque['nativeSamplesEachBracket'] == 12 and opaque['pairedOperationsPerSubject'] == 16
    assert opaque['allActualsRetained'] and opaque['allOriginalGatesPassed'] and opaque['quietGuardsPassed']
    assert opaque['managedExact'] and opaque['completePacketExact'] and opaque['independentOwnedOutput']
    assert opaque['managedPacketBytes'] == opaque['nativePacketBytes'] == 121896
    assert opaque['packetSha256'] == opaque['nativePacketSha256']
    assert opaque['librarySha256'] == opaque_evidence['producers']['linux'] == opaque_evidence['qualificationReused']['actualPackage']['librarySha256']
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
    assert lossless_status_counts == {'pass': 8, 'gap': 0, 'uncertain': 0}
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
    vp8_rows = []
    for case, label in vp8_labels.items():
        c = vp8_encode_cases[case]
        assert not any(row.split('|')[1].strip().removesuffix(' H').removesuffix(' †') == label for _, row in rows)
        vp8_rows.append(('codec', f'| {label} † | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | '
            f'{c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | '
            f'{c["managedPacketBytes"]:,} / {c["nativePacketBytes"]:,} |'))
    rows[position:position] = vp8_rows
    vp8_status_counts = {state: sum(c['status'] == state for c in vp8_encode_cases.values())
                         for state in ('pass', 'gap', 'uncertain')}
    assert vp8_status_counts == {'pass': 9, 'gap': 0, 'uncertain': 0}
    selected_cost = vp8_encode_cases['selected']
    distortion_cost = vp8_encode_cases['distortion']
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
    assert png_gray_current['qualification']['actualPackage'] == {
        k: policy_q['actualPackage'][k] for k in ('sha256', 'librarySha256', 'bytes')}
    assert png_gray_current['qualification']['fullChecksEachOs'] == policy_q['fullChecksEachOs']
    assert set(png_gray_current['cases']) == {'EncodeGray8'}
    assert png_gray_current['cases']['EncodeGray8']['operationsPerSubject'] == png_gray_current['integration']['operationsPerSubject']
    png_cases = {**png_current['cases'], **png_gray_current['cases']}
    assert len(png_cases) == 20
    png_rgb16 = png_cases['EncodeRgb16']; png_gray8 = png_cases['EncodeGray8']
    assert png_rgb16['status'] == 'pass' and png_gray8['status'] == 'pass'
    png_oneshot_status = {status: sum(c['status'] == status and c['boundaryKind'] == 'equivalent-one-shot' for c in png_cases.values()) for status in ('pass', 'gap', 'uncertain')}
    assert png_oneshot_status == png_gray_current['oneShotStatusCounts'] == {'pass': 17, 'gap': 0, 'uncertain': 0}
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
    inspection_owner = json.loads((ROOT / 'benchmarks/probe-result-publication-evidence.json').read_text(encoding='utf-8'))
    sq = inspection_owner['qualification']
    assert inspection_owner['productionAccepted'] and inspection_owner['allAffectedPerformanceReconciled']
    assert inspection_owner['requiredMaterialPaths'] == 289 and inspection_owner['newMaterialPaths'] == 0
    assert inspection_owner['raw']['mirrored'] and inspection_owner['raw']['allMembersVerified']
    assert sq['fullChecksEachOs'] == sq['windows']['total'] == sq['linux']['total'] == 6712
    assert sq['sameActualPackageBothOs'] and sq['allThreeUsageExamplesBothOs']
    assert sq['package']['sha256'] == sq['windowsPackage']['sameActualPackageSha256']
    assert sq['package']['librarySha256'] == inspection_owner['producers']['linuxSha256']
    assert sq['acceptedArtifactRebuildsAfterTiming'] == 0 and sq['allAffectedPerformanceReconciledBeforeBroadQualification']
    probe_owner = stream_reader_stage()
    jpeg_markers = json.loads((ROOT / 'benchmarks/jpeg-stuffed-pairs-evidence.json').read_text(encoding='utf-8'))
    jq = jpeg_markers['qualification']
    assert jpeg_markers['productionAccepted'] and jpeg_markers['allAffectedPerformanceReconciled']
    assert jpeg_markers['requiredMaterialPaths'] == 289 and jpeg_markers['newRequiredPaths'] == 0
    assert jq['windows']['total'] == jq['linux']['total'] == jq['fullChecksEachOs'] == 6747
    assert jq['sameActualPackageBothOs'] and jq['allThreeUsageExamplesBothOs']
    assert jq['package']['sha256'] == jq['windowsPackage']['sameActualPackageSha256']
    assert jq['package']['librarySha256'] == jpeg_markers['producers']['linuxSha256']
    assert jq['allAffectedPerformanceReconciledBeforeBroadQualification'] and jq['acceptedArtifactRebuildsAfterTiming'] == 0
    assert jq['focusedEachMode'] == {'internal': 10, 'public': 156}
    assert jq['originalAndCandidateNewGuardsLinuxNormalScalar'] and jq['sameTimedPackageHeadersWindowsNormalScalar']
    assert jpeg_markers['raw']['mirrored'] and jpeg_markers['raw']['allMembersVerified']
    assert all(jpeg_markers['sourceProof'].values())
    jpeg_pending = jpeg_pending_publication_stage()
    probe_owner['cases'].update({name: c for name, c in jpeg_pending['cases'].items() if name.startswith('Probe')})
    probe_integration = jpeg_probe_longer_integration_stage(jpeg_pending)
    current_async_probe = probe_integration['case']
    probe_owner['cases']['ProbeDimensionsJpegAsync'] = current_async_probe
    for label, (case, observations) in zip(read_labels, reads['cases'].items(), strict=True):
        provider = probe_owner
        if case in provider['cases']:
            current = provider['cases'][case]
            assert current['allOriginalGatesPassed'] and current['quietGuardsPassed'] and current['allActualsRetained']
            # stream_reader_stage validates each current or source-valid reused producer.
            status = 'Confirmed pass' if current['status'] == 'pass' else 'Confirmed gap' if current['status'] == 'gap' else 'Confidence open'
            read_rows.append(f'| {label} | {current["managedUs"]:,.3f} | {current["nativeUs"]:,.3f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} | {status} |')
            continue
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
    headers = probe_owner['cases']['PngHeaders']
    assert headers['descriptorExact'] and headers['allOriginalGatesPassed'] and headers['quietGuardsPassed']
    # The unchanged PNG-header path retains its separately pinned prior producer.
    assert headers['managedActuals'] == headers['nativeSamplesEachBracket'] == 12 and headers['upper'] <= 1
    read_rows.append(f'| Inspect PNG8 complete headers, seekable, adapter reference | '
                     f'{headers["managedUs"]:,.3f} | {headers["nativeUs"]:,.3f} | '
                     f'{headers["ratio"]:.3f} / {headers["upper"]:.3f} | '
                     f'{headers["allocatedBytes"]:,} | Confirmed pass |')
    identify_labels = {
        'Png8': 'Identify PNG8 complete container, resident, adapter reference',
        'Jpeg8': 'Identify JPEG420, resident, canonical adapter',
        'Webp8': 'Identify VP8L+alpha, resident, parser adapter',
        'PngSeekable': 'Identify PNG8, fresh seekable Stream, adapter reference',
        'PngAsync': 'Identify PNG8, async nonseekable, adapter reference',
    }
    assert set(inspection_owner['identifyCases']) == set(identify_labels)
    resident_png = png_scan_stage()
    resident_names = {'Png8': 'identify/Png8', 'PngSeekable': 'streamIdentify/PngSeekable', 'PngAsync': 'streamIdentify/PngAsync'}
    for name, label in identify_labels.items():
        provider = resident_png if name in resident_names else inspection_owner
        if name in resident_names:
            source = resident_png['cases'][resident_names[name]]
            c = {**source, **source['metadata'], 'managedActuals': source['actualIterations']}
        else:
            c = inspection_owner['identifyCases'][name]
        assert c['librarySha256'] == provider['producers']['linuxSha256']
        assert c['managedActuals'] == c['nativeSamplesEachBracket'] == 12
        assert c['allActualsRetained'] and c['allOriginalGatesPassed'] and c['quietGuardsPassed']
        assert c['descriptorExact'] and c['independentOwnedOutput']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        status = 'Confirmed pass' if c['status'] == 'pass' else 'Confirmed gap' if c['status'] == 'gap' else 'Confidence open'
        read_rows.append(f'| {label} | {c["managedUs"]:,.3f} | {c["nativeUs"]:,.3f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | {status} |')
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
    for name, prefix in [('ValidatePng16Async', '| Validate PNG16, async nonseekable')]:
        c = direct_inputs['cases'][name]
        indices = [i for i, line in enumerate(read_rows) if line.startswith(prefix)]
        assert len(indices) == 1
        i = indices[0]; label = read_rows[i].split('|')[1].strip()
        status = 'Confirmed pass' if c['status'] == 'pass' else 'Confirmed gap' if c['status'] == 'gap' else 'Confidence open'
        read_rows[i] = f'| {label} | {c["managedUs"]:,.3f} | {c["nativeUs"]:,.3f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | {status} |'

    codecs, editing, read_rows, storage_current = apply_released_image_storage(codecs, editing, read_rows)
    codecs, read_rows = apply_current_jpeg_preparation(codecs, read_rows)
    codecs, read_rows = apply_current_prepared_input(codecs, read_rows)
    editing = apply_current_lanczos(editing)
    editing, png_composition_one_shot = append_one_shot_png_composition(editing)
    editing = apply_mixed_word_cache(editing)
    editing = apply_bounded_wide_quartet(editing)
    owned_copy = owned_edit_costs(resident_png)
    for name, label in [('WarmTranspose16', 'Plain transpose RGBA16 † → 259×513'),
                        ('WarmTranspose8', 'Plain transpose RGBA8 † → 259×513'),
                        ('WarmCounterclockwise8', 'Counterclockwise quarter turn RGBA8 † → 259×513'),
                        ('WarmCounterclockwise16', 'Counterclockwise quarter turn RGBA16 † → 259×513'),
                        ('WarmTransverse8', 'Mirrored transpose RGBA8 † → 259×513'),
                        ('WarmHalfTurn8', 'Half turn RGBA8 †'),
                        ('WarmVerticalFlip8', 'Vertical flip RGBA8 (parity confirmed) †')]:
        c = owned_copy['cases'][name]
        editing += f'\n| {label} | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} |'
    byte_flip, word_flip = (owned_copy['cases'][name] for name in ('WarmVerticalFlip8', 'WarmVerticalFlip16'))
    flip_gains = owned_flip_reference_stage()['pairedEffects']
    owned_copy_note = f"""[Prepared vertical rows](../benchmarks/VERTICAL-PREPARED-ROWS.md)
close both original owned-flip controls: **{byte_flip['managedUs']:.3f} / FFmpeg {byte_flip['nativeUs']:.3f} µs,
upper {byte_flip['upper']:.6f}** for RGBA8 and **{word_flip['managedUs']:.3f} /
{word_flip['nativeUs']:.3f} µs, upper {word_flip['upper']:.6f}** for RGBA16, **56 B/op** each.
Both pass/stop. Reuse the existing bounded-row helper with opposite steps,
preparing bulk/tail geometry once; no new kernel, workspace, pool, pin or API.
Final-DLL controlled pairs save **{flip_gains['WarmVerticalFlip8']['savedUs']:.3f} µs /
{flip_gains['WarmVerticalFlip8']['gainPercent']:.2f}%** and **{flip_gains['WarmVerticalFlip16']['savedUs']:.3f} µs /
{flip_gains['WarmVerticalFlip16']['gainPercent']:.2f}%**. Private/historical effects are not added.
Complete public validation/admission, independent output materialization and disposal
stay timed; FFmpeg retains graph/source/frame wrappers. Every original full sample,
owner and guard passes, with all12actuals/three interleaved native groups retained.
Windows/Linux each pass **6,823 full/live checks**, focused normal/AVX-512-disabled/
scalar checks and the **SAME actual package/all three examples**. The timed DLL is
never rebuilt. Actual result ownership, unchanged four-array/4 MiB idle storage,
zero added scratch and unmeasured physical/native/JIT peaks remain distinct.
Other copy, font/text, codec and resizing paths retain identified source-valid
costs. The complete final-package refresh remains outstanding."""
    read_table = '\n'.join([header + ' Status |', separator + ' --- |', *read_rows])
    png_stream_word_note = current_png_stream_word_note(resident_png, png_composition_one_shot)
    codecs, read_table = apply_current_vp8_fixed_code(codecs, read_table, reader_current)
    editing, bicubic_adjacent_note, streaming_current = apply_current_bicubic_adjacent_confidence(editing)
    opaque_live = reader_current['cases']['OpaqueCancelable8']
    bilinear_word_current = json.loads((ROOT / 'benchmarks/bilinear-partitioned-reference-evidence.json').read_text(encoding='utf-8'))
    bilinear_word_case = bilinear_word_current['case']
    assert bilinear_word_current['measurementAccepted'] and not bilinear_word_current['productionChanged'] and not bilinear_word_current['productionSpeedupClaim']
    assert bilinear_word_current['builds']['productionEmbeddedPublicHarnessNativeFixturePackage'] == bilinear_word_current['qualificationReused']['newBroadPackageChecks'] == 0
    assert bilinear_word_current['qualificationReused']['fullLiveEachOs'] == 6823
    assert all(bilinear_word_current['sourceProof'].values())
    assert bilinear_word_current['producers'] == owned_flip_reference_stage()['producers']
    assert bilinear_word_current['measurement']['referenceGroupsInterleaved'] and bilinear_word_current['measurement']['operationsPerSubject'] == 256
    bm = bilinear_word_current['measurement']
    assert bm['statisticalUnitsPerGroup'] == bm['actualPairs'] == 12
    assert bm['segmentsPerUnit'] * bm['operationsPerSegment'] == bm['operationsPerSubject']
    assert bm['allSubdivisionCountsAndAggregationVerified'] and all(s['N'] == 12 for s in [bm['managedStatistics'], *bm['nativeStatistics']])
    assert bilinear_word_current['raw']['mirrored'] and bilinear_word_current['raw']['allMembersVerified']
    assert bilinear_word_case['librarySha256'] == bilinear_word_current['producers']['linuxSha256']
    assert bilinear_word_case['allActualsRetained'] and bilinear_word_case['allOriginalGatesPassed'] and bilinear_word_case['quietGuardsPassed']
    assert bilinear_word_case['managedRationalReferenceExact'] and bilinear_word_case['sourceUnchanged'] and bilinear_word_case['independentOwnedOutput']
    assert bilinear_word_case['upper'] <= 1 and bilinear_word_case['status'] == 'pass'
    assert math.isclose(bilinear_word_case['ratio'], bilinear_word_case['managedUs'] / bilinear_word_case['nativeUs'], rel_tol=1e-12)
    assert bilinear_word_case['metadata']['nativeRgbMaximum'] <= 1 and bilinear_word_case['metadata']['nativeAlphaMaximum'] <= 1
    assert bilinear_word_case['metadata']['nativeRgbRmse'] <= .25 and bilinear_word_case['metadata']['nativeAlphaRmse'] <= .25
    anchor = next(line for line in editing.splitlines() if line.startswith('| Bilinear upscale RGBA8'))
    bilinear_word_row = (f'| Bilinear RGBA16 enlargement, fresh managed / prepared FFmpeg graph (parity confirmed) † → 1027×519 | '
                         f'{bilinear_word_case["managedUs"]:,.2f} | {bilinear_word_case["nativeUs"]:,.2f} | '
                         f'{bilinear_word_case["ratio"]:.3f} / {bilinear_word_case["upper"]:.6f} | {bilinear_word_case["allocatedBytes"]:,} |')
    editing = editing.replace(anchor, anchor + '\n' + bilinear_word_row)
    bilinear_one_shot = json.loads((ROOT / 'benchmarks/bilinear-equivalent-one-shot-evidence.json').read_text(encoding='utf-8'))
    bilinear_fresh_case = bilinear_one_shot['case']
    fresh_q = json.loads((ROOT / bilinear_one_shot['qualification']['provider']).read_text(encoding='utf-8'))
    assert bilinear_one_shot['measurementAccepted'] and not bilinear_one_shot['productionChanged'] and not bilinear_one_shot['productionSpeedupClaim']
    assert bilinear_one_shot['requiredMaterialPaths'] == 289 and bilinear_one_shot['newMaterialPaths'] == 0
    assert bilinear_one_shot['raw']['mirrored'] and bilinear_one_shot['raw']['allMembersVerified']
    assert bilinear_one_shot['producers'] == fresh_q['producers']
    assert bilinear_fresh_case['librarySha256'] == fresh_q['qualification']['actualPackage']['librarySha256']
    assert bilinear_fresh_case['actualIterations'] == bilinear_fresh_case['nativeSamplesEachBracket'] == 12
    assert all(bilinear_fresh_case[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed', 'managedRationalReferenceExact', 'sourceUnchanged', 'independentOwnedOutput'))
    assert bilinear_fresh_case['status'] == 'pass' and bilinear_fresh_case['upper'] <= 1
    assert math.isclose(bilinear_fresh_case['ratio'], bilinear_fresh_case['managedUs'] / bilinear_fresh_case['nativeUs'], rel_tol=1e-12)
    assert bilinear_fresh_case['nativeReady']['graphPreparationTimed'] and not bilinear_fresh_case['nativeReady']['graphRetainedBetweenOperations']
    assert bilinear_fresh_case['metadata'] == bilinear_word_case['metadata']
    fresh_row = (f'| Bilinear one-shot RGBA16 / FFmpeg one-shot † → 1027×519 | '
                 f'{bilinear_fresh_case["managedUs"]:,.2f} | {bilinear_fresh_case["nativeUs"]:,.2f} | '
                 f'{bilinear_fresh_case["ratio"]:.3f} / {bilinear_fresh_case["upper"]:.3f} | {bilinear_fresh_case["allocatedBytes"]:,} |')
    editing = editing.replace(bilinear_word_row, bilinear_word_row + '\n' + fresh_row)
    codecs, read_table = apply_current_png_scan(codecs, read_table, resident_png)
    header_reference = jpeg_markers
    hq = header_reference['qualification']
    assert header_reference['productionChanged'] and header_reference['newRequiredPaths'] == 0
    assert hq['fullChecksEachOs'] == 6747 and hq['package']['sha256'] == header_reference['producers']['packageSha256']
    assert header_reference['raw']['mirrored'] and header_reference['raw']['allMembersVerified']
    entropy = json.loads((ROOT / 'benchmarks/jpeg-batched-entropy-evidence.json').read_text(encoding='utf-8'))
    eq = entropy['qualification']
    assert entropy['productionAccepted'] and entropy['allAffectedPerformanceReconciled']
    assert entropy['requiredMaterialPaths'] == 289 and entropy['newRequiredPaths'] == 0
    assert eq['fullChecksEachOs'] == 6749 and eq['sameActualPackageBothOs']
    assert eq['package']['sha256'] == entropy['producers']['packageSha256']
    assert eq['package']['librarySha256'] == entropy['producers']['linuxSha256']
    assert eq['package']['acceptedArtifactRebuildsAfterTiming'] == 0
    assert entropy['raw']['mirrored'] and entropy['raw']['allMembersVerified']
    assert all(entropy['sourceProof'].values())
    assert entropy['reference']['nativeSha256'] == header_reference['reference']['nativeSha256']
    ep = entropy['incremental']
    assert ep['upper'] < 1 and ep['allActualsRetained']
    assert ep['baselineAllocatedBytes'] == ep['candidateAllocatedBytes'] == 8776
    assert entropy['case']['status'] == 'pass' and entropy['case']['allocatedBytes'] == 8768
    for case, label in [('JpegHeaders', 'Complete JPEG headers, fresh source / FFmpeg AVIO adapter'),
                        ('WebpHeaders', 'Complete WebP headers, fresh seekable source / FFmpeg AVIO adapter')]:
        c = jpeg_pending['cases'][case] if case == 'JpegHeaders' else header_reference['cases'][case]
        producer = jpeg_pending if case == 'JpegHeaders' else header_reference
        assert c['librarySha256'] == producer['producers']['linuxSha256']
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12
        assert all(c[k] for k in ('allActualsRetained', 'quietGuardsPassed', 'descriptorExact', 'eofChecked',
                                 'independentSurvivingOutputs', 'inputUnchanged'))
        assert c['nativeReady']['freshSeekableStream'] and not c['nativeReady']['inputView'] and not c['nativeReady']['wholeInputCopy']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
        read_table += (f'\n| {label} ({c["status"]}) | {c["managedUs"]:.3f} | {c["nativeUs"]:.3f} | '
                       f'{c["ratio"]:.3f} / {c["upper"]:.6f} | {c["allocatedBytes"]:,.0f} |')
    original_fills = json.loads((ROOT / 'benchmarks/original-clipped-fill-evidence.json').read_text(encoding='utf-8'))
    fq = original_fills['qualification']
    assert original_fills['measurementAccepted'] and not original_fills['productionChanged']
    assert not original_fills['productionSpeedupClaim'] and original_fills['newRequiredPaths'] == 0
    assert original_fills['requiredMaterialPaths'] == 289 and original_fills['producers'] == jpeg_markers['producers']
    assert fq['reusedForUnchangedProduction'] and fq['newBroadOrPackageChecks'] == 0
    assert fq['fullChecksEachOs'] == 6747 and fq['sameActualPackageBothOs'] and fq['usageExamples'] == 3
    assert fq['packageSha256'] == jpeg_markers['producers']['packageSha256']
    assert original_fills['raw']['mirrored'] and original_fills['raw']['allMembersVerified']
    for depth in (8, 16):
        c = original_fills['cases'][f'DrawingFill{depth}OriginalClipped']
        assert c['managedActuals'] == c['nativeSamplesEachBracket'] == 12
        assert c['librarySha256'] == jpeg_markers['producers']['linuxSha256'] and c['allocatedBytes'] == 0
        assert (c['x'], c['y'], c['fillWidth'], c['fillHeight'], c['clippedWidth']) == (-3, 5, 257, 129, 254)
        assert all(c[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed',
                                 'fullCanvasExact', 'alphaExact', 'hiddenRgbExact', 'untouchedExact', 'callerBuffersAlive'))
        assert c['status'] == 'pass' and c['upper'] < 1
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
        assert original_fills['scalar'][f'DrawingFill{depth}OriginalClipped']['sameCurrentPackageLibrary']
        editing += (f'\n| Original clipped RGBA{depth} fill, 254×129 (parity confirmed) | {c["managedUs"]:.3f} | '
                    f'{c["nativeUs"]:.3f} | {c["ratio"]:.3f} / {c["upper"]:.6f} | 0 |')
    cc = copy_current['cases']['OriginalClippedCopy8']
    assert (cc['sourceX'], cc['sourceY'], cc['destinationX'], cc['destinationY'], cc['copyWidth'], cc['copyHeight'], cc['clippedWidth']) == (0, 0, -3, 5, 257, 129, 254)
    assert all(cc[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed', 'clippingTimed',
                              'fullCanvasExact', 'alphaExact', 'hiddenRgbExact', 'untouchedExact', 'sourceUnchanged', 'callerBuffersAlive'))
    editing += (f'\n| Original clipped RGBA8 copy, 254×129 ({cc["status"]}) | {cc["managedUs"]:.3f} | '
                f'{cc["nativeUs"]:.3f} | {cc["ratio"]:.3f} / {cc["upper"]:.6f} | 0 |')
    clipped_composite = json.loads((ROOT / 'benchmarks/original-clipped-composite-evidence.json').read_text(encoding='utf-8'))
    oq, oc = clipped_composite['qualification'], clipped_composite['case']
    assert clipped_composite['measurementAccepted'] and not clipped_composite['productionChanged']
    assert not clipped_composite['productionSpeedupClaim'] and clipped_composite['newRequiredPaths'] == 0
    assert clipped_composite['requiredMaterialPaths'] == 289 and clipped_composite['producers'] == jpeg_markers['producers']
    assert oq['reusedForUnchangedProduction'] and oq['newBroadOrPackageChecks'] == 0
    assert oq['fullChecksEachOs'] == 6747 and oq['sameActualPackageBothOs'] and oq['normalScalarCurrentPackage']
    assert oq['packageSha256'] == jpeg_markers['producers']['packageSha256']
    assert clipped_composite['raw']['mirrored'] and clipped_composite['raw']['allMembersVerified']
    assert oc['managedActuals'] == oc['nativeSamplesEachBracket'] == 12 and oc['allocatedBytes'] == 0
    assert oc['librarySha256'] == jpeg_markers['producers']['linuxSha256']
    assert (oc['x'], oc['y'], oc['effectiveWidth'], oc['effectiveHeight']) == (-3, 5, 510, 254)
    assert all(oc[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed',
                              'exactManagedPixels', 'sourceUnchanged', 'borrowedCanvas', 'resetExcludedBeforeEveryOperation'))
    assert oc['managedInvocationsPerIteration'] == oc['managedUnrollFactor'] == 1
    assert oc['status'] == 'pass' and oc['upper'] < 1
    assert math.isclose(oc['ratio'], oc['managedUs'] / oc['nativeUs'], rel_tol=1e-12)
    pg = clipped_composite['pixelGates']
    assert pg['nativeRgbMaximum'] <= 1 and pg['nativeRgbRmse'] <= .5 and pg['nativeActiveRgbRmse'] <= .5
    assert pg['alphaMaximum'] == 0 and pg['untouchedExactPixels'] > 0 and pg['zeroSourceAlphaExactPixels'] > 0
    all_borrowed_cases = [*drawing_cases.values(), *original_fills['cases'].values(), cc, oc]
    all_borrowed_counts = {field: sum(c['status'] == status for c in all_borrowed_cases)
                           for field, status in [('passCount', 'pass'), ('gaps', 'gap'), ('uncertain', 'uncertain')]}
    all_borrowed_counts['total'] = len(all_borrowed_cases)
    assert all_borrowed_counts == copy_current['currentBorrowedDrawingCounts']
    editing += (f'\n| Original clipped RGBA8 composite / retained FFmpeg (parity confirmed) | {oc["managedUs"]:.3f} | '
                f'{oc["nativeUs"]:.3f} | {oc["ratio"]:.3f} / {oc["upper"]:.6f} | 0 |')
    default_variation = json.loads((ROOT / 'benchmarks/default-variation-evidence.json').read_text(encoding='utf-8'))
    dv = default_variation['case']
    assert default_variation['measurementAccepted'] and not default_variation['productionChanged']
    assert not default_variation['productionSpeedupClaim'] and default_variation['newRequiredPaths'] == 0
    assert default_variation['producers'] == jpeg_markers['producers']
    assert default_variation['raw']['mirrored'] and default_variation['raw']['allMembersVerified']
    assert dv['originalRequiredControl'] == 'TextLayout/OpenTypeDefaultVariation'
    assert dv['actualsEach'] == 12 and dv['allocatedBytes'] == 3608 and dv['referenceAllocatedBytes'] == 58496
    assert all(dv[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed',
                              'exactManagedObservations', 'referenceObservationsUnchanged', 'independentOutputs'))
    assert dv['status'] == 'pass' and dv['upper'] < 1
    assert math.isclose(dv['ratio'], dv['managedUs'] / dv['referenceUs'], rel_tol=1e-12)
    assert default_variation['qualification']['newBroadOrPackageChecks'] == 0
    editing += (f'\n| Authored default-variation layout / ImageSharp public integration (parity confirmed) | '
                f'{dv["managedUs"]:.3f} | {dv["referenceUs"]:.3f} | {dv["ratio"]:.3f} / {dv["upper"]:.6f} | {dv["allocatedBytes"]:,} |')
    authored = json.loads((ROOT / 'benchmarks/authored-font-comparisons-evidence.json').read_text(encoding='utf-8'))
    assert authored['measurementAccepted'] and not authored['productionChanged']
    assert authored['requiredMaterialPaths'] == 289 and authored['newRequiredPaths'] == 0
    assert authored['producers'] == entropy['producers']
    assert authored['raw']['mirrored'] and authored['raw']['allMembersVerified']
    for name, label in [('OriginalUnicode', 'Authored Unicode layout'), ('PackedBmp', 'Authored cmap4 layout'),
                        ('CompositeTransforms', 'Authored compound layout'), ('Render8', 'Authored RGBA8 layout + draw'),
                        ('Render16', 'Authored RGBA16 layout + draw')]:
        a = text_band['authoredCases'][name] if name in text_band['authoredCases'] else authored['cases'][name]
        assert a['actualsEach'] == 12 and a['status'] == 'pass' and a['upper'] < 1
        assert all(a[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed', 'exactManagedObservations', 'independentOutputs'))
        editing += (f'\n| {label} / corrected ImageSharp integration (parity confirmed) | '
                    f'{a["managedUs"]:.3f} | {a["referenceUs"]:.3f} | {a["ratio"]:.3f} / {a["upper"]:.6f} | {a["allocatedBytes"]:,} |')
    png_package = json.loads((ROOT / 'benchmarks/png-current-package-cost-evidence.json').read_text(encoding='utf-8'))
    png_package_case = png_package['case']
    assert png_package['measurementAccepted'] and not png_package['productionChanged']
    assert not png_package['productionSpeedupClaim'] and png_package['newRequiredPaths'] == 0
    assert png_package['producers'] == jpeg_pending['producers']
    assert png_package['raw']['mirrored'] and png_package['raw']['allMembersVerified']
    assert png_package['sourceReconciliation']['originalPngDecoderCases'] == 47
    assert png_package_case['caseId'] == 'Decode8' and png_package_case['status'] == 'pass'
    assert png_package_case['librarySha256'] == png_package['producers']['linuxSha256']
    assert png_package_case['actualIterations'] == png_package_case['nativeSamplesEachBracket'] == 12
    assert all(png_package_case[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed',
                                           'originalGeneratedLoopsAndConsumer', 'completePublicDecodeAndIndependentOutputDisposal'))
    assert not png_package_case['forceGcBetweenIterations'] and png_package_case['upper'] <= 1
    assert math.isclose(png_package_case['ratio'], png_package_case['managedUs'] / png_package_case['nativeUs'], rel_tol=1e-12)
    png_decoders = json.loads((ROOT / 'benchmarks/png-decoder-current-package-evidence.json').read_text(encoding='utf-8'))
    assert png_decoders['measurementAccepted'] and png_decoders['producers'] == png_package['producers']
    assert png_decoders['raw']['mirrored'] and png_decoders['raw']['allMembersVerified']
    assert png_decoders['measuredCases'] == len(png_decoders['cases']) == 46 and not png_decoders['unresolved']
    png_tight_word = png_decoders['tightWordConfirmation']
    assert png_tight_word['raw']['mirrored'] and png_tight_word['raw']['allMembersVerified']
    assert png_tight_word['status'] == 'pass' and png_tight_word['upper'] <= 1
    assert png_decoders['cases']['DecodeTight16']['status'] == 'uncertain'
    assert png_tight_word['inputSha256'] == png_decoders['cases']['DecodeTight16']['inputSha256']
    assert png_tight_word['outputSha256'] == png_decoders['cases']['DecodeTight16']['outputSha256']
    png_word_stream = png_decoders['wordStream']
    assert png_word_stream['raw']['mirrored'] and png_word_stream['raw']['allMembersVerified']
    assert png_word_stream['librarySha256'] == png_package['producers']['linuxSha256']
    assert png_word_stream['actualPackageSha256'] == png_package['producers']['packageSha256']
    assert png_word_stream['status'] == 'pass' and png_word_stream['upper'] <= 1
    assert png_word_stream['actualIterations'] == 12 and png_word_stream['allActualsRetained']
    assert png_word_stream['allOriginalGatesPassed'] and png_word_stream['quietGuardsPassed']
    png_passes = 2 + png_decoders['statusCounts']['pass']
    png_status = {'pass': 'pass', 'gap': 'gap', 'uncertain': 'confidence open'}
    png_storage = png_decoders['storage']
    assert png_storage['producers'] == png_package['producers']
    assert png_storage['raw']['mirrored'] and png_storage['raw']['allMembersVerified']
    assert png_storage['asyncProofRestoration']['restoredExactly']
    for c in png_storage['cases'].values():
        assert c['librarySha256'] == png_package['producers']['linuxSha256']
        assert len(c['actualOperationCounts']) == 12 and c['upper'] <= 1 and c['status'] == 'pass'
        assert all(c[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed'))
    for label, name in (
        ('PNG RGBA8 async, one-shot Optimal6, nonseekable output †', 'async-encode'),
        ('PNG8 decode, two retained images / shared caller root †', 'retained'),
    ):
        c = png_storage['cases'][name]
        output = '—'
        if name == 'async-encode':
            assert c['managedOutputBytes'] == c['nativeOutputBytes']
            output = f'{c["managedOutputBytes"]:,} / {c["nativeOutputBytes"]:,}'
        heading = f'| {label} |'
        old, = (line for line in codecs.splitlines() if line.startswith(heading))
        result = (f'| {label} | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | '
                  f'{c["ratio"]:.3f} / {c["upper"]:.6f} | {c["allocatedBytes"]:,} | {output} |')
        codecs = codecs.replace(old, result)
    png_validators = png_decoders['validators']
    assert png_validators['producers'] == png_package['producers']
    assert png_validators['raw']['mirrored'] and png_validators['raw']['allMembersVerified']
    assert png_validators['preparation']['exactPreparationExceptPackage']
    for label, name in (
        ('Validate PNG8, span †', 'ValidatePng8'),
        ('Validate PNG16, span †', 'ValidatePng16'),
        ('Validate indexed Adam7 19×17, span †', 'ValidateIndexedAdam7'),
        ('Validate PNG16, async nonseekable †', 'ValidatePng16Async'),
    ):
        c = png_validators['cases'][name]
        assert c['librarySha256'] == png_package['producers']['linuxSha256']
        assert len(c['actualOperationCounts']) == 12 and c['upper'] <= 1 and c['status'] == 'pass'
        assert all(c[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed'))
        heading = f'| {label} |'
        old, = (line for line in read_table.splitlines() if line.startswith(heading))
        result = (f'| {label} | {c["managedUs"]:,.3f} | {c["nativeUs"]:,.3f} | '
                  f'{c["ratio"]:.3f} / {c["upper"]:.6f} | {c["allocatedBytes"]:,} | Confirmed pass |')
        read_table = read_table.replace(old, result)
    for label, name in (
        ('PNG RGBA8 decode, live token †', 'DecodeCancelable8'),
        ('PNG RGBA8 decode, tight working limit †', 'DecodeTight8'),
        ('PNG RGBA16 decode, tight working limit †', 'DecodeTight16'),
    ):
        c = png_tight_word if name == 'DecodeTight16' else png_decoders['cases'][name]
        assert c['librarySha256'] == png_package['producers']['linuxSha256']
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12
        assert all(c[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed'))
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        heading = f'| {label} |'
        old, = (line for line in codecs.splitlines() if line.startswith(heading))
        result = (f'| {label} ({png_status[c["status"]]}) | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | '
                  f'{c["ratio"]:.3f} / {c["upper"]:.6f} | {c["allocatedBytes"]:,} | — |')
        codecs = codecs.replace(old, result)
    bicubic_package = json.loads((ROOT / 'benchmarks/bicubic-current-package-evidence.json').read_text(encoding='utf-8'))
    assert bicubic_package['producers'] == png_package['producers']
    assert bicubic_package['raw']['mirrored'] and bicubic_package['raw']['allMembersVerified']
    adjacent = bicubic_package['defaultReductionAdjacent']
    assert adjacent['raw']['mirrored'] and adjacent['raw']['allMembersVerified']
    assert adjacent['nativeAdjacentOperationsEach'] == 240 and adjacent['nativeSamplesEachBracket'] == 12
    for label, name in (
        ('Bicubic resize RGBA8', 'Down8'),
        ('Bicubic RGBA8 reduction, input+output+4 KiB', 'TightDown8'),
        ('Bicubic RGBA8 enlargement, input+output+4 KiB', 'TightUp8'),
    ):
        c = bicubic_package['cases'][name]
        if name == 'Down8':
            assert all(adjacent[key] == c[key] for key in ('inputSha256', 'managedOutputSha256', 'nativeOutputSha256'))
            c = {**c, **adjacent}
        assert c['librarySha256'] == png_package['producers']['linuxSha256']
        assert len(c['actualOperationCounts']) == 12
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert all(c[k] for k in ('allActualsRetained', 'allOriginalGatesPassed', 'quietGuardsPassed'))
        size = f'{c["outputWidth"]}×{c["outputHeight"]}'
        heading = f'| {label} † → {size} |'
        old, = (line for line in editing.splitlines() if line.startswith(heading))
        status = f' ({png_status[c["status"]]})' if c['status'] != 'pass' else ''
        result = (f'| {label}{status} † → {size} | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | '
                  f'{c["ratio"]:.3f} / {c["upper"]:.6f} | {c["allocatedBytes"]:,} |')
        editing = editing.replace(old, result)
    displayed_count = sum(len(table.splitlines()) - 2 for table in (codecs, editing, read_table))
    return f"""# Lokad.Image benchmarks

Current development version **0.2.1**. Warm single-CPU parity remains open across
**289 named material paths**; these **{displayed_count} comparisons** summarize that scope.
This is an incremental update. Nearest resizing and PNG composition use the
completed-row package; self copies retain the source-valid shared-self-row package.
Three separate/clipped copy comparisons use the pending-publication package with
corrected per-operation native preparation. Current
font loading and complete text integrations use the mapping-validation package;
ordinary/clipped drawing retains the source-valid scan-band-coordinate package.
JPEG stream probes and complete headers use the pending-publication package.
[Resident PNG decode](../benchmarks/PNG-CURRENT-PACKAGE.md) passes for
**{png_passes} of 47** original controls on that actual package with natural GC.
The tight working limit RGBA16 confirmation has an upper ratio of **{png_tight_word['upper']:.6f}**
and **{png_tight_word['allocatedBytes']:,} B/op**. The three displayed resident decode rows use these measurements.
Fresh seekable RGBA16 PNG decode also passes on the same package at
**{png_word_stream['managedUs']:,.2f} / FFmpeg {png_word_stream['nativeUs']:,.2f} µs**,
upper **{png_word_stream['upper']:.6f}**, **{png_word_stream['allocatedBytes']:,} B/op**.
The RGBA8 seekable and async nonseekable loads, async nonseekable encode,
and caller-budget two-result decode also pass on that package. The encode
and retained-output rows use the current measurements, as do all four PNG
pixel-coding validation rows.
[Current bicubic measurements](../benchmarks/BICUBIC-CURRENT-PACKAGE.md) update
the default and tight RGBA8 reductions and the tight RGBA8 enlargement.
Other entries that
call the changed shared image/font pool, PNG workspace or async encoded-output
pool retain **historical artifact costs pending refresh**, not current timing
proof. [Affected boundaries](../benchmarks/COMPOSITION-STORAGE-REUSE.md) are explicit.
This is not a full release refresh or a whole-library parity claim.

{jpeg_direct_note}

{borrowed_drawing_note}

[Original planned coverage](../benchmarks/ORIGINAL-PLANNED-COVERAGE-RECONCILIATION.md)
retains inherited unmeasured names. Passing related inputs or different public
boundaries do not retire those requirements; the full 289-path objective stays open.

[Original clipped fills](../benchmarks/ORIGINAL-CLIPPED-FILL.md) pass; the
[clipped byte copy](../benchmarks/COPY-OPERATION-BOUNDARY.md) is a demonstrated gap. Both use the original 254×129 effective region; earlier 256-pixel controls
stay separate. The [original byte composite](../benchmarks/ORIGINAL-CLIPPED-COMPOSITE.md)
passes at **{oc['managedUs']:.3f} / {oc['nativeUs']:.3f} µs**, upper **{oc['upper']:.6f}**, over 510×254 pixels.
Its reference is a **retained FFmpeg n8.1 overlay graph/static source**, with reset
excluded before EVERY operation on both sides; graph preparation is outside this
repeated borrowed mutation, not a one-shot cost. Managed pixels remain exact;
native RGB max 1/RMSE .403307, alpha/untouched/zero-source-alpha gates pass.
All four original clipped operations allocate **0 B/public operation**; composite
allocation uses a separate 3×128 normal/scalar diagnostic because its BDN driver
emits no GC line. Added owned capacity per operation is zero; standing caller pixels,
native graph/allocator retention and unmeasured physical/JIT peaks stay separate.
Reuse unchanged **6,747 checks per OS/SAME actual package/all 3 examples**. All inherited
drawing recipes now have measurements: **thirteen borrowed controls, ten passes,
two gaps, one uncertainty**. [Authored default variation](../benchmarks/DEFAULT-VARIATION.md)
now passes/stops at **{dv['managedUs']:.3f} / {dv['referenceUs']:.3f} µs**, upper **{dv['upper']:.6f}**,
with **3,608 / 58,496 B/op**. Four reference public layout calls, outlines,
measurement, independent output and disposal are included; fonts are prepared
outside both. This is ImageSharp integration, not FFmpeg, bare shaping or one-shot
font loading. Managed surviving-layout charge **3,568 B**, governed peak **5,738 B**
and unmeasured reference/physical retention are separate from allocations.
The [five original authored-font controls](../benchmarks/AUTHORED-FONT-COMPARISONS.md)
remain passing: affected rendering costs use the conditional-polygon-scratch package, while
the three layout costs reuse unchanged paths. Their reference is explicitly
**source-corrected Fonts 2.1.3 / Drawing 2.1.7 public integration**, retaining stock
identities, exact patches and independent defect reproductions. All nine public
layout calls/tab adaptation, owned results, optional drawing and disposal stay timed;
fonts and per-operation canvas reset stay outside both. Managed frames/fixtures and
all exact/visual gates remain unchanged. Public-operation allocations are listed;
single-invocation BDN drawing additionally counts 736 B of common scaffolding.
Independent confirmation retains all actuals/guards and stops all five. All 17
inherited names now have measurements: **16 passes, one copy gap**. Broader text
processing and the remaining 289-path objective stay open; no historical stage sums.

[Batched JPEG entropy inspection](../benchmarks/JPEG-BATCHED-ENTROPY.md) saves
**{ep['elapsedReduction']:.2%} / {ep['savedUs']:.3f} µs** in independent controlled
complete-operation pairs. Current standalone **JPEG {entropy['case']['managedUs']:.3f} /
{entropy['case']['nativeUs']:.3f} µs, upper {entropy['case']['upper']:.6f}, 8,768 B/op**
passes/stops. Two new lane/buffer/pending-read controls pass original/current,
scalar and AVX2-disabled execution; **6,749 full/live checks per OS** and the SAME
actual package/all three examples pass, without rebuilding the timed artifact.
No added heap capacity or retention; admission stays 512/8,448 B, native bounded
scratch 16,416 B excluding contexts/descriptors, physical/JIT peaks unmeasured.
The [earlier stuffed-pair round](../benchmarks/JPEG-STUFFED-PAIRS.md) retains history;
its gains are not added. Source-valid **WebP .142106 / .310231 µs, upper .471884,
552 B/op** stays stopped. The earlier **2.698 ns / 1.255% async JPEG dimension-probe
regression** remains explicit; its unchanged-path cost is **.220388 / .201119 µs,
upper1.133673**, still a gap. Sync probe pairs are inconclusive; unchanged prefix
paths reuse their identified evidence. Different wrappers'8,776/8,768B are not an
allocation improvement. Header references are
explicitly **FFmpeg AVIO/owned descriptor APIs plus original bounded structural/EXIF
adapters**, not public packet-parser performance. Both include fresh non-exposed
source/working state/descriptor ownership, complete late-metadata/EOF checks and
exact nine-field source metadata, with no pixels or whole-input copy. Twenty
same-packet controls and two new one-byte/pending/late-EXIF/restart-limit cases pass.
After affected costs reconcile, Windows/Linux each pass **6,747 full/live checks**,
**10 internal/156 public normal/scalar**, the **SAME actual package/all three examples**.
One core/embedded/public build per OS, zero helper/native/harness/fixture builds,
one NoBuild pack, no timed Linux rebuild. New storage/retention stays zero;
admission remains512/8,448B. Native scratch16,416B excludes contexts/descriptors;
physical/native/JIT peaks remain unmeasured. Packets/pixels and ownership stay exact.

[Prepared stream input](../benchmarks/PREPARED-MEMORY-INPUT.md) saves **2.75 µs /
15.40% raw**, **15.01% reference-normalized**, on complete PNG `Identify(Stream)`:
**15.05 / reference adapter 15.88 µs, upper .950**, **192 B/op** on the
current [publication round](../benchmarks/PROBE-RESULT-PUBLICATION.md). One owned input
capacity replaces geometric growth for ordinary synchronous `MemoryStream` inputs
up to 4 MiB. This control's logical acquisition peak falls **393,216 → 262,144 B**;
no idle-pool or complete-decode peak saving is inferred. The original uncertain
stream captures remain linked beside their current confirmations.

{png_stream_word_note}

[Current VP8 arithmetic](../benchmarks/VP8-FIXED-CODE.md) retains the
stream pass: **{reader_current['cases']['StreamNative8']['managedUs']:,.2f} / FFmpeg
{reader_current['cases']['StreamNative8']['nativeUs']:,.2f}µs, upper
{reader_current['cases']['StreamNative8']['upper']:.3f}**, **240,496 B/op**. This complete
operation includes fresh stream acquisition, decoding, owned output and disposal.

[Opaque lossless WebP](../benchmarks/WEBP-OPAQUE-CURRENT-CONFIDENCE.md) now passes:
**67.959 / libwebp 69.457 ms, upper .985**, **4,822,760 B/op**, exact **121,896 B**.
One prespecified longer batch resolves uncertainty on the actual package DLL,
with zero builds or qualification repeats; no production speedup is claimed.
All eight lossless/alpha controls pass and stop. [Word alpha](../benchmarks/WARM-ADJACENT-CONFIDENCE.md)
now also passes on the actual package, upper .979, **6,241,792 B/op**, exact
**98,590 B**; its prespecified longer batch also requires no builds or qualification repeats.

{owned_copy_note}

The [family reports](../benchmarks/WARM-COVERAGE.md) identify source-valid passing
paths and remaining copy/inspection gaps. Historical image-view aggregates
stay in their original reports; the complete final-package refresh remains open.
{bicubic_adjacent_note}
[RGBA16 bilinear enlargement](../benchmarks/BILINEAR-PARTITIONED-REFERENCE.md)
passes/stops: **{bilinear_word_case['managedUs']:,.2f} / FFmpeg {bilinear_word_case['nativeUs']:,.2f}µs,
upper {bilinear_word_case['upper']:.6f}**, **{bilinear_word_case['allocatedBytes']:,} B/op**.
Finer interleaving partitions each256-operation actual into16subdivisions while
retaining twelve statistical units per group, every operation and the unchanged
warmups/conservative gate. Extra clock reads stay measured; more frequent native
IPC/client work stays outside. Reuse6,823checks per OS/SAME actual C620/f034 package/
all3examples; one small driver build, zero production/harness/native/package builds
or broad repeats. The frozen manifest is restored as an independent file before
timing, preserving current metadata. Old uncertainty remains history; no production
speedup or host cause is claimed. Fresh managed preparation and retained native
graph/geometry stay timed as declared, distinct from the equivalent view below.
[Equivalent one-shot bilinear](../benchmarks/BILINEAR-EQUIVALENT-ONE-SHOT.md) now passes:
**{bilinear_fresh_case['managedUs']:,.2f} / FFmpeg {bilinear_fresh_case['nativeUs']:,.2f} µs, upper {bilinear_fresh_case['upper']:.3f}**,
**{bilinear_fresh_case['allocatedBytes']:,} B/op**. Fresh graph/context/geometry, lazy first use,
result wrapper, filtering and owned-output disposal are timed. Both sides borrow resident
source rasters. Exact managed/full-word/native/ownership gates remain; the existing
qualified package is reused with no production rebuild. This qualifies the complete
bilinear boundary; the eight earlier equivalent views cover bicubic. Both bilinear
views pass/stop without a production speedup or historical phase-cost subtraction.
No material path or settings grid is added.
Longer-integration [grayscale PNG](../benchmarks/PNG-GRAY-CURRENT-PATH.md) resolves
uncertainty at **{png_gray8['managedUs']:,.2f} / FFmpeg {png_gray8['nativeUs']:,.2f}µs,
upper {png_gray8['upper']:.6f}**, **{png_gray8['allocatedBytes']:,} B/op**. All 17
equivalent fresh-state PNG encodes pass/stop. Original uncertain samples remain;
this measurement-only update is not a production speedup. Stored RGBA16's
post-GC diagnostic stays separate from its passing natural-GC workload.
[Variable-price preparation](../benchmarks/VP8-VARIABLE-PRICE-PREPARATION.md) closes
the photograph gap: **{vp8_encode_cases['photograph']['managedUs']:,.2f} / FFmpeg
{vp8_encode_cases['photograph']['nativeUs']:,.2f}µs, upper {vp8_encode_cases['photograph']['upper']:.3f}**,
**809,080 B/op**, exact **17,688 B**. All nine lossy and all eight lossless/alpha
controls pass/stop. Independent controlled gains are **154.41µs/1.623%** photograph
and **228.60µs/.433%** RGBA16alpha; allocations/retention remain unchanged.
The longer trellis integration retains its original uncertain capture. Unchanged
encoder paths reuse this source-valid evidence on the current qualified package.
This scoped refresh does not establish whole-library parity; publication is postponed.
[Fixed-position VP8 arithmetic](../benchmarks/VP8-FIXED-CODE.md) keeps the code
byte at a fixed UInt64 position, preserving scalar cursors, every cancellation
check and input-refill addresses. Independent live-token pairs save **41.53µs/.86%**
in their scoped ALC comparison. The actual standalone live comparison now passes:
**{opaque_live['managedUs']:,.2f} / FFmpeg {opaque_live['nativeUs']:,.2f}µs**, ratio
**{opaque_live['ratio']:.4f}**, upper **{opaque_live['upper']:.6f}**,
**{opaque_live['allocatedBytes']:,} B/op**. All pixels remain exact; original
uncertainties remain in history. All six affected costs refresh on the timed,
immutable actual package; **all twelve WebP reads pass/stop**. Separate ordinary,
stream and coding-validation points are higher than the preceding round; current
costs stay visible and no uniform speedup or historical-clock gain is claimed.
Allocations, capacity and retention are unchanged. Both OSes pass **6,692 full/live** checks,
focused normal/scalar checks and the **same actual package/all three examples**.
[Earlier unaccepted arithmetic-window trials](../benchmarks/VP8-LIVE-WINDOW-TRIALS.md)
remain separate history; none of their coefficient state-bridge changes survives.
[Canonical Lanczos identity axes](../benchmarks/LANCZOS-IDENTITY-AXIS.md) remove
zero-only coefficient storage and size working rows from actual taps.
[Current-package measurements](../benchmarks/LANCZOS-CURRENT-PACKAGE.md) confirm
all six affected two-axis comparisons and both one-axis controls. Both RGBA16
enlargements exceed the pool's unchanged 4 MiB cap and keep valid passing evidence.

Two additional complete managed-DLL diagnostics use the existing full-word input:
513×259→513×129 independently improves **{lanczos_current['identityAxisDiagnostics']['Vertical16']['rawSavedFraction']:.2%} /
{lanczos_current['identityAxisDiagnostics']['Vertical16']['savedUs']:.2f} µs**. The horizontal
513×259→256×259 CPU effect is inconclusive, while allocations fall about
**100,136→44,712 B/op** and retained preparation **102,000→46,656 B**.
Its governed one-shot peak falls **632,432→577,088 B**. Vertical allocation falls
about **267,016→254,672 B/op**, retained **269,364→257,100 B**, and peak
**798,780→786,516 B**. Ranges and raw observations remain in the report.
Those diagnostics establish scoped prior/candidate effects. Both [one-axis
comparisons](../benchmarks/LANCZOS-IDENTITY-AXIS-REFERENCE.md) retain their
complete one-shot boundaries and original quality checks.
Managed GC ranges **44,712–44,720 / 254,672–254,679B/op** remain separate from
retained plans and governed peaks. All ten Lanczos views pass/stop; no stage-cost
sum or broader grid is implied. The full289-path objective stays open.
Windows/Linux each pass **6,742 full/live**, **163 internal/15 public** focused
normal/scalar/AVX512-disabled checks, the three named resize-storage suites and
the SAME actual package/all three examples, including one-axis asynchronous
survivors. Allocation, retained admission and physical peaks stay distinct.
[Horizontal JPEG preparation](../benchmarks/JPEG-HORIZONTAL-PREPARATION.md) saves
**4.03% / 16.92 µs** in controlled complete-decode pairs, with unchanged **20,944 B/op**.
The 27 unchanged RGBA/fresh/caller-buffer read views retain passing evidence;
the affected stream view refreshes and passes in the prepared-input round. The
horizontal-CMYK cost is **410.41 / FFmpeg 418.11 µs, upper .988**. Companion
confidence is reconciled before acceptance; historical clock shifts are not gains.
Adverse and uncertain captures stay linked; copy/turn confidence remains open.

Every operation runs sequentially on the dedicated quiet VM, pinned to one CPU,
with .NET 10, optimized JIT and nonconcurrent workstation GC. Startup, transport,
fixture preparation and validation are outside the clocks. Timed calls retain
their declared normalization, input delivery, output ownership and disposal.
Ratios are Lokad/reference; **upper** is the conservative confidence bound, with
all actuals retained. Upper ≤1 passes, lower >1 is a gap; otherwise uncertainty stays open.
**GC B/op** reports managed allocation, including result ownership where applicable,
not peak live capacity or native allocation.

The shared image/font store now has **four slots / 4 MiB**; codec-local pools
retain **two slots / 4 MiB** each. Source-derived word-composition idle payload
is **2,455,088 B**, up from **2,125,872 B**; four simultaneous image payloads still
total **2,455,088 B**. This added idle retention is separate from reduced allocation.
Caller-governed storage bypasses that cache. Logical admitted
capacity, allocation and retained storage are separate; physical/native/JIT peaks
are unmeasured. Prepared word Lanczos reduction retains **156,248 B** of plan
storage, separately from its **138 B/op**. That storage stage passed **6,683
full/live checks** per OS, focused normal/scalar copy-ownership checks and the
**same actual Release-package consumer / three examples**.
Unchanged arithmetic/ISA qualification is reused; publication is postponed.

[JPEG pending publication](../benchmarks/JPEG-PENDING-PUBLICATION.md) keeps typed
suspension construction outside immediate parsing. Final complete async-probe
pairs save **{jpeg_pending['pairedEffect']['elapsedReduction']:.2%} /
{jpeg_pending['pairedEffect']['savedUs']*1000:.2f} ns**, without a uniform or cumulative
gain claim. JPEG sync now passes/stops (upper
**{jpeg_pending['cases']['ProbeDimensionsJpeg']['upper']:.3f}**). Async remains uncertain after
[one longer integration](../benchmarks/JPEG-PROBE-LONGER-INTEGRATION.md):
**{current_async_probe['managedUs']:.3f}/{current_async_probe['nativeUs']:.3f} µs, upper
{current_async_probe['upper']:.3f}**. Both actual counts increase fourfold; original
warmups, twelve units/group, sixteen subdivisions and every observation remain.
Its adverse wider interval is retained; unchanged-case captures stop.
Complete headers pass; their slower historical point is retained, while the matched
original-worker guard is neutral/inconclusive. No favorable recapture follows.
Immediate probes/header calls allocate **568/8,768 B**. Original compiled loops,
every observation and descriptor/source/owner gates remain.
The immediate parser frame shrinks **1,080→472 B**; this is not a whole-operation
physical peak. Admission stays **512/8,448 B**, with no new retained workspace.
Gated-read diagnostics stay **1,960/9,984 B**, including equal delivery/bridge/batch
overhead, not standalone production allocation. Each OS passes **6,844 full/live
checks**, **11 internal/182 public** normal/scalar checks and the SAME actual
package/three examples, without rebuilding the timed DLL. Unchanged PNG/WebP
probes/header and other families retain their identified evidence; the final
complete package refresh remains pending. [Earlier owner work](../benchmarks/PROBE-OWNER-COMPLETION.md)
and [reference interleaving](../benchmarks/JPEG-PROBE-ORIGINAL-LOOP-REFERENCE.md)
retain history.
[Wide scan CRC](../benchmarks/WIDE-PNG-SCAN-CRC.md) closes resident PNG Identify:
**{resident_png['cases']['identify/Png8']['managedUs']:.3f} / FFmpeg adapter
{resident_png['cases']['identify/Png8']['nativeUs']:.3f} µs**, conservative upper
**{resident_png['cases']['identify/Png8']['upper']:.3f}**, **96 B/op**. Seekable/async Identify
also pass on the same timed/package DLL. Four existing 128-bit residues advance
together under 512-bit hardware guards; the seed, polynomial, joins, tails,
cancellation positions, portable fallbacks and ownership stay exact. Encoder CRC
and shared fields/helpers are unchanged. Earlier matched-batch diagnostics remain
[history](../benchmarks/PNG-RESIDENT-IDENTIFY-ADJACENT.md).
[Direct publication](../benchmarks/PROBE-RESULT-PUBLICATION.md) retains its allocation
reduction and two source-valid JPEG/WebP resident Identify passes. All 60 affected
PNG costs reconcile before one Windows/Linux qualification milestone: **6,721
full/live checks per OS**, **84 internal /247 public** normal/AVX512-off/scalar,
the SAME actual package and all three examples. One core/embedded build per OS,
one pack and no accepted-module rebuild after timing. No new heap array, static
field, pool or retained operation storage. Other codec/edit/resize paths reuse
unchanged evidence; font construction and complete text integrations are qualified
separately below. This is not a whole-release timing refresh. Probe/header admission stays
**512 B dimension /8,448 B bulk maximum**, zero retained operation workspace;
physical/native/JIT peaks are unmeasured. No historical stage gains are added.

† Reference boundaries require care. Codec contexts or filter graphs may remain
prepared; packed conversion, independent payload acquisition and release are timed
as declared. **zimg** is FFmpeg's labeled scaler backend, using float intermediates;
managed Q30/rational pixel guarantees remain exact. One-shot and prepared results
are distinct. Grayscale JPEG uses approved **SIMD libjpeg-turbo**; lossless WebP uses
**libwebp exact=1** where labeled. These are benchmark tools, never production dependencies.

Text comparisons use **ImageSharp.Drawing 2.1.7 / Fonts 2.1.3 / ImageSharp 3.1.12**;
the five authored controls explicitly label their scoped source corrections above.
They are public integration workloads, not FFmpeg or bare shaping-engine comparisons.
[Shared scan-band coordinates](../benchmarks/TEXT-SCAN-BAND-COORDINATES.md)
save **{text_band['pairedEffect']['savedUs']:.3f} µs / {text_band['pairedEffect']['gainPercent']:.2f}%**
in controlled complete ordinary-RGBA8 final-DLL pairs. Standalone RGBA8 now passes,
upper **{ordinary_text['cases']['Ordinary8']['upper']:.6f}**.
[Interleaved RGBA16 references](../benchmarks/ORDINARY-WORD-INTERLEAVED-REFERENCE.md)
resolve the affected word comparison as **{ordinary_text['cases']['Ordinary16']['status']}**,
interval **{ordinary_text['cases']['Ordinary16']['lower']:.6f}–{ordinary_text['cases']['Ordinary16']['upper']:.6f}**.
The same 36 native actuals now span the same twelve pairs; the earlier grouped
uncertainty and all samples remain retained. This is a scheduling correction on
unchanged production, not a historical-clock speedup. Both ordinary depths stop.
Original exact frames,
stock reference identities, symmetric per-operation reset/checks, all twelve
16,384-operation actuals and three reference groups remain unchanged.
Ordinary public allocations are **83,008 B/draw**, up **7,168 B**, plus 24 B of
adapter storage; reference allocations remain **48,576 B/draw**. Clipped controls
retain their approved source-corrected references and both pass.

Current complete one-shot and preparation-plus-eight-draw integrations also pass,
including all public layout calls, 30 fitting trials, fresh explicitly cleared
owned canvases, independent results and disposal. These are complete public-API
integrations, not bare shaping-engine comparisons. The measured batch amortizes to
**{font_batch['amortizedManagedUs']:.2f} / {font_batch['amortizedReferenceUs']:.2f} µs/draw**.
Preparation-only **{font_preparation['managedUs']:.2f} / {font_preparation['referenceUs']:.2f} µs**
is diagnostic; historical stages are never summed. Reference preparation retains
positioned paths; managed preparation retains full 16-bit coverage. Validation
eagerness and GC-driven reference font release differ. Reference one-shot/batch
allocations are **9,256,953 /9,494,633 B**. Latin/fit retained plans remain
**2,112 /4,160 B**. Full-fit governed peak is **748,984 B**, up **6,144 B** for
temporary crossings. Total allocations, peak admitted capacity, retained state
and unmeasured physical/native/JIT peaks are separate.

[Batched font mapping validation](../benchmarks/FONT-MAPPING-GROUP-VALIDATION.md) saves
**{font_current['pairedEffect']['savedUs']:.3f} µs / {font_current['pairedEffect']['gainPercent']:.2f}%** in controlled
complete final-DLL pairs. Current **{font_cost['managedUs']:.3f} / {font_cost['nativeUs']:.3f} µs**,
upper **{font_cost['upper']:.6f}**, passes/stops against the scoped **FreeType 2.13.2
copy/open/retain/dispose** reference at **176 B/op**. The original borrowed face is
757,076 B; managed owned backing is 678,016 B, native owned input stays 757,076 B.
Managed live admission is 678,272 B; default idle storage is separate. Original
directory/input limits remain; owned tables retain exact offsets/bytes. Eager/lazy
validation differs, not general parser equivalence or an overall text slowdown. Unchanged [prepared ordinary drawing](../benchmarks/TEXT-PREPARED-COVERAGE.md)
passes/stops at **56 B/op** with its distinct retained-state boundary.

Windows/Linux each pass **6,839 full/live checks**, focused **322 internal/222 public**
normal/AVX2-disabled/scalar checks and the SAME actual mapping-validation package/all three
examples. Affected performance reconciles before qualification; no timed DLL
rebuild occurs. Other families retain identified unchanged-path evidence. The
complete final-package refresh remains pending.

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
    assert len(re.findall(r'^\|', text, re.M)) - 6 == 165, 'Expected 165 comparison rows.'
    output = ROOT / 'docs/benchmark.md'
    if '--check' in sys.argv:
        assert output.read_text(encoding='utf-8') == text, 'Benchmark summary differs from retained evidence.'
        print('Benchmark summary matches current and retained release evidence (three tables).')
    else:
        output.write_text(text, encoding='utf-8', newline='\n')
