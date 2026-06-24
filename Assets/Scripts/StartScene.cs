using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    [Header("Next scene")]
    [Tooltip("Load a scene when all dialogue lines finish.")]
    public bool loadNextSceneOnComplete = true;

    [Tooltip("Scene name as listed in Build Settings (e.g. Meta, Level1).")]
    public string nextSceneName;

    public void StartGame()
    {
        if (loadNextSceneOnComplete && !string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
