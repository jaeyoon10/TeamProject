using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvnetoryOpen : MonoBehaviour
{
    public Button InventoryOpenButton;   // “상점 열기” 버튼
    public GameObject InventoryPanel;    // StorePanel(GameObject)
    public Button InventoryCloseButton;  // “X” 버튼

    void Start()
    {
        // 시작할 때 StorePanel을 꺼 두고,
        if (InventoryPanel.activeSelf)
            InventoryPanel.SetActive(false);

        // 열기 버튼 누르면 ShowStorePanel() 호출
        InventoryOpenButton.onClick.AddListener(ShowInventoryPanel);

        // 닫기 버튼 누르면 CloseStorePanel() 호출
        InventoryCloseButton.onClick.AddListener(CloseInventoryPanel);
    }

    public void ShowInventoryPanel()
    {
        InventoryPanel.SetActive(true);
    }

    public void CloseInventoryPanel()
    {
        InventoryPanel.SetActive(false);
    }
}
