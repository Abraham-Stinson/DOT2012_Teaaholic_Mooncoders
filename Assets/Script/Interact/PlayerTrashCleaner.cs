using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTrashCleaner : MonoBehaviour
{
    [Header("Süpürge Ayarları")]
    public float cleaningTime = 3f;
    private bool isCleaning = false;

    [Header("Çöp Temizleme Ayarları")]
    public float cleaningRadius = 9f;

    [Header("Held Object")]
    public Player_RayCast rayCast;

    [Header("UI")]
    public Image progressBar;

    private Coroutine cleaningCoroutine;

    void Update()
    {
        HeldObject held = rayCast.GetHeldObject();

        // Süpürge eldeyse barı göster, değilse gizle
        if (held != null && held.isMop)
        {
            progressBar.gameObject.SetActive(true);

            // Sol tık basılıysa ve henüz temizlik başlamadıysa
            if (Input.GetMouseButton(0) && !isCleaning)
            {
                // Kameranın baktığı yerde çöp var mı kontrol et
                if (LookingAtTrash())
                {
                    cleaningCoroutine = StartCoroutine(CleanTrash());
                }
            }
            // Sol tık bırakılırsa temizlik iptal edilir
            else if (Input.GetMouseButtonUp(0) && isCleaning)
            {
                StopCleaning();
            }
        }
        else
        {
            progressBar.gameObject.SetActive(false);
            if (isCleaning) StopCleaning();
        }
    }

    private IEnumerator CleanTrash()
    {
        isCleaning = true;
        float timeElapsed = 0f;
        progressBar.fillAmount = 0f;

        while (timeElapsed < cleaningTime)
        {
            // Sol tık bırakılırsa ya da artık çöpün üstüne bakılmıyorsa iptal et
            if (!Input.GetMouseButton(0) || !LookingAtTrash())
            {
                StopCleaning();
                yield break;
            }

            timeElapsed += Time.deltaTime;
            progressBar.fillAmount = timeElapsed / cleaningTime;

            yield return null;
        }

        // Temizleme tamamlandıysa çöpleri sil
        Collider[] trashInRange = Physics.OverlapSphere(transform.position, cleaningRadius, LayerMask.GetMask("Trash"));

        foreach (var trash in trashInRange)
        {
            Destroy(trash.gameObject);
        }

        StopCleaning();
    }

    private void StopCleaning()
    {
        isCleaning = false;
        progressBar.fillAmount = 0f;

        if (cleaningCoroutine != null)
        {
            StopCoroutine(cleaningCoroutine);
            cleaningCoroutine = null;
        }
    }

    private bool LookingAtTrash()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        return Physics.Raycast(ray, out RaycastHit hit, 9f, LayerMask.GetMask("Trash"));
    }
}
