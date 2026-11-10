"""Download verified CC0 music; extract only selected Kenney sounds from cached official ZIPs."""
import concurrent.futures
import hashlib
import http.cookiejar
import json
from pathlib import Path
import tempfile
import urllib.request
import uuid
import zipfile

ROOT = Path(__file__).resolve().parents[1]
DEST = ROOT / 'Assets/OfficeRampage/Resources/OfficeAudio'
LICENSE = ROOT / 'Assets/OfficeRampage/AudioCredits'
CACHE = Path(tempfile.gettempdir()) / 'OfficeRampageAudio'
MUSIC = [
    ('office1', 'singularity_calm_0.mp3', 'Singularity (calm)', 'vitalezzz', 'singularity-0'),
    ('office2', 'SimpleBeat_0.mp3', 'Simple Action Beat', 'LushoGames', 'simple-action-beat'),
    ('office3', 'The%20Hunt.ogg', 'The Hunt', 'Sudocolon', 'the-hunt'),
    ('office4', 'Pressure.ogg', 'Pressure', 'yd', 'pressure'),
    ('office5', 'singularity_action_0.mp3', 'Singularity (action)', 'vitalezzz', 'singularity-0'),
    ('boss', 'action%20game%20theme.wav', 'Action Theme Song', 'Ogrebane', 'action-theme-song'),
]
SFX = {
    'impact-sounds': {
        **{'wood' + str(i): 'impactWood_heavy_%03d.ogg' % i for i in range(3)},
        **{'metal' + str(i): 'impactMetal_heavy_%03d.ogg' % i for i in range(3)},
        **{'glass' + str(i): 'impactGlass_heavy_%03d.ogg' % i for i in range(2)},
        'thud': 'impactPunch_heavy_000.ogg', 'wall': 'impactPlate_heavy_000.ogg',
    },
    'digital-audio': {'combo': 'highUp.ogg', 'door': 'powerUp1.ogg',
                      'win': 'threeTone1.ogg', 'lose': 'lowDown.ogg', 'swing': 'phaserDown1.ogg'},
    'sci-fi-sounds': {'shot': 'explosionCrunch_000.ogg', 'rocket': 'laserLarge_000.ogg',
                      'explosion': 'lowFrequency_explosion_000.ogg'},
}

def meta(path, music=False):
    target = Path(str(path) + '.meta')
    if target.exists():
        return  # Never replace Unity-generated GUIDs.
    guid = uuid.uuid4().hex
    if path.is_dir():
        text = f'fileFormatVersion: 2\nguid: {guid}\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {{}}\n'
    elif path.suffix.lower() in ('.ogg', '.mp3', '.wav'):
        text = f'''fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 7
  defaultSettings:
    serializedVersion: 2
    loadType: {2 if music else 0}
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 1
    quality: 0.7
    conversionMode: 0
    preloadAudioData: {0 if music else 1}
  platformSettingOverrides: {{}}
  forceToMono: {0 if music else 1}
  normalize: 1
  loadInBackground: {1 if music else 0}
  ambisonic: 0
  3D: 1
  userData:
  assetBundleName:
  assetBundleVariant:
'''
    else:
        text = f'fileFormatVersion: 2\nguid: {guid}\n'
    target.write_text(text, encoding='utf-8')

def record(path, source, title, author, original):
    return dict(file=str(path.relative_to(ROOT)).replace('\\', '/'), title=title,
                author=author, license='CC0-1.0', source=source, original=original,
                sha256=hashlib.sha256(path.read_bytes()).hexdigest())

def download_music(item):
    key, filename, title, author, page = item
    path = DEST / 'Music' / (key + Path(filename).suffix)
    url = 'https://opengameart.org/sites/default/files/' + filename
    if not path.exists():
        data = urllib.request.urlopen(url, timeout=60).read()
        if len(data) < 10000 or data.lstrip().startswith(b'<!'):
            raise ValueError('Invalid audio download: ' + key)
        path.write_bytes(data)
    meta(path, True)
    return record(path, 'https://opengameart.org/content/' + page, title, author, url)

def get_pack(pack):
    path = CACHE / (pack + '.zip')
    if path.exists() and zipfile.is_zipfile(path):
        return path
    CACHE.mkdir(parents=True, exist_ok=True)
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
    opener.addheaders = [('User-Agent', 'Mozilla/5.0')]
    page = 'https://kenney.nl/assets/' + pack
    opener.open(page, timeout=60).read()
    request = urllib.request.Request('https://kenney.nl/download-link/' + ('assets/' + pack).encode().hex(),
                                     headers={'Referer': page, 'Accept': 'application/json'})
    url = json.load(opener.open(request, timeout=60))['url']
    path.write_bytes(opener.open(urllib.request.Request(url, headers={'Referer': page}), timeout=60).read())
    if not zipfile.is_zipfile(path):
        raise ValueError('Invalid Kenney download: ' + pack)
    return path

if __name__ == '__main__':
    for folder in [DEST / 'Music', DEST / 'Sfx', LICENSE]:
        folder.mkdir(parents=True, exist_ok=True)
    records = []
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        records.extend(pool.map(download_music, MUSIC))
    for pack, mapping in SFX.items():
        with zipfile.ZipFile(get_pack(pack)) as archive:
            license_path = LICENSE / (pack + '-LICENSE.txt')
            license_path.write_bytes(archive.read('License.txt'))
            for key, filename in mapping.items():
                path = DEST / 'Sfx' / (key + '.ogg')
                path.write_bytes(archive.read('Audio/' + filename))
                meta(path)
                records.append(record(path, 'https://kenney.nl/assets/' + pack,
                                      pack, 'Kenney', 'Audio/' + filename))
    (LICENSE / 'audio-manifest.json').write_text(json.dumps(records, indent=2, ensure_ascii=False), encoding='utf-8')
    for path in [DEST.parent, DEST, *DEST.rglob('*'), LICENSE, *LICENSE.rglob('*')]:
        if path.suffix != '.meta':
            meta(path, path.parent.name == 'Music')
    print('Imported', len(records), 'audio clips;', round(sum((ROOT / r['file']).stat().st_size for r in records) / 1048576, 1), 'MiB')
