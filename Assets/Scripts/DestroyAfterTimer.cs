using UnityEngine;

public class DestroyAfterTimer : MonoBehaviour
{
    [SerializeField] float timer;

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
            Destroy(gameObject);
    }
}
