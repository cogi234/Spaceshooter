using UnityEngine;

public class StarRepeatComponent : MonoBehaviour
{
    [SerializeField] private Renderer sprite;
    private float heigth, minY;
    // Start is called before the first frame update
    private void Awake()
    {
    }

    void Start()
    {
        heigth = sprite.bounds.size.y;
        // transform.Translate(new Vector2(0, speed*Time.deltaTime));
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -heigth * 0.70f)
        {
            //return to the top
            transform.position += new Vector3(0, heigth * 2, 0);

        }
    }
}
