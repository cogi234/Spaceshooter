using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuComponent : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SpaceShooter");
    }
}
