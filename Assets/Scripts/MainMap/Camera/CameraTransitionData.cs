using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CameraTransitionData
{
    // 카메라 상태 저장
    public static Vector3 camPos;   // 위치
    public static float camSize;    // OrthographicSize 또는 FieldOfView
    public static Quaternion camRot;

    // 전환 상태
    public static bool continueZoomIn;
    public static bool continueZoomOut;
}
