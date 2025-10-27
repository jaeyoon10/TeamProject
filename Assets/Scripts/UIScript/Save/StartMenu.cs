using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void OnNewGameClick()
    {
        SceneManager.LoadScene("CharacterSelectScene");
    }

    public void OnContinueClick()
    {
        Debug.Log("이어하기");
    }

    public void OnSettingClick()
    {
        Debug.Log("설정 창");
    }

    public void OnQuitClick()
    {
        Application.Quit();
        Debug.Log("종료됨");
    }
}