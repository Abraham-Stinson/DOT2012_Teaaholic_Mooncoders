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
    [SerializeField] public bool isFillOralet=false;
    [SerializeField] public bool isFillTea=false;
    [SerializeField] public bool isFullTea=false;
    [SerializeField] public bool isFullOralet=false;

    [Header ("Meshler")]
    [SerializeField] Mesh[] teaMeshList;
    
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
        if(!isFillOralet&&!isFillTea){
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

    public void FillHotWater(){
        currentTeaCupMagazine=currentTeaCupMagazine+(maxTeaCupMagazine-currentTeaCupMagazine);
        ChangeMeshTea();
    }

    private void ChangeMeshTea(){
        /*if(currentTeaCupMagazine==0){
            //EMPTY MESH
            GetComponent<MeshFilter>().mesh=teaMeshList[0];
        }
        else if(currentTeaCupMagazine==1&&currentTeaCupTeaMagazine==1){
            //1 DEM DOLU MESH
            GetComponent<MeshFilter>().mesh=teaMeshList[1];
        }
        else if(currentTeaCupMagazine==2&&currentTeaCupTeaMagazine==2){
            //2DEM DOLU MESH
            GetComponent<MeshFilter>().mesh=teaMeshList[2];
        }
        else if(currentTeaCupMagazine==3&&currentTeaCupTeaMagazine==3){
            //3DEM DOLU MESH
            GetComponent<MeshFilter>().mesh=teaMeshList[3];
        }
        else if(currentTeaCupMagazine==5&&currentTeaCupTeaMagazine==1){
            //AÇIK ÇAY MESH
            GetComponent<MeshFilter>().mesh=teaMeshList[4];
        }
        else if(currentTeaCupMagazine==5&&currentTeaCupTeaMagazine==2){
            //TAVŞAN KANI ÇAY MESH
            GetComponent<MeshFilter>().mesh=teaMeshList[5];
        }
        else if(currentTeaCupMagazine==5&&currentTeaCupTeaMagazine==3){
            //DEMLİ ÇAY MESH
            GetComponent<MeshFilter>().mesh=teaMeshList[6];
        }
        else{
            Debug.Log("HATA");
        }*/
    }

    public void EmptyCup(){
        isFillOralet=false;
        isFillTea=false;
        isFullTea=false;
        isFullOralet=false;
        currentTeaCupMagazine=0;
        currentTeaCupTeaMagazine=0;
        ChangeMeshTea();
    }
}
