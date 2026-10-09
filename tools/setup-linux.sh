#!/usr/bin/env bash
set -euo pipefail

# Original development bootstrap. No downloads or native tools enter the package.
repo_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd -P)
linux_root="$repo_root/external/linux-x64"
manifest="$repo_root/tools/external-sources.json"
jobs=4
toolchain_only=false
for argument in "$@"; do
    case "$argument" in
        --toolchain-only) toolchain_only=true ;;
        --jobs=*) jobs=${argument#--jobs=} ;;
        *) echo "Usage: tools/setup-linux.sh [--toolchain-only] [--jobs=N]" >&2; exit 2 ;;
    esac
done
[[ $(uname -s) == Linux && $(uname -m) == x86_64 ]] || { echo 'This bootstrap supports Linux x86_64.' >&2; exit 1; }
[[ $jobs =~ ^[1-9][0-9]*$ ]] || { echo 'Build jobs must be positive.' >&2; exit 2; }
for executable in python3 curl tar sha256sum sha512sum; do command -v "$executable" >/dev/null; done
mkdir -p "$repo_root/external/downloads" "$linux_root"

value() {
    python3 - "$manifest" "$1" <<'PY'
import json, sys
value = json.load(open(sys.argv[1], encoding="utf-8"))
for part in sys.argv[2].split("."):
    value = value[part]
print(value)
PY
}

download() {
    local name=$1 path url algorithm digest actual
    path="$repo_root/external/downloads/$(value "linuxBootstrap.$name.file")"
    url=$(value "linuxBootstrap.$name.url")
    algorithm=sha256
    [[ $name != dotnet ]] || algorithm=sha512
    digest=$(value "linuxBootstrap.$name.$algorithm")
    if [[ ! -f $path ]]; then
        curl --fail --location --retry 3 --output "$path.part" "$url"
        mv -- "$path.part" "$path"
    fi
    actual=$("${algorithm}sum" "$path")
    [[ ${actual%% *} == "$digest" ]] || { echo "Archive identity differs: $name" >&2; exit 1; }
    printf '%s\n' "$path"
}

sdk_archive=$(download dotnet)
if [[ ! -x $linux_root/dotnet/dotnet ]]; then
    mkdir -p "$linux_root/dotnet"
    tar -xzf "$sdk_archive" -C "$linux_root/dotnet"
fi
export DOTNET_ROOT="$linux_root/dotnet"
export PATH="$linux_root/dotnet:$linux_root/powershell:$linux_root/native/bin:$PATH"
[[ $("$linux_root/dotnet/dotnet" --version) == "$(value linuxBootstrap.dotnet.version)" ]] || { echo 'Installed SDK differs from the pin.' >&2; exit 1; }
pwsh_archive=$(download powershell)
if [[ ! -f $linux_root/powershell/pwsh ]]; then
    mkdir -p "$linux_root/powershell"
    tar -xzf "$pwsh_archive" -C "$linux_root/powershell"
    chmod +x "$linux_root/powershell/pwsh"
fi
[[ $("$linux_root/powershell/pwsh" -NoLogo -NoProfile -Command '$PSVersionTable.PSVersion.ToString()') == "$(value linuxBootstrap.powershell.version)" ]] || { echo 'Installed PowerShell differs from the pin.' >&2; exit 1; }

# Source this environment in subsequent shells; no global installation is needed.
python3 - "$linux_root" <<'PY'
import pathlib, shlex, sys
root = pathlib.Path(sys.argv[1])
q = lambda value: shlex.quote(str(value))
text = "export DOTNET_ROOT=" + q(root / "dotnet") + "\n"
text += "export PATH=" + q(str(root / "dotnet") + ":" + str(root / "powershell") + ":" + str(root / "native/bin")) + ':"$PATH"\n'
text += "export LOKAD_IMAGE_FFMPEG=" + q(root / "native/bin/ffmpeg") + "\n"
text += "export LOKAD_IMAGE_FFPROBE=" + q(root / "native/bin/ffprobe") + "\n"
(root / "environment.sh").write_text(text, encoding="utf-8")
PY
echo 'Pinned Linux SDK and PowerShell are ready.'
if $toolchain_only; then exit 0; fi
for executable in git gcc g++ make cmake pkg-config perl autoconf automake libtoolize; do
    command -v "$executable" >/dev/null || { echo "Missing native build prerequisite: $executable" >&2; exit 1; }
done

while IFS=$'\t' read -r name url tag revision destination; do
    path="$repo_root/$destination"
    if [[ ! -d $path/.git ]]; then git clone --quiet --depth 1 --single-branch --branch "$tag" "$url" "$path"; fi
    [[ $(git -C "$path" rev-parse HEAD) == "$revision" ]] || { echo "Source revision differs: $name" >&2; exit 1; }
    [[ -z $(git -C "$path" status --porcelain --untracked-files=no) ]] || { echo "Source checkout is modified: $name" >&2; exit 1; }
done < <(python3 - "$manifest" <<'PY'
import json, sys
for item in json.load(open(sys.argv[1], encoding="utf-8"))["sources"]:
    print("\t".join(item[key] for key in ("name", "url", "tag", "commit", "destination")))
PY
)

prefix="$linux_root/native"
build_root="$linux_root/build"
mkdir -p "$prefix" "$build_root" "$linux_root/sources"
nasm_archive=$(download nasm)
if [[ ! -d $linux_root/sources/nasm-2.16.03 ]]; then tar -xJf "$nasm_archive" -C "$linux_root/sources"; fi
mkdir -p "$build_root/nasm"
(
    cd "$build_root/nasm"
    "$linux_root/sources/nasm-2.16.03/configure" --prefix="$prefix"
    make -j"$jobs"
    make install
)
[[ $("$prefix/bin/nasm" -v) == "NASM version $(value linuxBootstrap.nasm.version)"* ]] || { echo 'Built NASM version differs.' >&2; exit 1; }

# zlib's CMake setup renames a tracked source header. Its configure script supports
# an out-of-tree static build that leaves the pinned reference checkout untouched.
mkdir -p "$build_root/zlib-static"
(
    cd "$build_root/zlib-static"
    "$repo_root/external/zlib/configure" --static --prefix="$prefix" --libdir="$prefix/lib"
    make -j"$jobs"
    make install
)
# Static native dependencies avoid global codec-library search paths.
cmake -S "$repo_root/external/libwebp" -B "$build_root/libwebp" -DCMAKE_BUILD_TYPE=Release \
    -DCMAKE_INSTALL_PREFIX="$prefix" -DCMAKE_INSTALL_LIBDIR=lib -DBUILD_SHARED_LIBS=OFF \
    -DWEBP_ENABLE_SIMD=ON -DWEBP_BUILD_ANIM_UTILS=OFF -DWEBP_BUILD_CWEBP=OFF -DWEBP_BUILD_DWEBP=OFF \
    -DWEBP_BUILD_GIF2WEBP=OFF -DWEBP_BUILD_IMG2WEBP=OFF -DWEBP_BUILD_VWEBP=OFF \
    -DWEBP_BUILD_WEBPINFO=OFF -DWEBP_BUILD_WEBPMUX=OFF -DWEBP_BUILD_LIBWEBPMUX=OFF -DWEBP_BUILD_EXTRAS=OFF
cmake --build "$build_root/libwebp" --parallel "$jobs"
cmake --install "$build_root/libwebp"
# Generate autotools files in an ignored copy, preserving the reference checkout.
zimg_revision=$(git -C "$repo_root/external/zimg" rev-parse HEAD)
zimg_source="$linux_root/sources/zimg-$zimg_revision"
mkdir -p "$zimg_source" "$build_root/zimg-static"
git -C "$repo_root/external/zimg" archive --format=tar "$zimg_revision" | tar -xf - -C "$zimg_source"
(
    cd "$zimg_source"
    bash ./autogen.sh
)
(
    cd "$build_root/zimg-static"
    "$zimg_source/configure" --prefix="$prefix" --libdir="$prefix/lib" --disable-shared --enable-static
    make -j"$jobs"
    make install
)
export PKG_CONFIG_PATH="$prefix/lib/pkgconfig:$prefix/share/pkgconfig${PKG_CONFIG_PATH:+:$PKG_CONFIG_PATH}"
[[ $(pkg-config --modversion libwebp) == 1.6.0 ]] || { echo 'libwebp dependency differs.' >&2; exit 1; }
[[ $(pkg-config --modversion zlib) == 1.3.1 ]] || { echo 'zlib dependency differs.' >&2; exit 1; }
[[ $(pkg-config --modversion zimg) == 3.0.6 ]] || { echo 'zimg dependency differs.' >&2; exit 1; }
mkdir -p "$build_root/ffmpeg"
(
    cd "$build_root/ffmpeg"
    "$repo_root/external/ffmpeg/configure" --prefix="$prefix" --disable-debug --disable-doc --disable-network \
        --disable-autodetect --disable-everything --enable-ffmpeg --enable-ffprobe --disable-ffplay \
        --enable-avcodec --enable-avformat --enable-avfilter --enable-swscale --enable-swresample \
        --enable-zlib --enable-libwebp --enable-libzimg --pkg-config-flags=--static \
        --enable-decoder=png,mjpeg,webp,vp8,rawvideo --enable-encoder=png,mjpeg,libwebp,rawvideo \
        --enable-demuxer=image2,image2pipe,image_png_pipe,image_jpeg_pipe,image_webp_pipe,mjpeg,rawvideo --enable-muxer=image2,image2pipe,rawvideo,webp,null \
        --enable-parser=png,mjpeg,webp,vp8 --enable-protocol=file,pipe \
        --enable-filter=scale,zscale,format,hflip,vflip,transpose,crop,pad,extractplanes,mergeplanes,alphamerge,colorkey,lutrgb,geq,setsar,null \
        --extra-cflags="-I$prefix/include" --extra-ldflags="-L$prefix/lib" --extra-libs="-lstdc++ -lm"
    make -j"$jobs"
    make install
)

python3 - "$repo_root" "$linux_root" <<'PY'
import hashlib, json, pathlib, platform, struct, subprocess, sys
repo, root = map(pathlib.Path, sys.argv[1:])
run = lambda *args: subprocess.check_output(args, text=True).strip()
digest = lambda path: hashlib.sha256(path.read_bytes()).hexdigest()
manifest = json.loads((repo / "tools/external-sources.json").read_text())
for source in manifest["sources"]:
    path = str(repo / source["destination"])
    if run("git", "-C", path, "rev-parse", "HEAD") != source["commit"] or run("git", "-C", path, "status", "--porcelain", "--untracked-files=no"):
        raise RuntimeError("Pinned source checkout changed during its build: " + source["name"])
capabilities = []
for pattern in ("png-rgba8-plain.png", "jpeg-ffmpeg-YCbCr444-9x7.jpg", "webp-*.webp", "vp8-*.webp"):
    fixture = next(iter(sorted((repo / "tests/Fixtures").glob(pattern))), None)
    if fixture is None:
        raise RuntimeError("Original codec bootstrap fixture is missing: " + pattern)
    decoded = subprocess.check_output([str(root / "native/bin/ffmpeg"), "-nostdin", "-hide_banner", "-loglevel", "error",
        "-threads:v", "1", "-f", "image2pipe", "-i", str(fixture), "-frames:v", "1", "-threads:v", "1",
        "-pix_fmt", "rgba", "-f", "rawvideo", "pipe:1"])
    if not decoded:
        raise RuntimeError("Native codec bootstrap produced no samples: " + fixture.name)
    capabilities.append({"fixture": fixture.name, "inputSha256": digest(fixture),
                         "decodedBytes": len(decoded), "decodedSha256": hashlib.sha256(decoded).hexdigest()})
normalization_checks = []
# Original point controls for SPEC.md's JPEG background/depth equation. This
# verifies the required geq filter and both normal/scalar byte conversions.
for depth in (8, 16):
    maximum = (1 << depth) - 1
    pixels = [(0, maximum, 1, 0), (1, maximum - 1, maximum, 1),
              (maximum // 2, 1, maximum - 1, maximum // 2),
              (maximum, 0, maximum // 3, (maximum + 1) // 2),
              (maximum - 1, maximum, 0, maximum - 1), (maximum, 1, maximum, maximum)]
    raw = bytes(v for p in pixels for v in p) if depth == 8 else struct.pack('<24H', *(v for p in pixels for v in p))
    denominator = maximum * maximum
    for background in (0x000000, 0xFFFFFF, 0x204060):
        target = bytes(v for p in pixels for v in [*((p[c] * p[3] * 255
            + ((background >> (16 - c * 8)) & 255) * (maximum - p[3]) * maximum
            + denominator // 2) // denominator for c in range(3)), 255])
        equations = [f"{name}='floor(({name}(X,Y)*alpha(X,Y)*255+{(background >> (16-c*8)) & 255}*({maximum}-alpha(X,Y))*{maximum}+{denominator // 2})/{denominator})*{256 if depth == 16 else 1}'"
                     for c, name in enumerate('rgb')]
        graph = f"format={'gbrap16le' if depth == 16 else 'gbrap'},geq=" + ':'.join(equations) + f':a={maximum}:i=nearest'
        graph += ',scale=flags=accurate_rnd+bitexact:sws_dither=none,format=rgba' if depth == 16 else ',format=rgba'
        for scalar in (False, True):
            decoded = subprocess.check_output([str(root / 'native/bin/ffmpeg'), '-nostdin', '-hide_banner', '-loglevel', 'error',
                *(['-cpuflags', '0'] if scalar else []), '-threads:v', '1', '-f', 'rawvideo', '-pixel_format', 'rgba64le' if depth == 16 else 'rgba',
                '-video_size', '3x2', '-i', 'pipe:0', '-filter_threads', '1', '-vf', graph, '-frames:v', '1', '-threads:v', '1',
                '-pix_fmt', 'rgba', '-f', 'rawvideo', 'pipe:1'], input=raw)
            if decoded != target:
                raise RuntimeError('Native JPEG single-round flattening/depth control failed.')
            normalization_checks.append({'depth': depth, 'backgroundRgb': background, 'scalar': scalar,
                'inputSha256': hashlib.sha256(raw).hexdigest(), 'decodedSha256': hashlib.sha256(decoded).hexdigest(), 'filterGraph': graph})
nearest_checks = []
# Original tiny coordinate-equation controls require pad/geq/crop and exact
# scalar-plane sample transport. They are correctness checks, not scaler timings.
for depth in (8, 16):
    values = [((12347 * x + 7919 * y + 23459 * channel + x * y * 101) & 65535)
              for y in range(2) for x in range(3) for channel in range(4)]
    if depth == 8:
        values = [(value + 128) // 257 for value in values]
    pack = lambda samples: bytes(samples) if depth == 8 else struct.pack('<' + str(len(samples)) + 'H', *samples)
    raw = pack(values)
    for width, height in ((7, 5), (2, 1)):
        target = pack([values[((2 * y + 1) * 2 // (2 * height) * 3
                              + (2 * x + 1) * 3 // (2 * width)) * 4 + channel]
                       for y in range(height) for x in range(width) for channel in range(4)])
        select = f"pad={max(3, width)}:{max(2, height)}:0:0,geq=lum='p(floor((2*X+1)*3/(2*{width})),floor((2*Y+1)*2/(2*{height})))':i=nearest,crop={width}:{height}:0:0:exact=1,setsar=1"
        packed, planar = ('rgba64le', 'gbrap16le') if depth == 16 else ('rgba', 'gbrap')
        graph = f'extractplanes=r+g+b+a[r][g][b][a];[r]{select}[rs];[g]{select}[gs];[b]{select}[bs];[a]{select}[as];[gs][bs][rs][as]mergeplanes=0x00102030:format={planar},format={packed}'
        for scalar in (False, True):
            decoded = subprocess.check_output([str(root / 'native/bin/ffmpeg'), '-nostdin', '-hide_banner', '-loglevel', 'error',
                *(['-cpuflags', '0'] if scalar else []), '-threads:v', '1', '-f', 'rawvideo', '-pixel_format', packed,
                '-video_size', '3x2', '-i', 'pipe:0', '-filter_complex_threads', '1', '-filter_complex', graph,
                '-frames:v', '1', '-threads:v', '1', '-pix_fmt', packed, '-f', 'rawvideo', 'pipe:1'], input=raw)
            if decoded != target:
                raise RuntimeError('Native nearest coordinate-equation control failed.')
            nearest_checks.append({'depth': depth, 'width': width, 'height': height, 'scalar': scalar,
                'inputSha256': hashlib.sha256(raw).hexdigest(), 'decodedSha256': hashlib.sha256(decoded).hexdigest(), 'filterGraph': graph})
nearest_point_checks = []
# The native point baseline keeps RGB and alpha independent. Zimg dispatches
# its own CPU implementation even when FFmpeg's -cpuflags is disabled.
for depth in (8, 16):
    values = [((12347 * x + 7919 * y + 23459 * channel + x * y * 101) & 65535)
              for y in range(2) for x in range(3) for channel in range(4)]
    if depth == 8:
        values = [(value + 128) // 257 for value in values]
    pack = lambda samples: bytes(samples) if depth == 8 else struct.pack('<' + str(len(samples)) + 'H', *samples)
    raw = pack(values)
    packed, planar, rgb = ('rgba64le', 'gbrap16le', 'gbrp16le') if depth == 16 else ('rgba', 'gbrap', 'gbrp')
    for width, height in ((7, 5), (2, 1)):
        target = pack([values[((2 * y + 1) * 2 // (2 * height) * 3
                              + (2 * x + 1) * 3 // (2 * width)) * 4 + channel]
                       for y in range(height) for x in range(width) for channel in range(4)])
        point = f'zscale=w={width}:h={height}:filter=point:rangein=full:range=full:matrixin=gbr:matrix=gbr:dither=none,format={rgb},setsar=1'
        graph = f'extractplanes=r+g+b+a[r][g][b][a];[g][b][r]mergeplanes=0x001020:format={rgb},{point},extractplanes=r+g+b[rs][gs][bs];[a]mergeplanes=0x000000:format={rgb},{point},extractplanes=g[as];[gs][bs][rs][as]mergeplanes=0x00102030:format={planar},format={packed}'
        for ffmpeg_cpu_flags_disabled in (False, True):
            decoded = subprocess.check_output([str(root / 'native/bin/ffmpeg'), '-nostdin', '-hide_banner', '-loglevel', 'error',
                *(['-cpuflags', '0'] if ffmpeg_cpu_flags_disabled else []), '-threads:v', '1', '-f', 'rawvideo', '-pixel_format', packed,
                '-video_size', '3x2', '-i', 'pipe:0', '-filter_complex_threads', '1', '-filter_complex', graph,
                '-frames:v', '1', '-threads:v', '1', '-pix_fmt', packed, '-f', 'rawvideo', 'pipe:1'], input=raw)
            if decoded != target:
                raise RuntimeError('Native nearest zscale point control failed.')
            nearest_point_checks.append({'depth': depth, 'width': width, 'height': height,
                'ffmpegCpuFlagsDisabled': ffmpeg_cpu_flags_disabled, 'zimgCpuDispatch': 'auto',
                'inputSha256': hashlib.sha256(raw).hexdigest(), 'decodedSha256': hashlib.sha256(decoded).hexdigest(), 'filterGraph': graph})
report = {
    "schemaVersion": 1, "platform": "linux-x64", "machine": platform.machine(),
    "sources": [{"name": source["name"], "commit": source["commit"]} for source in manifest["sources"]],
    "sdkVersion": run(str(root / "dotnet/dotnet"), "--version"),
    "powerShellVersion": manifest["linuxBootstrap"]["powershell"]["version"],
    "compiler": run("gcc", "--version"), "cppCompiler": run("g++", "--version"), "cmake": run("cmake", "--version"),
    "nasm": run(str(root / "native/bin/nasm"), "-v"),
    "libwebpVersion": run("pkg-config", "--modversion", "libwebp"), "zlibVersion": run("pkg-config", "--modversion", "zlib"),
    "zimgVersion": run("pkg-config", "--modversion", "zimg"),
    "setupScriptSha256": digest(repo / "tools/setup-linux.sh"),
    "ffmpegSha256": digest(root / "native/bin/ffmpeg"), "ffprobeSha256": digest(root / "native/bin/ffprobe"),
    "ffmpegVersion": run(str(root / "native/bin/ffmpeg"), "-version"),
    "dynamicDependencies": run("ldd", str(root / "native/bin/ffmpeg")),
    "nativeArchiveSha256": {name: digest(root / "native/lib" / name)
                            for name in ("libz.a", "libwebp.a", "libsharpyuv.a", "libzimg.a")},
    "codecInvocationChecks": capabilities,
    "jpegNormalizationChecks": normalization_checks,
    "nearestCoordinateEquationChecks": nearest_checks,
    "nearestPointChecks": nearest_point_checks,
    "configuration": (root / "build/ffmpeg/ffbuild/config.mak").read_text(),
    "method": "Pinned-source Linux development oracle. Compiler/platform-specific binary hashes are recorded; this is not a dedicated-VM performance result."
}
(root / "oracle-identity.json").write_text(json.dumps(report, indent=2) + "\n")
PY
echo "Pinned Linux oracle built; identity: $linux_root/oracle-identity.json"
echo "Source environment: $linux_root/environment.sh"
