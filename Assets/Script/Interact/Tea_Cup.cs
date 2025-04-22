using UnityEngine;

public class Tea_Cup : MonoBehaviour
{
    [Header("Tea_Cup")]
    [SerializeField] private float maxTeaCupMagazine = 5f;
    [SerializeField] private float maxTeaCupTeaMagazine=3f;
    [SerializeField] private float minTeaCupMagazine = 0f;
    [SerializeField] private float currentTeaCupMagazine;
    [SerializeField] private float currentTeaCupTeaMagazine;
    
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
        if(currentTeaCupTeaMagazine<3){
            currentTeaCupMagazine+=1;
            currentTeaCupTeaMagazine+=1;
            Debug.Log("Çay eklendi");
        }
    }
}
