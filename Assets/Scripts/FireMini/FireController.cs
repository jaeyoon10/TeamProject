using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : MonoBehaviour
{
    public RectTransform moveArea; // ��� ��
    public float speed = 100f;

    private RectTransform rect;
    private float minY;
    private float maxY;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        float halfAreaH = moveArea.rect.height * 0.5f;
        float halfBarH = rect.rect.height * 0.5f;

        // �߽��� ������ �� �ִ� ����
        minY = -halfAreaH + halfBarH;
        maxY = halfAreaH - halfBarH;

        // ������ �ٴ�����
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, minY);
    }

    void Update()
    {
        // PingPong ������ (maxY - minY)
        float range = maxY - minY;
        float y = Mathf.PingPong(Time.time * speed, range);

        // minY �������� offset
        rect.anchoredPosition = new Vector2(
            rect.anchoredPosition.x,
            minY + y
        );
    }
}
