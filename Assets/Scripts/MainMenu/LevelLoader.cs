using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;

    IEnumerator LoadLevel(int levelIndex)
    {
    transition.SetTrigger("Start");
    yield return new WaitForSeconds(transitionTime);
    SceneManager.LoadScene(levelIndex);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
