# JPEG complete-packet coverage

All 19 existing JPEG encode rows are reconciled on the immutable C124 producer:
**18 default boundaries plus one optional CompactColor boundary**. Five default
cases pass complete-packet size and quality; thirteen remain open. CompactColor
passes separately. This audit adds no image, quality, setting or benchmark case.

All rows retain their frozen source/reference inputs, sampling, encoder settings
and unchanged quality/alpha/decoder gates. Source-PSNR difference remains <=0.25 dB.
Existing C124 evidence supplies the six sampling/depth controls, caller samples
and fixed4208. Eight missing boundary facts use existing compiled helpers; the
RGBA-to-gray boundary uses its existing two-fresh-operation validator. Only two
new packet hashes need the existing native-decode quality check. No production
or worker build, native encode, timing, independent confirmation or package proof
is repeated. Current C124 tests and the exact-package qualification remain valid.

| Existing case / boundary | Managed / reference packet B | Size gate |
| --- | ---: | --- |
| Encode8 | 158,812 / 133,041 | Open |
| Encode422_8 | 66,050 / 71,168 | Pass |
| Encode420_8 | 55,797 / 52,093 | Open |
| Encode16 | 158,827 / 133,043 | Open |
| Encode422_16 | 66,071 / 71,210 | Pass |
| Encode420_16 | 55,834 / 52,117 | Open |
| Encode420Restart8 | 55,897 / 58,012 | Pass |
| Encode420Stream16 | 61,039 / 52,117 | Open |
| Encode420Cancelable16 | 61,039 / 52,117 | Open |
| Encode444Stream8 | 170,978 / 133,041 | Open |
| Encode444Cancelable8 | 170,978 / 133,041 | Open |
| JpegBudgetEncode4208 | 55,797 / 52,093 | Open |
| Jpeg8AsyncNonSeekable | 158,812 / 133,041 | Open |
| JpegRgb420AsyncSamples | 55,797 / 52,093 | Open |
| EncodeGray8 | 58,823 / 58,910 | Pass |
| CallerGray | 58,823 / 58,910 | Pass |
| CallerRgb420 | 55,797 / 52,093 | Open |
| CompactColorRgb420 | 48,701 / 52,093 | Optional pass |
| Encode420Stream8 | 60,987 / 52,093 | Open |

The largest demonstrated gap is 4:4:4 forward-only/live-cancellable encoding:
170,978/ref133,041 B, 28.5% excess. Its existing adaptive counterpart is 158,812 B
with exactly the same pixels and quality. Reusing admitted adaptive Huffman
planning for callback/cancellable outputs can therefore save 12,166 B without
another coefficient decision. The remaining default 4:4:4 gap would stay open.
This is the next bounded primitive; checked emission, terminal shared refusal,
cancellation, quota/failure cleanup and nested ownership must qualify before it
can be accepted. Passing 4:2:2, grayscale, restart 4:2:0, optional CompactColor and all
confirmed WebP controls stay stopped.

The RGBA8 async case is explicitly 4:4:4/q69 and bypasses the new subsampled AC
pass; its initial invalidation label is corrected. Exactly **10** subsampled CPU
rows are superseded, with their historical costs retained. The finite 256-case
performance scope remains unchanged. CPU parity and speed-only work stay deferred.

[Numeric evidence](jpeg-packet-ledger-evidence.json) identifies every packet,
reference, reused quality sample and ownership check. Raw scripts/results are
mirrored as `c125-jpeg-packet-ledger.tar.gz`, SHA256
`27bde5ce6fc22f0ec02a351c3504c5821c7ec2c792ed60702dd8d1c550c0c35c`.
The source/quality contract and retained allocation/cost evidence are in
[the accepted JPEG round](WARM-JPEG-AC-DECISION.md).
