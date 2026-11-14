# 그래픽·음악 교체 가이드

현재 프로젝트 기준: 2026-11-14. 먼저 프로젝트를 백업하고 **Play를 종료한 상태**에서 수정하세요. 한 번에 한 종류만 바꾸면 문제 원인을 찾기 쉽습니다.

## 먼저 알아둘 구조

- **음악·효과음:** 실제 파일을 이름으로 로드하므로 파일 교체만으로 변경할 수 있습니다.
- **인간 캐릭터:** Kenney 모델 프리팹과 `Resources/OfficeCharacters/Catalog.asset`을 사용합니다. 아래 새 캐릭터 절을 먼저 보세요.
- **가구·무기·경비견·효과:** 기본 도형을 코드로 생성합니다. 범용 프리팹 교체 슬롯은 아직 없으며 생성 코드 연결이 필요합니다.
- Play 중 Hierarchy의 `Generated Office Rampage`를 수정한 결과는 게임을 다시 시작하면 사라집니다.

## 음악 바꾸기: 코드 수정 없이

위치: `Assets/OfficeRampage/Resources/OfficeAudio/Music/`

| 이름(확장자 제외) | 재생 시점 | 현재 파일 |
| --- | --- | --- |
| `office1` | 1층, 출근 결말의 기반 음악 | `office1.mp3` |
| `office2` | 2층 | `office2.mp3` |
| `office3` | 3층 | `office3.ogg` |
| `office4` | 4층 | `office4.ogg` |
| `office5` | 대표층의 경비견 전투 | `office5.mp3` |
| `boss` | 경비견 처치 후 대표전 | `boss.wav` |

1. 사용할 음악을 준비합니다. 해당 파일을 게임에 배포해도 되는지 원본 제공처의 사용 조건을 확인하고 출처·조건을 함께 보관하세요.
2. 기존 파일을 **Assets 밖**에 백업합니다. 예: 프로젝트 밖 `MyAudioBackup/office1.mp3`.
3. 새 음악이 같은 형식이라면 기존과 **동일한 파일명**으로 덮어씁니다. `office1.mp3.meta`는 그대로 둡니다. 새 파일의 `.meta`를 가져와 기존 `.meta`를 덮어쓰지 마세요.
4. Unity 임포트가 끝나면 Project 창에서 AudioClip을 선택해 Inspector 미리 듣기로 정상 재생되는지 확인합니다.
5. 게임을 다시 Play하고 해당 층의 음악을 듣습니다. 이미 실행 중인 게임은 이전 AudioClip을 캐시하므로 종료 후 다시 실행하세요.

### MP3 대신 OGG/WAV로 바꾸려면

확장자만 바꾸는 것은 변환이 아닙니다. 실제 오디오 도구로 올바른 형식으로 내보내야 합니다. 예를 들어 `office1.ogg`를 사용할 경우:

1. 원본과 메타를 Assets 밖에 백업합니다.
2. Unity Project 창에서 기존 `office1.mp3`를 제거하고 새 `office1.ogg`를 넣습니다. Unity가 새 `.meta`를 생성하게 둡니다.
3. 같은 폴더에 `office1.mp3`와 `office1.ogg`를 동시에 두지 않습니다. 현재 코드는 확장자를 제외한 `OfficeAudio/Music/office1`로 찾으므로 중복 이름이 모호해집니다.
4. 다른 씬/프리팹에서 기존 AudioClip을 직접 참조했다면 새 파일을 다시 연결합니다. 현재 게임의 음악 코드는 이름 기반 로드입니다.

`Resources/OfficeAudio` 이하의 경로와 파일 이름은 코드의 약속입니다. 음악을 `Art`나 임의의 `Music` 폴더로 옮기는 것만으로는 재생되지 않습니다. 백업 파일을 다른 `Resources` 폴더에 같은 경로로 넣어도 충돌할 수 있습니다.

### 볼륨·루프·전환

- 게임 설정의 음악 볼륨과 M 음소거를 먼저 확인합니다. Unity Game 창의 Mute Audio도 확인하세요.
- 루프는 `Scripts/Audio/OfficeAudio.cs`의 `Awake`에서 `music[i].loop = true`로 설정합니다. 새 곡의 시작/끝이 자연스럽게 이어지는지도 들어보세요.
- 곡 전환은 `Crossfade`의 1.2초입니다.
- `ChangeRoom`의 `gains` 배열은 이전 곡에 맞춘 보정값입니다. 새 곡의 음량이 유난히 작으면 이 배열의 해당 순서(office1~5, boss)를 조정하세요. 모두 `1f`로 시작해 비교할 수 있습니다. 전체 설정 볼륨과 함께 곱해집니다.
- 긴 음악의 임포트 Load Type은 기존 Streaming 설정을 확인하고, 수정 시 Apply 후 재생합니다. 짧은 효과음은 음악과 별도로 다룹니다. 플랫폼별 최적값은 실제 빌드에서 메모리와 끊김을 확인해 결정하세요.

## 효과음 바꾸기

위치: `Assets/OfficeRampage/Resources/OfficeAudio/Sfx/`. 음악과 같은 방법으로 교체합니다. 현재 확장자는 `.ogg`입니다.

| 이름 | 사용처 |
| --- | --- |
| `wood0`~`wood2` | 나무 타격 변형 |
| `metal0`~`metal2` | 금속 타격 변형 |
| `glass0`, `glass1` | 유리 파손 변형 |
| `swing`, `shot`, `rocket` | 휘두르기·총격·로켓 발사 |
| `explosion` | 폭발 및 일부 강한 연출 |
| `thud`, `wall` | 피격/충격, 벽/음파 등 |
| `combo`, `door` | 콤보·회복·스킬 등 알림, 문 개방 |
| `win`, `lose` | 승리·패배 |

여러 기능에서 같은 효과음을 재사용하므로 `explosion`을 교체하면 로켓 외의 연출도 함께 달라질 수 있습니다. 특정 공격만 다른 소리를 쓰려면 새 키를 `OfficeAudio.Awake`의 목록에 추가하고, 해당 공격의 `audioBus.Play("키")` 호출을 바꿉니다.

기존 `AudioCredits`의 라이선스/목록은 최초 도입 음원 기록입니다. 이미 사용자가 바꾼 MP3 등에까지 자동으로 적용되는 라이선스가 아닙니다. 새 음원의 출처와 사용 조건도 함께 기록하세요. `Tools/Import-OfficeAudio.py`는 초기 음원 도입용이므로 음악 교체 때 무심코 재실행하지 마세요.

## 기존 도형 그래픽 바꾸기: 색·크기부터

파일 경로는 `Assets/OfficeRampage/Scripts/`를 기준으로 합니다.

| 바꿀 것 | 파일과 함수 |
| --- | --- |
| 주인공/기본 사람의 몸·얼굴·정장 | `Core/OfficeRampageGame.cs` → `BuildPerson` |
| 남녀 외형·이름표 색 | `UI/OfficeRampageGame.Company.cs` → `ApplyCompanyIdentity` |
| 경비견·보스 외형/분노 장식 | `Enemies/OfficeRampageGame.Defenders.cs` → `BuildDog`, `CreateDefender` 주변 생성 코드 |
| 무기 외형 | `Combat/OfficeRampageGame.Combat.cs` → `BuildExpandedWeapon` |
| 미인계 더미 | `Combat/OfficeRampageGame.Decoy.cs` → `BuildDecoyModel` |
| 책상·컴퓨터·정수기·복사기·캐비닛·화분 | `Core/OfficeRampageGame.cs` → `SpawnProp` |
| 바닥·조명·환경 | `Core/OfficeRampageGame.cs` → `BuildRoom`, `ResetRun` |
| 엘리베이터 내부 | `World/OfficeRampageGame.Elevator.cs` → `BuildElevatorCab` |
| 미닫이문 모양 | `World/OfficeRampageGame.Effects.cs` → `CreateSlidingDoor` |
| 로켓/핵폭발 | `Effects/OfficeRocketTrail.cs`, `Effects/OfficeBlastEffect.cs` |
| 불꽃·얼음 결정·번개 | `Combat/OfficeBurn.cs`, `Enemies/OfficeDefender.Frozen.cs`, `Combat/OfficeRampageGame.Elements.cs` |
| 메뉴 글자·크기·색 | `UI/OfficeRampageGame.Menu.cs` → `EnsureMenuStyles`, 각 화면 함수 |

예: 기존 도형 정장 색은 `BuildPerson`의 `Color suit`를 바꿉니다. 현재 새 인간 모델의 옷 색은 이 값으로 바뀌지 않으며, Kenney의 colormap/재질을 수정해야 합니다. `new Color(.12f, .22f, .29f)`의 RGB 값은 0~1 범위입니다. 함수 안의 `Shape`는 순서대로 부모, 이름, 위치, 크기, 색 등을 받습니다. 모델을 키우기 전에는 **충돌 크기와 다른 부품의 위치도** 확인하세요.

공통 `Mat(Color)`는 색상별 머티리얼을 재사용합니다. 하나의 `sharedMaterial`을 실행 중 수정하면 같은 재질을 쓰는 다른 물체까지 달라질 수 있습니다. 색상별로 다른 색 값을 전달하거나, 의도적으로 별도 머티리얼을 생성하고 수명을 관리하세요. 프로젝트는 URP이므로 새 재질도 URP에 맞춰야 합니다.

## FBX·프리팹으로 교체하기: 코드 연결 필요

이번 정리에서 다음 보관 폴더를 만들었습니다. **여기에 넣기만 하면 자동 적용되는 기능은 아닙니다.**

```text
Assets/OfficeRampage/Art/
  Models/      FBX 등 모델 원본
  Materials/   URP 머티리얼
  Textures/    텍스처
  Prefabs/     게임에 사용할 조립된 모델
```

권장 작업 순서:

1. 모델/텍스처를 해당 폴더에 임포트하고 재질을 연결해 프리팹을 만듭니다. 기준 방향은 앞쪽 +Z, 위쪽 +Y이며 바닥에 발이 닿도록 피벗·크기를 맞춥니다.
2. 생성 함수가 **기존 판정 루트 아래에 시각 모델만 Instantiate**하도록 연결합니다. 주인공의 CharacterController, 적의 OfficeDefender/CharacterController, 물건의 OfficeProp 및 충돌체를 제거하지 마세요.
3. 프리팹 연결은 현재 코드에 추가해야 합니다. 예를 들어 `OfficeRampageGame`에 `[SerializeField] private GameObject playerVisualPrefab;` 필드를 추가하고, 씬의 OfficeRampageGame 컴포넌트에 프리팹을 연결한 뒤 주인공 생성 부분에서 사용합니다. 필드만 선언하고 Inspector에 넣는 것으로는 생성 코드가 바뀌지 않습니다.
4. 같은 `BuildPerson`이 주인공·적·비서·더미·출근 군중에서 재사용됩니다. 주인공만 바꾸려면 주인공 생성 지점에서 분기하세요.
5. 현재 애니메이션은 이름으로 찾은 도형을 움직입니다. 새 모델을 쓰면 아래 계약을 유지하거나 애니메이션 코드를 Animator 기반으로 교체해야 합니다.

| 기존 계약 | 영향을 받는 기능 |
| --- | --- |
| `Trouser`, `Shoe`가 avatar의 직접 자식 | 걷기/달리기 다리 움직임 |
| `Jacket`, `Tie`, `Sleeve` 등 기본 부품 | 성별 외형 조정, 공격/패닉 동작 |
| `Baton`, `A4 hand` | 적 공격 동작 |
| `visual` 루트 | 보스 일어나기/쓰러짐, 패닉, 출근 군중 동작 |
| `weapon` 루트 | 휘두르기, 토르 스킬의 망치 치켜들기(확대 없음) |

특히 `ApplyCompanyIdentity`는 `Jacket`을 직접 찾습니다. 새 모델에 이 부품이 없는데 해당 코드를 그대로 실행하면 오류가 날 수 있습니다. 외형 교체와 연결 코드를 함께 바꾸세요. 인물별 성별 모델을 사용한다면 기존 치마/머리 도형 추가도 중복되지 않게 조정합니다.

가구는 `OfficeLayout.Point.Footprint`를 기준으로 배치 간격과 길찾기를 계산합니다. 책상을 크게 바꿨다면 Collider와 Footprint도 함께 맞추세요. 폭발은 현재 코드 생성 효과이며 ParticleSystem 프리팹 교체 슬롯은 없습니다. 새로운 효과를 연결한다면 생성 위치와 수명, 일시정지, 층 이동 시 제거까지 처리해야 합니다.

## 변경 후 확인

- Console에 컴파일 오류, Missing Script, 오디오 누락이 없는지 확인합니다.
- 1층에서 이동·달리기·점프·바닥/가구 충돌, 남녀 외형, 파괴를 확인합니다.
- 해당 무기와 F 스킬, 화상/동결/더미를 확인합니다.
- 층 이동 후 이전 효과가 남지 않는지, BFG 사막 전환과 출근/대표 결말이 정상인지 확인합니다.
- 음악은 각 층·대표전·음소거·일시정지·루프 경계를 들어봅니다.
- Play를 종료하고 다시 실행해도 같은 결과인지 확인한 뒤 백업을 갱신합니다.


## 현재 적용된 무료 인간 모델 교체

- 원본: [Kenney Mini Characters](https://kenney.nl/assets/mini-characters), 팩 2.0, CC0. `Art/KenneyMiniCharacters/License.txt` 및 `SOURCE.md`에 원문/출처를 보관합니다.
- FBX: `Art/KenneyMiniCharacters/character-male-*.fbx`, `character-female-*.fbx`
- 텍스처/재질: `Textures/colormap.png`, `CharacterURP.mat`
- 정규화된 키 약 1.9m의 프리팹: `Art/KenneyMiniCharacters/Prefabs/`
- 게임 연결: `Resources/OfficeCharacters/Catalog.asset`의 Male/Female 배열. 배열 순서와 직급/인물 순번으로 모델을 선택합니다. 이름/성별 설정은 기존대로 유지합니다.

### 같은 Kenney 팩에서 모델 교체

1. Play 종료 후 `Catalog.asset`을 선택합니다.
2. Male/Female 배열의 해당 칸에 **생성된 Prefabs 폴더의 프리팹**을 넣습니다. 빈 칸을 만들지 않습니다. FBX를 그대로 넣으면 높이/재질/애니메이션 연결이 빠질 수 있습니다.
3. 새 게임에서 해당 성별/직급 외형을 확인합니다. 카탈로그는 실행 중 캐시되므로 Play를 다시 시작합니다.
4. 원본 FBX 또는 텍스처를 바꾸고 재생성이 필요하면 **Tools → Office Rampage → Rebuild Character Prefabs**를 실행할 수 있습니다. 이 명령은 카탈로그 배열과 생성 프리팹을 다시 쓰므로 수동 수정한 프리팹/배열을 먼저 백업하세요.

### 다른 무료 모델을 연결할 때

현재 `OfficeCharacterVisual`은 Legacy Animation의 `idle`, `walk`, `sprint`, `attack-melee-right`, `holding-right-shoot`라는 클립 이름을 사용합니다. 같은 이름과 Legacy Animation이 준비된 프리팹이면 연결할 수 있습니다. Humanoid Animator 전용 모델을 넣는 것만으로는 이 어댑터와 호환되지 않습니다. 그 경우 어댑터를 Animator 기반으로 변경해야 합니다.

모델 자체에는 게임용 충돌체를 추가하지 않습니다. 기존 부모의 CharacterController와 OfficeDefender가 판정을 담당합니다. 기존 도형 부품은 자세/성별 코드와의 호환을 위해 남기고 Renderer를 숨깁니다. 걷기/달리기는 실제 이동량으로 애니메이션을 선택하고, 동결/일시정지는 재생을 멈춥니다. 쓰러짐은 기존 visual 루트 회전 연출을 유지합니다.

공용 colormap을 편집하면 여러 인물이 함께 달라집니다. 한 명만 바꾸려면 텍스처와 머티리얼을 복제하고 해당 프리팹 Renderer에 별도로 연결하세요. 재생성 메뉴를 다시 실행하면 이 수동 변경이 덮일 수 있습니다.

## 고정 배치 직접 편집

`Assets/OfficeRampage/Resources/OfficeLayouts/floor1.json`~`floor5.json`을 백업한 뒤 수정합니다. 실행 시 난수로 배치하지 않고 이 좌표를 그대로 읽습니다.

| 필드 | 의미 |
| --- | --- |
| width / length / name | 사무실 가로/세로/표시 이름 |
| furniture | 가구: x, z 위치, yaw 회전, kind 종류 |
| oil | 기름 위치 |
| enemies | 적 시작 위치. 1~5층 순서로 18/11/7/6/5개 유지. 대표는 별도 의자 위치에 생성 |
| potions | 포션 위치. 층당 2개 유지 |
| routes | 가구를 배치하지 않는 통로 영역 x/z/width/depth |

가구 kind는 0 책상, 1 컴퓨터 책상, 2 정수기, 3 복사기, 4 캐비닛, 5 화분입니다. x=0은 가운데, z=0은 입구 쪽이며 출구는 z=length입니다. 가구는 `OfficeLayout.Point.Footprint` 크기를 고려해 서로와 통로를 침범하지 않게 옮깁니다. 인물과 포션은 가구 안에 배치하지 마세요. 적 시작 위치 배열의 순서는 해당 층의 직급 순서와 연결됩니다.

파일 편집 후 Play를 재시작해야 캐시가 갱신됩니다. `OfficeLayout.Create`는 검사/최초 배치 작성용으로 남아 있지만 정상 게임의 `BuildRoom`은 `OfficeFixedFloor`를 읽습니다. 기존 Tools의 무작위 배치 검사만으로 수정한 JSON을 검증하지는 못하므로 수정한 층의 이동/충돌/출구/적 수를 실제로 확인하세요.


## 무기 아이콘·천장·창문 배경 변경

현재 무기 아이콘은 `Resources/OfficeIcons/`의 bat, hammer, pistol, machinegun, decoy, shotgun, rocket, bfg PNG입니다. 같은 경로/이름으로 이미지를 교체하면 됩니다. 기존 .meta를 유지하세요. `Scripts/UI/OfficeRampageGame.WeaponHUD.cs`에서 비율을 유지하여 표시하며 잔량은 별도 숫자입니다. 원본 무료 아이콘의 제작자 표기와 라이선스는 `Art/CombatRefresh-LICENSES.md` 및 첫 메뉴에 있습니다.

천장과 창틀, 창밖 도시 건물은 `Scripts/World/OfficeRampageGame.Architecture.cs`의 `BuildOfficeShell`에서 생성합니다. 천장은 높이 11.6m이며 위쪽 카메라에서는 OfficeRoofCutaway가 벽/천장 외형을 숨깁니다. 높이를 바꾸면 cutaway.height와 3인칭 카메라·점프 충돌도 함께 확인하세요. 창문은 외부가 보이는 창틀 구조이며 기존 보이지 않는 경계 충돌체는 그대로 남아 밖으로 나가지 못합니다.


## 새 전투 효과음
`Resources/OfficeAudio/CombatRefresh/`에 새 총성·타격음이 있습니다. `Scripts/Audio/OfficeAudio.cs`의 Load switch가 기존 이벤트 키를 이 파일들에 연결합니다. 이 연결이 있는 키는 기존 Sfx 폴더 파일보다 새 파일을 사용합니다. 음악 폴더는 그대로입니다.
- pistol/machinegun/shotgun.wav: 보존 중인 대체 총성. 현재는 사용하지 않고 세 총기 모두 이전 `Sfx/shot.ogg`로 복구했습니다.
- impact1~6.ogg: 물건 타격 변형, impact7.ogg: 사람 타격.
- rocket/explosion/thunder.ogg: 로켓 발사/폭발/낙뢰.
출처와 라이선스는 CombatRefresh-LICENSES.md를 참고하세요. 체감 음량은 사운드 설정과 OfficeAudio.Weapon의 개별 배율에서 조정합니다.


## 대표 붉은 오라 / 핵폭발 붕괴
- `Scripts/Effects/OfficeRageAura.cs`: 붉은 에너지 띠 12개+고리4개. 오렌지 화염과 독립적인 LineRenderer 효과입니다.
- `Scripts/Effects/OfficeBuildingCollapse.cs`: 천장48개·옆벽48개·앞뒤벽36개, 총132개 파편. 고정 개수로 생성하고 원래 외형을 숨깁니다. 충돌 없는 파편을 폭심 반대 방향으로 날린 뒤 중력으로 떨어뜨립니다. 6초 후 및 사막 전환 때 정리됩니다.
