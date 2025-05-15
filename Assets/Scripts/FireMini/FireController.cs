using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : MonoBehaviour
{
    public RectTransform moveArea; // 흰색 바
    public float speed = 100f;

    private RectTransform rect;
    private float minY;
    private float maxY;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        float halfAreaH = moveArea.rect.height * 0.5f;
        float halfBarH = rect.rect.height * 0.5f;

        // 중심이 움직일 수 있는 범위
        minY = -halfAreaH + halfBarH;
        maxY = halfAreaH - halfBarH;

        // 시작을 바닥으로
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, minY);
    }

    void Update()
    {
        // PingPong 범위는 (maxY - minY)
        float range = maxY - minY;
        float y = Mathf.PingPong(Time.time * speed, range);

        // minY 기준으로 offset
        rect.anchoredPosition = new Vector2(
            rect.anchoredPosition.x,
            minY + y
        );
    }
}
