# Prepared live PNG run invariants

Historical stage: [current block Adler sums](PNG-BLOCK-ADLER.md) supersedes
the affected CPU costs. Its format, ownership and non-checksum implementation
changes remain qualified; raw evidence and original boundaries are retained.

Complete live RGBA16 decoding saves **93.08 µs / 2.89% raw / 1.86%
reference-normalized** against a contemporary unchanged predecessor. Filtered,
all-actual and normalized confidence intervals separate. Native time moves −1.05%;
raw and normalized gains remain distinct. The current upper 1.001253 remains
**uncertain**, not a parity pass. The two existing affected controls refresh:

| Complete public Decode | Lokad µs | FFmpeg µs | Ratio / upper | GC B/op | Status |
| --- | ---: | ---: | ---: | ---: | --- |
| DecodeCancelable16 | 3,126.33 | 3,153.78 | 0.991 / 1.001 | 8,568 | uncertain |
| DecodeCancelable8 | 1,529.69 | 1,535.97 | 0.996 / 1.012 | 4,472 | uncertain |

Both use original 513×259 encoded inputs, complete parsing/preparation, full-depth
RGBA normalization, independent output and disposal. Native codec/input/frame
wrappers remain retained, as originally declared. Pixels, alpha/hidden RGB,
normal/scalar native CLI samples, ownership and quiet idle-v2 gates stay exact.
No new fixtures, settings, worker/selector builds or operation-boundary changes.

The [source-valid profile](PNG-LIVE-WORD-CURRENT-ATTRIBUTION.md) identifies the
single-symbol literal loop; no capture is repeated. Prepare the existing live
token, 512-entry borrowed lookup and input view once per run. Actual timed-module
code removes two repeated source-null checks and the masked lookup's bounds
guard, while retaining **both cancellation-state reads per symbol** in original
order. Its body is 296 versus 305 B; code size is not elapsed-time proof. The
source remains one single-symbol loop, without an unrolled group or new helper.
Earlier [two-symbol experiments](PNG-CACHED-LITERAL-RUN-EXPERIMENT.md) remain
unselected history rather than an automatic ban or an accepted implementation.

The sole dispatcher selects a live token; its production lookup always has 1024
entries. Empty destinations return 0 without touching state. Existing independent
RFC1951 prefix/phase/cut/output-boundary, pre-cancel, borrowed-storage, completed
cursor and zero-allocation/root tests pass. Removing only the guard and borrowed
views reconstructs the full old source. Ordinary, rolling-history, counting,
encoding, framing errors, short tails, fallback, repair and owners remain unchanged.

Byte 1529.69/native1535.97 µs stays uncertain. Compared with its retained prior,
raw time rises 9.90% while native rises 10.19%; normalized intervals overlap.
This is no byte speedup, uniform raw-gain or proven nonregression claim. One
filtered byte outlier remains in the all-actual view. No successful job is repeated
for favorable bounds. The family now has **49 pass / 0 gaps / 2 uncertain** across
the same 51 read/validation controls; passing paths stop.

**Storage:** GC 4,472/8,568 B per byte/word operation is unchanged. Framing admission
stays 4 KiB for the existing 4,063 B array payload. No owned array, explicit stack
array capacity, owner, reservation, pool/session or retained scratch is added.
Independent result 531,468/1,062,936 B remains charged through disposal. Materialized
inflation workspace, default idle-pool retention and governed bypass are unchanged;
physical/native/JIT peaks are unmeasured. Admission refusal remains terminal.

After both complete costs reconcile: **6,351 full/live checks per OS**,
712 internal/246 public PNG/admission checks in normal/scalar modes, then SAME actual
Release-package/all three usage examples on Windows/Linux. Consumers exercise
byte/word sync/async samples, input/stream ownership, aggregate admission,
refusal/recovery and independent survivors. Unchanged resize/codec/text ISA proof
is reused, including 442/789 resize checks and the three named storage suites.
One main/embedded version per OS; no public/harness/native/fixture builds or new
tests, and one broad/package milestone. Published releases remain unchanged.

Private 0.2.1 package `2787274463cf81f9737b65869b0aa01990efac23b561b17c93ed70b0ada2712f` contains timed DLL `8e44e29f453cee05d471045e2b382574a8ca4fd452f5b5c77499a276a014e150`.
[Evidence](png-live-run-preparation-evidence.json) records current costs, source
scope, comparison/clock views and qualification. Raw `png-live-run-preparation.tar.xz`:
2,463,116 B/129 members, SHA256 `8e86848221b82b699e6d228925e6de0ca3a535ec68b43ba2556b3032e12eb10d`;
mirrored/member-verified, with original inputs/reference/profile/experiment/current
qualification dependencies retained. All 289 material paths remain required;
publication is postponed.
