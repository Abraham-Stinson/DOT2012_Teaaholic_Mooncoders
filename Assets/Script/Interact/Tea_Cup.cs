using UnityEditor;
using UnityEngine;

public class Tea_Cup : MonoBehaviour
{
    [Header("Tea_Cup")]
    [SerializeField] private float maxTeaCupMagazine = 5f;
    [SerializeField] private float maxTeaCupTeaMagazine = 3f;
    [SerializeField] private float minTeaCupMagazine = 0f;
    [SerializeField] private float currentTeaCupMagazine;
    [SerializeField] private float currentTeaCupTeaMagazine;
    [SerializeField] public bool isOnTray = false;
    [SerializeField] public bool isFillOraletorCoffee = false;
    [SerializeField] public bool isFullOraletorCoffee = false;
    [SerializeField] public bool isFillTea = false;
    [SerializeField] public bool isFullTea = false;
    [SerializeField] public string productType = "";

    [Header ("What is in cup")]
    [SerializeField] public string inCup="";


    [Header("Meshler")]

    [SerializeField] public GameObject cup;
    [SerializeField] GameObject[] teaCupsLevels;
    [SerializeField] GameObject[] coffeeObjects;//0 toz,1 dolu
    [SerializeField] GameObject[] orangeOraletObjects;
    [SerializeField] GameObject[] kiwiOraletObjects;
    [SerializeField] GameObject[] strawberryOraletObjects;
    [SerializeField] GameObject[] bananaOraletObjects;


    void Start()
    {
        inCup="Empty";
        currentTeaCupMagazine = minTeaCupMagazine;
        currentTeaCupTeaMagazine = minTeaCupMagazine;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddTea()
    {
        if (!isFillOraletorCoffee && !isFillTea)
        {
            isFillTea = true;
        }
        if (currentTeaCupTeaMagazine < 3)
        {
            currentTeaCupMagazine += 1;
            currentTeaCupTeaMagazine += 1;
        }
        else
        {
            isFullTea = true;
        }
        ChangeMeshTea();
    }

    public void AddOraletOrCoffee(string product)
    {
        Debug.Log(product + " tozu eklendi eklendi");
        productType = product;
        ChangeMeshOraletorCoffee(product, 0, false);
        isFillOraletorCoffee = true;

    }

    public void FillHotWaterToTea()
    {
        currentTeaCupMagazine = currentTeaCupMagazine + (maxTeaCupMagazine - currentTeaCupMagazine);
        isFullTea = true;
        ChangeMeshTea();
    }

    public void FillHotWaterToCoffeeOrOralet()
    {
        ChangeMeshOraletorCoffee(productType, 1, true);
        isFullOraletorCoffee = true;
    }
    #region change mesh of tea
    private void ChangeMeshTea()
    {
        if (currentTeaCupMagazine == 0)
        {
            //EMPTY MESH
            productType="Empty";
            for (int i = 0; i < teaCupsLevels.Length; i++)
            {
                teaCupsLevels[i].SetActive(false);
            }

        }
        else if (currentTeaCupMagazine == 1 && currentTeaCupTeaMagazine == 1)
        {
            //1 DEM DOLU MESH
            productType="Little_Brew_Tea";
            for (int i = 0; i < teaCupsLevels.Length; i++)
            {
                teaCupsLevels[i].SetActive(false);
            }
            teaCupsLevels[0].SetActive(true);
        }
        else if (currentTeaCupMagazine == 2 && currentTeaCupTeaMagazine == 2)
        {
            //2DEM DOLU MESH
            productType="Normal_Brew_Tea";
            for (int i = 0; i < teaCupsLevels.Length; i++)
            {
                teaCupsLevels[i].SetActive(false);
            }
            teaCupsLevels[0].SetActive(true);
            teaCupsLevels[1].SetActive(true);
        }
        else if (currentTeaCupMagazine == 3 && currentTeaCupTeaMagazine == 3)
        {
            //3DEM DOLU MESH
            productType="More_Brew_Tea";
            for (int i = 0; i < teaCupsLevels.Length; i++)
            {
                teaCupsLevels[i].SetActive(false);
            }
            teaCupsLevels[0].SetActive(true);
            teaCupsLevels[1].SetActive(true);
            teaCupsLevels[2].SetActive(true);
        }
        else if (currentTeaCupMagazine == 5 && currentTeaCupTeaMagazine == 1)
        {
            //AÇIK ÇAY MESH
            inCup="Light_Tea";
            for (int i = 0; i < teaCupsLevels.Length; i++)
            {
                teaCupsLevels[i].SetActive(false);
            }
            teaCupsLevels[3].SetActive(true);
        }
        else if (currentTeaCupMagazine == 5 && currentTeaCupTeaMagazine == 2)
        {
            //TAVŞAN KANI ÇAY MESH
            inCup="Rabbit_Blood_Tea";
            for (int i = 0; i < teaCupsLevels.Length; i++)
            {
                teaCupsLevels[i].SetActive(false);
            }
            teaCupsLevels[4].SetActive(true);
        }
        else if (currentTeaCupMagazine == 5 && currentTeaCupTeaMagazine == 3)
        {
            //DEMLİ ÇAY MESH
            inCup="Brewed_Tea";
            for (int i = 0; i < teaCupsLevels.Length; i++)
            {
                teaCupsLevels[i].SetActive(false);
            }
            teaCupsLevels[5].SetActive(true);
        }
        else
        {
            Debug.Log("HATA");
        }
    }
    #endregion
    #region change mesh of oralets or coffee
    void ChangeMeshOraletorCoffee(string product, int activeType, bool isHotWater)
    {
        switch (product)
        {
            case "Coffee":
                if (activeType == 0 && !isHotWater)
                {
                    coffeeObjects[activeType].SetActive(true);
                    inCup="Coffee_Powder";
                }
                else if (activeType == 1 && isHotWater)
                {
                    coffeeObjects[0].SetActive(false);
                    coffeeObjects[activeType].SetActive(true);
                    inCup="Coffee_Drink";
                }
                break;
            case "Banana":
                if (activeType == 0 && !isHotWater)
                {
                    bananaOraletObjects[activeType].SetActive(true);
                    inCup="Banana_Powder";
                }
                else if (activeType == 1 && isHotWater)
                {
                    bananaOraletObjects[0].SetActive(false);
                    bananaOraletObjects[activeType].SetActive(true);
                    inCup="Banana_Oralet";
                }
                break;
            case "Kiwi":
                if (activeType == 0 && !isHotWater)
                {
                    kiwiOraletObjects[activeType].SetActive(true);
                    inCup="Kiwi_Powder";
                }
                else if (activeType == 1 && isHotWater)
                {
                    kiwiOraletObjects[0].SetActive(false);
                    kiwiOraletObjects[activeType].SetActive(true);
                    inCup="Kiwi_Oralet";
                }
                break;
            case "Orange":
                if (activeType == 0 && !isHotWater)
                {
                    orangeOraletObjects[activeType].SetActive(true);
                    inCup="Orange_Powder";
                }
                else if (activeType == 1 && isHotWater)
                {
                    orangeOraletObjects[0].SetActive(false);
                    orangeOraletObjects[activeType].SetActive(true);
                    inCup="Orange_Oralet";
                }
                break;
            case "Strawberry":
                if (activeType == 0 && !isHotWater)
                {
                    strawberryOraletObjects[activeType].SetActive(true);
                    inCup="Strawberry_Powder";
                }
                else if (activeType == 1 && isHotWater)
                {
                    strawberryOraletObjects[0].SetActive(false);
                    strawberryOraletObjects[activeType].SetActive(true);
                    inCup="Strawberry_Oralet";
                }
                break;
            case "Clear":
                    strawberryOraletObjects[0].SetActive(false);
                    strawberryOraletObjects[1].SetActive(false);

                    orangeOraletObjects[0].SetActive(false);
                    orangeOraletObjects[1].SetActive(false);

                    kiwiOraletObjects[0].SetActive(false);
                    kiwiOraletObjects[1].SetActive(false);

                    bananaOraletObjects[0].SetActive(false);
                    bananaOraletObjects[1].SetActive(false);

                    coffeeObjects[0].SetActive(false);
                    coffeeObjects[1].SetActive(false);

                    
                break;
            default:
                Debug.Log("Hata");
                break;

        }
    }
    #endregion

    public void EmptyCup()
    {
        isFillOraletorCoffee = false;
        isFillTea = false;
        isFullTea = false;
        isFullOraletorCoffee = false;
        currentTeaCupMagazine = 0;
        currentTeaCupTeaMagazine = 0;
        inCup="Empty";
        ChangeMeshTea();
        ChangeMeshOraletorCoffee("Clear", 0, false);
    }
}
