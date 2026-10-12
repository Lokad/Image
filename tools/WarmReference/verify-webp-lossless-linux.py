"""Original VP8L worker protocol/output controls on the retained raster; no timing claim."""
import argparse,hashlib,json,os,subprocess,tempfile
from pathlib import Path

parser=argparse.ArgumentParser();parser.add_argument('--cpu',type=int,default=0);args=parser.parse_args()
r=Path(__file__).resolve().parents[2];worker=r/'external/warm-reference/libwebp-lossless-warm'
meta=json.loads((r/'artifacts/warm-reference/webp-decode/preparation.json').read_text())
raster=r/meta['RasterFile'];raw=raster.read_bytes()
assert hashlib.sha256(raw).hexdigest()=='70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'
assert args.cpu in os.sched_getaffinity(0) and len(raw)==513*259*4
with tempfile.TemporaryDirectory(prefix='lokad-image-vp8l-controls-') as temporary:
 output=Path(temporary)/'output.webp'
 command=[str(worker),'webp-lossless-encode',str(raster),'513','259','8',str(output),'75']
 p=subprocess.run(['taskset','-c',str(args.cpu),*command],input='sample 1\nsample 2\ncheck\nsample 1\nquit\n',text=True,capture_output=True,timeout=60)
 assert p.returncode==0 and not p.stderr,p.stderr
 rows=[json.loads(line) for line in p.stdout.splitlines()];ready=rows[0]
 assert ready['operation']=='webp-lossless-encode' and ready['lossless'] and ready['quality']==75 and ready['effort']==4
 assert ready['codecThreads']==ready['conversionThreads']==ready['affinityCount']==1 and ready['threadType']==ready['activeThreadType']==0
 assert ready['cpu']==args.cpu and ready['exactRgb'] and ready['inputImportAllocationTimed'] and ready['repackingTimed'] and ready['independentLiveOutputs'] and ready['bitstreamDeterministic']
 samples=[v for v in rows if v['state']=='sample'];assert [v['operations'] for v in samples]==[1,2,1]
 assert [v['completedOperations'] for v in samples]==[4,6,8] and all(v['cpu']==args.cpu and v['elapsedNanoseconds']>0 for v in samples)
 assert rows[3]==dict(state='checked',exact=True,sourceUnchanged=True)
 assert raster.read_bytes()==raw and all(v['outputBytes']==output.stat().st_size for v in samples)
 for scalar in (False,True):
  cmd=['ffmpeg','-v','error','-threads','1','-filter_threads','1']+(['-cpuflags','0'] if scalar else [])
  cmd+=['-i',str(output),'-frames:v','1','-threads:v','1','-thread_type:v','0','-pix_fmt','rgba','-f','rawvideo','pipe:1']
  decoded=subprocess.run(cmd,capture_output=True,check=True,timeout=30).stdout
  assert decoded==raw
 bad=command.copy();bad[5]='16'
 assert subprocess.run(['taskset','-c',str(args.cpu),*bad],capture_output=True,timeout=10).returncode!=0
 assert subprocess.run(command,capture_output=True,timeout=10).returncode!=0 # Four-CPU default affinity must fail.
 print(json.dumps(dict(correctnessOnly=True,noTimingClaim=True,existingOriginalRasters=1,nativeNormalScalarExact=True,
                      sourceUnchanged=True,packetOwnersIndependent=True,outputBytes=output.stat().st_size,
                      outputSha256=hashlib.sha256(output.read_bytes()).hexdigest())))
