"""Acquire only the pinned, licensed comparison faces into ignored external/."""
from pathlib import Path
import hashlib,json,tarfile,urllib.request
root=Path(__file__).resolve().parents[2]
proof=json.loads((Path(__file__).parent/'font-provenance.json').read_text(encoding='utf-8'))
download=root/'external/downloads/dejavu-fonts-ttf-2.37.tar.bz2'
download.parent.mkdir(parents=True,exist_ok=True)
if not download.exists():urllib.request.urlretrieve(proof['url'],download)
assert hashlib.sha256(download.read_bytes()).hexdigest()==proof['archiveSha256'],'Font archive checksum mismatch'
destination=root/'external/text-reference/fonts';destination.mkdir(parents=True,exist_ok=True)
with tarfile.open(download) as archive:
    for entry in proof['files']:
        name=entry['file'];matches=[m for m in archive.getmembers() if m.name.endswith('/'+name)]
        assert len(matches)==1
        data=archive.extractfile(matches[0]).read()
        assert len(data)==entry['bytes'] and hashlib.sha256(data).hexdigest()==entry['sha256']
        (destination/name).write_bytes(data)
print('Pinned comparison fonts and license verified in external/text-reference/fonts.')
