using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuComponent : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
