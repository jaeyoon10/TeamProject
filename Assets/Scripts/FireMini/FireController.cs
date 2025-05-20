using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : MonoBehaviour
{
    [Header("이동 영역")]
    public RectTransform moveArea;

    [Header("속도 범위 (유닛/초)")]
    public float minSpeed = 50f;
    public float maxSpeed = 200f;

    [Header("방향/속도 변경 주기 (초)")]
    public float changeInterval = 1f;

    private RectTransform rect;
    private float minY;
    private float maxY;

    private float speed;       // 현재 속도
    private int direction;     // 1=위, -1=아래
    private float changeTimer; // 타이머

    void Start()
    {
        rect = GetComponent<RectTransform>();

        // 영역 세팅
        float halfAreaH = moveArea.rect.height * 0.5f;
        float halfBarH = rect.rect.height * 0.5f;
        minY = -halfAreaH + halfBarH;
        maxY = halfAreaH - halfBarH;

        // 초기 위치, 속도, 방향
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, minY);
        speed = Random.Range(minSpeed, maxSpeed);
        direction = Random.value < 0.5f ? 1 : -1;
        changeTimer = changeInterval;
    }

    void Update()
    {
        // 1) 주기마다 랜덤으로 속도·방향 변경
        changeTimer -= Time.deltaTime;
        if (changeTimer <= 0f)
        {
            speed = Random.Range(minSpeed, maxSpeed);
            direction = Random.value < 0.5f ? 1 : -1;
            changeTimer += changeInterval;
        }

        // 2) 위치 업데이트
        Vector2 pos = rect.anchoredPosition;
        pos.y += direction * speed * Time.deltaTime;

        // 3) 바닥/천장 충돌 처리 (튕겨 나오는 효과)
        if (pos.y > maxY)
        {
            pos.y = maxY;
            direction = -direction;           // 반대 방향으로
            speed = Random.Range(minSpeed, maxSpeed);
            changeTimer = changeInterval;     // 즉시 다음 랜덤 주기 시작
        }
        else if (pos.y < minY)
        {
            pos.y = minY;
            direction = -direction;
            speed = Random.Range(minSpeed, maxSpeed);
            changeTimer = changeInterval;
        }

        // 4) 적용
        rect.anchoredPosition = pos;
    }
}
