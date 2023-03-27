using UnityEngine;
using Random = UnityEngine.Random;

public class backgroundScrollComponent : MonoBehaviour
{
    [SerializeField] float speed;
    GameManager gameManager;

    Renderer backgroundRenderer;
    private int nbMaterials;

    private void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
        backgroundRenderer = GetComponent<MeshRenderer>();
        nbMaterials = backgroundRenderer.materials.Length;
        backgroundRenderer.material = backgroundRenderer.materials[Random.Range(0, nbMaterials)];
    }

    // Update is called once per frame
    void Update()
    {
        backgroundRenderer.material.mainTextureOffset += new Vector2(0, speed * Time.deltaTime * gameManager.Difficulty);
    }
}
