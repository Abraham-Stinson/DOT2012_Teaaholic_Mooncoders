using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class representing a customer group
public class CustomerGroup
{
    public int groupSize;
    public List<CustomerNPC> members;
    public CustomerNPC leader;
    public TableController assignedTable;
    public string requestedGame;
    public Dictionary<CustomerNPC, string> drinkOrders = new Dictionary<CustomerNPC, string>();
    public bool hasOrderedGame = false;
    public bool hasReceivedGame = false;
    public bool allOrderedDrinks = false;
    public bool hasPaid = false;
    public GameObject gameOnTable;
    public List<GameObject> drinksOnTable;
    
    // Patience tracking
    public float initialGameOrderPatience = 60f; // 1 minute to wait for game
    public float initialDrinkOrderPatience = 90f; // 1.5 minutes to wait for drinks
    public float initialPaymentPatience = 45f; // 45 seconds to wait for payment
    public float gameOrderPatience;
    public float drinkOrderPatience;
    public float paymentPatience;
    public bool gameOrderTimerActive = false;
    public bool drinkOrderTimerActive = false;
    public bool paymentTimerActive = false;
    public bool hasReceivedAnyDrinks = false; // Track if group received any drinks
    
    // Track group state
    public enum GroupState
    {
        Entering,
        FindingTable,
        Seated,
        OrderingGame,
        WaitingForGame,
        PlayingGame,
        OrderingDrinks,
        WaitingForDrinks,
        Enjoying,
        Leaving,
        PayingBill
    }
    
    public GroupState currentState = GroupState.Entering;
    
    public void ResetGamePatience()
    {
        gameOrderPatience = initialGameOrderPatience;
        gameOrderTimerActive = true;
    }
    
    public void ResetDrinkPatience()
    {
        drinkOrderPatience = initialDrinkOrderPatience;
        drinkOrderTimerActive = true;
    }
    
    public void ResetPaymentPatience()
    {
        paymentPatience = initialPaymentPatience;
        paymentTimerActive = true;
    }
    
    public bool AreAllDrinksOrdered()
    {
        return drinkOrders.Count >= groupSize;
    }
    
    public void UpdatePatience(float deltaTime)
    {
        // Update game patience timer
        if (gameOrderTimerActive)
        {
            gameOrderPatience -= deltaTime;
            if (gameOrderPatience <= 0f)
            {
                // Run out of patience for game
                gameOrderTimerActive = false;
                if (leader != null)
                {
                    leader.PatienceExpired("game");
                }
            }
        }
        
        // Update drink patience timer
        if (drinkOrderTimerActive)
        {
            drinkOrderPatience -= deltaTime;
            if (drinkOrderPatience <= 0f)
            {
                // Run out of patience for drinks
                drinkOrderTimerActive = false;
                if (leader != null)
                {
                    leader.PatienceExpired("drinks");
                }
            }
        }

        // Update payment patience timer
        if (paymentTimerActive)
        {
            paymentPatience -= deltaTime;
            if (paymentPatience <= 0f)
            {
                // Run out of patience while waiting to pay
                paymentTimerActive = false;
                if (leader != null)
                {
                    leader.PatienceExpired("payment");
                }
            }
        }
    }
}