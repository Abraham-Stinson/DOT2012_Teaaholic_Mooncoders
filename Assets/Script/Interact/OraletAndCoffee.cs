using System.Net.Http.Headers;
using UnityEngine;

public class OraletAndCoffee : MonoBehaviour
{
    [SerializeField] int fullMagazine=10;
    public int currentMagazine=10;
    public string typeOfProduct="";

    [Header ("Products Game Objects")]
    [SerializeField] private GameObject[] productParts; //0 kapak, 1 birinci seviye, 2 ikici seviye, 3 ucuncu seviye, 4 dorduncu seviye, 5 ise dibi

    [SerializeField] private GameObject spoon;
    void Start()
    {
        currentMagazine=fullMagazine;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void reduceProduct(){
        currentMagazine-=1;
        ChangeAppearanceOfProduct();
    }

    void ChangeAppearanceOfProduct(){
        if(currentMagazine>8){
            for (int i=1; i<productParts.Length;i++){
                productParts[i].SetActive(false);
            }
            for (int i=1; i<productParts.Length;i++){
                productParts[i].SetActive(true);
            }
        }
        else if (currentMagazine>6){
            for (int i=1; i<productParts.Length;i++){
                productParts[i].SetActive(false);
            }
            for (int i=2; i<productParts.Length;i++){
                productParts[i].SetActive(true);
            }
        }
        else if (currentMagazine>4){
            for (int i=1; i<productParts.Length;i++){
                productParts[i].SetActive(false);
            }
            for (int i=3; i<productParts.Length;i++){
                productParts[i].SetActive(true);
            }
        }
        else if (currentMagazine>2){
            for (int i=1; i<productParts.Length;i++){
                productParts[i].SetActive(false);
            }
            for (int i=4; i<productParts.Length;i++){
                productParts[i].SetActive(true);
            }
        }
        else if (currentMagazine>0){
            for (int i=1; i<productParts.Length;i++){
                productParts[i].SetActive(false);
            }
            for (int i=5; i<productParts.Length;i++){
                productParts[i].SetActive(true);
            }
        }
        else if(currentMagazine==0){
            for (int i=1; i<productParts.Length;i++){
                productParts[i].SetActive(false);
            }
        }
        else{
            Debug.Log("Hata");
        }
    }

    void Cover(bool isInHand){
        if(isInHand){
            productParts[0].SetActive(false);
        }
        else{
            productParts[0].SetActive(true);
        }
    }
}
