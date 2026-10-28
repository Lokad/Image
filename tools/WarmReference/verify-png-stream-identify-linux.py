# Original same-packet controls for the benchmark-only PNG Identify adapter.
# Run after the Release harness/probe and native worker have been built.
from pathlib import Path
import json,hashlib,os,resource,shutil,struct,subprocess,zlib
r=Path(__file__).resolve().parents[2];resource.setrlimit(resource.RLIMIT_CORE,(0,0));out=r/'tmp/c04-png-stream-identify-controls';out.mkdir(exist_ok=True);sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest();base=r/'benchmarks/Lokad.Image.Benchmarks/bin/Release/net10.0/Lokad.Image.dll';probe=r/'tools/ImageProbe/bin/Release/net10.0/ImageProbe.dll';core=sha(base)
if sha(probe.parent/'Lokad.Image.dll')!=core:shutil.copyfile(base,probe.parent/'Lokad.Image.dll')
assert sha(probe.parent/'Lokad.Image.dll')==core
b=(r/'artifacts/warm-reference/png-cancel8/input.png').read_bytes();assert hashlib.sha256(b).hexdigest()=='4c0c823a1b84d7619f784ace6c2749ed10c0d54b7dc353beed3ad4888e0af57f';sig=b[:8];chunks=[];pos=8
while pos<len(b):
 n=struct.unpack_from('>I',b,pos)[0];chunks.append((b[pos+4:pos+8],b[pos+8:pos+8+n]));pos+=n+12
chunk=lambda t,p:struct.pack('>I',len(p))+t+p+struct.pack('>I',zlib.crc32(t+p));packet=lambda cs:sig+b''.join(chunk(t,p) for t,p in cs);same=[0,513,259,8,1,1];controls=[]
def add(name,data,expected=None):controls.append((name,data,expected))
add('original',b,same)
raw=list(chunks);raw[1]=(raw[1][0],bytes([raw[1][1][0]^255])+raw[1][1][1:]);add('invalid-deflate-valid-crc',packet(raw),same)
h=bytearray(chunks[0][1]);h[:4]=struct.pack('>I',514);add('changed-metadata-width',packet([(b'IHDR',bytes(h)),*chunks[1:]]),[0,514,259,8,1,1])
exif=b'II'+struct.pack('<HIH',42,8,1)+struct.pack('<HHIHHI',0x112,3,1,6,0,0);add('exif-orientation6',packet([chunks[0],(b'eXIf',exif),*chunks[1:]]),[0,513,259,8,1,6])
add('truncated',b[:-1]);add('missing-iend',packet(chunks[:-1]));add('trailing-byte',b+b'!');add('duplicate-ihdr',packet([chunks[0],chunks[0],*chunks[1:]]));add('duplicate-iend',b+chunk(b'IEND',b''));add('unknown-critical',packet([chunks[0],(b'ABCD',b''),*chunks[1:]]));add('nonletter-name',packet([chunks[0],(b'a1cd',b''),*chunks[1:]]));add('split-idat-order',packet([*chunks[:2],(b'tEXt',b'x'),*chunks[2:]]));add('no-idat',packet([chunks[0],chunks[-1]]));add('empty-idat',packet([chunks[0],(b'IDAT',b''),chunks[-1]]));add('animated-marker',packet([chunks[0],(b'acTL',b'\0'*8),*chunks[1:]]));add('rgba-trns',packet([chunks[0],(b'tRNS',b'\0\0'),*chunks[1:]]));add('duplicate-exif',packet([chunks[0],(b'eXIf',exif),(b'eXIf',exif),*chunks[1:]]));bad=bytearray(exif);bad[18:20]=struct.pack('<H',9);add('invalid-exif-value',packet([chunks[0],(b'eXIf',bytes(bad)),*chunks[1:]]));h[:4]=struct.pack('>I',32769);add('dimension-limit',packet([(b'IHDR',bytes(h)),*chunks[1:]]));bad=bytearray(b);bad[41]^=1;add('idat-crc',bytes(bad));bad=bytearray(b);bad[-1]^=1;add('iend-crc',bytes(bad))
worker=r/'external/warm-reference/ffmpeg-png-stream-identify-warm';env=dict(os.environ);env.update(DOTNET_ROOT=str((r/'external/linux-x64/dotnet').resolve()),DOTNET_PROCESSOR_COUNT='1',DOTNET_DbgEnableMiniDump='0');dotnet=r/'external/linux-x64/dotnet/dotnet';assert dotnet.is_file();proof=[]
for name,data,expected in controls:
 p=out/(name+'.png');p.write_bytes(data);n=subprocess.run(['taskset','-c','0',str(worker),'png-stream-identify',str(p),str(out/(name+'.info'))],input='quit\n',capture_output=True,text=True);m=subprocess.run(['taskset','-c','0',str(dotnet),str(probe),'identify',str(p)],capture_output=True,text=True,env=env)
 for side,result in [('native',n),('managed',m)]:
  (out/(name+'-'+side+'.stdout')).write_text(result.stdout);(out/(name+'-'+side+'.stderr')).write_text(result.stderr)
  assert (result.returncode==0)==(expected is not None),(name,side,result.returncode,result.stderr[-800:])
 if expected is not None:
  info=json.loads(m.stdout);actual=[info['Format'],info['Width'],info['Height'],info['BitDepth'],int(info['HasAlpha']),info['Orientation'],info['SourceColor'],info['SourceAlpha'],info['SourceComponentCount']];expected=expected+[2,2,4];assert actual==expected,(name,actual,expected);assert list(struct.unpack('<9I',(out/(name+'.info')).read_bytes()))==expected;ready=json.loads(n.stdout);assert ready['independentLiveOutputs'] and not ready['pixelDecode'] and ready['fullCrcTimed'] and ready['freshSeekableStream'] and ready['inputCopyTimed'] and ready['inputLengthQueryTimed'] and ready['eofChecked'] and ready['callerSourceLeftOpen'] and ready['streamAcquisitionAndReleaseTimed'] and not ready['inputView']
 else:assert any(x in m.stderr for x in ['ImageDataException','ImageLimitException','UnsupportedImageException']),name
 proof.append(dict(name=name,inputSha256=hashlib.sha256(data).hexdigest(),accepted=expected is not None,descriptor=expected,managedAndNativeAgree=True))
 print(json.dumps(dict(control=name,accepted=expected is not None,matched=True)),flush=True)
result=dict(schemaVersion=1,project='Lokad.Image',case='C04 fresh Stream PNG Identify',timing=False,librarySha256=core,inputSha256=hashlib.sha256(b).hexdigest(),inputBytes=len(b),nativeBuild=json.loads((r/'external/warm-reference/png-stream-identify-identity.json').read_text()),controls=proof,managedBoundary='Existing ImageProbe Identify Stream exercises the full qualified ReadInput/IdentifyCore/PngIdentifier; the warm selector adds direct ownership/admission/EOF controls.',equivalentMetadataAndValidation=True,typedSourceMetadataExact=True,descriptorFields=9,completeFreshStreamBoundary=True,invalidDeflateAcceptedWithoutPixelDecode=True,changedMetadataObserved=True,nativeLiveDescriptorsIndependent=True,noProductionBuild=True,noQualificationRepeat=True)
(out/'proof.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(dict(verified=len(proof),qualifiedCore=core,performanceMeasurement=False)))
