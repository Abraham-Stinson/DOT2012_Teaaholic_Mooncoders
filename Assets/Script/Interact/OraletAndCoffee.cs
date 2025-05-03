using UnityEngine;

public class OraletAndCoffee : MonoBehaviour
{
    [SerializeField] int fullMagazine = 10;
    public int currentMagazine = 10;
    public string typeOfProduct = "";

    [Header("Products Game Objects")]
    [SerializeField] private GameObject[] productParts; //0 kapak, 1 birinci seviye, 2 ikinci seviye, 3 üçüncü seviye, 4 dördüncü seviye, 5 dibi

    [SerializeField] private GameObject spoon;

    private int currentActivePartIndex = -1; // Track which product part is currently active

    void Start()
    {
        currentMagazine = fullMagazine;
        ChangeAppearanceOfProduct();
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Space))
        {
            reduceProduct();
        }*/
    }

    public void reduceProduct()
    {
        if (currentMagazine > 0)
        {
            currentMagazine -= 1;
            ChangeAppearanceOfProduct();
        }
        else
        {
            Debug.Log("Ürün tükenmiş.");
        }
    }

    void ChangeAppearanceOfProduct()
    {
        int newActiveIndex = -1; // no active by default

        if (currentMagazine > 8)
        {
            newActiveIndex = 1; // Birinci seviye
        }
        else if (currentMagazine > 6)
        {
            newActiveIndex = 2; // İkinci seviye
        }
        else if (currentMagazine > 4)
        {
            newActiveIndex = 3; // Üçüncü seviye
        }
        else if (currentMagazine > 2)
        {
            newActiveIndex = 4; // Dördüncü seviye
        }
        else if (currentMagazine > 0)
        {
            newActiveIndex = 5; // Dibi
        }
        else if (currentMagazine == 0)
        {
            newActiveIndex = -1; // None active
        }
        else
        {
            Debug.Log("Hata: Geçersiz currentMagazine değeri.");
            return;
        }

        // If different from currently active part, update active parts
        if (currentActivePartIndex != newActiveIndex)
        {
            // Disable old active part if any
            if (currentActivePartIndex != -1 && currentActivePartIndex < productParts.Length)
            {
                productParts[currentActivePartIndex].SetActive(false);
            }

            // Enable new active part if any
            if (newActiveIndex != -1 && newActiveIndex < productParts.Length)
            {
                productParts[newActiveIndex].SetActive(true);
            }

            currentActivePartIndex = newActiveIndex;
        }
        // else: no change needed, same active part keeps active
    }

    public void CoverPutAndRemove(bool isInHand)
    {
        productParts[0].SetActive(!isInHand);
        spoon.SetActive(isInHand);
    }
}
