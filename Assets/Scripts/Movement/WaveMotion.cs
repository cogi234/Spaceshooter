using UnityEngine;

public class WaveMotion : MonoBehaviour
{
    public Vector2 direction;
    public float radPerSecond;
    public Space space;

    float currentRadAngle = 0;
    float lastRadAngle = 0;

    void Update()
    {
        currentRadAngle = (currentRadAngle + (radPerSecond * Time.deltaTime)) % (2 * Mathf.PI);

        Vector2 delta = (Mathf.Sin(currentRadAngle) * direction) - (Mathf.Sin(lastRadAngle) * direction);
        transform.Translate(delta, space);

        lastRadAngle = currentRadAngle;
    }

    public void Reset()
    {
        currentRadAngle = 0;
        lastRadAngle = 0;
    }
}
