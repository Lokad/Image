# Packed Lanczos3 byte-premultiplication trial

One exact UInt16 premultiplication trial is rejected. The existing complete
RGBA8 reduction screens **721.258 µs,1.472× / upper1.622,138,455B/op**:
only1.98% below the retained735.824-µs confirmation, missing the5% triage gate.
The all-actual point comparison is2.09%; no allocation saving, independent gain
claim, longer confirmation or variants. Accepted source/tests and exact qualified
core/PDB/XML are restored. No current benchmark cost changes.

Byte products fit unsigned16 exactly before widening for unchanged signed
coefficient weighting. Accumulation order, Q30 coefficients, vertical certificate,
exact fallback, storage, cancellation and admission/ownership are unchanged.
The trial passes55 focused checks on Linux normal/scalar/AVX512-off and Windows
normal/scalar, including all65,536 byte/alpha products. Exact frozen managed/native
frames, independent owners and both quiet guards pass. Restored54-case focused
checks pass each OS; current full/package qualification is reused.

One preceding2-second current-code diagnostic collects1,902samples: horizontal
filtering46.3%, vertical accumulation19.5%, application15.1%. All sampled addresses
in those methods map to the same process's disassembly. These are diagnostic
fractions, not measured component costs. No diagnostic build or new workload.
One Linux core build is used for the rejected short; cached harness, native worker
and selector are reused. Original quality headers are restored after both jobs.

[Evidence](lanczos3-packed-premultiplication-rejected-evidence.json) retains every
actual, bracket, confidence view, proof and restoration. Mirrored/member-verified
raw archive:
lanczos3-packed-premultiplication-rejected.tar.xz,76members,SHA256
4f26f78c8af3ed23f9041642af38fdbc0cad77ea6da446562a7512b2ab24dea0.
