using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask npcLayer;

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & npcLayer.value) != 0)
        {
            Debug.Log("NPC girdi - Kapı açılıyor");
            animator.SetBool("isOpen", true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & npcLayer.value) != 0)
        {
            Debug.Log("NPC çıktı - Kapı kapanıyor");
            animator.SetBool("isOpen", false);
        }
    }
}
