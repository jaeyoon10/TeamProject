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
    public void ShowDialog(RecipeData recipe)
    {
        if (recipe == null) return;
            dialogPanel.SetActive(true);
        icon.sprite = recipe.icon;
        dialogText.text = $"[손님] {recipe.weaponName}을(를) 제작해 주세요!";

        OKText.text = $"G 키를 눌러 나가기";

        isShowing = true;
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
