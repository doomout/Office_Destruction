# 성인 직장인 캐릭터

- 제작자: Quaternius
- 원본: [Ultimate Modular Men](https://quaternius.com/packs/ultimatemodularcharacters.html), [Ultimate Modular Women](https://quaternius.com/packs/ultimatemodularwomen.html)
- 라이선스: CC0 1.0. 함께 보관한 Men-License.txt / Women-License.txt 참고.
- 다운로드: 원본 Google Drive가 다운로드 할당량 초과를 반환하여 [공개 CC0 배포본](https://github.com/agentkaerf/FreeModels)을 사용했다. Men/Suit, Women/Suit, Women/Formal의 Individual Characters/glTF 파일이다.
- 적용일: 2026-11-27

## 프로젝트 적용

원본 glTF의 남성 정장, 여성 바지 정장, 여성 정장 치마 모델을 FBX로 변환했다. 원본의 부속 권총과 미리보기용 구체는 제거하고 URP 재질을 적용했다. 정장 색상 3종씩, 총 남녀 6개 프리팹을 사용한다. 모델 높이는 기존 캐릭터 판정에 맞춰 약 1.95 Unity 단위로 맞췄다.

`Resources/OfficeCharacters/Catalog.asset`의 male/female 목록에서 게임 모델을 선택한다. 이름·성별 설정과 캐릭터 AI·전투 판정은 기존 기능을 그대로 사용한다. 기존 Kenney 원본은 삭제하지 않았다.

| 게임 클립 | 원본 애니메이션 |
| --- | --- |
| idle | Idle_Neutral |
| walk | Walk |
| sprint | Run |
| attack-melee-right | Sword_Slash |
| holding-right-shoot | Gun_Shoot |

`Animations`에는 위 다섯 모션을 Legacy Animation으로 저장했다. 프리팹의 Animation 클립 이름은 게임 코드에서 사용하므로 유지한다. 정장 색상은 `Materials`, 모델 크기와 방향은 `Prefabs`에서 편집한다. 사실적인 사진 기반 모델은 아니며, 성인 비율의 저폴리곤 정장 캐릭터다.
