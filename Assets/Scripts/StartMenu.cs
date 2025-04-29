using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void OnNewGameClick() //���ο� ���� ��ư
    {
        SceneManager.LoadScene("CharacterSelectScene"); // ĳ���� ���� ������ �̵�
    }

    public void OnContinueClick() //�̾��ϱ� ��ư
    {
        Debug.Log("�̾��ϱ�");
    }

    public void OnSettingClick() //���� ��ư
    {
        Debug.Log("���� â");
    }

    public void OnQuitClick() //�����ϱ� ��ư
    {
        Application.Quit(); //����
        Debug.Log("�����");
    }

}
