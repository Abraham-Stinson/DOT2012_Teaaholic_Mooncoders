using UnityEngine;

public class TableGame : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private string gameType; // "Backgammon", "Cards", or "Okey"
    
    [Header("Object States")]
    [SerializeField] private GameObject pickupObject; // The object state when it can be picked up
    [SerializeField] private GameObject tableObject;  // The object state when it's placed on a table
    
    private bool isOnTable = false;
    private Table currentTable = null;

    private void Start()
    {
        // Initially show pickup state
        UpdateVisuals();
    }
    
    public void PlaceOnTable(Table table)
    {
        if (table != null)
        {
            currentTable = table;
            isOnTable = true;
            table.PlaceGame(gameType);
            UpdateVisuals();
        }
    }
    
    public void RemoveFromTable()
    {
        if (isOnTable && currentTable != null && currentTable.IsAvailable)
        {
            currentTable.RemoveGame();
            isOnTable = false;
            currentTable = null;
            UpdateVisuals();
        }
    }
    
    public bool IsPlacedOnTable()
    {
        return isOnTable;
    }
    
    public string GetGameType()
    {
        return gameType;
    }
    
    private void UpdateVisuals()
    {
        if (pickupObject != null) pickupObject.SetActive(!isOnTable);
        if (tableObject != null) tableObject.SetActive(isOnTable);
    }
} 