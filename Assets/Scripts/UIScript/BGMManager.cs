using UnityEngine;

public class BGMManager : MonoBehaviour
{
    private static BGMManager instance;
    public AudioSource bgm;

    [Header("BGM 설정")]
    public AudioClip bgmClip;
    [Range(0f, 1f)]
    public float bgmVolume = 0.6f;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            bgm = GetComponent<AudioSource>();
            if (bgm == null)
                bgm = gameObject.AddComponent<AudioSource>();

            bgm.clip = bgmClip;
            bgm.loop = true;
            bgm.playOnAwake = false;
            bgm.volume = bgmVolume;

            if (!bgm.isPlaying)
                bgm.Play();
        }
        else
        {
            Destroy(gameObject); // 중복 생성 방지
        }
    }
}