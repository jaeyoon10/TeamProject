using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public GameObject characterButtonPanel;// 캐릭터 선택 3개 버튼이 들어있는 패널
    public Image characterImageDisplay;     // 왼쪽 큰 이미지 보여줄 곳
    public Sprite edwinSprite, isabellaSprite, tuskSprite;
    public GameObject characterInfoPanel; // 캐릭터 정보를 표시할 전체 패널 (오른쪽 패널)

    // 각각의 텍스트 UI (이름, 설명, 능력치)
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text statsText;

    // 현재 선택된 캐릭터 ID (Warrior, Archer, Mage 중 하나)
    private string selectedCharacter;

    public void OnClickCharacter(string characterId)  // 버튼 클릭 시 실행될 함수 (각 캐릭터 버튼에 이 함수 연결함)
    {
        selectedCharacter = characterId;
        characterButtonPanel.SetActive(false); // 선택 버튼 숨기기
        characterInfoPanel.SetActive(true);    // 설명 패널 보이기

        //캐릭터 정보
        switch (characterId)
        {
            case "Edwin":
                characterImageDisplay.sprite = edwinSprite;
                nameText.text = "에드윈";
                descriptionText.text = "근접 전투에 능한 강력한 캐릭터.";
                statsText.text = "힘: 8\n민첩: 4\n지능: 2"; // 줄바꿈(\n)으로 한 줄씩 표시
                break;
            case "Isabella":
                characterImageDisplay.sprite = isabellaSprite;
                nameText.text = "이사벨라";
                descriptionText.text = "원거리에서 빠르게 적을 제압.";
                statsText.text = "힘: 4\n민첩: 8\n지능: 3";
                break;

            case "Tusk":
                characterImageDisplay.sprite = tuskSprite;
                nameText.text = "터스크";
                descriptionText.text = "전 세계 몇 명 밖에 없는 그랜드 마스터라는 \n칭호를 보유하고 있는 베테랑 명장.\n모든 것을 만들 수 있지만, 반대로 고집이 세 자신의 철학에 \n이의를 제기하는" +
                    " 잔챙이들을 매우 싫어한다.\n시간이 지나면 대장간을 처분하고 소리소문 \n없이 사라진다고 한다. 자세한 이유는 아무도 모른다고 한다.";
                statsText.text = "힘: 2\n민첩: 3\n지능: 9";
                break;
        }

    }

    public void OnClickBack()
    {
        characterInfoPanel.SetActive(false);  // 설명 끄기
        characterButtonPanel.SetActive(true); // 캐릭터 선택 다시 보이기
    }

    public void OnClickSelect() // "캐릭터 선택" 버튼을 누르면 호출됨
    {
        Debug.Log($"{selectedCharacter} 선택됨!");

        // 다음 씬 로딩 예시
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene"); // GameScene 씬으로 전환 (선택 완료 후 게임 시작)
    }
}
