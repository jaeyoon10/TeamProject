using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    public RectTransform banner;   // "Level UP!!!" 배너 (중앙에서 시작)
    public RectTransform content;  // 네모 박스 (처음엔 scaleY = 0)
    public float moveUpDistance = 100f;
    public float moveDuration = 0.5f;
    public float expandDuration = 0.5f;

    [Header("확인 버튼 연결")]
    public Button confirmButton;

    [Header("해금 리스트")]
    public Transform unlockListParent;
    public GameObject unlockSlotPrefab;

    [Header("데이터베이스 연결")]
    public UnlockDB unlockDB;

    private Vector2 bannerStartPos;

    void Awake()
    {
        // 시작 위치 저장
        bannerStartPos = banner.anchoredPosition;
        content.localScale = new Vector3(1, 0, 1); // 접힌 상태
        gameObject.SetActive(false);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);
    }

    public void ShowLevelUp(int level)
    {

        gameObject.SetActive(true);
        StartCoroutine(PlayAnimation(level));

        ModalController.Show();
    }

    private IEnumerator PlayAnimation(int level)
    {
        // 해금 슬롯 비우기 
        foreach (Transform child in unlockListParent)
            Destroy(child.gameObject);

        // Step 1: 배너 먼저 등장 
        banner.anchoredPosition = bannerStartPos;
        content.localScale = new Vector3(1, 0, 1);

        yield return new WaitForSeconds(0.5f);

        // Step 2: 배너 위로 이동
        float elapsed = 0f;
        Vector2 targetPos = bannerStartPos + new Vector2(0, moveUpDistance);

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            banner.anchoredPosition = Vector2.Lerp(bannerStartPos, targetPos, t);
            yield return null;
        }
        banner.anchoredPosition = targetPos;

        // Step 3: 네모 박스 펼치기
        elapsed = 0f;
        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expandDuration;
            content.localScale = new Vector3(1, Mathf.Lerp(0, 1, t), 1);
            yield return null;
        }
        content.localScale = Vector3.one;

        // Step 4: 해금 리스트 표시
        var unlockedItems = unlockDB.GetItemsForLevel(level);
        if (unlockedItems.Count > 0)
            yield return StartCoroutine(ShowUnlockedItems(unlockedItems));
    }

    private IEnumerator ShowUnlockedItems(List<UnlockItemSO> unlockedItems)
    {
        foreach (Transform child in unlockListParent)
            Destroy(child.gameObject);

        foreach (var item in unlockedItems)
        {
            var slot = Instantiate(unlockSlotPrefab, unlockListParent);

            var iconObj = slot.transform.Find("Icon");
            var textObj = slot.GetComponentInChildren<TMP_Text>();

            if (iconObj == null) Debug.LogError("[LevelUpUI] 프리팹 안에 'Icon' 없음!");
            if (textObj == null) Debug.LogError("[LevelUpUI] 프리팹 안에 'Text' 없음!");

            var img = iconObj?.GetComponent<Image>();
            var txt = textObj?.GetComponent<TMPro.TMP_Text>();

            if (img) img.sprite = item.icon;
            if (txt) txt.text = item.description;

            // 도장 애니메이션
            slot.transform.localScale = Vector3.one * 0.8f;
            var cg = slot.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = slot.AddComponent<CanvasGroup>();
                Debug.Log("[LevelUpUI] CanvasGroup 자동 추가");
            }
            cg.alpha = 0;

            float t = 0, duration = 0.2f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                slot.transform.localScale = Vector3.one * Mathf.Lerp(1.2f, 1f, p);
                cg.alpha = p;
                yield return null;
            }

            slot.transform.localScale = Vector3.one;
            cg.alpha = 1f;

            yield return new WaitForSeconds(0.2f);
        }
    }


    public void OnConfirm()
    {
        // 확인 버튼 누르면 창 닫기
        gameObject.SetActive(false);
        Debug.Log("[LevelUpUI] 확인 버튼 클릭 → 창 닫힘");

        ModalController.Hide();
    }
}