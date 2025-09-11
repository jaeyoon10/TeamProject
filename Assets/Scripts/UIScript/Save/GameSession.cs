using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    private CharacterInfoManager _info;
    private InventoryUI _inv;
    private StoreSlotLoader _store;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 씬이 바뀔 때마다 매니저 참조를 다시 찾아 줍니다
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 타이틀에서 continue/newGame 버튼이 LoadFlag를 세팅해 주기 때문에
        // 여기에 따라 불러오기를 실행하거나 완전 새 게임으로 초기화합니다.
        if (PlayerPrefs.GetInt("LoadFlag", 0) == 1)
            LoadState();
        else
            StartNewGame();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // CharacterSelect을 지나 Ingame_main이 로드되면 여기에 매니저들이 생깁니다
        _info = FindObjectOfType<CharacterInfoManager>();
        _inv = FindObjectOfType<InventoryUI>();
        _store = FindObjectOfType<StoreSlotLoader>();
    }
    /// <summary>
    /// 현재 게임 상태를 파일로 저장합니다.
    /// </summary>
    public void SaveState()
    {
        var info = FindObjectOfType<CharacterInfoManager>();
        var inv = FindObjectOfType<InventoryUI>();
        var storeLoader = FindObjectOfType<StoreSlotLoader>();

        // 저장에 필요한 컴포넌트가 모두 있는지 확인
        if (info == null || inv == null || storeLoader == null)
        {
            Debug.LogWarning("SaveState: 필수 매니저를 찾을 수 없어 저장을 건너뜁니다.");
            return;
        }

        var data = new SaveData
        {
            currentDay = info.CurrentDay,
            currentLevel = info.CurrentLevel,
            currentXP = info.CurrentXP,
            currentStress = info.CurrentStress,
            inventoryItems = inv.allItems.Select(x => new InventoryItemData
            {
                iconName = x.icon.name,
                category = x.category.ToString(),
                effectType = x.effectType.ToString(),
                quantity = x.quantity,
                acquireIndex = x.acquireIndex
            }).ToList(),
            // 상점 상태 저장
            specialOffers = storeLoader.CurrentSpecialOffers.Select(i => new StoreItemRecord
            {
                iconName = i.icon.name,
                baseCategory = i.baseCategory.ToString(),
                itemCategory = i.itemCategory.ToString(),
                price = i.price,
                amount = i.amount,
                effectType = i.effectType.ToString()
            }).ToList(),
            materialItems = storeLoader.CurrentMaterialItems.Select(i => new StoreItemRecord
            {
                iconName = i.icon.name,
                baseCategory = i.baseCategory.ToString(),
                itemCategory = i.itemCategory.ToString(),
                price = i.price,
                amount = i.amount,
                effectType = i.effectType.ToString()
            }).ToList(),
            enhanceItems = storeLoader.CurrentEnhanceItems.Select(i => new StoreItemRecord
            {
                iconName = i.icon.name,
                baseCategory = i.baseCategory.ToString(),
                itemCategory = i.itemCategory.ToString(),
                price = i.price,
                amount = i.amount,
                effectType = i.effectType.ToString()
            }).ToList(),
            todayItems = storeLoader.CurrentTodayItems.Select(i => new StoreItemRecord
            {
                iconName = i.icon.name,
                baseCategory = i.baseCategory.ToString(),
                itemCategory = i.itemCategory.ToString(),
                price = i.price,
                amount = i.amount,
                effectType = i.effectType.ToString()
            }).ToList()
        };

        SaveManager.SaveGame(data);
        Debug.Log("GameSession: 게임 상태가 저장되었습니다.");
    }

    /// <summary>
    /// 저장된 게임 상태를 파일에서 불러와 적용합니다.
    /// </summary>
    void LoadState()
    {
        var data = SaveManager.LoadGame();
        if (data == null)
        {
            Debug.LogWarning("LoadState: 저장된 데이터가 없어 불러오기를 건너뜁니다.");
            return;
        }

        var info = FindObjectOfType<CharacterInfoManager>();
        var inv = FindObjectOfType<InventoryUI>();
        var storeLoader = FindObjectOfType<StoreSlotLoader>();

        if (info == null || inv == null || storeLoader == null)
        {
            Debug.LogError("LoadState: 필수 매니저를 찾을 수 없습니다.");
            return;
        }

        // 캐릭터 초상화 설정
        switch (PlayerPrefs.GetString("SelectedCharacter", ""))
        {
            case "Edwin": info.SetCharacterPortrait(info.edwinPortrait); break;
            case "Isabella": info.SetCharacterPortrait(info.isabellaPortrait); break;
            case "Tusk": info.SetCharacterPortrait(info.tuskPortrait); break;
            default: Debug.LogWarning("선택된 캐릭터 정보가 없습니다."); break;
        }

        // 레벨/XP/날짜/스트레스 로드
        info.SetLevel(data.currentLevel, data.currentXP);
        info.SetDay(data.currentDay);
        info.SetStress(data.currentStress);

        // 인벤토리 로드
        inv.allItems = data.inventoryItems.Select(d => new InventoryItem
        {
            icon = Resources.Load<Sprite>(d.iconName),
            category = System.Enum.Parse<ItemCategory>(d.category),
            effectType = System.Enum.Parse<EffectType>(d.effectType),
            quantity = d.quantity,
            acquireIndex = d.acquireIndex
        }).ToList();
        inv.Refresh();

        // 상점 상태 로드
        storeLoader.LoadFromSave(
            data.specialOffers,
            data.materialItems,
            data.enhanceItems,
            data.todayItems
        );

        Debug.Log("GameSession: 저장된 상태가 성공적으로 로드되었습니다.");
    }

    /// <summary>
    /// 새 게임을 시작하며, 기존 저장 파일을 삭제합니다.
    /// </summary>
    void StartNewGame()
    {
        SaveManager.DeleteSave(); // 이전 저장 제거
        Debug.Log("GameSession: 새로운 게임을 시작합니다.");
    }
}
