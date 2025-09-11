using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterJLoader : MonoBehaviour
{
    [Header("캐릭터 프리팹")]
    public GameObject edwinPrefab;
    public GameObject isabellaPrefab;
    public GameObject tuskPrefab;

    [Header("스폰 위치")]
    public Transform spawnPoint;

    void Start()
{
    string selected = PlayerPrefs.GetString("SelectedCharacter", "");
    if (string.IsNullOrEmpty(selected))
    {
        Debug.LogError("선택된 캐릭터 정보가 없습니다.");
        return;
    }

    GameObject prefabToSpawn = null;
    switch (selected)
    {
        case "Edwin":    prefabToSpawn = edwinPrefab;    break;
        case "Isabella": prefabToSpawn = isabellaPrefab; break;
        case "Tusk":     prefabToSpawn = tuskPrefab;     break;
        default:
            Debug.LogError($"알 수 없는 캐릭터 ID: {selected}");
            return;
    }

    // **한 번만 Instantiate** 하고, 그 인스턴스에만 태그를 붙입니다.
    GameObject spawned = Instantiate(
        prefabToSpawn,
        spawnPoint.position,
        spawnPoint.rotation
    );
    spawned.tag = "Player";
    Debug.Log($"{selected} 을(를) 스폰하고 Player 태그를 붙였습니다.");
}
}