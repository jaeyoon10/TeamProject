using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SharpeningSwipeGame : MonoBehaviour
{
    [Header("Game Settings")]
    public int sequenceLength = 4;               // 표시할 키 개수 
    public float displayDuration = 1.0f;         // 키 표시 시간 (초)

    [Header("UI Components")]
    public Transform slotContainer;              // HorizontalLayoutGroup이 붙은 컨테이너
    public GameObject slotPrefab;                // TextMeshProUGUI 컴포넌트가 있는 프리팹
    public TextMeshProUGUI resultText;           // 성공/실패 메시지
    public TextMeshProUGUI inputPromptText;      // "입력하세요" 안내 텍스트

    private List<KeyCode> sequence;              
    private int inputIndex;                      // 현재 입력 인덱스
    private bool inputEnabled;                   // 입력 가능 여부

    void Start()
    {
        inputPromptText.gameObject.SetActive(false);
        StartGame();
    }

    void StartGame()
    {
        resultText.text = "";
        inputPromptText.gameObject.SetActive(false);
        inputEnabled = false;
        GenerateSequence();
        ShowSequence();
    }

    void GenerateSequence()
    {
        sequence = new List<KeyCode>(sequenceLength);
        for (int i = 0; i < sequenceLength; i++)
        {
            sequence.Add(Random.value < 0.5f ? KeyCode.A : KeyCode.D);
        }
        Debug.Log("[SharpeningGame] Generated Sequence: " + string.Join(",", sequence));
    }

    void ShowSequence()
    {
        // 이전 슬롯들 제거
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        foreach (KeyCode key in sequence)
        {
            GameObject go = Instantiate(slotPrefab, slotContainer);
            TextMeshProUGUI txt = go.GetComponent<TextMeshProUGUI>()
                                 ?? go.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = key.ToString();
        }

        Debug.Log("[SharpeningGame] Showing Sequence");
        Invoke(nameof(HideSequence), displayDuration);
    }

    void HideSequence()
    {
        // 표시된 슬롯 제거
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        inputIndex = 0;
        inputEnabled = true;
        inputPromptText.text = "입력하세요";
        inputPromptText.gameObject.SetActive(true);

        Debug.Log("[SharpeningGame] Input Enabled. Ready for input.");
    }

    void Update()
    {
        if (!inputEnabled) return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
        {
            KeyCode pressed = Input.GetKeyDown(KeyCode.A) ? KeyCode.A : KeyCode.D;
            
            if (pressed == sequence[inputIndex])
            {
                inputIndex++;
                if (inputIndex == 1)
                    inputPromptText.gameObject.SetActive(false);

                if (inputIndex >= sequenceLength)
                {
                    resultText.text = "성공!";
                    inputEnabled = false;
                    Invoke(nameof(StartGame), 2f);
                }
            }
            else
            {
                resultText.text = "실패!";
                inputEnabled = false;
                Invoke(nameof(StartGame), 2f);
            }
        }
    }
}
