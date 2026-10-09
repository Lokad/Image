"""Render the original narrow-VM snapshot; never measure on the caller's machine."""
import json
from pathlib import Path
import sys

sys.stdout.reconfigure(encoding="utf-8")

snapshot = json.loads(Path(__file__).with_name("horizontal-flip-2026-10-09.json").read_text())
baseline, candidate = snapshot["runs"]

def row(run, kind, method, depth):
    return next(r for r in run["rows"] if r["type"] == kind and r["method"] == method
                and r["parameters"] == f"Depth={depth}")["statistics"]

print("| Precision | Before µs | After µs | Warm speedup |")
print("| --- | ---: | ---: | ---: |")
for depth in (8, 16):
    old = row(baseline, "HorizontalFlipPrimitive", "Managed", depth)["Mean"]
    new = row(candidate, "HorizontalFlipPrimitive", "Managed", depth)["Mean"]
    print(f"| RGBA{depth} | {old / 1000:.2f} | {new / 1000:.2f} | {old / new:.2f}x |")

print()
print("| Precision | Before managed ms | After managed ms | After FFmpeg ms | Current batch ratio |")
print("| --- | ---: | ---: | ---: | ---: |")
for depth in (8, 16):
    old = row(baseline, "HorizontalFlipBatch", "Managed", depth)["Mean"]
    new = row(candidate, "HorizontalFlipBatch", "Managed", depth)["Mean"]
    native = row(candidate, "HorizontalFlipBatch", "Ffmpeg", depth)["Mean"]
    print(f"| RGBA{depth} | {old / 1e6:.2f} | {new / 1e6:.2f} | {native / 1e6:.2f} | {new / native:.2f}x |")
