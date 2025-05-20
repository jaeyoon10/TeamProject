using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        // 여기에 미니게임 종료 로직을 넣어도 됨
        // 예: 게임오브젝트 끄기, 이벤트 전송, 상위 매니저 호출 등
        // gameObject.SetActive(false);
    }
}