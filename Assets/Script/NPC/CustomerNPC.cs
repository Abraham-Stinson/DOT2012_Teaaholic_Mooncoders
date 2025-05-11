using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Individual NPC customer behavior
public class CustomerNPC : MonoBehaviour, IHighlightable
{
    private NavMeshAgent navAgent;
    private Animator animator;
    public CustomerGroup myGroup;
    private NPCCustomerSystem systemRef;
    private Transform targetPosition;
    private bool isMoving = false;
    private bool hasReachedDestination = false;
    
    // Chair reference when seated
    private Transform assignedChair;
    
    // Personal drink order
    private string myDrinkOrder = "";
    private bool hasOrderedDrink = false;

    // Interface implementation for highlight system
    public string GetHighlightText()
    {
        if (myGroup == null)
            return "New Customer";

        // Get player's held item
        GameObject playerHeldItem = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>()?.inHandItem;

        // Build info based on group state
        switch (myGroup.currentState)
        {
            case CustomerGroup.GroupState.Entering:
            case CustomerGroup.GroupState.FindingTable:
                return "Looking for a table";

            case CustomerGroup.GroupState.Seated:
            case CustomerGroup.GroupState.OrderingGame:
                return "Ready to order game";
                
            case CustomerGroup.GroupState.WaitingForGame:
                float gamePatience = Mathf.Max(0, myGroup.gameOrderPatience);
                if (playerHeldItem != null)
                {
                    bool isCorrectGame = false;
                    switch (myGroup.requestedGame)
                    {
                        case "Backgammon" when playerHeldItem.CompareTag("Backgammon"):
                        case "Cards" when playerHeldItem.CompareTag("Cards"):
                        case "Okey" when playerHeldItem.CompareTag("Okey"):
                            isCorrectGame = true;
                            break;
                    }
                    
                    if (isCorrectGame)
                        return $"Press F to Give {myGroup.requestedGame}\n(Patience: {gamePatience:0} seconds)";
                    else if (playerHeldItem.CompareTag("Backgammon") || playerHeldItem.CompareTag("Cards") || playerHeldItem.CompareTag("Okey"))
                        return $"Wrong Game! They want {myGroup.requestedGame}\n(Patience: {gamePatience:0} seconds)";
                    else
                        return $"Waiting for: {myGroup.requestedGame}\n(Patience: {gamePatience:0} seconds)";
                }
                return $"Waiting for: {myGroup.requestedGame}\n(Patience: {gamePatience:0} seconds)";
                
            case CustomerGroup.GroupState.PlayingGame:
                if (!hasOrderedDrink)
                    return $"Playing {myGroup.requestedGame}\nReady to order drink";
                else if (myGroup.drinkOrderTimerActive)
                {
                    float drinkPatience = Mathf.Max(0, myGroup.drinkOrderPatience);
                    if (myGroup.drinkOrders.TryGetValue(this, out string orderedDrink))
                    {
                        string displayDrink = GetDisplayDrinkName(orderedDrink);
                        
                        if (playerHeldItem != null && playerHeldItem.CompareTag("Tea_Cup"))
                        {
                            Tea_Cup teaCup = playerHeldItem.GetComponent<Tea_Cup>();
                            if (teaCup != null)
                            {
                                bool isCorrectDrink = false;
                                string cupContent = teaCup.inCup;

                                switch (orderedDrink.ToLower())
                                {
                                    case "coffee":
                                        isCorrectDrink = cupContent == "Coffee_Drink";
                                        break;
                                    case "orange oralet":
                                        isCorrectDrink = cupContent == "Orange_Oralet";
                                        break;
                                    case "banana oralet":
                                        isCorrectDrink = cupContent == "Banana_Oralet";
                                        break;
                                    case "kiwi oralet":
                                        isCorrectDrink = cupContent == "Kiwi_Oralet";
                                        break;
                                    case "strawberry oralet":
                                        isCorrectDrink = cupContent == "Strawberry_Oralet";
                                        break;
                                    case "light tea":
                                        isCorrectDrink = cupContent == "Light_Tea";
                                        break;
                                    case "normal tea":
                                        isCorrectDrink = cupContent == "Rabbit_Blood_Tea";
                                        break;
                                    case "strong tea":
                                        isCorrectDrink = cupContent == "Brewed_Tea";
                                        break;
                                }
                                
                                if (isCorrectDrink)
                                    return $"Press F to Serve {displayDrink}\n(Patience: {drinkPatience:0} seconds)";
                                else if (cupContent == "Empty")
                                    return $"Empty Cup\nCustomer wants: {displayDrink}\n(Patience: {drinkPatience:0} seconds)";
                                else if (cupContent.Contains("Powder"))
                                    return $"Need Hot Water\nCustomer wants: {displayDrink}\n(Patience: {drinkPatience:0} seconds)";
                                else
                                    return $"Wrong Drink!\nCustomer wants: {displayDrink}\n(Patience: {drinkPatience:0} seconds)";
                            }
                        }
                        return $"Playing {myGroup.requestedGame}\nWaiting for: {displayDrink}\n(Patience: {drinkPatience:0} seconds)";
                    }
                    else
                        return $"Playing {myGroup.requestedGame}\nWaiting for drink\n(Patience: {drinkPatience:0} seconds)";
                }
                else
                    return $"Playing {myGroup.requestedGame}";

            case CustomerGroup.GroupState.OrderingDrinks:
                return $"Playing {myGroup.requestedGame}\nOrdering drink";

            case CustomerGroup.GroupState.WaitingForDrinks:
                float drinkPatienceWait = Mathf.Max(0, myGroup.drinkOrderPatience);
                
                // Display detailed drink order information for this specific NPC
                if (myGroup.drinkOrders.TryGetValue(this, out string myOrderedDrink))
                {
                    string displayOrderedDrink = GetDisplayDrinkName(myOrderedDrink);
                    return $"Playing {myGroup.requestedGame}\nWaiting for: {displayOrderedDrink}\n(Patience: {drinkPatienceWait:0} seconds)";
                }
                
                return $"Playing {myGroup.requestedGame}\nWaiting for drinks\n(Patience: {drinkPatienceWait:0} seconds)";

            case CustomerGroup.GroupState.Enjoying:
                if (myGroup.drinkOrders.TryGetValue(this, out string myDrink))
                    return $"Playing {myGroup.requestedGame}\nDrinking {GetDisplayDrinkName(myDrink)}";
                else
                    return $"Playing {myGroup.requestedGame}";

            case CustomerGroup.GroupState.PayingBill:
                float paymentPatience = Mathf.Max(0, myGroup.paymentPatience);
                if (playerHeldItem == null) // Assuming empty hands means ready to take payment
                    return $"Press F to Take Payment\n(Patience: {paymentPatience:0} seconds)";
                else
                    return $"Hands must be empty to take payment\n(Patience: {paymentPatience:0} seconds)";

            case CustomerGroup.GroupState.Leaving:
                return "Getting ready to leave";

            default:
                return "Customer";
        }
    }
    
    void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = gameObject.AddComponent<Animator>();
        }

        // Add a collider if it doesn't have one
        if (GetComponent<Collider>() == null)
        {
            CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
            collider.height = 1.8f;
            collider.radius = 0.3f;
            collider.center = new Vector3(0, 0.9f, 0);
        }

        // Make sure NPC is in the NPC layer
        gameObject.layer = LayerMask.NameToLayer("NPC");
    }
    
    void Update()
    {
        // Update patience timers if this is the group leader
        if (myGroup != null && myGroup.leader == this)
        {
            myGroup.UpdatePatience(Time.deltaTime);
        }
    }
    
    public void SetGroup(CustomerGroup group)
    {
        myGroup = group;
    }
    
    public void SetSystemReference(NPCCustomerSystem system)
    {
        systemRef = system;
    }
    
    public void PatienceExpired(string itemType)
    {
        switch (itemType)
        {
            case "game":
                if (myGroup.currentState == CustomerGroup.GroupState.WaitingForGame)
                {
                    LeaveWithoutPaying("Waited too long for game!");
                }
                break;
                
            case "drinks":
                if (myGroup.currentState == CustomerGroup.GroupState.WaitingForDrinks)
                {
                    // Only leave without paying if no drinks were received
                    if (!myGroup.hasReceivedAnyDrinks)
                    {
                        LeaveWithoutPaying("Waited too long for drinks and received none!");
                    }
                    else
                    {
                        // If they got some drinks, they'll pay
                        PrepareToLeave();
                    }
                }
                break;
                
            case "payment":
                if (myGroup.currentState == CustomerGroup.GroupState.PayingBill)
                {
                    LeaveWithoutPaying("Waited too long at cashier!");
                }
                break;
        }
    }
    
    public void GoToDoor()
    {
        StartCoroutine(MoveToPosition(systemRef.doorPoint.position, OnReachedDoor));
    }
    
    void OnReachedDoor()
    {
        // Play door opening animation
        if (animator != null)
        {
            animator.SetTrigger("OpenDoor");
        }
        
        // Wait for animation to complete
        StartCoroutine(WaitForAnimation(1.0f, CheckForTable));
    }
    
    void CheckForTable()
    {
        if (myGroup.leader == this)
        {
            // Leader checks for empty table
            TableController emptyTable = systemRef.FindEmptyTable(myGroup.groupSize);
            
            if (emptyTable != null)
            {
                // Found a table, assign it to our group
                myGroup.assignedTable = emptyTable;
                emptyTable.SetOccupied(true);
                
                // Leader goes to table first
                GoToTable();
                
                // Tell other members to follow
                for (int i = 1; i < myGroup.members.Count; i++)
                {
                    myGroup.members[i].GoToTable();
                }
            }
            else
            {
                // No empty table, group leaves
                GoToExit();
                
                // Tell other members to follow
                for (int i = 1; i < myGroup.members.Count; i++)
                {
                    myGroup.members[i].GoToExit();
                }
            }
        }
    }
    
    void GoToTable()
    {
        // Find available chair at the table
        Transform chair = myGroup.assignedTable.GetAvailableChair();
        
        if (chair != null)
        {
            // Mark chair as occupied
            myGroup.assignedTable.OccupyChair(chair);
            assignedChair = chair;
            
            // Move to position in front of chair
            Vector3 chairPosition = chair.position;
            Vector3 approachPosition = chairPosition - chair.forward * 0.5f;
            
            // Move to chair
            StartCoroutine(MoveToPosition(approachPosition, OnReachedChair));
        }
    }
    
    void OnReachedChair()
    {
        // First disable NavMeshAgent
        if (navAgent != null)
        {
            navAgent.isStopped = true;
            navAgent.updatePosition = false;
            navAgent.updateRotation = false;
        }
        
        // Face the chair
        Vector3 lookDirection = assignedChair.forward;
        transform.rotation = Quaternion.LookRotation(lookDirection);
        
        // Play sitting animation
        if (animator != null)
        {
            animator.SetTrigger("SitDown");
        }
        
        // Wait for animation and then position on chair
        StartCoroutine(WaitForAnimation(1.0f, () => {
            // Move to exact chair position
            transform.position = assignedChair.position;
            
            // Set exact chair rotation (facing away from chair)
            transform.rotation = Quaternion.Euler(0, assignedChair.eulerAngles.y, 0);
            
            // Permanently disable NavMeshAgent (don't move when seated)
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }
            
            // Check if all group members are seated
            CheckAllSeated();
        }));
    }
    
    void CheckAllSeated()
    {
        if (myGroup.leader == this)
        {
            // Check if all members are seated
            bool allSeated = true;
            foreach (CustomerNPC member in myGroup.members)
            {
                if (member.assignedChair == null)
                {
                    allSeated = false;
                    break;
                }
            }
            
            if (allSeated)
            {
                // Everyone is seated, time to order game
                myGroup.currentState = CustomerGroup.GroupState.Seated;
                // Add a small delay before ordering to make it more natural
                StartCoroutine(WaitForAnimation(2.0f, OrderGame));
            }
        }
    }
    
    void OrderGame()
    {
        if (myGroup.leader == this && !myGroup.hasOrderedGame)
        {
            myGroup.currentState = CustomerGroup.GroupState.OrderingGame;
            string gameToOrder = "";
            
            // Order appropriate game based on group size
            if (myGroup.groupSize == 2)
            {
                // 2-person group orders either backgammon or cards
                gameToOrder = Random.value < 0.5f ? "Backgammon" : "Cards";
            }
            else
            {
                // 4-person group orders Okey
                gameToOrder = "Okey";
            }
            
            myGroup.requestedGame = gameToOrder;
            myGroup.hasOrderedGame = true;
            
            // Start patience timer for game delivery
            myGroup.ResetGamePatience();
            myGroup.currentState = CustomerGroup.GroupState.WaitingForGame;
            
            // Trigger UI notification or other game mechanics to notify player
            Debug.Log($"Group ordered: {gameToOrder} - Waiting for {myGroup.gameOrderPatience} seconds");
        }
    }
    
    // Called when player delivers the game to the table
    public void ReceiveGame()
    {
        if (myGroup.leader == this && !myGroup.hasReceivedGame)
        {
            // Get the game prefab from the system
            GameObject gamePrefab = systemRef.GetGamePrefab(myGroup.requestedGame);
            
            if (gamePrefab != null && myGroup.assignedTable != null)
            {
                // Set rotation and scale based on game type
                Quaternion rotation = Quaternion.identity;
                Vector3 scale = Vector3.one;
                float heightOffset = 0;
                
                switch (myGroup.requestedGame)
                {
                    case "Backgammon":
                        rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        scale = new Vector3(1.2f, 0.3f, 1.2f);
                        heightOffset = 0.02f; // Small additional height offset
                        break;
                    case "Cards":
                        rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        scale = new Vector3(1.0f, 0.2f, 1.0f);
                        heightOffset = 0.01f;
                        break;
                    case "Okey":
                        rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        scale = new Vector3(1.5f, 0.4f, 1.5f);
                        heightOffset = 0.03f;
                        break;
                }
                
                // Get game position directly from the table controller
                Vector3 placementPosition = myGroup.assignedTable.GetGamePosition();
                placementPosition += Vector3.up * heightOffset; // Add small extra height based on game type
                
                // Instantiate the game on the table
                GameObject gameObj = Instantiate(gamePrefab, placementPosition, rotation);
                
                // Parent to table and set local scale
                gameObj.transform.parent = myGroup.assignedTable.transform;
                gameObj.transform.localScale = scale;
                
                // Set to Interactable layer so player can pick it up
                gameObj.layer = LayerMask.NameToLayer("Interactable");
                
                // Make sure all children are also interactable
                foreach (Transform child in gameObj.transform)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Interactable");
                }
                
                // Store original properties for when it becomes interactable
                GameScaleManager scaleManager = gameObj.AddComponent<GameScaleManager>();
                scaleManager.originalScale = scale;
                scaleManager.originalRotation = rotation;
                scaleManager.gameType = myGroup.requestedGame;
                
                myGroup.gameOnTable = gameObj;
            }
            
            myGroup.hasReceivedGame = true;
            myGroup.currentState = CustomerGroup.GroupState.PlayingGame;
            
            // Now each member will order drinks individually
            foreach (CustomerNPC member in myGroup.members)
            {
                member.StartCoroutine(member.DelayedDrinkOrder());
            }
        }
    }
    
    IEnumerator DelayedDrinkOrder()
    {
        // Wait a bit before ordering drink
        yield return new WaitForSeconds(Random.Range(5f, 15f));
        
        // Order a drink if game has been received
        if (myGroup.hasReceivedGame && !hasOrderedDrink)
        {
            OrderDrink();
        }
    }
    
    void OrderDrink()
    {
        // Each member orders a random drink
        if (!hasOrderedDrink && myGroup.currentState == CustomerGroup.GroupState.PlayingGame)
        {
            myGroup.currentState = CustomerGroup.GroupState.OrderingDrinks;
            
            if (systemRef.drinkMenu.Length > 0)
            {
                // Select random drink from menu
                int drinkIndex = Random.Range(0, systemRef.drinkMenu.Length);
                string selectedDrink = systemRef.drinkMenu[drinkIndex].drinkName;
                
                // Add to group's order list
                myGroup.drinkOrders[this] = selectedDrink;
                hasOrderedDrink = true;
                
                // Start patience timer for drinks if this is the first order
                if (myGroup.drinkOrders.Count == 1)
                {
                    myGroup.ResetDrinkPatience();
                }
                
                // Check if everyone has ordered
                if (myGroup.drinkOrders.Count >= myGroup.groupSize)
                {
                    myGroup.allOrderedDrinks = true;
                    myGroup.currentState = CustomerGroup.GroupState.WaitingForDrinks;
                }
                else
                {
                    // Go back to playing state
                    myGroup.currentState = CustomerGroup.GroupState.PlayingGame;
                }
                
                Debug.Log($"NPC ordered drink: {selectedDrink} - Waiting for {myGroup.drinkOrderPatience} seconds");
            }
        }
    }
    
    // Called when player delivers drinks to the table
    public void ReceiveDrink(string drinkName)
    {
        // Check if this NPC ordered this drink
        if (myGroup.drinkOrders.TryGetValue(this, out string orderedDrink) && orderedDrink == drinkName)
        {
            // Get the Tea_Cup component from the player's hand
            GameObject playerHeldItem = GameObject.FindGameObjectWithTag("Tea_Cup");
            Tea_Cup teaCup = playerHeldItem?.GetComponent<Tea_Cup>();
            
            if (teaCup != null)
            {
                bool isCorrectDrink = false;
                string cupContent = teaCup.inCup;

                switch (drinkName.ToLower())
                {
                    case "coffee":
                        isCorrectDrink = cupContent == "Coffee_Drink";
                        break;
                    case "orange oralet":
                        isCorrectDrink = cupContent == "Orange_Oralet";
                        break;
                    case "banana oralet":
                        isCorrectDrink = cupContent == "Banana_Oralet";
                        break;
                    case "kiwi oralet":
                        isCorrectDrink = cupContent == "Kiwi_Oralet";
                        break;
                    case "strawberry oralet":
                        isCorrectDrink = cupContent == "Strawberry_Oralet";
                        break;
                    case "light tea":
                        isCorrectDrink = cupContent == "Light_Tea";
                        break;
                    case "normal tea":
                        isCorrectDrink = cupContent == "Rabbit_Blood_Tea";
                        break;
                    case "strong tea":
                        isCorrectDrink = cupContent == "Brewed_Tea";
                        break;
                }

                if (isCorrectDrink)
                {
                    // Calculate drink position on table
                    Vector3 drinkPosition = CalculateDrinkPosition();
                    
                    // Place the cup on the table
                    playerHeldItem.transform.parent = myGroup.assignedTable.transform;
                    playerHeldItem.transform.position = drinkPosition;
                    playerHeldItem.transform.rotation = Quaternion.identity;
                    
                    // Set the layer to Default (not interactable while customers are using it)
                    playerHeldItem.layer = LayerMask.NameToLayer("Default");
                    
                    // Add to group's drinks list
                    if (myGroup.drinksOnTable == null)
                        myGroup.drinksOnTable = new List<GameObject>();
                    myGroup.drinksOnTable.Add(playerHeldItem);
                    
                    Debug.Log($"NPC received their ordered drink: {drinkName}");
                    
                    // If all drinks are delivered, stop patience timer
                    bool allDrinksDelivered = true;
                    foreach (CustomerNPC member in myGroup.members)
                    {
                        if (myGroup.drinkOrders.ContainsKey(member) && !member.hasOrderedDrink)
                        {
                            allDrinksDelivered = false;
                            break;
                        }
                    }
                    
                    if (allDrinksDelivered)
                    {
                        myGroup.drinkOrderTimerActive = false;
                        myGroup.currentState = CustomerGroup.GroupState.Enjoying;
                        
                        // Start the stay timer for the group
                        if (myGroup.leader == this)
                        {
                            StartCoroutine(EnjoyTime());
                        }
                    }
                }
            }
        }
    }
    
    private Vector3 CalculateDrinkPosition()
    {
        // Get chair position and calculate drink offset
        Vector3 chairPos = assignedChair.position;
        Vector3 tableCenter = myGroup.assignedTable.transform.position;
        
        // Calculate direction from table center to chair
        Vector3 directionToChair = (chairPos - tableCenter).normalized;
        
        // Place drink slightly in front of chair position
        Vector3 drinkPosition = tableCenter + directionToChair * 0.3f; // Adjust multiplier as needed
        drinkPosition.y = tableCenter.y + 0.7f; // Adjust height as needed
        
        return drinkPosition;
    }
    
    IEnumerator EnjoyTime()
    {
        // Group stays for a random amount of time
        float stayDuration = Random.Range(systemRef.minStayTime, systemRef.maxStayTime);
        yield return new WaitForSeconds(stayDuration);
        
        // Decide if they want to reorder or leave
        if (Random.value < systemRef.reorderChance)
        {
            // Reset drink orders and order again
            myGroup.drinkOrders.Clear();
            myGroup.allOrderedDrinks = false;
            
            foreach (CustomerNPC member in myGroup.members)
            {
                member.hasOrderedDrink = false;
                member.StartCoroutine(member.DelayedDrinkOrder());
            }
        }
        else
        {
            // Time to leave
            PrepareToLeave();
        }
    }
    
    void PrepareToLeave()
    {
        if (myGroup.leader == this)
        {
            myGroup.currentState = CustomerGroup.GroupState.Leaving;
            
            // All members stand up
            foreach (CustomerNPC member in myGroup.members)
            {
                member.StandUp();
                
                // Leader goes to cashier, others go to exit
                if (member == this)
                {
                    member.GoToCashier();
                }
                else
                {
                    member.GoToExit();
                }
            }
        }
    }
    
    void StandUp()
    {
        // Re-enable NavMeshAgent
        if (navAgent != null)
        {
            navAgent.enabled = true;
            navAgent.isStopped = false;
            navAgent.updatePosition = true;
            navAgent.updateRotation = true;
        }
        
        // Play standing animation
        if (animator != null)
        {
            animator.SetTrigger("StandUp");
        }
        
        // Release the chair
        if (assignedChair != null && myGroup.assignedTable != null)
        {
            myGroup.assignedTable.ReleaseChair(assignedChair);
            assignedChair = null;
        }
    }
    
    void GoToCashier()
    {
        if (myGroup.leader == this && !myGroup.hasPaid)
        {
            myGroup.currentState = CustomerGroup.GroupState.PayingBill;
            
            // Start payment patience timer
            myGroup.ResetPaymentPatience();
            
            // Leader goes to cashier to pay
            StartCoroutine(MoveToPosition(systemRef.cashierPoint.position, WaitForInteraction));
        }
        else
        {
            // Others go to exit point and wait
            GoToExit();
        }
    }
    
    void WaitForInteraction()
    {
        // Display UI prompt for player to interact and take payment
        Debug.Log("NPC is waiting at cashier for payment interaction");
        
        // This should be replaced with your game's interaction system
        // For simulation, we'll just wait a bit and then proceed
        StartCoroutine(WaitForAnimation(5.0f, ProcessPayment));
    }
    
    public void ProcessPayment()
    {
        myGroup.hasPaid = true;
        
        // Make games and drinks interactable before leaving
        if (myGroup.gameOnTable != null)
        {
            myGroup.gameOnTable.layer = LayerMask.NameToLayer("Interactable");
            
            // Make sure all children are also interactable
            foreach (Transform child in myGroup.gameOnTable.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Interactable");
            }
        }
        
        if (myGroup.drinksOnTable != null)
        {
            foreach (GameObject drink in myGroup.drinksOnTable)
            {
                if (drink != null)
                {
                    drink.layer = LayerMask.NameToLayer("Interactable");
                }
            }
        }
        
        // Release the table but keep items on it
        if (myGroup.assignedTable != null)
        {
            // Keep game and drink objects on table
            if (myGroup.gameOnTable != null)
            {
                myGroup.gameOnTable.transform.parent = myGroup.assignedTable.transform;
                myGroup.gameOnTable = null;
            }
            
            if (myGroup.drinksOnTable != null)
            {
                foreach (GameObject drink in myGroup.drinksOnTable)
                {
                    if (drink != null)
                    {
                        drink.transform.parent = myGroup.assignedTable.transform;
                    }
                }
                myGroup.drinksOnTable.Clear();
            }
            
            myGroup.assignedTable.SetOccupied(false);
            myGroup.assignedTable = null;
        }
        
        // After payment, leader also goes to exit
        GoToExit();
    }
    
    void GoToExit()
    {
        // Move to the exit point
        StartCoroutine(MoveToPosition(systemRef.exitPoint.position, OnReachedExitPoint));
    }
    
    void OnReachedExitPoint()
    {
        hasReachedDestination = true;
        
        // If this is the leader, check if everyone has reached exit
        if (myGroup.leader == this)
        {
            bool allAtExit = true;
            foreach (CustomerNPC member in myGroup.members)
            {
                if (!member.hasReachedDestination)
                {
                    allAtExit = false;
                    break;
                }
            }
            
            if (allAtExit)
            {
                // Remove the entire group
                systemRef.RemoveGroup(myGroup);
            }
        }
    }
    
    IEnumerator MoveToPosition(Vector3 position, System.Action onComplete)
    {
        isMoving = true;
        hasReachedDestination = false;
        
        // Set destination for NavMeshAgent
        navAgent.SetDestination(position);
        
        // Activate walking animation
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }
        
        // Wait until we reach the destination
        while (navAgent.pathPending || navAgent.remainingDistance > navAgent.stoppingDistance)
        {
            yield return null;
        }
        
        // Stop walking animation
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }
        
        isMoving = false;
        
        // Call the completion callback
        if (onComplete != null)
        {
            onComplete();
        }
    }
    
    IEnumerator WaitForAnimation(float duration, System.Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        
        if (onComplete != null)
        {
            onComplete();
        }
    }

    private void LeaveWithoutPaying(string reason)
    {
        Debug.Log($"Group is leaving without paying: {reason}");
        
        // Make sure all members stand up and go to exit
        foreach (CustomerNPC member in myGroup.members)
        {
            member.StandUp();
            member.GoToExit();
        }
        
        // Leave items on table but make them interactable
        if (myGroup.gameOnTable != null)
        {
            myGroup.gameOnTable.layer = LayerMask.NameToLayer("Interactable");
            
            // Make sure all children are also interactable
            foreach (Transform child in myGroup.gameOnTable.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Interactable");
            }
            
            myGroup.gameOnTable.transform.parent = myGroup.assignedTable.transform;
            myGroup.gameOnTable = null;
        }
        
        if (myGroup.drinksOnTable != null)
        {
            foreach (GameObject drink in myGroup.drinksOnTable)
            {
                if (drink != null)
                {
                    drink.layer = LayerMask.NameToLayer("Interactable");
                    drink.transform.parent = myGroup.assignedTable.transform;
                }
            }
            myGroup.drinksOnTable.Clear();
        }
        
        // Release the table
        if (myGroup.assignedTable != null)
        {
            myGroup.assignedTable.SetOccupied(false);
            myGroup.assignedTable = null;
        }
        
        // Set state to leaving
        myGroup.currentState = CustomerGroup.GroupState.Leaving;
    }

    private string GetDisplayDrinkName(string orderedDrink)
    {
        switch (orderedDrink.ToLower())
        {
            case "coffee":
                return "Coffee";
            case "orange oralet":
                return "Orange Oralet";
            case "banana oralet":
                return "Banana Oralet";
            case "kiwi oralet":
                return "Kiwi Oralet";
            case "strawberry oralet":
                return "Strawberry Oralet";
            case "light tea":
                return "Light Tea";
            case "normal tea":
                return "Normal Tea";
            case "strong tea":
                return "Strong Tea";
            default:
                return orderedDrink;
        }
    }

    // Update the GameScaleManager to handle both scale and rotation
    public class GameScaleManager : MonoBehaviour
    {
        public Vector3 originalScale;
        public Quaternion originalRotation;
        
        // Which original game prefab this corresponds to
        public string gameType; // "Backgammon", "Cards", or "Okey"
        
        void OnEnable()
        {
            // When the object becomes interactable (picked up), restore original properties
            if (gameObject.layer == LayerMask.NameToLayer("Interactable"))
            {
                transform.localScale = originalScale;
                transform.rotation = originalRotation;
            }
        }
    }
}