using UnityEngine;

public class Kettle : MonoBehaviour
{
    [Header("About Kettle")]
    [SerializeField] private float maxKettleMagazine=10f;
    [SerializeField] private float minKettleMagazine=0f;
    [SerializeField] private float currentKettleMagazine;
    void Start()
    {
        currentKettleMagazine=minKettleMagazine;
    }

    
    void Update()
    {
        
    }

    public void PourTea(){
        currentKettleMagazine-=1;
        Debug.Log("Çay verildi");
    }
}
