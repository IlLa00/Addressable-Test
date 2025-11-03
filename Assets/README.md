# 🎮 Addressables 학습 프로젝트

Unity Addressables를 처음 사용하는 개발자를 위한 실습 프로젝트입니다.

---

## 📁 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── AddressableTest01_BasicLoad.cs      ← 1단계: 스프라이트 로드
│   ├── AddressableTest02_PrefabLoad.cs     ← 2단계: 프리팹 생성/삭제
│   ├── AddressableTest03_ProgressBar.cs    ← 3단계: 다운로드 진행률
│   └── AddressablesHelper.cs               ← 실전용 유틸리티 클래스
│
├── TestAssets/                             ← 테스트용 에셋 폴더
│   ├── TestSprite.png                      (준비 필요)
│   └── TestCube.prefab                     (준비 필요)
│
├── Scenes/                                 ← 테스트 씬들
│   ├── Test01_BasicLoad.unity              (생성 필요)
│   ├── Test02_PrefabLoad.unity             (생성 필요)
│   └── Test03_ProgressBar.unity            (생성 필요)
│
├── SETUP_GUIDE.md                          ← 📚 상세 설정 가이드
├── CHEATSHEET.md                           ← 🚀 빠른 참고 치트시트
└── README.md                               ← 이 파일
```

---

## 🚀 빠른 시작

### 1단계: Addressables 초기화
1. Unity 메뉴: `Window > Asset Management > Addressables > Groups`
2. `Create Addressables Settings` 클릭

### 2단계: 테스트 에셋 준비
1. `Assets/TestAssets/` 폴더에 이미지 추가 (PNG/JPG)
2. Inspector에서 `Addressable` 체크박스 체크
3. Address 이름: `TestSprite` 입력

### 3단계: 테스트 씬 구성
1. 새 씬 생성: `Test01_BasicLoad.unity`
2. Canvas > Image, Button, Text 추가
3. `AddressableTest01_BasicLoad.cs` 스크립트 연결
4. Inspector에서 UI 연결

### 4단계: 실행!
1. Groups 창에서 `Build > New Build > Default Build Script` (필수!)
2. Play 버튼 클릭
3. Load 버튼 클릭 → 이미지 로드 확인 ✅

---

## 📚 학습 자료

### 필수 읽기
- **[SETUP_GUIDE.md](SETUP_GUIDE.md)** - 처음부터 끝까지 상세 설명
- **[CHEATSHEET.md](CHEATSHEET.md)** - 빠른 API 참고용

### 스크립트 학습 순서
1. `AddressableTest01_BasicLoad.cs` - 가장 기본적인 로드/해제
2. `AddressableTest02_PrefabLoad.cs` - 오브젝트 생성 및 관리
3. `AddressableTest03_ProgressBar.cs` - 다운로드 및 진행률
4. `AddressablesHelper.cs` - 실전 패턴 모음

---

## 🎯 학습 목표

### 초급 (1~2일)
- [x] Addressables 개념 이해
- [x] 에셋을 Addressable로 등록
- [x] LoadAssetAsync로 에셋 로드
- [x] Release로 메모리 해제

### 중급 (3~4일)
- [x] InstantiateAsync로 프리팹 생성
- [x] 진행률 표시 (PercentComplete)
- [x] 다운로드 크기 확인 (GetDownloadSizeAsync)
- [x] Groups 구성 및 빌드

### 고급 (5~7일)
- [ ] Remote 배포 설정
- [ ] Label 활용
- [ ] 카탈로그 업데이트
- [ ] 실제 프로젝트 적용

---

## 💡 핵심 개념

### Addressables가 필요한 이유
```
기존 (Resources.Load):
❌ 모든 리소스가 빌드에 포함 → 앱 크기 ↑
❌ 업데이트 불가 (앱 재배포 필요)
❌ 메모리 관리 어려움

Addressables:
✅ 필요한 것만 다운로드 → 앱 크기 ↓
✅ 원격 업데이트 가능 (앱 재배포 불필요)
✅ 자동 의존성 관리 + 메모리 최적화
```

### 주소(Address) 개념
```
기존: "Assets/Resources/Sprites/Player.png"  ← 경로 의존
Addressables: "Player"                        ← 주소만 알면 됨

장점:
- 파일 위치 변경해도 주소만 유지하면 OK
- 그룹/라벨로 유연한 관리
- 로컬/원격 전환 간편
```

### 메모리 관리 철칙
```csharp
var handle = Addressables.LoadAssetAsync<T>(address);
T asset = await handle.Task;

// ... 사용 ...

// ⚠️ 반드시 해제!
Addressables.Release(handle);
```

---

## 🐛 문제 해결

### "InvalidKeyException" 발생 시
```
1. Groups 창에서 에셋이 등록되어 있는지 확인
2. Address 이름이 코드와 일치하는지 확인
3. Addressables 빌드 실행했는지 확인
```

### 에디터에서는 되는데 빌드하면 안 될 때
```
Groups > Build > New Build 먼저 실행!
```

### 메모리 누수 의심 시
```
Event Viewer 확인:
Window > Asset Management > Addressables > Event Viewer
```

---

## 📊 실전 적용 예시

### 게임 모드별 분리
```
Groups:
├── Common (Local)          ← 공통 UI, 사운드
├── Roguelike (Remote)      ← 로그라이크 모드
├── Shooting (Remote)       ← 슈팅 모드
└── DeckStrategy (Remote)   ← 덱 스트래티지 모드
```

### 패치 시스템
```csharp
// 게임 모드 진입 시 다운로드 체크
bool isReady = await AddressablesHelper.Instance.IsDownloadedAsync("Roguelike");

if (!isReady)
{
    // 다운로드 팝업 표시
    ShowDownloadPopup("Roguelike", downloadSize);
}
```

---

## 🎓 다음 단계

이 프로젝트를 마스터했다면:

1. **실제 프로젝트에 적용**
   - ResourceManager에 Addressables 통합
   - 기존 Resources.Load → Addressables 마이그레이션

2. **고급 기능 학습**
   - Sprite Atlas + Addressables
   - ScriptableObject 동적 로드
   - Scene 동적 로드

3. **배포 파이프라인 구축**
   - GitHub Releases 연동
   - 자동 빌드 스크립트
   - 버전 관리 시스템

---

## 📞 도움이 필요하신가요?

- Unity 공식 문서: https://docs.unity3d.com/Packages/com.unity.addressables@latest
- Unity Forum: https://forum.unity.com/forums/addressables.156/
- GitHub Issues: (프로젝트 이슈 트래커)

---

## ✅ 최종 체크리스트

학습 완료 후 체크하세요:

- [ ] Addressables Settings 생성 완료
- [ ] 에셋을 Addressable로 등록하는 방법 이해
- [ ] LoadAssetAsync + Release 코드 작성 가능
- [ ] InstantiateAsync로 오브젝트 생성 가능
- [ ] 진행률 UI 구현 가능
- [ ] Groups 빌드 실행 방법 숙지
- [ ] 메모리 관리 (Release) 중요성 이해
- [ ] 실제 프로젝트 적용 계획 수립

---

**축하합니다! Addressables 마스터의 첫 걸음을 시작했습니다! 🎉**

Happy Coding! 🚀
