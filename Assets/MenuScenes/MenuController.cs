using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartMap(int seed, int mountainModifier, int forestModifier)
    {
        CrossSceneData.Seed = seed;
        CrossSceneData.MountainModifier = mountainModifier;
        CrossSceneData.ForestModifier = forestModifier;
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
