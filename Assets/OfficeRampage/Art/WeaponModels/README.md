# 무기 모델 교체 안내

## 적용 모델
- Bat / Pistol / Machinegun / Shotgun: Quaternius **Zombie Apocalypse Kit**의 WoodenBat_Barbed / Pistol / SMG / Shotgun, CC0 1.0. Unity FBX로 변환하고 크기를 조정했습니다.
- Hammer / Rocket / BFG: 이 프로젝트를 위해 새로 만든 모델입니다. 둥근 손잡이, 모서리 처리, 원통 발사관, 녹색 코어로 구분합니다. 외부 프랜차이즈의 모델을 가져오지 않았습니다.
- 이번에는 일반 무기 7종을 교체했습니다. 광선검, 미인계 더미와 기타 무한 모드 소환 도구는 기존 디자인을 사용합니다.

## 출처와 라이선스
- 공식 팩: https://quaternius.com/packs/zombieapocalypsekit.html
- 공식 팩은 CC0로 공개되어 개인·상업용 사용이 가능합니다: https://creativecommons.org/publicdomain/zero/1.0/
- 실제 다운로드: https://github.com/agentkaerf/FreeModels/tree/main/Zombie%20Apocalypse%20Kit%20-%20March%202024/Weapons/glTF
- 원본 라이선스는 Quaternius-Source-License.txt로 보존했습니다. 미러의 원문 제목은 Ultimate Platformer Pack으로 되어 있어, 해당 Zombie Apocalypse 팩의 CC0 여부는 공식 팩 페이지에서도 별도로 확인했습니다.
- SourceHashes.json은 다운로드한 원본 파일의 SHA-256 기록입니다. Kenney/Ultimate Guns/Poly Pizza 후보는 실제 게임 에셋에 포함하지 않았습니다.

## 나중에 그래픽을 교체하려면
1. `Assets/OfficeRampage/Resources/OfficeWeapons`에서 해당 무기의 프리팹을 엽니다.
2. 모델의 총구/타격 방향은 로컬 +Z, 위쪽은 +Y입니다. 원점(0,0,0)은 오른손 손잡이 위치입니다.
3. 원점을 손잡이에 맞추고 모델 크기를 조정하세요. Collider와 Rigidbody는 붙이지 않습니다. 공격 판정은 기존 코드가 처리합니다.
4. URP/Lit 재질을 사용합니다. 원본 4종은 각 Atlas 텍스처를 Base Map에 연결합니다.
5. 기존 프리팹과 meta를 유지해서 수정하면 코드 연결을 바꿀 필요가 없습니다. 스토리에서 무기를 교체하고 무한 모드의 바닥 미리보기도 확인하세요.

`OfficeRampageGame.WeaponModels.cs`가 프리팹을 로드합니다. 파일이 없으면 기존 도형 무기로 대체합니다. 손 위치 보정은 OfficeCharacterVisual.Hands.cs가 담당합니다.
