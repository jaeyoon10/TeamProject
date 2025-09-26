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

    private void Update()
    {
        if (!isShowing) return;

        if (Input.GetKeyDown(KeyCode.G) || Input.GetMouseButtonDown(0))
        {
            HideDialog();
        }
    }
    public void ShowDialog(RecipeData recipe)
    {
        if (recipe == null) return;

        dialogPanel.SetActive(true);
        icon.sprite = recipe.icon;
        dialogText.text = $"[손님] {recipe.weaponName}을(를) 제작해 주세요!";

        isShowing = true;
    }

    public void HideDialog()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        isShowing = false;
    }
}
