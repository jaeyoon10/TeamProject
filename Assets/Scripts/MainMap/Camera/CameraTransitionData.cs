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

    public static bool resumeAfterReturn;   // 추가됨
    public static int nextStepIndex;        // 추가됨 (1=해머, 2=연마, -1=완료)
    public static int savedQuality;         // 추가됨 (품질 점수 이어 받기)
}
