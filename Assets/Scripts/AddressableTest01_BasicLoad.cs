using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

/// <summary>
/// Addressables 기초 테스트 #1: 스프라이트 로드
///
/// 학습 목표:
/// 1. Addressables로 에셋 로드하는 방법
/// 2. AsyncOperationHandle 사용법
/// 3. 로드 성공/실패 처리
/// </summary>
public class AddressableTest01_BasicLoad : MonoBehaviour
{
    [Header("UI 연결")]
    public Image targetImage;           // 로드한 이미지를 표시할 UI Image
    public Button loadButton;           // 로드 버튼
    public Text statusText;             // 상태 표시 텍스트

    [Header("Addressables 설정")]
    public string spriteAddress = "TestSprite";  // Addressables에 등록할 주소

    private AsyncOperationHandle<Sprite> loadHandle;  // 로드 핸들 (메모리 관리용)

    void Start()
    {
        // 버튼 클릭 이벤트 연결
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(OnLoadButtonClick);
        }

        UpdateStatus("준비 완료. 버튼을 눌러 이미지를 로드하세요.");
    }

    /// <summary>
    /// 버튼 클릭 시 호출
    /// </summary>
    void OnLoadButtonClick()
    {
        LoadSpriteAsync();
    }

    /// <summary>
    /// Addressables로 스프라이트 로드 (비동기)
    /// </summary>
    async void LoadSpriteAsync()
    {
        UpdateStatus("로딩 중...");

        try
        {
            // ===== 핵심 코드 #1: Addressables 로드 =====
            // LoadAssetAsync<T>(address) : 주소로 에셋을 비동기로 로드
            loadHandle = Addressables.LoadAssetAsync<Sprite>(spriteAddress);

            // Task로 변환하여 await 사용 가능
            Sprite loadedSprite = await loadHandle.Task;

            // ===== 핵심 코드 #2: 로드 성공 확인 =====
            if (loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                // 로드 성공!
                targetImage.sprite = loadedSprite;
                UpdateStatus($"✓ 로드 성공: {spriteAddress}");
                Debug.Log($"스프라이트 로드 완료: {loadedSprite.name}");
            }
            else
            {
                // 로드 실패
                UpdateStatus("✗ 로드 실패");
                Debug.LogError($"스프라이트 로드 실패: {spriteAddress}");
            }
        }
        catch (System.Exception ex)
        {
            // 예외 처리 (주소가 잘못되었거나 에셋이 없을 때)
            UpdateStatus($"✗ 에러: {ex.Message}");
            Debug.LogError($"로드 중 오류 발생: {ex.Message}");
        }
    }

    /// <summary>
    /// 상태 텍스트 업데이트
    /// </summary>
    void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[Status] {message}");
    }

    /// <summary>
    /// 씬 종료 시 메모리 해제
    /// </summary>
    void OnDestroy()
    {
        // ===== 핵심 코드 #3: 메모리 해제 (매우 중요!) =====
        // Addressables로 로드한 에셋은 반드시 Release 해야 함
        if (loadHandle.IsValid())
        {
            Addressables.Release(loadHandle);
            Debug.Log("메모리 해제 완료");
        }
    }
}
