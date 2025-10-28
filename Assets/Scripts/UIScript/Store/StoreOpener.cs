using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreOpener : MonoBehaviour
{
    public Button storeOpenButton;   // “상점 열기” 버튼
    public GameObject storePanel;    // StorePanel(GameObject)
    public Button storeCloseButton;  // “X” 버튼

    [Header("사운드")]
    public AudioClip openSound;
    public AudioClip closeSound;
    [Range(0f, 1f)]
    public float volume = 1f;

    private AudioSource _audio;

    void Awake()
    {
        // AudioSource 자동 생성
        _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 0f; // 2D 사운드
    }

    void Start()
    {
        if (storePanel.activeSelf)
            storePanel.SetActive(false);

        storeOpenButton.onClick.AddListener(ShowStorePanel);
        storeCloseButton.onClick.AddListener(CloseStorePanel);
    }

    void ShowStorePanel()
    {
        storePanel.SetActive(true);
        PlaySFX(openSound);
    }

    void CloseStorePanel()
    {
        storePanel.SetActive(false);
        PlaySFX(closeSound);
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            _audio.PlayOneShot(clip, volume);
    }
}