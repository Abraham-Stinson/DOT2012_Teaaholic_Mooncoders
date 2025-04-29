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

        isDirty=false;
    }
}
