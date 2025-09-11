using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class WeaponCraftingManager : MonoBehaviour
{
    /* --------------------------------------------------
       INSPECTOR FIELDS
    --------------------------------------------------*/

    [Header("Customer Spawner")]
    public CustomerSpawner customerSpawner;  // 인스펙터에 연결!
    private CustomerController _currentCustomer;

    [Header("작업대 앞 위치 (빈 오브젝트)")]
    public Transform heatPosition;
    public Transform hammerPosition;
    public Transform polishPosition;

    [Header("제작 캐릭터")]
    public GameObject player;
    public float moveSpeed = 2f;

    [Header("UI 연결 (제작 완료 팝업)")]
    public CraftingCompletePopup completePopup;   // 별·이름·아이콘 표시용 UI

    [Header("스트레스 시스템")]
    public int currentStress = 0;
    public int maxStress = 10;
    public System.Action<int> OnStressChanged;

    [Header("문(손님 위치) : 완료 시 이동할 곳")]
    public Transform doorPosition;

    /* --------------------------------------------------
       내부 변수
    --------------------------------------------------*/
    private Queue<IEnumerator> craftingSteps;
    private int qualityScore;             // 0~100

    /* ==================================================
       PUBLIC ENTRY POINT
    ==================================================*/

    public static WeaponCraftingManager Instance { get; private set; }  // ← 추가

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

    }
    public void OnCustomerArrived(CustomerController customer)
    {
        _currentCustomer = customer;
    }

    public void StartCrafting(RecipeData recipe)
    {
        // 1) CharacterInfoManager 찾아서
        var info = FindObjectOfType<CharacterInfoManager>();
        if (info == null)
            Debug.LogError("CharacterInfoManager가 없습니다!");
        else
        {
            // 2) PlayerPrefs에 저장된 키로 어떤 캐릭터인지 확인
            string sel = PlayerPrefs.GetString("SelectedCharacter", "");
            switch (sel)
            {
                case "Edwin":
                    info.SetCharacterPortrait(info.edwinPortrait);
                    break;
                case "Isabella":
                    info.SetCharacterPortrait(info.isabellaPortrait);
                    break;
                case "Tusk":
                    info.SetCharacterPortrait(info.tuskPortrait);
                    break;
            }
        }

        // CharacterJLoader 가 Start() 에서 태그를 붙인 뒤라면
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
                Debug.LogError("[Crafting] Player 태그 달린 오브젝트를 찾지 못했습니다!");
        }

        // 혹시 Awake 전에 호출될 수 있다면, 여기서도 한 번 안전하게 확보
        if (completePopup == null)
        {
            completePopup = FindObjectOfType<CraftingCompletePopup>(true);
            if (completePopup == null)
                Debug.LogError("[Crafting] CraftingCompletePopup을 찾을 수 없습니다!");
        }

        qualityScore = 100;               // 점수 초기화

        craftingSteps = new Queue<IEnumerator>();
        craftingSteps.Enqueue(HandleHeatStep());
        craftingSteps.Enqueue(HandleHammerStep());
        craftingSteps.Enqueue(HandlePolishStep());

        StartCoroutine(ProcessCrafting(recipe));
    }

    private IEnumerator Start()
    {
        // UIScene이 로드될 때까지 대기
        yield return new WaitUntil(() => SceneManager.GetSceneByName("UIScene").isLoaded);

        // CraftingCompletePopup 찾기 (비활성 포함)
        yield return new WaitUntil(() =>
        {
            completePopup = FindObjectOfType<CraftingCompletePopup>(true);
            return completePopup != null;
        });

        Debug.Log("[Crafting] CraftingCompletePopup 연결 완료");

        customerSpawner.SpawnCustomer();
    }

    /* ==================================================
       메인 제작 루프
    ==================================================*/
    private IEnumerator ProcessCrafting(RecipeData recipe)
    {
        while (craftingSteps.Count > 0)
            yield return StartCoroutine(craftingSteps.Dequeue());

        yield return new WaitForSeconds(0.3f);

        int star = CalcStar(qualityScore);

        //    이전에는 completePopup.Show(..., ()=>{ ... }) 였던 걸 제거하고
        //    새 코루틴으로 교체
        StartCoroutine(OnCraftingComplete(recipe, qualityScore, star));  // ← 변경
        
    }

    /* ==================================================
       각 스텝 코루틴
    ==================================================*/
    //----------------------------------------------------------------
    private IEnumerator HandleHeatStep()
    {
        // 이동 & 애니
        yield return StartCoroutine(MoveTo(heatPosition.position));
        PlayWorkAnim();

        bool done = false;
        float t0 = Time.time;

        yield return StartCoroutine(LoadMiniGameScene(
            "MiniGameFire",
            slider => slider.onMiniGameSuccess += () => done = true
        ));

        // 성공 콜백이 불린 후 품질 감점
        yield return new WaitUntil(() => done);
        float elapsed = Time.time - t0;
        if (elapsed >= 11f) qualityScore -= 25;
        else if (elapsed >= 8f) qualityScore -= 15;
        else if (elapsed >= 5f) qualityScore -= 10;
    }

    // 2) 해머 스텝
    private IEnumerator HandleHammerStep()
    {
        // 1) 자리로 이동
        yield return StartCoroutine(MoveTo(hammerPosition.position));
        PlayWorkAnim();

        // 2) 미니게임 씬 로드 (콜백은 null)
        yield return StartCoroutine(LoadMiniGameScene("MinigameHammerHIt", null));

        // 3) 씬이 언로드된 뒤 FindObjectOfType 로 직접 꺼내오기
        var mini = FindObjectOfType<HammerMiniGame>(true);
        if (mini == null)
        {
            Debug.LogError("[Crafting] HammerMiniGame 을 씬에서 찾지 못했습니다!");
        }
        else
        {
            Debug.Log($"[Crafting] Hammer failCount={mini.failCount}, perfectCount={mini.perfectCount}");
        }

        // 4) 품질 계산
        int fails = mini ? mini.failCount : 0;
        int perfect = mini ? mini.perfectCount : 0;

        if (fails == 1) qualityScore -= 7;
        else if (fails == 2) qualityScore -= 16;
        else if (fails >= 3) qualityScore -= 30;

        qualityScore += perfect * 5;
    }

    // 3) 연마 스텝
    private IEnumerator HandlePolishStep()
    {
        yield return StartCoroutine(MoveTo(polishPosition.position));
        PlayWorkAnim();

        yield return StartCoroutine(LoadMiniGameScene("MiniGameRub", null));

        var mini = FindObjectOfType<SharpeningSwipeGame>(true);
        if (mini == null)
        {
            Debug.LogError("[Crafting] SharpeningSwipeGame 을 씬에서 찾지 못했습니다!");
        }
        else
        {
            Debug.Log($"[Crafting] Sharpening failCount={mini.failCount}");
        }

        int fails = mini ? mini.failCount : 0;
        qualityScore -= fails * 20;
    }

    /* ==================================================
       공통 유틸
    ==================================================*/
    private void PlayWorkAnim()
    {
        var anim = player.GetComponent<Animator>();
        if (anim) anim.SetTrigger("Work");
    }

    //----------------------------------------------------------------
    private IEnumerator MoveTo(Vector3 target)
    {
        while (Vector3.Distance(player.transform.position, target) > 0.05f)
        {
            Vector3 dir = (target - player.transform.position).normalized;
            player.transform.forward = new Vector3(dir.x, 0, dir.z);
            player.transform.position = Vector3.MoveTowards(player.transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    //----------------------------------------------------------------
    private IEnumerator LoadMiniGameScene(
    string sceneName,
    System.Action<SliderController> onSliderLoaded = null,
    System.Action<HammerMiniGame> onHammerLoaded = null,
    System.Action<SharpeningSwipeGame> onPolishLoaded = null)
    {
        // 1. Additive 로드
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!op.isDone) yield return null;
        yield return null; // 1프레임 유예

        // 2. 씬의 루트 오브젝트 전부 가져오기
        var miniScene = SceneManager.GetSceneByName(sceneName);
        var roots = miniScene.GetRootGameObjects();

        SliderController sliderCtrl = null;
        HammerMiniGame hammerCtrl = null;
        SharpeningSwipeGame polishCtrl = null;

        foreach (var root in roots)
        {
            // 혹시 중복 EventSystem이 있으면 제거
            var ev = root.GetComponent<EventSystem>();
            if (ev) Destroy(ev.gameObject);

            if (sliderCtrl == null)
                sliderCtrl = root.GetComponentInChildren<SliderController>(true);
            if (hammerCtrl == null)
                hammerCtrl = root.GetComponentInChildren<HammerMiniGame>(true);
            if (polishCtrl == null)
                polishCtrl = root.GetComponentInChildren<SharpeningSwipeGame>(true);
        }

        if (sliderCtrl == null && onSliderLoaded != null)
            Debug.LogError($"[Crafting] {sceneName} 씬에서 SliderController를 찾지 못함!");
        if (hammerCtrl == null && onHammerLoaded != null)
            Debug.LogError($"[Crafting] {sceneName} 씬에서 HammerMiniGame을 찾지 못함!");
        if (polishCtrl == null && onPolishLoaded != null)
            Debug.LogError($"[Crafting] {sceneName} 씬에서 SharpeningSwipeGame을 찾지 못함!");

        // 3. 타입별 콜백 연결
        if (sliderCtrl != null && onSliderLoaded != null) { onSliderLoaded(sliderCtrl); }
        if (hammerCtrl != null && onHammerLoaded != null) { onHammerLoaded(hammerCtrl); }
        if (polishCtrl != null && onPolishLoaded != null) { onPolishLoaded(polishCtrl); }

        // 4. 내부 스크립트가 씬을 언로드할 때까지 대기
        yield return new WaitUntil(() => !SceneManager.GetSceneByName(sceneName).isLoaded);
    }
    private IEnumerator OnCraftingComplete(RecipeData recipe, int quality, int star)
    {
        bool popupClosed = false;
        completePopup.Show(
            recipe,
            quality,
            star,
            () => popupClosed = true
        );
        yield return new WaitUntil(() => popupClosed);

        StartCoroutine(MoveTo(doorPosition.position));

        // 2) 지금 문 앞에 대기 중인 손님에게 무기 전달
        if (_currentCustomer != null)
                {
            _currentCustomer.ServeWeapon(quality);
                }
    }
    //----------------------------------------------------------------
    private int CalcStar(int score)
    {
        if (score >= 80) return 5;
        if (score >= 50) return 4;
        if (score >= 30) return 3;
        if (score >= 10) return 2;
        return 1;
    }

}

