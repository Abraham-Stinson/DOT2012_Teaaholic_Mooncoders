using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles UI display for items the player is holding or looking at
/// </summary>
public class MagazineUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;

    [Header("Hand and Look UI")]
    [SerializeField] private GameObject onHandUI;
    [SerializeField] private GameObject hitUI;
    [SerializeField] private UnityEngine.UI.Image onHandImageUI;
    [SerializeField] private UnityEngine.UI.Image hitImageUI;
    
    [Header("Inside Look UI")]
    [SerializeField] private GameObject insideLookUI;
    [SerializeField] private TextMeshProUGUI hitInsideTextUI;
    [SerializeField] private string[] insideLookTexts = new string[] { };

    [Header("Inside Held UI")]
    [SerializeField] private GameObject insideHeldUI;
    [SerializeField] private TextMeshProUGUI heldInsideTextUI;
    [SerializeField] private string[] insideHeldTexts = new string[] { };

    [Header("Dirty Look UI")]
    [SerializeField] private GameObject dirtyLookUI;
    [SerializeField] private TextMeshProUGUI dirtyLookTextUI;

    [Header("Dirty Held UI")]
    [SerializeField] private GameObject dirtyHeldUI;
    [SerializeField] private TextMeshProUGUI dirtyHeldTextUI;
    
    [Header("Performance Settings")]
    [SerializeField] private float refreshRate = 0.1f;
    private float nextRefreshTime;

    private void Start()
    {
        // Initialize UI states
        DisableAllUI();
        
        // Validate required components
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player == null)
            {
                Debug.LogError("MagazineUI: Player reference is missing!");
                enabled = false;
                return;
            }
        }
    }

    private void Update()
    {
        // Refresh UI at the specified rate instead of every frame
        if (Time.time >= nextRefreshTime)
        {
            RefreshUI();
            nextRefreshTime = Time.time + refreshRate;
        }
    }

    /// <summary>
    /// Disable all UI elements
    /// </summary>
    private void DisableAllUI()
    {
        if (onHandUI) onHandUI.SetActive(false);
        if (hitUI) hitUI.SetActive(false);
        if (insideLookUI) insideLookUI.SetActive(false);
        if (insideHeldUI) insideHeldUI.SetActive(false);
        if (dirtyLookUI) dirtyLookUI.SetActive(false);
        if (dirtyHeldUI) dirtyHeldUI.SetActive(false);
    }

    /// <summary>
    /// Refreshes all UI elements based on what player is holding/looking at
    /// </summary>
    private void RefreshUI()
    {
        RefreshHeldItemUI();
        RefreshLookingAtUI();
    }

    /// <summary>
    /// Updates UI for item player is currently holding
    /// </summary>
    private void RefreshHeldItemUI()
    {
        if (player == null || player.inHandItem == null)
        {
            if (onHandUI) onHandUI.SetActive(false);
            if (insideHeldUI) insideHeldUI.SetActive(false);
            if (dirtyHeldUI) dirtyHeldUI.SetActive(false);
            return;
        }

        GameObject heldItem = player.inHandItem;
        
        // Check for dirty status
        UpdateDirtyStatus(heldItem, dirtyHeldUI, dirtyHeldTextUI);
        
        // Check held item type and update UI accordingly
        if (heldItem.CompareTag("Tray"))
        {
            HandleTrayUI(heldItem);
        }
        else if (heldItem.TryGetComponent(out Kettle kettleScript))
        {
            HandleKettleUI(kettleScript, onHandUI, onHandImageUI, insideHeldUI, heldInsideTextUI, insideHeldTexts);
        }
        else if (heldItem.TryGetComponent(out Tea_Cup teaCupScript))
        {
            HandleTeaCupUI(teaCupScript, onHandUI, onHandImageUI, insideHeldUI, heldInsideTextUI, insideHeldTexts);
        }
        else if (heldItem.TryGetComponent(out TeaCanScript teaCanScript))
        {
            HandleTeaCanUI(teaCanScript, onHandUI, onHandImageUI, insideHeldUI, heldInsideTextUI, insideHeldTexts);
        }
        else if (heldItem.TryGetComponent(out OraletAndCoffee oraletAndCoffeeScript))
        {
            HandleOraletAndCoffeeUI(oraletAndCoffeeScript, onHandUI, onHandImageUI, insideHeldUI, heldInsideTextUI, insideHeldTexts);
        }
        else
        {
            // Unknown or unsupported item type
            if (onHandUI) onHandUI.SetActive(false);
            if (insideHeldUI) insideHeldUI.SetActive(false);
        }
    }

    /// <summary>
    /// Updates UI for item player is looking at
    /// </summary>
    private void RefreshLookingAtUI()
    {
        if (player == null) return;

        if (!Physics.Raycast(player.playerCam.position, player.playerCam.forward, out player.hit, player.rayCastRange))
        {
            if (hitUI) hitUI.SetActive(false);
            if (insideLookUI) insideLookUI.SetActive(false);
            if (dirtyLookUI) dirtyLookUI.SetActive(false);
            return;
        }

        GameObject lookingAt = player.hit.collider.gameObject;
        
        // Check for dirty status
        UpdateDirtyStatus(lookingAt, dirtyLookUI, dirtyLookTextUI);
        
        // Check item type and update UI accordingly
        if (lookingAt.TryGetComponent(out Kettle kettleScript))
        {
            HandleKettleUI(kettleScript, hitUI, hitImageUI, insideLookUI, hitInsideTextUI, insideLookTexts);
        }
        else if (lookingAt.TryGetComponent(out Tea_Cup teaCupScript))
        {
            HandleTeaCupUI(teaCupScript, hitUI, hitImageUI, insideLookUI, hitInsideTextUI, insideLookTexts);
        }
        else if (lookingAt.TryGetComponent(out TeaCanScript teaCanScript))
        {
            HandleTeaCanUI(teaCanScript, hitUI, hitImageUI, insideLookUI, hitInsideTextUI, insideLookTexts);
        }
        else if (lookingAt.TryGetComponent(out OraletAndCoffee oraletAndCoffeeScript))
        {
            HandleOraletAndCoffeeUI(oraletAndCoffeeScript, hitUI, hitImageUI, insideLookUI, hitInsideTextUI, insideLookTexts);
        }
        else
        {
            if (hitUI) hitUI.SetActive(false);
            if (insideLookUI) insideLookUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// Updates the dirty status UI for an object
    /// </summary>
    private void UpdateDirtyStatus(GameObject item, GameObject dirtyUI, TextMeshProUGUI dirtyTextUI)
    {
        if (item == null || dirtyUI == null || dirtyTextUI == null) return;
        
        if (item.TryGetComponent(out DirtyStatus dirtyStatus))
        {
            dirtyUI.SetActive(true);
            dirtyTextUI.text = dirtyStatus.isDirty ? "Kirlilik durumu: Kirli" : "Kirlilik durumu: Temiz";
        }
        else
        {
            dirtyUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// Handles the UI display for a tray
    /// </summary>
    private void HandleTrayUI(GameObject tray)
    {
        if (tray == null) return;
        
        // Count items on the tray
        int itemCount = tray.transform.childCount;
        
        if (onHandUI) onHandUI.SetActive(true);
        if (insideHeldUI) insideHeldUI.SetActive(true);
        
        if (onHandImageUI) onHandImageUI.fillAmount = itemCount > 0 ? 1f : 0f;
        
        if (heldInsideTextUI)
        {
            heldInsideTextUI.text = $"Tepside: {itemCount} ürün";
        }
    }
    
    /// <summary>
    /// Handles UI display for kettle objects
    /// </summary>
    private void HandleKettleUI(Kettle kettleScript, GameObject uiElement, UnityEngine.UI.Image fillImage, 
                                GameObject textContainer, TextMeshProUGUI textUI, string[] texts)
    {
        if (kettleScript == null || uiElement == null || fillImage == null || 
            textContainer == null || textUI == null) return;
        
        uiElement.SetActive(true);
        textContainer.SetActive(true);
        
        // Make sure the array has enough elements
        if (texts.Length >= 2)
        {
            fillImage.fillAmount = kettleScript.currentKettleMagazine / kettleScript.maxKettleMagazine;
            textUI.text = $"İçinde: {kettleScript.currentKettleMagazine} {texts[1]}";
        }
        else
        {
            textUI.text = $"İçinde: {kettleScript.currentKettleMagazine}";
        }
    }
    
    /// <summary>
    /// Handles UI display for tea cup objects
    /// </summary>
    private void HandleTeaCupUI(Tea_Cup teaCupScript, GameObject uiElement, UnityEngine.UI.Image fillImage, 
                              GameObject textContainer, TextMeshProUGUI textUI, string[] texts)
    {
        if (teaCupScript == null || uiElement == null || fillImage == null ||
            textContainer == null || textUI == null) return;
        
        if (teaCupScript.isFillTea && !teaCupScript.isFullTea)
        {
            uiElement.SetActive(true);
            textContainer.SetActive(true);
            fillImage.fillAmount = (float)teaCupScript.currentTeaCupMagazine / (float)teaCupScript.maxTeaCupMagazine;
            textUI.text = $"İçinde: {teaCupScript.currentTeaCupTeaMagazine} {texts[1]}";
        }
        else if (teaCupScript.isFillOraletorCoffee && !teaCupScript.isFullOraletorCoffee)
        {
            uiElement.SetActive(true);
            textContainer.SetActive(true);
            fillImage.fillAmount = 0.5f; // Half filled
            
            switch (teaCupScript.inCup)
            {
                case "Coffee_Powder":
                    textUI.text = $"İçinde: {texts[2]}";
                    break;
                case "Banana_Powder":
                    textUI.text = $"İçinde: {texts[3]}";
                    break;
                case "Kiwi_Powder":
                    textUI.text = $"İçinde: {texts[4]}";
                    break;
                case "Orange_Powder":
                    textUI.text = $"İçinde: {texts[5]}";
                    break;
                case "Strawberry_Powder":
                    textUI.text = $"İçinde: {texts[6]}";
                    break;
                default:
                    textUI.text = $"İçinde: {teaCupScript.inCup}";
                    break;
            }
        }
        else if (teaCupScript.isFullOraletorCoffee)
        {
            uiElement.SetActive(true);
            textContainer.SetActive(true);
            fillImage.fillAmount = 1.0f; // Fully filled
            
            switch (teaCupScript.inCup)
            {
                case "Coffee_Drink":
                    textUI.text = texts[12];
                    break;
                case "Banana_Oralet":
                    textUI.text = texts[13];
                    break;
                case "Kiwi_Oralet":
                    textUI.text = texts[14];
                    break;
                case "Orange_Oralet":
                    textUI.text = texts[15];
                    break;
                case "Strawberry_Oralet":
                    textUI.text = texts[16];
                    break;
                default:
                    textUI.text = $"İçinde: {teaCupScript.inCup}";
                    break;
            }
        }
        else if (teaCupScript.isFullTea && teaCupScript.currentTeaCupMagazine == 5)
        {
            uiElement.SetActive(true);
            textContainer.SetActive(true);
            fillImage.fillAmount = (float)teaCupScript.currentTeaCupMagazine / (float)teaCupScript.maxTeaCupMagazine;
            
            switch (teaCupScript.currentTeaCupTeaMagazine)
            {
                case 1:
                    textUI.text = texts[9];
                    break;
                case 2:
                    textUI.text = texts[10];
                    break;
                case 3:
                    textUI.text = texts[11];
                    break;
                default:
                    textUI.text = $"İçinde: Çay ({teaCupScript.currentTeaCupTeaMagazine})";
                    break;
            }
        }
        else if (!teaCupScript.isFillTea && !teaCupScript.isFillOraletorCoffee)
        {
            // Empty cup
            uiElement.SetActive(false);
            textContainer.SetActive(false);
        }
    }
    
    /// <summary>
    /// Handles UI display for tea can objects
    /// </summary>
    private void HandleTeaCanUI(TeaCanScript teaCanScript, GameObject uiElement, UnityEngine.UI.Image fillImage,
                              GameObject textContainer, TextMeshProUGUI textUI, string[] texts)
    {
        if (teaCanScript == null || uiElement == null || fillImage == null ||
            textContainer == null || textUI == null) return;
        
        uiElement.SetActive(true);
        textContainer.SetActive(true);
        
        // Make sure texts has enough elements
        if (texts.Length >= 8)
        {
            fillImage.fillAmount = (float)teaCanScript.currentTeaCanMagazine / (float)teaCanScript.maxTeaCanMagazine;
            textUI.text = $"İçinde: {teaCanScript.currentTeaCanMagazine} {texts[7]}";
        }
        else
        {
            fillImage.fillAmount = (float)teaCanScript.currentTeaCanMagazine / (float)teaCanScript.maxTeaCanMagazine;
            textUI.text = $"İçinde: {teaCanScript.currentTeaCanMagazine}";
        }
    }
    
    /// <summary>
    /// Handles UI display for oralet and coffee objects
    /// </summary>
    private void HandleOraletAndCoffeeUI(OraletAndCoffee oraletAndCoffeeScript, GameObject uiElement, 
                                       UnityEngine.UI.Image fillImage, GameObject textContainer, 
                                       TextMeshProUGUI textUI, string[] texts)
    {
        if (oraletAndCoffeeScript == null || uiElement == null || fillImage == null ||
            textContainer == null || textUI == null) return;
        
        uiElement.SetActive(true);
        textContainer.SetActive(true);
        
        fillImage.fillAmount = (float)oraletAndCoffeeScript.currentMagazine / (float)oraletAndCoffeeScript.maxMagazine;
        
        // Make sure texts has enough elements
        if (texts.Length < 9)
        {
            textUI.text = $"İçinde: {oraletAndCoffeeScript.currentMagazine} {oraletAndCoffeeScript.typeOfProduct}";
            return;
        }
        
        switch (oraletAndCoffeeScript.typeOfProduct)
        {
            case "Coffee":
                textUI.text = $"İçinde: {oraletAndCoffeeScript.currentMagazine} Kahve {texts[8]}";
                break;
            case "Banana":
                textUI.text = $"İçinde: {oraletAndCoffeeScript.currentMagazine} Muz Oralet {texts[8]}";
                break;
            case "Orange":
                textUI.text = $"İçinde: {oraletAndCoffeeScript.currentMagazine} Portakal Oralet {texts[8]}";
                break;
            case "Kiwi":
                textUI.text = $"İçinde: {oraletAndCoffeeScript.currentMagazine} Kivi Oralet {texts[8]}";
                break;
            case "Strawberry":
                textUI.text = $"İçinde: {oraletAndCoffeeScript.currentMagazine} Çilek Oralet {texts[8]}";
                break;
            default:
                textUI.text = $"İçinde: {oraletAndCoffeeScript.currentMagazine} {oraletAndCoffeeScript.typeOfProduct}";
                break;
        }
    }
}
