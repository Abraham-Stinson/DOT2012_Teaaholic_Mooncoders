using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

[System.Serializable]
public class MarketItem
{
    [Header("Item Information")]
    public string itemName;        // Ürünün adı
    public string description;     // Ürün açıklaması

    [Header("3D Object")]
    public GameObject itemPrefab;  // Sipariş verildiğinde gelecek 3D obje prefabı

    [Header("UI Elements")]
    public Sprite itemIcon;        // UI'da gösterilecek ikon
    public float price;            // Ürün fiyatı
    public float blackMarketPrice; // Black market price will be cheaper
}

public class MarketSystem : MonoBehaviour, IInteractable
{
    public static bool isMarketOpen = false;
    public static bool isMarketSelectionOpen = false;

    [Header("Market Settings")]
    public List<MarketItem> availableItems = new List<MarketItem>();
    public GameObject marketUIObject; // Sahnedeki MarketPanel referansı
    public GameObject infoItemPrefab; // InfoItem UI prefabı
    public Transform deliveryBoxSpawnPoint;
    public GameObject deliveryBoxPrefab;
    [SerializeField] private float deliveryDelay = 30f; // Delivery delay in seconds

    [Header("Input Settings")]
    [SerializeField] private InputActionReference escapeAction;
    [SerializeField] private PlayerInput playerInput;

    [Header("UI References - Inspector'dan Ata")]
    [SerializeField] private Transform contentParent; // Market UI'daki Content parent
    [SerializeField] private TextMeshProUGUI totalPriceText; // Total price text
    [SerializeField] private Button orderButton; // Order button
    [SerializeField] private Button closeButton; // Close button

    [Header("Market Type Selection UI")]
    [SerializeField] private GameObject marketTypeSelectionUI;
    [SerializeField] private Button normalMarketButton;
    [SerializeField] private Button blackMarketButton;

    private GameObject marketUI;
    private Dictionary<string, int> cartItems = new Dictionary<string, int>();
    private Dictionary<string, MarketItem> itemDatabase = new Dictionary<string, MarketItem>();
    private float totalPrice = 0f;

    private MoneyManager moneyManager;
    private bool isUIOpen = false;
    private Dictionary<MarketItem, int> itemQuantities = new Dictionary<MarketItem, int>();

    [Header("Market Type Settings")]
    [SerializeField] private float wearAmount = 10f; // Amount of wear to add per black market order
    private bool isBlackMarket = false;
    [SerializeField] private WearManager wearManager;

    private void Start()
    {
        moneyManager = FindObjectOfType<MoneyManager>();
        InitializeItemDatabase();
        SetupInputSystem();
        wearManager = FindObjectOfType<WearManager>();
        SetupMarketTypeButtons();
        SetupUIButtons();
    }

    private void SetupInputSystem()
    {
        // PlayerInput referansı yoksa bul
        if (playerInput == null)
        {
            playerInput = FindObjectOfType<PlayerInput>();
        }

        // Input action'ları etkinleştir
        if (escapeAction != null)
        {
            escapeAction.action.Enable();
            escapeAction.action.performed += HandleEscapeInput;
        }

    }

    private void OnDestroy()
    {
        if (escapeAction != null)
        {
            escapeAction.action.performed -= HandleEscapeInput;
        }
    }

    private void HandleEscapeInput(InputAction.CallbackContext context)
    {
        if (isMarketSelectionOpen)
        {
            CloseMarketSelection();
        }
        else if (isMarketOpen)
        {
            CloseMarketUI();
        }
    }

    private void CloseMarketSelection()
    {
        if (marketTypeSelectionUI != null)
        {
            marketTypeSelectionUI.SetActive(false);
        }
        isMarketSelectionOpen = false;
        Time.timeScale = 1f;

        // Re-enable player movement and camera
        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void InitializeItemDatabase()
    {
        foreach (var item in availableItems)
        {
            itemDatabase[item.itemName] = item;
        }
    }

    private void SetupMarketTypeButtons()
    {
        if (normalMarketButton != null)
        {
            normalMarketButton.onClick.AddListener(() =>
            {
                isBlackMarket = false;
                if (marketTypeSelectionUI != null)
                {
                    marketTypeSelectionUI.SetActive(false);
                }
                isMarketSelectionOpen = false;
                OpenMarketUI();
            });
        }

        if (blackMarketButton != null)
        {
            blackMarketButton.onClick.AddListener(() =>
            {
                isBlackMarket = true;
                if (marketTypeSelectionUI != null)
                {
                    marketTypeSelectionUI.SetActive(false);
                }
                isMarketSelectionOpen = false;
                OpenMarketUI();
            });
        }
    }

    private void SetupUIButtons()
    {
        // Close button setup
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseMarketUI);
            Debug.Log("CloseButton listener eklendi (Inspector'dan)");
        }

        // Order button setup
        if (orderButton != null)
        {
            orderButton.onClick.RemoveAllListeners();
            orderButton.onClick.AddListener(PlaceOrder);
            Debug.Log("OrderButton listener eklendi (Inspector'dan)");
        }
    }

    public void interact()
    {
        if (marketTypeSelectionUI != null)
        {
            marketTypeSelectionUI.SetActive(true);
            isMarketSelectionOpen = true;
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerInput != null)
            {
                playerInput.DeactivateInput();
                playerInput.actions.FindActionMap("UI").Enable();
            }
        }
        else
        {
            Debug.LogError("Market Type Selection UI atanmamış!");
        }
    }

    public void OpenMarketUI()
    {
        if (marketUIObject != null)
        {
            marketUI = marketUIObject;
            marketUI.SetActive(true);
            isUIOpen = true;
            isMarketOpen = true;
            isMarketSelectionOpen = false;
            Time.timeScale = 0;

            // Inspector kontrolleri
            if (contentParent == null)
            {
                Debug.LogError("Content Parent Inspector'dan atanmamış!");
                return;
            }

            if (totalPriceText == null)
            {
                Debug.LogError("Total Price Text Inspector'dan atanmamış!");
            }

            if (orderButton == null)
            {
                Debug.LogError("Order Button Inspector'dan atanmamış!");
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerInput != null)
            {
                playerInput.DeactivateInput();
                playerInput.actions.FindActionMap("UI").Enable();
            }

            SetupUI();
        }
        else
        {
            Debug.LogError("Market UI Object Inspector'dan atanmamış!");
        }
    }

    private void SetupUI()
    {
        if (contentParent == null)
        {
            Debug.LogError("Content parent null!");
            return;
        }

        // Eski item'ları temizle
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        itemQuantities.Clear();
        totalPrice = 0f;

        foreach (var item in availableItems)
        {
            if (item == null || item.itemIcon == null || item.itemPrefab == null)
            {
                Debug.LogWarning("MarketItem eksik! İsim: " + (item != null ? item.itemName : "null"));
                continue;
            }

            GameObject go = Instantiate(infoItemPrefab, contentParent);
            var img = go.transform.Find("ImageOfItem")?.GetComponent<Image>();
            var name = go.transform.Find("NameOfItem")?.GetComponent<TextMeshProUGUI>();
            var price = go.transform.Find("PriceOfItem")?.GetComponent<TextMeshProUGUI>();
            var total = go.transform.Find("TotalOfItem")?.GetComponent<TextMeshProUGUI>();
            var qtyText = go.transform.Find("QuantityControls/QuantityText")?.GetComponent<TextMeshProUGUI>();
            var incBtn = go.transform.Find("QuantityControls/IncreaseButton")?.GetComponent<Button>();
            var decBtn = go.transform.Find("QuantityControls/DecreaseButton")?.GetComponent<Button>();

            if (img == null || name == null || price == null || total == null || qtyText == null || incBtn == null || decBtn == null)
            {
                Debug.LogError("InfoItem prefabında eksik component var!");
                continue;
            }

            img.sprite = item.itemIcon;
            name.text = item.itemName;

            // Update price display based on market type
            float currentPrice = isBlackMarket ? item.blackMarketPrice : item.price;
            price.text = $"{currentPrice:F2} TL";

            int quantity = 0;
            itemQuantities[item] = quantity;
            qtyText.text = quantity.ToString();
            total.text = $"{quantity * currentPrice:F2} TL";

            // Decrease button başlangıçta disable
            decBtn.interactable = false;

            // Closure problemi için item referansını kopyala
            MarketItem currentItem = item;
            
            incBtn.onClick.AddListener(() =>
            {
                int currentQuantity = itemQuantities[currentItem];
                if (currentQuantity < 10) // Maximum 10 items
                {
                    currentQuantity++;
                    itemQuantities[currentItem] = currentQuantity;
                    qtyText.text = currentQuantity.ToString();
                    float itemPrice = isBlackMarket ? currentItem.blackMarketPrice : currentItem.price;
                    total.text = $"{currentQuantity * itemPrice:F2} TL";
                    UpdateTotalPrice();

                    // Disable increase button if max quantity reached
                    if (currentQuantity >= 10)
                    {
                        incBtn.interactable = false;
                    }
                    // Enable decrease button since we have items
                    decBtn.interactable = true;
                }
            });

            decBtn.onClick.AddListener(() =>
            {
                int currentQuantity = itemQuantities[currentItem];
                if (currentQuantity > 0)
                {
                    currentQuantity--;
                    itemQuantities[currentItem] = currentQuantity;
                    qtyText.text = currentQuantity.ToString();
                    float itemPrice = isBlackMarket ? currentItem.blackMarketPrice : currentItem.price;
                    total.text = $"{currentQuantity * itemPrice:F2} TL";
                    UpdateTotalPrice();

                    // Enable increase button since we're below max
                    if (currentQuantity < 10)
                    {
                        incBtn.interactable = true;
                    }
                    // Disable decrease button if no items
                    if (currentQuantity <= 0)
                    {
                        decBtn.interactable = false;
                    }
                }
            });
        }

        UpdateTotalPrice(); // İlk açılışta total price'ı göster
    }

    private void UpdateTotalPrice()
    {
        totalPrice = 0f;
        foreach (var kvp in itemQuantities)
        {
            float itemPrice = isBlackMarket ? kvp.Key.blackMarketPrice : kvp.Key.price;
            totalPrice += itemPrice * kvp.Value;
        }

        Debug.Log($"Total Price Updated: {totalPrice:F2} TL");

        // Inspector'dan atanan TotalPriceText'i kullan
        if (totalPriceText != null)
        {
            totalPriceText.text = $"Genel Toplam: {totalPrice:F2} TL";
            Debug.Log($"Total text updated: {totalPriceText.text}");
        }
        else
        {
            Debug.LogError("TotalPriceText Inspector'dan atanmamış!");
        }

        // Inspector'dan atanan OrderButton'u güncelle
        if (orderButton != null)
        {
            bool canOrder = moneyManager != null && moneyManager.GetMoney() >= totalPrice && totalPrice > 0;
            orderButton.interactable = canOrder;
            Debug.Log($"Order button interactable: {canOrder}, Money: {moneyManager?.GetMoney()}, Total: {totalPrice}");
        }
        else
        {
            Debug.LogError("OrderButton Inspector'dan atanmamış!");
        }
    }

    public void PlaceOrder()
    {
        Debug.Log("PlaceOrder çağrıldı. Total Price: " + totalPrice);
        
        if (totalPrice <= 0)
        {
            Debug.LogWarning("Hiç ürün seçilmemiş!");
            return;
        }

        if (moneyManager != null && moneyManager.GetMoney() >= totalPrice)
        {
            moneyManager.SpendMoney(totalPrice);
            
            // Create a copy of itemQuantities for delivery box before clearing
            Dictionary<MarketItem, int> orderItems = new Dictionary<MarketItem, int>(itemQuantities);
            
            // Start delivery with delay
            StartCoroutine(DeliveryWithDelay(orderItems));

            // Add wear if using black market - BU ÖNEMLİ!
            if (isBlackMarket)
            {
                if (wearManager != null)
                {
                    Debug.Log($"Black market kullanıldı! {wearAmount} wear ekleniyor...");
                    wearManager.AddWear(wearAmount);
                }
                else
                {
                    Debug.LogError("WearManager bulunamadı! Black market wear eklenemedi!");
                }
            }

            Debug.Log("Sipariş başarılı, UI kapatılıyor...");
            
            // UI'ı kapatmadan önce verileri temizle
            itemQuantities.Clear();
            cartItems.Clear();
            totalPrice = 0f;
            
            // UI'ı kapat
            CloseMarketUI();
        }
        else
        {
            Debug.LogWarning($"Yetersiz para! Gerekli: {totalPrice}, Mevcut: {moneyManager?.GetMoney()}");
        }
    }

    private IEnumerator DeliveryWithDelay(Dictionary<MarketItem, int> orderItems)
    {
        Debug.Log($"Sipariş alındı! {deliveryDelay} saniye sonra teslim edilecek...");
        yield return new WaitForSeconds(deliveryDelay);
        SpawnDeliveryBox(orderItems);
    }

    private void SpawnDeliveryBox(Dictionary<MarketItem, int> orderItems)
    {
        // Önce sipariş edilen ürün var mı kontrol et
        bool hasAnyItems = orderItems.Any(kvp => kvp.Value > 0);
        if (!hasAnyItems)
        {
            Debug.LogWarning("No items ordered!");
            return;
        }

        // Her sipariş için ayrı kutu oluştur (eski kutuyu yok etme)
        // Delivery box spawn et
        if (deliveryBoxPrefab != null && deliveryBoxSpawnPoint != null)
        {
            GameObject newDeliveryBox = Instantiate(deliveryBoxPrefab, deliveryBoxSpawnPoint.position, deliveryBoxSpawnPoint.rotation);

            // Delivery box içeriklerini ayarla
            var deliveryContents = newDeliveryBox.GetComponent<DeliveryBox>();
            if (deliveryContents != null)
            {
                deliveryContents.SetContents(orderItems);
            }
            else
            {
                Debug.LogWarning("Delivery box component not found on the delivery box prefab!");
            }
            
            Debug.Log("Delivery box spawn edildi!");
        }
        else
        {
            Debug.LogError("Delivery box prefab veya spawn point atanmamış!");
        }
    }

    public void CloseMarketUI()
    {
        Debug.Log("CloseMarketUI çağrıldı");
        
        if (marketUI != null)
        {
            marketUI.SetActive(false);
            isUIOpen = false;
            isMarketOpen = false;
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerInput != null)
            {
                playerInput.ActivateInput();
            }
            
            Debug.Log("Market UI kapatıldı");
        }
        else
        {
            Debug.LogWarning("MarketUI null, sadece flag'leri sıfırlıyorum");
            isUIOpen = false;
            isMarketOpen = false;
            Time.timeScale = 1f;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (playerInput != null)
            {
                playerInput.ActivateInput();
            }
        }
    }
}
