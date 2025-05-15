using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public GameObject characterButtonPanel;// �̷¼� ��� �г�

    public Image characterImageDisplay;     // ���� ū �̹��� ������ ��
    public GameObject characterInfoPanel; // ĳ���� ������ ǥ���� ��ü �г� (������ �г�)

    public Sprite edwinSprite, isabellaSprite, tuskSprite; //ĳ���� ��������Ʈ

    public Button checkbtn;
    public Button BackScenebtn;
    public Button BackPanelbtn;

    // ������ �ؽ�Ʈ UI (�̸�, ����, �ɷ�ġ)
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text statsText;

    // ���� ���õ� ĳ���� ID (������, �̻级��, �ͽ�ũ �� �ϳ�)
    private string selectedCharacter;

    public void OnClickCharacter(string characterId)  // ��ư Ŭ�� �� ����� �Լ� (�� ĳ���� ��ư�� �� �Լ� ������)
    {
        selectedCharacter = characterId;

        characterButtonPanel.SetActive(false); // ���� ��ư �����
        characterInfoPanel.SetActive(true);    // ���� �г� ���̱�

        checkbtn.gameObject.SetActive(true); // ���� ��ư ���̱�
        BackPanelbtn.gameObject.SetActive(true); // ĳ���� ���� �гη� �ڷΰ��� ���̱�
        BackScenebtn.gameObject.SetActive(false); // ���� �� �ڷΰ��� ����

        //�̷¼� ���� ǥ��
        switch (characterId)
        {
            case "Edwin":
                characterImageDisplay.sprite = edwinSprite;
                nameText.text = "������ (Edwin)";
                descriptionText.text = "������ ��ȣ�ϴ� ��� ����� ����, ���� �ϸ� ��� ��Ȱ�� ���� ���� �ܵ� ���� ���� ���尣�� ���ȴ�.\n �������̷μ� ���Ӱ� ���� ������ �� ���ڴ� ���������� �������̸� �޲ٸ� ���õ� ������ ��ġ�� �ε���.";
                statsText.text = "�ɷ�\n�ͼ��� �׸��� : ���� �迭 ��� ���� ��, ���̵� ��� ���� ���� Ȯ���� ������Ų��.\n���� ����� �� : ��ȭ ���� Ȯ���� + 10% ��ŭ ������Ų��."; // �ٹٲ�(\n)���� �� �پ� ǥ��
                break;
            case "Isabella":
                characterImageDisplay.sprite = isabellaSprite;
                nameText.text = "�̻级�� (Isabella)";
                descriptionText.text = "��� �������� ���� ��Ҵ� �ƹ����� ����� ��ġ �ʴ� ������ �������� �Ƹ����� �ܸ� ���� �ҳ�.\n�������� �ҷ��� �µ�, ���� ������ �ϰ� �ϱ� �Ⱦ��ϴ� �׳࿡�� �ູ ���� ����� �ִٴ� ����� �׳� ���� ��� ���� ������� �˰� �ִ�.";
                statsText.text = "�ɷ�\n�ʽ����� ��� : ���� ���� ��, �Ƿ� ���� �մ��� ������ ��� ���� ������ �߰��� �޴´�.\n������ �ճ : ���� �� �ߵ��Ǵ� �̴ϰ����� ������ ���� �ٲ��ش�.\n���ะ �ո� : �ϻ� ���� �̿��� ���� ������ �� ����.";
                break;

            case "Tusk":
                characterImageDisplay.sprite = tuskSprite;
                nameText.text = "�ͽ�ũ (Tusk)";
                descriptionText.text = "�� ���� �� �� �ۿ� ���� �׷��� �����Ͷ�� Īȣ�� �����ϰ� �ִ� ���׶� ����.\n��� ���� ���� �� ������, �ݴ�� ������ �� �ڽ��� ö�п� ���Ǹ� �����ϴ�" +
                    " ��ì�̵��� �ſ� �Ⱦ��Ѵ�.\n�ð��� ������ ���尣�� ó���ϰ� �Ҹ��ҹ� ���� ������ٰ� �Ѵ�. �ڼ��� ������ �ƹ��� �𸥴ٰ� �Ѵ�.";
                statsText.text = "�ɷ�\n������ ǰ�� : ���õ�(����) ��� ���� ��� ���� ���� �� �� �ִ�.\n������ ���� : ��� ���� �ڽŸ��� ö�а� �������� �����Ѵ�.(���� ������ Ȯ�� �Ұ�)";
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

        // �� �̵�
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");

    }

    public void OnClickSceneBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }
}
