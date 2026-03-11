using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "DemoScene";
    [SerializeField] private GameObject optionsPanel;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenOptions()
    {
        if (optionsPanel == null) return;
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel == null) return;
        optionsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
