"""Untimed four-case radius-three/alpha/ownership qualification; no shape grid."""
import array,hashlib,json,math,os,subprocess,tempfile
from pathlib import Path
r=Path(__file__).resolve().parents[2];sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
folder=r/'artifacts/warm-reference/lanczos3';worker=r/'external/warm-reference/zimg-lanczos3-warm'
managed=json.loads((folder/'managed-controls.json').read_text());assert managed['correctnessOnly'] and managed['noTimingClaim']
assert managed['productionSha256']==sha(r/'src/Lokad.Image/bin/Release/net10.0/Lokad.Image.dll')
build=json.loads((r/'external/warm-reference/lanczos3-identity.json').read_text())
assert build['workerSha256']==sha(worker) and build['sourceSha256']==sha(r/'tools/WarmReference/zimg-lanczos3-warm.c')
cpu=0;assert cpu in os.sched_getaffinity(0);reports=[]
def samples(data,depth):
 if depth==8:return data
 result=array.array('H');result.frombytes(data);return result
with tempfile.TemporaryDirectory(prefix='lokad-image-lanczos3-') as name:
 temporary=Path(name);output=temporary/'output.raw'
 for case in managed['cases']:
  depth=case['depth'];width=case['outputWidth'];height=case['outputHeight'];caseid=case['caseId']
  source=r/('artifacts/warm-reference/webp-decode/source.rgba' if depth==8 else 'artifacts/warm-reference/bilinear16-audit/input.rgba64le')
  expected=folder/(caseid+'.raw');assert sha(source)==case['inputSha256'] and sha(expected)==case['outputSha256']
  base=[str(worker),'lanczos3-resize',str(source),'513','259',str(depth),str(output),str(expected),str(width),str(height)]
  def run(arguments,protocol='check\nquit\n',cpus=str(cpu)):
   return subprocess.run(['taskset','-c',cpus,*arguments],input=protocol,capture_output=True,text=True,timeout=30)
  modes=[]
  for mode in ['normal','scalar']:
   response=run(base+(['--scalar'] if mode=='scalar' else []))
   assert output.is_file(),response.stderr
   actual=output.read_bytes();assert len(actual)==case['outputBytes']
   a=samples(actual,depth);e=samples(expected.read_bytes(),depth);rgbmax=alphamax=rgbsquare=alphasquare=bad=0;examples=[]
   for i in range(0,len(e),4):
    assert a[i+3] or not any(a[i:i+3])
    for c in range(4):
     error=abs(a[i+c]-e[i+c])
     if c==3:alphamax=max(alphamax,error);alphasquare+=error*error
     else:rgbmax=max(rgbmax,error);rgbsquare+=error*error
     if error>1:
      bad+=1
      if len(examples)<8:examples.append(dict(x=i//4%width,y=i//4//width,channel=c,actual=a[i+c],expected=e[i+c],referenceAlpha=e[i+3],nativeAlpha=a[i+3]))
   rgbmse=rgbsquare/(width*height*3);alphamse=alphasquare/(width*height)
   passed=rgbmax<=1 and alphamax<=1 and rgbmse<=.0625 and alphamse<=.0625
   if passed:
    assert response.returncode==0 and not response.stderr,response.stderr
    ready,checked=[json.loads(line) for line in response.stdout.splitlines()]
    assert ready['operation']=='lanczos3-resize' and ready['radius']==3 and ready['haloPixels']==8
    assert ready['cpu']==cpu and ready['affinityCount']==ready['filterThreads']==1 and ready['threadType']==0
    assert (ready['depth'],ready['outputWidth'],ready['outputHeight'])==(depth,width,height)
    for key in ['haloAndPackingTimed','outputAllocationAndRepackingTimed','premultipliedFloatOutput','unpremultiplicationAndQuantizationTimed','independentLiveOutputs']:assert ready[key]
    assert checked['matched'] and checked['sourceUnchanged'] and checked['alphaMaximum']==alphamax and checked['rgbMaximum']==rgbmax
   else:assert response.returncode!=0 and 'no timing permitted' in response.stderr
   modes.append(dict(mode=mode,passQuality=passed,outputSha256=hashlib.sha256(actual).hexdigest(),rgbMaximum=rgbmax,alphaMaximum=alphamax,
    rgbRmse=math.sqrt(rgbmse),alphaRmse=math.sqrt(alphamse),exceedingSamples=bad,examples=examples,stderr=response.stderr))
  controls=[]
  def reject(label,arguments,protocol='quit\n',cpus=str(cpu)):
   response=run(arguments,protocol,cpus);assert response.returncode!=0 and response.stderr,label;controls.append(label)
  reject('missing argument',base[:-1]);reject('unknown dispatch',base+['--bad'])
  for label,index,value in [('operation',1,'bicubic-resize'),('geometry',3,'512'),('precision',5,'12'),('output geometry',8,'255')]:
   changed=base.copy();changed[index]=value;reject(label,changed)
  for label,data in [('truncated input',source.read_bytes()[:-1]),('extra input',source.read_bytes()+b'\0')]:
   path=temporary/(label+'.raw');path.write_bytes(data);changed=base.copy();changed[2]=str(path);reject(label,changed)
  if all(m['passQuality'] for m in modes):
   badexpected=bytearray(expected.read_bytes());badexpected[0]^=127;path=temporary/'false-expected.raw';path.write_bytes(badexpected)
   changed=base.copy();changed[7]=str(path);reject('false expected pixels',changed)
   reject('zero operations',base,'sample 0\nquit\n');reject('unknown protocol',base,'garbage\nquit\n')
   cpus=sorted(os.sched_getaffinity(0));assert len(cpus)>=2;reject('multiple CPUs',base,cpus=','.join(map(str,cpus[:2])))
  assert sha(source)==case['inputSha256'] and sha(expected)==case['outputSha256']
  reports.append(dict(**case,passQuality=all(m['passQuality'] for m in modes),modes=modes,negativeControls=controls))
  print(json.dumps(dict(caseId=caseid,modes=[{k:m[k] for k in ['mode','passQuality','rgbMaximum','alphaMaximum','rgbRmse','alphaRmse']} for m in modes])),flush=True)
record=dict(schemaVersion=1,project='Lokad.Image',correctnessOnly=True,noTimingClaim=True,complete=True,
 allCasesPassed=all(c['passQuality'] for c in reports),productionSha256=managed['productionSha256'],workerSha256=sha(worker),
 sourceSha256=sha(r/'tools/WarmReference/zimg-lanczos3-warm.c'),extraSourceRasters=0,cpu=cpu,cases=reports)
(folder/'native-controls.json').write_text(json.dumps(record,indent=2)+'\n')
if not record['allCasesPassed']:raise SystemExit('Native quality remains open; no timing permitted for failing cases.')
