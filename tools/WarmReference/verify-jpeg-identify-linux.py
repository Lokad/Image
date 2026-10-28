# Original same-packet controls for the benchmark-only JPEG Identify adapter.
# Run after the Release harness/probe and native worker have been built.
from pathlib import Path
import json,hashlib,os,resource,shutil,struct,subprocess
r=Path(__file__).resolve().parents[2];resource.setrlimit(resource.RLIMIT_CORE,(0,0));out=r/'tmp/c02-jpeg-identify-controls';out.mkdir(exist_ok=True);sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest();base=r/'benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.dll';probe=r/'tools/ImageProbe/bin/Release/net10.0/ImageProbe.dll';core=sha(base)
if sha(probe.parent/'Lokad.Image.dll')!=core:shutil.copyfile(base,probe.parent/'Lokad.Image.dll')
assert sha(probe.parent/'Lokad.Image.dll')==core
b=(r/'artifacts/warm-reference/jpeg-decode420/native.jpg').read_bytes();assert hashlib.sha256(b).hexdigest()=='ccca91bcdaddc90c7aecf62d07cdc629c09c27e607c955cfcc97a7bf91caaf08'
segments=[];position=2
while position<len(b):
 start=position;assert b[position]==255;position+=1
 while b[position]==255:position+=1
 marker=b[position];position+=1
 if marker==0xd9:break
 length=struct.unpack_from('>H',b,position)[0];end=position+length;segments.append(dict(marker=marker,start=start,lengthPosition=position,payload=position+2,end=end));position=end
 if marker==0xda:break
sof=next(s for s in segments if s['marker'] in [0xc0,0xc2]);sos=next(s for s in segments if s['marker']==0xda);dqt=next(s for s in segments if s['marker']==0xdb);entropy=sos['end'];same=[1,513,259,8,0,1];controls=[]
def add(name,data,expected=None):controls.append((name,data,expected))
def change(name,offset,data,expected=None):
 result=bytearray(b);result[offset:offset+len(data)]=data;add(name,bytes(result),expected)
def segment(marker,payload):return bytes([255,marker])+struct.pack('>H',len(payload)+2)+payload
exif=b'Exif\0\0'+b'II'+struct.pack('<HIH',42,8,1)+struct.pack('<HHIHHI',0x112,3,1,6,0,0);app1=segment(0xe1,exif)
add('original',b,same)
add('opaque-entropy-not-decoded',b[:entropy]+b'\x7f\xff\xd9',same)
change('bad-dqt-index',dqt['payload'],b'\x0f',same)
change('bad-sof-sampling',sof['payload']+7,b'\0',same)
change('changed-metadata-width',sof['payload']+3,struct.pack('>H',514),[1,514,259,8,0,1])
add('exif-orientation6',b[:2]+app1+b[2:],[1,513,259,8,0,6])
add('stuffed-entropy',b[:entropy]+b'\x11\xff\0\x22\xff\xd9',same)
add('truncated',b[:-1]);add('missing-eoi',b[:-2]);add('trailing-byte',b+b'!')
add('duplicate-sof',b[:sof['start']]+b[sof['start']:sof['end']]+b[sof['start']:])
add('missing-sof',b[:sof['start']]+b[sof['end']:])
change('empty-sos',sos['payload'],b'\0')
change('unsupported-precision',sof['payload'],b'\x0c')
change('zero-dimension',sof['payload']+1,b'\0\0')
change('bad-segment-length',sof['lengthPosition'],b'\0\1')
add('restart-outside-scan',b[:2]+b'\xff\xd0'+b[2:]);add('zero-marker-outside-scan',b[:2]+b'\xff\0'+b[2:])
add('duplicate-exif',b[:2]+app1+app1+b[2:]);bad=bytearray(exif);bad[24:26]=struct.pack('<H',9);add('invalid-exif-value',b[:2]+segment(0xe1,bytes(bad))+b[2:])
add('multiple-scan-headers',b[:entropy]+b'\x11'+b[sos['start']:entropy]+b'\x22\xff\xd9',same)
worker=r/'external/warm-reference/ffmpeg-jpeg-identify-warm';env=dict(os.environ);env.update(DOTNET_ROOT=str((r/'external/linux-x64/dotnet').resolve()),DOTNET_PROCESSOR_COUNT='1',DOTNET_DbgEnableMiniDump='0');dotnet=r/'external/linux-x64/dotnet/dotnet';assert dotnet.is_file();proof=[]
for name,data,expected in controls:
 p=out/(name+'.jpg');p.write_bytes(data);n=subprocess.run(['taskset','-c','0',str(worker),'jpeg-identify',str(p),str(out/(name+'.info'))],input='quit\n',capture_output=True,text=True);m=subprocess.run(['taskset','-c','0',str(dotnet),str(probe),'identify',str(p)],capture_output=True,text=True,env=env)
 for side,result in [('native',n),('managed',m)]:
  (out/(name+'-'+side+'.stdout')).write_text(result.stdout);(out/(name+'-'+side+'.stderr')).write_text(result.stderr)
  assert (result.returncode==0)==(expected is not None),(name,side,result.returncode,result.stderr[-800:])
 if expected is not None:
  info=json.loads(m.stdout);actual=[info['Format'],info['Width'],info['Height'],info['BitDepth'],int(info['HasAlpha']),info['Orientation'],info['SourceColor'],info['SourceAlpha'],info['SourceComponentCount']];expected=expected+[0,1,3];assert actual==expected,(name,actual,expected);assert list(struct.unpack('<9I',(out/(name+'.info')).read_bytes()))==expected;ready=json.loads(n.stdout);assert ready['independentLiveOutputs'] and not ready['pixelDecode'] and ready['fullStructureTimed'] and ready['canonicalHeaderProjectionTimed'] and ready['discardAllHeaderExtractionTimed'] and ready['pixelBufferRequests']==0
 else:assert any(x in m.stderr for x in ['ImageDataException','ImageLimitException','UnsupportedImageException']),name
 proof.append(dict(name=name,inputSha256=hashlib.sha256(data).hexdigest(),accepted=expected is not None,descriptor=expected,managedAndNativeAgree=True))
 print(json.dumps(dict(control=name,accepted=expected is not None,matched=True)),flush=True)
result=dict(schemaVersion=1,project='Lokad.Image',case='C02 resident JPEG Identify',timing=False,librarySha256=core,inputSha256=hashlib.sha256(b).hexdigest(),inputBytes=len(b),nativeBuild=json.loads((r/'external/warm-reference/jpeg-identify-identity.json').read_text()),controls=proof,managedBoundary='Existing ImageProbe Identify Stream exercises the same qualified IdentifyCore/ContainerHeaders.Jpeg; subsequent resident benchmark validates the span directly.',equivalentMetadataAndValidation=True,typedSourceMetadataExact=True,descriptorFields=9,canonicalSof2HeaderProjectionTimed=True,unadjustedNativeStrongerDqtValidationRemoved=True,opaqueEntropyAcceptedWithoutPixelDecode=True,changedMetadataObserved=True,nativeLiveDescriptorsIndependent=True,noProductionBuild=True,noQualificationRepeat=True)
(out/'proof.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(dict(verified=len(proof),qualifiedCore=core,performanceMeasurement=False)))
