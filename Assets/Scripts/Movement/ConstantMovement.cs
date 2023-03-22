using UnityEngine;

public class ConstantMovement : MonoBehaviour
{
    public float speed = 1;
    public Space space;

    void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime, space);
    }
}
