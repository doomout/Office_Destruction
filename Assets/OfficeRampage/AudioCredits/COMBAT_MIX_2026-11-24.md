# 2026-11-24 전투 사운드 교체

둠 원본 음악·효과음은 포함하지 않았습니다. 아래 제작자의 CC0 음원을 사용했습니다.

| 사용 위치 | 파일 / 원작 | 제작자 / 출처 |
| --- | --- | --- |
| 음악 1·3·5 슬롯 | once_more.wav / Once More [Metal] | nene — https://opengameart.org/content/once-more-metal |
| 음악 2·4·보스 슬롯 | heavy_battle.ogg / Heavy Battle 1 | MintoDog — https://opengameart.org/content/heavy-battle-1 |
| 휘두르기 | swing.ogg / OGG/knifeSlice.ogg | Kenney — https://lpc.opengameart.org/content/50-rpg-sound-effects |
| 적 음파 | sonic.ogg / OGG/cloth1.ogg | 같은 Kenney RPG Audio 팩 |
| 적 종이 투척 | paper.ogg / OGG/bookFlip1.ogg | 같은 Kenney RPG Audio 팩 |

라이선스: https://creativecommons.org/publicdomain/zero/1.0/ . 원본은 파일명만 변경했습니다. Unity 임포트 시 Vorbis 압축을 사용하며 음악은 Compressed In Memory, 짧은 효과음은 Decompress On Load입니다. 원본 파일과 출처별 SHA-256은 `combat-mix-manifest.json`, Kenney 동봉 라이선스는 `combat-mix-kenney-license.txt`에 있습니다.

새 원본 위치는 `Assets/OfficeRampage/Resources/OfficeAudio/CombatMix/`입니다. 기존 `Music/` 곡들은 보존했지만 현재 음악 슬롯은 OfficeAudio.Load에서 위 두 곡으로 연결합니다. 음악을 직접 바꾸려면 이 새 파일을 교체하거나 Load의 매핑을 수정하세요. Music/office 파일만 교체해도 이번 매핑에는 반영되지 않습니다.

물건 타격과 피격은 이미 포함된 Kenney Impact Sounds의 Sfx/wood0~2, metal0~2, thud를 다시 사용합니다(https://kenney.nl/assets/impact-sounds, CC0). 이전 CombatRefresh/impact1~7의 사람 목소리 계열 타격음 매핑은 해제했습니다. 총소리는 기존 권총·기관총·샷건 소스를 유지했습니다.

## 믹싱 변경

- 일반 공격/가구 충돌 때마다 음악을 65%로 줄이던 덕킹 제거. 이전 곡별 0.47~1.1 배율은 새 곡에 적용하지 않습니다.
- 음악 설정값에 1.2 배율, 전체 효과음에 0.75 배율, 피격 thud에 추가 0.55 배율 적용.
- 적 공격음은 최대 0.12초당 1회, 추가 0.55 배율. 기존 음파의 1.8배 고음 피치 제거. 공격 판정/피해/AI는 그대로입니다.
- 가구 충돌음은 최소 0.055초 간격으로 제한합니다. 물건 파괴 판정에는 영향이 없습니다.
- 신규 사용자 기본값: 음악 0.38(슬라이더 약63%), 효과음 0.6(60%). 기존 저장값·음소거는 덮어쓰지 않습니다.
- 점프 코드에는 전용 효과음 호출이 없었습니다. 점프 사운드를 새로 추가하지 않고 함께 들리는 적 공격·피격·휘두르기 소리를 조정했습니다.

## 검증과 청취

격리 Unity Editor에서 새 음원 5개의 디코딩·무음 여부·길이·피크/RMS, 6개 음악 슬롯, 새 효과음 연결, 적 100회 동시 공격의 소리 1회 제한, 음소거/일시정지/초기화 및 기존 이동·점프·충돌·UFO Play 검사를 수행했습니다. 실제 검사 수치는 별도 작업 산출물에 저장했습니다.

실제 스피커/헤드폰 청취와 Web 브라우저 출력은 검증하지 않았습니다. 음악의 취향과 체감 밸런스는 Play 모드에서 직접 확인해 주세요. 저장된 음악 음량이 0이거나 음소거 상태라면 설정에서 직접 조절해야 합니다.
