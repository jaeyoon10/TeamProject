using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsUI : MonoBehaviour
{
    [Header("패널")]
    public GameObject panel;            // 옵션 패널 전체
    [Header("오디오 믹서")]
    public AudioMixer mixer;            // GameAudio 믹서
    [Header("슬라이더")]
    public Slider bgSlider;         // 배경음량 슬라이더
    public Slider sfxSlider;        // 효과음량 슬라이더
    [Header("해상도")]
    public Dropdown resolutionDropdown;
    private Resolution[] options;
    [Header("창 모드")]
    public Toggle windowToggle;

    void Start()
    {
        // 1) 최초 값 로드
        panel.SetActive(false);
        // AudioMixer 파라미터 불러오기
        float bgVol, sfxVol;
        mixer.GetFloat("BackgroundVolume", out bgVol);
        mixer.GetFloat("SFXVolume", out sfxVol);
        bgSlider.value = Mathf.Pow(10, bgVol / 20f);
        sfxSlider.value = Mathf.Pow(10, sfxVol / 20f);

        // 2) Resolution Dropdown 세팅
        options = new Resolution[] {
            new Resolution{width=1920,height=1080},
            new Resolution{width=1366,height=768},
            new Resolution{width=1600,height=900},
            new Resolution{width=2560,height=1440}
        };
        var list = new List<string>();
        for (int i = 0; i < options.Length; i++)
            list.Add(options[i].width + "×" + options[i].height);
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(list);

        // 현재 해상도와 일치하는 인덱스 찾기
        for (int i = 0; i < options.Length; i++)
        {
            if (Screen.currentResolution.width == options[i].width &&
               Screen.currentResolution.height == options[i].height)
            {
                resolutionDropdown.value = i; break;
            }
        }

        // 3) 창 모드 토글 초기값 (false=전체화면)
        windowToggle.isOn = !Screen.fullScreen;

        // 4) 콜백 등록
        bgSlider.onValueChanged.AddListener(SetBackgroundVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        windowToggle.onValueChanged.AddListener(SetWindowMode);
    }

    // “톱니바퀴” 버튼 OnClick 에 연결
    public void OpenPanel() => panel.SetActive(true);
    // 옵션창 X 버튼 OnClick 에 연결
    public void ClosePanel() => panel.SetActive(false);

    void SetBackgroundVolume(float v)
    {
        // 로그 스케일: 0.0001~1 -> -80~0dB
        mixer.SetFloat("BackgroundVolume", Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f);
    }
    void SetSFXVolume(float v)
    {   
        mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(v, 0.0001f)) * 20f);
    }
    void SetResolution(int idx)
    {
        var r = options[idx];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);
    }
    void SetWindowMode(bool isWindowed)
    {
        Screen.fullScreen = !isWindowed;
    }

    public void QuitGame()
    {
        // 에디터에서는 플레이 모드를 종료하고
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // 빌드된 애플리케이션에서는 완전히 종료
    Application.Quit();
#endif
    }
}
