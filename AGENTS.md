# Unity 프로젝트 작업 지침

- 초보자에게 한국어로 변경 내용과 Unity에서 실행할 순서를 설명한다.
- 먼저 ProjectSettings/ProjectVersion.txt와 Packages/manifest.json을 읽는다.
- 기존 Unity 버전, 렌더 파이프라인, 입력 시스템을 유지한다.
- C# 런타임 코드는 Assets 아래, UnityEditor 코드는 Editor 폴더 아래에 둔다.
- .meta 파일과 GUID를 보존한다. Library, Temp, Obj, Logs 생성 파일은 직접 편집하지 않는다.
- 기존 씬을 덮어쓰지 않고 연습용 기능은 별도 씬으로 만든다.
- 검증은 컴파일 오류, Play 모드 이동, 점프, 바닥/장애물 충돌 순서로 진행한다.
- 실제 실행한 검사와 아직 확인하지 못한 검사를 명확히 구분한다.
- OfficeRampage 업데이트 후에는 프로젝트 루트의 OfficeRampage_Update.md에 날짜, 변경 내용, Unity 실행/확인 순서, 실제 검사 결과와 미확인 항목을 한국어로 추가한다. 기존 기록은 보존하고 새 기록을 파일 끝에 누적한다.
