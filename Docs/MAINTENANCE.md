# 유지보수 안내

기준: 2026-11-24. 게임 동작을 바꾸지 않고 파일 위치를 정리했습니다. 파일명·클래스명과 기존 자산의 GUID는 유지합니다.

스토리 확장 작업 전에는 [스토리 모드 확장 기획서](STORY_MODE_PLAN.md)를 읽고, 구현 후에는 해당 체크리스트와 검증 기록을 갱신하세요. 현재 구현과 앞으로 만들 기능을 구분해 관리합니다.

## 프로젝트 입구

| 위치 | 용도 |
| --- | --- |
| `Assets/OfficeRampage/` | 실제 게임 코드·씬·그래픽·런타임 자산 |
| `Assets/Fonts/` | 원본 한글 폰트. Resources의 GameFont.asset에서 참조 |
| `Assets/Settings/` | 렌더 파이프라인 설정. 이름만 보고 삭제하지 마세요 |
| `Assets/Samples/` | 초기 CodexStarter 연습 코드, 기본 SampleScene, Unity TutorialInfo |
| `Assets/InputSystem_Actions.inputactions` | 프로젝트 입력 설정에서 참조하는 액션 자산 |
| `Docs/` | 편집·리소스 교체·모드별 안내 |
| `Tools/` | 규칙 검사·음원 가져오기 도구 |
| `Packages/`, `ProjectSettings/` | 패키지·Unity 설정 |
| `Library/`, `Temp/`, `Logs/` | Unity 생성 데이터. 유지보수용 소스 폴더가 아닙니다 |

시작 씬은 `Assets/OfficeRampage/Scenes/OfficeRampage.unity`입니다. 빌드 목록의 SampleScene은 `Assets/Samples/Scenes/SampleScene.unity`로 경로만 변경했고 기존 포함 설정은 유지했습니다.

## 기능별 코드 찾기

아래 경로는 `Assets/OfficeRampage/Scripts/` 기준입니다.

| 수정할 기능 | 위치 / 시작 파일 |
| --- | --- |
| 초기화·루프·입력·카메라·기본 생성 | `Core/OfficeRampageGame.cs` |
| 외형·달리기·가로 휘두르기 | `Player/` |
| 기본 무기 수치·콤보·상태 이상·투사체 | `Combat/OfficeCombatRules.cs`, `Combat/OfficeRampageGame.Combat.cs` |
| 적 AI·보스·경비견·경비 | `Enemies/` |
| 층·가구·문·엘리베이터·진행 조건 | `World/` |
| 도입·대표 결말·우주 연출 | `Story/` |
| 무한 모드 시작·루프 | `Endless/OfficeRampageGame.Endless.cs` |
| 무한 모드 공통 초기화·적·환경 | `Endless/OfficeRampageGame.EndlessExtras.cs`, `OfficeDefender.Endless.cs`, `OfficeRampageGame.EndlessEnvironment.cs` |
| 무한 모드 일반 무기·발사 효과 | `Endless/Weapons/` |
| UFO·블랙홀·항공 지원·로봇·위성·악마·교통사고 | `Endless/Support/` |
| 무한 보급 생성·획득·피해 2배 표시 | `Endless/Pickups/` |
| 무한 모드 상단 HUD·랭킹·저장 | `Endless/UI/` |
| 공통 메뉴·직원 설정·한글 입력·아이콘·폰트 | `UI/` |
| 음악·효과음 | `Audio/OfficeAudio.cs` |
| 폭발·불꽃·지구·건물 붕괴 | `Effects/` |

`OfficeRampageGame.*`와 `OfficeDefender.*`는 여러 파일로 나눈 partial 클래스입니다. 폴더를 옮겨도 같은 클래스이며 서로 private 멤버를 공유합니다. 임의로 asmdef를 추가해 서로 다른 어셈블리로 나누지 마세요. `Core`의 기본 생성 함수 등 내부 설계까지 분리한 작업은 아닙니다.

## 눈으로 수정하는 자산

`Assets/OfficeRampage/` 기준:

| 자산 | 위치 |
| --- | --- |
| 실제 1~4층 배치 | `Resources/OfficeFloors/Floor1~4.prefab` |
| 이전 배치·복구용 JSON | `Resources/OfficeLayouts/` (정상 플레이는 프리팹 우선) |
| 직원 원본 모델·재질·프리팹 | `Art/KenneyMiniCharacters/` |
| 층 재질 | `Art/FloorMaterials/` |
| 캐릭터 카탈로그 | `Resources/OfficeCharacters/Catalog.asset` |
| 음악·효과음 | `Resources/OfficeAudio/` |
| 무기 PNG 아이콘 | `Resources/OfficeIcons/` |
| 한글 폰트 연결 | `Resources/OfficeFonts/GameFont.asset` |
| 지구 자산 | `Resources/OfficeSpace/` |
| 음원 라이선스 | `AudioCredits/` |

층은 [그래픽 편집 가이드](VISUAL_FLOOR_EDITING.md), 모델·음악은 [교체 가이드](ASSET_GUIDE.md), 무한 모드는 [모드 안내](ENDLESS_MODE.md)를 참고하세요. 무한 전장은 실행 시 생성합니다.

## 새 기능을 넣을 때

1. 게임 공통 기능은 해당 기능 폴더에, 무한 전용 기능은 `Endless`의 해당 하위 폴더에 추가합니다.
2. 새 무한 무기는 `Endless.cs`의 이름/ID, `Weapons/OfficeRampageGame.EndlessWeapons.cs`의 공격 분기, `Pickups/OfficeRampageGame.EndlessLoot.cs`의 모델·탄약·획득, 보급 목록과 HUD를 함께 확인합니다. 파일만 추가한다고 자동 등록되지는 않습니다.
3. 지속 소환 효과는 초기화·사망·일시정지·재시작 시 종료/복구 경로를 함께 만듭니다. 스킬 피해는 `SkillDamage`를 사용해 콤보를 다시 채우지 않도록 합니다.
4. Editor 도구는 `Assets/OfficeRampage/Editor/`에 둡니다. 기존 층/캐릭터 재생성 도구는 수동 편집을 덮을 수 있으므로 실행 전 백업합니다.

## 파일 이동과 빈 폴더

- Unity Project 창에서 이동하거나 원본 `.meta`를 반드시 함께 이동합니다. 이름이 같아도 GUID가 바뀌면 씬·프리팹 참조가 끊깁니다.
- `Resources` 아래 상대 경로는 런타임 로딩 계약입니다. 이번 정리에서는 Resources와 그 내용의 위치를 변경하지 않았습니다.
- `Editor` 폴더는 에디터 전용 컴파일 구분입니다. 일반 폴더로 바꾸지 마세요.
- `Scripts`, `Resources`, `OfficeAudio`처럼 직접 파일이 없어도 하위 폴더에 자산이 있으면 빈 폴더가 아닙니다. 이번 조사에서는 Assets에 내용이 전혀 없는 폴더가 없어 삭제하지 않았습니다.
- 경로를 문자열로 사용하는 도구·빌드 씬 목록·문서를 이동과 함께 갱신합니다. 날짜별 과거 기록은 당시 경로를 보존합니다.

## 확인 순서

1. Unity 재임포트/컴파일 후 Console 오류 및 Missing Script 확인.
2. 시작 씬 Play → 이동·점프·바닥/벽 충돌 확인.
3. 층 편집 메뉴로 Floor1~4 열기, 프리팹·재질·캐릭터·폰트·오디오 연결 확인.
4. 변경 기능, 메뉴 복귀·재시작·일시정지 확인.
5. Web 배포 시 별도로 빌드하고 브라우저에서 한글 표시와 입력을 확인.

규칙 검사는 프로젝트 루트에서 `Tools/Test-OfficeLayout.ps1`, `Tools/Test-OfficeDefense.ps1`, `Tools/Test-CombatRules.ps1`을 각각 실행합니다. 규칙 검사만으로 화면·사운드·전체 플레이가 검증되지는 않습니다.

이번 재배치의 전체 이전/이후 경로는 [이동 목록](REORGANIZATION_2026-11-24.md)에 있습니다. 문제 시 기존 GUID를 유지한 채 이동 목록을 역순으로 적용하고, 경로를 갱신한 코드·문서·빌드 설정도 함께 복원하세요.
