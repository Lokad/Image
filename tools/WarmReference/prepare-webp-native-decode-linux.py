"""Freeze one exact native VP8L input on the existing original raster; no timing."""
import argparse,datetime,hashlib,json,os,subprocess
from pathlib import Path
parser=argparse.ArgumentParser();parser.add_argument('--cpu',type=int,default=0);args=parser.parse_args()
r=Path(__file__).resolve().parents[2];directory=r/'artifacts/warm-reference/webp-native-decode'
assert args.cpu in os.sched_getaffinity(0) and not directory.exists(),'Reuse the frozen preparation; do not regenerate it.'
old=json.loads((r/'artifacts/warm-reference/webp-decode/preparation.json').read_text())
raster=r/old['RasterFile'];pixels=raster.read_bytes();digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
assert len(pixels)==513*259*4 and digest(raster)==old['RasterSha256']=='70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'
worker=r/'external/warm-reference/libwebp-lossless-warm';identity=json.loads((worker.parent/'libwebp-lossless-identity.json').read_text())
assert digest(worker)==identity['workerSha256']
assert digest(r/'tools/WarmReference/libwebp-lossless-warm.c')==identity['sourceSha256']
assert digest(r/'tools/WarmReference/build-libwebp-lossless-linux.sh')==identity['buildScriptSha256']
directory.mkdir(parents=True);output=directory/'input.webp'
p=subprocess.run(['taskset','-c',str(args.cpu),str(worker),'webp-lossless-encode',str(raster),'513','259','8',str(output),'75'],input='quit\n',text=True,capture_output=True,check=True,timeout=30)
ready=json.loads(p.stdout);assert not p.stderr and ready['exactRgb'] and ready['lossless'] and ready['effort']==4 and ready['quality']==75
assert ready['codecThreads']==ready['conversionThreads']==ready['affinityCount']==1 and ready['threadType']==ready['activeThreadType']==0
assert output.stat().st_size==183812 and digest(output)=='08ddac115f1770d778e6dbc76373440224b32e6f93b616605e234c63206eebde'
assert output.read_bytes()[8:16]==b'WEBPVP8L'
for scalar in (False,True):
 command=[os.environ.get('LOKAD_IMAGE_FFMPEG','ffmpeg'),'-v','error','-threads:v','1','-thread_type:v','0','-filter_threads','1','-noautorotate']+(['-cpuflags','0'] if scalar else [])
 command+=['-i',str(output),'-frames:v','1','-threads:v','1','-pix_fmt','rgba','-f','rawvideo','pipe:1']
 decoded=subprocess.run(command,capture_output=True,check=True,timeout=30);assert not decoded.stderr and decoded.stdout==pixels
assert raster.read_bytes()==pixels
record=dict(SchemaVersion=1,Project='Lokad.Image',Recipe='HorizontalFlipInput.Create(8): same original 513x259 asymmetric RGBA8. One frozen libwebp exact=1 (FFmpeg encoder backend) VP8L input, lossless=1/method=4/quality=75/thread_level=0. Exact RGBA including hidden RGB. No source, quality, effort or strategy enumeration.',
 InputFile=output.relative_to(r).as_posix(),RasterFile=old['RasterFile'],InputBytes=output.stat().st_size,InputSha256=digest(output),RasterSha256=digest(raster),
 LibrarySha256=digest(r/'src/Lokad.Image/bin/Release/net10.0/Lokad.Image.dll'),PreparedUtc=datetime.datetime.now(datetime.timezone.utc).isoformat(),NativeEncoderBuild=identity)
(directory/'preparation.json').write_text(json.dumps(record,indent=2)+'\n')
(directory/'controls.json').write_text(json.dumps(dict(correctnessOnly=True,noTimingClaim=True,nativeNormalScalarExact=True,sourceUnchanged=True,nativeReady=ready),indent=2)+'\n')
print(json.dumps(dict(frozen=True,inputBytes=record['InputBytes'],inputSha256=record['InputSha256'],extraRasters=0,noTimingClaim=True)))
