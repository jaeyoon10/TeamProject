using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void OnNewGameClick() //새로운 시작 버튼
    {
        SceneManager.LoadScene("CharacterSelectScene"); // 캐릭터 선택 씬으로 이동
    }

    public void OnContinueClick() //이어하기 버튼
    {
        Debug.Log("이어하기");
    }

    public void OnSettingClick() //설정 버튼
    {
        Debug.Log("설정 창");
    }

    public void OnQuitClick() //종료하기 버튼
    {
        Application.Quit(); //종료
        Debug.Log("종료됨");
    }

}
