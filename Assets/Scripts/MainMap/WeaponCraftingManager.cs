using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WeaponCraftingManager : MonoBehaviour
{
    [Header("Customer/Player")]
    public CustomerSpawner customerSpawner;
    public GameObject player;
    public float moveSpeed = 2f;

    [Header("Anchors")]
    public Transform heatPosition;
    public Transform hammerPosition;
    public Transform grindingPosition;
    public Transform doorPosition;

    [Header("Result UI")]
    public CraftingCompletePopup completePopup;

    [Header("Camera Swap")]
    public CameraSwap camSwap; // MainCamera에 붙은 컴포넌트

    [Header("MiniGame Scene Names")]
    public string furnaceSceneName = "MiniGameBrasier";
    public string anvilSceneName = "MiniGameHammer";
    public string grindingSceneName = "MiniGameGrinding";

    [Header("Quality Weights")]
    public int baseQuality = 0;
    public int wPerfect = +10;
    public int wGreat = +7;
    public int wGood = +5;
    public int wMiss = -10;


    [System.Serializable]
    public class MiniSceneBinding
    {
        public GameObject root;  // 모듈 루트 (예: FurnaceModule, AnvilModule)
        public Camera cam;       // 미니게임 전용 카메라
        public Canvas ui;        // 미니게임 UI(Canvas)
    }

    private GradeCounts _totalCounts;

    private Queue<IEnumerator> craftingSteps;
    private int qualityScore;
    private CustomerController _currentCustomer;

    public static WeaponCraftingManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    IEnumerator Start()
    {
        MiniGameState.ResetAll();

        if (!player) player = GameObject.FindWithTag("Player");
        if (!completePopup) completePopup = FindObjectOfType<CraftingCompletePopup>(true);
        if (!customerSpawner) customerSpawner = FindObjectOfType<CustomerSpawner>();

        yield return new WaitUntil(() => customerSpawner != null && customerSpawner.IsReady);

        // 준비된 뒤 스폰 (한 번 더 안전)
        yield return StartCoroutine(customerSpawner.SpawnWhenReady());
    }

    public void OnCustomerArrived(CustomerController c) => _currentCustomer = c;

    public void StartCrafting(RecipeData recipe)
    {
        _totalCounts = default;     //  합산 카운트 초기화
        qualityScore = baseQuality; //  시작점은 baseQuality

        var info = FindObjectOfType<CharacterInfoManager>(); 
        if (info == null) 
            Debug.LogError("CharacterInfoManager가 없습니다!"); 
        else 
        { 
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
        if (player == null) 
        { 
            player = GameObject.FindWithTag("Player"); 
            if (player == null) 
                Debug.LogError("[Crafting] Player 태그 달린 오브젝트를 찾지 못했습니다!"); 
        } 
        if (completePopup == null) 
        { 
            completePopup = FindObjectOfType<CraftingCompletePopup>(true); 
            if (completePopup == null) 
                Debug.LogError("[Crafting] CraftingCompletePopup을 찾을 수 없습니다!"); 
        }

        // 🔹 제작 단계 등록
        craftingSteps = new Queue<IEnumerator>();
        craftingSteps.Enqueue(HandleHeatStep(recipe));
        craftingSteps.Enqueue(HandleHammerStep());
        craftingSteps.Enqueue(HandleGrindingStep()); 

        StartCoroutine(ProcessCrafting(recipe));
    }


    int CalcQualityFromCounts(GradeCounts c)
    {
        int q = baseQuality
              + c.perfect * wPerfect
              + c.great * wGreat
              + c.good * wGood
              + c.miss * wMiss;
        Debug.Log($"[Craft] P:{c.perfect * wPerfect} G:{c.great * wGreat} D:{c.good * wGood} M:{c.miss * wMiss} RawScore={q}");
        return Mathf.Clamp(q, 0, 100);
    }

    IEnumerator ProcessCrafting(RecipeData recipe)
        {

        while (craftingSteps.Count > 0)
            yield return StartCoroutine(craftingSteps.Dequeue());

        yield return new WaitForSeconds(0.2f);
        qualityScore = CalcQualityFromCounts(_totalCounts);
        Debug.Log($"[Craft] FINAL qualityScore={qualityScore}");
        qualityScore = Mathf.Clamp(qualityScore, 0, 100);

        int star = CalcStar(qualityScore);
        Debug.Log($"[Craft] FINAL stars={star}");

        yield return StartCoroutine(OnCraftingComplete(recipe, qualityScore, star));

        }

    // ================== Heat (Furnace) ==================
    IEnumerator HandleHeatStep(RecipeData recipe)
    {
        yield return StartCoroutine(MoveTo(heatPosition.position));
        PlayWorkAnim();

        // 1) 씬 로드 + 바인딩
        MiniSceneBinding bind = null;
        yield return StartCoroutine(LoadMiniSceneAndBind(
            furnaceSceneName, "FurnaceModule",
            r => bind = r
        ));

        // 방어
        if (bind == null || bind.root == null || bind.cam == null)
        {
            Debug.LogError("[Crafting] Furnace 바인딩 실패");
            yield break;
        }

        // 2) 진입(카메라 스왑)
        camSwap.EnterMiniGame(bind.cam, bind.root, bind.ui);

        // 3) 완료 대기 (Blower가 MiniGameState.FurnaceDone = true 설정)
        yield return new WaitUntil(() => MiniGameState.FurnaceDone);
        MiniGameState.FurnaceDone = false;

        // 4) 종료(복귀)
        camSwap.ExitMiniGame(bind.cam, bind.root, bind.ui);

        // 5) 씬 언로드
        yield return SceneManager.UnloadSceneAsync(furnaceSceneName);
    }

    // ================== Hammer (Anvil Rhythm) ==================
    IEnumerator HandleHammerStep()
    {
        yield return StartCoroutine(MoveTo(hammerPosition.position));
        PlayWorkAnim();

        MiniSceneBinding bind = null;
        yield return StartCoroutine(LoadMiniSceneAndBind(
            anvilSceneName, "AnvilModule",
            r => bind = r
        ));

        if (bind == null || bind.root == null || bind.cam == null)
        {
            Debug.LogError("[Crafting] Anvil 바인딩 실패");
            yield break;
        }

        camSwap.EnterMiniGame(bind.cam, bind.root, bind.ui);

        // RhythmGameManager가 끝날 때 MiniGameState.HammerDone = true 설정
        yield return new WaitUntil(() => MiniGameState.HammerDone);
        MiniGameState.HammerDone = false;

        camSwap.ExitMiniGame(bind.cam, bind.root, bind.ui);
        yield return SceneManager.UnloadSceneAsync(anvilSceneName);

        if (HammerResultData.hasValue)
        {
            var r = HammerResultData.Consume();
            _totalCounts.perfect += r.perfect;
            _totalCounts.great += r.great;
            _totalCounts.good += r.good;
            _totalCounts.miss += r.miss;

            Debug.Log($"[Craft] SUM(Hammer) => P:{_totalCounts.perfect} G:{_totalCounts.great} D:{_totalCounts.good} M:{_totalCounts.miss}");

        }
    }

    //=================== Grinding ========================
    IEnumerator HandleGrindingStep()
    {
        yield return StartCoroutine(MoveTo(grindingPosition.position));
        PlayWorkAnim();

        MiniSceneBinding bind = null;
        yield return StartCoroutine(LoadMiniSceneAndBind(
            grindingSceneName, "GrindingModule",
            r => bind = r
        ));

        if (bind == null || bind.root == null || bind.cam == null)
        {
            Debug.LogError("[Crafting] Grinding 바인딩 실패");
            yield break;
        }

        // 2) 진입(카메라 스왑)
        camSwap.EnterMiniGame(bind.cam, bind.root, bind.ui);

        // 3) 완료 대기 (Blower가 MiniGameState.FurnaceDone = true 설정)
        yield return new WaitUntil(() => MiniGameState.GrindingDone);
        MiniGameState.GrindingDone = false;

        // 4) 종료(복귀)
        camSwap.ExitMiniGame(bind.cam, bind.root, bind.ui);

        /*if (PolishResultData.hasValue)         // 네가 예전 코드에서 쓰던 데이터 구조
        {
            int fails = PolishResultData.Consume();
            qualityScore -= fails * 20;        // 원하는 규칙으로 조정
        }*/

        // 5) 씬 언로드
        yield return SceneManager.UnloadSceneAsync(grindingSceneName);

        if (GrindingResultData.hasValue)
        {
            var r = GrindingResultData.Consume();
            _totalCounts.perfect += r.perfect;
            _totalCounts.great += r.great;
            _totalCounts.good += r.good;
            _totalCounts.miss += r.miss;

            Debug.Log($"[Craft] SUM(Grinding) => P:{_totalCounts.perfect} G:{_totalCounts.great} D:{_totalCounts.good} M:{_totalCounts.miss}");
        }
    }


// ================== 공통 로더/바인더 ==================
    private IEnumerator LoadMiniSceneAndBind(string sceneName, string moduleRootName, System.Action<MiniSceneBinding> onDone)
    {
        MiniSceneBinding result = new MiniSceneBinding();

        // 씬 로드 대기 (완료 + 한 프레임 유예)
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!op.isDone) yield return null;
        }
        yield return null;

        Scene miniScene = SceneManager.GetSceneByName(sceneName);
        if (!miniScene.IsValid())
        {
            Debug.LogError($"[Crafting] '{sceneName}' 씬 로드 실패");
            onDone?.Invoke(result);
            yield break;
        }

        // 루트 탐색 (이름 우선 → 없으면 첫 번째)
        GameObject[] roots = miniScene.GetRootGameObjects();
        GameObject moduleRoot = null;

        if (!string.IsNullOrEmpty(moduleRootName))
        {
            foreach (var go in roots)
            {
                if (go.name == moduleRootName) { moduleRoot = go; break; }
            }
        }
        if (!moduleRoot && roots.Length > 0) moduleRoot = roots[0];

        result.root = moduleRoot;
        if (moduleRoot)
        {
            result.cam = moduleRoot.GetComponentInChildren<Camera>(true);
            result.ui = moduleRoot.GetComponentInChildren<Canvas>(true);

            if (result.ui) result.ui.enabled = false;
            if (result.cam) result.cam.enabled = false;
            moduleRoot.SetActive(false);
        }

        onDone?.Invoke(result);
    }

    // ================== 유틸 ==================
    private void PlayWorkAnim()
    {
        var anim = player ? player.GetComponent<Animator>() : null;
        if (anim) anim.SetTrigger("Work");
    }

    IEnumerator MoveTo(Vector3 target)
    {
        while (Vector3.Distance(player.transform.position, target) > 0.05f)
        {
            if (camSwap && camSwap.IsLocked) { yield return null; continue; }

            Vector3 dir = (target - player.transform.position).normalized;
            player.transform.forward = new Vector3(dir.x, 0, dir.z);
            player.transform.position =
                Vector3.MoveTowards(player.transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        player.transform.position = target;
    }

    IEnumerator OnCraftingComplete(RecipeData recipe, int quality, int star)
    {
        bool closed = false;
        completePopup.Show(recipe, quality, star, () => closed = true);
        yield return new WaitUntil(() => closed);

        yield return StartCoroutine(MoveTo(doorPosition.position));
        yield return new WaitForSeconds(1f);

        _currentCustomer?.ServeWeapon(recipe, quality);
    }

    public int CalcStar(int score)
    {
        if (score >= 90) return 5;
        if (score >= 70) return 4;
        if (score >= 50) return 3;
        if (score >= 35) return 2;
        return 1;
    }
}
