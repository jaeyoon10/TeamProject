using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchBarController : MonoBehaviour
{
    public float moveSpeed = 200f;
    public float gravity = 300f;
    public RectTransform moveArea;

    private RectTransform rect;
    private float velocity;

    private float minY;
    private float maxY;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        float halfAreaH = moveArea.rect.height * 0.48f;
        float halfBarH = rect.rect.height * 0.5f;

        minY = -halfAreaH + halfBarH;
        maxY = halfAreaH - halfBarH;

        
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, minY);
        velocity = 0f;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
            velocity = moveSpeed;
        else
            velocity -= gravity * Time.deltaTime;

        // 2) 이동
        Vector2 pos = rect.anchoredPosition;
        pos.y += velocity * Time.deltaTime;

<<<<<<< HEAD
        // 바깥으로 안 나가게 제한
=======
        // 3) 바깥으로 안 나가게 제한
>>>>>>> c472041acceb31b1cea906b1b395bcfbff9a1fcc
        float clampedY = Mathf.Clamp(pos.y, minY, maxY);
        pos.y = clampedY;
        rect.anchoredPosition = pos;

<<<<<<< HEAD
        if(clampedY >= maxY && velocity > 0f)
=======
        // 4) 천장(maxY)에 닿았을 때 위로 가는 속도 제거
        if (clampedY >= maxY && velocity > 0f)
        {
            velocity = 0f;
        }

        // 5) 바닥(minY)에 닿았을 때 아래로 가는 속도 제거
        if (clampedY <= minY && velocity < 0f)
>>>>>>> c472041acceb31b1cea906b1b395bcfbff9a1fcc
        {
            velocity = 0f;
        }
    }
}
