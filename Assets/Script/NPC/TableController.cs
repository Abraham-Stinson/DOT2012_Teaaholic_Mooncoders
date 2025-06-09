using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableController : MonoBehaviour, IInteractable
{
    [Header("Table Settings")]
    [SerializeField] public string tableName;
    [SerializeField] private List<Chair> chairs = new List<Chair>();
    [SerializeField] private Transform gameBoxPosition;
    [SerializeField] private GameObject tavlaGamePrefab;
    [SerializeField] private GameObject iskambilGamePrefab;
    [SerializeField] private GameObject okeyGamePrefab;
    
    [Header("Game Box Visuals")]
    [SerializeField] private GameObject tavlaGamePlaced; // Visual when placed on table
    [SerializeField] private GameObject iskambilGamePlaced;
    [SerializeField] private GameObject okeyGamePlaced;
    
    [Header("Adisyon Sistemi")]
    [SerializeField] private Transform adisyonPosition; // Adisyon 3D nesnesi konumu
    [SerializeField] public GameObject adisyonObject; // Adisyon 3D nesnesi 
    [SerializeField] public GameObject adisyonUI; // Adisyon UI nesnesi 
    
    // State tracking
    private bool isOccupied = false;
    private NPCGroup occupyingGroup;
    private GameObject currentGameObject;
    private string currentGameType;
    private bool gamePlaced = false;

    private void Start()
    {
        // Initialize chairs if not already set in inspector
        if (chairs.Count == 0)
        {
            Chair[] foundChairs = GetComponentsInChildren<Chair>();
            chairs.AddRange(foundChairs);
            
            // Set up chairs to reference this table
            foreach (Chair chair in chairs)
            {
                chair.SetTable(this);
            }
        }
        
        // Ensure this object is in the Usable layer
        gameObject.layer = LayerMask.NameToLayer("Usable");
    }
    
    /// <summary>
    /// Called when player interacts with this table using F key
    /// </summary>
    public void interact()
    {
        Debug.Log($"Player interacted with table {name}");
        
        // Get reference to the player
        Player player = FindObjectOfType<Player>();
        
        if (player == null) {
            Debug.LogError("Player bulunamadı!");
            return;
        }
        
        Debug.Log($"Oyuncunun elinde: {(player.inHandItem != null ? player.inHandItem.name : "hiçbir şey yok")}");
        Debug.Log($"Masada grup var mı: {HasGroup()}, Grup tam oturmuş mu: {IsGroupSeated()}");
        
        if (player.inHandItem != null)
        {
            GameItem gameItem = player.inHandItem.GetComponent<GameItem>();
            Debug.Log($"Eşya bir GameItem mi: {gameItem != null}");
            
            if (gameItem != null) {
                Debug.Log($"Oyun tipi: {gameItem.GetGameType()}, İstenen oyun: {GetRequestedGameType()}");
            }
        }
        
        if (player != null && player.inHandItem != null)
        {
            // Check if the player is holding a game
            GameItem gameItem = player.inHandItem.GetComponent<GameItem>();
            
            if (gameItem != null && HasGroup() && IsGroupSeated())
            {
                // Check if this is the requested game type
                string requestedGame = GetRequestedGameType();
                string gameType = gameItem.GetGameType();
                
                Debug.Log($"Table requested game: {requestedGame}, Player holding: {gameType}");
                
                if (requestedGame == gameType)
                {
                    Debug.Log($"Placing correct game {gameType} on table");
                    // Place the game on the table
                    PlaceGameBox(gameType);

                    // Destroy the pickup item
                    Destroy(player.inHandItem);
                    player.inHandItem = null;
                    
                    // Reset player pickup state to ensure they can pick up new items
                    player.SetPickedStatus(false);
                    //Adisyon nesnesini göster
                    adisyonObject.SetActive(true);
                    // Show message
                    player.ShowUIMessage($"{gameType} oyununu masaya koydunuz");

                }
                else
                {
                    // Wrong game type
                    Debug.LogWarning($"Wrong game type! Table wants {requestedGame}, but player has {gameType}");
                    player.ShowUIMessage($"Yanlış oyun! Bu masa {requestedGame} istiyor");
                }
            }
            else if (!HasGroup())
            {
                player.ShowUIMessage("Bu masada oturan müşteri yok");
            }
            else if (!IsGroupSeated())
            {
                player.ShowUIMessage("Müşteriler henüz oturmayı tamamlamadı");
            }
        }
    }
    
    /// <summary>
    /// Checks if the table is available
    /// </summary>
    public bool IsAvailable()
    {
        return !isOccupied;
    }
    
    /// <summary>
    /// Checks if table has enough chairs for the specified group size
    /// </summary>
    public bool CanFitGroup(int groupSize)
    {
        return chairs.Count >= groupSize;
    }

    public bool IsGamePlaced()
    {
        return gamePlaced;
    }

    /// <summary>
    /// Get all chairs associated with this table
    /// </summary>
    public List<Chair> GetChairs()
    {
        return chairs;
    }
    
    /// <summary>
    /// Sets the table as occupied by a group
    /// </summary>
    public void SetOccupiedBy(NPCGroup group)
    {
        isOccupied = true;
        occupyingGroup = group;
        Debug.Log($"Table {name} is now occupied by group");
        
    }
    
    /// <summary>
    /// Sets the table as available again
    /// </summary>
    public void SetAvailable()
    {
        isOccupied = false;
        occupyingGroup = null;
        Debug.Log($"Table {name} is now available");
    }
    // Place a game box on the table
    public void PlaceGameBox(string gameType)
    {
        currentGameType = gameType;
        
        // Remove any existing game box
        if (currentGameObject != null)
        {
            Destroy(currentGameObject);
        }
        
        // Place the new game box
        GameObject placedGameVisual = null;
        
        switch (gameType)
        {
            case "Tavla":
                placedGameVisual = tavlaGamePlaced;
                break;
            case "Iskambil":
                placedGameVisual = iskambilGamePlaced;
                break;
            case "Okey":
                placedGameVisual = okeyGamePlaced;
                break;
        }
        
        if (placedGameVisual != null && gameBoxPosition != null)
        {
            currentGameObject = Instantiate(placedGameVisual, gameBoxPosition.position, gameBoxPosition.rotation);
            currentGameObject.transform.SetParent(transform);
            
            // Notify the group
            if (occupyingGroup != null)
            {
                occupyingGroup.ReceiveGameBox(gameType);
            }

            gamePlaced = true;
        }
    }
    
    // Make the game box pickable again after NPCs leave
    public void MakeGameBoxPickable()
    {
        // Remove the placed game visual
        if (currentGameObject != null)
        {
            Destroy(currentGameObject);
        }

        //Adisyonu yok et
        adisyonObject.SetActive(false);
        // Spawn the pickable version
        GameObject pickablePrefab = null;
        
        switch (currentGameType)
        {
            case "Tavla":
                pickablePrefab = tavlaGamePrefab;
                break;
            case "Iskambil":
                pickablePrefab = iskambilGamePrefab;
                break;
            case "Okey":
                pickablePrefab = okeyGamePrefab;
                break;
        }
        
        if (pickablePrefab != null && gameBoxPosition != null)
        {
            currentGameObject = Instantiate(pickablePrefab, gameBoxPosition.position, gameBoxPosition.rotation);
            
            // Make sure it's interactable
            if (currentGameObject.GetComponent<Collider>() == null)
            {
                currentGameObject.AddComponent<BoxCollider>();
            }
            
            // Set it to the interactable layer
            currentGameObject.layer = LayerMask.NameToLayer("Interactable");
        }
        
        currentGameType = null;
        gamePlaced = false;
    }
    
    /// <summary>
    /// Get the requested game type for the group at this table
    /// </summary>
    public string GetRequestedGameType()
    {
        // İlk olarak occupyingGroup üzerinden kontrol et
        string gameType = occupyingGroup != null ? occupyingGroup.GetRequestedGame() : null;
        
        // Eğer gameType null veya boşsa, currentGameType'ı kontrol et
        if (string.IsNullOrEmpty(gameType) && !string.IsNullOrEmpty(currentGameType))
        {
            gameType = currentGameType;
            //Debug.Log($"[TABLE] Grup üzerinden oyun tipi alınamadı, currentGameType kullanılıyor: {currentGameType}");
        }
        
        //Debug.Log($"[TABLE] Masa {name} için istenen oyun tipi: {gameType}");
        return gameType;
    }
    
    /// <summary>
    /// Set the requested game type for debugging purposes
    /// </summary>
    public void SetRequestedGame(string gameType)
    {
        Debug.Log($"Setting requested game for table {name} to: {gameType}");
        // This is a workaround in case the occupyingGroup reference is lost
        currentGameType = gameType;
    }
    
    /// <summary>
    /// Check if there is a group occupying this table
    /// </summary>
    public bool HasGroup()
    {
        return occupyingGroup != null;
    }
    
    /// <summary>
    /// Check if the group at this table is fully seated
    /// </summary>
    public bool IsGroupSeated()
    {
        // Eğer grup yoksa, oturmuş sayılmaz
        if (occupyingGroup == null)
            return false;
            
        // NPCGroup'dan tüm üyelerin oturup oturmadığını kontrol et
        return occupyingGroup.IsFullySeated();
    }
    
    /// <summary>
    /// Get the game box position
    /// </summary>
    public Transform GetGameBoxPosition()
    {
        return gameBoxPosition;
    }
    
    /// <summary>
    /// Update a specific NPC's drink request on this table's UI
    /// </summary>
    public void UpdateNPCRequest(NPC npc, string request)
    {
        // This method just passes the information to any UI component or system
        // that needs to display NPC requests
        
        Debug.Log($"Table {name} updated NPC request for {npc.name} to: {request}");
        
        // If your game has a UI manager that shows these requests, you would call it here:
        // UIManager.Instance?.UpdateNPCRequest(npc, request);
    }
} 