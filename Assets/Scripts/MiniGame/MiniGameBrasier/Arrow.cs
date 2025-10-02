using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    public Camera mainCam;            // 메인 카메라
    public Transform target;          // 풀무 오브젝트
    public RectTransform arrowUI;     // UI 화살표 (Canvas 안의 Image)

    public float bounceHeight = 20f;  // 위아래 이동 높이
    public float bounceSpeed = 2f;    // 이동 속도

    private Vector3 basePos;

    void Start()
    {
        if (mainCam == null)
            mainCam = Camera.main;
    }

    void Update()
    {
        if (target == null || arrowUI == null) return;

        // 1) 3D 위치 → 화면 좌표
        Vector3 screenPos = mainCam.WorldToScreenPoint(target.position);

        // 2) 화면 좌표를 UI에 적용
        arrowUI.position = screenPos + new Vector3(0, 100, 0); // 오프셋 (위쪽에 표시)

        // 3) 위아래로 흔들리기 (Sin 함수 이용)
        float offsetY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        arrowUI.position += new Vector3(0, offsetY, 0);
    }
}