# Web 빌드 폰트 수정

작성일: 2026-11-23. 게임 로직 및 README 전면 재작성은 이번 작업 범위에서 제외했습니다.

## 원인과 수정 범위

기존 `CreateDefender`, `ApplyKoreanWorldFont`, `EnsureMenuStyles`는 `Font.CreateDynamicFontFromOSFont`로 운영체제 폰트를 가져왔습니다. Unity Web은 사용자 PC의 설치 폰트에 접근하지 못합니다. 따라서 Windows Editor에서 표시되던 글자가 Web에서는 사라질 수 있는 구조였습니다. 이 플랫폼 제한은 [Unity 6.5 공식 문서](https://docs.unity.com/en-us/engine/6000.5/manual/platform-specific/webgl/develop/graphics/web-gl2)에 명시되어 있습니다. 기존 배포본의 브라우저 로그를 확보한 것은 아니므로, 그 배포본에서 일어난 모든 렌더링 문제의 단일 원인이라고 단정하지는 않습니다.

일부 런타임 TextMesh도 기본 폰트에 의존했습니다. 특히 나중에 생성되는 파괴 알림은 층 생성 때 수행하는 일괄 적용에서 빠질 수 있었습니다. 모든 TextMesh 생성 위치에 동일한 폰트를 지정하고, 기존 프리팹의 TextMesh 일괄 적용도 유지했습니다.

- 원본: `Assets/Fonts/NotoSansKR-VariableFont_wght.ttf` (파일·위치·GUID 유지)
- 빌드 참조: `Assets/OfficeRampage/Resources/OfficeFonts/GameFont.asset`
- 공통 로더: `Assets/OfficeRampage/Scripts/UI/OfficeGameFont.cs`
- `Resources.Load<OfficeGameFont>("OfficeFonts/GameFont")` → 원본 TTF 직접 참조. 폰트를 Resources로 이동하거나 복제하지 않습니다.
- TextMesh의 `font`와 `MeshRenderer.sharedMaterial`을 함께 설정합니다.
- 메뉴/버튼/HUD/유닛 설정/랭킹/이름 입력창은 `menuFont`를 지정하거나 그 스타일을 상속합니다. GUI.skin 전역 변경은 하지 않습니다.
- 기존 `OfficeNameEditor`의 TextField, 포커스, 한글 조합 확인 및 적용 처리는 변경하지 않았습니다.
- 공유 폰트 에셋을 `OnDestroy`에서 삭제하지 않습니다.
- 참조 누락·재질 로드 실패·한글/영문/숫자 샘플 글리프 누락은 `[OfficeFont]` 오류로 한 번 보고합니다. 실패 시 OS 폰트로 조용히 대체하지 않습니다.

## Unity 설정 확인

1. 원본 TTF Inspector에서 Characters를 **Dynamic**, **Include Font Data**를 활성화합니다. 원본 `.meta`에서 이미 동적 폰트와 `includeFontData: 1`이 설정되어 있어 이번에 변경하지 않았습니다.
2. `GameFont.asset`의 Font가 위 원본 TTF를 가리키는지 확인합니다. 새로 씬이나 프리팹의 Inspector를 연결할 필요는 없습니다.
3. 기존 Web Build Profile로 다시 빌드하고 HTTP(S)로 실행합니다. 예전 빌드를 덮어쓴 경우 브라우저 캐시를 비우거나 새 빌드 주소로 확인합니다.
4. Console 및 브라우저 개발자 도구에서 `[OfficeFont]`, 폰트 face/글리프/재질 오류를 확인합니다.

## 가변 폰트와 고정 굵기 TTF

제공 파일의 `fvar` 테이블은 `wght` 축 100~900, 기본값 100입니다. 가변 폰트라는 이유만으로 미표시 원인이라고 판단하거나 원본을 교체하지 않습니다. 기본 굵기가 얇은 문제와 폰트 자체가 전혀 표시되지 않는 문제는 구분해야 합니다.

대상 플랫폼에서 이 파일의 글리프 생성 실패가 재현되거나 굵기를 일정하게 고정해야 한다면, 별도 고정 굵기 TTF로 변환하고 `GameFont.asset`의 Font 참조만 교체하면 됩니다. 한 축의 특정 값을 확정하면 가변 축 해석이 필요 없는 정적 폰트가 됩니다. [fontTools 공식 instancer 문서](https://fonttools.readthedocs.io/en/latest/varLib/instancer.html)를 기준으로 프로젝트 루트에서 실행할 예시는 다음과 같습니다. 이번 수정에서 아래 설치·변환 명령을 실행한 것은 아닙니다.

```powershell
python -m pip install fonttools
fonttools varLib.instancer "Assets/Fonts/NotoSansKR-VariableFont_wght.ttf" wght=400 --output="Assets/Fonts/NotoSansKR-Regular.ttf"
```

생성된 파일도 Dynamic/Include Font Data를 확인하고 Editor와 Web에서 다시 검사하세요. 원본 TTF 및 기존 `.meta`는 삭제하지 않습니다.

## 직접 확인할 항목

- 첫 메뉴·설정·버튼·설명에서 한글/영문/숫자 표시
- 유닛 이름 편집에서 실제 한글 키보드 조합, 조합 중 취소/확정, 저장 후 재시작
- 적 이름·체력 숫자, 층/문/엘리베이터 표시, 파괴 후 새로 생기는 텍스트
- 무한 모드 상단 바·장비명·스킬 숫자·랭킹·결과 화면
- 보통/굵게 스타일, 새 글자 등장 후에도 기존 이름표가 유지되는지
- 브라우저별 실제 IME 동작: 폰트 표시 수정은 브라우저의 한글 입력 지원을 추가하는 작업과 다릅니다. Editor 한글 표시만으로 Web의 실제 조합 입력까지 검증했다고 보지 않습니다.

## 실제 검증 결과


- Editor: 원본 가변 TTF Dynamic 임포트, Include Font Data, 일반/굵게 한글·영문·숫자 글리프 생성 통과. 메뉴·설정·이름 입력·스토리·무한 모드 실행 화면 캡처, 메뉴와 입력창 표시 확인.
- 활성/비활성 TextMesh 전체 Font와 sharedMaterial 참조 일치, 이동·점프·바닥·벽 충돌 통과. 모든 거리의 작은 이름표 가독성까지 검증한 것은 아닙니다.
- 한글 문자열의 입력창 적용/반영 통과. 실제 키보드 IME 조합 입력은 별도 수동 확인이 필요합니다. 입력 코드는 유지했습니다.
- TTF 문자표에 없는 연필 기호 U+270E는 [편집]으로 교체했습니다. 교체 후 코드 내 비ASCII 문자 누락은 없습니다.
- 격리 프로젝트 실제 Web 개발 빌드 성공, 오류 0개. 검증용 압축 설정만 Disabled였으며 원본 프로젝트 설정은 변경하지 않았습니다.
- 브라우저: 2026-11-23 확인을 시도했으나 Computer Use가 현재 브라우저 URL의 접근 제한으로 중단되었습니다. 실제 Web 표시·런타임 오류 유무·키보드 한글 조합 입력은 미검증입니다. 빌드 성공을 화면 표시 성공으로 간주하지 않습니다.
- 가변 폰트의 미표시 문제는 Editor에서 재현되지 않았습니다. 기본 굵기 100은 얇게 보일 수 있으나 고정 TTF로 강제 교체하지 않았습니다.


## 수정 파일 목록

- `Assets/OfficeRampage/Scripts/Core/OfficeRampageGame.cs`
- `Assets/OfficeRampage/Scripts/Endless/Pickups/OfficeRampageGame.EndlessLoot.cs`
- `Assets/OfficeRampage/Scripts/Enemies/OfficeRampageGame.Defenders.cs`
- `Assets/OfficeRampage/Scripts/Story/OfficeRampageGame.Ending.cs`
- `Assets/OfficeRampage/Scripts/Story/OfficeRampageGame.Intro.cs`
- `Assets/OfficeRampage/Scripts/UI/OfficeGameFont.cs`
- `Assets/OfficeRampage/Scripts/UI/OfficeRampageGame.Company.cs`
- `Assets/OfficeRampage/Scripts/UI/OfficeRampageGame.Menu.cs`
- `Assets/OfficeRampage/Scripts/World/OfficeRampageGame.Effects.cs`
- `Assets/OfficeRampage/Scripts/World/OfficeRampageGame.Elevator.cs`
- `Assets/OfficeRampage/Scripts/World/OfficeRampageGame.FloorIdentity.cs`
- `Assets/OfficeRampage/Resources/OfficeFonts/GameFont.asset` 및 신규 자산의 `.meta`
- `Docs/WEB_FONT_FIX.md`
- `OfficeRampage_Update.md`

원본 TTF와 기존 GUID, 씬·프리팹, 프로젝트 설정, README는 변경하지 않았습니다. 테스트 코드는 실제 프로젝트에 복사하지 않습니다.
