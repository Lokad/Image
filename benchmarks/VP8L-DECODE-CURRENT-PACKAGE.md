# VP8L decode on the current package

All five original owned-output controls pass on the immutable 0.2.1 package.
The coding-only validator retains its earlier evidence because its descriptor
boundary skips the changed output pool.

| Complete operation | Lokad.Image µs | FFmpeg µs | Conservative interval | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Frozen native packet, live token | 3,053.98 | 3,258.47 | 0.918219–0.956837 | 152,728 |
| Frozen native packet, fresh seekable stream | 2,942.95 | 3,283.26 | 0.888630–0.902094 | 152,824 |
| Frozen native packet, resident | 2,951.16 | 3,269.16 | 0.895948–0.907616 | 152,728 |
| Indexed palette16 packet, resident | 408.78 | 544.44 | 0.735546–0.761869 | 288,464 |
| Original managed packet, resident | 1,438.35 | 2,160.41 | 0.652579–0.680333 | 10,852 |

Each case preserves complete public decode, independent output and disposal.
The fresh stream includes input acquisition. FFmpeg retains its declared
reference state and materializes an owned output per call. All frozen packet,
exact-pixel, alpha, hidden-RGB, normal/scalar, cancellation and ownership checks
pass with the original quiet-VM guards.

The live-token case keeps its twelve paired four-call batches and all three
native groups. The other four keep the original generated delegates, twelve
actuals, 200 ms target and natural GC. GC allocation measures managed bytes
allocated per operation. Retained capacity and physical peaks are separate.

Four runtime groups reuse the original harnesses, reference binaries and package.
One orchestration build supplies the natural-GC driver for generated jobs.
The collector recovered a receipt field-name mismatch from the verified raw
archive without repeating timing. The mirrored `vp8l-owned-decode-current-package-confirmations.tar.xz` retains
39 verified members and 49,784 B, SHA-256
`7f52679bf9c224e43d8d232b67009f7cfbbce3e5ae0f170c54446515696895f6`. [Evidence](vp8l-decode-current-package-evidence.json)
