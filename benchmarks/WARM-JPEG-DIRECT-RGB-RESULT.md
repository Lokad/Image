# Direct retained RGB JPEG result

Caller-YCCK RGB, owned YCCK and caller-CMYK costs are superseded by
[exact caller YCCK rows](WARM-JPEG-CALLER-YCCK-ROWS.md). The other 26 measured
paths, including the direct-RGB gain, remain applicable.

The existing direct-RGB444 control independently saves **4.27% elapsed**
(4.33% all-actual, **4.56% native-normalized**), with separated raw and normalized
confidence. It remains a CPU gap at **1.383× / upper 1.397**. Managed allocation
falls from **966,367 to 10,088 B/op** (98.96%) on this default ungoverned control.

The frozen 133,057-byte, 513×259 input produces the same exact 531,468-byte RGBA8
result. Three padded component planes (424,320 bytes) and the later interleave
are removed for unambiguous equal-grid baseline RGB. IDCT writes final channels
into the private retained image; tails and separate scans preserve exact pixels.
The existing overwrite pool can reuse free ungoverned capacity only after disposal.
The output still owns and charges its actual capacity; governed results bypass
that pool. No output lifetime, admission, cancellation or encoded-byte contract changes.

| Existing complete decode | Lokad µs | Reference µs | Ratio / upper | Managed B/op | Status |
|---|---:|---:|---:|---:|---|
| DirectRgb444 | 2,008.82 | 1,452.11 | 1.383 / 1.397 | 10,088 | confirmed-gap |
| Decode8 | 2,145.98 | 1,902.45 | 1.128 / 1.158 | 948,069 | confirmed-gap |
| NativeDecode8 | 2,140.52 | 1,721.74 | 1.243 / 1.262 | 966,367 | confirmed-gap |
| NativeDecode420 | 1,106.36 | 1,010.22 | 1.095 / 1.130 | 757,362 | confirmed-gap |
| NativeDecode422 | 1,423.98 | 1,345.65 | 1.058 / 1.067 | 829,164 | confirmed-gap |
| OwnedDecodeGray | 829.88 | 680.10 | 1.220 / 1.226 | 672,196 | confirmed-gap |
| ProgressiveGray | 2,264.55 | 1,711.98 | 1.323 / 1.339 | 683,126 | confirmed-gap |
| Progressive420 | 2,149.68 | 1,965.54 | 1.094 / 1.127 | 775,318 | confirmed-gap |
| Progressive444 | 4,517.40 | 3,363.37 | 1.343 / 1.377 | 984,212 | confirmed-gap |
| Progressive422 | 2,741.92 | 2,249.28 | 1.219 / 1.254 | 847,116 | confirmed-gap |
| NativeDecode420Restart | 1,293.20 | 1,019.15 | 1.269 / 1.302 | 761,736 | confirmed-gap |
| StreamDecode420Restart | 1,295.22 | 999.64 | 1.296 / 1.340 | 761,832 | confirmed-gap |
| NativeDecodeCancelable8 | 2,269.81 | 1,723.90 | 1.317 / 1.324 | 966,367 | confirmed-gap |
| ProgressiveCancelable444 | 4,859.37 | 3,352.56 | 1.449 / 1.463 | 984,212 | confirmed-gap |
| DirectRgb422 | 1,436.80 | 1,092.46 | 1.315 / 1.336 | 829,164 | confirmed-gap |
| DirectRgb420 | 1,028.74 | 742.41 | 1.386 / 1.409 | 757,363 | confirmed-gap |
| CmykAdobe | 361.69 | 380.51 | 0.951 / 0.966 | 1,085,766 | confirmed-pass-stop |
| CmykAdobeAc | 527.30 | 392.30 | 1.344 / 1.351 | 1,085,755 | confirmed-gap |
| CmykAdobeGeneral | 2,782.23 | 3,124.32 | 0.891 / 0.898 | 1,085,661 | confirmed-pass-stop |
| CmykAdobeSparse2D | 609.12 | 436.83 | 1.394 / 1.401 | 1,085,757 | confirmed-gap |
| Ycck420 | 417.49 | 709.08 | 0.589 / 0.597 | 895,547 | confirmed-pass-stop |
| CmykCaller | 344.80 | 331.50 | 1.040 / 1.050 | 1,085,958 | confirmed-gap |
| Ycck420CallerRgb | 3,404.56 | 605.55 | 5.622 / 5.719 | 762,625 | confirmed-gap |
| Decode411 | 518.70 | 447.24 | 1.160 / 1.167 | 751,295 | confirmed-gap |
| Decode410 | 365.01 | 508.34 | 0.718 / 0.726 | 720,838 | confirmed-pass-stop |
| CallerGray | 756.22 | 647.64 | 1.168 / 1.181 | 273,427 | confirmed-gap |
| CallerRgb | 1,076.30 | 1,169.39 | 0.920 / 0.967 | 624,516 | confirmed-pass-stop |
| ValidateJpegBaseline420 | 587.00 | 680.85 | 0.862 / 0.892 | 9,984 | confirmed-pass-stop |
| ValidateJpegProgressive420 | 300.29 | 270.37 | 1.111 / 1.141 | 4,072 | confirmed-gap |

These are all 29 existing affected boundaries: 25 RGBA decode cases, two caller
sample outputs and two container validations. Only DirectRgb444 has a paired
speedup claim. Other rows refresh current costs after the common decoder changes;
they do not establish causal gains or regressions. Caller-YCCK420 RGB24 is now the
largest confirmed JPEG gap (5.622× / upper 5.719) and takes priority next; its old
SIMD costs predate the current byte-exact conversion path. Passing cases stop. Frozen inputs,
sampling, reference settings, complete ownership/delivery and existing exact or
explicitly bounded native pixels remain. Caller samples retain no RGBA intermediate.
All measurements use one pinned CPU, normal native SIMD, 12 actuals and idle-v2
guards. Native allocations are unmeasured. No new workload grid or quality search.

The original direct-RGB confirmation is reused because intervening encoder/editing
changes do not call its decoder path. Raw and all-actual intervals separate; the
candidate normalized upper 1.396930 is below the original lower 1.439625. The
native bracket is 0.31% slower, so elapsed and normalized gains are disclosed separately.

Fresh **5,367 checks per OS** (2,174 internal, 2,245 public, 948 live), focused
65 internal/399 public checks normal/scalar per OS, and the **same actual Release
package/three examples on Windows and Linux** pass. New checks cover channel order,
visible tails, separate scans/restarts, late declarations, caller buffers, retained
results, terminal refusal, cancellation, async ownership and recovery.
One core/internal/public build per OS; zero harness, native, selector or differential
builds. One short/confirmation for the primary and one confirmation per companion.

Older affected decode costs are superseded by this table. Applicable encoder,
other-codec, editing, text and inspection evidence is reused. Stale preparation
pixel hashes are reconciled using actual original/candidate normal/scalar DLLs
and the existing phase-correct normal/scalar native oracle; original metadata is
restored byte-exactly. CMYK derived expectations reuse their already-qualified
canonical/DC/Q14 snapshots. YCCK caller RGB projects the existing qualified RGBA
expectation and matches frozen normal/scalar native RGB exactly. No packet or
native comparison changes. Failed setup/guard preparations and corrections are
retained, with no completed score repeated and no reference gate relaxed.

[Evidence](jpeg-direct-rgb-result-evidence.json). Raw jpeg-direct-rgb-result-qualified.tar.xz,
956 members, SHA256 c1c78391e1b59f5fda90ab160a8f73183b4fee8df8738e3fa549a23b9dee27a6.
The mirrored, member-verified archive retains sources, original/selected binaries,
frozen inputs, cached selectors/native workers, every actual/guard/receipt, metadata
audits, both-platform qualification and actual packages. The full 268-path warm
parity objective remains open; this is not a release-wide benchmark refresh.
