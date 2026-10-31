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
    vp8l_bins = json.loads((ROOT / 'benchmarks/warm-vp8l-color-bin-reuse-evidence.json').read_text(encoding='utf-8'))
    assert vp8l_bins['acceptance']['retained'] and vp8l_bins['independentQuietConfirmation']
    assert not vp8l_bins['acceptance']['normalizedConfidenceSeparated']
    assert vp8l_bins['qualification']['fullChecksEachOs'] == 5240
    assert vp8l_bins['qualification']['sameActualPackageBothOs']
    assert vp8l_bins['qualification']['package']['sha256'] == vp8l_bins['qualification']['linuxPackage']['sameActualPackageSha256']
    assert set(vp8l_bins['cases']) == {'Encode8', 'Encode16', 'EncodeOpaque8', 'PhotographLossless8'}
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
    horizontal_word_current = json.loads((ROOT / 'benchmarks/current-tight-horizontal-word-bicubic-evidence.json').read_text(encoding='utf-8'))
    assert not horizontal_word_current['productionChanged'] and horizontal_word_current['allBuilds'] == 0
    assert horizontal_word_current['comparisonLowerBound'] > 1
    horizontal_word_observation = horizontal_word_current['cases']['TightHorizontal16'][-1]
    assert len(horizontal_word_observation['managed']['actual']) == 12
    assert all(q['passed'] for q in horizontal_word_observation['quietness'].values())
    assert horizontal_word_current['qualificationReused']['sameActualPackageBothOs']
    assert horizontal_word_current['recovery']['measurementReruns'] == 0
    lanczos_cost = json.loads((ROOT / 'benchmarks/current-lanczos3-reduce8-cost-evidence.json').read_text(encoding='utf-8'))
    assert not lanczos_cost['productionChanged'] and lanczos_cost['productionHarnessNativeBuilds'] == 0
    assert lanczos_cost['selectorBuilds'] == 1 and lanczos_cost['independentQuietConfirmation']
    lanczos_word_cost = json.loads((ROOT / 'benchmarks/lanczos3-centered-word-products-rejected-evidence.json').read_text(encoding='utf-8'))
    assert not lanczos_word_cost['productionChanged'] and lanczos_word_cost['candidateRejected']
    assert lanczos_word_cost['originalRestored'] and lanczos_word_cost['independentQuietConfirmation']
    assert lanczos_word_cost['qualificationReuse']['sameActualPackageBothOs']
    assert lanczos_word_cost['comparisonLowerBounds']['Reduce16'] > 1
    assert len(lanczos_word_cost['cases']['Reduce16'][-1]['managed']['actual']) == 12
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
    async_vp8l = json.loads((ROOT / 'benchmarks/current-async-vp8l-encode-cost-evidence.json').read_text(encoding='utf-8'))
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
        elif case['id'] == 'case14':
            producer = async_vp8l
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
        if case['id'] in {'case12', 'case13', 'case15'}:
            producer = vp8l_bins
            observation = vp8l_bins['cases'][{'case12': 'Encode8', 'case13': 'Encode16', 'case15': 'PhotographLossless8'}[case['id']]][-1]
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
        samples = 12 if case['id'] in {'case00', 'case02', 'case08', 'case10', 'case12', 'case13', 'case14', 'case15', 'case21', 'case22', 'case23', 'case24', 'case27', 'case28', 'case29', 'case30'} or updated and receipt['caseId'] in {'EncodeSmallest8', 'EncodeSmallest16', 'EncodeNoCompression16', 'EncodeFast8'} else 3
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
    observation = vp8l_bins['cases']['EncodeOpaque8'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == vp8l_bins['producers']['linuxSha256'] and receipt['completePacketExact']
    assert receipt['managedOutputBytes'] == receipt['nativeOutputBytes'] == 121896
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert all(q['passed'] for q in observation['quietness'].values())
    native = min(b['mean'] for b in observation['nativeBrackets'])
    opaque_row = f'| VP8L opaque RGBA8 / libwebp exact=1 † | {stats["Mean"] / 1000:,.2f} | {native / 1000:,.2f} | {observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | {observation["managedMemory"]["BytesAllocatedPerOperation"]:,} | 121,896 / 121,896 |'
    position = next(i for i, (kind, row) in enumerate(rows) if row.startswith('| VP8L RGBA8 /'))
    rows.insert(position + 1, ('codec', opaque_row))
    assert not async_vp8l['productionChanged'] and async_vp8l['allBuilds'] == 0
    assert async_vp8l['independentQuietConfirmation'] and async_vp8l['qualificationReuse']['sameActualPackageBothOs']
    assert async_vp8l['comparisonLowerBounds']['WebpLossless8AsyncNonSeekable'] > 1
    observation = async_vp8l['cases']['WebpLossless8AsyncNonSeekable'][-1]
    receipt, stats = observation['receipt'], observation['managed']['statistics']
    assert receipt['librarySha256'] == async_vp8l['producers']['linuxSha256'] and receipt['completePacketExact']
    assert receipt['managedOutputBytes'] == receipt['nativeOutputBytes'] == 183812
    assert receipt['managedOutputSha256'] == receipt['nativeOutputSha256']
    assert len(observation['managed']['actual']) == len(receipt['nativeBefore']) == len(receipt['nativeAfter']) == 12
    assert all(q['passed'] for q in observation['quietness'].values())
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
    wide_current = json.loads((ROOT / 'benchmarks/current-wide-bicubic-costs-evidence.json').read_text(encoding='utf-8'))
    assert not wide_current['productionChanged'] and wide_current['allBuilds'] == 0
    assert wide_current['independentQuietConfirmations'] == 7 and wide_current['reusedIndependentConfirmations'] == 1
    assert len(wide_current['cases']) == 8 and list(wide_current['status'].values()).count('confirmed-pass') == 4
    assert wide_current['evidenceReuse']['relevantSourcesByteExactSinceA128']
    for case, observations in wide_current['cases'].items():
        observation = observations[-1]
        assert len(observation['managed']['actual']) == 12 and all(q['passed'] for q in observation['quietness'].values())
        assert (observation['conservativeRatioUpper'] <= 1) == (wide_current['status'][case] == 'confirmed-pass')
        assert observation['receipt']['managedRationalReferenceExact'] and observation['receipt']['independentOwnedOutput']
        if case != 'Wide8':
            assert observation['receipt']['librarySha256'] == wide_current['producers']['currentLinuxSha256']
    observation = wide_current['cases']['Down32x16_8'][-1]
    matches = [i for i, (kind, row) in enumerate(rows) if kind == 'editing' and row.startswith('| Bicubic RGBA8, 32×16 / default')]
    assert len(matches) == 1
    rows[matches[0]] = ('editing', f'| Bicubic RGBA8, 32×16 / default workspace (confirmed pass) † → 32×16 | '
                       f'{observation["managed"]["statistics"]["Mean"] / 1000:,.2f} | '
                       f'{min(b["mean"] for b in observation["nativeBrackets"]) / 1000:,.2f} | '
                       f'{observation["worstBracketRatio"]:.3f} / {observation["conservativeRatioUpper"]:.3f} | '
                       f'{observation["managedMemory"]["BytesAllocatedPerOperation"]:,} |')
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
        producer = (refinement_window if case in refinement_window['cases'] else vp8_stream if case == 'ValidateVp8Alpha' else resident_decode if case == 'ValidateVp8lAlpha' else png_gfni if case in png_gfni['cases'] else baseline_blocks if case == 'ValidateJpegBaseline420' else png_average if case in png_average['cases'] else png_crc_scan if case in png_crc_scan['cases'] else vp8_validation if case in vp8_validation['cases'] else png_cursor if case in png_cursor['cases']
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
retained unchanged-path measurements. Seven affected JPEG decode costs are current;
eight named paths retain proven unchanged-path evidence.
These **76 tabulated comparisons** rank the remaining work; completion requires every
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

Lossless WebP color-bin reuse retains **4.02% less elapsed time** on the opaque
control, with unchanged **19,992,640 B/op**. Raw confidence intervals separate;
normalized intervals overlap. This specific gain is explicitly accepted with
that caveat. All four displayed color-search controls have current independent
costs; their complete packets remain exact and all remain CPU gaps.
[Color-bin evidence](../benchmarks/WARM-VP8L-COLOR-BIN-REUSE.md).

The existing async lossless WebP output boundary now has current independent
evidence: **1.293× / upper 1.316**, **29,697,312 B/op**, exact **183,812-byte** packets.
Staging, actual WriteAsync delivery, growth and output release are timed on both
subjects. [Async evidence](../benchmarks/CURRENT-ASYNC-VP8L-ENCODE-COST.md).

Async JPEG confirms **1.143× / upper 1.154**, **309,526 B/op** for caller RGB420,
and **1.199× / upper 1.210**, **820,475 B/op** for RGBA8/444 including flattening.
Reconstruction is exact; complete packets differ only by approved attribution.
[Caller RGB420](../benchmarks/CURRENT-ASYNC-JPEG-RGB420-COST.md) and
[RGBA8/444](../benchmarks/CURRENT-ASYNC-JPEG-RGBA8-COST.md) retain current evidence.

Byte-bilinear enlargement reuses exact horizontal rows: its paired round reduced
elapsed by **21.66%**, without added admitted capacity. The table includes later
shared alpha-rounding costs; enlargement and reduction remain gaps. [Row-reuse evidence](../benchmarks/WARM-BILINEAR-BYTE-ROW-REUSE.md).

Lanczos3 word reduction has a fresh current cost: **1.432× / upper 1.446**,
**138,437 B/op**. A centered-product candidate fails independent confirmation;
the qualified implementation is restored and new exact boundary tests retained.
[Current cost and rejected trial](../benchmarks/LANCZOS3-CENTERED-WORD-PRODUCTS-REJECTED.md).

Exact Int128 alpha accumulation improves tight RGBA8 bicubic enlargement
**7.26%**, removing **360,288 allocated B/op (14.45%)**: **1.473× / upper 1.481**,
**2,132,668 B/op**, exact pixels and the same **4 KiB** admission.
Five shared-caller costs are refreshed on the immutable build; all six remain gaps.
[Exact-alpha evidence](../benchmarks/WARM-EXACT-ALPHA-INT128.md).

Local original-order repair improves tight RGBA8 bicubic reduction **5.61%**
(all actuals **5.12%**), preserving exact pixels and the same **4 KiB** admission:
**1.474× / upper 1.540**, **132,309 B/op** (+3 B/op noise). Uncertain pairs
repair locally; tiny alpha still unwinds before exact scratch.
[Local-repair evidence](../benchmarks/WARM-BICUBIC-LOCAL-REPAIR.md).

The remaining shared-alpha caller audit reuses six proven unchanged paths and
confirms four current costs. Word vertical enlargement passes at **0.973× /
upper 0.985**, **2,514,272 B/op** and stops. Word bicubic enlargement remains
**1.230× / upper 1.244**, **5,016,602 B/op**. Existing word-bilinear/default-byte
enlargements remain gaps; no new workload or production change.
[Caller audit](../benchmarks/CURRENT-SHARED-ALPHA-CALLER-COSTS.md).
Exact completed-row reuse improves horizontal CMYK decoding **10.48%** independently
(all actuals **10.26%**, normalized **8.00%**) with separated confidence:
**1.366× / upper 1.396**, **1,085,755 B/op** (−2 B/op noise), unchanged owned capacity.
All seven affected displayed JPEG costs are reconciled on the immutable build;
progressive420 is now a confirmed gap. Eight named no-call proofs retain valid
costs, including grayscale, dense CMYK and both coding validations.
[Completed-row evidence](../benchmarks/WARM-JPEG-COMPLETED-ROW-REUSE.md).

Dense/general CMYK JPEG decoding independently passes at **0.894× / upper 0.916**,
**1,085,661 B/op**, with exact independent-matrix/native normal/scalar pixels.
The stale derived reference is reconciled without changing production or inputs;
this already-required case stops. [Current cost](../benchmarks/CURRENT-DENSE-CMYK-JPEG-COST.md).

Two retained PNG8 decodes under one borrowed caller root confirm **1.157× /
upper 1.168**, **1,637,520 B/op**, with **1,766,641 governed peak bytes**. Each
operation includes both complete decodes, retained overlap and releases on both
subjects; native has no shared admission API. Exact pixels and root cleanup pass.
[Retained-output cost](../benchmarks/CURRENT-RETAINED-PNG-DECODE-COST.md).

The already-required tight-workspace RGBA16 horizontal resize now has an independent
current cost: **1.267× / upper 1.326**, **530,838 B/op** (lower 1.201 confirms a gap).
Exact managed pixels and frozen native bounds pass; no build or optimization.
[Current horizontal cost](../benchmarks/CURRENT-TIGHT-HORIZONTAL-WORD-BICUBIC.md).

A bounded wider-support audit resolves eight existing default bicubic paths.
Both depths at **32×16 and 16×8 pass**, with conservative uppers **0.670–0.948×**,
**199,372–202,248 B/op**, and stop unchanged. Seven current confirmations reuse
the immutable build; Wide8 retains its proven unchanged-path confirmation.
Four larger-output controls remain gaps. No build, profile, new input or grid.
[Current wider-support costs](../benchmarks/CURRENT-WIDE-BICUBIC-COSTS.md).

Owned DC-only CMYK JPEG now has a current independent cost: **1.011× / upper
1.023**, **1,085,758 B/op**. Confidence remains open (lower 0.994); no repeat or
parity claim. The other three owned-CMYK costs retain valid evidence.
[Current CMYK scope and costs](../benchmarks/CURRENT-OWNED-CMYK-JPEG-COSTS.md).

Current evidence covers **5,305 checks per OS**: 2,164 freshly run internal
checks, with unchanged 2,193 public/948 live and actual-package proofs reused.
Two new JPEG cursor/fallback controls also pass scalar/ISA modes. Applicable
focused coverage includes 355 internal and 354 public checks normal/scalar and
Linux with AVX512 disabled. Header/probe proofs remain applicable.
[Retained JPEG boundary coverage](../benchmarks/JPEG-CROSS-BLOCK-LOOKAHEAD-REJECTED.md).
The same actual Release package, governed
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
and Lanczos3 have twelve measured iterations. Retained
[RGBA16 bilinear](../benchmarks/WARM-BILINEAR-WORD-GEOMETRY.md) and
[RGBA8 bicubic reduction](../benchmarks/CURRENT-BICUBIC-BYTE-REDUCTION-COST.md)
also have independent twelves and proven unchanged executed paths.
Remaining short screens retain three and do not establish independent parity.

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
