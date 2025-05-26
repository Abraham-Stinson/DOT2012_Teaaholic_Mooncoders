using UnityEngine;

public class DirtyStatus : MonoBehaviour
{
    public bool isDirty = false;
    [SerializeField] private Material dirtyMaterial;
    [SerializeField] private Material cleanMaterial;
    [SerializeField] private Renderer cupRenderer;
    
    private bool lastDirtyState;
    private Material currentMaterial;

    private void Start()
    {
        // Find renderer if not assigned
        if (cupRenderer == null)
        {
            cupRenderer = GetComponentInChildren<Renderer>();
        }
        
        // Load materials if not assigned
        if (dirtyMaterial == null)
        {
            dirtyMaterial = Resources.Load<Material>("dirty_glass");
        }
        
        if (cleanMaterial == null)
        {
            cleanMaterial = cupRenderer?.material;
        }
        
        lastDirtyState = isDirty;
        currentMaterial = cupRenderer?.material;
    }
    
    private void Update()
    {
        // Sadece durum değiştiğinde material'i güncelle
        if (isDirty != lastDirtyState && cupRenderer != null)
        {
            if (isDirty && dirtyMaterial != null && currentMaterial != dirtyMaterial)
            {
                cupRenderer.material = dirtyMaterial;
                currentMaterial = dirtyMaterial;
            }
            else if (!isDirty && cleanMaterial != null && currentMaterial != cleanMaterial)
            {
                cupRenderer.material = cleanMaterial;
                currentMaterial = cleanMaterial;
            }
            
            lastDirtyState = isDirty;
        }
    }
    
    public void CleanDirt()
    {
        Debug.Log("Yıkandı");
        if(GetComponent<Tea_Cup>() != null)
        {
            GetComponent<Tea_Cup>().EmptyCup();
        }
        if(GetComponent<Kettle>() != null)
        {
            GetComponent<Kettle>().EmptyKettle();
            Debug.Log("Kettle yıkandı");
        }
        isDirty = false;
    }
}
