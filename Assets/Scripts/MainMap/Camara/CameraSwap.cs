using UnityEngine;

public class CameraSwap : MonoBehaviour
{
    [Header("Main Camera")]
    public Camera mainCamera;   // 메인 카메라만 연결
    public bool IsLocked { get; private set; } = false;

    void Awake()
    {
        if (!mainCamera)
            mainCamera = GetComponent<Camera>(); // 자동 연결
    }

    /// <summary>
    /// 미니게임 진입 (메인 → 미니)
    /// </summary>
    public void EnterMiniGame(Camera miniCam, GameObject moduleRoot, Canvas moduleUI)
    {
        if (IsLocked) return;
        IsLocked = true;

        // 메인 카메라 끄기
        if (mainCamera) mainCamera.enabled = false;

        // 미니게임 활성화
        if (moduleRoot) moduleRoot.SetActive(true);
        if (moduleUI) moduleUI.enabled = true;
        if (miniCam) miniCam.enabled = true;
    }

    /// <summary>
    /// 미니게임 종료 (미니 → 메인)
    /// </summary>
    public void ExitMiniGame(Camera miniCam, GameObject moduleRoot, Canvas moduleUI)
    {
        if (!IsLocked) return;
        IsLocked = false;

        // 미니게임 끄기
        if (moduleRoot) moduleRoot.SetActive(false);
        if (moduleUI) moduleUI.enabled = false;
        if (miniCam) miniCam.enabled = false;

        // 메인 카메라 복귀
        if (mainCamera) mainCamera.enabled = true;
    }
}
