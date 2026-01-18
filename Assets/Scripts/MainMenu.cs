using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    public GameObject menuPanel;

    void Start()
    {
        menuPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void StartGame()
    {
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // oprește Play Mode
#else
        Application.Quit(); // în build închide aplicația
#endif
    }
}
