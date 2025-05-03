using UnityEditor;
using UnityEngine;

public class Tea_Cup : MonoBehaviour
{
    [Header("Tea_Cup")]
    [SerializeField] private float maxTeaCupMagazine = 5f;
    [SerializeField] private float maxTeaCupTeaMagazine=3f;
    [SerializeField] private float minTeaCupMagazine = 0f;
    [SerializeField] private float currentTeaCupMagazine;
    [SerializeField] private float currentTeaCupTeaMagazine;
    [SerializeField] public bool isOnTray=false;
    [SerializeField] public bool isFillOraletorCoffee=false;
    [SerializeField] public bool isFullOraletorCoffee=false;
    [SerializeField] public bool isFillTea=false;
    [SerializeField] public bool isFullTea=false;
    

    [Header ("Meshler")]

    [SerializeField] public GameObject cup; 
    [SerializeField] GameObject[] teaCupsLevels;

    
    void Start()
    {

        currentTeaCupMagazine=minTeaCupMagazine;
        currentTeaCupTeaMagazine=minTeaCupMagazine;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddTea(){
        if(!isFillOraletorCoffee&&!isFillTea){
            isFillTea=true;
        }
        if(currentTeaCupTeaMagazine<3){
            currentTeaCupMagazine+=1;
            currentTeaCupTeaMagazine+=1;
        }
        else{
            isFullTea=true;
        }
        ChangeMeshTea();
    }

    public void AddOraletOrCoffee(string product){
        Debug.Log(product+" tozu eklendi eklendi");
        isFillOraletorCoffee=true;
        
    }

    public void FillHotWater(){
        currentTeaCupMagazine=currentTeaCupMagazine+(maxTeaCupMagazine-currentTeaCupMagazine);
        isFullTea=true;
        ChangeMeshTea();
    }
#region change mesh of tea
    private void ChangeMeshTea(){
        if(currentTeaCupMagazine==0){
            //EMPTY MESH
            for(int i=0;i<teaCupsLevels.Length;i++){
                teaCupsLevels[i].SetActive(false);
            }
            
        }
        else if(currentTeaCupMagazine==1&&currentTeaCupTeaMagazine==1){
            //1 DEM DOLU MESH
            for(int i=0;i<teaCupsLevels.Length;i++){
                teaCupsLevels[i].SetActive(false);
            }
            teaCupsLevels[0].SetActive(true);
        }
        else if(currentTeaCupMagazine==2&&currentTeaCupTeaMagazine==2){
            //2DEM DOLU MESH
            for(int i=0;i<teaCupsLevels.Length;i++){
                teaCupsLevels[i].SetActive(false);
            }
            teaCupsLevels[0].SetActive(true);
            teaCupsLevels[1].SetActive(true);
        }
        else if(currentTeaCupMagazine==3&&currentTeaCupTeaMagazine==3){
            //3DEM DOLU MESH
            for(int i=0;i<teaCupsLevels.Length;i++){
                teaCupsLevels[i].SetActive(false);
            }
            teaCupsLevels[0].SetActive(true);
            teaCupsLevels[1].SetActive(true);
            teaCupsLevels[2].SetActive(true);
        }
        else if(currentTeaCupMagazine==5&&currentTeaCupTeaMagazine==1){
            //AÇIK ÇAY MESH
            for(int i=0;i<teaCupsLevels.Length;i++){
                teaCupsLevels[i].SetActive(false);
            }
            teaCupsLevels[3].SetActive(true);
        }
        else if(currentTeaCupMagazine==5&&currentTeaCupTeaMagazine==2){
            //TAVŞAN KANI ÇAY MESH
            for(int i=0;i<teaCupsLevels.Length;i++){
                teaCupsLevels[i].SetActive(false);
            }
            teaCupsLevels[4].SetActive(true);
        }
        else if(currentTeaCupMagazine==5&&currentTeaCupTeaMagazine==3){
            //DEMLİ ÇAY MESH
            for(int i=0;i<teaCupsLevels.Length;i++){
                teaCupsLevels[i].SetActive(false);
            }
            teaCupsLevels[5].SetActive(true);
        }
        else{
            Debug.Log("HATA");
        }
    }
#endregion
#region change mesh of oralets or coffee
    void ChangeMeshOraletorCoffee(){
        
    }
#endregion

    public void EmptyCup(){
        isFillOraletorCoffee=false;
        isFillTea=false;
        isFullTea=false;
        isFullOraletorCoffee=false;
        currentTeaCupMagazine=0;
        currentTeaCupTeaMagazine=0;
        ChangeMeshTea();
    }
}
