using UnityEngine;

public class bgMenuComponent : MonoBehaviour
{
    [SerializeField] float speed;
    Renderer backgroundRenderer;
    // Start is called before the first frame update
    void Awake()
    {
        backgroundRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        backgroundRenderer.material.mainTextureOffset += new Vector2(0, speed * Time.deltaTime);
    }
}
