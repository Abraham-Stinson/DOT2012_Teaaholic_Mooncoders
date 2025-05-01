using UnityEngine;

public class DirtyStatus : MonoBehaviour
{
    public bool isDirty=false;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void CleanDirt(){
        Debug.Log("Yıkandı");
        if(GetComponent<Tea_Cup>()!=null){
            GetComponent<Tea_Cup>().EmptyCup();
        }
        isDirty=false;
    }
}
