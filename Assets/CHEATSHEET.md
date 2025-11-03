# 🚀 Addressables 치트시트 (빠른 참고용)

## 📌 핵심 API 한눈에 보기

### 1. 에셋 로드

```csharp
// 기본 로드
var handle = Addressables.LoadAssetAsync<Sprite>("TestSprite");
Sprite sprite = await handle.Task;

// 로드 + 콜백
Addressables.LoadAssetAsync<AudioClip>("BGM").Completed += (handle) => {
    AudioClip clip = handle.Result;
    audioSource.clip = clip;
};
```

### 2. 프리팹 인스턴스 생성

```csharp
// 방법 1: InstantiateAsync (권장)
var handle = Addressables.InstantiateAsync("Player");
GameObject player = await handle.Task;

// 방법 2: LoadAssetAsync + Instantiate
var handle = Addressables.LoadAssetAsync<GameObject>("Player");
GameObject prefab = await handle.Task;
GameObject instance = Instantiate(prefab);
```

### 3. 다운로드 관련

```csharp
// 다운로드 크기 확인
var sizeHandle = Addressables.GetDownloadSizeAsync("TestSprite");
long bytes = await sizeHandle.Task;
Addressables.Release(sizeHandle);

// 다운로드 실행 (진행률 포함)
var downloadHandle = Addressables.DownloadDependenciesAsync("TestSprite");
while (!downloadHandle.IsDone)
{
    float progress = downloadHandle.PercentComplete; // 0.0 ~ 1.0
    Debug.Log($"다운로드: {progress * 100}%");
    await Task.Yield();
}
Addressables.Release(downloadHandle);
```

### 4. 메모리 해제 (필수!)

```csharp
// 에셋 해제
Addressables.Release(handle);

// 인스턴스 해제 (InstantiateAsync로 생성한 경우)
Addressables.ReleaseInstance(handle);

// 또는 오브젝트로 해제
Addressables.ReleaseInstance(gameObject);
```

### 5. 카탈로그 업데이트

```csharp
// 원격 카탈로그 업데이트 확인
var checkHandle = Addressables.CheckForCatalogUpdates();
List<string> catalogs = await checkHandle.Task;
Addressables.Release(checkHandle);

if (catalogs.Count > 0)
{
    // 카탈로그 업데이트
    var updateHandle = Addressables.UpdateCatalogs(catalogs);
    await updateHandle.Task;
    Addressables.Release(updateHandle);
}
```

### 6. Label로 여러 에셋 로드

```csharp
// "Character" 라벨이 붙은 모든 에셋 로드
var handle = Addressables.LoadAssetsAsync<GameObject>("Character", (obj) => {
    Debug.Log($"로드됨: {obj.name}");
});
IList<GameObject> characters = await handle.Task;
Addressables.Release(handle);
```

---

## 🎯 자주 사용하는 패턴

### 패턴 1: 안전한 로드 (에러 처리)

```csharp
public async Task<T> SafeLoad<T>(string address) where T : Object
{
    try
    {
        var handle = Addressables.LoadAssetAsync<T>(address);
        T asset = await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
            return asset;
        else
            return null;
    }
    catch (Exception ex)
    {
        Debug.LogError($"로드 실패: {address}\n{ex}");
        return null;
    }
}
```

### 패턴 2: 캐싱 시스템

```csharp
private Dictionary<string, AsyncOperationHandle> _cache = new();

public async Task<T> LoadWithCache<T>(string address) where T : Object
{
    if (_cache.ContainsKey(address))
        return _cache[address].Convert<T>().Result;

    var handle = Addressables.LoadAssetAsync<T>(address);
    T asset = await handle.Task;
    _cache[address] = handle;
    return asset;
}

public void ReleaseCache(string address)
{
    if (_cache.ContainsKey(address))
    {
        Addressables.Release(_cache[address]);
        _cache.Remove(address);
    }
}
```

### 패턴 3: 프리로드 (미리 다운로드)

```csharp
public async Task PreloadAssets(string[] addresses)
{
    foreach (var address in addresses)
    {
        var handle = Addressables.DownloadDependenciesAsync(address);
        await handle.Task;
        Addressables.Release(handle);
    }
    Debug.Log("프리로드 완료!");
}
```

### 패턴 4: 진행률 UI 업데이트

```csharp
public async Task LoadWithProgress(string address, Slider progressBar)
{
    var handle = Addressables.LoadAssetAsync<Sprite>(address);

    while (!handle.IsDone)
    {
        progressBar.value = handle.PercentComplete;
        await Task.Yield();
    }

    progressBar.value = 1f;
    Sprite result = handle.Result;
}
```

---

## ⚠️ 주의사항 체크리스트

- [ ] **모든 핸들은 반드시 Release!**
  ```csharp
  ✗ 나쁜 예:
  var handle = Addressables.LoadAssetAsync<Sprite>("Test");
  Sprite s = await handle.Task;
  // Release 안 함 → 메모리 누수!

  ✓ 좋은 예:
  var handle = Addressables.LoadAssetAsync<Sprite>("Test");
  Sprite s = await handle.Task;
  // ... 사용 ...
  Addressables.Release(handle); // 반드시 해제
  ```

- [ ] **InstantiateAsync는 ReleaseInstance 사용**
  ```csharp
  var handle = Addressables.InstantiateAsync("Cube");
  GameObject cube = await handle.Task;

  // ✗ Destroy(cube);              // 나쁜 예
  // ✓ Addressables.ReleaseInstance(handle);  // 좋은 예
  ```

- [ ] **Addressables 빌드 후 Unity 빌드**
  ```
  1. Groups > Build > New Build > Default Build Script
  2. File > Build Settings > Build
  ```

- [ ] **Address 중복 체크**
  - 같은 Address를 여러 에셋에 사용하면 충돌!

- [ ] **에디터 플레이 모드 설정**
  - Play Mode Script: `Use Asset Database (fastest)` (개발 중)
  - 빌드 테스트 시: `Use Existing Build` (실제와 동일)

---

## 🔧 Groups 창 주요 기능

| 메뉴 | 설명 |
|------|------|
| **Build > New Build** | Addressables 빌드 (필수!) |
| **Build > Clean Build** | 기존 빌드 삭제 후 재빌드 |
| **Tools > Analyze** | 중복/누락 에셋 분석 |
| **Profile** | Local/Remote 설정 전환 |
| **Create > Group** | 새 그룹 생성 |

---

## 📊 Build Path vs Load Path

| | Local | Remote |
|---|-------|--------|
| **Build Path** | `[BuildPath]/[BuildTarget]` | `ServerData/[BuildTarget]` |
| **Load Path** | `{UnityEngine.Application...}` | `https://your-cdn.com/[BuildTarget]` |
| **용도** | 앱에 포함 | 다운로드 |

---

## 🐛 자주 발생하는 에러

### InvalidKeyException
```
원인: Address가 잘못되었거나 에셋이 Addressable로 등록 안 됨
해결: Groups 창에서 에셋 등록 확인
```

### Cannot read ContentCatalog
```
원인: Addressables 빌드를 안 함
해결: Groups > Build > New Build 실행
```

### Exception: Provider operation failed
```
원인: 다운로드 실패 또는 네트워크 오류
해결: Load Path URL 확인
```

---

## 💡 디버깅 팁

```csharp
// 1. 이벤트 뷰어 (Unity 2021+)
// Window > Asset Management > Addressables > Event Viewer
// 실시간 로드/해제 모니터링

// 2. 핸들 유효성 체크
if (handle.IsValid())
    Debug.Log("핸들 유효");

// 3. 로드 상태 확인
Debug.Log($"Status: {handle.Status}");
Debug.Log($"IsDone: {handle.IsDone}");
Debug.Log($"Progress: {handle.PercentComplete}");

// 4. 의존성 확인
var deps = Addressables.GetDownloadSizeAsync(address);
Debug.Log($"의존성 크기: {deps} bytes");
```

---

## 🚀 성능 최적화 팁

1. **그룹 분리**: 게임 모드별/씬별로 그룹 나누기
2. **Label 활용**: 동시에 로드할 에셋은 같은 Label
3. **Preload**: 로딩 화면에서 미리 다운로드
4. **Atlas 사용**: 여러 스프라이트는 Sprite Atlas로 묶기
5. **Bundle 크기 제한**: 그룹당 10MB 이하 권장

---

## 📚 핵심만 외우기

```csharp
// 로드
var handle = Addressables.LoadAssetAsync<T>(address);
T result = await handle.Task;

// 생성
var handle = Addressables.InstantiateAsync(address);
GameObject obj = await handle.Task;

// 해제
Addressables.Release(handle);
Addressables.ReleaseInstance(handle);

// 다운로드
var size = await Addressables.GetDownloadSizeAsync(address).Task;
var download = Addressables.DownloadDependenciesAsync(address);
```

**이 4가지만 알아도 80% 커버됩니다!**

---

## 🎓 학습 로드맵

```
Day 1: LoadAssetAsync + Release 마스터
Day 2: InstantiateAsync + ReleaseInstance
Day 3: GetDownloadSizeAsync + DownloadDependenciesAsync
Day 4: Groups 설정 + Build
Day 5: Remote 배포 테스트
```

---

**이 치트시트를 프린트해서 모니터 옆에 붙여두세요! 📌**
