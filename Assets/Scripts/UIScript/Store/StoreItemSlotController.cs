using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemSlotController : MonoBehaviour
{
    [Header("UI 참조 (Inspector에서 반드시 할당)")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text amountText;
    public Button purchaseButton;
    public Image backgroundImage;

    [Header("효과음")]
    public AudioClip purchaseSound;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [Header("효과별 배경색")]
    public Color colorNone = new Color(0.9f, 0.9f, 0.9f);
    public Color colorEasyMiniGame = new Color(0.7f, 0.5f, 0.8f);
    public Color colorNoBelowB = new Color(0.4f, 0.6f, 1.0f);

    private StoreItemData data;
    private InventoryUI inventoryUI;
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;

    public CharacterInfoManager characterInfoManager;

    private void Awake()
    {
        inventoryUI = FindObjectOfType<InventoryUI>(true);
        characterInfoManager = FindObjectOfType<CharacterInfoManager>(true);

        // UI 사운드 전용 AudioSource 생성
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D 사운드
        audioSource.volume = 1f;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Init(StoreItemData itemData)
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        data = itemData;

        iconImage.sprite = data.icon;
        iconImage.preserveAspect = true;
        nameText.text = data.itemName;
        priceText.text = data.price.ToString();

        if (data.amount > 0)
        {
            amountText.gameObject.SetActive(true);
            amountText.text = "수량: " + data.amount;

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            purchaseButton.interactable = true;
        }
        else
        {
            amountText.gameObject.SetActive(false);
            canvasGroup.alpha = 1f;
            purchaseButton.interactable = true;
        }

        switch (data.effectType)
        {
            case EffectType.EasyMiniGame:
                backgroundImage.color = colorEasyMiniGame;
                break;
            case EffectType.NoBelowBQuality:
                backgroundImage.color = colorNoBelowB;
                break;
            default:
                backgroundImage.color = colorNone;
                break;
        }

        purchaseButton.onClick.RemoveAllListeners();
        purchaseButton.onClick.AddListener(OnClickPurchase);
    }

    private void OnClickPurchase()
    {
        PlaySFX(purchaseSound);

        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>(true);
        if (characterInfoManager == null)
            characterInfoManager = FindObjectOfType<CharacterInfoManager>(true);

        if (!MoneyManager.Instance.SpendGold(data.price))
        {
            Debug.Log("[Store] 잔액 부족: 구매 불가");
            return;
        }

        if (data.amount > 0)
        {
            data.amount--;
            if (data.amount > 0)
            {
                amountText.text = $"수량: {data.amount}";
            }
            else
            {
                amountText.gameObject.SetActive(false);
                DisableAsSoldOut();
            }
        }

        characterInfoManager?.AddStoreCost(data.price);

        ItemCategory inventoryCat = ItemCategory.Weapon;
        if (data.itemCategory == StoreItemCategory.Enhance)
            inventoryCat = ItemCategory.Enhancement;
        else if (data.itemCategory == StoreItemCategory.TodaySpecial)
            inventoryCat = (data.baseCategory == StoreItemCategory.Material)
                           ? ItemCategory.Weapon
                           : ItemCategory.Enhancement;

        inventoryUI.AddOrIncreaseItem(data.icon, inventoryCat, data.effectType);
    }

    private void DisableAsSoldOut()
    {
        canvasGroup.alpha = 0.5f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        purchaseButton.interactable = false;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, sfxVolume);
    }
}