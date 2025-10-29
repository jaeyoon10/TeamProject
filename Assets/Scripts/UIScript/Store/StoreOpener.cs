using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreOpener : MonoBehaviour
{
    public Button storeOpenButton;   // “상점 열기” 버튼
    public GameObject storePanel;    // StorePanel(GameObject)
    public Button storeCloseButton;  // “X” 버튼

    void Start()
    {
        // 시작할 때 StorePanel을 꺼 두고,
        if (storePanel.activeSelf)
            storePanel.SetActive(false);

        // 열기 버튼 누르면 ShowStorePanel() 호출
        storeOpenButton.onClick.AddListener(ShowStorePanel);

        // 닫기 버튼 누르면 CloseStorePanel() 호출
        storeCloseButton.onClick.AddListener(CloseStorePanel);
    }

    public void ShowStorePanel()
    {
        storePanel.SetActive(true);
    }

    public void CloseStorePanel()
    {
        storePanel.SetActive(false);
    }
}