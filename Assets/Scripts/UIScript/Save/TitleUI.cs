using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    public Button continueButton;
    public Button newGameButton;

    void Start()
    {
        continueButton.interactable = SaveManager.HasSave();
        continueButton.onClick.AddListener(OnContinue);
        newGameButton.onClick.AddListener(OnNewGame);
    }

    void OnContinue()
    {
        PlayerPrefs.SetInt("LoadFlag", 1);
        SceneManager.LoadScene("Ingame_main");
    }

    void OnNewGame()
    {
        SaveManager.DeleteSave();
        PlayerPrefs.SetInt("LoadFlag", 0);
        PlayerPrefs.Save();
        SceneManager.LoadScene("CharacterSelectScene");
    }
}