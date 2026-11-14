# 유지보수 안내

기준: 2026-11-14. 기능을 찾을 때는 아래 표에서 폴더를 고른 다음 함수 이름을 검색하세요. 파일 수를 억지로 줄이지 않고 관련 기능끼리 묶었습니다.

## 어디부터 고칠까?

| 작업 | 시작 위치 (`Assets/OfficeRampage/Scripts/` 기준) |
| --- | --- |
| 플레이 시작·이동·입력·기본 HUD | `Core/OfficeRampageGame.cs` |
| 달리기 시간·게이지·다리 동작 | `Player/OfficeRampageGame.Sprint.cs` |
| 공격력·사거리·재사용 간격 | `Combat/OfficeCombatRules.cs`, 실제 적용은 `Combat/OfficeRampageGame.Combat.cs` |
| 스킬·스킬 콤보 제외 | `Combat/OfficeRampageGame.WeaponSkills.cs`, `OfficeRampageGame.Elements.cs` |
| 더미·화상 | `Combat/OfficeRampageGame.Decoy.cs`, `OfficeBurn.cs` |
| 적 공격·이동·동결·유인 | `Enemies/OfficeDefender*.cs`, `OfficeRampageGame.Defenders.cs` |
| 회사 직급·고정 인원·이름 저장 | `UI/OfficeCompanySettings.cs`, `OfficeRampageGame.Company.cs` |
| 메뉴·난이도·이름 입력창 | `UI/OfficeRampageGame.Menu.cs`, `OfficeMenuRules.cs`, `OfficeNameEditor.cs` |
| 가구 위치·통로·길찾기 | `World/OfficeLayout.cs`, `OfficeNavigation.cs` |
| 물건 파괴·환경·회복 | `World/OfficeProp.cs`, `OfficeRules.cs`, `OfficeHealingPotion.cs` |
| 60% 조건·제한시간·엘리베이터 | `World/OfficeRampageGame.Progress.cs`, `OfficeProgressRules.cs`, `OfficeRampageGame.Elevator.cs` |
| 도입 선택·출근 결말 | `Story/OfficeRampageGame.Intro.cs` |
| 대표 사망·마지막 대사·엔딩 버튼 | `Story/OfficeRampageGame.Ending.cs` |
| 비서·BFG·사막·돈 갑옷 전환 | `Combat/OfficeRampageGame.Combat.cs` |
| 폭발·로켓 꼬리·효과 수명 | `Effects/` |
| 배경음·효과음·볼륨·전환 | `Audio/OfficeAudio.cs` |

주의: 현재 `Core/OfficeRampageGame.cs`에는 `BuildPerson`, `SpawnProp`, `BuildRoom` 같은 기본 생성 함수도 있습니다. 이번 변경은 파일 이동 중심으로, 모든 함수를 새 클래스로 재설계한 것은 아닙니다. `World/OfficeRampageGame.Effects.cs`는 이름과 달리 문 생성/폭발 호출 보조 코드를 포함합니다.

## partial 클래스와 파일명

`OfficeRampageGame.Menu.cs`, `OfficeRampageGame.Combat.cs` 등은 별도 컴포넌트가 아니라 **같은 OfficeRampageGame 클래스의 일부**입니다. 서로의 private 필드/함수를 사용합니다. 파일이 다른 폴더에 있어도 namespace와 클래스 선언을 유지하면 함께 컴파일됩니다. `OfficeDefender.*`도 같은 방식입니다.

현재 별도 asmdef를 추가하지 않았습니다. partial 파일을 임의로 서로 다른 어셈블리로 나누면 컴파일되지 않습니다. `Editor/`는 UnityEditor 전용 코드이며 런타임 코드와 구분해야 합니다.

## 수정 작업 순서

1. Play를 종료하고 Git 커밋 또는 프로젝트 백업을 만듭니다.
2. 관련 파일/함수를 찾고 한 기능씩 수정합니다. 파일 이동 시 Unity Project 창을 쓰거나 원본 `.meta`를 반드시 함께 이동합니다.
3. Unity 임포트·컴파일을 기다리고 Console 오류를 먼저 해결합니다.
4. 해당 기능을 검사한 뒤 이동·점프·충돌, 층 이동, 재시작 같은 연결 기능을 확인합니다.
5. 동작이 확인되면 README 또는 가이드를 갱신하고 `OfficeRampage_Update.md` 끝에 날짜/변경/검사/미확인 범위를 적습니다.

`Library`, `Temp`, `Logs`, 생성된 `.csproj`는 직접 수정하지 않습니다. 폴더 이동 후 IDE 파일 목록이 낡았으면 Unity의 External Tools에서 프로젝트 파일 재생성을 사용하세요. 예전 절대 경로를 사용하는 개인 스크립트가 있다면 아래 이동 지도에 맞춰 수정합니다.

## 실행 경로와 자산 참조

- 시작 씬: `Assets/OfficeRampage/Scenes/OfficeRampage.unity`
- Unity 메뉴 경로 상수: `Assets/OfficeRampage/Editor/OfficeRampageSceneMenu.cs`
- 빌드 씬 목록: `ProjectSettings/EditorBuildSettings.asset`
- 오디오 검색 루트: `Resources/OfficeAudio` (폴더를 그대로 유지)
- 기존 씬/스크립트의 GUID를 바꾸지 않았으므로 직렬화된 컴포넌트 참조는 유지됩니다. 기존 씬 내용은 수정하지 않고 위치만 옮겼습니다.

그래픽 교체는 [그래픽·음악 가이드](ASSET_GUIDE.md)를 보세요. `Art/`는 교체 자산 보관용이며 자동 로드 슬롯은 아닙니다.

## 검사 도구

프로젝트 루트에서 PowerShell로 각각 실행합니다. 경로에 맞게 파일을 찾도록 이번 폴더 구조에 맞춰 수정했습니다.

```powershell
powershell -NoProfile -File .\Tools\Test-OfficeLayout.ps1
powershell -NoProfile -File .\Tools\Test-OfficeDefense.ps1
powershell -NoProfile -File .\Tools\Test-CombatRules.ps1
```

각각 가구 배치/통로/환경, 적 규칙/길찾기, 전투 규칙을 검사합니다. 이 도구는 Unity 화면과 소리를 실행하지 않습니다. 일부 `OfficeDefenseRules`의 기존 규칙 검사와 실제 회사 설정의 43명 생성 검사는 범위가 다르므로 이 검사만으로 전체 플레이를 보장하지 않습니다.

Unity에서 함께 확인할 항목:

- 씬에 Missing Script가 없는지, 정상 시작되는지
- 한글 이름 입력/저장, 설정 취소/적용
- 이동·점프·바닥/가구 충돌·달리기
- 무기·F 스킬·더미·화상·동결과 콤보 제외
- 60% 조건·문·엘리베이터 이동·이전 층 제거
- 대표전·BFG·사막·엔딩·재시작
- 출근 선택의 43명/12초 집중 공격과 사망 화면
- 음악·효과음·볼륨·일시정지

이번 작업의 별도 Unity 자동 검사는 Codex 작업 공간의 `work/CombatValidation`에서 실행했습니다. 이 복제 프로젝트와 검사 파일은 배포 게임에 포함하지 않습니다. 프로젝트의 과거 `OfficeRampage_검증.md`, `OfficeRampage_사운드안내.md`는 초기 기록으로 남기고 안내 링크를 추가했습니다.

## 문제 복구

- **Missing Script:** 새로 만든 스크립트로 대체하기 전에 이동 전 `.meta` GUID와 현재 파일이 같은지 확인합니다.
- **씬이 안 열림:** 새 `Scenes` 경로를 사용하고 Editor 메뉴 상수와 빌드 씬 경로를 확인합니다.
- **음악 누락:** Resources 상대 경로/중복 이름/파일 형식과 Play 재시작 여부를 확인합니다.
- **컴파일 중복 정의:** 이동 전 파일과 이동 후 파일이 둘 다 남았는지 확인합니다. 원본을 복사만 한 뒤 둘 다 Assets에 두지 마세요.
- **롤백:** 아래 이동 표를 역순으로 적용하고 `.meta`도 함께 되돌립니다. Editor 메뉴 경로와 빌드 씬 목록, Tools의 경로도 같은 버전으로 복구합니다. 덮어쓰기 전 현재 수정본을 보관하세요.

## 전체 파일 이동 지도

아래 표는 이번 정리 시점의 정확한 파일 이동 목록입니다. 각 `.meta`도 동일하게 이동했습니다.

| 이전 | 현재 |
| --- | --- |
| OfficeRampageGame.cs | Scripts/Core/OfficeRampageGame.cs |
| OfficeRampageGame.Sprint.cs | Scripts/Player/OfficeRampageGame.Sprint.cs |
| OfficeRampageGame.Combat.cs | Scripts/Combat/OfficeRampageGame.Combat.cs |
| OfficeRampageGame.WeaponSkills.cs | Scripts/Combat/OfficeRampageGame.WeaponSkills.cs |
| OfficeRampageGame.Elements.cs | Scripts/Combat/OfficeRampageGame.Elements.cs |
| OfficeRampageGame.Decoy.cs | Scripts/Combat/OfficeRampageGame.Decoy.cs |
| OfficeCombatRules.cs | Scripts/Combat/OfficeCombatRules.cs |
| OfficeProjectile.cs | Scripts/Combat/OfficeProjectile.cs |
| OfficeBurn.cs | Scripts/Combat/OfficeBurn.cs |
| OfficeDefender.cs | Scripts/Enemies/OfficeDefender.cs |
| OfficeDefender.Distraction.cs | Scripts/Enemies/OfficeDefender.Distraction.cs |
| OfficeDefender.Frozen.cs | Scripts/Enemies/OfficeDefender.Frozen.cs |
| OfficeRampageGame.Defenders.cs | Scripts/Enemies/OfficeRampageGame.Defenders.cs |
| OfficeDefenseRules.cs | Scripts/Enemies/OfficeDefenseRules.cs |
| OfficeLayout.cs | Scripts/World/OfficeLayout.cs |
| OfficeNavigation.cs | Scripts/World/OfficeNavigation.cs |
| OfficeProp.cs | Scripts/World/OfficeProp.cs |
| OfficeRules.cs | Scripts/World/OfficeRules.cs |
| OfficeHealingPotion.cs | Scripts/World/OfficeHealingPotion.cs |
| OfficeSlidingDoor.cs | Scripts/World/OfficeSlidingDoor.cs |
| OfficeRampageGame.Elevator.cs | Scripts/World/OfficeRampageGame.Elevator.cs |
| OfficeRampageGame.Progress.cs | Scripts/World/OfficeRampageGame.Progress.cs |
| OfficeProgressRules.cs | Scripts/World/OfficeProgressRules.cs |
| OfficeRampageGame.Effects.cs | Scripts/World/OfficeRampageGame.Effects.cs |
| OfficeRampageGame.Menu.cs | Scripts/UI/OfficeRampageGame.Menu.cs |
| OfficeRampageGame.Company.cs | Scripts/UI/OfficeRampageGame.Company.cs |
| OfficeRampageGame.NameInput.cs | Scripts/UI/OfficeRampageGame.NameInput.cs |
| OfficeNameEditor.cs | Scripts/UI/OfficeNameEditor.cs |
| OfficeMenuRules.cs | Scripts/UI/OfficeMenuRules.cs |
| OfficeCompanySettings.cs | Scripts/UI/OfficeCompanySettings.cs |
| OfficeRampageGame.Intro.cs | Scripts/Story/OfficeRampageGame.Intro.cs |
| OfficeRampageGame.Ending.cs | Scripts/Story/OfficeRampageGame.Ending.cs |
| OfficeBlastEffect.cs | Scripts/Effects/OfficeBlastEffect.cs |
| OfficeRocketTrail.cs | Scripts/Effects/OfficeRocketTrail.cs |
| OfficeVisualEffect.cs | Scripts/Effects/OfficeVisualEffect.cs |
| OfficeAudio.cs | Scripts/Audio/OfficeAudio.cs |
| OfficeRampage.unity | Scenes/OfficeRampage.unity |


## 고정 배치·캐릭터 에셋 추가

- `Scripts/World/OfficeFixedFloor.cs`: 고정 JSON 읽기, 런타임 배치 생성. 가구 좌표 편집은 `Resources/OfficeLayouts`에서 합니다.
- `Scripts/Player/OfficeCharacterCatalog.cs`: 프리팹 배열 정의. 실제 값은 `Resources/OfficeCharacters/Catalog.asset`.
- `Scripts/Player/OfficeCharacterVisual.cs`: 인간 외형 교체, 기존 도형 숨김, 이동/공격/동결 애니메이션 연결.
- `Editor/OfficeArtBuilder.cs`: 원본 FBX의 Legacy 애니메이션·URP 재질·키 정규화 프리팹 생성. 카탈로그/프리팹 수동 편집을 덮으므로 재생성 전 백업합니다.
- `Art/KenneyMiniCharacters`: 원본 FBX, 텍스처, 재질, 프리팹 및 CC0 출처. .meta를 포함해 함께 보관합니다.
- 그래픽 교체 시 기존 Body/CharacterController를 제거하지 않고 시각 모델만 교체합니다. 새 모델 연결과 고정 JSON 편집 절차는 ASSET_GUIDE.md의 마지막 두 절을 확인하세요.


## 무기창·탄약·실내·출근 대사

- `Scripts/UI/OfficeRampageGame.WeaponHUD.cs`: 아이콘 실루엣, 잔탄 표시, 새 게임 탄약 지급과 1회 소비. 권총/샷건/로켓 수량은 `ResetAmmo`에서 조절합니다.
- `Scripts/Combat/OfficeRampageGame.Combat.cs`: 발사 전에 탄약을 검사합니다. 빈 탄창은 발사/투사체 생성을 막고 무기 교체나 층 이동으로 재충전되지 않습니다.
- `Scripts/Combat/OfficeRampageGame.Decoy.cs`: `DecoysPerOffice=3`, 조준/투척 제한과 HUD.
- `Scripts/World/OfficeRampageGame.Architecture.cs`: 천장·창틀·벽 외형·도시 배경 생성. 기존 경계 Collider를 보존합니다.
- `Scripts/World/OfficeRoofCutaway.cs`: 카메라가 천장보다 높을 때 지붕/벽 Renderer만 숨기는 처리. 충돌체는 유지합니다.
- `Scripts/Story/OfficeRampageGame.Intro.cs`: `OvertimeLines` 대사 배열, 3겹 반경, 높은 연출 카메라. 15회×0.8초 공격 중 2회마다 대사를 바꿉니다.


## 연속 낙뢰와 새 미디어 유지보수
- Combat/OfficeRampageGame.Elements.cs: 3초·0.5초 간격 총 6웨이브, 웨이브당 낙뢰 3개. 탐색 반경 14m/타격 반경 1.7m, 사람 피해 12·기절 3초, 물건 피해 18. SkillDamage 범위에서 적용하여 콤보 보상 방지.
- Enemies/OfficeDefender.Frozen.cs: Stun/TickStun. 기존 얼음과 기절을 독립적으로 감소시키고 두 상태 중 하나라도 남으면 이동/공격 중단. 일시정지·연출 중에는 시간 정지.
- Player/OfficeCharacterVisual.cs와 Core/OfficeRampageGame.cs: 망치 치켜들기 자세. Combat/OfficeRampageGame.Combat.cs: 망치 머리 90도 회전.
- UI/OfficeRampageGame.WeaponHUD.cs → Resources/OfficeIcons PNG. Audio/OfficeAudio.cs → Resources/OfficeAudio/CombatRefresh 효과음.
- Story/OfficeRampageGame.Intro.cs: 출근 연출 상단 자막. 결과 버튼은 사망 후에만 표시.
