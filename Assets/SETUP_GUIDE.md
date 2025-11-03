# 🚀 Addressables 테스트 프로젝트 설정 가이드

이 가이드는 Unity Addressables를 처음 사용하는 분들을 위한 단계별 튜토리얼입니다.

---

## 📋 목차

1. [Addressables 초기 설정](#1-addressables-초기-설정)
2. [에셋 Addressable로 만들기](#2-에셋-addressable로-만들기)
3. [테스트 씬 구성](#3-테스트-씬-구성)
4. [빌드 및 테스트](#4-빌드-및-테스트)
5. [원격 배포 설정 (심화)](#5-원격-배포-설정-심화)

---

## 1. Addressables 초기 설정

### 1-1. Addressables 시스템 초기화

1. Unity 상단 메뉴: **Window > Asset Management > Addressables > Groups**
2. 처음 열면 "Create Addressables Settings" 버튼이 보입니다
3. **Create Addressables Settings** 클릭

✅ 이렇게 하면 다음 파일들이 자동 생성됩니다:
- `Assets/AddressableAssetsData/` 폴더
- `AddressableAssetSettings.asset` (설정 파일)
- `DefaultLocalGroup` (기본 그룹)

### 1-2. 생성된 구조 확인

```
Assets/
├── AddressableAssetsData/
│   ├── AddressableAssetSettings.asset
│   ├── AssetGroups/
│   │   └── Default Local Group.asset
│   └── DataBuilders/
└── ...
```

---

## 2. 에셋 Addressable로 만들기

### 2-1. 테스트용 에셋 준비

#### 방법 A: 스프라이트 테스트용
1. `Assets/TestAssets/` 폴더 생성
2. 아무 이미지 파일을 드래그 앤 드롭 (PNG, JPG 등)
3. Import Settings에서 **Texture Type: Sprite (2D and UI)** 설정

#### 방법 B: 프리팹 테스트용
1. Hierarchy에서 **우클릭 > 3D Object > Cube**
2. Cube를 원하는 색상/크기로 설정
3. `Assets/TestAssets/` 폴더로 드래그하여 Prefab 생성

### 2-2. Addressable 등록 (중요!)

#### 방법 1: Inspector에서 등록 (간단)
1. Project 창에서 에셋 선택 (예: TestSprite.png)
2. Inspector 창 상단에 **"Addressable"** 체크박스가 보임
3. ✅ **Addressable 체크박스 체크**
4. Address 입력창에 주소 입력 (예: `TestSprite`)

![Addressable 체크박스 위치]
```
Inspector
┌─────────────────────────┐
│ TestSprite              │
│ ☑ Addressable          │ ← 이 체크박스!
│   Address: TestSprite   │ ← 여기에 주소 입력
└─────────────────────────┘
```

#### 방법 2: Groups 창에서 등록 (권장)
1. **Window > Asset Management > Addressables > Groups** 열기
2. Project 창에서 에셋을 **Groups 창으로 드래그 앤 드롭**
3. `Default Local Group`에 추가됨
4. Address 이름 더블클릭하여 수정 가능

### 2-3. Address 이름 규칙

```
좋은 예:
✓ "TestSprite"              ← 간단명료
✓ "UI/MainMenu/Logo"        ← 계층 구조
✓ "Characters/Player/Idle"  ← 카테고리 분류

나쁜 예:
✗ "Assets/TestAssets/TestSprite.png"  ← 경로 그대로 (비추)
✗ "testsprite123_final_v2"            ← 일관성 없음
```

---

## 3. 테스트 씬 구성

### 3-1. Test01 씬 (스프라이트 로드)

1. **새 씬 생성**: Scenes/Test01_BasicLoad.unity
2. **UI 생성**:
   ```
   Canvas
   ├── Image (로드한 스프라이트 표시용)
   │   └─ Name: TargetImage
   ├── Button (로드 버튼)
   │   └─ Name: LoadButton
   └── Text (상태 표시)
       └─ Name: StatusText
   ```

3. **빈 오브젝트 생성**: Hierarchy > Create Empty > 이름: `TestManager`
4. **스크립트 연결**:
   - `AddressableTest01_BasicLoad.cs` 를 `TestManager`에 추가
   - Inspector에서 필드 연결:
     - Target Image → Canvas/Image
     - Load Button → Canvas/Button
     - Status Text → Canvas/Text
     - Sprite Address → `"TestSprite"` (Addressable로 등록한 이름)

### 3-2. Test02 씬 (프리팹 생성)

1. **새 씬 생성**: Scenes/Test02_PrefabLoad.unity
2. **UI 생성**:
   ```
   Canvas
   ├── Button (생성 버튼)
   │   └─ Name: LoadButton
   ├── Button (삭제 버튼)
   │   └─ Name: ClearButton
   ├── Text (상태)
   │   └─ Name: StatusText
   └── Text (개수)
       └─ Name: CountText
   ```

3. **Spawn Parent 생성**:
   - Hierarchy > Create Empty > 이름: `SpawnParent`
   - Position: (0, 0, 0)

4. **스크립트 연결**:
   - `AddressableTest02_PrefabLoad.cs` 를 빈 오브젝트에 추가
   - Inspector에서 필드 연결
   - Prefab Address → `"TestCube"` (Prefab의 Address)
   - Spawn Parent → SpawnParent 오브젝트 연결

### 3-3. Test03 씬 (진행률 표시)

1. **새 씬 생성**: Scenes/Test03_ProgressBar.unity
2. **UI 생성**:
   ```
   Canvas
   ├── Slider (진행률 바)
   │   └─ Name: ProgressBar
   ├── Text (진행률 텍스트)
   │   └─ Name: ProgressText
   ├── Text (크기 표시)
   │   └─ Name: SizeText
   ├── Button (다운로드 버튼)
   │   └─ Name: DownloadButton
   └── Image (미리보기)
       └─ Name: PreviewImage
   ```

3. **스크립트 연결**:
   - `AddressableTest03_ProgressBar.cs` 추가
   - Inspector에서 UI 연결

---

## 4. 빌드 및 테스트

### 4-1. Addressables 빌드

**중요**: Addressables 에셋은 일반 Unity 빌드와 별도로 빌드해야 합니다!

1. **Groups 창 열기**: Window > Asset Management > Addressables > Groups
2. **Build 메뉴 클릭**: 창 상단의 **Build > New Build > Default Build Script**
3. 빌드 완료 메시지 확인

✅ 빌드 결과물 위치:
```
Library/
└── com.unity.addressables/
    └── aa/
        └── Windows/  (또는 Android, iOS 등)
            ├── defaultlocalgroup_assets_all.bundle
            └── catalog.json
```

### 4-2. 에디터에서 테스트

1. Test01 씬 열기
2. **Play 버튼** 클릭
3. **Load 버튼** 클릭
4. 이미지가 표시되면 성공! ✅

### 4-3. 빌드 테스트 (실제 앱)

1. **Addressables 빌드**: Groups > Build > New Build > Default Build Script
2. **Unity 빌드**: File > Build Settings > Build
3. 생성된 .exe 실행하여 테스트

---

## 5. 원격 배포 설정 (심화)

### 5-1. Profile 설정

1. **Groups 창**: Window > Asset Management > Addressables > Groups
2. 상단 **Profile: Default** 클릭 > **Manage Profiles**
3. 새 Profile 생성: **Create > Profile** > 이름: `Remote`

### 5-2. Remote Build Path 설정

1. `Remote` Profile 선택
2. **BuildPath** 수정:
   ```
   기본값: [UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]

   변경:
   ServerData/[BuildTarget]
   ```

3. **LoadPath** 수정 (GitHub 예시):
   ```
   https://github.com/USER/REPO/releases/download/v1.0.0/[BuildTarget]
   ```

### 5-3. Group을 Remote로 설정

1. Groups 창에서 그룹 선택 (예: Default Local Group)
2. Inspector에서:
   - **Build Path**: RemoteBuildPath
   - **Load Path**: RemoteLoadPath
3. 적용

### 5-4. 원격 빌드 및 배포

1. Addressables 빌드: **Build > New Build > Default Build Script**
2. 생성된 파일 확인:
   ```
   ServerData/
   └── Windows/
       ├── defaultlocalgroup_assets_all.bundle
       └── catalog.json
   ```
3. 이 파일들을 GitHub Releases에 업로드
4. 앱 실행 시 자동 다운로드 됨!

---

## 🎓 학습 순서 권장

```
1주차: 기본 로드
  ├─ Test01 실습 (스프라이트 로드)
  ├─ Address 등록 연습
  └─ Inspector 확인

2주차: 프리팹 관리
  ├─ Test02 실습 (프리팹 생성/삭제)
  ├─ InstantiateAsync 이해
  └─ 메모리 관리 (Release)

3주차: 진행률 표시
  ├─ Test03 실습 (다운로드 진행률)
  ├─ GetDownloadSizeAsync
  └─ PercentComplete

4주차: 실전 적용
  ├─ 게임 모드별 그룹 분리
  ├─ Label 활용
  └─ 원격 배포 테스트
```

---

## 🐛 자주 발생하는 문제

### Q1: "InvalidKeyException: Exception of type 'UnityEngine.AddressableAssets.InvalidKeyException' was thrown."
**원인**: Address 이름이 잘못되었거나 에셋이 Addressable로 등록 안 됨
**해결**: Groups 창에서 에셋이 등록되어 있는지 확인

### Q2: 에디터에서는 되는데 빌드하면 안 됨
**원인**: Addressables 빌드를 안 함
**해결**: Groups > Build > New Build 먼저 실행

### Q3: 이미지가 안 보임 (Missing Sprite)
**원인**: Release를 너무 빨리 호출함
**해결**: 핸들을 멤버 변수로 저장하고 OnDestroy에서 해제

### Q4: 메모리 누수
**원인**: Addressables.Release() 호출 안 함
**해결**: 로드한 모든 핸들은 반드시 Release!

---

## 📚 추가 학습 자료

- [Unity 공식 문서](https://docs.unity3d.com/Packages/com.unity.addressables@latest)
- [Addressables 샘플 프로젝트](https://github.com/Unity-Technologies/Addressables-Sample)

---

## ✅ 체크리스트

- [ ] Addressables Settings 생성 완료
- [ ] 테스트 에셋 Addressable로 등록
- [ ] Test01 씬 구성 및 테스트
- [ ] Test02 씬 구성 및 테스트
- [ ] Test03 씬 구성 및 테스트
- [ ] Addressables 빌드 실행
- [ ] Release 메모리 관리 이해

---

**축하합니다! 이제 Addressables 기초를 마스터했습니다! 🎉**
