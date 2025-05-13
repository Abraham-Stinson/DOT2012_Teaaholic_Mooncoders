using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    [Header("Table Properties")]
    [SerializeField] private Chair[] chairs;
    [SerializeField] private Transform gameItemPlacement;
    [SerializeField] private bool isReserved = false;
    [SerializeField] private int maxSeats;
    [SerializeField] private int tableTypeID; // Masanın tipini tanımlamak için ID
    
    [Header("Game Items")]
    [SerializeField] private GameObject currentGameItem;
    [SerializeField] private Transform[] drinkPlacements;
    
    private List<Chair> availableChairs = new List<Chair>();
    
    private void Start()
    {
        maxSeats = chairs.Length;
        UpdateAvailableChairs();
    }
    
    private void UpdateAvailableChairs()
    {
        availableChairs.Clear();
        foreach (Chair chair in chairs)
        {
            if (!chair.IsOccupied)
            {
                availableChairs.Add(chair);
            }
        }
    }
    
    public Chair GetAvailableChair()
    {
        UpdateAvailableChairs();
        
        if (availableChairs.Count > 0)
        {
            // Rastgele bir sandalye seç
            int randomIndex = Random.Range(0, availableChairs.Count);
            Chair selectedChair = availableChairs[randomIndex];
            availableChairs.RemoveAt(randomIndex);
            return selectedChair;
        }
        return null;
    }
    
    public Chair[] GetAvailableChairs(int count)
    {
        UpdateAvailableChairs();
        
        if (availableChairs.Count >= count)
        {
            Chair[] selectedChairs = new Chair[count];
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(0, availableChairs.Count);
                selectedChairs[i] = availableChairs[randomIndex];
                availableChairs.RemoveAt(randomIndex);
            }
            return selectedChairs;
        }
        return null;
    }
    
    public int AvailableSeats
    {
        get
        {
            int count = 0;
            foreach (Chair chair in chairs)
            {
                if (!chair.IsOccupied)
                {
                    count++;
                }
            }
            return count;
        }
    }
    
    public bool IsReserved
    {
        get { return isReserved; }
    }
    
    public void ReserveTable(int numSeats)
    {
        if (AvailableSeats >= numSeats)
        {
            isReserved = true;
        }
    }
    
    public void ReleaseTable()
    {
        isReserved = false;
        
        // If no NPCs are at the table, remove the game item
        bool anyOccupied = false;
        foreach (Chair chair in chairs)
        {
            if (chair.IsOccupied)
            {
                anyOccupied = true;
                break;
            }
        }
        
        if (!anyOccupied && currentGameItem != null)
        {
            // Allow the game item to be picked up again
            if (currentGameItem.TryGetComponent<PickableGameItem>(out var gameItem))
            {
                gameItem.MakePickable();
            }
        }
    }
    
    public int GetChairIndex(Chair chair)
    {
        if (chairs == null) return -1;
        
        for (int i = 0; i < chairs.Length; i++)
        {
            if (chairs[i] == chair)
            {
                return i;
            }
        }
        return -1;
    }
    
    public Transform GetDrinkPlacement(int chairIndex)
    {
        if (drinkPlacements == null || chairIndex < 0 || chairIndex >= drinkPlacements.Length)
        {
            return null;
        }
        return drinkPlacements[chairIndex];
    }
    
    public void SetGameItem(GameObject gameItem)
    {
        currentGameItem = gameItem;
        
        if (gameItem != null && gameItemPlacement != null)
        {
            gameItem.transform.position = gameItemPlacement.position;
            gameItem.transform.rotation = gameItemPlacement.rotation;
            gameItem.transform.SetParent(gameItemPlacement);
            
            // Make the game item not pickable while NPCs are using it
            if (gameItem.TryGetComponent<PickableGameItem>(out var pickableItem))
            {
                pickableItem.MakeNotPickable();
                // Farklı masa tipine göre görünümü ayarla
                pickableItem.UpdateAppearanceForTable(tableTypeID);
            }
        }
    }
    
    public bool IsTableEmpty()
    {
        foreach (Chair chair in chairs)
        {
            if (chair.IsOccupied)
            {
                return false;
            }
        }
        return true;
    }
    
    public int TableTypeID
    {
        get { return tableTypeID; }
    }
} 