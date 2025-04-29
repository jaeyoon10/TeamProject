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
    }

    public void OnClickSelect() // "ĳ���� ����" ��ư�� ������ ȣ���
    {
        Debug.Log($"{selectedCharacter} ���õ�!");

        // ���� �� �ε� ����
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene"); // GameScene ������ ��ȯ (���� �Ϸ� �� ���� ����)
    }
}
