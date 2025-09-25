using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewRecipeData", menuName = "Crafting/Recipe Data")]
public class RecipeData : ScriptableObject
{
    public string weaponName;
    public Sprite icon;           // 중앙에 크게 띄울 이미지
    public int baseXP;            // 제작 시 소모되거나 기준이 되는 경험치
    public int requiredLevel;     // Lv 텍스트용
    public int xpReward;          // 제작 성공 시 얻는 경험치
    public int basePrice;         // 무기 기본 가격

    [System.Serializable]
    public struct Ingredient
    {
        public string name;
        public Sprite icon;
        public int amount;
    }

    public List<Ingredient> ingredients = new List<Ingredient>();
}
