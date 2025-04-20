using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTrashCleaner : MonoBehaviour
{
    [Header("Süpürge Ayarları")]
    public float cleaningTime = 3f;
    private bool isCleaning = false;

    [Header("Çöp Temizleme Ayarları")]
    public float cleaningRadius = 2f;

    [Header("Held Object")]
    public Player_RayCast rayCast; // Raycast üzerinden HeldObject'e ulaşacağız

    [Header("UI")]
    public Image progressBar; // İlerleme çubuğu için Image türünde bir alan ekledik

    private bool canCleanTrash = false; // Çöp temizleme işlemi başlatılabilir mi?

    void Start()
    {
        progressBar.gameObject.SetActive(false); // Başlangıçta progress bar'ı gizle
    }

    void Update()
    {
        HeldObject held = rayCast.GetHeldObject(); // Elindeki objeyi al

        // Eğer süpürgeyi tutuyorsa
        if (held != null && held.isMop) 
        {
            // Süpürgeyi aldığında barı göster
            if (!progressBar.gameObject.activeSelf) // Eğer bar aktif değilse
            {
                progressBar.gameObject.SetActive(true); // Barı göster
            }

            // Çöp kontrolü yap ve temizleme işlemi başlatılmadan önce barı kontrol et
            Collider[] trashInRange = Physics.OverlapSphere(transform.position, cleaningRadius, LayerMask.GetMask("Trash"));
            canCleanTrash = trashInRange.Length > 0; // Çöp var mı kontrolü

            // Eğer sol tıklama yapıyorsa ve çöp varsa işlemi başlat
            if (Input.GetMouseButton(0) && !isCleaning && canCleanTrash)
            {
                StartCoroutine(CleanTrash(trashInRange));
            }
            else if (!canCleanTrash) // Çöp yoksa barı sıfırla
            {
                progressBar.fillAmount = 0f;
            }
        }
        else
        {
            // Süpürgeyi bırakınca barı gizle
            if (progressBar.gameObject.activeSelf)
            {
                progressBar.gameObject.SetActive(false); // Barı gizle
            }
        }
    }

    private IEnumerator CleanTrash(Collider[] trashInRange)
    {
        isCleaning = true;
        float timeElapsed = 0f;

        while (timeElapsed < cleaningTime)
        {
            // Eğer sol tıklama bırakılırsa işlemi iptal et
            if (!Input.GetMouseButton(0))
            {
                isCleaning = false;
                progressBar.gameObject.SetActive(false); // Barı gizle
                yield break; // İşlem durduruluyor
            }

            timeElapsed += Time.deltaTime;
            progressBar.fillAmount = timeElapsed / cleaningTime; // Çubuğun dolmasını sağla

            // Çöp temizleme işlemi sırasında çöpleri sil
            if (timeElapsed >= cleaningTime)
            {
                foreach (var trash in trashInRange)
                {
                    Destroy(trash.gameObject);
                }
            }

            yield return null;
        }

        isCleaning = false;
        progressBar.gameObject.SetActive(false); // Temizleme tamamlandığında barı gizle
    }
}
