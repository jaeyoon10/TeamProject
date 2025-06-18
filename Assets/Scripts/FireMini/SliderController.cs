using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public RectTransform catchBar;
    public RectTransform fire;
    public Slider progressSlider;
    public GameObject perfectText;  // "Perfect!" UI 오브젝트

    public float fillSpeed = 0.5f;
    public float drainSpeed = 0.3f;

    private bool isGameEnded = false;

    public System.Action onMiniGameSuccess;

    void Update()
    {
        if (isGameEnded) return;

        // 거리 계산 (중심 간 y 거리)
        float distance = Mathf.Abs(catchBar.anchoredPosition.y - fire.anchoredPosition.y);
        float hitThreshold = catchBar.rect.height * 0.5f;

        if (distance <= hitThreshold)
        {
            progressSlider.value += fillSpeed * Time.deltaTime;
        }
        else
        {
            progressSlider.value -= drainSpeed * Time.deltaTime;
        }

        progressSlider.value = Mathf.Clamp01(progressSlider.value);

        // 성공 체크
        if (progressSlider.value >= 1.0f)
        {
            Debug.Log("Perfect!");
            ShowPerfect();
            EndMiniGame();
        }
    }

    void ShowPerfect()
    {
        if (perfectText != null)
            perfectText.SetActive(true);
    }

    void EndMiniGame()
    {
        isGameEnded = true;

        ShowPerfect();

        onMiniGameSuccess?.Invoke();

        // 언로드 전에 딜레이를 주도록 코루틴 실행
        StartCoroutine(DelayedUnload());
    }

    private IEnumerator DelayedUnload()
    {
        // 0.5초 동안 피드백이 유지됩니다
        yield return new WaitForSeconds(1f);
        SceneManager.UnloadSceneAsync("MiniGameFire");
    }
}