using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewRecipeData", menuName = "Crafting/Recipe Data")]
public class RecipeData : ScriptableObject
{
    public string weaponName;
    public Sprite icon;           // 중앙에 크게 띄울 이미지
    public int baseXP;
    public int requiredLevel;     // Lv 텍스트용
    public int xpReward;

    [System.Serializable]
    public struct Ingredient
    {
        public string name;
        public Sprite icon;
        public int amount;
    }

    public List<Ingredient> ingredients = new List<Ingredient>();
}
