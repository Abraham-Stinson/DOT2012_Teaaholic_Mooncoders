using System.Collections.Generic;
using UnityEngine;

public static class SaveManager
{
    private static List<Saveable> saveables = new List<Saveable>();

    public static void Register(Saveable saveable)
    {
        if (!saveables.Contains(saveable))
            saveables.Add(saveable);
    }

    public static void Unregister(Saveable saveable)
    {
        if (saveables.Contains(saveable))
            saveables.Remove(saveable);
    }

    public static void SaveAll()
    {
        for (int i = saveables.Count - 1; i >= 0; i--)
        {
            if (saveables[i] == null)
            {
                saveables.RemoveAt(i);
                continue;
            }
            saveables[i].Save();
        }
        PlayerPrefs.Save();
        Debug.Log("Tüm objeler kaydedildi.");
    }

    public static void LoadAll()
    {
        for (int i = saveables.Count - 1; i >= 0; i--)
        {
            if (saveables[i] == null)
            {
                saveables.RemoveAt(i);
                continue;
            }
            saveables[i].Load();
        }
        Debug.Log("Tüm objeler yüklendi.");
    }
}
