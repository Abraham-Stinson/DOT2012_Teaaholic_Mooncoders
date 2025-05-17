using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class MagazineUI : MonoBehaviour
{
    [Header("Hand and Look UI")]
    [SerializeField] private Player player;
    [SerializeField] private GameObject onHandUI;
    [SerializeField] private GameObject hitUI;
    [SerializeField] private UnityEngine.UI.Image onHandImageUI;
    [SerializeField] private UnityEngine.UI.Image hitImageUI;
    [SerializeField] private Color teaColor = Color.magenta;

    void Start()
    {
        onHandUI.SetActive(false);
        hitUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        RefreshUI();
    }
    void RefreshUI()
    {
        if (player.inHandItem != null)
        {
            onHandUI.SetActive(true);
            if (player.inHandItem.GetComponent<Kettle>())
            {
                var kettleScript = player.inHandItem.GetComponent<Kettle>();
                onHandImageUI.fillAmount = (float)kettleScript.currentKettleMagazine / (float)kettleScript.maxKettleMagazine;
            }
            else if (player.inHandItem.GetComponent<Tea_Cup>())
            {
                var teaCupScript = player.inHandItem.GetComponent<Tea_Cup>();
                if (teaCupScript.isFillTea)
                {
                    onHandImageUI.fillAmount = (float)teaCupScript.currentTeaCupMagazine / (float)teaCupScript.maxTeaCupMagazine;
                }
                /*else if (teaCupScript.isFillOraletorCoffee)
                {
                    switch (teaCupScript.inCup)
                    {
                        case "Coffee_Powder":
                            
                            break;
                    }
                }*/
            }
            else if (player.inHandItem.GetComponent<TeaCanScript>())
            {
                var teaCanScript = player.inHandItem.GetComponent<TeaCanScript>();
                onHandImageUI.fillAmount = (float)teaCanScript.currentTeaCanMagazine / (float)teaCanScript.maxTeaCanMagazine;
            }
            else if (player.inHandItem.GetComponent<OraletAndCoffee>())
            {
                var oraletAndCoffeeScript = player.inHandItem.GetComponent<OraletAndCoffee>();
                onHandImageUI.fillAmount = (float)oraletAndCoffeeScript.currentMagazine / (float)oraletAndCoffeeScript.maxMagazine;
            }
            else
            {
                onHandUI.SetActive(false);
            }
        }
        else
        {
            onHandUI.SetActive(false);
        }

        if (Physics.Raycast(player.playerCam.position, player.playerCam.forward, out player.hit, player.rayCastRange))
        {
            var looking = player.hit.collider.gameObject;

            if (looking.GetComponent<Kettle>())
            {
                hitUI.SetActive(true);
                var kettleScript = player.hit.collider.gameObject.GetComponent<Kettle>();
                hitImageUI.fillAmount = (float)kettleScript.currentKettleMagazine / (float)kettleScript.maxKettleMagazine;
            }
            else if (looking.GetComponent<Tea_Cup>())
            {
                hitUI.SetActive(true);
                var teaCupScript = player.hit.collider.gameObject.GetComponent<Tea_Cup>();
                if (teaCupScript.isFillTea)
                {
                    hitImageUI.fillAmount = (float)teaCupScript.currentTeaCupMagazine / (float)teaCupScript.maxTeaCupMagazine;
                }
                /*else if (teaCupScript.isFillOraletorCoffee)
                {
                    switch (teaCupScript.inCup)
                    {
                        case "Coffee_Powder":
                            
                            break;
                    }
                }*/
            }
            else if (looking.GetComponent<TeaCanScript>())
            {
                hitUI.SetActive(true);
                var teaCanScript = player.hit.collider.gameObject.GetComponent<TeaCanScript>();
                hitImageUI.fillAmount = (float)teaCanScript.currentTeaCanMagazine / (float)teaCanScript.maxTeaCanMagazine;
            }
            else if (looking.GetComponent<OraletAndCoffee>())
            {
                hitUI.SetActive(true);
                var oraletAndCoffeeScript = player.hit.collider.gameObject.GetComponent<OraletAndCoffee>();
                hitImageUI.fillAmount = (float)oraletAndCoffeeScript.currentMagazine / (float)oraletAndCoffeeScript.maxMagazine;
            }
            else
            {
                hitUI.SetActive(false);
            }
        }
        else
        {
            hitUI.SetActive(false);
        }
    }
}
