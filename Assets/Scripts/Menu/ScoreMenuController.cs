using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreMenuController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;

    private void Awake()
    {
        int highscore = PlayerPrefs.GetInt("highscore");
        int lastScore = PlayerPrefs.GetInt("lastScore");

        scoreText.text = $"Highscore: {highscore}\n\nLast Score: {lastScore}";
    }

    public void Play()
    {
        SceneManager.LoadScene("SpaceShooter");
    }
}
