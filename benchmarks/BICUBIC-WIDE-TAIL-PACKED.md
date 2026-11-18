# Packed weights in the bounded RGBA8 bicubic tail

The four-column tail reuses the compact coefficients and binary64 filter
already used by the wide word path. The existing overlap check proves that
both columns use contiguous sixteen-tap windows with a four-pixel
displacement. The original multiply/add order and normalization stay intact.
For exact repair, each lane's weights are restored in decoded scratch after
horizontal readers have finished. The row loan, cache lifetime, 4 KiB
working allowance, and independent output stay intact.

The original 513×259 to 128×64 control reaches this tail after 42 output rows.
The geometry model finds 60 eligible column pairs and 95 source-row fills per
strip. It replaces 182,400 per-weight broadcast requests with 22,800 packed
loads and lane selections. This is a work count for the frozen operation.

| Complete TightWide8 resize | Baseline or zimg µs | Candidate µs | Conservative ratio interval | GC B/op |
| --- | ---: | ---: | ---: | ---: |
| Three alternating managed pairs | 365.52 | 359.45 | 0.860461–1.123903 | 112 |
| Twelve independent managed pairs | 372.97 | 365.53 | 0.972956–0.987247 | 112 |
| Qualified DLL versus prepared zimg | 366.62 | 356.63 | 0.949152–0.986644 | 112 |

The independent pairs confirm **7.43 µs / 1.99%** lower elapsed time.
The same immutable DLL passes the complete standalone comparison with
twelve 64-operation pairs and the original native brackets. Exact managed
pixels, native bounds, source, surviving owners, and both quiet guards
pass. The default workspace control reaches the earlier cache, and
the original inventory has one case that uses the changed executor.
All 61 direct bicubic controls now have passing evidence.

Windows and Linux each pass **6,856** full and live checks and **94**
focused checks per normal, AVX-512-disabled and scalar mode. The new test
forces exact half ties in the packed tail and checks admitted lifetime
and independent pixels. The actual dependency-free development package
contains unchanged timed DLL `b874f8d2`. Both consumers and all three
usage examples pass.

[Evidence](bicubic-wide-tail-packed-evidence.json) pins the archived
measurements, source, package, and qualification. The
[earlier preparation trials](BICUBIC-WIDE-PREPARATION-TRIALS.md) retain
their adverse and uncertain results.
