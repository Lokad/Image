"""Render current benchmarks from retained evidence; never measure."""
import json
import math
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def render():
    evidence = json.loads((ROOT / 'benchmarks/release-0.2.0-benchmark-evidence.json').read_text(encoding='utf-8'))
    assert evidence['version'] == '0.2.0' and len(evidence['cases']) == 31
    assert evidence['allDisplayedCasesRefreshed'] and not evidence['productionChanged']
    stored = json.loads((ROOT / 'benchmarks/warm-png-bulk-stored-evidence.json').read_text(encoding='utf-8'))
    current = json.loads((ROOT / 'benchmarks/warm-png-distance-symbol-evidence.json').read_text(encoding='utf-8'))
    vp8 = json.loads((ROOT / 'benchmarks/warm-vp8-simd-input-evidence.json').read_text(encoding='utf-8'))
    trellis = json.loads((ROOT / 'benchmarks/warm-vp8-trellis-caller-invariants-evidence.json').read_text(encoding='utf-8'))
    photograph = json.loads((ROOT / 'benchmarks/current-vp8-photograph-encoding-evidence.json').read_text(encoding='utf-8'))
    alpha = json.loads((ROOT / 'benchmarks/current-vp8-alpha8-encoding-evidence.json').read_text(encoding='utf-8'))
    vp8_quantization = json.loads((ROOT / 'benchmarks/warm-vp8-quantization-caller-invariants-evidence.json').read_text(encoding='utf-8'))
    assert vp8_quantization['qualification']['sameActualPackageBothOs']
    assert vp8_quantization['sourceScope']['quantizedLevelArithmeticUnchanged']
    assert len(vp8_quantization['cases']) == 6
    vp8_companion_costs = json.loads((ROOT / 'benchmarks/current-vp8-companion-costs-evidence.json').read_text(encoding='utf-8'))
    assert not vp8_companion_costs['productionChanged'] and vp8_companion_costs['allBuilds'] == 0
    assert vp8_companion_costs['independentQuietConfirmations'] == 4
    assert all(s == 'confirmed-gap' for s in vp8_companion_costs['status'].values())
    assert all(v > 1 for v in vp8_companion_costs['comparisonLowerBounds'].values())
    assert all(len(v[-1]['managed']['actual']) == 12 and v[-1]['receipt']['completePacketExact']
               for v in vp8_companion_costs['cases'].values())
    vp8_alpha16 = json.loads((ROOT / 'benchmarks/current-vp8-alpha16-cost-evidence.json').read_text(encoding='utf-8'))
    assert not vp8_alpha16['productionChanged'] and vp8_alpha16['allBuilds'] == vp8_alpha16['newCases'] == 0
    assert vp8_alpha16['case'] == 'WarmWebp/Encode16' and vp8_alpha16['independentConfirmations'] == 1
    assert vp8_alpha16['raw']['mirrored'] and vp8_alpha16['raw']['allMembersVerified']
    alpha16_cost = vp8_alpha16['cost']
    assert alpha16_cost['actuals'] == alpha16_cost['nativeSamplesEachBracket'] == 12
    assert all(alpha16_cost[k] for k in ['quietGuardsPassed', 'completePacketExact',
                                        'fullSourceNormalizationTimed', 'normalizedReferenceExact'])
    assert alpha16_cost['packetBytes'] == 98590 and alpha16_cost['status'] == 'confirmed-gap'
    assert alpha16_cost['lower'] > 1 and math.isclose(alpha16_cost['managedMicroseconds'] /
                                                   alpha16_cost['nativeMicroseconds'], alpha16_cost['ratio'], rel_tol=1e-12)
    vp8_opaque8 = json.loads((ROOT / 'benchmarks/current-vp8-opaque8-cost-evidence.json').read_text(encoding='utf-8'))
    assert not vp8_opaque8['productionChanged'] and vp8_opaque8['allBuilds'] == vp8_opaque8['newCases'] == 0
    assert vp8_opaque8['case'] == 'WarmWebp/EncodeOpaque8' and vp8_opaque8['independentConfirmations'] == 1
    assert vp8_opaque8['raw']['mirrored'] and vp8_opaque8['raw']['allMembersVerified']
    opaque8_cost = vp8_opaque8['cost']
    assert opaque8_cost['actuals'] == opaque8_cost['nativeSamplesEachBracket'] == 12
    assert opaque8_cost['quietGuardsPassed'] and opaque8_cost['completePacketExact']
    assert opaque8_cost['depth'] == 8 and opaque8_cost['opaque'] and opaque8_cost['packetBytes'] == 72380
    assert opaque8_cost['status'] == 'confirmed-gap' and opaque8_cost['lower'] > 1
    assert math.isclose(opaque8_cost['managedMicroseconds'] / opaque8_cost['nativeMicroseconds'], opaque8_cost['ratio'], rel_tol=1e-12)
    trellis_cost = json.loads((ROOT / 'benchmarks/current-vp8-effort6-cost-evidence.json').read_text(encoding='utf-8'))
    assert not trellis_cost['productionChanged'] and trellis_cost['allBuilds'] == 0
    assert trellis_cost['independentQuietConfirmation']
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
    vp8_decoder_costs = json.loads((ROOT / 'benchmarks/current-vp8-decoder-costs-evidence.json').read_text(encoding='utf-8'))
    assert not vp8_decoder_costs['productionChanged'] and vp8_decoder_costs['allBuilds'] == 0
    assert vp8_decoder_costs['independentQuietConfirmation'] and len(vp8_decoder_costs['cases']) == 4
    vp8_stream = json.loads((ROOT / 'benchmarks/warm-vp8l-group-dispatch-evidence.json').read_text(encoding='utf-8'))
    assert vp8_stream['productionChanged'] and vp8_stream['independentQuietConfirmation']
    assert vp8_stream['decision']['passed'] and vp8_stream['qualification']['sameActualPackageBothOs']
    assert vp8_stream['qualification']['compressedAlphaTwoFramesActualPackagePassed']
    assert vp8_stream['status']['Vp8Stream'] == 'confirmed-gap'
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
    png_materialized_rejected = json.loads((ROOT / 'benchmarks/png-cancelable-materialized-cursor-rejected-evidence.json').read_text(encoding='utf-8'))
    assert png_materialized_rejected['candidateRejected'] and not png_materialized_rejected['productionChanged']
    assert not png_materialized_rejected['decision']['passed']
    assert png_materialized_rejected['qualification']['confirmationRuns'] == 0
    vp8l_histogram_rejected = json.loads((ROOT / 'benchmarks/vp8l-shared-predictor-histogram-rejected-evidence.json').read_text(encoding='utf-8'))
    assert vp8l_histogram_rejected['candidateRejected'] and not vp8l_histogram_rejected['productionChanged']
    assert not vp8l_histogram_rejected['decision']['passed']
    assert vp8l_histogram_rejected['qualification']['confirmationRuns'] == 0
    vp8_inverse_rejected = json.loads((ROOT / 'benchmarks/vp8-packed-inverse-reconstruction-rejected-evidence.json').read_text(encoding='utf-8'))
    assert vp8_inverse_rejected['candidateRejected'] and not vp8_inverse_rejected['productionChanged']
    assert not vp8_inverse_rejected['decision']['passed']
    assert vp8_inverse_rejected['qualification']['confirmationRuns'] == 0
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
    png_indexed = json.loads((ROOT / 'benchmarks/warm-png-indexed-validation-evidence.json').read_text(encoding='utf-8'))
    assert png_indexed['productionChanged'] and png_indexed['independentQuietConfirmation'] and png_indexed['decision']['passed']
    assert png_indexed['decision']['defaultFivePercentPassed'] and png_indexed['decision']['normalizedSeparated']
    assert set(png_indexed['cases']) == {'ValidatePng8', 'ValidatePng16', 'ValidateIndexedAdam7', 'ValidatePng16Async'}
    assert png_indexed['qualification']['fullChecksEachOs'] == 5310
    assert png_indexed['qualification']['package']['sha256'] == png_indexed['qualification']['linuxPackage']['sameActualPackageSha256']
    assert png_indexed['memory']['measuredWarmReductionBytes'] == 376
    assert png_indexed['memory']['governedActualPackagePeakEachOs'] == 4581
    for case, observations in png_indexed['cases'].items():
        observation = observations[-1]
        assert observation['receipt']['librarySha256'] == png_indexed['producers']['linuxSha256']
        assert len(observation['managed']['actual']) == len(observation['receipt']['nativeBefore']) == len(observation['receipt']['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        assert (observation['conservativeRatioUpper'] <= 1) == (png_indexed['status'][case] == 'confirmed-pass')
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
    fastest_word = json.loads((ROOT / 'benchmarks/current-fastest-word-png-costs-evidence.json').read_text(encoding='utf-8'))
    assert not fastest_word['productionChanged'] and fastest_word['independentQuietConfirmations'] == 3
    assert fastest_word['producers'] == header_skips['producers']
    assert fastest_word['qualificationReused']['packageSha256'] == header_skips['qualification']['package']['sha256']
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
    async_png_cost = json.loads((ROOT / 'benchmarks/current-async-png-encode-cost-evidence.json').read_text(encoding='utf-8'))
    assert not async_png_cost['productionChanged'] and async_png_cost['independentQuietConfirmation']
    assert async_png_cost['producers'] == word_color_png['producers']
    assert async_png_cost['qualificationReused']['packageSha256'] == word_color_png['qualification']['package']['sha256']
    assert async_png_cost['builds'] == dict(core=0, publicTests=0, internalTests=0, differential=0, selector=0, native=0, harnessEachOs=1)
    cursor_reassessment = json.loads((ROOT / 'benchmarks/png-cancelable-cursor-reassessment-evidence.json').read_text(encoding='utf-8'))
    assert cursor_reassessment['candidateRejected'] and not cursor_reassessment['productionChanged']
    assert cursor_reassessment['independentQuietConfirmation'] and not cursor_reassessment['independentCandidateConfirmation']
    assert not cursor_reassessment['decision']['passed'] and cursor_reassessment['sourceScope']['sourceRestoredByteExact']
    assert cursor_reassessment['producers'] == word_color_png['producers']
    assert cursor_reassessment['qualification']['packageSha256'] == word_color_png['qualification']['package']['sha256']
    tight_png_costs = json.loads((ROOT / 'benchmarks/current-tight-png-decode-costs-evidence.json').read_text(encoding='utf-8'))
    assert not tight_png_costs['productionChanged'] and tight_png_costs['allBuilds'] == 0
    assert tight_png_costs['independentQuietConfirmations'] == 2 and tight_png_costs['producers'] == word_color_png['producers']
    assert tight_png_costs['qualificationReused']['packageSha256'] == word_color_png['qualification']['package']['sha256']
    resident = json.loads((ROOT / 'benchmarks/current-png-resident-identify-evidence.json').read_text(encoding='utf-8'))
    resident_other = json.loads((ROOT / 'benchmarks/current-jpeg-webp-resident-identify-evidence.json').read_text(encoding='utf-8'))
    stream_identify = json.loads((ROOT / 'benchmarks/current-png-stream-identify-evidence.json').read_text(encoding='utf-8'))
    decoders = json.loads((ROOT / 'benchmarks/current-default-decoders-evidence.json').read_text(encoding='utf-8'))
    resident_decode = json.loads((ROOT / 'benchmarks/warm-vp8l-group-dispatch-evidence.json').read_text(encoding='utf-8'))
    indexed_decode = json.loads((ROOT / 'benchmarks/warm-vp8l-group-dispatch-evidence.json').read_text(encoding='utf-8'))
    indexed_observation = indexed_decode['cases']['IndexedNative16Colors8'][-1]
    assert indexed_decode['productionChanged'] and indexed_decode['independentQuietConfirmation']
    assert indexed_decode['producers'] == resident_decode['producers']
    assert len(indexed_observation['managed']['actual']) == 12
    assert indexed_observation['receipt']['allRgbaSamplesExact']
    assert indexed_observation['receipt']['managedOutputSha256'] == indexed_observation['receipt']['nativeOutputSha256']
    assert all(q['passed'] for q in indexed_observation['quietness'].values())
    live_decode = json.loads((ROOT / 'benchmarks/warm-vp8l-group-dispatch-evidence.json').read_text(encoding='utf-8'))
    live_observation = live_decode['cases']['NativeLosslessCancelable8'][-1]
    assert live_decode['productionChanged'] and live_decode['decision']['passed']
    assert live_decode['producers'] == resident_decode['producers']
    assert len(live_observation['managed']['actual']) == 12
    assert live_observation['receipt']['allRgbaSamplesExact'] and live_observation['receipt']['alreadyCanceledRejected']
    assert live_observation['receipt']['cancellationCanBeCanceled'] and not live_observation['receipt']['cancellationRequested']
    assert all(q['passed'] for q in live_observation['quietness'].values())
    jpeg_decode = baseline_blocks
    assert not decoders['productionChanged'] and len(decoders['cases']) == 3
    assert resident_decode['productionChanged'] and resident_decode['independentQuietConfirmation']
    assert resident_decode['decision']['passed'] and resident_decode['qualification']['sameActualPackageBothOs']
    assert resident_decode['memory']['newOwnedCapacityBytes'] == 0
    assert resident_decode['memory']['measuredWarmAllocatedBefore'] == resident_decode['memory']['measuredWarmAllocatedAfter'] == 152728
    assert resident_decode['qualification']['focusedInternalEachModeEachOs'] == 58
    assert resident_decode['qualification']['longGreenFixtureInteroperatesExactly']
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
    assert vp8['productionChanged'] and vp8['independentQuietConfirmationThisPair']
    assert trellis['productionChanged'] and trellis['independentQuietConfirmation']
    assert trellis['qualification']['sameActualPackageBothOs']
    assert not photograph['productionChanged'] and photograph['independentQuietConfirmation']
    assert not alpha['productionChanged'] and alpha['independentQuietConfirmation']
    assert jpeg['productionChanged'] and jpeg['independentQuietConfirmationThisPair']
    assert vp8l['version'] == '0.2.1' and vp8l['independentQuietConfirmationThisPair']
    assert current['version'] == '0.2.1' and current['independentQuietConfirmationThisPair']
    assert set(current['cases']) == {'EncodeFast8', 'EncodeSmallest8', 'EncodeSmallest16'}
    vp8_cases = {'case06': 'DiagramLossyDistortion8',
                 'case08': 'PhotographLossy8', 'case09': 'DiagramLossy8',
                 'case10': 'DiagramLossyTrellis8', 'case11': 'Opaque16'}
    rows = []
    for case in evidence['cases']:
        if case['id'] in {'case07', 'case13', 'case14', 'case15', 'case16', 'case27', 'case28', 'case29', 'case30'}:
            continue  # Replaced by current self-contained family evidence below.
        observation = case['observation']
        vp8l_case = case['id'] in {'case12', 'case13', 'case14'}
        vp8_case = case['id'] in vp8_cases
        updated = vp8_case or vp8l_case or case['id'] in editing_costs or case['id'] in {'case00', 'case02', 'case03', 'case16', 'case17', 'case18', 'case19', 'case20'}
        producer = vp8_quantization if vp8_case else jpeg if case['id'] in {'case02', 'case03'} else vp8_stream if case['id'] == 'case00' else vp8l if vp8l_case else stored if case['id'] == 'case20' else current
        if case['id'] == 'case10':
            producer = trellis_cost
        elif case['id'] in {'case06', 'case09', 'case11'}:
            producer = vp8_companion_costs
        if case['id'] in {'case17', 'case18'}:
            producer = predictive_cost
        elif case['id'] in {'case19', 'case20'}:
            producer = png_prediction
        if case['id'] in editing_costs:
            observation = editing_costs[case['id']]
        elif vp8_case:
            observation = producer['cases'][vp8_cases[case['id']]][-1]
        elif case['id'] == 'case00':
            observation = producer['cases']['Vp8Stream'][-1]
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
    for case, label in [('EncodeFast16', 'PNG RGBA16, Fastest1 / native1'),
                        ('EncodeFastCancelable16', 'PNG RGBA16 Fastest1, live cancelable token'),
                        ('EncodeFastForward16', 'PNG RGBA16 Fastest1, forward-only output')]:
        observation = fastest_word['cases'][case][-1]
        receipt = observation['receipt']
        assert fastest_word['status'][case] == 'confirmed-gap'
        assert receipt['librarySha256'] == fastest_word['producers']['linuxSha256']
        assert len(observation['managed']['actual']) == 12 and all(q['passed'] for q in observation['quietness'].values())
        assert receipt['managedOutputBytes'] == receipt['nativeOutputBytes'] == 968216
        assert receipt['managedEncodedSha256'] == receipt['nativeOutputSha256']
        stats = observation['managed']['statistics']
        native = min(b['mean'] for b in observation['nativeBrackets'])
        rows.append(('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | 968,216 / 968,216 |'))
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
    observation = async_png_cost['cases']['Png8AsyncNonSeekable'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert async_png_cost['status']['Png8AsyncNonSeekable'] == 'confirmed-gap'
    assert receipt['librarySha256'] == async_png_cost['producers']['linuxSha256']
    assert receipt['managedLevel'] == receipt['nativeLevel'] == 6 and receipt['filterNone']
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert all(q['passed'] for q in observation['quietness'].values())
    assert receipt['managedOutputBytes'] == receipt['nativeOutputBytes'] == 210973
    assert receipt['managedOutputSha256'] == 'ad9ed127da583d68bdb749bb983b73966e8e814527216449c4557fdd89668920'
    assert receipt['completePacketSizeGate'] and receipt['allRgbaSamplesExact'] and receipt['resourceAndCancellationGates']
    native = min(b['mean'] for b in observation['nativeBrackets'])
    rows.append(('codec', f'| PNG RGBA8 async, fresh nonseekable output, Optimal6 / native6 † | {stats["Mean"] / 1000:,.2f} | '
                 f'{native / 1000:,.2f} | {observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                 f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | 210,973 / 210,973 |'))
    observation = cursor_reassessment['cases']['DecodeCancelable8'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert cursor_reassessment['status']['DecodeCancelable8'] == 'confirmed-gap'
    assert receipt['librarySha256'] == cursor_reassessment['producers']['linuxSha256']
    assert receipt['inputSha256'] == '4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f'
    assert receipt['cancellationCanBeCanceled'] and not receipt['cancellationRequested']
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert all(q['passed'] for q in observation['quietness'].values())
    assert all(receipt[k] for k in ('allRgbaSamplesExact', 'alphaExact', 'hiddenRgbExact', 'sourceUnchanged', 'independentOwnedOutput'))
    native = min(b['mean'] for b in observation['nativeBrackets'])
    rows.append(('codec', f'| PNG RGBA8 decode, live token † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                 f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                 f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | — |'))
    for case, label, limit in [('DecodeTight8', 'PNG RGBA8 decode, tight working limit', 1235173),
                               ('DecodeTight16', 'PNG RGBA16 decode, tight working limit (confidence open)', 3254193)]:
        observation = tight_png_costs['cases'][case][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert receipt['librarySha256'] == tight_png_costs['producers']['linuxSha256']
        assert receipt['tightWorkingBytes'] == limit and not receipt['cancellationCanBeCanceled']
        assert receipt['nativeDecodeEndianConversionTimed'] == (case == 'DecodeTight16')
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        assert all(receipt[k] for k in ('allRgbaSamplesExact', 'alphaExact', 'hiddenRgbExact', 'sourceUnchanged', 'independentOwnedOutput'))
        if case == 'DecodeTight8':
            assert tight_png_costs['status'][case] == 'confirmed-gap' and tight_png_costs['comparisonLowerBounds'][case] > 1
        else:
            assert tight_png_costs['status'][case] == 'confirmed-confidence-open'
            assert tight_png_costs['comparisonLowerBounds'][case] <= 1 < observation['conservativeRatioUpper']
        native = min(b['mean'] for b in observation['nativeBrackets'])
        rows.append(('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | — |'))
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
        assert producer['status'][case] == ('confirmed-gap' if samples == 12 else 'screen-gap')
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
    observation = tight_byte_up['cases']['TightUp8'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == tight_byte_up['producers']['linuxSha256']
    assert receipt['managedRationalReferenceExact'] and receipt['managedMaxWorkingBytes'] == 2667616
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert all(q['passed'] for q in observation['quietness'].values())
    rows.append(('editing', f'| Bicubic RGBA8 enlargement, input+output+4 KiB † → 1027×519 | {stats["Mean"] / 1000:,.2f} | '
                 f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                 f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                 f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
    axial_byte = json.loads((ROOT / 'benchmarks/current-byte-bicubic-axial-costs-evidence.json').read_text(encoding='utf-8'))
    assert not axial_byte['productionChanged'] and axial_byte['allBuilds'] == 0
    assert axial_byte['producers'] == tight_byte_up['producers']
    assert axial_byte['qualificationReuse']['sameActualPackageBothOs']
    for case, axis in [('TightHorizontalUp8', 'width'), ('TightVerticalUp8', 'height')]:
        observation = axial_byte['cases'][case][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert receipt['librarySha256'] == axial_byte['producers']['linuxSha256']
        assert receipt['managedQualifiedOriginalPixelsExact']
        assert receipt['managedRationalReferenceExact'] == (axis == 'width')
        assert receipt['certifiedExactRationalRgbHalves'] == (0 if axis == 'width' else 1)
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        rows.append(('editing', f'| Bicubic RGBA8 {axis} enlargement, input+output+4 KiB † → {receipt["outputWidth"]}×{receipt["outputHeight"]} | '
                     f'{stats["Mean"] / 1000:,.2f} | {min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
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
    integer_dot = json.loads((ROOT / 'benchmarks/bicubic-integer-dot-rejected-evidence.json').read_text(encoding='utf-8'))
    assert integer_dot['candidateRejected'] and not integer_dot['productionChanged']
    assert not integer_dot['decision']['passed']
    assert all(v['productionSourceRestored'] and v['sourceManifestRestored'] for v in integer_dot['restoration'].values())
    separable_alpha = json.loads((ROOT / 'benchmarks/exact-alpha-separable-rejected-evidence.json').read_text(encoding='utf-8'))
    assert separable_alpha['candidateRejected'] and not separable_alpha['productionChanged']
    assert not separable_alpha['decision']['passed'] and separable_alpha['decision']['allocatedDifference'] == 0
    assert all(v['productionSourceRestored'] and v['sourceManifestRestored'] for v in separable_alpha['restoration'].values())
    first_ac = json.loads((ROOT / 'benchmarks/jpeg-first-ac-confidence-open-evidence.json').read_text(encoding='utf-8'))
    assert first_ac['confidenceOpen'] and not first_ac['accepted'] and not first_ac['productionChanged']
    assert all(v['productionSourceAndConsumerRestored'] for v in first_ac['restoration'].values())
    refinement_window = json.loads((ROOT / 'benchmarks/warm-jpeg-refinement-window-evidence.json').read_text(encoding='utf-8'))
    assert refinement_window['qualification']['fullChecksEachOs'] == 5289
    assert refinement_window['decisions']['confirm']['passed'] and refinement_window['decisions']['confirm']['normalizedSeparated']
    assert refinement_window['qualification']['sameActualPackageBothOs']
    assert refinement_window['memory']['primaryAllocatedDifference'] == refinement_window['memory']['newOwnedCapacityBytes'] == 0
    assert refinement_window['correctness']['headersAndReadProofRestored']['originalHeadersAndReadProofByteExact']
    assert set(refinement_window['cases']) == {'ProgressiveGray', 'Progressive420', 'Progressive422', 'Progressive444', 'ProgressiveCancelable444', 'ValidateJpegProgressive420'}
    for key, label in [('ProgressiveGray', 'JPEG progressive grayscale, resident decode'),
                       ('Progressive420', 'JPEG progressive420, resident decode (confidence open)'),
                       ('Progressive422', 'JPEG progressive422, resident decode'),
                       ('Progressive444', 'JPEG ordinary progressive444, resident decode'),
                       ('ProgressiveCancelable444', 'JPEG progressive444, live-token resident decode')]:
        observation = refinement_window['cases'][key][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert receipt['librarySha256'] == refinement_window['producers']['linuxSha256']
        assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256']
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        assert (refinement_window['comparisonLowerBounds'][key] <= 1) == (key == 'Progressive420')
        rows.append(('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | '
                     f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | — |'))
    dense_cmyk = json.loads((ROOT / 'benchmarks/current-dense-cmyk-jpeg-cost-evidence.json').read_text(encoding='utf-8'))
    assert not dense_cmyk['productionChanged'] and dense_cmyk['status']['CmykAdobeGeneral'] == 'confirmed-pass'
    assert dense_cmyk['oracle']['independentArbitraryPrecisionMatrix'] and dense_cmyk['oracle']['normalScalarNativeAndManagedExact']
    assert dense_cmyk['windowsProof']['normalScalarIndependentMatrixAndNativeExact'] and dense_cmyk['restoration']['originalDerivedFixtureByteExact']
    observation = dense_cmyk['cases']['CmykAdobeGeneral'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == dense_cmyk['producers']['linuxSha256']
    assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256'] == dense_cmyk['oracle']['rgbaSha256']
    assert observation['conservativeRatioUpper'] <= 1 and all(q['passed'] for q in observation['quietness'].values())
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    rows.append(('codec', f'| JPEG dense CMYK, resident decode (confirmed pass) † | {stats["Mean"] / 1000:,.2f} | '
                 f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                 f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                 f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | — |'))
    horizontal_cmyk = json.loads((ROOT / 'benchmarks/current-horizontal-cmyk-jpeg-cost-evidence.json').read_text(encoding='utf-8'))
    assert not horizontal_cmyk['productionChanged'] and horizontal_cmyk['status']['CmykAdobeAc'] == 'confirmed-gap'
    assert horizontal_cmyk['comparisonLowerBound'] > 1
    assert horizontal_cmyk['oracle']['independentIntegerQ14Q16'] and horizontal_cmyk['oracle']['managedAndNativeNormalScalarExact']
    assert horizontal_cmyk['windowsProof']['managedNormalScalarIndependentOracleAndNativeExact'] and horizontal_cmyk['restoration']['originalDerivedFixtureByteExact']
    observation = horizontal_cmyk['cases']['CmykAdobeAc'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == horizontal_cmyk['producers']['linuxSha256']
    assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256'] == horizontal_cmyk['oracle']['rgbaSha256']
    assert all(q['passed'] for q in observation['quietness'].values())
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    rows.append(('codec', f'| JPEG horizontal-only CMYK, resident decode † | {stats["Mean"] / 1000:,.2f} | '
                 f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                 f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                 f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | — |'))
    owned_cmyk = json.loads((ROOT / 'benchmarks/owned-cmyk-frame-unqualified-evidence.json').read_text(encoding='utf-8'))
    assert not owned_cmyk['accepted'] and not owned_cmyk['productionChanged']
    assert not owned_cmyk['decisions']['confirm']['passed'] and owned_cmyk['decisions']['short']['independentConfirmationAllowed']
    assert owned_cmyk['memory']['allocatedFractionRemoved'] > .5
    assert all(v['productionSourceAndTestSourceRestored'] and v['bothTestAssembliesRestored'] for v in owned_cmyk['restoration'].values())
    completed_rows = json.loads((ROOT / 'benchmarks/warm-jpeg-completed-row-reuse-evidence.json').read_text(encoding='utf-8'))
    assert completed_rows['accepted'] and completed_rows['decisions']['confirm']['defaultFivePercentPassed']
    assert completed_rows['decisions']['confirm']['normalizedSeparated']
    assert completed_rows['qualification']['fullChecksEachOs'] == 5291
    assert completed_rows['qualification']['sameActualPackageBothOs']
    assert completed_rows['qualification']['package']['sha256'] == completed_rows['qualification']['linuxPackage']['sameActualPackageSha256']
    assert completed_rows['correctness']['headersAndOraclesRestored']['originalHeadersAndOraclesRestoredByteExact']
    assert completed_rows['memory']['newOwnedCapacityBytes'] == 0
    assert len(completed_rows['sourceScope']['noCalls']) == 8
    for key, label in [('CmykAdobeAc', 'JPEG horizontal-only CMYK, resident decode'),
                       ('CmykAdobeSparse2D', 'JPEG CMYK sparse-2D, resident decode'),
                       ('NativeDecode420', 'JPEG baseline420, resident decode'),
                       ('Progressive420', 'JPEG progressive420, resident decode'),
                       ('Progressive422', 'JPEG progressive422, resident decode'),
                       ('Progressive444', 'JPEG ordinary progressive444, resident decode'),
                       ('ProgressiveCancelable444', 'JPEG progressive444, live-token resident decode')]:
        observation = completed_rows['cases'][key][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert receipt['librarySha256'] == completed_rows['producers']['linuxSha256']
        assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256']
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        assert completed_rows['comparisonLowerBounds'][key] > 1
        new_row = (f'| {label} † | {stats["Mean"] / 1000:,.2f} | '
                   f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                   f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                   f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | — |')
        matches = [i for i, (kind, row) in enumerate(rows) if kind == 'codec' and row.startswith('| ' + label) ]
        assert len(matches) == 1, (key, matches)
        rows[matches[0]] = ('codec', new_row)
    wide_view = json.loads((ROOT / 'benchmarks/bicubic-wide-tap-view-rejected-evidence.json').read_text(encoding='utf-8'))
    assert wide_view['candidateRejected'] and not wide_view['productionChanged'] and not wide_view['decision']['passed']
    assert all(v['productionSourceRestored'] and v['coreAndPublicCoreCopiesByteExact'] for v in wide_view['restoration'].values())
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
    tight_mixed = json.loads((ROOT / 'benchmarks/warm-mixed-bicubic-geometry-evidence.json').read_text(encoding='utf-8'))
    assert tight_mixed['productionChanged'] and tight_mixed['independentQuietConfirmation']
    assert tight_mixed['decision']['defaultFivePercentPassed'] and tight_mixed['decision']['normalizedSeparated']
    assert tight_mixed['memory']['newOwnedCapacityBytes'] == 0
    assert tight_mixed['workflow']['allMeasurementBuilds'] == 0
    assert tight_mixed['qualification']['fullChecksEachOs'] == 5314
    assert tight_mixed['qualification']['package']['sha256'] == tight_mixed['qualification']['linuxPackage']['sameActualPackageSha256']
    assert set(tight_mixed['cases']) == {'Mixed8', 'Mixed16', 'TightMixed8', 'TightMixed16', 'TightVertical8', 'TightVertical16'}
    for case, observations in tight_mixed['cases'].items():
        observation = observations[-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert receipt['librarySha256'] == tight_mixed['producers']['linuxSha256']
        assert receipt['managedQualifiedOriginalPixelsExact']
        assert all(q['passed'] for q in observation['quietness'].values())
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert tight_mixed['comparisonLowerBounds'][case] > 1 and tight_mixed['status'][case] == 'confirmed-gap'
        if case not in {'TightMixed8', 'TightMixed16'}:
            continue
        assert receipt['managedRationalReferenceExact']
        native = min(b['mean'] for b in observation['nativeBrackets'])
        rows.append(('editing', f'| Bicubic RGBA{receipt["depth"]}, width up / height down, input+output+4 KiB † → 1027×129 | '
                     f'{stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                     f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                     f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
    for depth, name in [(8, 'byte'), (16, 'word')]:
        tight_wide = json.loads((ROOT / f'benchmarks/current-tight-wide-{name}-bicubic-costs-evidence.json').read_text(encoding='utf-8'))
        assert not tight_wide['productionChanged'] and tight_wide['independentQuietConfirmation']
        assert tight_wide['workflow']['allBuilds'] == tight_wide['workflow']['newInputs'] == tight_wide['workflow']['newProfiles'] == 0
        assert tight_wide['qualificationReused']['fullChecksEachOs'] == 5314
        assert set(tight_wide['cases']) == {f'TightWide{depth}', f'Tight64x32_{depth}'}
        for case, observations in tight_wide['cases'].items():
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
    body_count = json.loads((ROOT / 'benchmarks/vp8l-prepared-body-count-rejected-evidence.json').read_text(encoding='utf-8'))
    assert body_count['candidateRejected'] and not body_count['productionChanged'] and not body_count['decision']['passed']
    assert not body_count['independentCandidateConfirmationRun']
    assert body_count['qualification']['applicableTotalChecksEachOs'] == 5321
    assert body_count['qualification']['full5314BothOsAndSameActualPackageProofReused']
    assert all(r['productionSourceRestoredByteExact'] for r in body_count['restoration'].values())
    alternating_taps = json.loads((ROOT / 'benchmarks/bicubic-alternating-tap-sums-rejected-evidence.json').read_text(encoding='utf-8'))
    assert alternating_taps['candidateRejected'] and not alternating_taps['productionChanged']
    assert not alternating_taps['decision']['passed'] and not alternating_taps['independentCandidateConfirmationRun']
    assert alternating_taps['qualification']['applicableTotalChecksEachOs'] == 5325
    assert alternating_taps['qualification']['focusedPublicEachMode'] == 221
    assert all(r['productionSourceRestoredByteExact'] for r in alternating_taps['restoration'].values())
    enlargement_geometry = json.loads((ROOT / 'benchmarks/enlargement-vertical-geometry-rejected-evidence.json').read_text(encoding='utf-8'))
    assert enlargement_geometry['candidateRejected'] and not enlargement_geometry['productionChanged']
    assert not enlargement_geometry['decision']['passed']
    assert enlargement_geometry['qualification']['applicableChecksEachOs'] == 5330
    assert enlargement_geometry['qualification']['prototypeOnlyInternalTestsRemoved'] == 3
    assert all(r['productionSourceRestoredByteExact'] for r in enlargement_geometry['restoration'].values())
    governed_jpeg16 = json.loads((ROOT / 'benchmarks/current-governed-jpeg-rgba16-cost-evidence.json').read_text(encoding='utf-8'))
    assert not governed_jpeg16['productionChanged'] and governed_jpeg16['independentQuietConfirmation']
    assert governed_jpeg16['allBuilds'] == governed_jpeg16['newProfiles'] == governed_jpeg16['newInputs'] == 0
    observation = governed_jpeg16['cases']['Rgba16'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == governed_jpeg16['producers']['linuxSha256']
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert governed_jpeg16['comparisonLowerBounds']['Rgba16'] > 1
    updated_governed16 = 0
    for index, (kind, row) in enumerate(rows):
        if row.startswith('| JPEG RGBA16 4:2:0 / caller IMemoryBudget'):
            label = row.split('|')[1].strip()
            rows[index] = (kind, f'| {label} | {stats["Mean"] / 1000:,.2f} | {min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                          f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                          f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | {receipt["managedOutputBytes"]:,} / {receipt["nativeOutputBytes"]:,} |')
            updated_governed16 += 1
    assert updated_governed16 == 1
    factored_jpeg16 = json.loads((ROOT / 'benchmarks/jpeg-factored-word-normalization-evidence.json').read_text(encoding='utf-8'))
    assert factored_jpeg16['accepted'] and factored_jpeg16['productionChanged']
    assert factored_jpeg16['decision']['passed'] and factored_jpeg16['decision']['normalizedSeparated']
    qualification = factored_jpeg16['qualification']
    assert qualification['fullChecksEachOs'] == 5331 and qualification['harnessNativeSelectorBuilds'] == 0
    assert qualification['package']['sha256'] == qualification['linuxPackage']['sameActualPackageSha256']
    observation = factored_jpeg16['cases']['GovernedRgba16'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == factored_jpeg16['producers']['linuxSha256']
    assert receipt['completePacketExact'] and receipt['callerMemoryBudgetPeakBytes'] == 1704856
    assert len(observation['managed']['actual']) == 12
    updated_factored16 = 0
    for index, (kind, row) in enumerate(rows):
        if row.startswith('| JPEG RGBA16 4:2:0 / caller IMemoryBudget'):
            label = row.split('|')[1].strip()
            rows[index] = (kind, f'| {label} | {stats["Mean"] / 1000:,.2f} | {min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                          f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                          f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | {receipt["managedOutputBytes"]:,} / {receipt["nativeOutputBytes"]:,} |')
            updated_factored16 += 1
    assert updated_factored16 == 1
    jpeg16_companions = json.loads((ROOT / 'benchmarks/current-jpeg-word-companion-costs-evidence.json').read_text(encoding='utf-8'))
    assert not jpeg16_companions['productionChanged'] and jpeg16_companions['allBuilds'] == 0
    assert jpeg16_companions['producers'] == factored_jpeg16['producers']
    assert jpeg16_companions['independentQuietConfirmations'] == 3
    assert all(v == 'confirmed-gap' for v in jpeg16_companions['status'].values())
    assert all(len(v[-1]['managed']['actual']) == 12 and v[-1]['receipt']['completePacketExact']
               for v in jpeg16_companions['cases'].values())
    opaque_alpha = json.loads((ROOT / 'benchmarks/vp8l-opaque-alpha-analysis-rejected-evidence.json').read_text(encoding='utf-8'))
    assert opaque_alpha['candidateRejected'] and not opaque_alpha['productionChanged']
    assert not opaque_alpha['decision']['independentCandidateConfirmation']
    assert opaque_alpha['qualification']['applicableChecksEachOs'] == 5332
    assert all(r['productionSourceRestoredByteExact'] for r in opaque_alpha['qualification']['restoration'].values())
    grayscale_costs = json.loads((ROOT / 'benchmarks/current-grayscale-jpeg-costs-evidence.json').read_text(encoding='utf-8'))
    assert not grayscale_costs['productionChanged'] and grayscale_costs['allBuilds'] == 0
    assert grayscale_costs['producers'] == factored_jpeg16['producers']
    assert grayscale_costs['independentQuietConfirmations'] == 2
    assert grayscale_costs['correctness']['rawCompletePacketExact']
    updated_gray = 0
    for case, label in [('RgbaGray', 'JPEG grayscale, standalone SIMD libjpeg-turbo'),
                        ('CallerGray', 'JPEG from caller GRAY8, standalone SIMD libjpeg-turbo')]:
        observation = grayscale_costs['cases'][case][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert len(observation['managed']['actual']) == 12 and grayscale_costs['comparisonLowerBounds'][case] > 1
        for index, (kind, row) in enumerate(rows):
            if row.startswith(f'| {label} |'):
                rows[index] = (kind, f'| {label} † | {stats["Mean"] / 1000:,.2f} | {min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                              f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                              f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | {receipt["managedOutputBytes"]:,} / {receipt["nativeOutputBytes"]:,} |')
                updated_gray += 1
    assert updated_gray == 2
    baseline_gray_cost = json.loads((ROOT / 'benchmarks/current-baseline-grayscale-jpeg-decode-cost-evidence.json').read_text(encoding='utf-8'))
    assert not baseline_gray_cost['productionChanged'] and baseline_gray_cost['allBuilds'] == 0
    assert baseline_gray_cost['producers'] == factored_jpeg16['producers']
    assert baseline_gray_cost['correctness']['allSixPixelOutputsExact']
    assert baseline_gray_cost['pixelReconciliation']['catalog']['firstSetupActuals'] == 0
    assert len(baseline_gray_cost['cases']['OwnedDecodeGray'][-1]['managed']['actual']) == 12
    cmyk_pack = json.loads((ROOT / 'benchmarks/cmyk-vbmi-packing-rejected-evidence.json').read_text(encoding='utf-8'))
    assert cmyk_pack['candidateRejected'] and not cmyk_pack['productionChanged'] and not cmyk_pack['decision']['passed']
    assert cmyk_pack['actualIntrinsicProof']['actualVbmiPackingExecuted']
    assert cmyk_pack['qualification']['totalApplicableEachOs'] == 5295
    assert cmyk_pack['qualification']['sameActualReleasePackageProofReused']
    assert all(v['productionSourceRestored'] and v['coreAndPublicCoreCopiesByteExact'] for v in cmyk_pack['restoration'].values())
    color_rows = json.loads((ROOT / 'benchmarks/cmyk-completed-color-rows-rejected-evidence.json').read_text(encoding='utf-8'))
    assert color_rows['candidateRejected'] and not color_rows['productionChanged'] and not color_rows['decision']['passed']
    assert color_rows['qualification']['totalApplicableEachOs'] == 5303
    assert color_rows['qualification']['sameActualReleasePackageProofReused']
    assert all(v['productionSourceRestored'] and v['coreAndPublicCoreCopiesByteExact'] and v['originalAcDerivedFixtureRestoredByteExact'] for v in color_rows['restoration'].values())
    assert color_rows['diagnostic']['profilingOnly'] and color_rows['diagnostic']['noTimingClaim']
    overwrite_cmyk = json.loads((ROOT / 'benchmarks/cmyk-overwrite-allocation-rejected-evidence.json').read_text(encoding='utf-8'))
    assert overwrite_cmyk['candidateRejected'] and not overwrite_cmyk['productionChanged'] and not overwrite_cmyk['decision']['passed']
    assert overwrite_cmyk['restoration']['productionSourceAndCoreCopiesRestoredByteExact']
    assert overwrite_cmyk['restoration']['internalAssembliesRestoredByteExact']
    assert overwrite_cmyk['qualification']['totalApplicableEachOs'] == 5303
    assert overwrite_cmyk['qualification']['sameActualReleasePackageProofReused']
    block_window = json.loads((ROOT / 'benchmarks/jpeg-cross-block-lookahead-rejected-evidence.json').read_text(encoding='utf-8'))
    assert block_window['candidateRejected'] and not block_window['productionChanged'] and not block_window['decision']['passed']
    assert block_window['qualification']['totalApplicableEachOs'] == 5305
    assert block_window['qualification']['sameActualReleasePackageProofReused']
    assert all(v['productionSourceRestored'] and v['coreAndPublicCoreCopiesByteExact'] for v in block_window['restoration'].values())
    owned_cmyk_current = json.loads((ROOT / 'benchmarks/current-owned-cmyk-jpeg-costs-evidence.json').read_text(encoding='utf-8'))
    assert not owned_cmyk_current['productionChanged'] and owned_cmyk_current['allBuilds'] == 0
    assert owned_cmyk_current['status']['CmykAdobe'] == 'confirmed-confidence-open'
    dc_cmyk = owned_cmyk_current['cases']['CmykAdobe'][-1]
    assert owned_cmyk_current['comparisonLowerBound'] <= 1 < dc_cmyk['conservativeRatioUpper']
    assert len(dc_cmyk['managed']['actual']) == 12 and all(q['passed'] for q in dc_cmyk['quietness'].values())
    assert dc_cmyk['receipt']['managedOutputSha256'] == dc_cmyk['receipt']['nativeOutputSha256']
    assert owned_cmyk_current['restoration']['originalDerivedFixtureRestoredByteExact']
    assert owned_cmyk_current['qualificationReused']['sameActualReleasePackageProofReused']
    jpeg444_rows = json.loads((ROOT / 'benchmarks/jpeg444-row-normalization-evidence.json').read_text(encoding='utf-8'))
    assert jpeg444_rows['accepted'] and jpeg444_rows['decision']['normalizedSeparated']
    assert all(q['total'] == 5342 for q in jpeg444_rows['qualification'].values())
    assert jpeg444_rows['package']['sameActualWindowsReleasePackageBothOs']
    for case, label in [('Encode444Stream8', 'JPEG 4:4:4, forward-only / exact reference'),
                        ('Jpeg8AsyncNonSeekable', 'JPEG RGBA8 async nonseekable, 444')]:
        observation = jpeg444_rows['cases'][case][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert receipt['librarySha256'] == jpeg444_rows['producers']['linuxSha256']
        assert receipt.get('completePacketExact', receipt.get('encodedPacketsExact'))
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        assert (receipt['managedOutputBytes'], receipt['nativeOutputBytes']) == (133039, 133041)
        status = jpeg444_rows['status'][case]
        assert (status == 'confirmed-pass') == (observation['conservativeRatioUpper'] <= 1)
        qualifier = 'confirmed pass' if status == 'confirmed-pass' else 'confidence open'
        replacement = (f'| {label} ({qualifier}) † | {stats["Mean"] / 1000:,.2f} | '
                       f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                       f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                       f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | 133,039 / 133,041 |')
        position = next(i for i, (kind, row) in enumerate(rows) if row.startswith('| ' + label))
        rows[position] = ('codec', replacement)
    jpeg444_decode = json.loads((ROOT / 'benchmarks/current-baseline-jpeg444-decode-costs-evidence.json').read_text(encoding='utf-8'))
    assert not jpeg444_decode['productionChanged'] and jpeg444_decode['allBuilds'] == 0
    assert jpeg444_decode['producers'] == jpeg444_rows['producers']
    for case in ['Decode8', 'NativeDecode8', 'NativeDecodeCancelable8']:
        observation = jpeg444_decode['cases'][case][-1]
        receipt = observation['receipt']
        assert len(observation['managed']['actual']) == 12 and all(q['passed'] for q in observation['quietness'].values())
        assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256']
        assert receipt['librarySha256'] == jpeg444_rows['producers']['linuxSha256']
        assert jpeg444_decode['comparisonLowerBounds'][case] > 1
    direct_rgb = json.loads((ROOT / 'benchmarks/current-direct-rgb-jpeg-decode-costs-evidence.json').read_text(encoding='utf-8'))
    assert not direct_rgb['productionChanged'] and direct_rgb['workflow']['allMeasurementBuilds'] == 0
    assert direct_rgb['producers'] == jpeg444_rows['producers']
    for case in ['DirectRgb444', 'DirectRgb422', 'DirectRgb420']:
        observation = direct_rgb['cases'][case][-1]
        receipt = observation['receipt']
        assert len(observation['managed']['actual']) == 12 and all(q['passed'] for q in observation['quietness'].values())
        assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256']
        assert receipt['librarySha256'] == jpeg444_rows['producers']['linuxSha256']
        assert direct_rgb['comparisonLowerBounds'][case] > 1
    caller_samples = json.loads((ROOT / 'benchmarks/current-caller-jpeg-decode-costs-evidence.json').read_text(encoding='utf-8'))
    assert not caller_samples['productionChanged'] and caller_samples['workflow']['allBuilds'] == 0
    assert caller_samples['producers'] == jpeg444_rows['producers']
    for case in ['Gray', 'Rgb']:
        observation = caller_samples['cases'][case][-1]
        receipt = observation['receipt']
        assert len(observation['managed']['actual']) == 12 and all(q['passed'] for q in observation['quietness'].values())
        assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256']
        assert receipt['librarySha256'] == jpeg444_rows['producers']['linuxSha256']
        assert receipt['callerPaddingUnchanged'] and receipt['independentOwnedOutput'] and receipt['noRgbaIntermediate']
    assert caller_samples['comparisonLowerBounds']['Gray'] > 1
    assert caller_samples['cases']['Rgb'][-1]['conservativeRatioUpper'] < 1
    mixed_costs = json.loads((ROOT / 'benchmarks/current-default-mixed-bicubic-costs-evidence.json').read_text(encoding='utf-8'))
    assert not mixed_costs['productionChanged'] and mixed_costs['allBuilds'] == mixed_costs['newCases'] == 0
    assert mixed_costs['producers'] == jpeg444_rows['producers']
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
    jpeg_vertical = json.loads((ROOT / 'benchmarks/jpeg-vertical-chroma-row-evidence.json').read_text(encoding='utf-8'))
    assert jpeg_vertical['accepted'] and jpeg_vertical['decision']['normalizedSeparated']
    assert jpeg_vertical['qualification']['fullChecksEachOs'] == 5354
    assert jpeg_vertical['qualification']['package']['sha256'] == jpeg_vertical['qualification']['linuxPackage']['sameActualPackageSha256']
    assert len(jpeg_vertical['cases']) == 11
    assert sum(s == 'confirmed-pass-stop' for s in jpeg_vertical['status'].values()) == 7
    updated_vertical = 0
    for case, prefix in [('CallerRGB420', '| JPEG 4:2:0 from caller RGB samples'),
                         ('GovernedRGBA16', '| JPEG RGBA16 4:2:0 / caller IMemoryBudget'),
                         ('JpegRgb420AsyncSamples', '| JPEG caller RGB24 async nonseekable, 420')]:
        observation = jpeg_vertical['cases'][case][-1]
        receipt, stats = observation['receipt'], observation['managed']['statistics']
        assert receipt['librarySha256'] == jpeg_vertical['producers']['linuxSha256']
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert all(q['passed'] for q in observation['quietness'].values())
        assert receipt['managedOutputBytes'] + 2 == receipt['nativeOutputBytes']
        if case == 'CallerRGB420':
            original = jpeg['cases']['Rgb420'][-1]['receipt']
            assert receipt['nativeReady']['frozenPacketExact'] and receipt['psnrGapDb'] == 0
            assert all(receipt[subject][field] == original[subject][field]
                       for subject in ('managedQuality', 'nativeQuality')
                       for field in ('encodedSha256', 'nativeDecodedSha256'))
        else:
            assert receipt.get('completePacketExact', receipt.get('encodedPacketsExact'))
        for index, (kind, row) in enumerate(rows):
            if row.startswith(prefix):
                label = row.split('|')[1].strip()
                if jpeg_vertical['status'][case] == 'confirmed-confidence-open':
                    label = label.replace(' †', ' (confidence open) †')
                rows[index] = (kind, f'| {label} | {stats["Mean"] / 1000:,.2f} | '
                               f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                               f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                               f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | '
                               f'{receipt["managedOutputBytes"]:,} / {receipt["nativeOutputBytes"]:,} |')
                updated_vertical += 1
    assert updated_vertical == 3
    # Retained gains remain claims about their original paired experiments.
    jpeg_rgb_result = json.loads((ROOT / 'benchmarks/jpeg-direct-rgb-result-evidence.json').read_text(encoding='utf-8'))
    caller_ycck = json.loads((ROOT / 'benchmarks/jpeg-caller-ycck-row-evidence.json').read_text(encoding='utf-8'))
    for retained in [jpeg_rgb_result, caller_ycck]:
        assert retained['accepted'] and retained['decision']['passed'] and retained['decision']['normalizedSeparated']
        assert retained['qualification']['sameActualPackageBothOs']
        assert retained['raw']['mirrored'] and retained['raw']['allMembersVerified']
    jpeg_scan = json.loads((ROOT / 'benchmarks/jpeg-container-scan-evidence.json').read_text(encoding='utf-8'))
    assert jpeg_scan['accepted'] and jpeg_scan['decision']['passed'] and jpeg_scan['decision']['normalizedSeparated']
    assert jpeg_scan['qualification']['fullChecksEachOs'] == 5369
    assert jpeg_scan['qualification']['sameActualPackageBothOs']
    assert jpeg_scan['raw']['mirrored'] and jpeg_scan['raw']['allMembersVerified']
    assert len(jpeg_scan['cases']) == 29
    assert jpeg_scan['cases']['Decode8']['status'] == 'confirmed-pass-stop'
    for cost in jpeg_scan['cases'].values():
        assert cost['managedActuals'] == cost['nativeSamplesEachBracket'] == 12 and cost['quietGuardsPassed']
        assert (cost['status'] == 'confirmed-pass-stop') == (cost['upper'] <= 1)
        assert math.isclose(cost['managedMicroseconds'] / cost['nativeMicroseconds'], cost['ratio'], rel_tol=1e-12)
    jpeg_confidence = json.loads((ROOT / 'benchmarks/current-jpeg-confidence-evidence.json').read_text(encoding='utf-8'))
    assert not jpeg_confidence['productionChanged'] and jpeg_confidence['allBuilds'] == 0
    assert jpeg_confidence['independentConfirmations'] == 6 and jpeg_confidence['newCases'] == 0
    assert set(jpeg_confidence['cases']) == {'NativeDecode420', 'NativeDecode422', 'OwnedDecodeGray',
                                            'CmykCaller', 'CallerGray', 'ValidateJpegProgressive420'}
    assert jpeg_confidence['producers'] == jpeg_scan['producers']
    assert jpeg_confidence['raw']['mirrored'] and jpeg_confidence['raw']['allMembersVerified']
    for case, cost in jpeg_confidence['cases'].items():
        assert jpeg_scan['cases'][case]['status'] == 'confirmed-confidence-open'
        assert cost['managedActuals'] == cost['nativeSamplesEachBracket'] == 12 and cost['quietGuardsPassed']
        assert (cost['status'] == 'confirmed-pass-stop') == (cost['upper'] <= 1)
        assert (cost['status'] == 'confirmed-gap') == (cost['lower'] > 1)
        assert math.isclose(cost['managedMicroseconds'] / cost['nativeMicroseconds'], cost['ratio'], rel_tol=1e-12)
        jpeg_scan['cases'][case] = cost
    replacements = [
        ('NativeDecode420', 'JPEG baseline420, resident decode'),
        ('CmykCaller', 'JPEG DC-only, caller CMYK decode'),
        ('CmykAdobeSparse2D', 'JPEG CMYK sparse-2D, resident decode'),
        ('Ycck420', 'JPEG YCCK420, resident decode'),
        ('Decode411', 'JPEG 4:1:1, resident decode'),
        ('Decode410', 'JPEG 4:1:0, resident decode'),
        ('ProgressiveGray', 'JPEG progressive grayscale, resident decode'),
        ('Progressive420', 'JPEG progressive420, resident decode'),
        ('Progressive422', 'JPEG progressive422, resident decode'),
        ('Progressive444', 'JPEG ordinary progressive444, resident decode'),
        ('ProgressiveCancelable444', 'JPEG progressive444, live-token resident decode'),
        ('CmykAdobeGeneral', 'JPEG dense CMYK, resident decode'),
        ('CmykAdobeAc', 'JPEG horizontal-only CMYK, resident decode'),
        ('Ycck420CallerRgb', 'JPEG YCCK420, caller RGB24 decode'),
    ]
    for case, label in replacements:
        c = jpeg_scan['cases'][case]
        qualifier = ' (confirmed pass)' if c['status'] == 'confirmed-pass-stop' else ' (confidence open)' if c['status'] == 'confirmed-confidence-open' else ''
        row = (f'| {label}{qualifier} † | {c["managedMicroseconds"]:,.2f} | '
               f'{c["nativeMicroseconds"]:,.2f} | {c["ratio"]:.3f} / {c["upper"]:.3f} | '
               f'{c["allocatedBytes"]:,} | — |')
        if case == 'Ycck420CallerRgb':
            rows.append(('codec', row))
        else:
            position = next(i for i, (kind, previous) in enumerate(rows) if previous.startswith('| ' + label))
            rows[position] = ('codec', row)
    position = next(i for i, (_, row) in enumerate(rows) if row.startswith('| VP8 opaque photograph'))
    rows.insert(position, ('codec',
        f'| VP8 RGBA16 + alpha, including normalization † | {alpha16_cost["managedMicroseconds"]:,.2f} | '
        f'{alpha16_cost["nativeMicroseconds"]:,.2f} | {alpha16_cost["ratio"]:.3f} / {alpha16_cost["upper"]:.3f} | '
        f'{alpha16_cost["allocatedBytes"]:,} | {alpha16_cost["packetBytes"]:,} / {alpha16_cost["packetBytes"]:,} |'))
    rows.insert(position + 1, ('codec',
        f'| VP8 opaque RGBA8 † | {opaque8_cost["managedMicroseconds"]:,.2f} | '
        f'{opaque8_cost["nativeMicroseconds"]:,.2f} | {opaque8_cost["ratio"]:.3f} / {opaque8_cost["upper"]:.3f} | '
        f'{opaque8_cost["allocatedBytes"]:,} | {opaque8_cost["packetBytes"]:,} / {opaque8_cost["packetBytes"]:,} |'))
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
    composites = json.loads((ROOT / 'benchmarks/compositing-shared-rows-evidence.json').read_text(encoding='utf-8'))
    assert composites['productionAccepted'] and composites['independentQuietConfirmation']
    assert composites['qualification']['fullChecksEachOs'] == 5423
    assert composites['qualification']['actualPackage']['sha256'] == composites['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert composites['cases']['DrawingComposite8']['status'] == 'Confirmed gap'
    assert composites['cases']['DrawingComposite16']['status'] == 'Confirmed pass'
    raster = json.loads((ROOT / 'benchmarks/text-edge-interval-preparation-evidence.json').read_text(encoding='utf-8'))
    assert raster['productionAccepted'] and raster['independentQuietConfirmation']
    assert raster['qualification']['fullChecksEachOs'] == 5466
    assert raster['qualification']['actualPackage']['sha256'] == raster['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert all(c['rawGainConfidenceSeparates'] and c['normalizedGainConfidenceSeparates'] for c in raster['comparison'])
    for case_id, previous in text_current['observations'].items():
        case = raster['cases'].get(case_id, composites['cases'].get(case_id, previous))
        if case_id in raster['cases']:
            assert case['librarySha256'] == raster['producers']['linuxSha256']
            assert case['validation']['frozenManagedObservationsExact']
            assert case['managedMemory']['BytesAllocatedPerOperation'] == 83712
            assert case['admittedWorkspacePeak'] == 48544 and case['retainedDrawingWorkspace'] == 0
        if case_id in composites['cases']:
            assert case['librarySha256'] == composites['producers']['linuxSha256']
            assert case['validation']['exactManagedPixels'] and case['managedMemory']['BytesAllocatedPerOperation'] == 56
        assert len(case['managed']['actual']) == 12 and case['quietGuardsPassed']
        assert case['approvedScopeDate'] == '2026-11-02'
        native_mean = min(bracket['mean'] for bracket in case['nativeBrackets'])
        status = 'pass' if case['status'] == 'Confirmed pass' else 'gap' if case['status'] == 'Confirmed gap' else 'confidence open'
        rows.append(('editing', f'| {case["label"]} ({status}) | {case["managed"]["statistics"]["Mean"] / 1000:,.2f} | '
                     f'{native_mean / 1000:,.2f} | {case["worstBracketRatio"]:.3f} / {case["conservativeRatioUpper"]:.3f} | '
                     f'{case["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
    lanczos_current = json.loads((ROOT / 'benchmarks/lanczos-paired-row-output-evidence.json').read_text(encoding='utf-8'))
    assert lanczos_current['productionAccepted'] and lanczos_current['independentQuietConfirmation']
    assert lanczos_current['qualification']['fullChecksEachOs'] == 5545
    assert lanczos_current['qualification']['actualPackage']['sha256'] == lanczos_current['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert len(lanczos_current['cases']) == 8
    assert sum(c['status'] == 'Confirmed pass' for c in lanczos_current['cases'].values()) == 5
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
        assert observation['managedExact'] and observation['nativeQualityPassed'] and observation['quietGuardsPassed']
        assert observation['librarySha256'] == lanczos_current['producers']['linuxSha256']
        assert len(observation['managed']['actual']) == 12
        stats = observation['managed']['statistics']
        native = min(b['mean'] for b in observation['nativeBrackets'])
        current_resize_rows.append(('editing', f'| {label} † → {size} | {stats["Mean"] / 1000:,.2f} | '
                                    f'{native / 1000:,.2f} | {observation["worstBracketRatio"]:.3f} / '
                                    f'{observation["conservativeRatioUpper"]:.3f} | '
                                    f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |'))
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
    for case, label in [('alpha', 'VP8 RGBA8 + lossless alpha'),
                        ('photograph', 'VP8L photograph / libwebp exact=1'),
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
    png_output = json.loads((ROOT / 'benchmarks/png-tree-workspace-evidence.json').read_text(encoding='utf-8'))
    assert png_output['productionAccepted'] and png_output['independentQuietConfirmation']
    assert png_output['qualification']['fullChecksEachOs'] == 5569
    assert png_output['qualification']['actualPackage']['sha256'] == png_output['qualification']['samePackageLinux']['sameActualPackageSha256']
    assert png_output['storage']['allocationSavingsSameBatch'] == {'EncodeSmallest16': 834544, 'EncodeSmallest8': 94960}
    assert not any(c['cpuGainOrRegressionClaim'] for c in png_output['comparisons'].values())
    # Dynamic encoding instances reuse operation-owned tree preparation storage.
    # Decode/inspection paths are separate and retain their applicable costs.
    rows = [(kind, row.replace(' † |', ' † H |', 1)
             if kind == 'codec' and row.startswith('| PNG ') and 'decode' not in row.lower()
             else row) for kind, row in rows]
    for case, old_label, label in [
            ('Encode8', 'PNG RGBA8, Optimal6 / native6', 'PNG RGBA8, one-shot Optimal6 / native6'),
            ('EncodeFast8', 'PNG RGBA8, Fastest1 / native1', 'PNG RGBA8, one-shot Fastest1 / native1'),
            ('Encode16', 'PNG RGBA16, Optimal6 / native6', 'PNG RGBA16, one-shot Optimal6 / native6'),
            ('EncodeFast16', 'PNG RGBA16, Fastest1 / native1', 'PNG RGBA16, one-shot Fastest1 / native1'),
            ('EncodeSmallest8', 'PNG RGBA8, SmallestSize9 / native9', 'PNG RGBA8, one-shot SmallestSize9 / native9'),
            ('EncodeSmallest16', 'PNG RGBA16, SmallestSize9 / native9', 'PNG RGBA16, one-shot SmallestSize9 / native9')]:
        observation = png_output['cases'][case]; receipt = observation['receipt']
        assert observation['status'] == ('Uncertain parity' if case == 'Encode8' else 'Confirmed gap')
        if observation['status'] == 'Confirmed gap':
            assert observation['conservativeRatioLower'] > 1
        assert observation['librarySha256'] == png_output['producers']['linuxSha256']
        assert observation['completePacketExact'] and observation['quietGuardsPassed']
        assert len(observation['managed']['actual']) == 12
        assert receipt['nativeReady']['codecPreparationTimed'] and not receipt['nativeReady']['codecRetainedBetweenOperations']
        if receipt['depth'] == 16:
            assert receipt['nativeReady']['wordWorkspacePreparationTimed']
            assert not receipt['nativeReady']['wordWorkspaceRetainedBetweenOperations']
        assert receipt['managedEncodedSha256'] == receipt['nativeOutputSha256']
        stats = observation['managed']['statistics']; native = min(b['mean'] for b in observation['nativeBrackets'])
        position = next(i for i, (_, row) in enumerate(rows) if row.startswith('| ' + old_label + ' † H |'))
        rows[position] = ('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                          f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                          f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | '
                          f'{receipt["managedOutputBytes"]:,} / {receipt["nativeOutputBytes"]:,} |')
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
    rows = [(kind, row.replace(' ‡', ' H').replace(' §', ' H').replace(' ¶', ' H'))
            for kind, row in rows]
    # Current VP8 family costs use one canonical receipt shape, independently of
    # the experiment reports retained above for unaffected paths and history.
    texture = json.loads((ROOT / 'benchmarks/vp8-source-texture-preparation-evidence.json').read_text(encoding='utf-8'))
    assert texture['productionAccepted'] and texture['independentQuietConfirmation']
    assert texture['qualification']['fullChecksEachOs'] == 5489
    assert texture['qualification']['actualPackage']['sha256'] == texture['qualification']['samePackageLinux']['sameActualPackageSha256']
    labels = {
        'PhotographLossy8': 'VP8 opaque photograph',
        'DiagramLossy8': 'VP8 opaque text/diagram',
        'DiagramLossyTrellis8': 'VP8 diagram, effort6 / trellis',
        'Opaque8': 'VP8 opaque RGBA8',
        'Opaque16': 'VP8 opaque RGBA16, including normalization',
        'Alpha8': 'VP8 RGBA8 + lossless alpha',
        'Alpha16': 'VP8 RGBA16 + alpha, including normalization',
    }
    assert set(texture['cases']) == set(labels)
    for case, label in labels.items():
        # One current owner for affected lossless-alpha costs; opaque VP8
        # reuses its unchanged source-texture evidence. No history overlay chain.
        producer = webp_current if case in {'Alpha8', 'Alpha16'} else texture
        observation = producer['cases'][{'Alpha8': 'alpha', 'Alpha16': 'alpha16'}.get(case, case)]
        receipt = observation['receipt']
        assert receipt['librarySha256'] == producer['producers']['linuxSha256']
        assert observation['completePacketExact'] and observation['quietGuardsPassed']
        assert observation['status'] == 'Confirmed gap' and observation['conservativeRatioLower'] > 1
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
        assert receipt['managedOutputBytes'] == receipt['nativeOutputBytes']
        assert receipt.get('managedOutputSha256', receipt.get('managedEncodedSha256')) == receipt['nativeOutputSha256']
        positions = [i for i, (_, row) in enumerate(rows) if row.split('|')[1].strip().removesuffix(' H').removesuffix(' †') == label]
        assert len(positions) == 1, label
        stats = observation['managed']['statistics']; native = min(b['mean'] for b in observation['nativeBrackets'])
        rows[positions[0]] = ('codec', f'| {label} † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | '
                             f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                             f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | '
                             f'{receipt["managedOutputBytes"]:,} / {receipt["nativeOutputBytes"]:,} |')
    # The output allocator changes both filters; overlay only refreshed controls.
    overwrite = json.loads((ROOT / 'benchmarks/resize-overwrite-output-storage-evidence.json').read_text(encoding='utf-8'))
    assert overwrite['productionAccepted'] and overwrite['independentQuietConfirmation']
    assert overwrite['qualification']['fullChecksEachOs'] == 5497
    assert overwrite['qualification']['actualPackage']['sha256'] == overwrite['qualification']['samePackageLinux']['sameActualPackageSha256']
    rows = [(kind, row.replace(' →', ' H →', 1)
             if kind == 'editing' and row.startswith(('| Bicubic', '| Bilinear')) and ' H ' not in row
             else row) for kind, row in rows]
    labels = {'Down8': 'Bicubic resize RGBA8', 'Up16': 'Bicubic upscale RGBA16',
              'Down32x16_8': 'Bicubic RGBA8, 32×16 / default workspace',
              'Up8': 'Bicubic upscale RGBA8, default workspace'}
    assert set(overwrite['cases']) == set(labels)
    for case, label in labels.items():
        observation = overwrite['cases'][case]; receipt = observation['receipt']
        assert receipt['librarySha256'] == overwrite['producers']['linuxSha256']
        assert receipt['managedRationalReferenceExact'] and observation['quietGuardsPassed']
        assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
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
    # One compact owner for the eight current WebP costs with scoped unchanged-path reuse. Older reports
    # above retain their source-specific proofs; do not copy their experiment
    # histories or substitute old costs for this accepted stage's current rows.
    webp_current = json.loads((ROOT / 'benchmarks/webp-color-histogram-separation-evidence.json').read_text(encoding='utf-8'))
    assert webp_current['productionAccepted'] and webp_current['cpuGainClaim']
    assert webp_current['confirmation']['photograph']['rawGainConfidenceSeparates'] and webp_current['confirmation']['photograph']['normalizedGainConfidenceSeparates']
    qualification = webp_current['qualification']
    assert qualification['windows']['total'] == qualification['linux']['total'] == 5541
    assert qualification['actualPackage']['sha256'] == qualification['samePackageLinux']['sameActualPackageSha256']
    webp_reconciled = json.loads((ROOT / 'benchmarks/vp8l-invariant-alpha-preparation-inconclusive-evidence.json').read_text(encoding='utf-8'))
    assert not webp_reconciled['productionChanged'] and not webp_reconciled['productionAccepted'] and not webp_reconciled['cpuGainClaim']
    assert set(webp_reconciled['currentCosts']) == {'opaque', 'photograph'}
    assert all(webp_reconciled['restoration'][os]['corePdbXmlSourcesAndInternalModuleRestored'] for os in ['windows', 'linux'])
    assert webp_reconciled['restoration']['windows']['packageSha256'] == webp_reconciled['restoration']['linux']['packageSha256']
    current_labels = {
        'ordinary': 'VP8L RGBA8 / libwebp exact=1',
        'photograph': 'VP8L photograph / libwebp exact=1',
        'diagram': 'VP8L text/diagram / libwebp exact=1',
        'opaque': 'VP8L opaque RGBA8 / libwebp exact=1',
        'rgba16': 'VP8L RGBA16, including normalization / libwebp exact=1',
        'async': 'VP8L async, fresh nonseekable output / libwebp exact=1',
        'alpha': 'VP8 RGBA8 + lossless alpha',
        'alpha16': 'VP8 RGBA16 + alpha, including normalization',
    }
    assert set(webp_current['cases']) == set(current_labels)
    for case, label in current_labels.items():
        current = webp_reconciled['currentCosts'].get(case, webp_current['cases'][case])
        assert current['actualIterations'] == 12 and current['quietGuardsPassed'] and current['allOriginalGatesPassed']
        expected_producer = (webp_reconciled['restoration']['linux']['productionSha256'] if case in webp_reconciled['currentCosts']
                             else webp_current['producers']['unchangedAlphaLinuxSha256'] if case in {'alpha', 'alpha16'} else webp_current['producers']['linuxSha256'])
        assert current['librarySha256'] == expected_producer
        assert current['completePacketExact'] and current['packetSha256'] == current['nativePacketSha256']
        assert current['status'] == ('pass' if current['upper'] <= 1 else 'gap' if current['lower'] > 1 else 'uncertain')
        if case in webp_reconciled['currentCosts']:
            assert current['strictParityStatus'] == 'open' and current['pairedLower'] > 1
            assert current['pairedUpper'] == webp_reconciled['pairedCurrentCosts'][case]['upper']
        positions = [i for i, (_, row) in enumerate(rows) if row.split('|')[1].strip().removesuffix(' H').removesuffix(' †') == label]
        assert len(positions) == 1, label
        rows[positions[0]] = ('codec', f'| {label} † | {current["managedUs"]:,.2f} | {current["nativeUs"]:,.2f} | '
                             f'{current["ratio"]:.3f} / {current["upper"]:.3f} | {current["allocatedBytes"]:,} | '
                             f'{current["managedPacketBytes"]:,} / {current["nativePacketBytes"]:,} |')
    header = '| Operation / fixed control | Lokad µs | Reference µs | Ratio / upper | Lokad allocated B/op |'
    separator = '| --- | ---: | ---: | ---: | ---: |'
    codecs = '\n'.join([header + ' Packet B, Lokad / reference |', separator + ' ---: |',
                         *(row for kind, row in rows if kind == 'codec')])
    editing = '\n'.join([header, separator, *(row for kind, row in rows if kind == 'editing')])
    read_labels = ['Probe PNG dimensions, seekable', 'Probe JPEG dimensions, seekable',
                   'Probe WebP dimensions, seekable', 'Probe JPEG dimensions, async nonseekable',
                   'Validate PNG8, span', 'Validate PNG16, span', 'Validate indexed Adam7 19×17, span',
                   'Validate JPEG baseline420, span', 'Validate JPEG progressive420, span',
                   'Validate VP8L+alpha, span', 'Validate VP8+alpha, span',
                   'Validate PNG16, async nonseekable']
    read_rows = []
    for label, (case, observations) in zip(read_labels, reads['cases'].items(), strict=True):
        producer = (png_probe if case in png_probe['cases'] else header_skips if case in header_skips['cases'] else png_indexed if case in png_indexed['cases'] else refinement_window if case in refinement_window['cases'] else vp8_stream if case == 'ValidateVp8Alpha' else resident_decode if case == 'ValidateVp8lAlpha' else png_gfni if case in png_gfni['cases'] else baseline_blocks if case == 'ValidateJpegBaseline420' else png_average if case in png_average['cases'] else png_crc_scan if case in png_crc_scan['cases'] else vp8_validation if case in vp8_validation['cases'] else png_cursor if case in png_cursor['cases']
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
    for case, label in [('ValidateJpegBaseline420', 'Validate JPEG baseline420, span'),
                        ('ValidateJpegProgressive420', 'Validate JPEG progressive420, span')]:
        c = jpeg_scan['cases'][case]
        status = 'Confirmed pass' if c['status'] == 'confirmed-pass-stop' else 'Confidence open' if c['status'] == 'confirmed-confidence-open' else 'Confirmed gap'
        position = next(i for i, row in enumerate(read_rows) if row.startswith('| ' + label))
        read_rows[position] = (f'| {label} | {c["managedMicroseconds"]:,.3f} | {c["nativeMicroseconds"]:,.3f} | '
                               f'{c["ratio"]:.3f} / {c["upper"]:.3f} | {c["allocatedBytes"]:,} | {status} |')
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
    read_table = '\n'.join([header + ' Status |', separator + ' --- |', *read_rows])
    displayed_count = sum(len(table.splitlines()) - 2 for table in (codecs, editing, read_table))
    return f"""# Lokad.Image benchmarks

Updated 2026-11-04. These **{displayed_count} selected comparisons** combine qualified
**0.2.1 development** costs with retained **0.2.0** evidence. Full parity remains
open across the [282 required paths](../benchmarks/WARM-COVERAGE.md). **†** marks
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

Current priorities and scope:

- **WebP:** [qualified candidate/token workspace reuse](../benchmarks/WEBP-TOKEN-TAIL-HISTORY.md)
  leaves alpha8 at 8,146,632 B/op, admitted peak 3,656,309 B and zero retained scratch.
  [Lossless photograph/opaque](../benchmarks/VP8L-INVARIANT-ALPHA-PREPARATION-INCONCLUSIVE.md)
  retain strict paired gaps (uppers 1.026/1.028), despite photograph's global
  upper 0.997. Original/diagram/depth/async passing cases stop. VP8 CPU gaps remain;
  [checkpoint screens](../benchmarks/VP8-BOUNDED-CHECKPOINT.md) are inconclusive
  and leave production/costs unchanged.
- **PNG:** [operation-owned tree reuse](../benchmarks/PNG-TREE-WORKSPACE.md)
  saves 834,544/94,960 B per complete word/byte Smallest9 encode (24.4%/10.2%, same
  batching), without a CPU gain/equivalence/regression claim. Five current gaps;
  byte Optimal6 remains uncertain (upper 1.055), to resolve before tuning. Existing
  384 KiB admission; 12,648 B tree payload during operation, zero after disposal.
- **Resizing:** [current Lanczos3](../benchmarks/LANCZOS-PAIRED-ROW-OUTPUT.md)
  has five passing boundaries; prepared byte reduction and word reduction/
  enlargement retain gaps (uppers 1.240/1.115/1.080). One-shot and prepared calls
  compare equivalent setup/ownership. Plans retain 139,800/148,040 B for byte/word
  reduction and 325,156/333,396 B for enlargement. Managed Q30/rational/certified
  pixels stay exact; zimg uses float. [Bicubic](../benchmarks/BICUBIC-SHARED-CACHED-DRIVER.md)
  retains native graphs/scratch while managed preparation is timed; affected H
  costs need reconciliation.
- **JPEG and reading:** current table statuses and
  [coverage](../benchmarks/WARM-COVERAGE.md) retain their named color/sampling/
  delivery scope. Sub-microsecond probe/copy gaps remain visible, with expensive
  codec/resize paths prioritized. Inspection, complete Identify and coding
  validation are distinct: PNG header inspection skips CRCs; Identify validates
  the container and nine source facts without pixel decoding; validation checks
  coding syntax without retaining an RGBA result.
- **Text/drawing:** [layout/fitting and RGBA16 compositing](../benchmarks/TEXT-DRAWING-CURRENT.md)
  pass their scoped comparisons. Loading, RGBA8 compositing and
  [rasterization](../benchmarks/TEXT-EDGE-INTERVAL-PREPARATION.md) retain gaps.
  These independent cases do not block codecs or resizing.

The current build passes **5,569 full/live checks per OS**, affected normal/scalar
checks, and the **same actual Release package/all three examples on Windows/Linux**.
Unchanged paths reuse valid evidence. This is correctness/package qualification,
not a full release refresh or whole-library performance pass. Complete physical/
admitted peaks are not inferred from allocation reductions.

**References and boundaries.** FFmpeg is the reference except for explicitly
labeled **libwebp exact=1** lossless WebP (hidden RGB preserved), SIMD-enabled
**libjpeg-turbo** grayscale JPEG, and **zimg** resizing (FFmpeg's zscale backend).
The six current one-shot PNG rows time fresh codec/zlib/packet state and independent
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
    output = ROOT / 'docs/benchmark.md'
    if '--check' in sys.argv:
        assert output.read_text(encoding='utf-8') == text, 'Benchmark summary differs from retained evidence.'
        print('Benchmark summary matches current and retained release evidence (three tables).')
    else:
        output.write_text(text, encoding='utf-8', newline='\n')
