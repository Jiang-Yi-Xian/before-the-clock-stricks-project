using UnityEngine;
using UnityEngine.SceneManagement;

public class LoopManager : MonoBehaviour
{
    public void RestartScene() 
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    public void LoopButtonPressed() 
    {
        RestartScene();
    }
    public void EndGame()
    {
        Debug.Log("QuitGame");
        Application.Quit();
    }
}
