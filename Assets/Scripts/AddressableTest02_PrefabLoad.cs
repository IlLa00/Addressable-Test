using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

/// <summary>
/// Addressables 기초 테스트 #2: 프리팹 로드 및 인스턴스 생성
///
/// 학습 목표:
/// 1. 프리팹을 Addressables로 로드
/// 2. InstantiateAsync로 오브젝트 생성
/// 3. 여러 오브젝트 관리 및 해제
/// </summary>
public class AddressableTest02_PrefabLoad : MonoBehaviour
{
    [Header("UI 연결")]
    public Button loadButton;
    public Button clearButton;
    public Text statusText;
    public Text countText;

    [Header("Addressables 설정")]
    public string prefabAddress = "TestCube";  // 프리팹 주소

    [Header("생성 설정")]
    public Transform spawnParent;              // 생성될 오브젝트의 부모
    public float spawnRadius = 3f;             // 생성 반경

    // 생성된 오브젝트들의 핸들 저장 (메모리 관리용)
    private List<AsyncOperationHandle<GameObject>> instanceHandles = new List<AsyncOperationHandle<GameObject>>();

    void Start()
    {
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadButtonClick);

        if (clearButton != null)
            clearButton.onClick.AddListener(OnClearButtonClick);

        UpdateStatus("준비 완료");
        UpdateCount();
    }

    /// <summary>
    /// 프리팹 로드 및 생성
    /// </summary>
    async void OnLoadButtonClick()
    {
        UpdateStatus("프리팹 로딩 중...");

        try
        {
            // ===== 방법 1: LoadAssetAsync + Instantiate (2단계) =====
            // var handle = Addressables.LoadAssetAsync<GameObject>(prefabAddress);
            // GameObject prefab = await handle.Task;
            // GameObject instance = Instantiate(prefab);

            // ===== 방법 2: InstantiateAsync (1단계, 권장!) =====
            // InstantiateAsync는 로드 + 생성을 한 번에 처리
            Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
            randomPos.y = Mathf.Abs(randomPos.y); // 땅 위에만 생성

            var handle = Addressables.InstantiateAsync(
                prefabAddress,
                randomPos,
                Quaternion.identity,
                spawnParent
            );

            GameObject instance = await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // 핸들 저장 (나중에 해제용)
                instanceHandles.Add(handle);

                UpdateStatus($"✓ 생성 완료: {instance.name}");
                UpdateCount();

                Debug.Log($"오브젝트 생성: {instance.name} at {randomPos}");
            }
            else
            {
                UpdateStatus("✗ 생성 실패");
            }
        }
        catch (System.Exception ex)
        {
            UpdateStatus($"✗ 에러: {ex.Message}");
            Debug.LogError($"로드 중 오류: {ex.Message}");
        }
    }

    /// <summary>
    /// 생성된 모든 오브젝트 삭제
    /// </summary>
    void OnClearButtonClick()
    {
        UpdateStatus("오브젝트 삭제 중...");

        // ===== 핵심: InstantiateAsync로 생성한 오브젝트는 ReleaseInstance로 해제 =====
        foreach (var handle in instanceHandles)
        {
            if (handle.IsValid())
            {
                // ReleaseInstance: 오브젝트 파괴 + 메모리 해제
                Addressables.ReleaseInstance(handle);
            }
        }

        instanceHandles.Clear();
        UpdateStatus("✓ 모두 삭제됨");
        UpdateCount();
    }

    void UpdateStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    void UpdateCount()
    {
        if (countText != null)
            countText.text = $"생성된 오브젝트: {instanceHandles.Count}개";
    }

    void OnDestroy()
    {
        // 씬 종료 시 자동 정리
        OnClearButtonClick();
    }
}
