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

        float halfAreaH = moveArea.rect.height * 0.5f;
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
        

        Vector2 pos = rect.anchoredPosition;
        pos.y += velocity * Time.deltaTime;

        // 바깥으로 안 나가게 제한
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        rect.anchoredPosition = pos;
    }
}
