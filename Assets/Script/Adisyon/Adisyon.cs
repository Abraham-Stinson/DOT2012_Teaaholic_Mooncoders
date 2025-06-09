using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Adisyon : MonoBehaviour, IInteractable
{
    [SerializeField] private TableController tableController; // Reference to the TableController script
    PauseMenuController pauseMenuController; // Reference to the PauseMenuController script
    PlayerMovementScript playerMovementScript; // Reference to the PlayerMovementScript script

    [Header("Adisyon Settings")]
    public bool isAdisyonOpen; // Indicates if the adisyon is open
    [SerializeField] private float adisyonTotalPrice; // Total price of the adisyon
    [SerializeField] private TextMeshProUGUI tableNameOnAdisyon; // Table name on adisyon
    //[SerializeField] private InputActionReference adisyonCloseAction;
    [SerializeField] private TextMeshProUGUI totalPriceText; // Tüm siparişin toplam fiyatını gösterecek UI elementi

    [Header("Adisyon Items")]
    public List<ReceiptItem> receiptItems; // List of items in the adisyon
    [System.Serializable]
    public class ReceiptItem
    {
        public string itemName;         // Ürün ismi
        public TextMeshProUGUI itemNameTextUI;         // Ürün ismi UI
        public float quantity;            // Adet
        public TextMeshProUGUI quantityTextUI;            // Adet UI
        public float price;             // Birim fiyat
        public GameObject plusButtonGO;   // + Butonu GameObject'i
        public Button plusButton;   // + Butonu 
        public GameObject minusButtonGO;  // - Butonu GameObject'i
        public Button minusButton;  // - Butonu

        // Total price yalnızca okunabilir, quantity * price otomatik hesaplanır
        public float TotalPrice => quantity * price;
        public TextMeshProUGUI TotalPriceUI; // Toplam fiyat UI
    }

    void Awake()
    {
        tableController = GetComponentInParent<TableController>();
        // Initialize controller and scripts references
        pauseMenuController = FindObjectOfType<PauseMenuController>();
        playerMovementScript = FindObjectOfType<PlayerMovementScript>();
    }

    void Start()
    {
        // Başlangıçta UI'ı güncelle
        UpdateAllUI();
        Debug.Log("UpdateAllUI called in Start");
    }

    /*private void OnEnable()
    {
        // Input action'ları etkinleştir
        if (adisyonCloseAction?.action != null)
        {
            adisyonCloseAction.action.Enable();
            adisyonCloseAction.action.performed += CloseAdisyonUI;
        }
    }

    private void OnDisable()
    {
        // Input action'ları devre dışı bırak
        if (adisyonCloseAction?.action != null)
        {
            adisyonCloseAction.action.performed -= CloseAdisyonUI;
            adisyonCloseAction.action.Disable();
        }
    }*/

    public void CloseAdisyonUI(/*InputAction.CallbackContext context*/)
    {
        Debug.Log("[Adisyon] CloseAdisyonUI çağrıldı"); 
        // Logic to close the adisyon UI
        if (isAdisyonOpen)
        {
            Debug.Log("[Adisyon] AdisyonUI açıldı"); 
            Time.timeScale = 1f; // Game time scale reset
            Debug.Log("Adisyon UI closed");
            if (playerMovementScript != null)
            {
                playerMovementScript.adisyonScript = null;
            }
            else
            {
                Debug.LogWarning("PlayerMovementScript is null in Adisyon.CloseAdisyonUI");
            }
            isAdisyonOpen = false;
            SoundManager.Instance.Write();
            OpenAdisyonUI(false);
        }
        else
        {
            Debug.Log("[Adisyon] Adisyon UI kapanmadı");
        }
    }

    public void interact()
    {
        // Open the adisyon UI
        OpenAdisyonUI(true);
    }

    private void OpenAdisyonUI(bool open)
    {
        if (pauseMenuController != null)
        {
            pauseMenuController.adisyonScript = this;
        }

        // Logic to open or close the adisyon UI
        if (open)
        {
            Time.timeScale = 0f; // Game time scale reset
            isAdisyonOpen = true;
            // Logic to open the adisyon UI
            Debug.Log("Adisyon UI opened");

            if (tableController != null && tableController.adisyonUI != null)
            {
                tableController.adisyonUI.SetActive(open);

                // Butonları initialize et ve UI'ı güncelle
                InitializeButtons();

                // Inspector'dan atanan butonları da kontrol et
                CheckInspectorAssignedButtons();

                UpdateAllUI();
                Debug.Log("Adisyon UI opened and InitializeButtons called");
            }
            else
            {
                Debug.LogError("TableController veya adisyonUI null!");
                return;
            }

            if (tableNameOnAdisyon != null && tableController != null)
            {
                tableNameOnAdisyon.text = tableController.tableName;
            }

            // Mouse visible
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            isAdisyonOpen = false;
            // Logic to close the adisyon UI
            Debug.Log("Adisyon UI closed");

            if (tableController != null && tableController.adisyonUI != null)
            {
                tableController.adisyonUI.SetActive(open);
            }

            // Mouse invisible
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Inspector'dan atanan butonları kontrol et
    private void CheckInspectorAssignedButtons()
    {
        // Tüm UI butonlarını bul ve onClick olaylarını kontrol et
        Button[] allButtons = tableController.adisyonUI.GetComponentsInChildren<Button>(true);

        foreach (Button btn in allButtons)
        {
            Debug.Log("Button bulundu: " + btn.name);

            // Adını kontrol ederek butonun ne için olduğunu belirle
            if (btn.name.Contains("Plus") || btn.name.Contains("plus") || btn.name.Contains("Artı") || btn.name.Contains("artı"))
            {
                // İsimden index çıkarmaya çalış
                int index = ExtractIndexFromButtonName(btn.name);
                if (index >= 0 && index < receiptItems.Count)
                {
                    // Listener ekle
                    btn.onClick.RemoveAllListeners();
                    int capturedIndex = index; // Closure problemi yaşamamak için kopyala
                    btn.onClick.AddListener(() => OnPlusButtonClick(capturedIndex));
                    Debug.Log("Plus button " + btn.name + " için listener eklendi, index: " + index);
                }
            }
            else if (btn.name.Contains("Minus") || btn.name.Contains("minus") || btn.name.Contains("Eksi") || btn.name.Contains("eksi"))
            {
                // İsimden index çıkarmaya çalış
                int index = ExtractIndexFromButtonName(btn.name);
                if (index >= 0 && index < receiptItems.Count)
                {
                    // Listener ekle
                    btn.onClick.RemoveAllListeners();
                    int capturedIndex = index; // Closure problemi yaşamamak için kopyala
                    btn.onClick.AddListener(() => OnMinusButtonClick(capturedIndex));
                    Debug.Log("Minus button " + btn.name + " için listener eklendi, index: " + index);
                }
            }
        }
    }

    // Buton adından index çıkar (örn: "PlusButton1" -> 1)
    private int ExtractIndexFromButtonName(string buttonName)
    {
        // Son karakteri rakam mı diye kontrol et
        char lastChar = buttonName[buttonName.Length - 1];
        if (char.IsDigit(lastChar))
        {
            return int.Parse(lastChar.ToString());
        }

        // Birden fazla rakam içerebilir, rakam olan kısmı bul
        string digits = "";
        foreach (char c in buttonName)
        {
            if (char.IsDigit(c))
            {
                digits += c;
            }
        }

        if (!string.IsNullOrEmpty(digits))
        {
            return int.Parse(digits);
        }

        return -1; // Index bulunamadı
    }

    // AdisyonManager'dan eklenen yeni fonksiyonlar
    void InitializeButtons()
    {
        Debug.Log($"Initializing buttons for {receiptItems.Count} items");

        for (int i = 0; i < receiptItems.Count; i++)
        {
            Debug.Log($"Processing item {i}: {receiptItems[i].itemName}");

            // Check plus button
            if (receiptItems[i].plusButton == null)
            {
                if (receiptItems[i].plusButtonGO != null)
                {
                    receiptItems[i].plusButton = receiptItems[i].plusButtonGO.GetComponent<Button>();
                    Debug.Log($"Plus button component found: {receiptItems[i].plusButton != null}");
                }
                else
                {
                    Debug.LogError($"No plus button reference found for item {i}");
                }
            }

            // Check minus button
            if (receiptItems[i].minusButton == null)
            {
                if (receiptItems[i].minusButtonGO != null)
                {
                    receiptItems[i].minusButton = receiptItems[i].minusButtonGO.GetComponent<Button>();
                    Debug.Log($"Minus button component found: {receiptItems[i].minusButton != null}");
                }
                else
                {
                    Debug.LogError($"No minus button reference found for item {i}");
                }
            }

            // Set up plus button listener
            if (receiptItems[i].plusButton != null)
            {
                int itemIndex = i;
                receiptItems[i].plusButton.onClick.RemoveAllListeners();
                receiptItems[i].plusButton.onClick.AddListener(() =>
                {
                    Debug.Log($"Plus button clicked for item {itemIndex}");
                    IncreaseQuantity(itemIndex);
                });

                // Butonun interactable durumunu kontrol et
                Debug.Log($"Plus button {i} interactable: {receiptItems[i].plusButton.interactable}");

                // Butonun GameObject'ini kontrol et
                Debug.Log($"Plus button {i} GameObject active: {receiptItems[i].plusButton.gameObject.activeInHierarchy}");
            }

            // Set up minus button listener
            if (receiptItems[i].minusButton != null)
            {
                int itemIndex = i;
                receiptItems[i].minusButton.onClick.RemoveAllListeners();
                receiptItems[i].minusButton.onClick.AddListener(() =>
                {
                    Debug.Log($"Minus button clicked for item {itemIndex}");
                    DecreaseQuantity(itemIndex);
                });

                // Butonun interactable durumunu kontrol et
                Debug.Log($"Minus button {i} interactable: {receiptItems[i].minusButton.interactable}");

                // Butonun GameObject'ini kontrol et
                Debug.Log($"Minus button {i} GameObject active: {receiptItems[i].minusButton.gameObject.activeInHierarchy}");
            }
        }
    }

    // Belirli bir ürünün miktarını artırma
    public void IncreaseQuantity(int itemIndex)
    {
        if (itemIndex >= 0 && itemIndex < receiptItems.Count)
        {
            receiptItems[itemIndex].quantity++;
            UpdateItemUI(itemIndex);
            UpdateTotalPrice();
        }
    }

    // Belirli bir ürünün miktarını azaltma
    public void DecreaseQuantity(int itemIndex)
    {
        if (itemIndex >= 0 && itemIndex < receiptItems.Count && receiptItems[itemIndex].quantity > 0)
        {
            receiptItems[itemIndex].quantity--;
            UpdateItemUI(itemIndex);
            UpdateTotalPrice();
        }
    }

    // Belirli bir ürün için UI'ı güncelleme
    void UpdateItemUI(int itemIndex)
    {
        ReceiptItem item = receiptItems[itemIndex];

        // Miktar metnini güncelle
        if (item.quantityTextUI != null)
        {
            item.quantityTextUI.text = item.quantity.ToString();
        }

        // Toplam fiyat metnini güncelle
        if (item.TotalPriceUI != null)
        {
            item.TotalPriceUI.text = item.TotalPrice.ToString("F2");
        }
    }

    // Tüm ürünler için UI'ı güncelleme
    void UpdateAllUI()
    {
        for (int i = 0; i < receiptItems.Count; i++)
        {
            UpdateItemUI(i);
        }

        UpdateTotalPrice();
    }

    // Tüm siparişin toplam fiyatını hesaplama ve UI'ı güncelleme
    void UpdateTotalPrice()
    {
        adisyonTotalPrice = 0;

        foreach (ReceiptItem item in receiptItems)
        {
            adisyonTotalPrice += item.TotalPrice;
        }

        // Toplam fiyat metnini güncelle (eğer referans verildiyse)
        if (totalPriceText != null)
        {
            totalPriceText.text = adisyonTotalPrice.ToString("F2") + " TL";
        }
    }

    // UI'dan butona doğrudan erişim sağlayan public fonksiyonlar
    public void OnPlusButtonClick(int itemIndex)
    {
        Debug.Log("OnPlusButtonClick called for index: " + itemIndex);
        IncreaseQuantity(itemIndex);
    }

    public void OnMinusButtonClick(int itemIndex)
    {
        Debug.Log("OnMinusButtonClick called for index: " + itemIndex);
        DecreaseQuantity(itemIndex);
    }

    public float GetTotalPrice()
    {
        return adisyonTotalPrice;
    }

    public void ResetAdisyon()
    {
        for (int i = 0; i < receiptItems.Count; i++)
        {
            receiptItems[i].quantity = 0;
            UpdateItemUI(i);
        }
        adisyonTotalPrice = 0;
        UpdateTotalPrice();
    }

    // Buton listener'larını temizle
    private void ClearButtonListeners()
    {
        for (int i = 0; i < receiptItems.Count; i++)
        {
            if (receiptItems[i].plusButton != null)
            {
                receiptItems[i].plusButton.onClick.RemoveAllListeners();
            }
            if (receiptItems[i].minusButton != null)
            {
                receiptItems[i].minusButton.onClick.RemoveAllListeners();
            }
        }
    }

    /*private void OnDestroy()
    {
        ClearButtonListeners();
        if (adisyonCloseAction?.action != null)
        {
            adisyonCloseAction.action.performed -= CloseAdisyonUI;
        }
    }*/

    // Inspector'dan atanabilen button callback fonksiyonları
    // Bu fonksiyonlar Button onClick olaylarına doğrudan Inspector'dan atanabilir
    public void OnItemPlus0() { OnPlusButtonClick(0); }
    public void OnItemPlus1() { OnPlusButtonClick(1); }
    public void OnItemPlus2() { OnPlusButtonClick(2); }
    public void OnItemPlus3() { OnPlusButtonClick(3); }
    public void OnItemPlus4() { OnPlusButtonClick(4); }

    public void OnItemMinus0() { OnMinusButtonClick(0); }
    public void OnItemMinus1() { OnMinusButtonClick(1); }
    public void OnItemMinus2() { OnMinusButtonClick(2); }
    public void OnItemMinus3() { OnMinusButtonClick(3); }
    public void OnItemMinus4() { OnMinusButtonClick(4); }
}