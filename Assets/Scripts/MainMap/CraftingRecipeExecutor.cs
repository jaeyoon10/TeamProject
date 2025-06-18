using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CraftingRecipeExecutor : MonoBehaviour
{
    public WeaponCraftingManager craftingManager;

    private InventoryUI inventoryUI;
    private CharacterInfoManager characterInfoManager;
    private RecipeData selectedRecipe;

    private IEnumerator Start()
    {
        // UI 씬이 로드될 때까지 대기
        yield return new WaitUntil(() => SceneManager.GetSceneByName("UIScene").isLoaded);

        // 비활성 포함해서 탐색
        yield return new WaitUntil(() =>
        {
            inventoryUI = FindObjectOfType<InventoryUI>(true);
            characterInfoManager = FindObjectOfType<CharacterInfoManager>(true);
            return inventoryUI != null && characterInfoManager != null;
        });

        Debug.Log("[Crafting] UI 컴포넌트 모두 성공적으로 찾음");
    }

    public void SetSelectedRecipe(RecipeData recipe)
    {
        selectedRecipe = recipe;
    }

    public void OnClickCraft()
    {
        if (selectedRecipe == null)
        {
            Debug.LogWarning("[Crafting] 레시피가 선택되지 않음");
            return;
        }

        if (inventoryUI == null || characterInfoManager == null)
        {
            Debug.LogError("[Crafting] InventoryUI 또는 CharacterInfoManager를 찾을 수 없음");
            return;
        }

        // 재료 확인
        if (!InventoryHelper.HasMaterials(inventoryUI.allItems, selectedRecipe))
        {
            Debug.Log("[Crafting] 재료가 부족합니다");
            return;
        }

        // 재료 차감
        InventoryHelper.ConsumeMaterials(inventoryUI.allItems, selectedRecipe);
        inventoryUI.Refresh();

        // 제작 시작
        craftingManager.StartCrafting(selectedRecipe);
    }
}