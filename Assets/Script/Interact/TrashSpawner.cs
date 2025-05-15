using System.Collections.Generic;
using UnityEngine;

public class TrashSpawner : MonoBehaviour
{
    [Header("Çöp prefabları")]
    public List<GameObject> trashPrefabs;

    [Header("Çıkabilecek yerler")]
    public List<Transform> trashSpawnPoints;

    [Header("Her gün kaç tane çöp çıksın?")]
    public int trashCountPerDay = 5;

    [Header("Rastgele çöp aralığı (saniye)")]
    public float minTrashInterval = 30f;
    public float maxTrashInterval = 90f;

    private float trashTimer;

    void Start()
    {
        SetNextTrashTime();
    }

    void Update()
    {
        // Zamanlayıcıyla otomatik çöp spawn
        trashTimer -= Time.deltaTime;

        if (trashTimer <= 0f)
        {
            HandleNewDayTrash();
            SetNextTrashTime();
        }
    }
    
    // Ana fonksiyon: eski çöpleri temizle, yeni çöpleri spawn et
    public void HandleNewDayTrash()
    {
        SpawnTrashForNewDay();
    }

    void SpawnTrashForNewDay()
{
    List<Transform> shuffledSpawnPoints = new List<Transform>(trashSpawnPoints);
    ShuffleList(shuffledSpawnPoints);

    int spawnedCount = 0;

    foreach (Transform spawnPoint in shuffledSpawnPoints)
    {
        // Bu noktada çöp var mı kontrol et
        Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, 1f);
        bool spotOccupied = false;

        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Trash"))
            {
                spotOccupied = true;
                break;
            }
        }

        if (spotOccupied) continue;

        // Boşsa çöp spawn et
        GameObject selectedTrash = trashPrefabs[Random.Range(0, trashPrefabs.Count)];
        GameObject spawned = Instantiate(selectedTrash, spawnPoint.position, Quaternion.identity);
        spawned.tag = "Trash";

        spawnedCount++;

        if (spawnedCount >= trashCountPerDay)
            break;
    }
}


    // Önceki çöpleri sil
    void ClearOldTrash()
    {
        GameObject[] existingTrash = GameObject.FindGameObjectsWithTag("Trash");

        foreach (GameObject t in existingTrash)
        {
            Destroy(t);
        }
    }

    // Listeyi karıştır (rastgele yerlerde çöp çıksın diye)
    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }

    // Yeni rastgele süre belirle
    void SetNextTrashTime()
    {
        trashTimer = Random.Range(minTrashInterval, maxTrashInterval);
    }
}
