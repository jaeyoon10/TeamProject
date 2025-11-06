using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogUIManager : MonoBehaviour
{
    private System.Action _afterClose;
    public static DialogUIManager Instance { get; private set; }
    private bool _ownsModal = false;

    [Header("Dialog UI")]
    public GameObject dialogPanel;
    public Image icon;
    public TMP_Text dialogText;
    public TMP_Text OKText;


    public float bounceHeight = 14f;  // 위아래 이동 높이
    public float bounceSpeed = 2f;    // 이동 속도
    private Vector3 okOriginalPos;
    private bool isShowing = false;

    [Header("보상 UI (행 전체)")]
    public GameObject rewardRow;          // "보상:" 한 줄 전체 (타이틀 + 슬롯 컨테이너). 기본 비활성 권장
    public TMP_Text rewardTitleText;      // "보상 :"
    public Transform rewardContainer;     // 슬롯들 부모
    public GameObject rewardSlotPrefab;   // RewardSlotUI 프리팹


    [Header("보상 아이콘")]
    public Sprite goldIcon;
    public Sprite expIcon;
    public Sprite stressIcon;
    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    private void Start()
    {
        okOriginalPos = OKText.rectTransform.anchoredPosition;
        if (rewardRow != null) rewardRow.SetActive(false);
    }

    private void Update()
    {
        if (!isShowing) return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            HideDialog();
        }

        float offsetY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        OKText.rectTransform.anchoredPosition = okOriginalPos + new Vector3(0, offsetY);
    }

    public void ShowDialog(RecipeData recipe, CustomerType customerType)
    {
        if (recipe == null || customerType == null) return;

        dialogPanel.SetActive(true);
        icon.sprite = recipe.icon;

        // 타입별 인삿말 변경
        string title = customerType.customerTitle;
        string line = GetDialogLine(title, recipe.weaponName);
        dialogText.text = line;

        OKText.text = "G 키를 눌러 나가기";
        isShowing = true;

        if (rewardRow != null) rewardRow.SetActive(false);

        _ownsModal = false;
    }

    private string GetDialogLine(string title, string weaponName)
    {
        switch (title)
        {
            case "거지":
                return $"[거지] ...이봐, 혹시 {weaponName} 좀 싸게 만들어줄 수 있겠나...";
            case "상인":
                return $"[상인] 장사할 {weaponName} 하나 주문하러 왔소.";
            case "귀족":
                return $"[귀족] 품격 있는 {weaponName}, 실망시키지 말게나.";
            case "왕":
                return $"[왕] 나를 위해 {weaponName}을(를) 제작하거라!";
            case "여왕":
                return $"[여왕] {weaponName}, 그대의 솜씨를 기대하겠어요.";
            case "부자":
                return $"[부자] 금은보화로 치르겠소. 최고의 {weaponName} 부탁하지.";
            default:
                return $"[손님] {weaponName}을(를) 제작해 주세요!";
        }
    }

    public void ShowRewardDialog(RecipeData recipe, CustomerType customerType, int gold, int exp, int stressDelta)
    {
        if (recipe == null || customerType == null) return;

        dialogPanel.SetActive(true);
        icon.sprite = recipe.icon;

        string line = GetLeaveLine(customerType.customerTitle, recipe.weaponName, customerType.leaveLine);
        dialogText.text = line;

        OKText.text = "G 키를 눌러 나가기";
        isShowing = true;

        // 보상 행 켜기 + 초기화
        if (rewardRow != null) rewardRow.SetActive(true);
        if (rewardTitleText != null) rewardTitleText.text = "보상 :";

        // 기존 슬롯 정리
        if (rewardContainer != null)
        {
            for (int i = rewardContainer.childCount - 1; i >= 0; i--)
                Destroy(rewardContainer.GetChild(i).gameObject);

            // 돈
            if (goldIcon != null && gold > 0)
                CreateRewardSlot(goldIcon, gold, prefixX: true, color: Color.white);

            // 경험치
            if (expIcon != null && exp > 0)
                CreateRewardSlot(expIcon, exp, prefixX: true, color: Color.white);

            // 스트레스 (감소는 초록, 증가면 빨강)
            if (stressIcon != null && stressDelta != 0)
            {
                Color c = stressDelta < 0 ? new Color(0.2f, 0.9f, 0.3f) : new Color(0.95f, 0.3f, 0.3f);
                // 스트레스는 ±표기로
                CreateRewardSlot(stressIcon, stressDelta, prefixX: false, color: c);
            }
        }
        _ownsModal = true;
        ModalController.Show();
    }

    private string GetLeaveLine(string title, string weaponName, string customLeaveLine)
    {
        // 만약 스크립터블에 문구가 직접 들어있다면 그대로 사용
        if (!string.IsNullOrEmpty(customLeaveLine))
            return $"[{title}] {customLeaveLine}";

        // 아니면 기본 템플릿 대사 fallback
        switch (title)
        {
            case "거지":
                return $"[거지] 이걸로도 충분해… 고맙네…";
            case "상인":
                return $"[상인] 품질이 좋군! 거래가 성사됐소.";
            case "귀족":
                return $"[귀족] 기대 이상이군. 계속 이런 품질을 유지하게.";
            case "왕":
                return $"[왕] 충실히 임무를 수행했군. 내 마음에 들었다.";
            case "여왕":
                return $"[여왕] 아름다운 솜씨네요. 수고하셨어요.";
            case "부자":
                return $"[부자] 훌륭하군! 다음에도 부탁하지.";
            default:
                return $"[손님] 고마워! 다음에도 부탁할게!";
        }
    }

    public void SetAfterClose(System.Action cb)
    {
        _afterClose = cb;
    }

    private void CreateRewardSlot(Sprite sprite, int value, bool prefixX, Color color)
    {
        var go = Instantiate(rewardSlotPrefab, rewardContainer);
        var slot = go.GetComponent<RewardSlotUI>();
        slot.Set(sprite, value, prefixX, color);
    }

    public void HideDialog()
    {
        if (_ownsModal)
        {
            ModalController.Hide();
            _ownsModal = false;
        }

        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        isShowing = false;

        // 위치 원상 복구
        OKText.rectTransform.anchoredPosition = okOriginalPos;

        var cb = _afterClose;
        _afterClose = null;     // 콜백은 한 번만 실행되도록 비워두기
        cb?.Invoke();
    }
}
