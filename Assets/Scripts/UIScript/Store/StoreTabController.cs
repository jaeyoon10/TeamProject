using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreTabController : MonoBehaviour
{
    [Header("Tab Buttons")]
    public Button materialStoreButton; // 재료 상점 버튼
    public Button enhanceStoreButton;  // 강화 상점 버튼
    public Button todayItemButton;     // 오늘의 상품 버튼

    [Header("Tab Panels")]
    public GameObject materialStorePanel; // 재료 상점 패널
    public GameObject enhanceStorePanel;  // 강화 상점 패널
    public GameObject todayItemPanel;     // 오늘의 상품 패널

    [Header("Tab Button Images (for color)")]
    public Image materialStoreImage; // 재료 상점 버튼의 Image 컴포넌트
    public Image enhanceStoreImage;  // 강화 상점 버튼의 Image 컴포넌트
    public Image todayItemImage;     // 오늘의 상품 버튼의 Image 컴포넌트

    [Header("Button Colors")]
    public Color activeColor = new Color(234f / 255f, 214f / 255f, 173f / 255f); // 선택 상태 색 (#EAD6AD)
    public Color inactiveColor = Color.white;                               // 기본 상태 색 (흰색)

    void Start()
    {
        // 버튼 클릭 시 해당 함수를 호출하도록 리스너 연결
        materialStoreButton.onClick.AddListener(ShowMaterialStore);
        enhanceStoreButton.onClick.AddListener(ShowEnhanceStore);
        todayItemButton.onClick.AddListener(ShowTodayItem);

        // 초기에는 재료 상점이 활성화되어 보이고 버튼도 활성화된 색으로 세팅
        ShowMaterialStore();
    }

    // 재료 상점 탭을 열 때
    void ShowMaterialStore()
    {
        // 패널 전환
        materialStorePanel.SetActive(true);
        enhanceStorePanel.SetActive(false);
        todayItemPanel.SetActive(false);

        // 버튼 색 전환
        materialStoreImage.color = activeColor;
        enhanceStoreImage.color = inactiveColor;
        todayItemImage.color = inactiveColor;
    }

    // 강화 상점 탭을 열 때
    void ShowEnhanceStore()
    {
        materialStorePanel.SetActive(false);
        enhanceStorePanel.SetActive(true);
        todayItemPanel.SetActive(false);

        materialStoreImage.color = inactiveColor;
        enhanceStoreImage.color = activeColor;
        todayItemImage.color = inactiveColor;
    }

    // 오늘의 상품 탭을 열 때
    void ShowTodayItem()
    {
        materialStorePanel.SetActive(false);
        enhanceStorePanel.SetActive(false);
        todayItemPanel.SetActive(true);

        materialStoreImage.color = inactiveColor;
        enhanceStoreImage.color = inactiveColor;
        todayItemImage.color = activeColor;
    }
}