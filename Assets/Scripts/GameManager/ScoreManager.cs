using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    SpawningController spawningController;

    [SerializeField] int scoreToPowerUp = 50;

    private void Awake()
    {
        spawningController = GetComponent<SpawningController>();
    }


    int score;
    public int Score
    {
        get { return score; }
        set
        {
            score = value;
            scoreText.text = $"Score: {score}";

            if (score >= scoreToPowerUp)
            {
                scoreToPowerUp *= 5;
                spawningController.SpawnPowerUp();
            }
        }
    }
}
