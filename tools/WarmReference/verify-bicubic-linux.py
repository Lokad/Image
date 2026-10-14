"""Correctness-only controls on the existing original RGBA8/16 rasters."""
import argparse,hashlib,json,math,os,struct,subprocess,tempfile
from pathlib import Path
parser=argparse.ArgumentParser()
parser.add_argument('--cpu',type=int,default=0)
parser.add_argument('--depth',type=int,choices=(8,16),default=8)
up=parser.add_mutually_exclusive_group()
up.add_argument('--up8',action='store_true')
up.add_argument('--up16',action='store_true')
up.add_argument('--mixed8',action='store_true')
up.add_argument('--mixed16',action='store_true')
up.add_argument('--cross8',action='store_true')
up.add_argument('--cross16',action='store_true')
up.add_argument('--wide8',action='store_true')
up.add_argument('--wide16',action='store_true')
up.add_argument('--horizontal8',action='store_true')
up.add_argument('--horizontal16',action='store_true')
args=parser.parse_args()
if args.up8 and args.depth!=8:parser.error('--up8 requires depth 8')
if args.up16 and args.depth!=16:parser.error('--up16 requires depth 16')
if args.mixed8 and args.depth!=8:parser.error('--mixed8 requires depth 8')
if args.mixed16 and args.depth!=16:parser.error('--mixed16 requires depth 16')
if args.cross8 and args.depth!=8:parser.error('--cross8 requires depth 8')
if args.cross16 and args.depth!=16:parser.error('--cross16 requires depth 16')
if args.wide8 and args.depth!=8:parser.error('--wide8 requires depth 8')
if args.wide16 and args.depth!=16:parser.error('--wide16 requires depth 16')
if args.horizontal8 and args.depth!=8:parser.error('--horizontal8 requires depth 8')
if args.horizontal16 and args.depth!=16:parser.error('--horizontal16 requires depth 16')
upsample=args.horizontal16 or args.horizontal8 or args.up8 or args.up16 or args.mixed8 or args.mixed16 or args.cross8 or args.cross16 or args.wide8 or args.wide16
width,height=(256,259) if args.horizontal8 or args.horizontal16 else (128,64) if args.wide8 or args.wide16 else (256,519) if args.cross8 or args.cross16 else (1027,129) if args.mixed8 or args.mixed16 else (1027,519) if upsample else (256,129)
root=Path(__file__).resolve().parents[2]
worker=root/('external/warm-reference/zimg-bicubic-horizontal16-warm' if args.horizontal16 else 'external/warm-reference/zimg-bicubic-horizontal8-warm' if args.horizontal8 else 'external/warm-reference/zimg-bicubic-wide16-warm' if args.wide16 else 'external/warm-reference/zimg-bicubic-wide8-warm' if args.wide8 else 'external/warm-reference/zimg-bicubic-cross16-warm' if args.cross16 else 'external/warm-reference/zimg-bicubic-cross8-warm' if args.cross8 else 'external/warm-reference/zimg-bicubic-mixed16-warm' if args.mixed16 else 'external/warm-reference/zimg-bicubic-mixed8-warm' if args.mixed8 else 'external/warm-reference/zimg-bicubic-up16-warm' if args.up16 else 'external/warm-reference/zimg-bicubic-up8-warm' if args.up8 else 'external/warm-reference/zimg-bicubic16-warm' if args.depth==16 else 'external/warm-reference/zimg-bicubic-warm')
source=root/('artifacts/warm-reference/bilinear16-audit/input.rgba64le' if args.depth==16 else 'artifacts/warm-reference/webp-decode/source.rgba')
reference=root/('artifacts/warm-reference/bicubic-horizontal16/rational.rgba64le' if args.horizontal16 else 'artifacts/warm-reference/bicubic-horizontal8/rational.rgba' if args.horizontal8 else 'artifacts/warm-reference/bicubic-wide16/rational.rgba64le' if args.wide16 else 'artifacts/warm-reference/bicubic-wide8/rational.rgba' if args.wide8 else 'artifacts/warm-reference/bicubic-cross16/rational.rgba64le' if args.cross16 else 'artifacts/warm-reference/bicubic-cross8/rational.rgba' if args.cross8 else 'artifacts/warm-reference/bicubic-mixed16/rational.rgba64le' if args.mixed16 else 'artifacts/warm-reference/bicubic-mixed8/rational.rgba' if args.mixed8 else 'artifacts/warm-reference/bicubic-up16/rational.rgba64le' if args.up16 else 'artifacts/warm-reference/bicubic-up8/rational.rgba' if args.up8 else 'artifacts/warm-reference/bicubic16/rational.rgba64le' if args.depth==16 else 'tmp/bicubic8-edge-reference.rgba')
pixels=source.read_bytes();expected=reference.read_bytes()
assert hashlib.sha256(pixels).hexdigest()==('3f861b61360b442c84063e7a6e1cb4f35957dcaa16f898048beaa36bbf4272f9' if args.depth==16 else '70f0fb18c341e2740b875a5a34e21ca63f0821b3386352fe2971020c5a6aac80')
assert len(expected)==width*height*4*(args.depth//8)
if args.horizontal16:assert hashlib.sha256(expected).hexdigest()=='ef78fb7dbfb05cb1a756a64702961cbe60abd5046fa5f2b7cc6fb91b3072cceb'
if args.wide16:assert hashlib.sha256(expected).hexdigest()=='3763385e1f45803392214ccf0b87ef6e4f1ec22f2f37701108cc52e9a20209d9'
if args.horizontal8:assert hashlib.sha256(expected).hexdigest()=='a5d2e75dbc30006f845c8a69061e096ded94ed06248e50350ec2c18456606a20'
if args.wide8:assert hashlib.sha256(expected).hexdigest()=='5067190b92fe644a75b0001848c4a555feb638c72592239dbf0ea0d63c518e71'
if args.cross16:assert hashlib.sha256(expected).hexdigest()=='ee02e57ff550047bf5296fb5af51c9396005653a8d37c3e0032abcdbb6db0db1'
if args.cross8:assert hashlib.sha256(expected).hexdigest()=='46b86650bc27febb8c0ed688a02591b660fe77b099c5b9e2ffe42a94e38cae5e'
if args.mixed16:assert hashlib.sha256(expected).hexdigest()=='6e289a9a8316325b78988adbad8ab7102c81e302216b79c45ef086410f6f691a'
if args.mixed8:assert hashlib.sha256(expected).hexdigest()=='1ca9938539ba1421fe4f1540055be5d0675ac8ddc1ea87df5932a93ca35a0356'
if args.depth==8 and not args.mixed8 and not args.cross8 and not args.wide8 and not args.horizontal8:assert hashlib.sha256(expected).hexdigest()==('08cde71ddb8b20be884c7ba812cec349f9302b21a348b79d3614bfda19805b94' if args.up8 else '656bb2248362c88dff8fe510dcc21055155976a3dd2b33aa20c48d2b036f59a8')
with tempfile.TemporaryDirectory(prefix='lokad-image-bicubic-') as name:
    folder=Path(name);output=folder/'output.rgba'
    base=[str(worker),'bicubic-resize',str(source),'513','259',str(args.depth),str(output),str(reference)]+([str(width),str(height)] if upsample else [])
    def run(arguments,protocol='check\nquit\n',cpus=None):
        return subprocess.run(['taskset','-c',str(args.cpu) if cpus is None else cpus,*arguments],
            input=protocol,capture_output=True,text=True,timeout=30)
    modes=[]
    for mode in ('normal','scalar'):
        good=run(base+(['--scalar'] if mode=='scalar' else []),'sample 1\ncheck\nquit\n')
        assert good.returncode==0 and not good.stderr,good.stderr
        ready,sample,check=[json.loads(line) for line in good.stdout.splitlines()]
        assert ready['operation']=='bicubic-resize' and ready['filterThreads']==1 and ready['threadType']==0
        assert ready['affinityCount']==1 and ready['cpu']==args.cpu and ready['independentLiveOutputs']
        assert (ready['outputWidth'],ready['outputHeight'],ready['depth'])==(width,height,args.depth)
        assert ready['haloPixels']==(8 if args.wide8 or args.wide16 else 4) and ready['haloAndPackingTimed'] and ready['outputAllocationAndRepackingTimed']
        assert sample['operations']==1 and sample['completedOperations']==ready['initialCompletedOperations']+1
        assert sample['outputStride']==width*4*(args.depth//8) and check['matched'] and check['sourceUnchanged']
        actual=output.read_bytes();assert len(actual)==len(expected)
        samples=struct.unpack('<'+str(len(actual)//2)+'H',actual) if args.depth==16 else actual
        references=struct.unpack('<'+str(len(expected)//2)+'H',expected) if args.depth==16 else expected
        rgb=[samples[i+c]-references[i+c] for i in range(0,len(samples),4) for c in range(3)]
        alpha=[samples[i+3]-references[i+3] for i in range(0,len(samples),4)]
        assert max(map(abs,rgb))<=1 and sum(d*d for d in rgb)/len(rgb)<=.0625
        assert max(map(abs,alpha))<=1 and sum(d*d for d in alpha)/len(alpha)<=.0625
        assert all(samples[i+3] or not any(samples[i:i+3]) for i in range(0,len(samples),4))
        modes.append(dict(mode=mode,outputSha256=hashlib.sha256(actual).hexdigest(),rgbMaximum=max(map(abs,rgb)),
            rgbRmse=math.sqrt(sum(d*d for d in rgb)/len(rgb)),alphaMaximum=max(map(abs,alpha)),
            alphaRmse=math.sqrt(sum(d*d for d in alpha)/len(alpha)),alphaDifferentPixels=sum(d!=0 for d in alpha)))
    assert source.read_bytes()==pixels and reference.read_bytes()==expected
    controls=[]
    def reject(label,arguments,protocol='quit\n',cpus=None):
        result=run(arguments,protocol,cpus)
        assert result.returncode!=0 and result.stderr,label
        controls.append(label)
    reject('missing argument',base[:-1])
    for label,index,value in [('operation',1,'hflip'),('geometry',3,'512'),('precision',5,'12')]:
        values=base.copy();values[index]=value;reject(label,values)
    reject('unknown dispatch',base+['--bad'])
    for label,data in [('truncated input',pixels[:-1]),('extra input',pixels+b'\0')]:
        path=folder/(label+'.raw');path.write_bytes(data);values=base.copy();values[2]=str(path);reject(label,values)
    bad=folder/'wrong-reference.rgba';bad.write_bytes(bytes(len(expected)))
    values=base.copy();values[7]=str(bad);reject('comparison bounds',values)
    for command in ('sample 0','sample 10000001','sample 1 extra','invalid'):
        reject(command,base,command+'\n')
    allowed=sorted(os.sched_getaffinity(0));assert len(allowed)>=2
    reject('multiple CPUs',base,cpus=','.join(map(str,allowed[:2])))
    record=dict(outputWidth=width,outputHeight=height,schemaVersion=1,project='Lokad.Image',correctnessOnly=True,noTimingClaim=True,complete=True,
        cpu=args.cpu,depth=args.depth,sourceSha256=hashlib.sha256(pixels).hexdigest(),referenceSha256=hashlib.sha256(expected).hexdigest(),
        referenceContract='Qualified original binary64 pixels; six certified exact rational alpha halves' if args.horizontal8 else 'Exact rational pixels',
        extraSourceRasters=0,sourceUnchanged=True,independentLiveOutputs=True,nativeOperationCountersExact=True,
        normalScalarBounded=True,modes=modes,controls=controls,workerSha256=hashlib.sha256(worker.read_bytes()).hexdigest())
    (root/('artifacts/warm-reference/bicubic-horizontal16-controls.json' if args.horizontal16 else 'artifacts/warm-reference/bicubic-horizontal8-controls.json' if args.horizontal8 else 'artifacts/warm-reference/bicubic-wide16-controls.json' if args.wide16 else 'artifacts/warm-reference/bicubic-wide8-controls.json' if args.wide8 else 'artifacts/warm-reference/bicubic-cross16-controls.json' if args.cross16 else 'artifacts/warm-reference/bicubic-cross8-controls.json' if args.cross8 else 'artifacts/warm-reference/bicubic-mixed16-controls.json' if args.mixed16 else 'artifacts/warm-reference/bicubic-mixed8-controls.json' if args.mixed8 else 'artifacts/warm-reference/bicubic-up16-controls.json' if args.up16 else 'artifacts/warm-reference/bicubic-up8-controls.json' if args.up8 else 'artifacts/warm-reference/bicubic16-controls.json' if args.depth==16 else 'artifacts/warm-reference/bicubic-controls.json')).write_text(json.dumps(record,indent=2)+'\n')
    print(json.dumps(record))
