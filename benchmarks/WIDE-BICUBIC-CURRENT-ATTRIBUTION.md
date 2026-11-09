# Current wide bicubic execution attribution

WHA681 records one two-second capture of the complete frozen RGBA8
513×259→128×64 compact bicubic resize on qualified Windows DLL13016616.
Preparation, working storage, independent output and disposal stay included;
the zimg reference retains its declared graph/scratch boundary. All existing
exact managed/native/source/ownership gates and idle-v2 guards pass. CPU0 runs
the operation; CPU1 samples. Instrumented elapsed is discarded.

Of1,996 sampled locations,1,760 are in the wide range driver. Its shared horizontal
tap loop has572 locations, paired pixel decoding434 and paired vertical filtering
248. The geometry reader has15; direct geometry construction100 and Kernel63.
Instruction ranges/maps are retained. Sampling skid and these location counts
are not phase times or projected speedups.

The current four-column driver already shares decode, geometry and storage,
but processes two horizontal pairs sequentially. A bounded next design exposes
four independent accumulators in one tap traversal, preserving each pixel's
exact Multiply/Add order and individual tail. Existing three-column/partial
fallback, unsigned products, certificates, repair, cancellation and4KiB ownership
remain required. Extra register pressure/code size could outweigh the benefit;
measure complete operations before acceptance. Earlier pair-view trials do not
establish this design, and the [guarded geometry-view trial](BICUBIC-GUARDED-GEOMETRY-VIEWS.md)
remains inconclusive. No family is permanently closed.

No production, worker, fixture or native build occurs. Unchanged full Windows/
Linux/scalar and actual-package qualification is reused. Current displayed costs
and the full283-path objective are unchanged; publication remains postponed.

[Evidence](wide-bicubic-current-attribution-evidence.json) pins sources, validated
boundaries, instruction ranges and the next hypothesis. Mirrored/member-verified
raw `wide-bicubic-current-attribution.tar.xz` has90,316 B /18members, SHA256
`69fec0a6000d3c52146860c2fa1a36f966ac5058a7f4359f5b1dbd2aa9430f6d`.
Runtime inputs are provided by verified hash from the retained geometry-trial
archive, without duplicate payloads. No renderer dependency is added.

The subsequent [ordered quartet experiment](BICUBIC-ORDERED-QUARTET-EXPERIMENT.md)
tests two implementations and restores production without a useful complete lead.
Its adverse/inconclusive results remain separate from these location counts.
