using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class SaveManager
{
    private static string Path => System.IO.Path.Combine(Application.persistentDataPath, "save.json");

    public static void SaveGame(SaveData data)
    {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Path, json);
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(Path)) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
    }

    public static bool HasSave() => File.Exists(Path);

    // ↓ 여기를 추가
    public static void DeleteSave()
    {
        if (File.Exists(Path))
            File.Delete(Path);
    }
}