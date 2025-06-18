using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMarge : MonoBehaviour
{
    void Start()
    {
        // "UI_Scene"은 UI 씬 이름 (확장자 .unity는 제외)
        SceneManager.LoadSceneAsync("UIScene", LoadSceneMode.Additive);
    }
}
