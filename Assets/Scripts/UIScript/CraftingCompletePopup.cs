using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingCompletePopup : MonoBehaviour
{
    [Header("UI Reference")]
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text qualityText;      // "품질 : ★★★☆☆" 같은 텍스트
    public Button closeBtn;

    private System.Action _onClose;

    private void Awake()
    {
        if (closeBtn == null)
            closeBtn = transform.Find("CloseButton")?.GetComponent<Button>();

        if (closeBtn != null)
            closeBtn.onClick.AddListener(Close);
        else
            Debug.LogError("[CraftingPopup] CloseButton 연결 필요!");

        // 패널 자체만 비활성화
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 팝업을 띄울 때
    /// </summary>
    public void Show(RecipeData recipe, int score, int starCount, System.Action onClose)
    {
        // (1) 최상위로 끌어올려 다른 UI 위에 표시
        transform.SetAsLastSibling();

        // (2) 데이터 세팅
        icon.sprite = recipe.icon;
        nameText.text = recipe.weaponName;
        string stars = new string('★', starCount) + new string('☆', 5 - starCount);
        qualityText.text = $"품질 : {stars} ({score})";

        _onClose = onClose;

        // (3) 패널 활성화
        gameObject.SetActive(true);

        Debug.Log("[CraftingPopup] Show() 호출됨: " + recipe.weaponName);
    }

    /// <summary>
    /// 제작 완료 버튼 눌렀을 때
    /// </summary>
    private void Close()
    {
        // 패널만 비활성화
        gameObject.SetActive(false);

        _onClose?.Invoke();
    }
}