"""Render current benchmarks from retained evidence; never measure."""
import json
import math
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


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
    prepared_model = json.loads((ROOT / 'benchmarks/vp8-frame-quantizer-preparation-evidence.json').read_text(encoding='utf-8'))
    assert prepared_model['productionAccepted'] and prepared_model['requiredMaterialPaths'] == 283
    assert prepared_model['raw']['mirrored'] and prepared_model['raw']['allMembersVerified']
    assert set(prepared_model['costs']) == {'Native8', 'Vp8Stream', 'Opaque8', 'SimpleOpaque8', 'OpaqueCancelable8', 'ValidateVp8Alpha'}
    for name, cost in prepared_model['costs'].items():
        assert cost['actualIterations'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['librarySha256'] == prepared_model['producers']['linuxSha256']
        assert cost['allOriginalGatesPassed'] and cost['quietGuardsPassed']
        assert cost['sourceUnchanged'] and cost['independentOwnedOutput']
        if name.startswith('Validate'):
            assert cost['descriptorExact'] and cost['prefixAndCodingScopesVerified']
        else:
            assert cost['managedOutputSha256'] == cost['nativeOutputSha256'] and cost['alphaExact']
            assert cost['decoderMaximum'] == 0 and cost['managedOutputBytes'] == 531468
        assert cost['status'] == ('pass' if cost['upper'] <= 1 else 'gap' if cost['lower'] > 1 else 'uncertain')
        assert math.isclose(cost['ratio'], cost['managedUs'] / cost['nativeUs'], rel_tol=1e-12)
    assert all(v['rawGainSeparated'] for c in prepared_model['comparisons'].values() for v in c['views'].values())
    assert all(v['normalizedGainSeparated'] for v in prepared_model['comparisons']['Native8']['views'].values())
    assert not prepared_model['comparisons']['OpaqueCancelable8']['views']['unfilteredActualStatistics']['normalizedGainSeparated']
    model_proof = prepared_model['sourceProof']
    assert model_proof['originalReconstructedOutsideFrameQuantizerPreparation']
    assert model_proof['allCoefficientReaderContextTransformPredictionFilteringColorOwnershipBodiesExact']
    assert prepared_model['storage']['modelPayloadBytes'] == 1536
    assert prepared_model['storage']['operationStackTableBytes'] == 96
    assert prepared_model['storage']['coarseHeaderWorkspaceBytes'] == 8192
    assert prepared_model['storage']['newOwnedHeapCapacityBytes'] == prepared_model['storage']['newRetainedStateBytes'] == 0
    q = prepared_model['qualification']
    assert q['fullChecksEachOs'] == q['windows']['total'] == q['linux']['total'] == 5955
    assert q['sameActualPackageBothOs'] and q['actualPackage']['sha256'] == q['samePackageLinux']['sameActualPackageSha256']
    live = prepared_model['costs']['OpaqueCancelable8']
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
    bilinear_word_cost = json.loads((ROOT / 'benchmarks/warm-bilinear-word-geometry-evidence.json').read_text(encoding='utf-8'))
    assert bilinear_word_cost['productionChanged'] and bilinear_word_cost['decision']['passed']
    assert bilinear_word_cost['qualification']['sameActualPackageBothOs']
    assert bilinear_word_cost['independentQuietConfirmation']
    bilinear_word = bilinear_word_cost['cases']['Down16'][-1]
    assert len(bilinear_word['managed']['actual']) == 12
    assert bilinear_word['receipt']['managedRationalReferenceExact']
    assert bilinear_word['receipt']['sourceUnchanged'] and bilinear_word['receipt']['independentOwnedOutput']
    assert bilinear_word_cost['comparisonLowerBounds']['Down16'] > 1
    assert bilinear_word_cost['status']['Down16'] == 'confirmed-gap'
    bilinear_byte_cost = json.loads((ROOT / 'benchmarks/current-byte-bilinear-costs-evidence.json').read_text(encoding='utf-8'))
    assert not bilinear_byte_cost['productionChanged'] and bilinear_byte_cost['allBuilds'] == 0
    assert bilinear_byte_cost['independentQuietConfirmation']
    assert set(bilinear_byte_cost['cases']) == {'Down8', 'Up8'}
    assert bilinear_byte_cost['qualification']['sameActualPackageBothOs']
    assert all(v > 1 for v in bilinear_byte_cost['comparisonLowerBounds'].values())
    bilinear_byte_reuse = json.loads((ROOT / 'benchmarks/warm-bilinear-byte-row-reuse-evidence.json').read_text(encoding='utf-8'))
    assert bilinear_byte_reuse['productionChanged'] and bilinear_byte_reuse['decision']['passed']
    assert bilinear_byte_reuse['decision']['normalizedSeparated']
    assert bilinear_byte_reuse['qualification']['fullChecksEachOs'] == 5241
    assert bilinear_byte_reuse['qualification']['sameActualPackageBothOs']
    assert bilinear_byte_reuse['qualification']['byteBilinearRowReusePixelsAndExistingAdmissionPassed']
    assert bilinear_byte_reuse['qualification']['package']['sha256'] == bilinear_byte_reuse['qualification']['linuxPackage']['sameActualPackageSha256']
    assert bilinear_byte_reuse['memory']['ownedCapacityIncrease'] == 0
    bicubic_byte_cost = json.loads((ROOT / 'benchmarks/current-bicubic-byte-reduction-cost-evidence.json').read_text(encoding='utf-8'))
    assert not bicubic_byte_cost['productionChanged'] and bicubic_byte_cost['allBuilds'] == 0
    assert bicubic_byte_cost['independentQuietConfirmation'] and bicubic_byte_cost['status']['Down8'] == 'confirmed-gap'
    assert len(bicubic_byte_cost['cases']['Down8'][-1]['managed']['actual']) == 12
    editing_costs = {'case21': bilinear_word,
                     'case22': bicubic_byte_cost['cases']['Down8'][-1],
                     'case23': word_publication['cases']['Up16'][-1],
                     'case24': cross16_cost['cases']['TightCross16'][-1]}
    editing_producers = {'case21': bilinear_word_cost,
                         'case22': bicubic_byte_cost, 'case23': word_publication,
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
    resident = json.loads((ROOT / 'benchmarks/current-png-resident-identify-evidence.json').read_text(encoding='utf-8'))
    resident_other = json.loads((ROOT / 'benchmarks/current-jpeg-webp-resident-identify-evidence.json').read_text(encoding='utf-8'))
    stream_identify = json.loads((ROOT / 'benchmarks/current-png-stream-identify-evidence.json').read_text(encoding='utf-8'))
    decoders = json.loads((ROOT / 'benchmarks/current-default-decoders-evidence.json').read_text(encoding='utf-8'))
    resident_decode = json.loads((ROOT / 'benchmarks/vp8l-final-publication-evidence.json').read_text(encoding='utf-8'))
    assert resident_decode['productionAccepted'] and resident_decode['requiredMaterialPaths'] == 283
    assert resident_decode['sourceProof']['wholeSourceUndoEqualsQualifiedOriginal']
    assert resident_decode['raw']['mirrored'] and resident_decode['raw']['allMembersVerified']
    assert set(resident_decode['cases']) == {'ValidateVp8lAlpha', 'ValidateVp8Alpha', 'WebpResident', 'Lossless8',
        'IndexedNative16Colors8', 'NativeLosslessCancelable8', 'WebpStream', 'Native8', 'Vp8Stream'}
    for name, observations in resident_decode['cases'].items():
        if name in {'Native8', 'Vp8Stream', 'ValidateVp8Alpha'}:
            continue  # Superseded by the prepared-model owner; six lossless paths are unchanged.
        observation = observations[-1]
        receipt = observation['receipt']
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert receipt['librarySha256'] == resident_decode['producers']['linuxSha256']
        assert receipt['sourceUnchanged'] and receipt['independentOwnedOutput']
        assert all(q['passed'] for q in observation['quietness'].values())
        if name.startswith('Validate'):
            assert receipt['descriptorExact'] and receipt['prefixAndCodingScopesVerified']
        else:
            assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256'] and receipt['alphaExact']
            assert receipt.get('allRgbaSamplesExact', receipt.get('decoderMaximum') == 0)
        assert resident_decode['status'][name] == ('confirmed-pass' if observation['conservativeRatioUpper'] <= 1
            else 'confirmed-gap' if observation['conservativeRatioLower'] > 1 else 'confirmed-confidence-open')
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated']
               for v in resident_decode['comparisons']['WebpResident']['views'].values())
    assert all(v['rawLossSeparated'] and v['normalizedLossSeparated']
               for v in resident_decode['comparisons']['IndexedNative16Colors8']['views'].values())
    assert resident_decode['storage']['ownedCapacityUnchanged']
    assert resident_decode['storage']['newHeapCapacityBytes'] == resident_decode['storage']['newRetainedStateBytes'] == 0
    for platform in ['windows', 'linux']:
        peaks = resident_decode['storage']['validationAdmissionEvidenceReused'][platform]['rows']['candidate']['cases']
        assert [p['firstPeakAdmittedBytes'] for p in peaks] == [235152, 11121]
        assert all(p['retainedBytes'] == 0 and p['borrowedRootAlive'] for p in peaks)
    lpf = {name: cost for name, cost in resident_decode['costs'].items()
           if name not in {'Native8', 'Vp8Stream', 'ValidateVp8Alpha'}}
    lpf['ValidateVp8Alpha'] = prepared_model['costs']['ValidateVp8Alpha']
    lpf.update(prepared_model['costs'])
    jpeg_decode = baseline_blocks
    assert not decoders['productionChanged'] and len(decoders['cases']) == 3
    assert resident_decode['productionChanged'] and resident_decode['independentQuietConfirmation']
    assert resident_decode['qualification']['sameActualPackageBothOs']
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
        if case['id'] in {'case00', 'case06', 'case08', 'case09', 'case10', 'case11', 'case07', 'case13', 'case14', 'case15', 'case16', 'case27', 'case28', 'case29', 'case30'}:
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
    for case, limit in [('DecodeCancelable8', None), ('DecodeTight8', 1235173), ('DecodeTight16', 3254193)]:
        framing_case = png_framing['cases'][case]
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
                        ('WebpStream', 'VP8L RGBA8, fresh seekable Stream decode')]:
        producer = jpeg_decode if case in {'NativeDecode420', 'CmykCaller'} else resident_decode
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
    position = next(i for i, (kind, row) in enumerate(rows) if row.startswith('| Bilinear resize RGBA16'))
    for ordinal, case in enumerate(['Down8', 'Up8']):
        producer = bilinear_byte_reuse if case == 'Up8' else bilinear_byte_cost
        observation = producer['cases'][case][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert receipt['librarySha256'] == producer['producers']['linuxSha256']
        assert receipt['managedRationalReferenceExact'] and receipt['sourceUnchanged'] and receipt['independentOwnedOutput']
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        native = min(b['mean'] for b in observation['nativeBrackets'])
        assert math.isclose(stats['Mean'] / native, observation['worstBracketRatio'], rel_tol=1e-12)
        label = f'Bilinear {"resize" if case == "Down8" else "upscale"} RGBA8 † → {receipt["outputWidth"]}×{receipt["outputHeight"]}'
        row = f'| {label} | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | {observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | {observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |'
        rows.insert(position + ordinal, ('editing', row))
    tight_byte_up = json.loads((ROOT / 'benchmarks/warm-bicubic-byte-fixed-certificate-evidence.json').read_text(encoding='utf-8'))
    assert tight_byte_up['productionChanged'] and tight_byte_up['independentQuietConfirmation']
    assert tight_byte_up['qualification']['fullChecksEachOs'] == 5245
    assert tight_byte_up['qualification']['sameActualPackageBothOs']
    assert tight_byte_up['decisions']['confirm']['passed'] and tight_byte_up['decisions']['confirm']['normalizedSeparated']
    # One current owner for shared byte/word enlargement boundaries.
    enlargement = json.loads((ROOT / 'benchmarks/bicubic-shared-enlargement-evidence.json').read_text(encoding='utf-8'))
    assert enlargement['productionAccepted'] and enlargement['raw']['mirrored'] and enlargement['raw']['allMembersVerified']
    eq = enlargement['qualification']
    assert eq['fullChecksEachOs'] == eq['windows']['total'] == eq['linux']['total'] == 5741
    assert eq['actualPackage']['sha256'] == eq['samePackageLinux']['sameActualPackageSha256']
    assert eq['actualPackage']['outputWorkspaceBicubicPassed'] and eq['samePackageLinux']['outputWorkspaceBicubicPassed']
    assert enlargement['sourceProof']['originalBytePublicationRepairAndGeometryBodiesExact']
    assert enlargement['sourceProof']['wordRepairBodiesExact'] and enlargement['sourceProof']['wordSmallSupportCertificateUnchanged']
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated']
               for v in enlargement['independentComparison']['views'].values())
    assert len(enlargement['cases']) == 12
    for current in enlargement['cases'].values():
        assert current['actualIterations'] == 12 and current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['librarySha256'] == enlargement['producers']['linuxSha256']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    assert enlargement['storage']['newOwnedHeapCapacityBytes'] == 0
    labels = {'TightUp8': 'Bicubic RGBA8 enlargement,',
              'TightHorizontalUp8': 'Bicubic RGBA8 width enlargement,',
              'TightVerticalUp8': 'Bicubic RGBA8 height enlargement,'}
    assert set(labels) <= set(enlargement['cases'])
    for case, label in labels.items():
        current = enlargement['cases'][case]
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed'] and current['actualIterations'] == 12
        assert current['librarySha256'] == enlargement['producers']['linuxSha256']
        assert current['sourceUnchanged'] and current['independentOwnedOutput'] and current['managedQualifiedOriginalPixelsExact']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
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
    wide_byte = json.loads((ROOT / 'benchmarks/current-wide-byte-bicubic-cost-evidence.json').read_text(encoding='utf-8'))
    assert not wide_byte['productionChanged'] and wide_byte['allBuilds'] == 0
    assert wide_byte['qualificationReuse']['sameActualPackageBothOs']
    observation = wide_byte['cases']['Wide8'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == wide_byte['producers']['linuxSha256']
    assert receipt['managedRationalReferenceExact'] and receipt['managedMaxWorkingBytes'] is None
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert all(q['passed'] for q in observation['quietness'].values())
    rows.append(('editing', f'| Bicubic RGBA8 wider-support reduction † → 128×64 | {stats["Mean"] / 1000:,.2f} | '
                 f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                 f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                 f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
    exact_alpha = json.loads((ROOT / 'benchmarks/warm-exact-alpha-int128-evidence.json').read_text(encoding='utf-8'))
    assert exact_alpha['qualification']['fullChecksEachOs'] == 5267
    assert exact_alpha['qualification']['sameActualPackageBothOs']
    assert exact_alpha['decisions']['confirm']['passed'] and exact_alpha['decisions']['confirm']['normalizedSeparated']
    alpha_callers = json.loads((ROOT / 'benchmarks/current-shared-alpha-caller-costs-evidence.json').read_text(encoding='utf-8'))
    assert alpha_callers['producers'] == exact_alpha['producers']
    assert not alpha_callers['productionChanged'] and alpha_callers['allBuilds'] == 0
    refreshed_prefixes = {
        'Bilinear upscale RGBA8': 'bilinear-Up8',
        'Bicubic RGBA8 enlargement,': 'bicubic-TightUp8',
        'Bicubic RGBA8 width enlargement,': 'bicubic-TightHorizontalUp8',
        'Bicubic RGBA8 height enlargement,': 'bicubic-TightVerticalUp8',
        'Bicubic RGBA8 reduction,': 'bicubic-TightDown8',
        'Bicubic RGBA8 wider-support reduction': 'bicubic-Wide8',
        'Bicubic upscale RGBA16': 'bicubic-Up16',
    }
    reused_prefixes = {
        'Bilinear resize RGBA8': 'bilinear-Down8',
        'Bilinear resize RGBA16': 'bilinear-Down16',
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
        if key in {'bicubic-TightUp8', 'bicubic-TightHorizontalUp8', 'bicubic-TightVerticalUp8'}:
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
    assert refreshed == set(exact_alpha['cases']) | {'bicubic-Up16'}
    assert reused == set(alpha_callers['noCallProof']['cases'])
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
    wide_current = json.loads((ROOT / 'benchmarks/wide-bicubic-independent-repair-evidence.json').read_text(encoding='utf-8'))
    assert wide_current['accepted'] and wide_current['decision']['normalizedSeparated']
    assert wide_current['independentQuietConfirmations'] == len(wide_current['cases']) == 8
    assert wide_current['qualification']['fullChecksEachOs'] == 5376
    assert wide_current['qualification']['package']['sha256'] == wide_current['qualification']['linuxPackage']['sameActualPackageSha256']
    assert list(wide_current['status'].values()).count('confirmed-pass-stop') == 4
    for case, observations in wide_current['cases'].items():
        observation = observations[-1]
        assert len(observation['managed']['actual']) == 12 and all(q['passed'] for q in observation['quietness'].values())
        assert (observation['conservativeRatioUpper'] <= 1) == (wide_current['status'][case] == 'confirmed-pass-stop')
        assert observation['receipt']['managedRationalReferenceExact'] and observation['receipt']['independentOwnedOutput']
        assert observation['receipt']['librarySha256'] == wide_current['producers']['linuxSha256']
    for prefix, case in [('| Bicubic RGBA8 wider-support reduction', 'Wide8'),
                         ('| Bicubic RGBA8, 32×16 / default', 'Down32x16_8')]:
        matches = [i for i, (kind, row) in enumerate(rows) if kind == 'editing' and row.startswith(prefix)]
        assert len(matches) == 1
        index = matches[0]
        label = rows[index][1].split('|')[1].strip()
        observation = wide_current['cases'][case][-1]
        rows[index] = ('editing', f'| {label} | '
                       f'{observation["managed"]["statistics"]["Mean"] / 1000:,.2f} | '
                       f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                       f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                       f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |')
    byte_bands = json.loads((ROOT / 'benchmarks/bicubic-byte-result-bands-evidence.json').read_text(encoding='utf-8'))
    result_bands = json.loads((ROOT / 'benchmarks/bicubic-result-backed-bands-evidence.json').read_text(encoding='utf-8'))
    for depth, current in [(8, byte_bands['cases']['TightMixed8']), (16, result_bands['cases']['TightMixed16'])]:
        rows.append(('editing', f'| Bicubic RGBA{depth}, width up / height down, input+output+4 KiB † → 1027×129 | '
                     f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                     f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |'))
    for depth, name in [(8, 'byte'), (16, 'word')]:
        tight_wide = json.loads((ROOT / f'benchmarks/current-tight-wide-{name}-bicubic-costs-evidence.json').read_text(encoding='utf-8'))
        assert not tight_wide['productionChanged'] and tight_wide['independentQuietConfirmation']
        assert tight_wide['workflow']['allBuilds'] == tight_wide['workflow']['newInputs'] == tight_wide['workflow']['newProfiles'] == 0
        assert tight_wide['qualificationReused']['fullChecksEachOs'] == 5314
        assert set(tight_wide['cases']) == {f'TightWide{depth}', f'Tight64x32_{depth}'}
        for case, observations in tight_wide['cases'].items():
            if case == f'TightWide{depth}':
                continue  # Current wide costs come directly from shared preparation.
            observation = observations[-1]
            receipt, stats = observation['receipt'], observation['managed']['statistics']
            assert receipt['librarySha256'] == tight_wide['producers']['linuxSha256']
            assert receipt['managedRationalReferenceExact'] and receipt['managedQualifiedOriginalPixelsExact']
            assert receipt['independentOwnedOutput'] and receipt['sourceUnchanged']
            assert all(q['passed'] for q in observation['quietness'].values())
            assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
            assert receipt['managedMaxWorkingBytes'] == (513 * 259 + receipt['outputWidth'] * receipt['outputHeight']) * (depth // 2) + 4096
            assert tight_wide['comparisonLowerBounds'][case] > 1 and tight_wide['status'][case] == 'confirmed-gap'
            native = min(b['mean'] for b in observation['nativeBrackets'])
            rows.append(('editing', f'| Bicubic RGBA{depth} reduction, input+output+4 KiB † → {receipt["outputWidth"]}×{receipt["outputHeight"]} | '
                         f'{stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                         f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                         f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
    wide_preparation = json.loads((ROOT / 'benchmarks/bicubic-wide-preparation-evidence.json').read_text(encoding='utf-8'))
    assert wide_preparation['productionAccepted'] and wide_preparation['requiredMaterialPaths'] == 282
    assert set(wide_preparation['cases']) == {'TightWide8', 'TightWide16'}
    assert wide_preparation['design']['workspaceAdmissionBytes'] == 4096
    assert wide_preparation['design']['addedOwnedCapacityBytes'] == 0
    assert wide_preparation['qualification']['fullChecksEachOs'] == 5696
    assert wide_preparation['qualification']['actualPackage']['sha256'] == wide_preparation['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert wide_preparation['qualification']['actualPackage']['wideColumnBothDepthsSharedSurvivorPassed']
    for case, cost in wide_preparation['cases'].items():
        assert cost['actualIterations'] == 12 and cost['managedExact'] and cost['allOriginalGatesPassed'] and cost['quietGuardsPassed']
        assert cost['librarySha256'] == wide_preparation['producers']['linuxSha256']
        assert cost['status'] == 'gap' and cost['lower'] > 1 and cost['allocatedBytes'] == 113
        assert all(v['rawSeparated'] and v['normalizedSeparated'] for v in wide_preparation['independentComparisons'][case]['views'])
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
    for name, c in jpeg_bank['cases'].items():
        assert c['actualIterations'] == c['nativeSamplesEachBracket'] == 12
        assert c['librarySha256'] == jpeg_bank['producers']['linuxSha256']
        assert c['allOriginalGatesPassed'] and c['quietGuardsPassed'] and c['independentOwnedOutput']
        assert c['completePacketExact'] and c['metadataExceptionBytes'] == (0 if name in ('RgbaGray', 'CallerGray') else 2)
        assert c['nativePacketBytes'] - c['managedPacketBytes'] == c['metadataExceptionBytes']
        assert c['status'] == ('pass' if c['upper'] <= 1 else 'gap' if c['lower'] > 1 else 'uncertain')
        assert math.isclose(c['ratio'], c['managedUs'] / c['nativeUs'], rel_tol=1e-12)
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated'] for v in jpeg_bank['comparison']['views'].values())
    assert jpeg_bank['sourceProof']['recordGenerationOwnershipGrowCommitDisposeExact']
    assert jpeg_bank['sourceProof']['writerValidationPollAppendStuffingFlushDisposeByteExact']
    assert jpeg_bank['qualification']['fullChecksEachOs'] == 5943
    assert jpeg_bank['qualification']['actualPackage']['sha256'] == jpeg_bank['qualification']['samePackageLinux']['sameActualPackageSha256']
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
        c = jpeg_bank['cases'][name]
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
    transpose_cost = json.loads((ROOT / 'benchmarks/word-transpose-matrix-evidence.json').read_text(encoding='utf-8'))
    assert transpose_cost['productionChanged'] and transpose_cost['decision']['passed']
    assert transpose_cost['decision']['normalizedSeparated']
    assert transpose_cost['qualification']['fullChecksEachOs'] == 5352
    assert transpose_cost['qualification']['package']['sha256'] == transpose_cost['qualification']['linuxPackage']['sameActualPackageSha256']
    byte_transpose_cost = json.loads((ROOT / 'benchmarks/byte-transpose-matrix-evidence.json').read_text(encoding='utf-8'))
    assert byte_transpose_cost['accepted'] and byte_transpose_cost['decision']['normalizedSeparated']
    assert byte_transpose_cost['qualification']['fullChecksEachOs'] == 5376
    assert byte_transpose_cost['qualification']['package']['sha256'] == byte_transpose_cost['qualification']['linuxPackage']['sameActualPackageSha256']
    assert byte_transpose_cost['reuse']['wordKernelAndEntryUnchanged']
    for case, depth in [('Whole16', 16), ('Whole8', 8)]:
        selected = byte_transpose_cost if depth == 8 else transpose_cost
        observation = selected['cases'][case][-1]
        receipt = observation['receipt']
        assert len(observation['managed']['actual']) == 12 and all(q['passed'] for q in observation['quietness'].values())
        assert receipt['librarySha256'] == selected['producers']['linuxSha256']
        assert receipt['allRgbaSamplesExact'] and receipt['alphaExact'] and receipt['hiddenRgbExact']
        assert receipt['sourceUnchanged'] and receipt['independentOwnedOutput']
        assert observation['conservativeRatioUpper'] < 1
        rows.append(('editing', f'| Plain transpose RGBA{depth} † → 259×513 | '
                     f'{observation["managed"]["statistics"]["Mean"] / 1000:,.2f} | '
                     f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
    # Current owners include explicitly reused costs for unchanged paths.
    jpeg_scan = json.loads((ROOT / 'benchmarks/jpeg-completed-output-evidence.json').read_text(encoding='utf-8'))
    assert jpeg_scan['productionAccepted'] and jpeg_scan['requiredMaterialPaths'] == 283
    assert jpeg_scan['qualification']['fullChecksEachOs'] == 5924
    assert jpeg_scan['qualification']['sameActualPackageBothOs']
    assert jpeg_scan['raw']['mirrored'] and jpeg_scan['raw']['allMembersVerified']
    assert jpeg_scan['sourceProof']['onlyGenericRenderAllocatorAndCommentChanged']
    assert jpeg_scan['sourceProof']['allParsingEntropyTransformsGeometrySamplingColorWritesPollsAndCleanupExact']
    assert sum(not c['reusedUnchangedPath'] for c in jpeg_scan['cases'].values()) == 12
    assert len(jpeg_scan['refreshedCases']) == 12 and len(jpeg_scan['reusedCases']) == 17
    assert jpeg_scan['qualification']['actualPackage']['genericCompletedPlanesOwnershipPassed']
    assert jpeg_scan['qualification']['samePackageLinux']['genericCompletedPlanesOwnershipPassed']
    assert len(jpeg_scan['cases']) == 29
    assert jpeg_scan['statusCounts'] == {state: sum(c['status'] == state for c in jpeg_scan['cases'].values())
                                        for state in ('confirmed-pass-stop', 'confirmed-gap', 'confirmed-confidence-open')}
    for cost in jpeg_scan['cases'].values():
        assert cost['managedActuals'] == cost['nativeSamplesEachBracket'] == 12
        assert cost['quietGuardsPassed'] and cost['allOriginalGatesPassed']
        assert cost['reusedUnchangedPath'] or cost['librarySha256'] == jpeg_scan['producers']['linuxSha256']
        assert cost['status'] == ('confirmed-pass-stop' if cost['upper'] <= 1 else 'confirmed-gap' if cost['lower'] > 1 else 'confirmed-confidence-open')
        assert math.isclose(cost['managedMicroseconds'] / cost['nativeMicroseconds'], cost['ratio'], rel_tol=1e-12)
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated']
               for v in jpeg_scan['comparison']['ProgressiveGray']['views'].values())
    assert jpeg_scan['comparison']['ProgressiveGray']['allocationReductionBytes'] == 531744
    replacements = [('NativeDecode420', 'JPEG baseline420, resident decode'), ('CmykCaller', 'JPEG DC-only, caller CMYK decode'), ('CmykAdobeSparse2D', 'JPEG CMYK sparse-2D, resident decode'), ('Ycck420', 'JPEG YCCK420, resident decode'), ('Decode411', 'JPEG 4:1:1, resident decode'), ('Decode410', 'JPEG 4:1:0, resident decode'), ('CmykAdobeGeneral', 'JPEG dense CMYK, resident decode'), ('CmykAdobeAc', 'JPEG horizontal-only CMYK, resident decode'), ('Ycck420CallerRgb', 'JPEG YCCK420, caller RGB24 decode')]
    replacements += [('ProgressiveGray', 'JPEG progressive grayscale, resident decode'),
                     ('Progressive420', 'JPEG progressive420, resident decode'),
                     ('Progressive422', 'JPEG progressive422, resident decode'),
                     ('Progressive444', 'JPEG ordinary progressive444, resident decode'),
                     ('ProgressiveCancelable444', 'JPEG progressive444, live-token resident decode')]
    for case, label in replacements:
        c = jpeg_scan['cases'][case]
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
    allocation_only = json.loads((ROOT / 'benchmarks/text-drawing-allocation-controls-evidence.json').read_text(encoding='utf-8'))
    assert allocation_only['allocationOnly'] and not allocation_only['timingClaim'] and not allocation_only['comparativeBaselineUsed']
    assert allocation_only['quietGuardsPassed'] and allocation_only['remainingAuditedWithoutElapsedTime'] == 9
    allocation_values = {c['caseId']: c['allocatedBytesPerOperation'] for c in allocation_only['observations']['cases']}
    assert len(allocation_values) == 9
    assert all(c['sampleValues'] == [c['allocatedBytesPerOperation']] * 3 and c['operationsPerSample'] == 16
               for c in allocation_only['observations']['cases'])
    assert allocation_values['DrawingComposite8'] == allocation_values['DrawingComposite16']
    assert allocation_values['DrawText8'] == allocation_values['DrawText16']
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
    raster = json.loads((ROOT / 'benchmarks/text-interior-coverage-evidence.json').read_text(encoding='utf-8'))
    assert raster['productionAccepted'] and raster['independentQuietConfirmation']
    assert raster['qualification']['fullChecksEachOs'] == 5911
    assert raster['qualification']['actualPackage']['sha256'] == raster['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert raster['decision']['cpuGainInconclusive']
    assert raster['sourceProof']['onlyBandCoverageLoopAndTwoInteriorHelpersChanged']
    assert raster['sourceProof']['clippingAreaOrderGeometryEventsSortBlendAdmissionAndPublicBoundaryUnchanged']
    assert raster['sourceProof']['sameSequentialPerPixelAdditionsAndPolls']
    assert raster['rawArchive']['mirrored'] and raster['rawArchive']['allMembersVerified']
    for case_id, previous in text_current['observations'].items():
        case = (font_load['current'] if case_id == 'FontLoadRegular'
                else raster['cases'].get(case_id, composites['cases'].get(case_id, previous)))
        if case_id in raster['cases']:
            assert case['librarySha256'] == raster['producers']['linuxSha256']
            assert case['validation']['frozenManagedObservationsExact']
            assert case['managedMemory']['BytesAllocatedPerOperation'] == 67368
            assert case['admittedWorkspacePeak'] == 48544 and case['retainedDrawingWorkspace'] == 0
        if case_id in composites['cases']:
            assert case['librarySha256'] == composites['producers']['linuxSha256']
            assert case['validation']['exactManagedPixels'] and case['managedMemory']['BytesAllocatedPerOperation'] == 56
        assert len(case['managed']['actual']) == 12 and case['quietGuardsPassed']
        assert case['approvedScopeDate'] == '2026-11-02'
        native_mean = min(bracket['mean'] for bracket in case['nativeBrackets'])
        status = 'pass' if case['status'] == 'Confirmed pass' else 'gap' if case['status'] == 'Confirmed gap' else 'confidence open'
        mark = ' †' if case_id == 'FontLoadRegular' or case_id in raster['cases'] else ''
        rows.append(('editing', f'| {case["label"]} ({status}){mark} | {case["managed"]["statistics"]["Mean"] / 1000:,.2f} | '
                     f'{native_mean / 1000:,.2f} | {case["worstBracketRatio"]:.3f} / {case["conservativeRatioUpper"]:.3f} | '
                     f'{case["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
    lanczos_current = json.loads((ROOT / 'benchmarks/lanczos-paired-certificate-preparation-evidence.json').read_text(encoding='utf-8'))
    assert lanczos_current['productionAccepted'] and lanczos_current['selectedStage'] == 1
    assert lanczos_current['pairedComparison']['pairs'] == 12 and lanczos_current['pairedComparison']['ratioConfidence'][1] < 1
    assert lanczos_current['qualification']['fullChecksEachOs'] == 5631
    assert lanczos_current['qualification']['actualPackage']['sha256'] == lanczos_current['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert len(lanczos_current['cases']) == 8
    assert sum(c['status'] == 'pass' for c in lanczos_current['cases'].values()) == 5
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
        observation = lanczos_current['cases'][case]
        assert observation['managedExact'] and observation['allOriginalGatesPassed'] and observation['quietGuardsPassed']
        assert observation['librarySha256'] == lanczos_current['producers']['linuxSha256']
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
        if kind != 'editing' or not row.startswith('| Bicubic'):
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
             else row) for kind, row in rows]
    labels = {'Down8': 'Bicubic resize RGBA8', 'Up16': 'Bicubic upscale RGBA16',
              'Down32x16_8': 'Bicubic RGBA8, 32×16 / default workspace',
              'Up8': 'Bicubic upscale RGBA8, default workspace'}
    # One unchanged-build confirmation resolves default-byte uncertainty;
    # other enlargement cases keep their source-valid current owners.
    default_byte = json.loads((ROOT / 'benchmarks/bicubic-byte-enlargement-current-evidence.json').read_text(encoding='utf-8'))
    assert not default_byte['productionChanged'] and default_byte['allBuilds'] == 0
    assert default_byte['raw']['mirrored'] and default_byte['raw']['allMembersVerified']
    assert default_byte['cases']['Up8']['status'] == 'gap' and default_byte['cases']['Up8']['lower'] > 1
    assert default_byte['qualificationReused']['sameActualPackageBothOs']
    assert set(overwrite['cases']) == set(labels)
    for case, label in labels.items():
        owner = default_byte if case == 'Up8' else enlargement if case == 'Up16' else overwrite
        observation = owner['cases'][case]; receipt = observation['receipt']
        assert receipt['librarySha256'] == owner['producers']['linuxSha256']
        assert receipt['managedRationalReferenceExact'] and observation['quietGuardsPassed']
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        if case not in {'Up8', 'Up16'}:
            assert (observation['conservativeRatioUpper'] <= 1) == (case == 'Down32x16_8')
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
    # Current WebP costs use scoped unchanged-path reuse. Older reports
    # above retain their source-specific proofs; do not copy their experiment
    # histories or substitute old costs for this accepted stage's current rows.
    webp_owned = json.loads((ROOT / 'benchmarks/webp-matching-population-storage-evidence.json').read_text(encoding='utf-8'))
    assert webp_owned['productionAccepted']
    qualification = webp_owned['qualification']
    assert qualification['fullChecksEachOs'] == qualification['windows']['total'] == qualification['linux']['total']
    assert qualification['windows']['fullLivePassed'] and qualification['linux']['fullLivePassed']
    assert qualification['actualPackage']['sha256'] == qualification['samePackageLinux']['sameActualPackageSha256']
    assert qualification['actualPackage']['actualConsumerPassed'] and qualification['samePackageLinux']['actualConsumerPassed']
    assert webp_owned['raw']['mirrored'] and webp_owned['raw']['allMembersVerified']
    assert set(webp_owned['cases']) == {'ordinary', 'opaque', 'photograph', 'diagram', 'rgba16', 'async', 'alpha', 'alpha16'}
    for current in webp_owned['cases'].values():
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['sourceUnchanged'] and current['independentOwnedOutput'] and current['completePacketExact']
        assert current['librarySha256'] == webp_owned['producers']['linuxSha256']
        assert current['packetSha256'] == current['nativePacketSha256']
        assert current['managedPacketBytes'] == current['nativePacketBytes']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    streaming = json.loads((ROOT / 'benchmarks/bicubic-shared-streaming-evidence.json').read_text(encoding='utf-8'))
    assert streaming['productionAccepted'] and streaming['design']['sharedDriversAfter'] == 2
    assert streaming['qualification']['fullChecksEachOs'] == 5587
    assert streaming['qualification']['actualPackage']['sha256'] == streaming['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert len(streaming['cases']) == 8 and sum(c['status'] == 'pass' for c in streaming['cases'].values()) == 6
    for current in streaming['cases'].values():
        assert current['actualIterations'] == 12 and current['managedExact'] and current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['librarySha256'] == streaming['producers']['linuxSha256']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    assert all(v['rawSeparated'] and v['normalizedSeparated'] for row in streaming['independentComparisons'] for v in row['views'].values())
    current_labels = {
        'ordinary': 'VP8L RGBA8 / libwebp exact=1',
        'photograph': 'VP8L photograph / libwebp exact=1',
        'diagram': 'VP8L text/diagram / libwebp exact=1',
        'opaque': 'VP8L opaque RGBA8 / libwebp exact=1',
        'rgba16': 'VP8L RGBA16, including normalization / libwebp exact=1',
        'async': 'VP8L async, fresh nonseekable output / libwebp exact=1',
    }
    assert set(current_labels) <= set(webp_owned['cases'])
    for case, label in current_labels.items():
        owner = webp_owned
        current = owner['cases'][case]
        positions = [i for i, (_, row) in enumerate(rows) if row.split('|')[1].strip().removesuffix(' H').removesuffix(' †') == label]
        assert len(positions) == 1, label
        rows[positions[0]] = ('codec', f'| {label} † | {current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} | '
                             f'{current["managedPacketBytes"]:,} / {current["nativePacketBytes"]:,} |')
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
        c = vp8_current['cases'][case]
        assert not any(row.split('|')[1].strip().removesuffix(' H').removesuffix(' †') == label for _, row in rows)
        vp8_rows.append(('codec', f'| {label} † | {c["managedUs"]:,.2f} | {c["nativeUs"]:,.2f} | '
            f'{c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | '
            f'{c["managedPacketBytes"]:,} / {c["nativePacketBytes"]:,} |'))
    rows[position:position] = vp8_rows
    vp8_status_counts = {state: sum(vp8_current['cases'][case]['status'] == state
                                  for case in vp8_current['cases'])
                         for state in ('pass', 'gap', 'uncertain')}
    selected_cost = vp8_current['cases']['selected']
    distortion_cost = vp8_current['cases']['distortion']
    assert selected_cost['managedPacketBytes'] == 7026
    stream = lpf['Vp8Stream']
    rows.insert(0, ('codec', f'| WebP VP8 + alpha, fresh seekable Stream (parity confirmed) † | {stream["managedUs"]:,.2f} | '
        f'{stream["nativeUs"]:,.2f} | {stream["ratio"]:.3f} / {stream["upper"]:.3f} | '
        f'{stream["allocatedBytes"]:,} | — |'))
    bilinear = json.loads((ROOT / 'benchmarks/bilinear-current-boundary-evidence.json').read_text(encoding='utf-8'))
    current = bilinear['current']
    assert not bilinear['productionChanged'] and bilinear['allBuilds'] == 0
    assert bilinear['raw']['mirrored'] and bilinear['raw']['allMembersVerified']
    assert current['librarySha256'] == current['receipt']['librarySha256']
    assert current['actualIterations'] == 12 and current['allOriginalGatesPassed'] and current['quietGuardsPassed']
    assert current['receipt']['managedRationalReferenceExact'] and current['receipt']['independentOwnedOutput']
    assert current['lower'] < 1 < current['upper'] and current['status'] == 'uncertain'
    positions = [i for i, (_, row) in enumerate(rows) if row.startswith('| Bilinear upscale RGBA8 ')]
    assert len(positions) == 1
    rows[positions[0]] = ('editing', f'| Bilinear upscale RGBA8 † → 1027×519 | {current["managedUs"]:,.2f} | '
        f'{current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |')
    column = json.loads((ROOT / 'benchmarks/bicubic-strided-column-cache-evidence.json').read_text(encoding='utf-8'))
    current = column['current']
    assert column['productionAccepted'] and column['requiredMaterialPaths'] == 283
    assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
    assert current['managedRationalReferenceExact'] and current['quietGuardsPassed'] and current['allOriginalGatesPassed']
    assert current['status'] == 'gap' and current['lower'] > 1
    cq = column['qualification']
    assert current['librarySha256'] == cq['actualPackage']['librarySha256'] == column['producers']['windowsSha256']
    assert cq['actualPackage']['sha256'] == cq['samePackageLinux']['sameActualPackageSha256']
    assert cq['fullChecksEachOs'] == cq['windows']['total'] == cq['linux']['total'] == 5973
    assert cq['allThreeExamplesBothOs'] and cq['byteColumnStripsOwnershipBothOs']
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated'] for v in column['comparison']['views'].values())
    assert column['sourceProof']['oneOriginalNumericLoopExactAfterRangeStrideCapacitySubstitution']
    assert column['sourceProof']['originalByteAndReciprocalCertificateVerbatim']
    assert column['storage']['rootWorkspaceBytes'] == 4096 and column['storage']['newOwnedCapacityBytes'] == 0
    assert column['raw']['mirrored'] and column['raw']['allMembersVerified']
    positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing'
                 and row.startswith('| Bicubic RGBA8 reduction, input+output+4 KiB ')
                 and '→ 256×129 |' in row]
    assert len(positions) == 1
    rows[positions[0]] = ('editing', f'| Bicubic RGBA8 reduction, input+output+4 KiB † → 256×129 | '
                         f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                         f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |')
    word_column = json.loads((ROOT / 'benchmarks/bicubic-word-column-preparation-evidence.json').read_text(encoding='utf-8'))
    current = word_column['cases']['TightDown16']
    assert word_column['productionAccepted'] and current['actualIterations'] == 12
    assert current['managedExact'] and current['quietGuardsPassed'] and current['allOriginalGatesPassed']
    assert current['librarySha256'] == word_column['producers']['linuxSha256']
    assert word_column['qualification']['actualPackage']['sha256'] == word_column['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert all(v['rawSeparated'] and v['normalizedSeparated'] for v in word_column['independentComparison']['views'])
    rows.insert(positions[0] + 1, ('editing', f'| Bicubic RGBA16 reduction, input+output+4 KiB † → 256×129 | '
                                f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                                f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} |'))
    current = png_framing['currentSharedRoot']
    assert current['actualIterations'] == len(current['managedActualNanoseconds']) == 12
    assert current['librarySha256'] == png_framing['producers']['linuxSha256']
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
        if current['caseId'] == 'TightVertical16':
            continue  # Refreshed by identity geometry below.
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['librarySha256'] == word_prep['producers']['linuxSha256']
        assert current['managedRationalReferenceExact'] and current['sourceUnchanged'] and current['independentOwnedOutput']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    word_labels = {'Up16': 'Bicubic upscale RGBA16',
                   'TightCross16': 'Bicubic RGBA16, width down / height up / input+output+4 KiB',
                   'TightMixed16': 'Bicubic RGBA16, width up / height down, input+output+4 KiB',
                   'Cross16': 'Bicubic RGBA16, width down / height up, default workspace'}
    for case, label in word_labels.items():
        current = word_prep['cases'][case]
        positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing' and row.startswith('| ' + label + ' ')]
        assert len(positions) == 1, label
        rows[positions[0]] = ('editing', f'| {label} † → {current["outputWidth"]}×{current["outputHeight"]} | '
            f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | '
            f'{current["allocatedBytes"]:,} |')
    vq = word_prep['qualification']
    assert vq['fullChecksEachOs'] == vq['windows']['total'] == vq['linux']['total'] == 5866
    assert vq['actualPackage']['sha256'] == vq['samePackageLinux']['sameActualPackageSha256']
    assert vq['actualPackage']['actualConsumerPassed'] and vq['samePackageLinux']['actualConsumerPassed']
    assert result_bands['productionAccepted'] and result_bands['requiredMaterialPaths'] == 283
    assert result_bands['raw']['mirrored'] and result_bands['raw']['allMembersVerified']
    assert set(result_bands['cases']) == {'Mixed16', 'TightMixed16', 'TightVertical16'}
    assert result_bands['sourceProof']['sharedLoopTokensExactAfterRangeAndCheckpointSubstitution']
    assert result_bands['sourceProof']['originalWordTokensExact']
    for current in result_bands['cases'].values():
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['librarySha256'] == result_bands['producers']['linuxSha256']
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['managedRationalReferenceExact'] and current['sourceUnchanged'] and current['independentOwnedOutput']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated']
               for c in result_bands['comparison'].values() for v in c['views'].values())
    current = result_bands['cases']['TightMixed16']
    label = word_labels['TightMixed16']
    positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing' and row.startswith('| ' + label + ' ')]
    assert len(positions) == 1
    rows[positions[0]] = ('editing', f'| {label} † → {current["outputWidth"]}×{current["outputHeight"]} | '
        f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | '
        f'{current["allocatedBytes"]:,} |')
    current = result_bands['cases']['TightVertical16']
    assert current['status'] == 'pass'
    rows.append(('editing', f'| Bicubic RGBA16 height reduction, input+output+4 KiB (pass) † → {current["outputWidth"]}×{current["outputHeight"]} | '
                 f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | '
                 f'{current["allocatedBytes"]:,} |'))
    bq = result_bands['qualification']
    assert bq['fullChecksEachOs'] == bq['windows']['total'] == bq['linux']['total'] == 5948
    assert bq['actualPackage']['sha256'] == bq['samePackageLinux']['sameActualPackageSha256']
    assert bq['actualPackage']['largeBandsExactAndAdmittedPassed'] and bq['samePackageLinux']['largeBandsExactAndAdmittedPassed']
    merged_word = {**word_prep['cases'], **result_bands['cases']}
    png = json.loads((ROOT / 'benchmarks/png-length-classification-evidence.json').read_text(encoding='utf-8'))
    assert png['productionAccepted'] and png['requiredMaterialPaths'] == 283
    pq = png['qualification']
    assert pq['fullChecksEachOs'] == pq['windows']['total'] == pq['linux']['total'] == 5907
    assert pq['actualPackage']['sha256'] == pq['samePackageLinux']['sameActualPackageSha256']
    assert pq['actualPackage']['actualConsumerPassed'] and pq['samePackageLinux']['actualConsumerPassed']
    assert pq['sameActualPackageBothOs']
    assert pq['actualPackage']['fastestPngExactByteWordSyncAsyncAdmissionPassed']
    assert pq['samePackageLinux']['fastestPngExactByteWordSyncAsyncAdmissionPassed']
    assert set(png['cases']) == {'Encode8', 'Encode16', 'EncodeFast8', 'EncodeFast16',
        'EncodeSmallest8', 'EncodeSmallest16', 'EncodeGray8', 'EncodeRgb8',
        'EncodeGrayAlpha8', 'EncodeGray16', 'EncodeRgb16', 'EncodeGrayAlpha16',
        'EncodeFastCancelable16', 'EncodeFastForward16', 'Png8AsyncNonSeekable'}
    assert png['storage']['workspaceAdmissionBytes'] == 393216 and png['storage']['retainedOperationScratchBytes'] == 0
    assert png['sourceProof']['wholeSourceUndoReconstructsOriginalOutsideExactLengthClassifier']
    assert png['sourceProof']['all256LegalLengthMappingsExact']
    assert png['sourceProof']['originalTokenOrderTreesBlockChoiceBitOutputPollsOwnershipAndFieldLayoutExact']
    assert not png['materialReview']['separatedRegressions']
    assert {t: sum(c['status'] == t for c in png['cases'].values()) for t in ['pass', 'gap', 'uncertain']} == png['statusCounts']
    rows = [(kind, row.replace(' † |', ' † H |', 1)
             if kind == 'codec' and row.startswith('| PNG ') and 'decode' not in row.lower()
             else row) for kind, row in rows]
    def png_label(label):
        return label.replace(' † H', '').replace(' †', '').replace(', one-shot ', ', ').replace(', red-plane reference', '')
    for case, current in png['cases'].items():
        assert current['caseId'] == case
        assert not current['reusedUnchangedPath']
        assert current['librarySha256'] == png['producers']['linuxSha256']
        assert current['allOriginalGatesPassed'] and current['quietGuardsPassed'] and current['completePacketExact']
        assert current['actualIterations'] == len(current['managedActualNanoseconds']) == 12
        assert current['nativeSamplesEachBracket'] == 12
        assert current['packetSha256'] == current['nativePacketSha256'] and current['packetBytes'] == current['nativePacketBytes']
        assert current['equivalentOneShotBoundary'] and current['codecPreparationTimed'] and not current['nativeCodecRetainedBetweenOperations']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
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
            current = png_framing['currentSharedRoot']
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
    merged_word.update({case: cost for case, cost in overlap['cases'].items() if case in merged_word})
    assert result_bands['mergedWordStatusCounts'] == {state: sum(c['status'] == state for c in merged_word.values())
                                                    for state in ('pass', 'gap', 'uncertain')}
    overlap_labels = {'Down8': 'Bicubic resize RGBA8',
                      'Down32x16_8': 'Bicubic RGBA8, 32×16 / default workspace',
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
        label = overlap_labels[case]
        positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing' and row.startswith('| ' + label + ' ')]
        assert len(positions) == 1, label
        rows[positions[0]] = ('editing', f'| {label} † → {current["outputWidth"]}×{current["outputHeight"]} | '
            f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | '
            f'{current["allocatedBytes"]:,} |')
    header = '| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |'
    separator = '| --- | ---: | ---: | ---: | ---: |'
    codecs = '\n'.join([header + ' Packet B, Lokad / reference |', separator + ' ---: |',
                         *(row for kind, row in rows if kind == 'codec')])
    read_labels = ['Probe PNG dimensions, seekable', 'Probe JPEG dimensions, seekable',
                   'Probe WebP dimensions, seekable', 'Probe JPEG dimensions, async nonseekable',
                   'Validate PNG8, span', 'Validate PNG16, span', 'Validate indexed Adam7 19×17, span',
                   'Validate JPEG baseline420, span', 'Validate JPEG progressive420, span',
                   'Validate VP8L+alpha, span', 'Validate VP8+alpha, span',
                   'Validate PNG16, async nonseekable']
    assert byte_bands['productionAccepted'] and byte_bands['requiredMaterialPaths'] == 283
    assert byte_bands['raw']['mirrored'] and byte_bands['raw']['allMembersVerified']
    assert set(byte_bands['cases']) == {'Mixed8', 'TightMixed8', 'TightVertical8'}
    bq = byte_bands['qualification']
    assert bq['fullChecksEachOs'] == bq['windows']['total'] == bq['linux']['total'] == 5964
    assert bq['actualPackage']['sha256'] == bq['samePackageLinux']['sameActualPackageSha256']
    assert bq['packageByteBandsBothOs'] and bq['allThreeExamplesBothOs']
    assert all(v['rawGainSeparated'] and v['normalizedGainSeparated'] for v in byte_bands['comparison']['views'].values())
    for current in byte_bands['cases'].values():
        assert current['actualIterations'] == current['nativeSamplesEachBracket'] == 12
        assert current['managedQualifiedOriginalPixelsExact'] and current['allOriginalGatesPassed'] and current['quietGuardsPassed']
        assert current['certifiedExactRationalAlphaHalves'] == 25 if current['caseId'] == 'TightVertical8' else current['managedRationalReferenceExact']
        assert current['sourceUnchanged'] and current['independentOwnedOutput']
        assert current['librarySha256'] == bq['actualPackage']['librarySha256'] == byte_bands['producers']['windowsSha256']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
    current = byte_bands['cases']['TightMixed8']
    label = 'Bicubic RGBA8, width up / height down, input+output+4 KiB'
    positions = [i for i, (kind, row) in enumerate(rows) if kind == 'editing' and row.startswith('| ' + label + ' ')]
    assert len(positions) == 1
    rows[positions[0]] = ('editing', f'| {label} † → 1027×129 | '
        f'{current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | {current["ratio"]:.3f} / {current["upper"]:.3f} | '
        f'{current["allocatedBytes"]:,} |')
    editing = '\n'.join([header, separator, *(row for kind, row in rows if kind == 'editing')])
    read_rows = []
    refreshed_png_reads = set()
    for label, (case, observations) in zip(read_labels, reads['cases'].items(), strict=True):
        if case in png_framing['cases']:
            current = png_framing['cases'][case]
            assert current['descriptorExact'] and current['prefixAndCodingScopesVerified'] and current['outputBytes'] == 36
            status = 'Confirmed pass' if current['status'] == 'pass' else 'Confirmed gap' if current['status'] == 'gap' else 'Confidence open'
            read_rows.append(f'| {label} † | {current["managedUs"]:,.3f} | {current["nativeUs"]:,.3f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} | {status} |')
            refreshed_png_reads.add(case)
            continue
        if case in {'ValidateJpegBaseline420', 'ValidateJpegProgressive420'}:
            current = jpeg_scan['cases'][case]
            assert current['descriptorSha256'] and current['quietGuardsPassed'] and current['allOriginalGatesPassed']
            assert current['managedActuals'] == current['nativeSamplesEachBracket'] == 12
            status = 'Confirmed pass' if current['status'] == 'confirmed-pass-stop' else 'Confirmed gap' if current['status'] == 'confirmed-gap' else 'Confidence open'
            read_rows.append(f'| {label} | {current["managedMicroseconds"]:,.3f} | {current["nativeMicroseconds"]:,.3f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} | {status} |')
            continue
        if case == 'ValidateVp8Alpha':
            current = prepared_model['costs'][case]
            assert current['descriptorExact'] and current['prefixAndCodingScopesVerified'] and current['managedOutputBytes'] == 36
            status = 'Confirmed pass' if current['status'] == 'pass' else 'Confirmed gap' if current['status'] == 'gap' else 'Confidence open'
            read_rows.append(f'| {label} † | {current["managedUs"]:,.3f} | {current["nativeUs"]:,.3f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} | {status} |')
            continue
        producer = (png_probe if case in png_probe['cases'] else header_skips if case in header_skips['cases'] else resident_decode if case == 'ValidateVp8lAlpha' else png_gfni if case in png_gfni['cases'] else baseline_blocks if case == 'ValidateJpegBaseline420' else png_average if case in png_average['cases'] else png_crc_scan if case in png_crc_scan['cases'] else vp8_validation if case in vp8_validation['cases'] else png_cursor if case in png_cursor['cases']
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
    read_table = '\n'.join([header + ' Status |', separator + ' --- |', *read_rows])
    displayed_count = sum(len(table.splitlines()) - 2 for table in (codecs, editing, read_table))
    return f"""# Lokad.Image benchmarks

Updated 2026-11-09. These **{displayed_count} selected comparisons** combine qualified
**0.2.1 development** costs with retained **0.2.0** evidence. Full parity remains
open across the [283 required paths](../benchmarks/WARM-COVERAGE.md). **†** marks
development costs; **H** marks affected historical costs that are not current
performance proof. Publication is postponed.

All measurements use the dedicated quiet Ubuntu x64/AMD EPYC 9V45 VM, sequential
warm operations on one pinned CPU, .NET 10 with tiering off and nonconcurrent
workstation GC. Times are **µs per complete operation**. Ratios use the faster
native bracket; **upper** includes conservative filtered/all-actual confidence.
Short screens do not establish independent parity. **B/op** is total managed
allocation, separate from peak live capacity and retained storage. Native
allocations are unmeasured. Normalization, output ownership and disposal stay
timed; setup, delivery and retained state follow the stated case boundaries.

Current scope and remaining work:

- **WebP decoding:** [fused final inverse/publication](../benchmarks/VP8L-FINAL-PUBLICATION.md)
  saves **155.66µs/4.79% raw**,4.53% native-normalized, on complete resident VP8L.
  Both confidence views separate. GC stays152,728B/op; no new owned capacity/cache.
  Current resident/live/stream uppers{lpf['WebpResident']['upper']:.3f}/
  {lpf['NativeLosslessCancelable8']['upper']:.3f}/{lpf['WebpStream']['upper']:.3f} remain gaps.
  Six unchanged lossless controls:2pass/4gaps. The contemporary palette
  control regresses **19.25µs/3.46%**, with separated raw/normalized intervals;
  retain that tradeoff, not a general decoder speedup claim.
  Validation passes/stop; source-valid coarse peaks235,152/11,121B and retained0
  are reused, not remeasured physical peaks. Full output retains531,468B until disposal.
  [Frame-private quantizers](../benchmarks/VP8-FRAME-QUANTIZER-PREPARATION.md)
  prepare six scalar dequantizers per segment once inside complete decoding,
  using96B within the existing8KiB allowance. Live opaque saves45.36µs/0.92%raw;
  raw confidence separates in both views, while0.86% normalized remains a point
  gain with all-actual uncertainty. Resident alpha saves110.85µs/1.83%raw/
  1.82%normalized with separated confidence. Contemporary originals resolve the
  resident historical-clock contradiction; no favorable repeats. Current resident
  {lpf['Native8']['managedUs']:,.2f}/native{lpf['Native8']['nativeUs']:,.2f}µs,
  upper{lpf['Native8']['upper']:.3f} is uncertain. Stream upper{stream['upper']:.3f}
  and validation upper{lpf['ValidateVp8Alpha']['upper']:.3f} pass/stop. Six current
  costs:{prepared_model['statusCounts']['pass']}pass/{prepared_model['statusCounts']['gap']}gaps/{prepared_model['statusCounts']['uncertain']}uncertain.
  Live opaque remains{live['managedUs']:,.2f}/native{live['nativeUs']:,.2f}µs,
  upper{live['upper']:.3f}gap,{live['allocatedBytes']:,}B/op. All allocation counts
  are unchanged; no new heap/retention. Existing1,536B probability model and
  independently retained531,468B frames remain. Physical/native peaks unmeasured;
  logical fixed payload/coarse allowance5,108B fits8,192B. Syntax, every original
  poll, cursor/pixels, admission and actual async lifetimes remain. Both OSes pass
  5,955full/live, normal/scalar/disabled-ISA and SAME package/three examples.
  [Model rows](../benchmarks/VP8-MODEL-ROW-STRIDE.md),
  [earlier preparation](../benchmarks/VP8-PREPARED-COEFFICIENT-MODEL.md) and
  [alpha publication](../benchmarks/WEBP-HORIZONTAL-ALPHA-PUBLICATION.md)
  retain their historical gains and raw evidence separately.
- **WebP encoding:** [matching-to-population storage](../benchmarks/WEBP-MATCHING-POPULATION-STORAGE.md)
  saves134,736B/encode on photograph/diagram and70,560B on transparent alpha.
  Separate admitted peaks remain3,470,059/3,251,328/3,656,309B; retained scratch zero.
  Six unchanged lossless comparisons reuse that evidence; photograph/opaque
  remain uncertain. No causal CPU-gain claim for that storage change.
  [Rounded-border prediction](../benchmarks/VP8-ROUNDED-BORDER-PREDICTION.md)
  saves **{vp8_current['comparison']['savedUs']:.2f}µs/{100*vp8_current['comparison']['rawReduction']:.2f}% raw**,
  {100*vp8_current['comparison']['normalizedPointReduction']:.2f}% reference-normalized, on complete effort2.
  Both filtered/all-actual confidence views separate. GC remains406,705B/op;
  no new owned heap capacity or retained state. Current
  {distortion_cost['managedUs']:,.2f}/native{distortion_cost['nativeUs']:,.2f}µs,
  upper{distortion_cost['upper']:.3f} is uncertain, not a parity pass or demonstrated gap.
  Eight unchanged encode costs reuse [histogram-stage evidence](../benchmarks/VP8-FOUR-BLOCK-HISTOGRAM.md);
  shared RDO/decoder prediction code is untouched. Combined lossy scope:
  {vp8_status_counts['pass']}pass/{vp8_status_counts['gap']}gaps/{vp8_status_counts['uncertain']}uncertain.
  Exact complete packets remain unchanged; passing controls stop.
- **PNG:** [ordinary/slab cursor publication](../benchmarks/PNG-NORMAL-CURSOR-PUBLICATION.md)
  removes exception-only publication of abandoned private state. Completed cursors,
  public errors/partial stores, lookahead, polls and owner cleanup remain exact.
  Tight/default RGBA16, RGB16 and tight-live RGBA16 save **192/125/216/217µs**;
  all four former gaps now pass. The two-image shared-root operation saves260µs:
  2610.86/native2847.04µs, upper0.920, passes. Filtered/allactual raw/normalized
  intervals separate. Its admitted peak1,766,641B, retained pair1,062,936B and
  GC1,638,214B/op remain unchanged; all51 inventory allocations are unchanged.
  [Current framing controls](../benchmarks/PNG-CURRENT-FRAMING-COSTS.md):
  {len(png_framing['cases'])} confirmed controls, {png_framing['statusCounts']['pass']} pass/stop,
  {png_framing['statusCounts']['gap']} gap and {png_framing['statusCounts']['uncertain']} uncertain.
  The publication stage refreshed43 affected costs and retained eight unchanged
  providers. The current encoding change preserves these decode paths.
  Live materialized RGBA16 remains2919.94/native2894.87µs upper1.012, a gap;
  liveRGBA8 upper1.004 remains uncertain. No favorable resampling or global parity
  claim. Header-only work is unchanged; older stream/composition
  costs remain historical. Physical/native peaks are not remeasured. Earlier
  [prefix](../benchmarks/PNG-PREPARED-PREFIX.md) and
  [live-cursor](../benchmarks/PNG-LIVE-PREPARED-CURSOR.md) gains remain linked history.
  [Exact length classification](../benchmarks/PNG-LENGTH-CLASSIFICATION.md) replaces
  repeated range search with the exact integer-logarithm formula. All15 current
  one-shot encodes refresh: **{png['statusCounts']['pass']} pass/stop, {png['statusCounts']['gap']} gaps,
  {png['statusCounts']['uncertain']} uncertain**. WordSmallest now passes. ByteFastest
  upper1.017 and Gray8 upper1.047 remain uncertain; resolve before assuming tuning.
  WordFastest saves180.09µs/1.00% raw with separated filtered/allactual intervals;
  normalized intervals overlap. Byte's84.29µs point gain has allactual uncertainty.
  No robust normalized or blanket gain claim. Exact packets,384KiB admission,
  retained0scratch and primaryGC820916/2437660Bop remain. Earlier
  [streaming census](../benchmarks/PNG-STREAMING-TOKEN-FREQUENCIES.md),
  [lazy](../benchmarks/PNG-LAZY-VIEW-PREPARATION.md) and
  [greedy preparation](../benchmarks/PNG-GREEDY-VIEW-PREPARATION.md) retain history.
- **Resizing:** [Strided column workspace](../benchmarks/BICUBIC-STRIDED-COLUMN-CACHE.md)
  saves **571.36→552.17µs /19.18µs/3.36%raw/3.30%normalized**, with separated
  filtered/all-actual raw/normalized confidence. Current zimg417.53µs,upper1.337
  remains a gap;113B/op, no added owned capacity/retained scratch. One numeric
  range serves a16column borrowed-output prefix and original8/6column suffix;
  separate non-inlined stack frames share one4KiB allowance and retire before
  overlap/replay. Both OSes pass5,973full/live, scalar/ISA, SAME actual package/
  column ownership checks and allthree examples. Other source-valid paths reuse
  evidence; this is one scoped control, not all-path parity.
  [Result-backed byte bands](../benchmarks/BICUBIC-BYTE-RESULT-BANDS.md)
  reduce tight mixed **1,130.05→980.90µs /149.15µs/13.20%raw/13.02%normalized**,
  with separated filtered/all-actual raw/normalized confidence. Current exact
  package DLL: default/tight mixed960.04/980.90µs, uppers1.116/1.142 remain gaps;
  vertical503.50/zimg587.18µs upper.862 passes/stops. Allocations114/114/113Bop,
  no added owned capacity/retained scratch;28,136B borrows existing output and
  retires before suffix writes. Existing4KiB admission and exact replay remain.
  The earlier1,319.86µs point overstates the contemporary gain; cross-artifact
  timing differences remain unexplained despite identical sources/configuration
  and targeted IL. Current claims use matched Windows-produced artifacts and
  the exact actual package DLL. Both OSes pass5,964full/live, affected scalar/ISA,
  SAME actual package/byte-band ownership checks and allthree examples.
  [Direct identity-axis cache rows](../benchmarks/BICUBIC-IDENTITY-CACHE-ROWS.md)
  close byte/word vertical reduction: **636.49→530.97µs / 16.58% raw / 16.50%
  normalized** and **666.59→511.76µs / 23.23% raw / 21.26% normalized**.
  Filtered/all-actual confidence separates; parity uppers0.908/0.904.
  The previous vertical-word regression is superseded. Nine refreshed cached
  controls:7pass/1gap/1uncertain. Byte reduction remains485.10/native416.46µs,
  upper1.169; horizontal word is uncertain (upper1.010). Passing cases stop.
  Identity workspace383,852→367,436B; total allocation367,675/367,595Bop,
  retainedscratch0/result264,708/529,416B until disposal. Exact repair's4KiB
  overlap stays separately admitted. Physical/native peaks are unmeasured;
  existing output-pool retention remains separate. No new API/owner/cache/pool.
  Native zimg retains graph/input/scratch; managed one-shot preparation is timed.
  Both OSes pass5,938full/live, affected normal/scalar/disabled-ISA checks and
  the SAME actual package/three examples. Four displayed rows refresh.
  [Result-backed bands](../benchmarks/BICUBIC-RESULT-BACKED-BANDS.md) save
  {result_bands['comparison']['Mixed16']['savedUs']:.2f}/{result_bands['comparison']['TightMixed16']['savedUs']:.2f}µs
  on default/4KiB mixed word: {result_bands['cases']['Mixed16']['managedUs']:.2f}/
  {result_bands['cases']['TightMixed16']['managedUs']:.2f}µs, uppers
  {result_bands['cases']['Mixed16']['upper']:.3f}/{result_bands['cases']['TightMixed16']['upper']:.3f}.
  Default passes/stops; tight remains a small gap. GC114B/op remains;28,136B is
  borrowed from already-admitted output and retired before suffix stores.
  No added heap or retained scratch. The shared vertical path regresses
  {result_bands['sentinel']['rawChangeUs']:.2f}µs/{100*result_bands['sentinel']['rawChange']:.2f}%
  against its contemporary original, but still passes upper
  {result_bands['cases']['TightVertical16']['upper']:.3f},113B/op. This tradeoff is
  retained with substantial mixed savings; no uniform gain claim. Both OSes pass
  5,948full/live, affected normal/scalar/disabled-ISA checks and SAME actual
  package/three examples. Fifteen word controls now
  {result_bands['mergedWordStatusCounts']['pass']}pass/{result_bands['mergedWordStatusCounts']['gap']}gap/{result_bands['mergedWordStatusCounts']['uncertain']}uncertain;
  twelve unchanged paths are reused. Historical bands/geometry stay in linked reports.
  [Lanczos](../benchmarks/LANCZOS-PAIRED-CERTIFICATE-PREPARATION.md) has five
  passes/three prepared gaps; plans retain139,800–333,396B and equivalent
  one-shot setup stays timed. [Streaming](../benchmarks/BICUBIC-SHARED-STREAMING.md)
  passes six of eight; small/wide-column gaps remain linked in coverage.
  [Byte enlargement](../benchmarks/BICUBIC-SHARED-ENLARGEMENT.md) remains
  3,108.21/native3,037.83µs,upper1.026/116Bop. Bilinear byte enlargement is
  uncertain at2,357µs/102,891Bop,upper1.134. Preserve exact Q30/rational/
  certified pixels versus native float; historical gains stay in linked reports.
- **JPEG and reading:** [completed output storage](../benchmarks/JPEG-COMPLETED-OUTPUT.md)
  saves **98.02µs/5.17% raw/5.11% normalized** on progressive grayscale; filtered/
  allactual raw/normalized intervals separate. Current1797.74/native1711.60µs,
  upper1.054 remains a gap. GC683,176→151,432B/op; governed gray peak699,834B and
  retained output531,468B stay unchanged. Sequential default idle pixels rise
  0→531,468B within the existing two-slot/4MiB pool; gray idlecoefficients stay
  549,120B. Governed calls bypass both pools; physical/native peaks are unmeasured.
  Twelve affected costs refresh and seventeen unchanged paths reuse evidence:
  {jpeg_scan['statusCounts']['confirmed-pass-stop']}pass/{jpeg_scan['statusCounts']['confirmed-gap']}gaps/{jpeg_scan['statusCounts']['confirmed-confidence-open']}uncertain. Owned gray now passes (602.47/native681.27µs,upper.891);
  stream420restart is uncertain (upper1.002); stop passing cases and resolve
  uncertainty before tuning. Live444 remains an independent201.57µs gap.
  [Paired component preparation](../benchmarks/JPEG-PAIRED-COMPONENT-PREPARATION.md)
  has no useful gain across three exact short stages; all qualified originals
  and package identities are restored. Its reports retain adverse/inconclusive
  evidence without adding rejected-stage checks to renderer dependencies.
  [Paired AC windows](../benchmarks/JPEG-PAIRED-AC-WINDOW.md) retain earlier causal
  RGB/CMYK gains, unchanged RGB1530.34/native1450.52µs upper1.065 and its574,069B
  governed peak. Current scan tables/owner/cancellation contracts remain intact.
  Earlier [live windows](../benchmarks/JPEG-LIVE-ENTROPY-WINDOW.md),
  [publication](../benchmarks/JPEG-LIVE-NORMAL-PUBLICATION.md) and
  [long lookup](../benchmarks/JPEG-BUFFERED-LONG-LOOKUP.md) retain historical gains.
  [Prepared Huffman replay](../benchmarks/JPEG-PREPARED-HUFFMAN-BANK.md) saves
  **{jpeg_bank['comparison']['savedUs']:.2f}µs/{100*jpeg_bank['comparison']['rawReduction']:.2f}% raw**,
  {100*jpeg_bank['comparison']['normalizedPointReduction']:.2f}% reference-normalized, on complete RGBA→gray encoding
  versus explicitly labeled SIMD libjpeg-turbo. Filtered/all-actual intervals separate;
  exact58,910B packets and155,436B/op remain. Current
  {jpeg_bank['cases']['RgbaGray']['managedUs']:,.2f}/native{jpeg_bank['cases']['RgbaGray']['nativeUs']:,.2f}µs,
  upper{jpeg_bank['cases']['RgbaGray']['upper']:.3f} remains a gap. All17 affected encoder
  boundaries refresh: {jpeg_bank['statusCounts']['pass']}pass/{jpeg_bank['statusCounts']['gap']}gaps/{jpeg_bank['statusCounts']['uncertain']}uncertain.
  A4KiB stack lookup fits the existing16KiB table allowance, with no new heap or
  retained state. GovernedRGBA16 peak remains1,704,856B. Existing coefficient pools
  and their governed bypass are unchanged; physical/native peaks are unmeasured.
  Strict JPEG metadata exceptions remain; passing cases stop. Restart/recomputed
  paths retain source-valid evidence. Older normalization/replay stages remain history.
- **Text/drawing:** [exact interior coverage](../benchmarks/TEXT-INTERIOR-COVERAGE.md)
  reuses one exact area across fully covered pixels. Current130.55/132.38µs means are5.94%/6.49%
  lower than the contemporaneous original; intervals overlap, so a causal CPUgain
  remains inconclusive. Both reference gaps remain. Allocation67,368B/draw,
  exactframes,48,544B admitted Latin workspace and zero retainedscratch are unchanged.
  [Shared sorting](../benchmarks/TEXT-RASTER-SHARED-SORT.md) retains the earlier allocation cut.
  [Layout/fitting and RGBA16 composition](../benchmarks/TEXT-DRAWING-CURRENT.md)
  pass/stop. These independent cases do not block codec or resize work.
  [Owned font storage](../benchmarks/FONT-OWNED-STORAGE.md) saves258.73µs/89.04%raw
  on complete loading:31.86µs versus labeledFreeType13.42µs, upper2.396 still a gap.
  Allocation falls757,597→176B/op; fullcopy/eager checks remain timed. The isolated
  load retains757,076B idle within the existing shared two-array/4MiB ceiling;
  governed calls bypass it. This is not a physical-peak reduction or parser-equivalence claim.

The current production build passes **{jpeg_bank['qualification']['fullChecksEachOs']:,} full/live checks per OS**, affected
normal/scalar checks, and the **same actual Release package/all three examples
on Windows/Linux**, including exact mixed-alpha text at both depths, alpha and
opaque-word, progressive-strip, wide-column, shared byte/word enlargement and live VP8
sync/async consumers, plus bounded main validation and exact method2 residual storage/cancellation/recovery.
The package also verifies frozen vertical RGBA16 pixels at4/8KiB, aggregate
admission, terminalrefusal, cancellationcleanup and independent survivors.
JPEG paths pass407 internal/375 public checks per normal/scalar mode
on each OS, including actual live-window DC/EOB/following-read witnesses,
truncation/stuffing/partial failures, refusal/cancellation cleanup and survivors.
The current PNG package also verifies ordinary/live byte/word sync/async pixels,
terminal refusal, cancellation cleanup/recovery and independent survivors, plus
frozen Fastest8/16 sync/async packets,
shared admission and independent ownership. The current package also exercises
authored grouped-AC JPEG pixels, sync/async scan storage, terminal refusal and
independent survivors. Its live baseline witnesses also pass. Unchanged
alpha prefix/border/tail/palette, compressed horizontal-alpha package and other
coefficient/decoder/word/ISA evidence is reused.
Unchanged paths reuse valid evidence. This is correctness/package qualification,
not a full release refresh or whole-library performance pass. The retained
long-run cases are included in this full qualification. Original adverse and
uncertain points remain in the [green-prediction report](../benchmarks/WEBP-ISOLATED-GREEN-PREDICTION.md),
including the [earlier compact layouts](../benchmarks/WEBP-COMPACT-PREDICTOR-TRADEOFF.md).
Complete physical/admitted peaks are not inferred from allocations.

**References and boundaries.** FFmpeg is the reference except for explicitly
labeled **libwebp exact=1** lossless WebP (hidden RGB preserved), SIMD-enabled
**libjpeg-turbo** grayscale JPEG, and **zimg** resizing (FFmpeg's zscale backend).
The fifteen current one-shot PNG rows time fresh codec/zlib/packet state and independent
output on both sides, including native word-frame endian conversion/allocation/
disposal. Other native codec workers retain their documented wrappers/contexts.
Stream rows include declared acquisition, copies, EOF and release.

Text compares the complete **ImageSharp.Drawing 2.1.7/Fonts 2.1.3 public integration**,
including multiple layout calls and the full fitting search, not bare shaping.
Font load compares **FreeType 2.13.2 copy/open/retain/dispose**, with eager/lazy
validation differences; it does not establish general parser equivalence.
RGBA16 SrcOver compares **ImageSharp 3.1.12 Rgba64**, not FFmpeg performance.
[Scoped native/visual tolerances](../benchmarks/TEXT-DRAWING-CURRENT.md) retain
exact managed contracts and full 16-bit samples; visual gates are not sample
exactness. Complete packet claims, metadata exceptions and codec pixel contracts
retain their declared [case scope](codec-parity.md). Equal numerical quality
settings do not establish matched quality.

**Codecs**

{codecs}

**Editing**

{editing}

**Inspection and pixel-coding validation**

{read_table}

[Coverage and linked family reports](../benchmarks/WARM-COVERAGE.md) retain producer
identities, confidence decisions, ownership/correctness gates and reproducible raw
evidence. Historical experiment reports remain separate. Update with
`python benchmarks/render-benchmark.py`; verify with `--check`.
"""

if __name__ == '__main__':
    text = render()
    assert len(re.findall(r'^\| ---', text, re.M)) == 3
    assert len(re.findall(r'^\|', text, re.M)) - 6 == 126, 'Expected126comparison rows.'
    output = ROOT / 'docs/benchmark.md'
    if '--check' in sys.argv:
        assert output.read_text(encoding='utf-8') == text, 'Benchmark summary differs from retained evidence.'
        print('Benchmark summary matches current and retained release evidence (three tables).')
    else:
        output.write_text(text, encoding='utf-8', newline='\n')
