using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;

        Vector3 direction = Camera.main.transform.position - transform.position;
        direction.y = 0f; // Eðer sadece yatay düzlemde dönsün istersen bunu ekle

        Quaternion rotation = Quaternion.LookRotation(-direction);
        transform.rotation = rotation;
    }
}
