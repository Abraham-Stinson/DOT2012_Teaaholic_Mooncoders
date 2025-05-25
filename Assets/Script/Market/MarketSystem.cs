using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;

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
    [TextArea(2, 3)]
    public string uiDescription;   // UI'da gösterilecek kısa açıklama
}

public class MarketSystem : MonoBehaviour, IInteractable
{
    public static bool isMarketOpen = false; // Static variable to track market state

    [Header("Market Settings")]
    public List<MarketItem> availableItems = new List<MarketItem>();
    public GameObject marketUIObject; // Sahnedeki MarketPanel referansı
    public GameObject infoItemPrefab; // InfoItem UI prefabı
    public Transform deliveryBoxSpawnPoint;
    public GameObject deliveryBoxPrefab;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference escapeAction;
    [SerializeField] private PlayerInput playerInput;

    [Header("UI References")]
    private GameObject marketUI;
    private Dictionary<string, int> cartItems = new Dictionary<string, int>();
    private Dictionary<string, MarketItem> itemDatabase = new Dictionary<string, MarketItem>();
    private float totalPrice = 0f;

    private MoneyManager moneyManager;
    private GameObject currentDeliveryBox;
    private bool isUIOpen = false;
    private Transform contentParent;
    private Dictionary<MarketItem, int> itemQuantities = new Dictionary<MarketItem, int>();

    private void Start()
    {
        moneyManager = FindObjectOfType<MoneyManager>();
        InitializeItemDatabase();
        SetupInputSystem();
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
            escapeAction.action.performed += OnEscapePressed;
        }
    }

    private void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (isUIOpen)
        {
            CloseMarketUI();
        }
    }

    private void OnDestroy()
    {
        // Input action'ları devre dışı bırak
        if (escapeAction != null)
        {
            escapeAction.action.performed -= OnEscapePressed;
            escapeAction.action.Disable();
        }
    }

    private void InitializeItemDatabase()
    {
        foreach (var item in availableItems)
        {
            itemDatabase[item.itemName] = item;
        }
    }

    public void interact()
    {
        if (marketUI == null)
        {
            marketUI = marketUIObject;
            contentParent = marketUI.transform.Find("ItemsContainer/Viewport/Content");
            SetupUI();
        }
        marketUI.SetActive(true);

        isUIOpen = true;
        isMarketOpen = true;
        Time.timeScale = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null)
        {
            playerInput.DeactivateInput();
            playerInput.actions.FindActionMap("UI").Enable();
        }
    }

    private void SetupUI()
    {
        if (contentParent == null)
        {
            Debug.LogError("Content parent bulunamadı! Hiyerarşi: ItemsContainer/Viewport/Content");
            return;
        }

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        itemQuantities.Clear();

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
            price.text = $"{item.price:F2} TL";
            int quantity = 0;
            itemQuantities[item] = quantity;
            qtyText.text = quantity.ToString();
            total.text = $"{quantity * item.price:F2} TL";

            incBtn.onClick.AddListener(() =>
            {
                if (quantity < 10) // Maximum 10 items
                {
                    quantity++;
                    itemQuantities[item] = quantity;
                    qtyText.text = quantity.ToString();
                    total.text = $"{quantity * item.price:F2} TL";
                    UpdateTotalPrice();
                    
                    // Disable increase button if max quantity reached
                    if (quantity >= 10)
                    {
                        incBtn.interactable = false;
                    }
                    // Enable decrease button since we have items
                    decBtn.interactable = true;
                }
            });
            decBtn.onClick.AddListener(() =>
            {
                if (quantity > 0)
                {
                    quantity--;
                    itemQuantities[item] = quantity;
                    qtyText.text = quantity.ToString();
                    total.text = $"{quantity * item.price:F2} TL";
                    UpdateTotalPrice();
                    
                    // Enable increase button since we're below max
                    if (quantity < 10)
                    {
                        incBtn.interactable = true;
                    }
                    // Disable decrease button if no items
                    if (quantity <= 0)
                    {
                        decBtn.interactable = false;
                    }
                }
            });
        }

        UpdateTotalPrice();
    }

    private void UpdateItemQuantity(string itemName, int change)
    {
        if (!cartItems.ContainsKey(itemName))
        {
            cartItems[itemName] = 0;
        }

        int newQuantity = cartItems[itemName] + change;
        if (newQuantity >= 0)
        {
            cartItems[itemName] = newQuantity;
            UpdateTotalPrice();
            UpdateUI();
        }
    }

    private void UpdateTotalPrice()
    {
        totalPrice = 0f;
        foreach (var kvp in itemQuantities)
            totalPrice += kvp.Key.price * kvp.Value;

        // Genel toplamı güncelle
        var totalText = marketUI.transform.Find("TotalPriceText")?.GetComponent<TextMeshProUGUI>();
        if (totalText != null)
            totalText.text = $"Genel Toplam: {totalPrice:F2} TL";
    }

    private void UpdateUI()
    {
        if (marketUI != null)
        {
            TextMeshProUGUI totalPriceText = marketUI.transform.Find("TotalPriceText").GetComponent<TextMeshProUGUI>();
            Button orderButton = marketUI.transform.Find("OrderButton").GetComponent<Button>();

            totalPriceText.text = $"Total: {totalPrice:F2} TL";
            orderButton.interactable = moneyManager.GetMoney() >= totalPrice;
        }
    }

    public void PlaceOrder()
    {
        if (moneyManager.GetMoney() >= totalPrice)
        {
            moneyManager.SpendMoney(totalPrice);
            SpawnDeliveryBox();
            
            // Tüm ürün miktarlarını sıfırla
            itemQuantities.Clear();
            cartItems.Clear();
            
            UpdateTotalPrice();
            SetupUI(); // UI'ı sıfırlanmış değerlerle yeniden oluştur
            
            marketUI.SetActive(false);
            isUIOpen = false;
            isMarketOpen = false;
            Time.timeScale = 1;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerInput != null)
            {
                playerInput.ActivateInput();
            }
        }
    }

    private void SpawnDeliveryBox()
    {
        // Önce sipariş edilen ürün var mı kontrol et
        bool hasAnyItems = itemQuantities.Any(kvp => kvp.Value > 0);
        if (!hasAnyItems)
        {
            Debug.LogWarning("No items ordered!");
            return;
        }

        if (currentDeliveryBox != null)
        {
            Destroy(currentDeliveryBox);
        }

        currentDeliveryBox = Instantiate(deliveryBoxPrefab, deliveryBoxSpawnPoint.position, Quaternion.identity);
        DeliveryBox deliveryBoxScript = currentDeliveryBox.GetComponent<DeliveryBox>();
        if (deliveryBoxScript == null)
        {
            deliveryBoxScript = currentDeliveryBox.AddComponent<DeliveryBox>();
        }

        Debug.Log("Initializing delivery box with ordered items...");
        deliveryBoxScript.Initialize(itemQuantities, itemDatabase);
    }

    public void CloseMarketUI()
    {
        if (marketUI != null)
        {
            marketUI.SetActive(false);
        }
        isUIOpen = false;
        isMarketOpen = false;
        Time.timeScale = 1;

        // Tüm adetleri sıfırla
        foreach (var key in new List<MarketItem>(itemQuantities.Keys))
            itemQuantities[key] = 0;

        SetupUI();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerInput != null)
        {
            playerInput.ActivateInput();
        }
    }
}