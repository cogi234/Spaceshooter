using UnityEngine;

public class PowerUpController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            col.gameObject.GetComponent<PlayerController>().PowerUp();
            Destroy(gameObject);
        }
    }
}
