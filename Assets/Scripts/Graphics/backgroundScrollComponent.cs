using UnityEngine;
using Random = UnityEngine.Random;

public class backgroundScrollComponent : MonoBehaviour
{
    [SerializeField] float speed;
    SpawningController spawningController;

    Renderer backgroundRenderer;
    private int nbMaterials;

    private void Awake()
    {
        spawningController = GameObject.FindGameObjectWithTag("GameController").GetComponent<SpawningController>();
        backgroundRenderer = GetComponent<MeshRenderer>();
        nbMaterials = backgroundRenderer.materials.Length;
        backgroundRenderer.material = backgroundRenderer.materials[Random.Range(0, nbMaterials)];
    }

    // Update is called once per frame
    void Update()
    {
        backgroundRenderer.material.mainTextureOffset += new Vector2(0, speed * Time.deltaTime * spawningController.Difficulty);
    }
}
