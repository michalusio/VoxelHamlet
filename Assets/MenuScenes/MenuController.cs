using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartMap()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void SetupNewGame()
    {
        SceneManager.LoadScene("MapSelectorScene", LoadSceneMode.Single);
    }

    public void ShowCredits()
    {
        SceneManager.LoadScene("CreditsScene", LoadSceneMode.Single);
    }
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }
}
