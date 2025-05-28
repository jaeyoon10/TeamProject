using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    public static CraftingUI Instance { get; private set; }

    [Header("간단 리스트")]
    public GameObject recipeScrollView;     // RecipeScrollView 루트 오브젝트
    public Transform content;              // Content(=슬롯Container) Transform
    public RecipeSlot slotPrefab;           // RecipeSlot 프리팹
    public RecipeData[] allRecipes;         // 만들어둔 SO 배열

    [Header("상세 뷰")]
    public GameObject detailPanel;     // RecipeDetailPanel
    public Image detailIcon;
    public TMP_Text detailName;
    public TMP_Text levelText;
    public Transform ingContainer;    // IngContainer
    public GameObject ingPrefab;       // IngItem 프리팹

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작 시 두 패널 다 끄기
        recipeScrollView.SetActive(false);
        detailPanel.SetActive(false);
    }

    // 1) “제작” 버튼에 연결
    public void ShowList()
    {
        recipeScrollView.SetActive(true);

        // 기존 슬롯 지우기
        foreach (Transform t in content) Destroy(t.gameObject);

        // SO만큼 슬롯 생성
        foreach (var r in allRecipes)
        {
            var slot = Instantiate(slotPrefab, content);
            slot.Init(r);
        }
    }

    // 2) RecipeSlot 클릭 시 호출
    public void ShowDetail(RecipeData rd)
    {
        detailPanel.SetActive(true);

        // 이미지·이름·레벨
        detailIcon.sprite = rd.icon;
        detailName.text = rd.weaponName;
        levelText.text = "" + rd.requiredLevel;

        // 재료 컨테이너 초기화
        foreach (Transform t in ingContainer) Destroy(t.gameObject);

        // 재료 슬롯 생성
        foreach (var ing in rd.ingredients)
        {
            var go = Instantiate(ingPrefab, ingContainer);
            var item = go.GetComponent<IngItem>();
            item.Init(ing.name, ing.icon, ing.amount);
        }
    }

    public void CloseDetail()
    {
        detailPanel.SetActive(false);
    }

    public void CloseList()
    {
        recipeScrollView.SetActive(false);
    }
}
