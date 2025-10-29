using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CraftingCompletePopup : MonoBehaviour
{
    [Header("UI Reference")]
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text qualityText;      // 품질
    public Button closeBtn;

    private RecipeData _lastRecipe;
    private int _lastQualityScore;
    public bool WentToEnhance { get; private set; }

    private System.Action _onClose;
    public GameObject lockPanel;
    Coroutine _gateLoop;

    bool HasEnchantAccess()
    {
        var cim = FindObjectOfType<CharacterInfoManager>(true);
        return cim != null && cim.CurrentLevel >= 5;
    }

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

        _lastRecipe = recipe;
        _lastQualityScore = score;
        WentToEnhance = false;

        // (2) 데이터 세팅
        icon.sprite = recipe.icon;
        nameText.text = recipe.weaponName;
        string stars = new string('★', starCount) + new string('☆', 5 - starCount);
        qualityText.text = $"품질 : {stars} ({score})";

        _onClose = onClose;

        // (3) 패널 활성화

        gameObject.SetActive(true);
        if (lockPanel) lockPanel.SetActive(!HasEnchantAccess());

        // 활성 동안 주기적으로 갱신
        if (_gateLoop != null) StopCoroutine(_gateLoop);
        _gateLoop = StartCoroutine(GateLoop());

        Debug.Log("[CraftingPopup] Show() 호출됨: " + recipe.weaponName);
    }

    IEnumerator GateLoop()
    {
        while (gameObject.activeSelf)
        {
            if (lockPanel) lockPanel.SetActive(!HasEnchantAccess());
            yield return new WaitForSeconds(0.25f);
        }
    }

    /// <summary>
    /// 제작 완료 버튼 눌렀을 때
    /// </summary>
    /// 
    public void OnClickEnhance()
    {
        if (!HasEnchantAccess())
        {
            if (lockPanel) lockPanel.SetActive(true);
            else Debug.Log("[CraftingPopup] 강화는 Lv.5부터 가능합니다.");
            return;
        }

        EnchantSession.Start(_lastRecipe, _lastQualityScore);
        WentToEnhance = true;

        Close();
    }

    private void Close()
    {
        // 패널만 비활성화
        gameObject.SetActive(false);
        if (_gateLoop != null) { StopCoroutine(_gateLoop); _gateLoop = null; }
        _onClose?.Invoke();
    }
}