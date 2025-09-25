using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class RecipeSlot : MonoBehaviour
{
    public Image iconImage;
    public TMP_Text nameText;
    public GameObject lockOverlay;
    private RecipeData data;

    // 리스트를 뿌울 때 호출
    public void Init(RecipeData recipe)
    {
        data = recipe;
        iconImage.sprite = recipe.icon;
        nameText.text = recipe.weaponName;

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();

        int currentLevel = CharacterInfoManager.Instance.CurrentLevel;
        if (currentLevel >= recipe.requiredLevel)
        {
            btn.interactable = true;
            iconImage.color = Color.white;
            nameText.color = Color.white;
            if (lockOverlay != null) lockOverlay.SetActive(false);

            btn.onClick.AddListener(OnClick);
        }
        else
        {
            btn.interactable = false;
            iconImage.color = Color.gray;
            nameText.color = Color.gray;
            nameText.text = $"{recipe.weaponName}(Lv.{recipe.requiredLevel} 해금)";
            if (lockOverlay != null) lockOverlay.SetActive(true);
        }
    }

    private void OnClick()
    {
        CraftingUI.Instance.ShowDetail(data);
    }
}