using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : MonoBehaviour
{
    public GameObject characterButtonPanel;// 이력서 목록 패널

    public Image characterImageDisplay;     // 왼쪽 큰 이미지 보여줄 곳
    public GameObject characterInfoPanel; // 캐릭터 정보를 표시할 전체 패널 (오른쪽 패널)

    public Sprite edwinSprite, isabellaSprite, tuskSprite; //캐릭터 스프라이트

    public Button checkbtn;
    public Button BackScenebtn;
    public Button BackPanelbtn;

    // 각각의 텍스트 UI (이름, 설명, 능력치)
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text statsText;

    // 현재 선택된 캐릭터 ID (에드윈, 이사벨라, 터스크 중 하나)
    private string selectedCharacter;

    public void OnClickCharacter(string characterId)  // 버튼 클릭 시 실행될 함수 (각 캐릭터 버튼에 이 함수 연결함)
    {
        selectedCharacter = characterId;

        characterButtonPanel.SetActive(false); // 선택 버튼 숨기기
        characterInfoPanel.SetActive(true);    // 설명 패널 보이기

        checkbtn.gameObject.SetActive(true); // 선택 버튼 보이기
        BackPanelbtn.gameObject.SetActive(true); // 캐릭터 선택 패널로 뒤로가기 보이기
        BackScenebtn.gameObject.SetActive(false); // 게임 씬 뒤로가기 끄기

        //이력서 정보 표시
        switch (characterId)
        {
            case "Edwin":
                characterImageDisplay.sprite = edwinSprite;
                nameText.text = "에드윈 (Edwin)";
                descriptionText.text = "마을을 수호하던 기사 출신의 남성, 은퇴를 하며 기사 생활을 접고 마을 외딴 곳에 작은 대장간을 차렸다.\n 대장장이로서 새롭게 삶을 시작한 이 남자는 세계제일의 대장장이를 꿈꾸며 오늘도 열심히 망치를 두들긴다.";
                statsText.text = "능력\n익숙한 그립감 : 전투 계열 장비 제작 시, 난이도 상관 없이 성공 확률을 증가시킨다.\n전직 기사의 힘 : 강화 성공 확률을 + 10% 만큼 증가시킨다."; // 줄바꿈(\n)으로 한 줄씩 표시
                break;
            case "Isabella":
                characterImageDisplay.sprite = isabellaSprite;
                nameText.text = "이사벨라 (Isabella)";
                descriptionText.text = "평생 대장장이 삶을 살았던 아버지의 강요로 원치 않는 가업을 물려받은 아리따운 외모를 가진 소녀.\n퉁명스럽고 불량한 태도, 만사 귀찮아 하고 하기 싫어하는 그녀에게 축복 받은 행운이 있다는 사실을 그녀 빼고 모든 마을 사람들이 알고 있다.";
                statsText.text = "능력\n초심자의 행운 : 제작 성공 시, 의뢰 받은 손님의 만족도 상관 없이 보상을 추가로 받는다.\n섬세한 손놀림 : 제작 시 발동되는 미니게임의 판정을 좋게 바꿔준다.\n가녀린 손목 : 일상 도구 이외의 장비는 제작할 수 없다.";
                break;

            case "Tusk":
                characterImageDisplay.sprite = tuskSprite;
                nameText.text = "터스크 (Tusk)";
                descriptionText.text = "전 세계 몇 명 밖에 없는 그랜드 마스터라는 칭호를 보유하고 있는 베테랑 명장.\n모든 것을 만들 수 있지만, 반대로 고집이 세 자신의 철학에 이의를 제기하는" +
                    " 잔챙이들을 매우 싫어한다.\n시간이 지나면 대장간을 처분하고 소리소문 없이 사라진다고 한다. 자세한 이유는 아무도 모른다고 한다.";
                statsText.text = "능력\n명장의 품격 : 숙련도(레벨) 상관 없이 모든 것을 제작 할 수 있다.\n명장의 고집 : 모든 것을 자신만의 철학과 감각으로 제작한다.(제작 레시피 확인 불가)";
                break;
        }

    }

    public void OnClickBack()
    {
        characterInfoPanel.SetActive(false);  // 설명 끄기
        characterButtonPanel.SetActive(true); // 캐릭터 선택 다시 보이기
        checkbtn.gameObject.SetActive(false); //선택 버튼 끄기
        BackPanelbtn.gameObject.SetActive(false); // 캐릭터 선택 패널로 뒤로가기 끄기
        BackScenebtn.gameObject.SetActive(true); // 게임 씬 뒤로가기 보이기
    }

    public void OnClickSelect() // "캐릭터 선택" 버튼을 누르면 호출됨
    {
        Debug.Log($"{selectedCharacter} 선택됨!");

        // 씬 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");

    }

    public void OnClickSceneBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }
}
