using System.Collections;
using UnityEngine;
using TMPro;

public class Table : MonoBehaviour
{
    [Header("Table Settings")]
    [SerializeField] private int capacity = 4; // How many people can sit here
    [SerializeField] private Transform[] chairPositions; // Positions for chairs
    [SerializeField] private Transform gamePlacementPosition; // Where to place the game
    
    [Header("Game Objects")]
    [SerializeField] private GameObject backgammonGameObject; // Backgammon game object
    [SerializeField] private GameObject cardsGameObject; // Cards game object
    [SerializeField] private GameObject okeyGameObject; // Okey game object
    [SerializeField] private GameObject backgammonPickupObject; // Backgammon pickup object
    [SerializeField] private GameObject cardsPickupObject; // Cards pickup object
    [SerializeField] private GameObject okeyPickupObject; // Okey pickup object
    
    [Header("UI")]
    [SerializeField] private GameObject[] drinkOrderBubbles; // UI for drink orders
    [SerializeField] private TextMeshProUGUI[] drinkOrderTexts; // Text components for drink orders
    [SerializeField] private GameObject requestGameUI;
    [SerializeField] private TextMeshProUGUI requestGameText;

    private NPCGroup currentGroup;
    private string currentGame = "";
    private bool isGameActive = false;
    
    public bool IsAvailable { get; private set; } = true;
    public int Capacity => capacity;
    
    private void Awake()
    {
        // Make sure all game objects are inactive at start
        if (backgammonGameObject) backgammonGameObject.SetActive(false);
        if (cardsGameObject) cardsGameObject.SetActive(false);
        if (okeyGameObject) okeyGameObject.SetActive(false);
        if (backgammonPickupObject) backgammonPickupObject.SetActive(true);
        if (cardsPickupObject) cardsPickupObject.SetActive(true);
        if (okeyPickupObject) okeyPickupObject.SetActive(true);
        
        // Deactivate all UI elements
        if (requestGameUI) requestGameUI.SetActive(false);
        
        for (int i = 0; i < drinkOrderBubbles.Length; i++)
        {
            if (drinkOrderBubbles[i]) drinkOrderBubbles[i].SetActive(false);
        }
    }

    public Transform GetChairTransform(int index)
    {
        if (index >= 0 && index < chairPositions.Length)
        {
            return chairPositions[index];
        }
        
        Debug.LogError($"Trying to access chair position {index} but only {chairPositions.Length} chairs exist");
        return transform;
    }

    public void Claim(NPCGroup group)
    {
        currentGroup = group;
        IsAvailable = false;
    }

    public void Release()
    {
        currentGroup = null;
        IsAvailable = true;
        
        // Hide all UI elements
        HideAllUI();
    }

    public void RequestGame(string gameType)
    {
        currentGame = gameType;
        
        if (requestGameUI && requestGameText)
        {
            requestGameUI.SetActive(true);
            requestGameText.text = $"Bring {gameType} Game";
        }
    }

    public bool HasGame(string gameType)
    {
        return currentGame == gameType && isGameActive;
    }

    public void PlaceGame(string gameType)
    {
        if (gameType != currentGame)
        {
            Debug.LogWarning($"Trying to place {gameType} but group requested {currentGame}");
            return;
        }
        
        // Deactivate all pickup objects
        if (backgammonPickupObject) backgammonPickupObject.SetActive(false);
        if (cardsPickupObject) cardsPickupObject.SetActive(false);
        if (okeyPickupObject) okeyPickupObject.SetActive(false);
        
        // Activate the appropriate game object
        switch (gameType)
        {
            case "Backgammon":
                if (backgammonGameObject) backgammonGameObject.SetActive(true);
                break;
            case "Cards":
                if (cardsGameObject) cardsGameObject.SetActive(true);
                break;
            case "Okey":
                if (okeyGameObject) okeyGameObject.SetActive(true);
                break;
        }
        
        isGameActive = true;
        
        // Hide the request UI
        if (requestGameUI)
        {
            requestGameUI.SetActive(false);
        }
    }

    public void ShowDrinkOrder(NPC npc, string drinkType)
    {
        // Find the index of this NPC in the group
        int npcIndex = -1;
        if (currentGroup != null)
        {
            for (int i = 0; i < capacity; i++)
            {
                if (Vector3.Distance(npc.transform.position, chairPositions[i].position) < 0.5f)
                {
                    npcIndex = i;
                    break;
                }
            }
        }
        
        if (npcIndex >= 0 && npcIndex < drinkOrderBubbles.Length && npcIndex < drinkOrderTexts.Length)
        {
            drinkOrderBubbles[npcIndex].SetActive(true);
            drinkOrderTexts[npcIndex].text = $"I want: {drinkType}";
        }
    }

    public void HideDrinkOrder(int npcIndex)
    {
        if (npcIndex >= 0 && npcIndex < drinkOrderBubbles.Length)
        {
            drinkOrderBubbles[npcIndex].SetActive(false);
        }
    }

    public void PlaceDrink(GameObject drinkObject, int npcIndex)
    {
        if (npcIndex >= 0 && npcIndex < chairPositions.Length)
        {
            // Position the drink on the table in front of the NPC
            Vector3 drinkPosition = chairPositions[npcIndex].position;
            drinkPosition.y = transform.position.y; // Align with table height
            
            // Offset slightly toward the center of the table
            Vector3 toTableCenter = (transform.position - chairPositions[npcIndex].position).normalized;
            drinkPosition += toTableCenter * 0.3f;
            
            drinkObject.transform.position = drinkPosition;
            drinkObject.transform.parent = transform; // Make it a child of the table
            
            // Hide the order bubble
            HideDrinkOrder(npcIndex);
        }
    }

    public void RemoveGame()
    {
        if (!IsAvailable)
        {
            Debug.LogWarning("Cannot remove game while table is occupied");
            return;
        }
        
        // Deactivate game objects
        if (backgammonGameObject) backgammonGameObject.SetActive(false);
        if (cardsGameObject) cardsGameObject.SetActive(false);
        if (okeyGameObject) okeyGameObject.SetActive(false);
        
        // Activate pickup objects
        if (backgammonPickupObject) backgammonPickupObject.SetActive(true);
        if (cardsPickupObject) cardsPickupObject.SetActive(true);
        if (okeyPickupObject) okeyPickupObject.SetActive(true);
        
        isGameActive = false;
        currentGame = "";
    }

    private void HideAllUI()
    {
        if (requestGameUI) requestGameUI.SetActive(false);
        
        for (int i = 0; i < drinkOrderBubbles.Length; i++)
        {
            if (drinkOrderBubbles[i]) drinkOrderBubbles[i].SetActive(false);
        }
    }
} 