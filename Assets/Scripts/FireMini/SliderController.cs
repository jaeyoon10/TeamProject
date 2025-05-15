using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public RectTransform target;
    public RectTransform catchBar;
    public Slider progressSlider;

    public float fillSpeed = 0.4f;
    public float drainSpeed = 0.5f;

    void Update()
    {
        float distance = Mathf.Abs(target.anchoredPosition.y - catchBar.anchoredPosition.y);
        float threshold = catchBar.rect.height / 2f;

        if (distance <= threshold)  
        {
            progressSlider.value += fillSpeed * Time.deltaTime;
        }
        else
        {
            progressSlider.value -= drainSpeed * Time.deltaTime;
        }

        progressSlider.value = Mathf.Clamp01(progressSlider.value);
    }
}
