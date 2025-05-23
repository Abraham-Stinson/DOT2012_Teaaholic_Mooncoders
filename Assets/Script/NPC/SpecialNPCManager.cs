using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SpecialNPCManager : MonoBehaviour
{
    public static SpecialNPCManager Instance { get; private set; }

    [System.Serializable]
    public class NPCSpawnData
    {
        public GameObject npcPrefab;
        public string npcName;
        public int spawnDay;
        public int minSpawnHour;
        public int maxSpawnHour;
        public DialogueData[] dialogueData;
    }

    [Header("Position Settings")]
    [SerializeField] private Transform[] spawnPositions;
    [SerializeField] private Transform[] waitingPositions;
    [SerializeField] private Transform[] exitPositions;

    [Header("UI References")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI npcDialogueText;
    [SerializeField] private Button[] dialogueButtons;
    [SerializeField] private TextMeshProUGUI[] buttonTexts;

    [Header("NPC Settings")]
    [SerializeField] private List<NPCSpawnData> npcSpawnDataList = new List<NPCSpawnData>();
    [SerializeField] private DayNightCycleController dayNightController;
    [SerializeField] private WearManager wearManager;
    [SerializeField] private MoneyManager moneyManager;

    private Dictionary<string, GameObject> activeNPCs = new Dictionary<string, GameObject>();
    private Dictionary<string, int> npcPositionIndices = new Dictionary<string, int>();
    private Dictionary<string, bool> npcHasInteracted = new Dictionary<string, bool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (dayNightController == null)
        {
            dayNightController = FindObjectOfType<DayNightCycleController>();
        }
        if (wearManager == null)
        {
            wearManager = FindObjectOfType<WearManager>();
        }
        if (moneyManager == null)
        {
            moneyManager = FindObjectOfType<MoneyManager>();
        }

        // UI'ı başlangıçta kapat
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }
    }

    private void Update()
    {
        CheckAndSpawnNPCs();
    }

    private void CheckAndSpawnNPCs()
    {
        if (dayNightController == null) return;

        int currentDay = dayNightController.GetCurrentDay();
        int currentHour = dayNightController.GetCurrentHour();

        foreach (var npcData in npcSpawnDataList)
        {
            // Eğer bu NPC zaten aktif değilse, spawn zamanı geldiyse ve daha önce etkileşime girilmemişse
            if (!activeNPCs.ContainsKey(npcData.npcName) && 
                !npcHasInteracted.ContainsKey(npcData.npcName) &&
                currentDay == npcData.spawnDay && 
                currentHour >= npcData.minSpawnHour && 
                currentHour <= npcData.maxSpawnHour)
            {
                SpawnNPC(npcData);
            }
        }
    }

    private int GetNextAvailablePositionIndex()
    {
        // Kullanılmayan ilk pozisyonu bul
        for (int i = 0; i < spawnPositions.Length; i++)
        {
            bool isPositionUsed = false;
            foreach (var index in npcPositionIndices.Values)
            {
                if (index == i)
                {
                    isPositionUsed = true;
                    break;
                }
            }
            if (!isPositionUsed)
            {
                return i;
            }
        }
        return 0; // Eğer boş pozisyon bulunamazsa ilk pozisyonu kullan
    }

    private void SpawnNPC(NPCSpawnData npcData)
    {
        int positionIndex = GetNextAvailablePositionIndex();
        npcPositionIndices[npcData.npcName] = positionIndex;

        GameObject npcInstance = Instantiate(npcData.npcPrefab, spawnPositions[positionIndex].position, Quaternion.identity);
        SpecialNPC specialNPC = npcInstance.GetComponent<SpecialNPC>();

        if (specialNPC != null)
        {
            // NPC'nin referanslarını ayarla
            specialNPC.SetReferences(
                npcData.npcName,
                npcData.spawnDay,
                npcData.minSpawnHour,
                npcData.maxSpawnHour,
                spawnPositions[positionIndex],
                waitingPositions[positionIndex],
                exitPositions[positionIndex],
                wearManager,
                moneyManager,
                dialogueUI,
                npcNameText,
                npcDialogueText,
                dialogueButtons,
                buttonTexts,
                npcData.dialogueData
            );

            // Aktif NPC'ler listesine ekle
            activeNPCs.Add(npcData.npcName, npcInstance);
        }
        else
        {
            Debug.LogError($"Spawn edilen NPC'de SpecialNPC component'i bulunamadı: {npcData.npcName}");
            Destroy(npcInstance);
        }
    }

    public void RemoveNPC(string npcName)
    {
        if (activeNPCs.ContainsKey(npcName))
        {
            // NPC'yi etkileşime girdi olarak işaretle
            npcHasInteracted[npcName] = true;
            
            // NPC'yi yok et
            Destroy(activeNPCs[npcName]);
            activeNPCs.Remove(npcName);
            npcPositionIndices.Remove(npcName);
        }
    }

    public DayNightCycleController GetDayNightController()
    {
        return dayNightController;
    }
} 