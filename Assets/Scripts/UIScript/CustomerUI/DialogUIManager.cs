using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogUIManager : MonoBehaviour
{
    public static DialogUIManager Instance { get; private set; }

    [Header("Dialog UI")]
    public GameObject dialogPanel;
    public Image icon;
    public TMP_Text dialogText;
    public TMP_Text OKText;


    public float bounceHeight = 14f;  // 위아래 이동 높이
    public float bounceSpeed = 2f;    // 이동 속도
    private Vector3 okOriginalPos;
    private bool isShowing = false;

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
            case "부자 손님":
                return $"[부자 손님] 금은보화로 치르겠소. 최고의 {weaponName} 부탁하지.";
            default:
                return $"[손님] {weaponName}을(를) 제작해 주세요!";
        }
    }

    public void HideDialog()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        isShowing = false;

        // 위치 원상 복구
        OKText.rectTransform.anchoredPosition = okOriginalPos;
    }
}
