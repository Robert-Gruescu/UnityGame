using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameOverMenu : MonoBehaviour
{
    public GameObject gameOverPanel; // panelul de Game Over

    void Start()
    {
        // la început panelul e ascuns
        gameOverPanel.SetActive(false);
    }

    // Apelează această funcție când playerul moare
    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);

        // oprește jocul cât timp e Game Over
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        // Repornim timpul
        Time.timeScale = 1f;

        // Reîncarcă scena curentă
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // oprește Play Mode în Editor
#else
        Application.Quit(); // în build închide aplicația
#endif
    }
}
