using UnityEngine;
using System.Collections.Generic;

public interface ICanSave
{
    void SaveData();
    void LoadData();
}

public static class SaveManager
{
    private static List<Saveable> saveables = new List<Saveable>();
    private static List<ICanSave> customSaveables = new List<ICanSave>();

    public static void Register(Saveable saveable)
    {
        if (!saveables.Contains(saveable))
            saveables.Add(saveable);
    }
    
    public static void RegisterCustom(ICanSave saveable)
    {
        if (!customSaveables.Contains(saveable))
            customSaveables.Add(saveable);
    }

    public static void Unregister(Saveable saveable)
    {
        if (saveables.Contains(saveable))
            saveables.Remove(saveable);
    }
    
    public static void UnregisterCustom(ICanSave saveable)
    {
        if (customSaveables.Contains(saveable))
            customSaveables.Remove(saveable);
    }

    public static void SaveAll()
    {
        // Normal Saveable objeler
        for (int i = saveables.Count - 1; i >= 0; i--)
        {
            if (saveables[i] == null)
            {
                saveables.RemoveAt(i);
                continue;
            }
            saveables[i].Save();
        }
        
        // Custom save sistemini kullanan objeler
        for (int i = customSaveables.Count - 1; i >= 0; i--)
        {
            if (customSaveables[i] == null)
            {
                customSaveables.RemoveAt(i);
                continue;
            }
            customSaveables[i].SaveData();
        }
        
        PlayerPrefs.Save();
        Debug.Log("Tüm objeler kaydedildi.");
    }

    public static void LoadAll()
    {
        // Normal Saveable objeler
        for (int i = saveables.Count - 1; i >= 0; i--)
        {
            if (saveables[i] == null)
            {
                saveables.RemoveAt(i);
                continue;
            }
            saveables[i].Load();
        }
        
        // Custom save sistemini kullanan objeler
        for (int i = customSaveables.Count - 1; i >= 0; i--)
        {
            if (customSaveables[i] == null)
            {
                customSaveables.RemoveAt(i);
                continue;
            }
            customSaveables[i].LoadData();
        }
        
        Debug.Log("Tüm objeler yüklendi.");
    }
    
    public static void DeleteAllSaveData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Tüm kayıt verileri silindi!");
    }
}