using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

/// <summary>
/// Addressables 기초 테스트 #3: 다운로드 진행률 표시
///
/// 학습 목표:
/// 1. GetDownloadSizeAsync로 다운로드 크기 확인
/// 2. PercentComplete로 진행률 추적
/// 3. 코루틴으로 실시간 UI 업데이트
/// </summary>
public class AddressableTest03_ProgressBar : MonoBehaviour
{
    [Header("UI 연결")]
    public Button downloadButton;
    public Slider progressBar;
    public Text progressText;
    public Text sizeText;
    public Image previewImage;

    [Header("Addressables 설정")]
    public string assetAddress = "TestSprite";

    private AsyncOperationHandle downloadHandle;

    void Start()
    {
        if (downloadButton != null)
            downloadButton.onClick.AddListener(OnDownloadButtonClick);

        if (progressBar != null)
            progressBar.value = 0f;

        CheckDownloadSize();
    }

    /// <summary>
    /// 다운로드 크기 확인 (에셋이 이미 로컬에 있는지 체크)
    /// </summary>
    async void CheckDownloadSize()
    {
        try
        {
            // ===== 핵심: GetDownloadSizeAsync로 다운로드 필요 크기 확인 =====
            var sizeHandle = Addressables.GetDownloadSizeAsync(assetAddress);
            long downloadSize = await sizeHandle.Task;
            Addressables.Release(sizeHandle);

            if (downloadSize > 0)
            {
                // 다운로드 필요
                float sizeMB = downloadSize / (1024f * 1024f);
                UpdateSizeText($"다운로드 필요: {sizeMB:F2} MB");
                Debug.Log($"다운로드 필요: {downloadSize} bytes ({sizeMB:F2} MB)");
            }
            else
            {
                // 이미 로컬에 있음
                UpdateSizeText("✓ 이미 다운로드됨");
                Debug.Log("에셋이 이미 로컬에 존재합니다.");
            }
        }
        catch (System.Exception ex)
        {
            UpdateSizeText($"크기 확인 실패: {ex.Message}");
        }
    }

    /// <summary>
    /// 다운로드 시작
    /// </summary>
    void OnDownloadButtonClick()
    {
        StartCoroutine(DownloadWithProgress());
    }

    /// <summary>
    /// 진행률을 표시하며 다운로드
    /// </summary>
    IEnumerator DownloadWithProgress()
    {
        UpdateProgressText("다운로드 중...");

        // ===== 1단계: 의존성 다운로드 (실제 다운로드) =====
        // DownloadDependenciesAsync: 에셋과 의존성을 미리 다운로드
        downloadHandle = Addressables.DownloadDependenciesAsync(assetAddress);

        // ===== 2단계: 진행률 추적 =====
        while (!downloadHandle.IsDone)
        {
            // PercentComplete: 0.0 ~ 1.0 (0% ~ 100%)
            float progress = downloadHandle.PercentComplete;

            if (progressBar != null)
                progressBar.value = progress;

            UpdateProgressText($"다운로드 중... {progress * 100f:F1}%");

            yield return null; // 다음 프레임까지 대기
        }

        // ===== 3단계: 다운로드 완료 확인 =====
        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            if (progressBar != null)
                progressBar.value = 1f;

            UpdateProgressText("✓ 다운로드 완료!");
            Debug.Log("다운로드 성공");

            // 다운로드 완료 후 에셋 로드
            yield return StartCoroutine(LoadAssetAfterDownload());
        }
        else
        {
            UpdateProgressText("✗ 다운로드 실패");
            Debug.LogError("다운로드 실패");
        }

        // 다운로드 핸들 해제
        Addressables.Release(downloadHandle);
    }

    /// <summary>
    /// 다운로드 완료 후 에셋 로드
    /// </summary>
    IEnumerator LoadAssetAfterDownload()
    {
        UpdateProgressText("에셋 로딩 중...");

        var loadHandle = Addressables.LoadAssetAsync<Sprite>(assetAddress);
        yield return loadHandle;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded && previewImage != null)
        {
            previewImage.sprite = loadHandle.Result;
            UpdateProgressText("✓ 로드 완료!");
        }

        Addressables.Release(loadHandle);
    }

    void UpdateProgressText(string text)
    {
        if (progressText != null)
            progressText.text = text;
    }

    void UpdateSizeText(string text)
    {
        if (sizeText != null)
            sizeText.text = text;
    }

    void OnDestroy()
    {
        if (downloadHandle.IsValid())
            Addressables.Release(downloadHandle);
    }
}
