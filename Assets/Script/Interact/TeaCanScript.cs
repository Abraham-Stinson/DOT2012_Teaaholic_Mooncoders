using UnityEngine;

public class TeaCanScript : MonoBehaviour
{
    [Header("About Tea Can")]
    [SerializeField] private int maxTeaCanMagazine = 10;
    [SerializeField] public int currentTeaCanMagazine;

    [Header("Tea Can Objects")]
    [SerializeField] private GameObject[] teaCanObj; //0 kapak, 1 100, 2 75, 3 50, 4 25, 5 0
    [SerializeField] private GameObject spoon;
    void Start()
    {
        currentTeaCanMagazine = maxTeaCanMagazine;
        ChangeCurrentViewOfTeaCan();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ReduceTeaOnCan()
    {
        currentTeaCanMagazine -= 1;
        ChangeCurrentViewOfTeaCan();
    }
    void ChangeCurrentViewOfTeaCan()
    {
        if (currentTeaCanMagazine > 8)
        {
            for (int i = 1; i < teaCanObj.Length; i++)
            {
                teaCanObj[i].SetActive(false);
            }
            teaCanObj[1].SetActive(true);
        }
        else if (currentTeaCanMagazine > 6)
        {
            for (int i = 1; i < teaCanObj.Length; i++)
            {
                teaCanObj[i].SetActive(false);
            }
            teaCanObj[2].SetActive(true);
        }
        else if (currentTeaCanMagazine > 4)
        {
            for (int i = 1; i < teaCanObj.Length; i++)
            {
                teaCanObj[i].SetActive(false);
            }
            teaCanObj[3].SetActive(true);
        }
        else if (currentTeaCanMagazine > 2)
        {
            for (int i = 1; i < teaCanObj.Length; i++)
            {
                teaCanObj[i].SetActive(false);
            }
            teaCanObj[4].SetActive(true);
        }
        else if (currentTeaCanMagazine > 0)
        {
            for (int i = 1; i < teaCanObj.Length; i++)
            {
                teaCanObj[i].SetActive(false);
            }
        }
        else
        {
            Debug.Log("Hata");
        }
    }

    public void CoverPutAndRemove(bool isInHand)
    {
        teaCanObj[0].SetActive(!isInHand);
        spoon.SetActive(isInHand);
    }
}
