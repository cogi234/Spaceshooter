using UnityEngine;

public class DisableOutsideScreen : MonoBehaviour
{
    [SerializeField] bool doWeDestroy = false;
    float minY, maxY, minX, maxX;

    void Awake()
    {
        maxY = Camera.main.orthographicSize * 1.2f;
        minY = -maxY;
        maxX = (Camera.main.orthographicSize * Screen.width / Screen.height) * 1.2f;
        minX = -maxX;
    }

    void Update()
    {
        if (transform.position.y < minY || transform.position.y > maxY || transform.position.x < minX || transform.position.x > maxX)
            if (doWeDestroy)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
    }
}
