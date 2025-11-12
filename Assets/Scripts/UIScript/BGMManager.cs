using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;
    public AudioSource bgm;

    [Header("BGM 설정")]
    public AudioClip bgmClip;
    [Range(0f, 1f)] public float bgmVolume = 0.6f;

    [Header("재생할 씬 이름들 (여기 씬에서만 브금 재생)")]
    public string[] playInScenes = { "StartScene", "CharaterSelectScene" }; // <- 필요시 수정

    [Header("페이드 옵션")]
    public bool useFade = true;
    public float fadeTime = 0.6f;

    void Awake()
    {
        if (instance != null) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // 자동으로 AudioSource 생성
        bgm = GetComponent<AudioSource>();
        if (bgm == null)
        {
            bgm = gameObject.AddComponent<AudioSource>();
            bgm.playOnAwake = false;
        }

        bgm.clip = bgmClip;
        bgm.loop = true;
        bgm.volume = bgmVolume;

        HandleForScene(SceneManager.GetActiveScene());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleForScene(scene);
    }

    void HandleForScene(Scene scene)
    {
        if (ShouldPlayIn(scene))
        {
            // 지정한 씬이면 재생
            if (!bgm.isPlaying)
            {
                if (useFade) StartCoroutine(FadeIn());
                else { bgm.volume = bgmVolume; bgm.Play(); }
            }
        }
        else
        {
            // 그 외 씬이면 정지 (겹침 방지)
            if (bgm.isPlaying)
            {
                if (useFade) StartCoroutine(FadeOutAndStop());
                else bgm.Stop();
            }
        }
    }

    bool ShouldPlayIn(Scene scene)
    {
        // 지정 목록에 포함된 씬에서만 재생
        for (int i = 0; i < playInScenes.Length; i++)
        {
            if (scene.name == playInScenes[i]) return true;
        }
        return false;
    }

    IEnumerator FadeIn()
    {
        bgm.volume = 0f;
        if (!bgm.isPlaying) bgm.Play();
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            bgm.volume = Mathf.Lerp(0f, bgmVolume, t / fadeTime);
            yield return null;
        }
        bgm.volume = bgmVolume;
    }

    IEnumerator FadeOutAndStop()
    {
        float start = bgm.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            bgm.volume = Mathf.Lerp(start, 0f, t / fadeTime);
            yield return null;
        }
        bgm.Stop();
        bgm.volume = bgmVolume; // 다음 재생 대비 원복
    }
}