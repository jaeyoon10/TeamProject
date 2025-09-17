using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SharpeningSwipeGame : MonoBehaviour
{
    [Header("Game Settings")]
    public int sequenceLength = 4;               // 표시할 키 개수 
    public float displayDuration = 1.0f;         // 키 표시 시간 (초)

    [Header("시도 결과 표시 (O 표시들)")]
    public Image[] resultIndicators; // 2개 연결
    public Color successColor = new Color(0.5f, 0.9f, 1f); // 하늘색
    public Color failColor = Color.red;

    [Header("UI Components")]
    public Transform slotContainer;              // HorizontalLayoutGroup이 붙은 컨테이너
    public GameObject slotPrefab;                // TextMeshProUGUI 컴포넌트가 있는 프리팹
    public TextMeshProUGUI resultText;           // 성공/실패 메시지
    public TextMeshProUGUI inputPromptText;      // "입력하세요" 안내 텍스트

    private int totalAttempts;
    private int totalSuccesse;
    private List<KeyCode> sequence;              
    private int inputIndex;                      // 현재 입력 인덱스
    private bool inputEnabled;                   // 입력 가능 여부

    /* === 품질 계산용 공개 카운터 추가 === */
    public int failCount { get; private set; }   

    public System.Action<int> onGameFinished; // 추가

    void Start()
    {
        inputPromptText.gameObject.SetActive(false);
        StartGame();
        ResetResults();
    }

    void ResetResults()
    {
        totalAttempts = 0;
        totalSuccesse = 0;
        failCount = 0;                      
        foreach (var img in resultIndicators)
            img.color = Color.gray;
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
                    EndGame(true);
                }
            }
            else
            {
                resultText.text = "실패!";
                failCount++;
                EndGame(false);
            }
        }
    }
    void EndGame(bool isSuccess)
    {
        inputEnabled = false;

        // 시도 결과 색상 표시
        if (totalAttempts < resultIndicators.Length)
        {
            resultIndicators[totalAttempts].color = isSuccess ? successColor : failColor;
        }

        totalAttempts++;
        if (isSuccess) totalSuccesse++;

        // 2번 다 했으면 판정
        if (totalAttempts >= 2)
        {
            failCount = 2 - totalSuccesse;
            onGameFinished?.Invoke(failCount);
            // 씬 언로드 딜레이 코루틴 호출
            StartCoroutine(DelayedUnload());
        }
        else
        {
            Invoke(nameof(StartGame), 1.5f); // 다음 시도
        }
    }
    private IEnumerator DelayedUnload()
    {
        // 결과가 보일 시간을 줍니다.
        yield return new WaitForSeconds(1f);

        // 씬 이름에 맞춰 언로드
        SceneManager.UnloadSceneAsync("MiniGameRub");
    }

}
