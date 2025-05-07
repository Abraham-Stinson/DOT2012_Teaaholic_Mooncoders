using UnityEditor;
using UnityEngine;

public class GarbageScript : MonoBehaviour
{
    public Player player;
    [Header("About Garbage Bin")]
    [SerializeField] private int currentGarbage;
    [SerializeField] private int maxGarbage = 10;

    [Header("Garbage Objects")]
    [SerializeField] private GameObject[] garbageObjs;

    [Header("Garbage Bag")]
    [SerializeField] public GameObject garbageBagObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentGarbage = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddThrashToGarbage()
    {
        currentGarbage += 1;
        if (currentGarbage == 10)
        {
            player.CreateGarbageBag(garbageBagObj,transform.position);
            currentGarbage=0;
        }
        ChangeViewOfGarbage();

    }
    void ChangeViewOfGarbage()
    {

        for (int i = 0; i < garbageObjs.Length; i++)
        {
            garbageObjs[i].SetActive(false);
        }
        if (currentGarbage > 8)
        {
            garbageObjs[0].SetActive(true);
        }
        else if (currentGarbage > 6)
        {
            garbageObjs[1].SetActive(true);
        }
        else if (currentGarbage > 3)
        {
            garbageObjs[2].SetActive(true);
        }
        else if (currentGarbage > 0)
        {
            garbageObjs[3].SetActive(true);
        }
    }

}
