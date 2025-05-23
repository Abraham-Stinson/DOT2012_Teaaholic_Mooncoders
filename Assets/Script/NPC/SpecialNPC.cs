using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class SpecialNPC : MonoBehaviour
{
    public static bool isInAnyDialogue = false;

    [Header("NPC Settings")]
    [SerializeField] private string npcName = "Özel NPC";
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float endDialogueEndDestroyDelay = 20f;

    [Header("Spawn Settings")]
    [SerializeField] private int spawnDay;
    [SerializeField] private int minSpawnHour;
    [SerializeField] private int maxSpawnHour;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private Transform waitingPosition;
    [SerializeField] private Transform exitPosition;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private WearManager wearManager;
    [SerializeField] private MoneyManager moneyManager;

    // UI referansları
    private GameObject dialogueUI;
    private TextMeshProUGUI npcNameText;
    private TextMeshProUGUI npcDialogueText;
    private Button[] dialogueButtons;
    private TextMeshProUGUI[] buttonTexts;
    private DialogueData[] dialogueData;

    private bool isInDialogue = false;
    private int currentDialogueIndex = 0;
    private bool hasSpawned = false;
    public bool hasReachedWaitingPosition = false;
    public bool hasInteracted = false; // Yeni değişken

    private void Start()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        // NavMeshAgent'ı başlangıçta devre dışı bırak
        if (navMeshAgent != null)
            navMeshAgent.enabled = false;

        // Butonları ayarla
        SetupDialogueButtons();
    }

    private void SetupDialogueButtons()
    {
        if (dialogueButtons == null || dialogueButtons.Length == 0) return;

        for (int i = 0; i < dialogueButtons.Length; i++)
        {
            int buttonIndex = i;
            if (dialogueButtons[i] != null)
            {
                dialogueButtons[i].onClick.RemoveAllListeners();
                dialogueButtons[i].onClick.AddListener(() => OnDialogueOptionSelected(buttonIndex));
                Debug.Log($"Buton {i} için listener eklendi");
            }
        }
    }

    private void Update()
    {
        if (!hasSpawned && SpecialNPCManager.Instance != null)
        {
            CheckSpawnConditions();
        }

        // NavMeshAgent'ın hedefine ulaşıp ulaşmadığını kontrol et
        if (hasSpawned && navMeshAgent != null && navMeshAgent.enabled)
        {
            if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.1f)
            {
                animator.SetTrigger("Idle");
                navMeshAgent.enabled = false;
                hasReachedWaitingPosition = true;
                Debug.Log($"NPC {npcName} bekleme noktasına ulaştı");
            }
        }

        // NPC'yi player'a döndür - sadece idle durumunda ve hareket etmiyorken
        if (hasSpawned && !isInDialogue && navMeshAgent != null && !navMeshAgent.enabled)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Vector3 direction = player.transform.position - transform.position;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
        }
    }

    private void CheckSpawnConditions()
    {
        if (SpecialNPCManager.Instance != null && SpecialNPCManager.Instance.GetDayNightController() != null)
        {
            DayNightCycleController dayNightController = SpecialNPCManager.Instance.GetDayNightController();
            if (dayNightController.GetCurrentDay() == spawnDay)
            {
                int currentHour = dayNightController.GetCurrentHour();
                if (currentHour >= minSpawnHour && currentHour <= maxSpawnHour)
                {
                    SpawnNPC();
                }
            }
        }
    }

    private void SpawnNPC()
    {
        hasSpawned = true;
        transform.position = spawnPosition.position;
        
        // NavMeshAgent'ı aktifleştir
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
            navMeshAgent.speed = moveSpeed;
            navMeshAgent.SetDestination(waitingPosition.position);
            animator.SetTrigger("Walk");
            Debug.Log($"NPC {npcName} spawn oldu ve bekleme noktasına gidiyor");
        }
        else
        {
            Debug.LogError($"NPC {npcName} için NavMeshAgent bulunamadı!");
        }
    }

    public void interact()
    {
        // Daha önce etkileşime girildiyse engelle
        if (hasInteracted)
        {
            Debug.Log($"NPC {npcName} ile zaten konuştunuz.");
            return;
        }

        // Sadece bekleme noktasına ulaştıysa ve diyalogda değilse etkileşime izin ver
        if (!isInDialogue && hasReachedWaitingPosition)
        {
            hasInteracted = true; // Etkileşimi işaretle
            StartDialogue();
        }
        else if (!hasReachedWaitingPosition)
        {
            Debug.Log($"NPC {npcName} henüz bekleme noktasına ulaşmadı");
        }
    }

    public void StartDialogue()
    {
        if (isInDialogue) return;

        isInDialogue = true;
        isInAnyDialogue = true;
        currentDialogueIndex = 0;
        
        // Pause game
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Show dialogue UI
        dialogueUI.SetActive(true);
        if (npcNameText != null)
        {
            npcNameText.text = npcName;
        }
        ShowCurrentDialogue();
    }

    private void ShowCurrentDialogue()
    {
        if (currentDialogueIndex >= dialogueData.Length)
        {
            Debug.Log("Diyalog bitti, kapatılıyor...");
            EndDialogue();
            return;
        }

        DialogueData currentDialogue = dialogueData[currentDialogueIndex];
        Debug.Log($"Diyalog gösteriliyor: {currentDialogue.npcQuote}");

        if (npcDialogueText != null)
        {
            npcDialogueText.text = currentDialogue.npcQuote;
        }
        else
        {
            Debug.LogWarning("npcDialogueText referansı eksik!");
        }

        // Setup buttons
        for (int i = 0; i < dialogueButtons.Length; i++)
        {
            if (dialogueButtons[i] != null)
            {
                if (i < currentDialogue.playerOptions.Length)
                {
                    dialogueButtons[i].gameObject.SetActive(true);
                    if (buttonTexts[i] != null)
                    {
                        buttonTexts[i].text = currentDialogue.playerOptions[i].text;
                        Debug.Log($"Buton {i} metni ayarlandı: {currentDialogue.playerOptions[i].text}");
                    }
                }
                else
                {
                    dialogueButtons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void OnDialogueOptionSelected(int optionIndex)
    {
        Debug.Log($"Diyalog seçeneği seçildi: {optionIndex}");
        
        if (currentDialogueIndex >= dialogueData.Length)
        {
            Debug.LogWarning("Geçersiz diyalog indeksi!");
            return;
        }

        DialogueData currentDialogue = dialogueData[currentDialogueIndex];
        if (optionIndex >= currentDialogue.playerOptions.Length)
        {
            Debug.LogWarning("Geçersiz seçenek indeksi!");
            return;
        }

        DialogueOption selectedOption = currentDialogue.playerOptions[optionIndex];
        Debug.Log($"Seçilen seçenek: {selectedOption.text}");

        // Apply effects
        if (selectedOption.wearEffect != 0)
        {
            if (wearManager != null)
            {
                if (selectedOption.wearEffect > 0)
                {
                    wearManager.AddWear(selectedOption.wearEffect);
                    Debug.Log($"Wear eklendi: {selectedOption.wearEffect}");
                }
                else
                {
                    wearManager.AddWear(selectedOption.wearEffect);
                    Debug.Log($"Wear azaltıldı: {selectedOption.wearEffect}");
                }
            }
            else
            {
                Debug.LogWarning("WearManager bulunamadı!");
            }
        }

        if (selectedOption.moneyEffect != 0)
        {
            if (moneyManager != null)
            {
                if (selectedOption.moneyEffect > 0)
                {
                    moneyManager.AddMoney(selectedOption.moneyEffect);
                    Debug.Log($"Para eklendi: {selectedOption.moneyEffect}");
                }
                else
                {
                    moneyManager.AddMoney(selectedOption.moneyEffect);
                    Debug.Log($"Para azaltıldı: {selectedOption.moneyEffect}");
                }
            }
            else
            {
                Debug.LogWarning("MoneyManager bulunamadı!");
            }
        }

        currentDialogueIndex++;
        ShowCurrentDialogue();
    }

    private void EndDialogue()
    {
        // Diyalog durumunu kapat
        isInDialogue = false;
        isInAnyDialogue = false;
        
        // UI'ı kapat
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }
        
        // Oyunu normal hızına döndür
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Çıkış noktasına git
        if (navMeshAgent != null && exitPosition != null)
        {
            navMeshAgent.enabled = true;
            navMeshAgent.SetDestination(exitPosition.position);
            
            if (animator != null)
            {
                animator.SetTrigger("Walk");
            }
            
            StartCoroutine(WaitForExit());
            Debug.Log($"NPC {npcName} diyalog bitti, çıkış noktasına gidiyor");
        }
        else
        {
            Debug.LogError($"NPC {npcName} için NavMeshAgent veya exitPosition bulunamadı!");
        }
    }

    private IEnumerator WaitForExit()
    {
        while (navMeshAgent != null && 
               navMeshAgent.enabled && 
               navMeshAgent.hasPath && 
               navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            Debug.Log($"NPC {npcName} çıkışa gidiyor. Kalan mesafe: {navMeshAgent.remainingDistance}");
            yield return null;
        }

        // Çıkış noktasına ulaştığında
        if (animator != null)
        {
            animator.SetTrigger("Idle");
        }

        // Çıkış animasyonu için 2 saniye bekle
        yield return new WaitForSeconds(endDialogueEndDestroyDelay);

        // NPC'yi yok et
        Debug.Log($"NPC {npcName} çıkış noktasına ulaştı ve yok ediliyor");
        if (SpecialNPCManager.Instance != null)
        {
            SpecialNPCManager.Instance.RemoveNPC(npcName);
        }
    }

    public string GetNPCName()
    {
        return npcName;
    }

    public void SetReferences(
        string newNPCName,
        int newSpawnDay,
        int newMinSpawnHour,
        int newMaxSpawnHour,
        Transform newSpawnPosition,
        Transform newWaitingPosition,
        Transform newExitPosition,
        WearManager newWearManager,
        MoneyManager newMoneyManager,
        GameObject newDialogueUI,
        TextMeshProUGUI newNPCNameText,
        TextMeshProUGUI newNPCDialogueText,
        Button[] newDialogueButtons,
        TextMeshProUGUI[] newButtonTexts,
        DialogueData[] newDialogueData)
    {
        npcName = newNPCName;
        spawnDay = newSpawnDay;
        minSpawnHour = newMinSpawnHour;
        maxSpawnHour = newMaxSpawnHour;
        spawnPosition = newSpawnPosition;
        waitingPosition = newWaitingPosition;
        exitPosition = newExitPosition;
        wearManager = newWearManager;
        moneyManager = newMoneyManager;
        dialogueUI = newDialogueUI;
        npcNameText = newNPCNameText;
        npcDialogueText = newNPCDialogueText;
        dialogueButtons = newDialogueButtons;
        buttonTexts = newButtonTexts;
        dialogueData = newDialogueData;

        // Referanslar ayarlandıktan sonra spawn ol
        StartCoroutine(DelayedSpawn());
    }

    private IEnumerator DelayedSpawn()
    {
        // Bir frame bekle
        yield return null;

        // Spawn ol
        hasSpawned = true;
        transform.position = spawnPosition.position;
        
        // NavMeshAgent'ı aktifleştir
        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
            navMeshAgent.speed = moveSpeed;
            yield return null; // NavMeshAgent'ın aktifleşmesi için bir frame daha bekle
            navMeshAgent.SetDestination(waitingPosition.position);
            animator.SetTrigger("Walk");
            Debug.Log($"NPC {npcName} spawn oldu ve bekleme noktasına gidiyor");
        }
        else
        {
            Debug.LogError($"NPC {npcName} için NavMeshAgent bulunamadı!");
        }
    }

    private void OnDestroy()
    {
        // Manager'dan kendini kaldır
        if (SpecialNPCManager.Instance != null)
        {
            SpecialNPCManager.Instance.RemoveNPC(npcName);
        }
    }
}

[System.Serializable]
public class DialogueData
{
    public string npcQuote;
    public DialogueOption[] playerOptions;
}

[System.Serializable]
public class DialogueOption
{
    public string text;
    public float wearEffect;
    public float moneyEffect;
}