using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Mobilyaları veya eşyaları otomatik kaydetmek için kullanılan yönetici sınıf.
/// Bu script bir GameObject'e eklendiğinde, belirtilen tag'lere sahip tüm objelere Saveable componenti ekler.
/// </summary>
public class SaveableFurniture : MonoBehaviour
{
    [Tooltip("Bu tag'lere sahip objeler otomatik olarak kaydedilecek")]
    [SerializeField] private string[] furnitureTags = { };
    
    private void Start()
    {
        RegisterAllFurniture();
    }

    /// <summary>
    /// Belirtilen tag'lere sahip tüm objelere Saveable component'i ekler
    /// </summary>
    public void RegisterAllFurniture()
    {
        int count = 0;
        
        foreach (string tag in furnitureTags)
        {
            if (string.IsNullOrEmpty(tag)) continue;
            
            GameObject[] tags = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in tags)
            {
                // Eğer obje zaten Saveable component'ine sahip değilse ekle
                if (obj.GetComponent<Saveable>() == null)
                {
                    Saveable saveable = obj.AddComponent<Saveable>();
                    saveable.saveKey = $"{tag}_{obj.name}_{obj.GetInstanceID()}";
                    saveable.savePosition = true;
                    saveable.saveRotation = true;
                    saveable.saveScale = false;
                    saveable.saveActiveState = true;
                    
                    count++;
                }
            }
        }
        
        Debug.Log($"Toplam {count} adet eşya otomatik olarak kayıt sistemine eklendi.");
    }
} 