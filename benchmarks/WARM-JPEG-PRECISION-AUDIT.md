# JPEG compression audit

The current caller-RGB420 control still produces **74,040 bytes versus 52,093**
for FFmpeg. This is a compression gap, not missing adaptive Huffman coding:
**21,835 of the 21,947 excess bytes are entropy data**. Header differences account
for only 112 bytes. The existing grayscale control remains competitive.

| Frozen 513×259 control | Managed / reference bytes | Managed / reference entropy bytes | Source PSNR difference |
| --- | ---: | ---: | ---: |
| Caller RGB8, 4:2:0 | 74,040 / 52,093 | 73,612 / 51,777 | 0.106110 dB |
| Caller gray8, one component | 58,823 / 58,910 | 58,593 / 58,680 | 0.000033 dB |

Both complete packets reproduce the qualified hashes with the current producer.
The unchanged source, packets and reference settings preserve the earlier quality
evidence; no quality search or timing run was repeated. RGB uses managed quality69
and FFmpeg qscale5; grayscale uses the approved SIMD libjpeg-turbo reference at69.
These numbers are encoder-specific, not an equivalence rule.

An independent baseline entropy reader attributes 119,317 excess coded bits to
luma and 53,884 to chroma. Managed RGB luma has 95,106 nonzero AC coefficients
versus 76,924; chroma has 21,062 versus 8,991. Its low-frequency quantizers are
substantially finer than the reference's. The grayscale quantization tables match
exactly. The [existing quantization formula](../SPEC.md) is an explicit API policy;
an alternative must preserve that policy rather than silently replace it.

Two untimed coefficient experiments bound the next step. A minimum quantizer of10
at quality69 produces65,163 bytes, missing the interim63,066-byte size milestone;
no quality qualification follows that failure. Adding10 to each existing RGB
quantizer produces **52,566 bytes**, a29.0% reduction and0.91% above the reference.
Its source PSNR is15.144788 versus15.256982dB, a **0.112194dB** difference inside
the existing0.25dB gate. Managed/native decode agreement is maximum3/RMSE0.5920;
alpha remains exact. The independent reader/writer first reproduces the original
entropy scan byte for byte, and the reused oracle reproduces its frozen decode hash.

These packets requantize the old rounded coefficient centers. They establish
feasibility only: they do not encode the original samples directly, measure time
or allocations, or qualify production resources and ownership. The next bounded
round implements an explicit optional color quantization policy, retains original
defaults and grayscale, then checks direct source encoding before short timing.
Independent confirmation and affected cross-platform/package qualification remain
required for acceptance. No photograph, diagram or general JPEG efficiency claim
follows from this procedural RGB420 control.

[Numeric evidence](jpeg-precision-audit-evidence.json) records the frozen inputs,
packet identities, coefficient counts and feasibility results. Raw reader/writer,
quality scripts and outputs are retained as `c36-jpeg-precision-audit.tar.gz`,
SHA256 `d1e0ecbcd87a2b24fc07e2992d345fddc505d98104c0e0df2e90dc0cf74a89bc`.
No production code, benchmark selection, timing evidence or published package
changes in this audit.
