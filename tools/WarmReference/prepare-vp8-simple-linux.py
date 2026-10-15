"""One original simple-filter keyframe on the existing opacity raster; no timings."""
from pathlib import Path
import hashlib,json,os,shlex,subprocess,tempfile

root=Path(__file__).resolve().parents[2]
prefix=root/'external/linux-x64/native'
output=root/'external/warm-reference/prepare-vp8-simple'
source=root/'tools/WarmReference/prepare-vp8-simple.c'
folder=root/'artifacts/warm-reference/vp8-simple-opaque'
sha=lambda data:hashlib.sha256(data).hexdigest()
env=dict(os.environ,PKG_CONFIG_PATH=str(prefix/'lib/pkgconfig'))
pkg=lambda *args:subprocess.check_output(['pkg-config',*args,'libwebp'],env=env,text=True).strip()
flags=['-std=c11','-O3','-Wall','-Wextra','-Werror']
output.parent.mkdir(parents=True,exist_ok=True)
subprocess.run(['gcc',*flags,*shlex.split(pkg('--cflags')),str(source),'-o',str(output),*shlex.split(pkg('--static','--libs'))],check=True)
original=(root/'artifacts/warm-reference/webp-decode/source.rgba').read_bytes()
assert len(original)==513*259*4 and sha(original)=='70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80'
rgba=bytearray(original);rgba[3::4]=bytes([255])*(513*259)
assert sha(rgba)=='54e27bd73c9ee221bdd4872e6b7bf79676257346cee2eff8ddacccb89e0cdb0a'
with tempfile.TemporaryDirectory(prefix='lokad-image-simple-vp8-') as temporary:
 work=Path(temporary);raw=work/'source.rgba';raw.write_bytes(rgba);packets=[];receipts=[]
 for index in range(2):
  target=work/f'{index}.webp'
  receipts.append(json.loads(subprocess.check_output([str(output),str(raw),str(target)],text=True)))
  packets.append(target.read_bytes())
 assert packets[0]==packets[1] and receipts[0]==receipts[1] and raw.read_bytes()==rgba
encoded=packets[0]
assert encoded[:4]==b'RIFF' and encoded[8:12]==b'WEBP' and int.from_bytes(encoded[4:8],'little')+8==len(encoded)
assert encoded[12:16]==b'VP8 ' and int.from_bytes(encoded[16:20],'little')+20+(len(encoded)&1)==len(encoded)
vp8=encoded[20:20+int.from_bytes(encoded[16:20],'little')]
assert vp8[0]&1==0 and vp8[3:6]==b'\x9d\x01\x2a' and int.from_bytes(vp8[6:8],'little')&16383==513 and int.from_bytes(vp8[8:10],'little')&16383==259

# Independent, original bitwise interval inspection of RFC 6386 sections7/9.
class Header:
 def __init__(self,data):
  self.data=data;self.end=10+(int.from_bytes(data[:3],'little')>>5)
  self.position=11;self.shift=7;self.range=255;self.value=data[10]
 def bit(self):
  assert self.value<self.range
  split=1+((self.range-1)*128//256);one=self.value>=split
  if one:self.value-=split;self.range-=split
  else:self.range=split
  while self.range<128:
   assert self.position<self.end
   self.range*=2;self.value=self.value*2+(self.data[self.position]>>self.shift&1)
   self.shift-=1
   if self.shift<0:self.position+=1;self.shift=7
  return int(one)
 def literal(self,count):
  result=0
  for _ in range(count):result=2*result+self.bit()
  return result
 def signed(self,count):
  value=self.literal(count);return -value if self.bit() else value
h=Header(vp8);assert h.bit()==0;h.bit();segmentation=h.bit()
# segments=1 makes the absence of segment-specific levels independently explicit.
assert segmentation==0,'Fixture has an unexpected segmentation model.'
simple=h.bit();level=h.literal(6);sharpness=h.literal(3);deltas=h.bit();reference=[0]*4;modes=[0]*4
if deltas and h.bit():
 for i in range(4):
  if h.bit():reference[i]=h.signed(6)
 for i in range(4):
  if h.bit():modes[i]=h.signed(6)
effective=[max(0,min(63,level+(reference[0]+modes[i] if deltas else 0))) for i in [0,1]]
assert simple==1 and level>0 and min(effective)>0,'Simple filter must execute for every possible intra mode.'
inspection=dict(simpleFilter=True,filterLevel=level,sharpness=sharpness,segmentation=False,deltasEnabled=bool(deltas),referenceDeltas=reference,modeDeltas=modes,minimumEffectiveFilterLevel=min(effective),maximumEffectiveFilterLevel=max(effective))
folder.mkdir(parents=True,exist_ok=True);packet=folder/'input.webp'
if packet.exists():assert packet.read_bytes()==encoded,'Preserve the frozen packet before changing preparation.'
else:packet.write_bytes(encoded)
record=dict(schemaVersion=1,project='Lokad.Image',purpose='Untimed original VP8 simple-filter fixture on the existing opacity projection',originalRasterSha256=sha(original),rasterSha256=sha(rgba),inputFile=packet.relative_to(root).as_posix(),inputSha256=sha(encoded),inputBytes=len(encoded),nativeQuality=75,nativeEffort=4,qualityEnumeration=False,deterministicRepeatExact=True,originalSourceUnchanged=True,simpleFilterInspection=inspection,referenceEncoder=receipts[0],referenceBuild=dict(sourceSha256=sha(source.read_bytes()),preparationScriptSha256=sha(Path(__file__).read_bytes()),workerSha256=sha(output.read_bytes()),compiler=subprocess.check_output(['gcc','--version'],text=True).splitlines()[0],flags=flags,version=pkg('--modversion'),staticArchives={n:sha((prefix/'lib'/n).read_bytes()) for n in ['libwebp.a','libsharpyuv.a'] if (prefix/'lib'/n).is_file()},libraries=pkg('--static','--libs').replace(str(prefix),'{native-prefix}')))
assert (root/'artifacts/warm-reference/webp-decode/source.rgba').read_bytes()==original
manifest=folder/'native-preparation.json';text=json.dumps(record,indent=2)+'\n'
if manifest.exists():assert manifest.read_text()==text,'Preserve existing provenance before changing authoring.'
else:manifest.write_text(text)
print(json.dumps(dict(prepared=True,inputSha256=record['inputSha256'],inputBytes=len(encoded),inspection=inspection)))
