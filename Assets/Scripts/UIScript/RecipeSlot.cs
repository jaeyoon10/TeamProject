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

    private RecipeData data;

    // 리스트를 뿌울 때 호출
    public void Init(RecipeData recipe)
    {
        data = recipe;
        iconImage.sprite = recipe.icon;
        nameText.text = recipe.weaponName;

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        CraftingUI.Instance.ShowDetail(data);
    }
}