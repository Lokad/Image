# WebP alpha output handoff

Compressed ALPH now decodes into the independently owned final image when its
transform layout and byte order allow it. Green samples move to alpha lanes,
inverse filtering uses those lanes, and YUV rendering writes RGB into the same
buffer. This removes a full temporary word raster and a separate alpha plane.
Palette/raw/byte-order paths retain compact storage, selected from the format
before admission. Refusal is terminal. Validation-only parsing is unchanged.

The codec owns the early result through alpha failure, then transfers it to the
renderer. Existing result admission precedes allocation; parser/transform storage
is released separately. No new owner, quota, pool or session is introduced.
Color arithmetic, chroma expansion, full lossless syntax/transform validation,
encoder packets and all managed pixels remain exact.

Complete 513×259 alpha span decoding improves 6,608.54→6,393.62µs:
**214.93µs/3.25% raw**,3.48% native-normalized. Stream decoding independently
improves 6,600.44→6,372.36µs:228.08µs/3.46% raw,3.72%normalized.
Both filtered/all-actual confidence views separate for both improvements. Valid
unchanged-path original twelves are reused; native points move+0.23%/+0.27%.
The initial three-actual screen and every outlier remain retained. No favorable
repeat or causal opaque gain is claimed.

| Existing complete operation | Managed µs | Native µs | Ratio / conservative upper | Managed B/op |
| --- | ---: | ---: | ---: | ---: |
| Native8 | 6,393.62 | 5,970.64 | 1.071 / 1.084 | 238,384 |
| StreamNative8 | 6,372.36 | 6,002.93 | 1.062 / 1.066 | 238,480 |
| Opaque8 | 5,280.10 | 4,784.19 | 1.104 / 1.107 | 760,564 |
| OpaqueCancelable8 | 5,468.99 | 4,783.96 | 1.143 / 1.150 | 760,564 |
| SimpleOpaque8 | 5,401.62 | 4,892.22 | 1.104 / 1.109 | 760,564 |
| ValidateVp8Alpha (reused) | 6,174.29 | 5,443.41 | 1.134 / 1.139 | 554,478 |

All six remain parity gaps. Five affected renderer callers refresh; validation
reuses unchanged production. Only the displayed stream row and family summary
change; 124 other displayed numerical rows reuse valid paths. This is not a full
actual-package release refresh or completion of the 283-path objective.

Warm allocation falls 1,434,731→238,384 B/span operation, and 1,434,827→238,480 B/stream.
The underlying temporary payload removal is664,335 B. Final storage additionally
uses the existing exact-size pixel pool on ungoverned calls: this sequential case
leaves one 531,468 B array idle after disposal. The existing ceiling is two arrays/
4 MiB. Governed calls bypass the pool. Coarse shared admitted peak on the existing
original alpha admission control falls 884,699→751,768 B; retained output stays
531,468 B until disposal. This control's packet differs from the timing packet;
its frozen identity is retained. GC allocation, admitted capacity, retained state
and physical process peak are distinct; physical/native allocation is unmeasured.

Actual standalone reference boundaries retain frozen 98,590 B input, exact 531,468 B
RGBA output, full parsing/reconstruction/filter/color work, independent output
and disposal. Fresh seekable Stream delivery remains timed. Native codec/conversion
contexts retain their documented setup; fresh-context setup parity is not claimed.

Windows/Linux each pass 5,747 full/live checks (2,491 internal/2,308 public/948 differential).
Focused normal/scalar coverage reuses 920 internal/427 public and adds two public
cases; 36 internal/58 public also pass with AVX2 and AVX512 separately disabled.
Tests cover 164 frozen frames, alpha filters/packing, scalar/SIMD edges/tails,
malformed alpha after output admission, terminal refusal, cancellation, dirty
storage, recovery and surviving independent owners. Old exact admission controls
now use the reduced peak and retain one-byte-below refusal assertions.

One immutable main/embedded build per OS is reused. Public tests rebuild for their
changes; no benchmark/native/selector/fixture build occurs. The SAME actual private
0.2.1 Release package and all three usage examples pass both OSes, including existing
compressed-alpha admission and sync/async ownership controls. Published releases
are unchanged; publication remains separate and postponed.

Package`1d0fd80a673753860085df045acccb10c24a78174889cabc64d12635dbafb93a` (418,862 B).
Windows main`1b17a0a3d2e9134658e635a393a7224b61aadf2d47e76497ecd4f66061f71ae2`;
Linux main`83ae78bf2f5ff27bb07ba5887dad6116e5f36f93c4237f8d0d62725696e60db4`.

[Evidence](vp8-alpha-output-handoff-evidence.json) retains producer/source identities,
points/brackets/guards, exact boundaries, ownership, package proof and source-valid
reuse. Raw`vp8-alpha-output-handoff-qualified.tar.xz`: 2,953,624 B/222 members,
SHA256`84cced428c1e306b20760e8b02e8763debfb67f871ef60f0fb18530929e7f1ea`,
mirrored and member-verified with pinned hash-addressed dependencies. Earlier
[storage/model/reader work](VP8-UNSHIFTED-LIVE-RESERVOIR.md) and
[encoder preparation](VP8-BOUNDED-PRICE-VIEW.md) remain linked history.
