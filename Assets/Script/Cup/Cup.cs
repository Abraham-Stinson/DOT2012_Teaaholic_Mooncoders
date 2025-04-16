using NUnit.Framework;
using UnityEngine;

public class Cup : MonoBehaviour
{
    void Start()
    {
        TeaCup fullOfTea = new TeaCup(true,true,true,"Tea_Cup");

        TeaCup fullOfDirtyTea = new TeaCup(true,false,true,"Tea_Cup");

        TeaCup emptyOfClearTea = new TeaCup(false,true,false,"Tea_Cup");

        TeaCup emptyOfDirtyTea = new TeaCup(false,false,false,"Tea_Cup");

    }  
}
public class TeaCup{
    public bool isDrinkable;
    public bool isClean;
    public bool isFull;
    public string typeOfGlass;
    
    public TeaCup(bool isDrinkable, bool isClean, bool isFull, string typeOfGlass){
        this.isDrinkable=isDrinkable;
        this.isClean=isClean;
        this.isFull=isFull;
        this.typeOfGlass=typeOfGlass;
    }

    public int TeaRatio(){
        //burada çayın demini vereceğiz
        return 5;
    }    
}

