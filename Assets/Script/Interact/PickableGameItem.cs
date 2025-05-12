using UnityEngine;

public class PickableGameItem : MonoBehaviour
{
    [Header("Game Properties")]
    [SerializeField] private string gameType; // "Backgammon", "Cards", or "Okey"
    [SerializeField] private GameObject pickableModel;
    [SerializeField] private GameObject placedModel;
    [SerializeField] private bool isPickable = true;
    
    [Header("Table Appearance")]
    [SerializeField] private GameObject[] tableSpecificModels; // Farklı masa tipleri için farklı modeller
    
    private HighLight highlight;
    private int currentTableType = -1;
    
    private void Awake()
    {
        highlight = GetComponent<HighLight>();
        if (highlight == null)
        {
            highlight = gameObject.AddComponent<HighLight>();
        }
        
        UpdateModels();
    }
    
    public void MakePickable()
    {
        isPickable = true;
        
        // Update layer to interactable
        gameObject.layer = LayerMask.NameToLayer("Interactable");
        
        UpdateModels();
    }
    
    public void MakeNotPickable()
    {
        isPickable = false;
        
        // Update layer to non-interactable
        gameObject.layer = LayerMask.NameToLayer("Default");
        
        UpdateModels();
    }
    
    private void UpdateModels()
    {
        if (pickableModel != null)
        {
            pickableModel.SetActive(isPickable);
        }
        
        if (placedModel != null)
        {
            placedModel.SetActive(!isPickable);
        }
        
        // Masa tipine göre belirli modelleri güncelle
        UpdateTableSpecificModels();
    }
    
    public void UpdateAppearanceForTable(int tableTypeID)
    {
        currentTableType = tableTypeID;
        UpdateTableSpecificModels();
    }
    
    private void UpdateTableSpecificModels()
    {
        // Tüm masa tipi modellerini gizle
        if (tableSpecificModels != null)
        {
            for (int i = 0; i < tableSpecificModels.Length; i++)
            {
                if (tableSpecificModels[i] != null)
                {
                    bool shouldShow = !isPickable && i == currentTableType;
                    tableSpecificModels[i].SetActive(shouldShow);
                }
            }
        }
    }
    
    public string GetGameType()
    {
        return gameType;
    }
    
    public bool IsPickable()
    {
        return isPickable;
    }
} 