# Original same-packet WebP Identify controls; no upstream fixtures or pixel decoding.
from pathlib import Path
import json,hashlib,os,resource,shutil,struct,subprocess
r=Path(__file__).resolve().parents[2];resource.setrlimit(resource.RLIMIT_CORE,(0,0));out=r/'tmp/c03-webp-identify-controls';out.mkdir(exist_ok=True);sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest();base=r/'benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.dll';probe=r/'tools/ImageProbe/bin/Release/net10.0/ImageProbe.dll';core=sha(base)
if sha(probe.parent/'Lokad.Image.dll')!=core:shutil.copyfile(base,probe.parent/'Lokad.Image.dll')
assert sha(probe.parent/'Lokad.Image.dll')==core
b=(r/'artifacts/warm-reference/webp-decode/input.webp').read_bytes();assert hashlib.sha256(b).hexdigest()=='a4ed2454404ca6c5798450fff0cfc502fd44a592c284b77e7861128b0c53927f'
same=[2,513,259,8,1,1];controls=[]
def chunk(kind,payload,pad=0):return kind+struct.pack('<I',len(payload))+payload+(bytes([pad]) if len(payload)&1 else b'')
def riff(*chunks):
 payload=b'WEBP'+b''.join(chunks);return b'RIFF'+struct.pack('<I',len(payload))+payload
def add(name,data,expected=None):controls.append((name,data,expected))
def canvas(flags=16,width=513,height=259,extra=b''):
 return chunk(b'VP8X',bytes([flags,0,0,0])+(width-1).to_bytes(3,'little')+(height-1).to_bytes(3,'little')+extra)
image=b[12:];header=b[20:25];short=chunk(b'VP8L',header)
exif=b'II'+struct.pack('<HIH',42,8,1)+struct.pack('<HHIHHI',0x112,3,1,6,0,0)
add('original',b,same)
add('opaque-payload-not-decoded',riff(short),same)
changed=bytearray(b);bits=struct.unpack_from('<I',changed,21)[0];struct.pack_into('<I',changed,21,(bits&~0x3fff)|513);add('changed-metadata-width',bytes(changed),[2,514,259,8,1,1])
add('exif-orientation6',riff(canvas(24),image,chunk(b'EXIF',exif)),[2,513,259,8,1,6])
changed=bytearray(b);struct.pack_into('<I',changed,4,len(b));add('invalid-riff-length',bytes(changed))
add('truncated',b[:-1]);add('trailing-byte',b+b'!')
changed=bytearray(b);changed[8]=0;add('invalid-webp-signature',bytes(changed))
changed=bytearray(b);struct.pack_into('<I',changed,16,len(b));add('invalid-chunk-length',bytes(changed))
add('duplicate-pixels',riff(image,image));add('missing-pixels',riff(chunk(b'JUNK',b'1234')))
add('invalid-vp8l-signature',riff(chunk(b'VP8L',b'!'+header[1:])))
changed=bytearray(header);changed[4]|=32;add('unsupported-vp8l-version',riff(chunk(b'VP8L',changed)))
add('nonzero-padding',riff(chunk(b'VP8L',header,pad=1)))
add('duplicate-exif',riff(canvas(24),image,chunk(b'EXIF',exif),chunk(b'EXIF',exif)))
add('exif-without-flag',riff(canvas(),image,chunk(b'EXIF',exif)))
changed=bytearray(exif);changed[18:20]=struct.pack('<H',9);add('invalid-exif-value',riff(canvas(24),image,chunk(b'EXIF',changed)))
add('canvas-mismatch',riff(canvas(width=514),image));add('animation-flag',riff(canvas(18),image))
add('unknown-padded-chunk',riff(chunk(b'JUNK',b'1'),image),same)
add('ignored-future-vp8x-fields',riff(canvas(0x90,extra=b'12'),image),same)
assert len(controls)==21
worker=r/'external/warm-reference/ffmpeg-webp-identify-warm';env=dict(os.environ);env.update(DOTNET_ROOT=str((r/'external/linux-x64/dotnet').resolve()),DOTNET_PROCESSOR_COUNT='1',DOTNET_DbgEnableMiniDump='0');dotnet=r/'external/linux-x64/dotnet/dotnet';assert dotnet.is_file();proof=[]
for name,data,expected in controls:
 p=out/(name+'.webp');p.write_bytes(data);n=subprocess.run(['taskset','-c','0',str(worker),'webp-identify',str(p),str(out/(name+'.info'))],input='quit\n',capture_output=True,text=True);m=subprocess.run(['taskset','-c','0',str(dotnet),str(probe),'identify',str(p)],capture_output=True,text=True,env=env)
 for side,result in [('native',n),('managed',m)]:
  (out/(name+'-'+side+'.stdout')).write_text(result.stdout);(out/(name+'-'+side+'.stderr')).write_text(result.stderr)
  assert (result.returncode==0)==(expected is not None),(name,side,result.returncode,result.stderr[-800:])
 if expected is not None:
  info=json.loads(m.stdout);actual=[info['Format'],info['Width'],info['Height'],info['BitDepth'],int(info['HasAlpha']),info['Orientation']];assert actual==expected,(name,actual,expected);assert list(struct.unpack('<6I',(out/(name+'.info')).read_bytes()))==expected;ready=json.loads(n.stdout);assert ready['independentLiveOutputs'] and not ready['pixelDecode'] and ready['fullStructureTimed'] and ready['publicParserTimed'] and ready['parserEofFlushTimed'] and ready['parserRequiredInputBufferingTimed']
 else:assert any(x in m.stderr for x in ['ImageDataException','ImageLimitException','UnsupportedImageException']),name
 proof.append(dict(name=name,inputSha256=hashlib.sha256(data).hexdigest(),accepted=expected is not None,descriptor=expected,managedAndNativeAgree=True))
 print(json.dumps(dict(control=name,accepted=expected is not None,matched=True)),flush=True)
result=dict(schemaVersion=1,project='Lokad.Image',case='C03 resident WebP Identify',timing=False,librarySha256=core,inputSha256=hashlib.sha256(b).hexdigest(),inputBytes=len(b),nativeBuild=json.loads((r/'external/warm-reference/webp-identify-identity.json').read_text()),controls=proof,managedBoundary='Existing ImageProbe Identify Stream exercises the same qualified IdentifyCore/ContainerHeaders.WebP; subsequent resident benchmark validates the span directly.',equivalentMetadataAndValidation=True,publicParserAndRequiredBufferingEofFlushTimed=True,opaquePayloadAcceptedWithoutPixelDecode=True,changedMetadataObserved=True,nativeLiveDescriptorsIndependent=True,noProductionBuild=True,noQualificationRepeat=True)
(out/'proof.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(dict(verified=len(proof),qualifiedCore=core,performanceMeasurement=False)))
