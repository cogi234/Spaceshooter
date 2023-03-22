using UnityEngine;

public class RandomSprite : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;

    private void OnEnable()
    {
        GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Length)];
    }
}
