using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance { get; private set; }

    public GameObject panel;     // TooltipPanel
    public TMP_Text text;      // TooltipText

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        panel.SetActive(false);
    }

    /// <summary>
    /// 보여주기
    /// </summary>
    /// <param name="s">툴팁으로 띄울 문자열</param>
    /// <param name="rt">아이템 RectTransform(마우스 위치 계산용)</param>
    public void Show(string s, RectTransform rt)
    {
        text.text = s;
        panel.SetActive(true);

        // 화면 좌표로 변환
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        // 위쪽 중앙에 띄우기
        Vector3 worldPos = (corners[1] + corners[2]) * 0.5f;
        panel.transform.position = worldPos + Vector3.up * 10;
    }

    /// <summary>숨기기</summary>
    public void Hide()
    {
        panel.SetActive(false);
    }
}