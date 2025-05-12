using UnityEngine;

public class PickableGameItem : MonoBehaviour
{
    [Header("Game Properties")]
    [SerializeField] private string gameType; // "Backgammon", "Cards", or "Okey"
    [SerializeField] private GameObject pickableModel;
    [SerializeField] private GameObject placedModel;
    [SerializeField] private bool isPickable = true;
    
    private HighLight highlight;
    
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