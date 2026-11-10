# Office Rampage audio credits

All 24 audio files were downloaded from the original asset pages below.
Every listed asset page identifies its license as **CC0**.
License: https://creativecommons.org/publicdomain/zero/1.0/
The bundled Kenney license files are preserved verbatim. Music retains its original audio content.
`audio-manifest.json` maps each local file to its source, original filename, author and SHA-256.

## Music

| Game stage | Track | Author | Original page |
| --- | --- | --- | --- |
| Office 1 | Singularity — calm | vitalezzz | https://opengameart.org/content/singularity-0 |
| Office 2 | Simple Action Beat | LushoGames | https://opengameart.org/content/simple-action-beat |
| Office 3 | The Hunt | Sudocolon | https://opengameart.org/content/the-hunt |
| Office 4 | Pressure | yd | https://opengameart.org/content/pressure |
| Office 5 | Singularity — action | vitalezzz | https://opengameart.org/content/singularity-0 |
| CEO boss | Action Theme Song | Ogrebane | https://opengameart.org/content/action-theme-song |

Offices 1 and 5 use two distinct arrangements of Singularity. Music follows the office number,
regardless of the randomized floor shape. Tracks loop and transitions crossfade for 1.2 seconds.
Per-track playback gain is adjusted from measured RMS levels; the downloaded files are unmodified.

## Effects — Kenney

- Impact Sounds: https://kenney.nl/assets/impact-sounds
  - wood0–2: impactWood_heavy_000–002.ogg
  - metal0–2: impactMetal_heavy_000–002.ogg
  - glass0–1: impactGlass_heavy_000–001.ogg
  - thud: impactPunch_heavy_000.ogg
  - wall: impactPlate_heavy_000.ogg
- Digital Audio: https://kenney.nl/assets/digital-audio
  - combo: highUp.ogg; door: powerUp1.ogg; win: threeTone1.ogg; lose: lowDown.ogg
  - swing: phaserDown1.ogg, used quietly as a stylized hammer swing
- Sci-fi Sounds: https://kenney.nl/assets/sci-fi-sounds
  - shot: explosionCrunch_000.ogg, used as a stylized shotgun report
  - rocket: laserLarge_000.ogg, used as a stylized rocket launch
  - explosion: lowFrequency_explosion_000.ogg

Effect playback uses pitch variation and layering. These are arcade-style effects, not recordings
of the depicted real-world weapons. They are freely distributed with this game under CC0.

## Technical verification

`audio-validation.json` records full-file decoder checks: duration, sample rate, channels,
peak and RMS amplitude. These are original decoded sample measurements before playback gain.
Lossy formats can contain decoded peaks above 1.0. All files passed hash and non-silence checks.
Actual listening and Unity Play-mode audio output have not been verified by the agent.
