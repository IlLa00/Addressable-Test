using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using System.Collections.Generic;

/// <summary>
/// Addressables 실전 헬퍼 클래스
///
/// 실제 프로젝트에서 바로 사용 가능한 유틸리티 모음
/// - 간편한 로드/해제
/// - 에러 처리
/// - 메모리 관리
/// </summary>
public class AddressablesHelper : MonoBehaviour
{
    // 싱글톤 (간단한 구현)
    private static AddressablesHelper _instance;
    public static AddressablesHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("AddressablesHelper");
                _instance = obj.AddComponent<AddressablesHelper>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    // 로드된 핸들 추적 (자동 메모리 관리)
    private Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

    /// <summary>
    /// 에셋 로드 (간편 버전)
    /// </summary>
    /// <typeparam name="T">에셋 타입 (Sprite, GameObject, AudioClip 등)</typeparam>
    /// <param name="address">Addressable 주소</param>
    /// <returns>로드된 에셋 (실패 시 null)</returns>
    public async Task<T> LoadAssetAsync<T>(string address) where T : Object
    {
        // 이미 로드된 경우 재사용
        if (_handles.ContainsKey(address))
        {
            var existingHandle = _handles[address].Convert<T>();
            if (existingHandle.IsValid() && existingHandle.IsDone)
            {
                Debug.Log($"[Addressables] 캐시에서 반환: {address}");
                return existingHandle.Result;
            }
        }

        try
        {
            Debug.Log($"[Addressables] 로딩 시작: {address}");

            var handle = Addressables.LoadAssetAsync<T>(address);
            T result = await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _handles[address] = handle;
                Debug.Log($"[Addressables] 로딩 성공: {address}");
                return result;
            }
            else
            {
                Debug.LogError($"[Addressables] 로딩 실패: {address}");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Addressables] 예외 발생: {address}\n{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 프리팹 인스턴스 생성 (간편 버전)
    /// </summary>
    public async Task<GameObject> InstantiateAsync(string address, Transform parent = null)
    {
        try
        {
            Debug.Log($"[Addressables] 인스턴스 생성: {address}");

            var handle = Addressables.InstantiateAsync(address, parent);
            GameObject instance = await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // 인스턴스별 고유 키로 저장
                string instanceKey = $"{address}_{instance.GetInstanceID()}";
                _handles[instanceKey] = handle;

                Debug.Log($"[Addressables] 생성 성공: {instance.name}");
                return instance;
            }
            else
            {
                Debug.LogError($"[Addressables] 생성 실패: {address}");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Addressables] 생성 예외: {address}\n{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 에셋 해제 (메모리 관리)
    /// </summary>
    public void ReleaseAsset(string address)
    {
        if (_handles.ContainsKey(address))
        {
            Addressables.Release(_handles[address]);
            _handles.Remove(address);
            Debug.Log($"[Addressables] 해제: {address}");
        }
    }

    /// <summary>
    /// 인스턴스 해제 및 파괴
    /// </summary>
    public void ReleaseInstance(GameObject instance)
    {
        if (instance == null) return;

        string instanceKey = FindInstanceKey(instance);
        if (!string.IsNullOrEmpty(instanceKey) && _handles.ContainsKey(instanceKey))
        {
            Addressables.ReleaseInstance(_handles[instanceKey]);
            _handles.Remove(instanceKey);
            Debug.Log($"[Addressables] 인스턴스 해제: {instance.name}");
        }
        else
        {
            // 일반 Destroy 폴백
            Destroy(instance);
        }
    }

    /// <summary>
    /// 모든 핸들 해제 (씬 전환 시 호출)
    /// </summary>
    public void ReleaseAll()
    {
        Debug.Log($"[Addressables] 전체 해제: {_handles.Count}개");

        foreach (var handle in _handles.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        _handles.Clear();
    }

    /// <summary>
    /// 다운로드 크기 확인
    /// </summary>
    public async Task<long> GetDownloadSizeAsync(string address)
    {
        try
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(address);
            long size = await sizeHandle.Task;
            Addressables.Release(sizeHandle);

            Debug.Log($"[Addressables] 다운로드 크기: {address} = {size} bytes");
            return size;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Addressables] 크기 확인 실패: {ex.Message}");
            return -1;
        }
    }

    /// <summary>
    /// 에셋이 다운로드되어 있는지 확인
    /// </summary>
    public async Task<bool> IsDownloadedAsync(string address)
    {
        long size = await GetDownloadSizeAsync(address);
        return size == 0; // 0이면 이미 다운로드됨
    }

    /// <summary>
    /// 인스턴스의 핸들 키 찾기
    /// </summary>
    private string FindInstanceKey(GameObject instance)
    {
        string targetKey = $"_{instance.GetInstanceID()}";
        foreach (var key in _handles.Keys)
        {
            if (key.EndsWith(targetKey))
                return key;
        }
        return null;
    }

    /// <summary>
    /// 디버그: 현재 로드된 핸들 목록
    /// </summary>
    public void PrintLoadedAssets()
    {
        Debug.Log($"=== 로드된 에셋 목록 ({_handles.Count}개) ===");
        foreach (var key in _handles.Keys)
        {
            Debug.Log($"  - {key}");
        }
    }

    private void OnDestroy()
    {
        // 앱 종료 시 자동 정리
        ReleaseAll();
    }
}

// ===== 사용 예시 =====
/*

// 1. 스프라이트 로드
Sprite sprite = await AddressablesHelper.Instance.LoadAssetAsync<Sprite>("TestSprite");
myImage.sprite = sprite;

// 2. 프리팹 생성
GameObject player = await AddressablesHelper.Instance.InstantiateAsync("Player");

// 3. 다운로드 확인
bool isReady = await AddressablesHelper.Instance.IsDownloadedAsync("TestSprite");
if (!isReady)
{
    Debug.Log("다운로드 필요!");
}

// 4. 해제
AddressablesHelper.Instance.ReleaseAsset("TestSprite");
AddressablesHelper.Instance.ReleaseInstance(player);

// 5. 씬 전환 시
AddressablesHelper.Instance.ReleaseAll();

*/
