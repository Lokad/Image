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
    trellis_cost = json.loads((ROOT / 'benchmarks/current-vp8-effort6-cost-evidence.json').read_text(encoding='utf-8'))
    assert not trellis_cost['productionChanged'] and trellis_cost['allBuilds'] == 0
    assert trellis_cost['independentQuietConfirmation']
    vp8l_word_cost = json.loads((ROOT / 'benchmarks/current-vp8l-rgba16-cost-evidence.json').read_text(encoding='utf-8'))
    assert not vp8l_word_cost['productionChanged'] and vp8l_word_cost['allBuilds'] == 0
    assert vp8l_word_cost['independentQuietConfirmation']
    vp8l_photo_cost = json.loads((ROOT / 'benchmarks/vp8l-shared-color-mask-rejected-evidence.json').read_text(encoding='utf-8'))
    assert not vp8l_photo_cost['productionChanged'] and vp8l_photo_cost['candidateRejected']
    assert vp8l_photo_cost['independentQuietConfirmation']
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
    vp8_stream = json.loads((ROOT / 'benchmarks/warm-vp8l-direct-output-evidence.json').read_text(encoding='utf-8'))
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
    probes = json.loads((ROOT / 'benchmarks/warm-probe-completed-reads-evidence.json').read_text(encoding='utf-8'))
    opaque_vp8l = json.loads((ROOT / 'benchmarks/current-opaque-vp8l-encode-cost-evidence.json').read_text(encoding='utf-8'))
    assert not opaque_vp8l['productionChanged'] and opaque_vp8l['measurementBuilds'] == 0
    assert opaque_vp8l['independentQuietConfirmation'] and opaque_vp8l['status']['EncodeOpaque8'] == 'confirmed-gap'
    webp_probe = json.loads((ROOT / 'benchmarks/current-webp-dimension-probe-cost-evidence.json').read_text(encoding='utf-8'))
    assert not webp_probe['productionChanged'] and webp_probe['allBuilds'] == 0
    assert webp_probe['independentQuietConfirmation'] and webp_probe['status']['ProbeDimensionsWebp'] == 'confirmed-gap'
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
    lanczos_cost = json.loads((ROOT / 'benchmarks/current-lanczos3-reduce8-cost-evidence.json').read_text(encoding='utf-8'))
    assert not lanczos_cost['productionChanged'] and lanczos_cost['productionHarnessNativeBuilds'] == 0
    assert lanczos_cost['selectorBuilds'] == 1 and lanczos_cost['independentQuietConfirmation']
    lanczos_word_cost = json.loads((ROOT / 'benchmarks/current-lanczos3-reduce16-cost-evidence.json').read_text(encoding='utf-8'))
    assert not lanczos_word_cost['productionChanged'] and lanczos_word_cost['productionHarnessNativeBuilds'] == 0
    assert lanczos_word_cost['selectorBuilds'] == 1 and lanczos_word_cost['independentQuietConfirmation']
    lanczos_enlarge8_cost = json.loads((ROOT / 'benchmarks/current-lanczos3-enlarge8-cost-evidence.json').read_text(encoding='utf-8'))
    assert not lanczos_enlarge8_cost['productionChanged'] and lanczos_enlarge8_cost['allBuilds'] == 0
    assert lanczos_enlarge8_cost['independentQuietConfirmation']
    lanczos_enlarge16_cost = json.loads((ROOT / 'benchmarks/current-lanczos3-enlarge16-cost-evidence.json').read_text(encoding='utf-8'))
    assert not lanczos_enlarge16_cost['productionChanged'] and lanczos_enlarge16_cost['allBuilds'] == 0
    assert lanczos_enlarge16_cost['independentQuietConfirmation']
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
    bicubic_byte_cost = json.loads((ROOT / 'benchmarks/current-bicubic-byte-reduction-cost-evidence.json').read_text(encoding='utf-8'))
    assert not bicubic_byte_cost['productionChanged'] and bicubic_byte_cost['allBuilds'] == 0
    assert bicubic_byte_cost['independentQuietConfirmation'] and bicubic_byte_cost['status']['Down8'] == 'confirmed-gap'
    assert len(bicubic_byte_cost['cases']['Down8'][-1]['managed']['actual']) == 12
    editing_costs = {'case21': bilinear_word,
                     'case22': bicubic_byte_cost['cases']['Down8'][-1],
                     'case23': word_publication['cases']['Up16'][-1],
                     'case24': cross16_cost['cases']['TightCross16'][-1],
                     'case27': lanczos_cost['cases']['Reduce8'][-1],
                     'case28': lanczos_word_cost['cases']['Reduce16'][-1],
                     'case29': lanczos_enlarge8_cost['cases']['Enlarge8'][-1],
                     'case30': lanczos_enlarge16_cost['cases']['Enlarge16'][-1]}
    editing_producers = {'case21': bilinear_word_cost,
                         'case22': bicubic_byte_cost, 'case23': word_publication,
                         'case24': cross16_cost, 'case27': lanczos_cost,
                         'case28': lanczos_word_cost, 'case29': lanczos_enlarge8_cost,
                         'case30': lanczos_enlarge16_cost}
    streamed = json.loads((ROOT / 'benchmarks/warm-png-streaming-validation-evidence.json').read_text(encoding='utf-8'))
    reconciled = json.loads((ROOT / 'benchmarks/warm-read-reconciliation-evidence.json').read_text(encoding='utf-8'))
    resident = json.loads((ROOT / 'benchmarks/current-png-resident-identify-evidence.json').read_text(encoding='utf-8'))
    resident_other = json.loads((ROOT / 'benchmarks/current-jpeg-webp-resident-identify-evidence.json').read_text(encoding='utf-8'))
    stream_identify = json.loads((ROOT / 'benchmarks/current-png-stream-identify-evidence.json').read_text(encoding='utf-8'))
    decoders = json.loads((ROOT / 'benchmarks/current-default-decoders-evidence.json').read_text(encoding='utf-8'))
    resident_decode = json.loads((ROOT / 'benchmarks/warm-vp8l-direct-output-evidence.json').read_text(encoding='utf-8'))
    indexed_decode = json.loads((ROOT / 'benchmarks/warm-vp8l-direct-output-evidence.json').read_text(encoding='utf-8'))
    indexed_observation = indexed_decode['cases']['IndexedNative16Colors8'][-1]
    assert indexed_decode['productionChanged'] and indexed_decode['independentQuietConfirmation']
    assert indexed_decode['producers'] == resident_decode['producers']
    assert len(indexed_observation['managed']['actual']) == 12
    assert indexed_observation['receipt']['allRgbaSamplesExact']
    assert indexed_observation['receipt']['managedOutputSha256'] == indexed_observation['receipt']['nativeOutputSha256']
    assert all(q['passed'] for q in indexed_observation['quietness'].values())
    live_decode = json.loads((ROOT / 'benchmarks/warm-vp8l-direct-output-evidence.json').read_text(encoding='utf-8'))
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
    assert resident_decode['memory']['measuredWarmReductionBytes'] == 531649
    assert resident_decode['memory']['governedBefore']['decode']['peakAdmittedBytes'] == 1063480
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
    vp8_cases = {'case06': 'DiagramLossyDistortion8', 'case07': 'Alpha8',
                 'case08': 'PhotographLossy8', 'case09': 'DiagramLossy8',
                 'case10': 'DiagramLossyTrellis8', 'case11': 'Opaque16'}
    rows = []
    for case in evidence['cases']:
        observation = case['observation']
        vp8l_case = case['id'] in {'case12', 'case13', 'case14', 'case15'}
        vp8_case = case['id'] in vp8_cases
        updated = vp8_case or vp8l_case or case['id'] in editing_costs or case['id'] in {'case00', 'case02', 'case03', 'case17', 'case18', 'case19', 'case20'}
        producer = vp8_quantization if vp8_case else jpeg if case['id'] in {'case02', 'case03'} else vp8_stream if case['id'] == 'case00' else vp8l if vp8l_case else stored if case['id'] == 'case20' else current
        if case['id'] == 'case10':
            producer = trellis_cost
        elif case['id'] == 'case13':
            producer = vp8l_word_cost
        elif case['id'] == 'case15':
            producer = vp8l_photo_cost
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
        samples = 12 if case['id'] in {'case00', 'case02', 'case08', 'case10', 'case12', 'case13', 'case15', 'case21', 'case22', 'case23', 'case24', 'case27', 'case28', 'case29', 'case30'} or updated and receipt['caseId'] in {'EncodeSmallest8', 'EncodeSmallest16', 'EncodeNoCompression16', 'EncodeFast8'} else 3
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
    observation = opaque_vp8l['cases']['EncodeOpaque8'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == opaque_vp8l['producers']['linuxSha256'] and receipt['completePacketExact']
    assert receipt['managedOutputBytes'] == receipt['nativeOutputBytes'] == 121896
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert all(q['passed'] for q in observation['quietness'].values())
    native = min(b['mean'] for b in observation['nativeBrackets'])
    opaque_row = f'| VP8L opaque RGBA8 / libwebp exact=1 † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | {observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | {observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | 121,896 / 121,896 |'
    position = next(i for i, (kind, row) in enumerate(rows) if row.startswith('| VP8L RGBA8 /'))
    rows.insert(position + 1, ('codec', opaque_row))
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
        producer = (vp8_stream if case == 'ValidateVp8Alpha' else resident_decode if case == 'ValidateVp8lAlpha' else png_gfni if case in png_gfni['cases'] else baseline_blocks if case == 'ValidateJpegBaseline420' else png_average if case in png_average['cases'] else png_crc_scan if case in png_crc_scan['cases'] else vp8_validation if case in vp8_validation['cases'] else png_cursor if case in png_cursor['cases']
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
    headers = reconciled['cases']['PngHeaders'][-1]
    assert headers['receipt']['descriptorExact'] and headers['receipt']['typedSourceMetadataExact']
    assert headers['receipt']['librarySha256'] == reconciled['producers']['linuxSha256']
    assert len(headers['managed']['actual']) == 3 and all(q['passed'] for q in headers['quietness'].values())
    read_rows.append(f'| Inspect PNG8 complete headers, seekable, adapter reference | '
                     f'{headers["managed"]["statistics"]["Mean"] / 1000:,.3f} | '
                     f'{min(b["mean"] for b in headers["nativeBrackets"]) / 1000:,.3f} | '
                     f'{headers["worstBracketRatio"]:.3f} / {headers["conservativeRatioUpper"]:.3f} | '
                     f'{headers["managedMemory"]["BytesAllocatedPerOperation"]:,} | Screen gap |')
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
    return f"""# Lokad.Image benchmarks

Updated 2026-10-31. Rows combine qualified **0.2.1 development** evidence with
retained 0.2.0 measurements whose relevant production paths remain unchanged.
These **59 comparisons** rank the remaining work; completion requires every
material path in the [268-case coverage checklist](../benchmarks/WARM-COVERAGE.md).

Measurements run sequentially on the dedicated quiet Ubuntu x64 VM
({evidence['machine']['cpuModel']}), with one pinned CPU, warm .NET 10, tiering
and concurrent GC disabled. Each operation includes admission, normalization,
input delivery as labeled, fresh output ownership and release. Startup,
preparation, transport and validation are excluded. Native brackets and the
versioned idle/process guards pass. Ratios use the faster native bracket;
**upper** includes conservative filtered/unfiltered confidence bounds.
Managed **B/op** is measured allocation; native allocation is unmeasured.

All twelve displayed PNG comparisons affected by
[GFNI CRC reversal](../benchmarks/WARM-PNG-GFNI-CRC.md) have fresh independent
confirmations. Complete seekable Identify improves **14.16%**, with unchanged
192 B/op, but remains 1.226× / upper 1.232. Resident Identify remains a small
confirmed gap; PNG8 validation is confidence-open and is retained without
retiming. Four PNG controls pass at this tested scope. Header/probe paths skip
CRC and retain applicable evidence. Other affected PNG paths need current costs
before CPU-pass claims.
Two resident PNG decode controls also remain confidence-open on this build;
[None/Paeth evidence](../benchmarks/CURRENT-PNG-NONE-PAETH-CRC-COSTS.md) retains
one confirmation each without retiming or assuming an optimization need.
The existing [RGBA8/None decode](../benchmarks/CURRENT-PNG-RGBA8-CRC-COST.md)
separately confirms a 1.092× gap (upper 1.098), with 4,472 B/op.

Non-indexed VP8L decoding now owns its decoded storage directly: **2.73%** less
elapsed time, **531,649 fewer allocated B/op** and **41.4% lower governed peak**.
Eight affected companions have current costs. Literal/run passes at **0.692× /
upper 0.696**, 10,840 B/op; indexed and live-token controls remain gaps.
[Direct-output evidence](../benchmarks/WARM-VP8L-DIRECT-OUTPUT.md).

JPEG dimension probing now combines marker reads: **6.73%** less filtered elapsed,
unchanged **744 B/op / 512 admitted bytes**. Both named probes remain CPU gaps.
[Marker-pair evidence](../benchmarks/WARM-JPEG-PROBE-MARKER-PAIRS.md).

The current producer passes **5,240 checks per OS**, plus 104 public header/probe
focused checks in normal/scalar modes. The same actual Release package, governed
async/one-frame lossless/alpha consumers and three examples pass both OSes.
Packets, pixels, source ownership, admission and cancellation remain qualified.
Every affected named WebP decode/validation control has current costs; the three
opaque VP8 controls retain unchanged-path evidence. The 268-case objective stays open.

**Codecs**

{codecs}

**Editing**

{editing}

**Inspection and pixel-coding validation**

{read_table}

† identifies qualified development-path costs. PNG encodes use the cached
four-invocation selector; predictable PNG16 native samples also batch four.
Every value is per complete operation. JPEG baseline decode, caller-CMYK, all
displayed PNG CRC callers, VP8/VP8L stream decode, the indicated JPEG/WebP anchors, bicubic word anchors
and Lanczos3 have twelve measured iterations. The existing
[RGBA16 bilinear reduction](../benchmarks/WARM-BILINEAR-WORD-GEOMETRY.md) now also
has an independent twelve: 1.125× / upper1.133, with exact managed pixels.
[RGBA8 bicubic reduction](../benchmarks/CURRENT-BICUBIC-BYTE-REDUCTION-COST.md) also
has an independent twelve: 1.384× / upper1.400. Remaining short screens retain three. Short screens do not establish independent parity.

FFmpeg is the reference, with approved backend boundaries: lossless WebP encode
uses **libwebp exact=1** to preserve hidden RGB; grayscale JPEG uses explicitly
labeled SIMD **libjpeg-turbo**. Bicubic/Lanczos3 use **zimg**, FFmpeg's zscale
backend, including required packing, halos and alpha normalization. Existing
case-scoped native-only pixel bounds remain; managed pixels stay exact.
Equal numeric quality settings establish no quality equivalence. Encoded sizes
and fixed packet claims retain their declared input/settings scope.

Probe/header inspection, complete Identify and pixel-coding validation have
distinct boundaries. Header inspection skips PNG CRCs; Identify validates the
complete container and nine source facts without decoding pixels. Validation
checks coding syntax without retaining an RGBA result. Stream rows include
actual acquisition, copies, EOF and release as declared.

[CRC evidence](../benchmarks/warm-png-gfni-crc-evidence.json),
[JPEG traversal](../benchmarks/WARM-JPEG-BASELINE-DC-AC-FUSION.md),
[codec parity](codec-parity.md) and
[coverage/history](../benchmarks/WARM-COVERAGE.md) retain scope, producer identities,
remaining gaps and raw evidence. Update with `python benchmarks/render-benchmark.py`;
verify with `--check`.
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
