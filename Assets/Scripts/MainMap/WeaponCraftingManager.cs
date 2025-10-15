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


    private bool _prevAgentEnabled = false;           
    private bool _hasAgent = false;                   
    private UnityEngine.AI.NavMeshAgent _agent;       

    private bool _hasRb = false;                      
    private Rigidbody _rb;                            
    private RigidbodyConstraints _prevConstraints;    

    private bool _hasAnim = false;                    
    private Animator _anim;                           
    private bool _prevRootMotion = false;             

    /* ==================================================
       PUBLIC ENTRY POINT
    ==================================================*/

    public static WeaponCraftingManager Instance { get; private set; }  

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        CachePlayerComponents();
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
            else
                CachePlayerComponents();
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
        if (CameraTransitionData.resumeAfterReturn)
        {
            switch (CameraTransitionData.nextStepIndex)
            {
                case 1: StartCoroutine(HandleHammerStep()); break;
                case 2: StartCoroutine(HandlePolishStep()); break;
                case -1:
                    int star = CalcStar(qualityScore);
                    StartCoroutine(OnCraftingComplete(null, qualityScore, star));
                    break;
            }

            CameraTransitionData.resumeAfterReturn = false; // 추가됨
            yield break; // 추가됨
        }

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

        StartCoroutine(OnCraftingComplete(recipe, qualityScore, star));  
        
    }

    /* ==================================================
       각 스텝 코루틴
    ==================================================*/
    //----------------------------------------------------------------
    private IEnumerator HandleHeatStep()
    {
        // 1) 자리로 이동 + 작업 포즈
        yield return StartCoroutine(MoveTo(heatPosition.position));
        PlayWorkAnim();

        // 2) 미니게임 들어갈 동안 캐릭터 완전 고정
        FreezePlayer();

        CameraTransitionData.resumeAfterReturn = true;
        CameraTransitionData.nextStepIndex = 1;
        CameraTransitionData.savedQuality = qualityScore;

        // 3) 카메라 줌인 + 씬 전환 (포커스 명시!)
        var camTrans = Camera.main.GetComponent<CameraSceneTransition>();
        if (camTrans != null)
        {
            camTrans.focusPoint = heatPosition;      //  포커스 지정
            camTrans.StartZoomIn("MiniGameBrasier");
        }
        else
        {
            SceneManager.LoadScene("MiniGameBrasier");
        }

        // 4) 미니게임 끝나고 메인으로 돌아올 때까지 대기
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Ingame_main");

        // 5) 잠금 해제 → 다음 스텝(모루)로 자연스럽게 진행
        UnfreezePlayer();
    }

    // 2) 해머 스텝
    private IEnumerator HandleHammerStep()
    {
        // 1) 자리로 이동 + 애니
        yield return StartCoroutine(MoveTo(hammerPosition.position));
        PlayWorkAnim();

        FreezePlayer();

        CameraTransitionData.resumeAfterReturn = true;
        CameraTransitionData.nextStepIndex = 2;
        CameraTransitionData.savedQuality = qualityScore;

        // 2) 카메라 줌인 + 씬 전환
        var camTrans = Camera.main.GetComponent<CameraSceneTransition>();
        if (camTrans != null)
        {
            camTrans.focusPoint = hammerPosition;              //  포커스 지점 지정
            camTrans.StartZoomIn("MiniGameHammer");         //  망치 미니게임 씬명
        }
        else
        {
            SceneManager.LoadScene("MiniGameHammer");
        }

        // 3) 미니게임 끝나고 Ingame_main으로 복귀할 때까지 대기
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Ingame_main");

        // 4) 결과 반영 (정적 버스에서 회수)
        if (HammerResultData.hasValue)
        {
            var r = HammerResultData.Consume();
            if (r.fails == 1) qualityScore -= 10;
            else if (r.fails == 2) qualityScore -= 20;
            else if (r.fails >= 3) qualityScore -= 35;

            qualityScore += r.perfect * 5;
        }
    }

    // 3) 연마 스텝
    private IEnumerator HandlePolishStep()
    {
        yield return StartCoroutine(MoveTo(polishPosition.position));
        PlayWorkAnim();

        FreezePlayer();

        CameraTransitionData.resumeAfterReturn = true;
        CameraTransitionData.nextStepIndex = -1;
        CameraTransitionData.savedQuality = qualityScore;

        var camTrans = Camera.main.GetComponent<CameraSceneTransition>();
        if (camTrans != null)
        {
            camTrans.focusPoint = polishPosition;              //  포커스 지점 지정
            camTrans.StartZoomIn("MiniGameRub");               // 연마 미니게임 씬명
        }
        else
        {
            SceneManager.LoadScene("MiniGameRub");
        }

        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Ingame_main");

        UnfreezePlayer();

        if (PolishResultData.hasValue)
        {
            int fails = PolishResultData.Consume();
            qualityScore -= fails * 20;
        }
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

        foreach (var root in roots)
        {
            if (onSliderLoaded != null)
            {
                var slider = root.GetComponentInChildren<SliderController>(true);
                if (slider != null) onSliderLoaded(slider);
            }

            if (onHammerLoaded != null)
            {
                var hammer = root.GetComponentInChildren<HammerMiniGame>(true);
                if (hammer != null) onHammerLoaded(hammer);
            }

            if (onPolishLoaded != null)
            {
                var polish = root.GetComponentInChildren<SharpeningSwipeGame>(true);
                if (polish != null) onPolishLoaded(polish);
            }
        }

        yield return new WaitUntil(() => !SceneManager.GetSceneByName(sceneName).isLoaded);
    }

    private IEnumerator OnCraftingComplete(RecipeData recipe, int quality, int star)
    {
        bool popupClosed = false;
        completePopup.Show(recipe, quality, star, () => popupClosed = true);
        yield return new WaitUntil(() => popupClosed);

        yield return StartCoroutine(MoveTo(doorPosition.position));

        yield return new WaitForSeconds(1f);

        if (_currentCustomer != null)
        {
            _currentCustomer.ServeWeapon(recipe,quality);
        }
    }

    public int CalcStar(int score)
    {
        if (score >= 90) return 5;
        if (score >= 70) return 4;
        if (score >= 50) return 3;
        if (score >= 35) return 2;
        return 1;
    }
    // ==== 플레이어 잠금/해제 유틸 ====
    private void CachePlayerComponents() // 추가됨
    {
        if (player == null) return;
        if (_anim == null) { _anim = player.GetComponent<Animator>(); _hasAnim = _anim != null; }
        if (_agent == null) { _agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>(); _hasAgent = _agent != null; }
        if (_rb == null) { _rb = player.GetComponent<Rigidbody>(); _hasRb = _rb != null; }
    }

    private void FreezePlayer() // 추가됨
    {
        CachePlayerComponents();

        if (_hasAnim)
        {
            _prevRootMotion = _anim.applyRootMotion;
            _anim.applyRootMotion = false;
            _anim.ResetTrigger("Work");
            _anim.Update(0f);
        }

        if (_hasAgent)
        {
            _prevAgentEnabled = _agent.enabled;
            _agent.isStopped = true;
            _agent.ResetPath();
            _agent.enabled = false;
        }

        if (_hasRb)
        {
            _prevConstraints = _rb.constraints;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    private void UnfreezePlayer() // 추가됨
    {
        if (_hasAnim) _anim.applyRootMotion = _prevRootMotion;
        if (_hasAgent) _agent.enabled = _prevAgentEnabled;
        if (_hasRb) _rb.constraints = _prevConstraints;
    }
}

