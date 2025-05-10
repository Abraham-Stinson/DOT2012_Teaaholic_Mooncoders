using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public enum NPCState { Arriving, Sitting, Ordering, Waiting, Paying, Leaving }
    public NPCState currentState;

    public Transform[] tables; // Array of table transforms
    public Transform cashier; // Cashier transform
    public Transform exit; // Exit point
    public GameObject[] drinks; // Array of drink prefabs
    public GameObject[] games; // Array of game prefabs (cards, backgammon, okey)

    private NavMeshAgent agent;
    private float orderTime = 5f; // Time before they reorder or leave
    private float waitTime = 10f; // Time they wait before deciding to leave
    private int groupSize;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        groupSize = Random.Range(2, 5); // Randomly choose 2 or 4
        currentState = NPCState.Arriving;
        StartCoroutine(NPCBehavior());
    }

    private IEnumerator NPCBehavior()
    {
        while (true)
        {
            switch (currentState)
            {
                case NPCState.Arriving:
                    MoveToTable();
                    break;
                case NPCState.Sitting:
                    yield return new WaitForSeconds(1f); // Simulate sitting down
                    Order();
                    break;
                case NPCState.Ordering:
                    yield return new WaitForSeconds(orderTime);
                    DecideNextAction();
                    break;
                case NPCState.Waiting:
                    yield return new WaitForSeconds(waitTime);
                    MoveToCashier();
                    break;
                case NPCState.Paying:
                    yield return new WaitForSeconds(1f); // Simulate payment
                    MoveToExit();
                    break;
                case NPCState.Leaving:
                    yield break; // End the coroutine
            }
            yield return null;
        }
    }

    private void MoveToTable()
    {
        Transform chosenTable = ChooseTable();
        agent.SetDestination(chosenTable.position);
        currentState = NPCState.Sitting;
    }

    private Transform ChooseTable()
    {
        // Logic to choose an empty table
        // For simplicity, we return the first table
        return tables[0]; // Replace with actual logic to find an empty table
    }

    private void Order()
    {
        if (groupSize == 2)
        {
            // Order cards or backgammon
            GameObject game = Instantiate(games[Random.Range(0, 2)], transform.position, Quaternion.identity);
            Debug.Log("Ordered: " + game.name);
        }
        else if (groupSize == 4)
        {
            // Order okey
            GameObject game = Instantiate(games[2], transform.position, Quaternion.identity);
            Debug.Log("Ordered: " + game.name);
        }

        // Order drinks
        GameObject drink = Instantiate(drinks[Random.Range(0, drinks.Length)], transform.position, Quaternion.identity);
        Debug.Log("Ordered drink: " + drink.name);

        currentState = NPCState.Ordering;
    }

    private void DecideNextAction()
    {
        if (Random.value > 0.5f) // 50% chance to reorder
        {
            Order();
        }
        else
        {
            currentState = NPCState.Waiting;
        }
    }

    private void MoveToCashier()
    {
        agent.SetDestination(cashier.position);
        currentState = NPCState.Paying;
    }

    private void MoveToExit()
    {
        agent.SetDestination(exit.position);
        currentState = NPCState.Leaving;
        Debug.Log("NPC has left the tavern.");
    }
}
