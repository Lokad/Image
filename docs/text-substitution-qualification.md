# Staged OpenType substitution

This records the internal GSUB checkpoint before public integration. The current
[OpenType layout mode](text-shaping-qualification.md) uses this primitive;
replacement qualification remains open.

The implementation independently follows the public [GSUB](https://learn.microsoft.com/en-us/typography/opentype/spec/gsub),
[layout common formats](https://learn.microsoft.com/en-us/typography/opentype/spec/chapter2)
and [GDEF](https://learn.microsoft.com/en-us/typography/opentype/spec/gdef) specifications.
It supports single substitutions, ligatures, chained contexts in formats 1/2/3,
extensions, coverage/class tables and GDEF filtering. Script/language selection,
required features and lookup order are explicit. Context actions address the
sequence modified by preceding actions; ligatures retain source coverage and
mark-component associations. Selected unsupported lookup families fail with
`UnsupportedImageFeature.FontTable`; nesting is bounded to sixteen lookups.

One temporary glyph buffer and one reused lookup selector use the caller's
operation context. Capacity growth admits old/new overlap, tables borrow font
storage, refusal is terminal, and cleanup does not depend on GC. No host governor,
upstream implementation, format table or font asset was incorporated.

Qualification passed **4,284 tests per OS** on Windows/Linux: 1,475 internal,
1,881 public API and 928 live FFmpeg comparisons. The **36 new parser checks per
OS** also passed with intrinsics disabled. They cover lookup semantics, malformed
prefixes/mutations, nesting, boundaries, mark filters, cancellation, work limits,
replacement peaks, refusal cleanup and subsequent admission. The same actual
private 0.2.0 Release package passed isolated consumers and all three usage
examples on both systems.

The Windows real-font probe matches the reference combining-mark **glyph
sequence** after composition; positioning/rendering are not qualified by this
probe. The original twelve public text controls remain six passing and six open.
No timing was measured; no performance conclusion follows from these checks.

Retained SHA-256 identities:

- Qualified source: `2ffeb0430b01e4c455f95a2933d30ffcccec614a10e485871c85a3c94b97f63a`.
- Windows producer: `748266b477a30988fdb4a2c6b858592fb5ceb666752856a987501b1f4e805f74`.
- Linux producer: `fb0adfe58878e866c7d05178e4c101799bcb2c81eb7c43830a5af8049f03396c`.
- Actual package: `da48b7cf85356b6da20dc37fb040a25ab395055ab4f54e98cb066382f350382b`.
- `p2b-qualified-windows.tar.gz`: `62f6f69f5441c26050c858d7ed95d8bfd14801e535692d460b09a51035a14a43`.
- `p2b-qualified-linux.tar.gz`: `fd272d6dee7beccdd0e182efce2e2c6ca1d00840dfad2ec638175ebdeefefad5`.

Published 0.1.0 is preserved. [Public shaping qualification](text-shaping-qualification.md)
records subsequent integration; [fitting](text-fitting-qualification.md) is also qualified.
