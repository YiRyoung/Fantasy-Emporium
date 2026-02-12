using UnityEngine;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;


[System.Serializable]
public class GameData
{
    // GameplayData
    public string FileName;
    public int Date;
    public int Coin;

    public void InitFile(string _fileName, int _date, int _coin)
    {
        FileName = _fileName;
        Date = _date;
        Coin = _coin;
    }
}

public static class GameDataManager
{
    public static int MultiMode = -1;

    #region PlayerPrefs Save & Load
    public static Resolution resolution;
    public static FullScreenMode fullScreenMode;

    public static void SaveResolution(Resolution _resolution, FullScreenMode _mode)
    {
        PlayerPrefs.SetInt("ResolutionWidth", _resolution.width);
        PlayerPrefs.SetInt("ResolutionHeight", _resolution.height);
        PlayerPrefs.SetInt("FullScreenMode", (int)_mode);
        PlayerPrefs.Save();
    }

    public static void LoadResolution()
    {
        resolution = new Resolution { width = PlayerPrefs.GetInt("ResolutionWidth", Screen.currentResolution.width), height = PlayerPrefs.GetInt("ResolutionHeight", Screen.currentResolution.height) };
        fullScreenMode = (FullScreenMode)PlayerPrefs.GetInt("FullScreenMode", (int)FullScreenMode.FullScreenWindow);
    }
    #endregion

    #region Json Save & Load
    static string GetSavePath(int _slot)
    {
        string fileName = $"slot{_slot}.json";
        return Path.Combine(Application.persistentDataPath, fileName);
        // Application.persistentDataPath -> C:\Users\username\AppData\LocalLow\CompanyName\ProductName (Windows 기준)
    }
    public static bool IsDuplicateFileName(string newName, int slotCount)
    {
        for (int i = 0; i < slotCount; i++)
        {
            string path = GetSavePath(i);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                GameData data = JsonUtility.FromJson<GameData>(json);

                if (data != null && data.FileName == newName)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static void Save(GameData data, int slot)
    {
        string json = JsonUtility.ToJson(data, true); // prettyPrint 적용 여부 (가독성 좋게 줄바꿈, 들여쓰기)
        string path = GetSavePath(slot);
        File.WriteAllText(path, json);
        Debug.Log($"슬롯 {slot} 저장 완료: {path}");
    }

    public static GameData Load(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<GameData>(json);
        }
        else
        {
            Debug.LogWarning($"슬롯 {slot}에 저장된 파일 없음");
            return null;
        }
    }

    public static GameData Delete(int slot)
    {
        string path = GetSavePath(slot);
        if (File.Exists(path))
        {
            GameData data = Load(slot);
            File.Delete(path);
            Debug.Log($"슬롯 {slot} 삭제 완료: {path}");
            return data;
        }
        else
        {
            Debug.LogWarning($"슬롯 {slot}에 저장된 파일 없음");
            return null;
        }
    }

    public static bool HasSave(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }
    #endregion

}
