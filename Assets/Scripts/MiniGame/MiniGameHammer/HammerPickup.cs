using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class HammerPickup : MonoBehaviour
{
    public GameObject hammerWorldObj;  // 모루 위 망치 3D 오브젝트
    public GameObject rhythmUIRoot;    // 리듬 UI 패널(PlayArea 포함)
    public RhythmGameManager mgr;          // 리듬 매니저
    public TextMeshProUGUI countdownText;  // 3-2-1 표기용 (패널 안에 배치)


    private bool hasHammer = false;

    void Start()
    {
        if (rhythmUIRoot) rhythmUIRoot.SetActive(false);
        if (countdownText) countdownText.gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        if (hasHammer) return;
        hasHammer = true;

        if (hammerWorldObj) hammerWorldObj.SetActive(false); // 통째로 꺼도 됨
        if (rhythmUIRoot) rhythmUIRoot.SetActive(true);

        if (mgr != null) mgr.BeginWithCountdown(countdownText);
    }
    
}

