"""One original raster; correctness-only controls for the filtered warm worker."""
import argparse,hashlib,json,os,subprocess,tempfile
from pathlib import Path
parser=argparse.ArgumentParser();parser.add_argument('--cpu',type=int,default=0);args=parser.parse_args()
root=Path(__file__).resolve().parents[2];worker=root/'external/warm-reference/ffmpeg-bilinear-warm'
source=root/'artifacts/warm-reference/webp-decode/source.rgba';pixels=source.read_bytes()
assert hashlib.sha256(pixels).hexdigest()=='70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'
# The worker additionally checks exact integer triangle reference/alpha and the
# existing filtered RGB maximum1/RMSE0.25. Here the independent public CLI must
# reproduce its whole native output exactly, with normal CPU dispatch.
graph='format=gbrap,zscale=w=256:h=129:filter=bilinear:rangein=full:range=full:matrixin=gbr:matrix=gbr:dither=none,format=gbrap,format=rgba'
with tempfile.TemporaryDirectory(prefix='lokad-image-bilinear-') as name:
 folder=Path(name);output=folder/'output.rgba';native=folder/'cli.rgba'
 base=[str(worker),'bilinear-resize',str(source),'513','259','8',str(output)]
 def run(arguments,protocol='check\nquit\n',cpus=None):
  return subprocess.run(['taskset','-c',str(args.cpu) if cpus is None else cpus,*arguments],input=protocol,capture_output=True,text=True,timeout=30)
 good=run(base,'sample 1\ncheck\nquit\n');assert good.returncode==0 and not good.stderr,good.stderr
 ready,sample,check=[json.loads(line) for line in good.stdout.splitlines()]
 assert ready['operation']=='bilinear-resize' and ready['filterThreads']==1 and ready['threadType']==0
 assert ready['affinityCount']==1 and ready['cpu']==args.cpu and ready['independentLiveOutputs']
 assert ready['premultipliedAlphaTimed'] and ready['encodedSampleSpace'] and ready['alphaMode']=='straight'
 assert (ready['outputWidth'],ready['outputHeight'])==(256,129)
 assert sample['operations']==1 and sample['completedOperations']==ready['initialCompletedOperations']+1
 assert sample['outputStride']>=256*4 and check['matched'] and check['alphaExact'] and check['sourceUnchanged']
 cli=['ffmpeg','-hide_banner','-loglevel','error','-threads:v','1','-f','rawvideo','-pixel_format','rgba','-video_size','513x259',
  '-i',str(source),'-filter_threads','1','-vf',graph,'-frames:v','1','-threads:v','1','-pix_fmt','rgba','-f','rawvideo','-y',str(native)]
 p=run(cli);assert p.returncode==0 and not p.stderr,p.stderr
 assert output.read_bytes()==native.read_bytes() and source.read_bytes()==pixels
 controls=[]
 def reject(label,arguments,protocol='quit\n',cpus=None):
  result=run(arguments,protocol,cpus);assert result.returncode!=0 and result.stderr,label;controls.append(label)
 reject('missing argument',base[:-1])
 reject('RGBA8 alpha tolerance forbidden',base+['--allow-alpha-rounding1'])
 for label,index,value in [('operation',1,'nearest-resize'),('dimension zero',3,'0'),('dimension text',3,'513x'),
  ('dimension bound',3,'32769'),('minimum output',3,'1'),('wrong precision',5,'12')]:
  values=base.copy();values[index]=value;reject(label,values)
 for label,data in [('truncated input',pixels[:-1]),('extra input',pixels+b'\0')]:
  path=folder/(label+'.raw');path.write_bytes(data);values=base.copy();values[2]=str(path);reject(label,values)
 for command in ('sample 0','sample 10000001','sample 1 extra','invalid'):reject(command,base,command+'\n')
 allowed=sorted(os.sched_getaffinity(0));assert len(allowed)>=2
 reject('multiple CPUs',base,cpus=','.join(map(str,allowed[:2])))
 record=dict(schemaVersion=1,project='Lokad.Image',correctnessOnly=True,noTimingClaim=True,complete=True,cpu=args.cpu,
  distinctSourceRasters=1,extraSourceRasters=0,sourceSha256=hashlib.sha256(pixels).hexdigest(),
  nativeOutputSha256=hashlib.sha256(output.read_bytes()).hexdigest(),inputBytes=len(pixels),outputBytes=output.stat().st_size,
  nativeCliPixelsExact=True,alphaExact=True,zeroAlphaRgbZero=True,independentLiveOutputs=True,
  nativeOperationCountersExact=True,sourceUnchanged=True,controls=controls,workerSha256=hashlib.sha256(worker.read_bytes()).hexdigest())
 destination=root/'artifacts/warm-reference/bilinear-controls.json';destination.write_text(json.dumps(record,indent=2)+'\n');print(json.dumps(record))
