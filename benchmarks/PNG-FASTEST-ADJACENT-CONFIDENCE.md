# PNG Fastest adjacent confidence

The existing513×259 RGBA8 Fastest1 control remains **uncertain** after its first
matched adjacent capture on the current actual package DLL. This is measurement
reconciliation, with no production change or assumed optimization need.

| Complete one-shot encode | Lokad / FFmpeg µs | Ratio interval | Direct managed B/op | Packet B, both |
| --- | ---: | ---: | ---: | ---: |
| Fastest1,4operations per subject | 4,397.62 / 4,374.65 | 0.944–1.044 | 820,843.67 | 236,828 |

Twelve alternating managed/native pairs and twelve native batches before/after
are retained. Confidence includes every actual, paired log ratios and all native
brackets. The dedicated quiet VM, CPU0/one thread, optimized JIT, nonconcurrent
workstation GC and idle-v2 guards are unchanged. This natural-GC view is separate
from the [earlier global-bracket result](PNG-FASTEST-CURRENT-CONFIDENCE.md),
4,757.43/4,717.98µs,interval0.992–1.028. Do not infer a causal CPU gain from them.

Both sides time complete fresh encoding: managed public EncodePng and fresh
MemoryStream creation/growth/disposal; native fresh codec/options/zlib/packet
preparation, independent payload and disposal. No final managed snapshot copy.
Borrowed resident source delivery, input/settings/full packet, all pixels/alpha/
hidden RGB, cancellation and independent output checks stay frozen. Native
`codecPreparationTimed=true` and `codecRetainedBetweenOperations=false` establish
the fresh-context boundary. The reused driver's generic prose about documented
retained wrappers does not describe a retained encoder context in this case.

Allocation is a direct total, distinct from the old820,916B/op BDN counter.
It establishes no storage saving. Logical deflater capacity remains384KiB;
operation scratch retention is zero. Source/output capacity is unchanged;
physical/native/JIT/pool peaks are unmeasured.

No production, embedded/public test, driver, harness, native, fixture or package
build. The immutable package and [6,449full/live checks perOS, affected scalar/ISA
and SAMEactualpackage/all3examples](PNG-STORED-WORKSPACE.md) are reused. No broad
qualification repeat, new image or settings grid. Thirteen of fifteen fresh PNG
encode controls still pass; Fastest8 and Gray8 remain uncertain. Stop unchanged
timing repeats; independent demonstrated work continues. Publication is postponed.

[Evidence](png-fastest-adjacent-confidence-evidence.json) preserves producers,
identities, exact packet, samples, boundary and qualification. Raw
`png-fastest-adjacent-confidence.tar.xz`, SHA256
`6dccb7b7d80dbd9eb1f3c63a429486e866e9849f9b31d4db18812bac11ade89c`
(500,956B/24members), is mirrored/member-verified; dependencies retain original
reference workers, harness/runtime, driver and current package. All289paths remain.
