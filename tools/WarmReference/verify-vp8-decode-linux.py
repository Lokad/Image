"""Original one-input VP8 protocol, conversion and ownership controls; no timing claim."""
import argparse,hashlib,json,os,subprocess,tempfile
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--root',type=Path,default=Path(__file__).resolve().parents[2]);p.add_argument('--cpu',type=int,default=0);a=p.parse_args()
root=a.root.resolve();prepared=json.loads((root/'artifacts/warm-reference/webp-match.json').read_text())
assert prepared['nativeQuality']==75 and prepared['match']['native']['codec']=='VP8'
input_file=root/prepared['nativeFile'];encoded=input_file.read_bytes()
assert hashlib.sha256(encoded).hexdigest()==prepared['match']['native']['encodedSha256']
worker=root/'external/warm-reference/ffmpeg-vp8-decode-warm';ffmpeg=os.environ.get('LOKAD_IMAGE_FFMPEG','ffmpeg')
expand='scale=iw*2:ih*2:flags=bilinear+accurate_rnd,crop=513:259,setsar=1'
color='scale=in_range=tv:out_range=pc:in_color_matrix=bt601:out_color_matrix=bt601:flags=bilinear+accurate_rnd+full_chroma_int:in_h_chr_pos=0:in_v_chr_pos=0,format=rgba64le,scale=flags=accurate_rnd+bitexact:sws_dither=none,format=rgba'
filter=f'extractplanes=y+u+v+a[y][u][v][a];[u]{expand}[ue];[v]{expand}[ve];[y]setsar=1[ye];[ye][ue][ve]mergeplanes=0x001020:format=yuv444p,{color}[color];[color][a]alphamerge,format=rgba'
cli=subprocess.run([ffmpeg,'-hide_banner','-loglevel','error','-threads:v','1','-thread_type:v','0','-filter_complex_threads','1','-noautorotate','-f','image2pipe','-i','pipe:0','-filter_complex',filter,'-frames:v','1','-threads:v','1','-pix_fmt','rgba','-f','rawvideo','pipe:1'],input=encoded,capture_output=True,check=True,timeout=30)
assert not cli.stderr and len(cli.stdout)==513*259*4
expected=cli.stdout
def run(cmd,protocol='quit\n',cpu=None):
    return subprocess.run(['taskset','-c',str(a.cpu) if cpu is None else cpu,*map(str,cmd)],input=protocol,capture_output=True,text=True,timeout=30)
with tempfile.TemporaryDirectory(prefix='vp8-decode-controls-',dir=root/'artifacts') as temporary:
    folder=Path(temporary);output=folder/'out.rgba';cmd=[worker,'vp8-decode',input_file,'513','259','8',output]
    r=run(cmd,'sample 1\nsample 2\ncheck\nsample 1\nquit\n');assert r.returncode==0 and not r.stderr,r.stderr
    rows=[json.loads(s) for s in r.stdout.splitlines()];ready=rows[0]
    assert ready['operation']=='vp8-decode' and ready['state']=='ready' and ready['inputFormat']=='YUVA420P' and not ready['lossless']
    assert ready['width']==513 and ready['height']==259 and ready['depth']==8 and ready['cpu']==a.cpu and ready['affinityCount']==1
    assert ready['codecThreads']==ready['conversionThreads']==1 and ready['threadType']==ready['activeThreadType']==0
    assert ready['colorConversionTimed'] and ready['stateResetTimed'] and ready['independentLiveOutputs'] and ready['initialCompletedOperations']==3
    assert [v['operations'] for v in rows if v['state']=='sample']==[1,2,1]
    assert [v['completedOperations'] for v in rows if v['state']=='sample']==[4,6,8]
    assert rows[3]==dict(state='checked',exact=True,sourceUnchanged=True)
    actual=output.read_bytes()
    if actual!=expected:
        diffs=[abs(x-y) for x,y in zip(actual,expected)]
        raise AssertionError(dict(nativeCliExact=False,maximum=max(diffs),different=sum(v!=0 for v in diffs),nativeSha256=hashlib.sha256(actual).hexdigest(),cliSha256=hashlib.sha256(expected).hexdigest()))
    assert input_file.read_bytes()==encoded
    for y in range(259):
        for x in range(513):
            alpha=0 if y==0 and x in (0,512) else (((x*12347+y*7919+3*23459+x*y*101)&65535)+128)//257
            assert actual[(y*513+x)*4+3]==alpha
    negatives=[]
    def reject(name,arguments,protocol='quit\n',cpu=None):
        v=run(arguments,protocol,cpu);assert v.returncode==1 and v.stderr,(name,v.returncode,v.stderr);negatives.append(name)
    reject('multiple CPUs',cmd,cpu=f'{a.cpu},{(a.cpu+1)%os.cpu_count()}')
    for protocol,name in [('sample 0\n','zero count'),('sample 10000001\n','count bound'),('sample 1 trailing\n','trailing protocol'),('unknown\n','unknown protocol')]:reject(name,cmd,protocol)
    reject('extra argument',cmd+['extra'])
    for index,value,name in [(1,'webp-decode','operation'),(3,'0','zero width'),(3,'32769','width bound'),(3,'512','wrong width'),(4,'258','wrong height'),(5,'16','depth'),(2,str(folder/'missing'),'missing input')]:
        v=cmd.copy();v[index]=value;reject(name,v)
    for name,data in [('truncated RIFF',encoded[:-1]),('RIFF signature',bytes([0])+encoded[1:]),('VP8X FourCC',encoded[:12]+b'XXXX'+encoded[16:]),('animated flag',encoded[:20]+bytes([encoded[20]|2])+encoded[21:])]:
        damaged=folder/(name.replace(' ','-')+'.webp');damaged.write_bytes(data);v=cmd.copy();v[2]=damaged;reject(name,v)
report=dict(project='Lokad.Image',schemaVersion=1,correctnessOnly=True,noTimingClaim=True,complete=True,distinctSourceRasters=1,qualityEnumeration=False,
    inputSha256=hashlib.sha256(encoded).hexdigest(),inputBytes=len(encoded),nativeCliOutputSha256=hashlib.sha256(expected).hexdigest(),nativeCliPixelsExact=True,alphaExact=True,
    originalRasterSha256=prepared['rasterSha256'],independentLiveOutputs=True,sourceUnchanged=True,countersExact=True,nativeReady=ready,negativeControls=negatives,
    colorPipeline='Centered 2x bilinear chroma expansion/crop; limited BT601 YUV444P to RGBA64LE; explicit no-dither RGBA8 reduction; exact decoded alpha merge. Normal SIMD, one thread per scaler. All conversion allocations and release timed.')
(root/'artifacts/warm-reference/vp8-decode-controls.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report))
