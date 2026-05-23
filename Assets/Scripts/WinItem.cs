using UnityEngine;

public class WinItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().TriggerWin();
            Destroy(gameObject);
        }
    }
}