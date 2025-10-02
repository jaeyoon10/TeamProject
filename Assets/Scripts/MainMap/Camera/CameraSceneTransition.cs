using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class CameraSceneTransition : MonoBehaviour
{
    public Camera cam;
    public Transform focusPoint;
    public float zoomFOV = 20f;
    public float duration = 1.5f;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private float originalFOV;

    void Start()
    {
        // 현재 씬 카메라 기본 상태 저장
        originalPos = cam.transform.position;
        originalRot = cam.transform.rotation;  // 회전값도 저장
        originalFOV = cam.fieldOfView;

        // 씬 전환 이어받기
        if (CameraTransitionData.continueZoomIn)
        {
            cam.transform.position = CameraTransitionData.camPos;
            cam.fieldOfView = CameraTransitionData.camSize;
            cam.transform.rotation = CameraTransitionData.camRot; // 회전 복원

            StartCoroutine(ContinueZoomIn());
            CameraTransitionData.continueZoomIn = false;
        }
        else if (CameraTransitionData.continueZoomOut)
        {
            cam.transform.position = CameraTransitionData.camPos;
            cam.fieldOfView = CameraTransitionData.camSize;
            cam.transform.rotation = CameraTransitionData.camRot; // 회전 복원

            StartCoroutine(ContinueZoomOut());
            CameraTransitionData.continueZoomOut = false;
        }
    }

    public void StartZoomIn(string sceneName)
    {
        StartCoroutine(ZoomInAndSwitch(sceneName));
    }

    public void StartZoomOut(string sceneName)
    {
        StartCoroutine(ZoomOutAndSwitch(sceneName));
    }

    IEnumerator ZoomInAndSwitch(string sceneName)
    {
        float t = 0;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        Vector3 targetPos = focusPoint.position - cam.transform.forward * 3f;
        Quaternion targetRot = Quaternion.LookRotation(focusPoint.position - targetPos);

        while (t < 0.5f)
        {
            t += Time.deltaTime / duration;
            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t); // 회전도 보간
            cam.fieldOfView = Mathf.Lerp(originalFOV, zoomFOV, t);
            yield return null;
        }

        // 상태 저장
        CameraTransitionData.camPos = cam.transform.position;
        CameraTransitionData.camSize = cam.fieldOfView;
        CameraTransitionData.camRot = cam.transform.rotation; // 회전 저장
        CameraTransitionData.continueZoomIn = true;

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator ContinueZoomIn()
    {
        float t = 0.5f;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        Vector3 targetPos = focusPoint.position - cam.transform.forward * 3f;
        Quaternion targetRot = Quaternion.LookRotation(focusPoint.position - targetPos);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            cam.fieldOfView = Mathf.Lerp(originalFOV, zoomFOV, t);
            yield return null;
        }
    }

    IEnumerator ZoomOutAndSwitch(string sceneName)
    {
        float t = 0;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;
        float startFOV = cam.fieldOfView;

        while (t < 0.5f)
        {
            t += Time.deltaTime / duration;
            cam.transform.position = Vector3.Lerp(startPos, originalPos, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, originalRot, t); // 원래 회전으로
            cam.fieldOfView = Mathf.Lerp(startFOV, originalFOV, t);
            yield return null;
        }

        CameraTransitionData.camPos = cam.transform.position;
        CameraTransitionData.camSize = cam.fieldOfView;
        CameraTransitionData.camRot = cam.transform.rotation; // 저장
        CameraTransitionData.continueZoomOut = true;

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator ContinueZoomOut()
    {
        float t = 0.5f;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;
        float startFOV = cam.fieldOfView;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cam.transform.position = Vector3.Lerp(startPos, originalPos, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, originalRot, t); // 원래 회전으로
            cam.fieldOfView = Mathf.Lerp(startFOV, originalFOV, t);
            yield return null;
        }
    }
}