using System.Collections.Generic;
using UnityEngine;

public class HighLight : MonoBehaviour
{
    [SerializeField] private List<Renderer> renderers = new List<Renderer>();
    private  Color highlightColor = Color.gray; // Varsayılan renk
    
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private bool isHighlighted = false;
    
    private void Awake()
    {
        // Renderer verilmediyse, bu objedeki tüm renderer'ları ekle
        if (renderers.Count == 0)
        {
            Renderer[] foundRenderers = GetComponentsInChildren<Renderer>();
            renderers.AddRange(foundRenderers);
        }
        
        // Her renderer için orijinal materyalleri kaydet
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                // Orijinal materyallerin bir kopyasını sakla
                Material[] originalMats = renderer.materials;
                originalMaterials.Add(renderer, originalMats);
            }
        }
    }
    
    public void ToggleHighLight(bool val)
    {
        if (isHighlighted == val) return; // Zaten istenen durumda ise işlem yapma
        
        isHighlighted = val;
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;
            
            Material[] materials = renderer.materials;
            
            if (val) // Highlight aç
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    // Her materyal için emission'ı etkinleştir
                    materials[i].EnableKeyword("_EMISSION");
                    materials[i].SetColor("_EmissionColor", highlightColor);
                }
            }
            else // Highlight kapat
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    // Her materyal için emission'ı kapat
                    materials[i].DisableKeyword("_EMISSION");
                    materials[i].SetColor("_EmissionColor", Color.black);
                }
            }
            
            // Değiştirilen materyalleri uygula
            renderer.materials = materials;
        }
    }
    
    // Enable/disable olaylarında highlight durumunu güncelle
    private void OnEnable()
    {
        if (isHighlighted)
        {
            ToggleHighLight(true);
        }
    }
    
    private void OnDisable()
    {
        // Obje devre dışı kaldığında highlight'ı kapat
        if (isHighlighted)
        {
            ToggleHighLight(false);
        }
    }
    
    // Test için yardımcı metodlar
    public void EnableHighlight()
    {
        ToggleHighLight(true);
    }
    
    public void DisableHighlight()
    {
        ToggleHighLight(false);
    }
}