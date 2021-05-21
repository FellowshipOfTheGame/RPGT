using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;

    public void OpenURL()
    {
    Application.OpenURL("https://www.patreon.com/");
    }

    public void Website()
    {
    Application.OpenURL("https://fog-icmc.itch.io/");
    }

    public void CreateGame()
    {
    StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 1));
	//SceneManager.LoadScene("MapMaker");
    }

    public void LoadLobby()
    {
    StartCoroutine(LoadLevel(SceneManager.GetActiveScene().buildIndex + 2));
	//SceneManager.LoadScene("Room");
    }

	public void QuitGame()
	{
	Application.Quit();
	}

    IEnumerator LoadLevel(int levelIndex)
    {
    transition.SetTrigger("Start");
    yield return new WaitForSeconds(transitionTime);
    SceneManager.LoadScene(levelIndex);
    }
}
