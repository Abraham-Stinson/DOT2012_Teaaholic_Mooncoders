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
        // Null referansları temizle
        saveables.RemoveAll(item => item == null);
        customSaveables.RemoveAll(item => item == null);

        // Normal Saveable objeler
        foreach (var saveable in saveables)
        {
            try
            {
                saveable.Save();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Save error for {saveable.name}: {e.Message}");
            }
        }
        
        // Custom save sistemini kullanan objeler
        foreach (var saveable in customSaveables)
        {
            try
            {
                saveable.SaveData();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Save error: {e.Message}");
            }
        }
        
        PlayerPrefs.Save();
        Debug.Log("Tüm objeler kaydedildi.");
    }

    public static void LoadAll()
    {
        // Null referansları temizle
        saveables.RemoveAll(item => item == null);
        customSaveables.RemoveAll(item => item == null);
        
        // Normal Saveable objeler
        foreach (var saveable in saveables)
        {
            try
            {
                saveable.Load();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Load error for {saveable.name}: {e.Message}");
            }
        }
        
        // Custom save sistemini kullanan objeler
        foreach (var saveable in customSaveables)
        {
            try
            {
                saveable.LoadData();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Load error: {e.Message}");
            }
        }
        
        Debug.Log("Tüm objeler yüklendi.");
    }
    
    public static void DeleteAllSaveData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Tüm kayıt verileri silindi!");
    }
}