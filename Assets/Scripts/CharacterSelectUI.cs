using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public GameObject characterButtonPanel;// ĳ���� ���� 3�� ��ư�� ����ִ� �г�
    public Image characterImageDisplay;     // ���� ū �̹��� ������ ��
    public Sprite edwinSprite, isabellaSprite, tuskSprite;
    public GameObject characterInfoPanel; // ĳ���� ������ ǥ���� ��ü �г� (������ �г�)

    public Button checkbtn;
    public Button BackScenebtn;
    public Button BackPanelbtn;

    public GameObject nicknamePanel;             // �г��� �г�
    public TMP_InputField nicknameInputField;    // �г��� �Է� �ʵ�
    public Button nicknameConfirmButton;         // Ȯ�� ��ư
    public Button nicknameCancelButton;

    // ������ �ؽ�Ʈ UI (�̸�, ����, �ɷ�ġ)
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text statsText;

    // ���� ���õ� ĳ���� ID (Warrior, Archer, Mage �� �ϳ�)
    private string selectedCharacter;

    public void OnClickCharacter(string characterId)  // ��ư Ŭ�� �� ����� �Լ� (�� ĳ���� ��ư�� �� �Լ� ������)
    {
        selectedCharacter = characterId;
        characterButtonPanel.SetActive(false); // ���� ��ư �����
        characterInfoPanel.SetActive(true);    // ���� �г� ���̱�
        checkbtn.gameObject.SetActive(true); // ���� ��ư ���̱�
        BackPanelbtn.gameObject.SetActive(true); // ĳ���� ���� �гη� �ڷΰ��� ���̱�
        BackScenebtn.gameObject.SetActive(false); // ���� �� �ڷΰ��� ����

        //ĳ���� ����
        switch (characterId)
        {
            case "Edwin":
                characterImageDisplay.sprite = edwinSprite;
                nameText.text = "������";
                descriptionText.text = "���� ������ ���� ������ ĳ����.";
                statsText.text = "��: 8\n��ø: 4\n����: 2"; // �ٹٲ�(\n)���� �� �پ� ǥ��
                break;
            case "Isabella":
                characterImageDisplay.sprite = isabellaSprite;
                nameText.text = "�̻级��";
                descriptionText.text = "���Ÿ����� ������ ���� ����.";
                statsText.text = "��: 4\n��ø: 8\n����: 3";
                break;

            case "Tusk":
                characterImageDisplay.sprite = tuskSprite;
                nameText.text = "�ͽ�ũ";
                descriptionText.text = "�� ���� �� �� �ۿ� ���� �׷��� �����Ͷ�� \nĪȣ�� �����ϰ� �ִ� ���׶� ����.\n��� ���� ���� �� ������, �ݴ�� ������ �� �ڽ��� ö�п� \n���Ǹ� �����ϴ�" +
                    " ��ì�̵��� �ſ� �Ⱦ��Ѵ�.\n�ð��� ������ ���尣�� ó���ϰ� �Ҹ��ҹ� \n���� ������ٰ� �Ѵ�. �ڼ��� ������ �ƹ��� �𸥴ٰ� �Ѵ�.";
                statsText.text = "��: 2\n��ø: 3\n����: 9";
                break;
        }

    }

    public void OnClickBack()
    {
        characterInfoPanel.SetActive(false);  // ���� ����
        characterButtonPanel.SetActive(true); // ĳ���� ���� �ٽ� ���̱�
        checkbtn.gameObject.SetActive(false); //���� ��ư ����
        BackPanelbtn.gameObject.SetActive(false); // ĳ���� ���� �гη� �ڷΰ��� ����
        BackScenebtn.gameObject.SetActive(true); // ���� �� �ڷΰ��� ���̱�
    }

    public void OnClickSelect() // "ĳ���� ����" ��ư�� ������ ȣ���
    {
        Debug.Log($"{selectedCharacter} ���õ�!");

        nicknamePanel.SetActive(true);
        
    }

    public void OnClickNicknameConfirm()
    {
        string nickname = nicknameInputField.text;

        if (string.IsNullOrEmpty(nickname))
        {
            Debug.Log("�г����� �Է����ּ���.");
            return;
        }

        Debug.Log($"���õ� ĳ����: {selectedCharacter}, �г���: {nickname}");

        // �÷��̾� ���� ���� ���� (PlayerPrefs ��)
        PlayerPrefs.SetString("SelectedCharacter", selectedCharacter);
        PlayerPrefs.SetString("Nickname", nickname);

        // �� �̵�
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    public void OnClickNicknameCancel()
    {
        nicknamePanel.SetActive(false);
    }
}
