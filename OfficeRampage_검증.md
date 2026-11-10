# Office Rampage 검사 기록

## 실제 실행한 검사

- `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, 입력 설정을 확인했습니다.
  Unity 6000.5.1f1, URP 17.5.0, Input System 1.19.0, activeInputHandler 1을 유지합니다.
- Unity 설치에 포함된 Roslyn C# 컴파일러로 새 런타임 코드 2개와 Editor 코드 1개를
  Unity 6.5의 실제 모듈 및 프로젝트의 Unity.InputSystem.dll에 연결해 컴파일했습니다.
  종료 코드 0, C# 오류 및 경고 없음. 출력 DLL은 OS 임시 폴더에만 생성했습니다.
  이 검사는 Unity Editor 내부의 에셋 임포트/Play 테스트와는 별개입니다.
- 별도 `Assets/OfficeRampage/OfficeRampage.unity`와 생성기 스크립트의 GUID 참조를 검사했습니다.
- 방 크기의 모든 조합과 배치 흔들림의 최댓값을 기준으로 중앙 통로, 벽 간격,
  입구/출구 여유 공간, 서로 다른 가구 배치 셀의 겹침 여부를 검사했습니다.
- 기존 SampleScene, 기존 .meta/GUID, Packages, ProjectSettings를 편집하지 않았습니다.
  Library, Temp, Obj, Logs의 생성 파일도 직접 편집하지 않았습니다.

## 아직 확인하지 못한 검사

Unity Hub를 통한 Play 검증을 시도했으나 컴퓨터 제어 도구에서
`Computer Use was not approved to use Unity Hub`가 반환되어 진행하지 못했습니다.

다음 항목은 실제 Unity Play 모드에서 확인이 필요합니다.

1. Unity 에셋 임포트 완료 및 Console 컴파일 오류 여부
2. WASD 이동 / 마우스 시점
3. Space 점프 / 착지
4. 바닥, 벽, 책상 충돌 및 열린 문 통과
5. 무기별 타격감, 파괴 이펙트, 콤보 시간 보너스
6. 사무실 5개 진행과 보스전, 승리/패배, 재시작
7. 화면 해상도별 UI와 실제 조명/재질 렌더링

Play 검사가 통과했다고 보고하지 않습니다. 상세 조작과 실행 순서는
`Assets/OfficeRampage/README_KO.md`에 있습니다.
