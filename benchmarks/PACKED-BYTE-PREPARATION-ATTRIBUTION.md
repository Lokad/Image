# Packed byte preparation attribution

PBA749 profiles the unchanged, qualified LBR748 package on the quiet VM. The
existing complete 513×259→64×32 RGBA8 resize remains458.14/zimg370.31µs,
upper1.245, GC113B/op and4KiB workspace from
[contiguous views](PACKED-BICUBIC-CONTIGUOUS-VIEWS.md). This capture adds no
timing/allocation result, source change, build, fixture or qualification repeat.
Instrumented elapsed values are discarded; original exact pixels, native bounds,
source/ownership and idle-v2 gates pass.
Managed preparation/output/disposal stay timed; native graph/scratch remain
retained and labeled. This is not a fresh-native-graph comparison.

One two-second CPU0 capture/CPU1 sampler records1,997 locations. Correlating
actual addresses with JIT sizes distinguishes suffix executor5768B/753samples
from prefix6284B/622samples, and suffix pair-helper178B/279samples from
prefix181B/240samples. Kernel has90samples. Executor total1375(68.9%) and leaf
pair helpers519(26.0%) are separate; they do not identify exact phase times or
removable percentages. Generic display names alone would merge distinct modes.
The [earlier profile](PACKED-BYTE-EXECUTION-ATTRIBUTION.md) predates the changed
helper boundary and is historical.

Instruction annotation identifies a more direct first lead: the indexed
horizontal fallback repeatedly loads/stores its512-bit sum at `[rbp-0x130]`.
The prefix/suffix store locations have103/81samples. Two UInt16 offset streams
and two coefficient streams identify this loop; it is not vertical accumulation.
The frozen control has two clipped pairs,16,576shared taps and2,121,728logical
stack read/write bytes. Sample skid and logical traffic do not quantify savings.
Isolating the same ordered indexed dot loop may retain its sum in a register;
inspect emitted code before timing, preserving repeated/unequal offsets, bounds,
tails, certificates and repair. This is a targeted compiler/lifetime hypothesis,
not blanket helper splitting or merge evidence.

Byte source preparation is a second lead. Current code multiplies two
RGBA pixels in UInt32 lanes and packs suffix products to UInt16. Byte products
are≤65025, so four pixels could use UInt16 lanes directly, with exact widening
for prefix Double storage and unchanged packed suffix bytes. Word products keep
their existing full UInt32 path; coefficients, ordered sums, certificates and
final pixels are unaffected by the proposed integer preparation.

A source-only model finds137,788two-pixel batches versus68,894four-pixel batches
and518unchanged single tails on the frozen control. This halves bulk iterations,
not all conversion work or elapsed time. All58decoded lengths preserve sample
coverage;65,792product/alpha checks fit unsigned16 and exact Double. These are
mathematical/model checks, not actual SIMD qualification or merge evidence.

Before building that second lead, establish shuffle lanes and source/store
bounds. If justified, share one byte bulk block
between prefix/suffix, retain existing word/tails and4KiB admission, then use
focused exact pixels/guards/root/cancellation and the existing short complete
control. Independent confirmation and affected word costs follow only a useful
stage; broad Windows/Linux/scalar/ISA/SAME-package proof belongs at acceptance.
No new buffer, cache, pool/session, owner or numerical policy is proposed.

[Evidence](packed-byte-current-preparation-evidence.json) pins current source and
actual package/DLL. Reuse6153full/live checks perOS,327/729normal/scalar/ISA and
SAMEpackage/allthree consumer examples. Mirrored/member-verified raw
`tmp/packed-byte-current-preparation-profile.tar.xz`, SHA-256
`51118e613ebd267f9f50bbbc5a73093a23f7732959b2bf649a410b1b4b3e0969`,
95,512bytes/18members, retains scripts, trace, samples, distinct instances,
instruction annotations/loop contexts, guards, receipt and model. No renderer
dependency is added.
